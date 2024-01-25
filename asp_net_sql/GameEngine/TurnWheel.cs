using System.Drawing;
using static imageUtility.ColorExtensions;

namespace asp_net_sql.GameEngine;

/// <summary>
/// The purpose is to go thru TurnList, identify a player as a Human or AI;<br/>
/// in case of AI, trigger logic for choosing a move and performing clicks,<br/>
/// in case of a Human player, wait for their action.<br/>
/// Update labels.
/// </summary>
internal class TurnWheel
{
    static int head;
    static bool isBusy;

    static internal int Head { get => head; }

    static internal Roster CurPlayer => Game.TurnList[head];

    static bool CheckPlayerType(Roster player, string type) => player.ToString().StartsWith(type);
    static bool CurPlayerIsHuman => CheckPlayerType(CurPlayer, "Human");
    static bool CurPlayerIsAI => CheckPlayerType(CurPlayer, "AI");

    static Action EnableUICb = () => { };
    static Action DisableUICb = () => { };

    static internal void SetCallbacks(Action _EnableUICb, Action _DisableUICb)
    {
        EnableUICb = _EnableUICb;
        DisableUICb = _DisableUICb;
    }

    static internal void Reset(int _head = -1)
    {
        head = _head;
        isBusy = false;
    }

    /// <summary>
    /// For Human players Comes from CellWrapper -> OnClick;<br/>
    /// For AI players from CellWrapper -> AIMovedHandler -> OnClick<br/>
    /// </summary>
    static internal readonly EventHandler<Point> PlayerMovedHandler = (object? sender, Point e) =>
    {
        if (isBusy) return;
        isBusy = true;

        if (sender is not IComponent iComp)
            throw new Exception($"TurnWheel.PlayerMovedHandler : '{sender}' is not IComponent");

        if (CurPlayerIsHuman) DisableUICb();

        iComp.IsLocked = true;

        Game.Update(CurPlayer, new Tile(e.X, e.Y));
    };

    static internal void Advance()
    {
        isBusy = false;

        GoNextPlayer();

        AssertPlayer();

        Game.GState = Game.State.Started;
    }

    static void GoNextPlayer() => head = head == Game.TurnList.Length - 1 ? 0 : head + 1;

    /// <summary>
    /// Ensure next click is scheduled and will be performed
    /// </summary>
    static internal void AssertPlayer()
    {
        if (CurPlayerIsAI)
        {
            DisableUICb();

         //   EM.Raise(Evt.AIMakeMove, new { }, CurPlayer);

        } else if (CurPlayerIsHuman)
        {
            EnableUICb();
        }

    //    EM.Raise(Evt.SyncMoveLabels, new { }, CurPlayer);
    }

    static internal void GameCountdown()
    {
        Game.GState = Game.State.Countdown;
        Thread thread = new(CntDown);
        thread.Start();
    }

    static void CntDown()
    {
        Thread.Sleep(500);
        foreach (LabelManager.Countdown e in Enum.GetValues(typeof(LabelManager.Countdown)))
        {
            EventLoop.Main.StartEvtChain(
                Evt.UpdateLabels,
                new Enum[] { e });

            Thread.Sleep(1000);
        }

        Advance();
    }

}
