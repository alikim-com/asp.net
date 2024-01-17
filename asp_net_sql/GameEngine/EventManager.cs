using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;

namespace asp_net_sql.GameEngine;

/// <summary>
/// Manages events
/// </summary>
public class EM
{
    /// <summary>
    /// Notifies Menu about changes in game state
    /// </summary>
    static event EventHandler<Game.State> EvtGStateChanged = delegate { };
    /// <summary>
    /// Translates game states into UI states in VBridge
    /// </summary>
    /// <param>List of cells to update,<br/>
    /// containing row(X) and column(Y) of a cell and the player occupying it</param>
    static event EventHandler<Dictionary<Tile, Game.Roster>> EvtSyncBoard = delegate { };
    /// <summary>
    /// Translates game states into UI states in VBridge
    /// </summary>
    /// <param>List of cells to grey out upon winning,<br/>
    /// containing row(X) and column(Y) of a cell and the player occupying it</param>
    static event EventHandler<Dictionary<Tile, Game.Roster>> EvtSyncBoardWin = delegate { };
    /// <summary>
    /// Sync the board with EvtSyncBoard translation done by VBridge
    /// </summary>
    /// <param>List of cells to update,<br/>
    /// containing row(X) and column(Y) of a cell and a background associated with the player</param>
    static event EventHandler<Dictionary<Point, CellWrapper.BgMode>> EvtSyncBoardUI = delegate { };
    /// <summary>
    /// Issued by TurnWheel to make AI take action
    /// </summary>
    static event EventHandler<Game.Roster> EvtAIMakeMove = delegate { };
    /// <summary>
    /// Issued by TurnWheel for VBridge to update labels on player move
    /// </summary>
    static event EventHandler<Game.Roster> EvtSyncMoveLabels = delegate { };
    /// <summary>
    /// Raised by clicking or simulating a click on a cell
    /// </summary>
    /// <param>Point containing row(X) and column(Y) of the cell clicked</param>
    static event EventHandler<Point> EvtPlayerMoved = delegate { };
    /// <summary>
    /// Raised by AI for a cell to simulate a click on it
    /// </summary>
    /// <param>Point containing row(X) and column(Y) of the cell clicked</param>
    static event EventHandler<Point> EvtAIMoved = delegate { };
    /// <summary>
    /// Raised by Game when the current game is over
    /// </summary>
    static event EventHandler<Game.Roster> EvtGameOver = delegate { };
    /// <summary>
    /// Raised by Game when the current game is a tie
    /// </summary>
    static event EventHandler EvtGameTie = delegate { };
    /// <summary>
    /// Updates labels in LabelManager
    /// </summary>
    static event EventHandler<Enum[]> EvtUpdateLabels = delegate { };

    /// <summary>
    /// Event associations
    /// </summary>
    public enum Evt
    {
        GStateChanged,
        SyncBoard,
        SyncBoardWin,
        SyncBoardUI,
        AIMakeMove,
        SyncMoveLabels,
        PlayerMoved,
        AIMoved,
        GameOver,
        GameTie,
        UpdateLabels
    }
    /// <summary>
    /// To raise or sub/unsub to events by their enum names
    /// </summary>
    static readonly Dictionary<Evt, Delegate> dict = new() {
        { Evt.GStateChanged, EvtGStateChanged },
        { Evt.SyncBoard, EvtSyncBoard },
        { Evt.SyncBoardWin, EvtSyncBoardWin },
        { Evt.SyncBoardUI, EvtSyncBoardUI },
        { Evt.AIMakeMove, EvtAIMakeMove },
        { Evt.SyncMoveLabels, EvtSyncMoveLabels },
        { Evt.PlayerMoved, EvtPlayerMoved },
        { Evt.AIMoved, EvtAIMoved },
        { Evt.GameOver, EvtGameOver },
        { Evt.GameTie, EvtGameTie },
        { Evt.UpdateLabels, EvtUpdateLabels },
    };

    // ----- event wrappers -----

    /// <summary>
    /// Multi-thread safe wrapper for raising events
    /// </summary>
    /// <param name="evt">Event to be raised</param>
    /// <param name="sender">Event sender object</param>
    /// <param name="e">Event arguments</param>
    static public void Raise<E>(Evt enm, object sender, E e)
    {
        if (!dict.TryGetValue(enm, out var _evt))
            throw new NotImplementedException($"EM.Raise : no event for Evt.{enm}");

        bool nonGeneric = _evt.GetType() == typeof(EventHandler);

        if (nonGeneric)
        {
            var evtNG = (EventHandler)_evt;
            evtNG?.Invoke(sender, new EventArgs());

        } else
        {
            var evtG = (EventHandler<E>)_evt;
            evtG?.Invoke(sender, e);
        }
    }

    static public void Subscribe(Evt enm, Delegate handler)
    {
        if (!dict.TryGetValue(enm, out var evt))
            throw new NotImplementedException($"EM.Subscribe : no event for Evt.{enm}");

        dict[enm] = Delegate.Combine(evt, handler);
    }

    static public void Unsubscribe(Evt enm, Delegate handler)
    {
        if (!dict.TryGetValue(enm, out var evt))
            throw new NotImplementedException($"EM.Unsubscribe : no event for Evt.{enm}");

        dict[enm] = Delegate.Remove(evt, handler) ?? (() => { });
    }

    // ----- cross-thread calls -----

    static internal AppForm? uiThread;

    /// <summary>
    /// Raise events from UI thread for safe UI access
    /// </summary>
    /// <param name="lambda"></param>
    static public void InvokeFromMainThread(Action lambda) { }
    // => uiThread?.Invoke(lambda);
}

// EVENT GRAPH

public class GNode<TEvtArg>(
    EventHandler<TEvtArg> _handler,
    TEvtArg _arg,
    bool _asyncFlag,
    bool _awaitFlag,
    List<GNode<TEvtArg>> _children)
{
    public EventHandler<TEvtArg> handler = _handler;
    public TEvtArg arg = _arg;
    public bool asyncFlag = _asyncFlag;
    public bool awaitFlag = _awaitFlag;

    public async Task RaiseAsync()
    {
        var evtG = handler;
        if(awaitFlag)
            await Task.Run(() => evtG.Invoke(this, arg));
        else
            evtG.Invoke(this, arg);
    }

    public readonly List<GNode<TEvtArg>> children = _children;
    public void AddChild(GNode<TEvtArg> node) => children.Add(node);
    public bool RemChild(GNode<TEvtArg> node) => children.Remove(node);
}

public class DirGraphGNode<TEvtArg>(List<GNode<TEvtArg>> _nodes)
{
    public readonly List<GNode<TEvtArg>> nodes = _nodes;
}
public class EventLoop<TEvtArg>(BlockingCollection<GNode<TEvtArg>> _dataQueue)
{
    public BlockingCollection<GNode<TEvtArg>> dataQueue = _dataQueue;

    public void Run()
    {
        Utils.Log("Consumer start");

        foreach (GNode<TEvtArg> item in dataQueue.GetConsumingEnumerable())
        {
            Utils.Log($"Consumed: {item}");

            //await item.RaiseAsync();
            
        }

        Utils.Log("Consumer end");
    }
}


