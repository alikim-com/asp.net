using System.Drawing;
using asp_net_sql.GameEnginePort;

namespace asp_net_sql.GameEngine;

class CellBg
{
    internal enum BgMode
    {
        // star
        Default,
        MouseEnter,
        MouseLeave,
        // tokens
        Player1,
        Player2,
        // grey tokens
        Player1Lost,
        Player2Lost,
    }

    static protected readonly Dictionary<BgMode, Image> backgr = [];

    static internal void CreateBgSet(Size boxSize)
    {
        Image bgHover = Resource.Star.GetOverlayOnBackground(
            new Size( // to improve quality in bigger app window
                (int)(boxSize.Width * 1.5),
                (int)(boxSize.Height * 1.5)),
            null,
            "center",
            "center"
        );
        Image bgDef = bgHover.GetImageCopyWithAlpha(0.70f);

        backgr.Add(BgMode.Default, bgDef);
        backgr.Add(BgMode.MouseEnter, bgHover);
        backgr.Add(BgMode.MouseLeave, bgDef);

        Image token1 = Resource.TokenLeft.GetOverlayOnBackground(
            new Size(
                (int)(boxSize.Width * 1.5),
                (int)(boxSize.Height * 1.5)),
            null,
            "center",
            "center"
        );

        Image token2 = Resource.TokenRight.GetOverlayOnBackground(
            new Size(
                (int)(boxSize.Width * 1.5),
                (int)(boxSize.Height * 1.5)),
            null,
            "center",
            "center"
        );

        backgr.Add(BgMode.Player1, token1);
        backgr.Add(BgMode.Player2, token2);

        backgr.Add(BgMode.Player1Lost, token1.Desaturate("PS", 0.8));
        backgr.Add(BgMode.Player2Lost, token2.Desaturate("PS", 0.8));
    }

}

/// <summary>
/// Controls board cells backgrounds and associated mouse events
/// </summary>
internal class CellWrapper : CellBg, IComponent
{
    readonly Panel box;
    readonly Point rc;
    internal Point RC { get => rc; }
    
    readonly Dictionary<BgMode, EventHandler> evtDetail = new();

    public bool IsLocked { get; set; } = false;
    bool isEnabled = false;

    internal CellWrapper(Panel _box, int _row, int _col)
    {
        box = _box;
        rc = new Point(_row, _col);

        AIMovedHandler = (object? _, Point pnt) =>
        {
            if (pnt != rc) return;
            OnClick(_, new EventArgs());
        };

        SyncBoardUIHandler = (object? _, Dictionary<Point, BgMode> e) =>
        {
            if (!e.TryGetValue(rc, out BgMode val)) return;
            SetBg(val);
        };

        CreateEventHandlers();
    }

    /// <summary>
    /// Subscribed EM.EvtSyncBoardUI event
    /// </summary>
    internal EventHandler<Dictionary<Point, BgMode>> SyncBoardUIHandler;

    void SetBg(BgMode mode) => evtDetail[mode](this, new EventArgs());

    EventHandler CreateEventHandler(Image? image) => (object? sender, EventArgs e) =>
    {
        if (box.BackgroundImage != image) box.BackgroundImage = image;
    };

    void CreateEventHandlers()
    {
        foreach (BgMode evtName in Enum.GetValues(typeof(BgMode)))
        {
            if (!backgr.TryGetValue(evtName, out Image? image))
            {
                Utils.Msg($"CellWrapper.CreateEventHandlers : no image for event '{evtName}' found");
                continue;
            }
            evtDetail.Add(evtName, CreateEventHandler(image));
        }
    }

    void AddHoverEventHandlers()
    {
        box.MouseEnter += evtDetail[BgMode.MouseEnter];
        box.MouseLeave += evtDetail[BgMode.MouseLeave];
    }

    void RemoveHoverEventHandlers()
    {
        box.MouseEnter -= evtDetail[BgMode.MouseEnter];
        box.MouseLeave -= evtDetail[BgMode.MouseLeave];
    }

    public void Enable()
    {
        if (IsLocked || isEnabled) return;

        isEnabled = true;
        AddHoverEventHandlers();
        box.Click += OnClick;
        box.Cursor = Cursors.Hand;
    }

    public void Disable()
    {
        if (IsLocked || !isEnabled) return;

        isEnabled = false;
        box.Click -= OnClick;
        RemoveHoverEventHandlers();
        box.Cursor = Cursors.Default;
    }

    public void Reset()
    {
        IsLocked = false;
        isEnabled = true;
        Disable();
    }

    /// <summary>
    /// Raises EM.EvtPlayerMoved event
    /// </summary>
    void OnClick(object? _, EventArgs __)
    {
        if (IsLocked) return;

        EM.Raise(EM.Evt.PlayerMoved, this, rc);
    }

    /// <summary>
    /// AI mouse clicks
    /// </summary>
    internal EventHandler<Point> AIMovedHandler;
}

