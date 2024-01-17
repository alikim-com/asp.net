using System.Reflection;
using System.Runtime.InteropServices;

using System.Drawing;
using asp_net_sql.GameEnginePort;

namespace asp_net_sql.GameEngine;

partial class AppForm : Control
{
    // ENGINE PORT

    public event EventHandler<ControlEventArgs> ControlAdded;
    bool DoubleBuffered;
    private TableLayoutPanel tLayout;
    private Panel[,] cells;
    private Label info;
    private MenuStrip menuStrip1;
    private ToolStripMenuItem menuSave;
    private ToolStripMenuItem menuLoad;
    private ToolStripMenuItem menuHelp;
   // private ToolStripMenuItem menuLoadOpen;
    private ToolStripMenuItem menuLoadCollection;
   // private ToolStripMenuItem menuSaveAs;
   // private ToolStripMenuItem menuHelpAbout;
   // private ToolStripLabel menuLabel;
    private ToolStripTextBox menuLayout;
    private ToolStripTextBox menuDummy;
    private Label labelLeft;
    private Label labelRight;
    private Label labelVS;
    private ToolStrip toolStripButton;
    private ToolStripLabel toolStripButtonLabel;

    // -----------

    double rcpClHeight; // for scaling fonts
    double clRatio; // main window
    Size ncSize; // non-client area
    readonly int minWndWidth = 200;
    int minWndHeight = 0;
    readonly Dictionary<Label, Font> scalableLabels = new();
    readonly Dictionary<ToolStripLabel, Font> scalableTSLabels = new();

    struct ScalableTSButton
    {
        internal Control control;
        internal Size size;
        internal Padding padding;

        internal ScalableTSButton(Control _control, Size _size, Padding _padding)
        {
            control = _control;
            size = _size;
            padding = _padding;
        }
    }
    readonly List<ScalableTSButton> scalableTSButtons = new();

    readonly CellWrapper[,] cellWrap = new CellWrapper[3, 3];

    readonly LabelManager labMgr;

    static readonly Dictionary<KeyValuePair<Game.Roster, Game.Roster>, Image> mainBg = new();

    static internal void ApplyDoubleBuffer(object control)
    {
        var propName = "DoubleBuffered";
        var bindFlags = BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic;
        var type = control.GetType();
        var prop = type.GetProperty(propName, bindFlags);
        prop?.SetValue(control, true);
    }

    UIColors.ColorTheme theme;

    SetupForm? setupForm;

    IEnumerable<ChoiceItem> chosen = Enumerable.Empty<ChoiceItem>();

    ChoiceItem[] chosenArr = Array.Empty<ChoiceItem>();

    bool firstChosenIsLeft = false;

    readonly List<AI> AIs = new();

    void SetupFormPopup()
    {
        setupForm ??= new SetupForm();

        // if (setupForm.ShowDialog(this) == DialogResult.OK) return;
        return;
    }

    readonly ButtonToolStripRenderer buttonRenderer;

    internal AppForm()
    {
        EM.uiThread = this;

        theme = UIColors.Steel;
        ForeColor = theme.Text;

        // init label manager
        labMgr = new();

        // to extend the behaviour of sub components
        ControlAdded += FormAspect_ControlAdded;

        // prevent main window flickering
        DoubleBuffered = true;

        //InitializeComponent();

        InitializeMenu();

        //BuildProfileListMenu();

        // prevent background flickering
        var doubleBuffed = new object[] { tLayout, labelLeft, labelRight, labelVS, toolStripButton, toolStripButtonLabel };
        foreach (var ctrl in doubleBuffed) ApplyDoubleBuffer(tLayout);

        // BLOCKS: player setup pop-up 
        SetupFormPopup();

        // restart game button
        buttonRenderer = UIRenderer.ButtonTSRenderer(toolStripButton, ButtonColors.Sunrise);
        SetupRestart();

        // adjust labels & setup percentage positioning
        SetupLabels();

        // LabelManager properties -> Labels
        SetupBinds();

        // Event subscriptions & callbacks
        SetupSubsAndCb();

        // MULTI-USE: resets everything and starts the game
        StartGame();
    }

    void InitializeMenu()
    {
        // menu appearance
        menuStrip1.BackColor = theme.Prime;
        menuStrip1.ForeColor = theme.Text;
        menuStrip1.Renderer = UIRenderer.MenuTSRenderer(theme, "MenuColorTable");
        menuStrip1.Font = UIFonts.menu;

        ToolStripMenuItem[] expandableItems = new[] { menuLoad, menuLoadCollection, menuSave, menuHelp };

        foreach (var item in expandableItems)
        {
           // if (item.DropDown is ToolStripDropDownMenu dropDownMenu)
           //     dropDownMenu.ShowImageMargin = false;

           // foreach (ToolStripItem subItem in item.DropDownItems) subItem.ForeColor = theme.Text;
        }

        menuLayout.BackColor = theme.Dark;
        menuLayout.ForeColor = theme.Text;
        menuLayout.BorderStyle = BorderStyle.None;

        menuDummy.BackColor = theme.Dark;
        menuDummy.ForeColor = theme.Text;
        menuDummy.BorderStyle = BorderStyle.None;
    }

    void SetupRestart()
    {
        toolStripButton.Renderer = buttonRenderer;
        toolStripButton.BackColor = toolStripButtonLabel.BackColor = UIColors.Transparent;
        buttonRenderer.SetOverState(toolStripButton, false);

        // center over VS
        toolStripButton.Location = new Point(
            labelVS.Location.X + (labelVS.Width - toolStripButton.Width) / 2,
            labelVS.Location.Y + menuStrip1.Height + (labelVS.Height - toolStripButton.Height) / 2
            );

        toolStripButton.MouseEnter += (object? s, EventArgs e) => buttonRenderer.SetOverState(toolStripButton, true);
        toolStripButton.MouseLeave += (object? s, EventArgs e) => buttonRenderer.SetOverState(toolStripButton, false);
    }

    void ShowEndGameButton(bool state)
    {
        toolStripButton.Enabled = state;
        toolStripButton.Visible = state;
    }

    void ResetUI()
    {
        ShowEndGameButton(false);

        foreach (var cw in cellWrap)
            if (cw is IComponent iComp) iComp.Reset();
    }
    void EnableUI()
    {
        foreach (var cw in cellWrap)
            if (cw is IComponent iComp) iComp.Enable();
    }
    void DisableUI()
    {
        foreach (var cw in cellWrap)
            if (cw is IComponent iComp) iComp.Disable();
    }

    void StartGame()
    {
        // retrieve players list
        AssertPlayers();

        // create player defined bg
        CreateBackground();

        // ready new game
        Reset();

        // (re)create AIs, if needed

        foreach (var aiAgent in AIs)
            EM.Unsubscribe(Evt.AIMakeMove, aiAgent.MoveHandler);
        AIs.Clear();

        foreach (var chItm in chosenArr)
            if (chItm.OriginType == "AI")
            {
                var logic = chItm.RosterId == Game.Roster.AI_One ? AI.Logic.RNG : AI.Logic.Easy;
                var aiAgent = new AI(logic, chItm.RosterId);
                AIs.Add(aiAgent);

                EM.Subscribe(Evt.AIMakeMove, aiAgent.MoveHandler);
            }

        // start game
        TurnWheel.GameCountdown();
    }

    class SaveGame : Utils.INamedProfile
    {
        public string Name { get; set; } = "";
        public Game.Roster[] Board { get; set; } = Array.Empty<Game.Roster>();
        public Game.Roster[] TurnList { get; set; } = Array.Empty<Game.Roster>();
        public Game.State State { get; set; }
        public int TurnWheelHead { get; set; }
        public IEnumerable<ChoiceItem> Chosen { get; set; } = Enumerable.Empty<ChoiceItem>();

        public SaveGame(
            string _name,
            Game.Roster[] _board,
            Game.Roster[] _turnList,
            Game.State _state,
            int _turnWheelHead,
            IEnumerable<ChoiceItem> _chosen
        )
        {
            Name = _name;

            var len = _board.Length;
            Board = new Game.Roster[len];
            Array.Copy(_board, Board, len);

            len = _turnList.Length;
            TurnList = new Game.Roster[len];
            Array.Copy(_turnList, TurnList, len);

            State = _state;
            TurnWheelHead = _turnWheelHead;

            Chosen = _chosen;
        }

        public SaveGame()
        {
            // for JsonSerializer.Deserialize<P>(input);
        }
    }

    void LoadGame(SaveGame prof)
    {
        // retrieve players list
        AssertPlayers(prof.Chosen);

        // create player defined bg
        CreateBackground();

        // ---- Reset() ---->

        // rebuild visual bridge for translation between
        // Game <-> (CellWrapper, LabelManager)
        VBridge.Reset(chosen);

        // reset the game and the board
        Game.Reset(
            chosen.Select(chItm => chItm.RosterId).ToArray(),
            prof.Board
        );
        // Game.SetTurns("random");

        TurnWheel.Reset(prof.TurnWheelHead);

        // ---- ResetUI() ---->

        var gameOver = prof.State == Game.State.Won || prof.State == Game.State.Tie;

        ShowEndGameButton(gameOver);

        foreach (var cw in cellWrap)
            if (cw is IComponent iComp)
            {
                iComp.Reset();
                var owned = Game.board[cw.RC.X, cw.RC.Y] != Game.Roster.None;
                if (owned || gameOver)
                {
                    iComp.Disable();
                    iComp.IsLocked = true;

                } else 
                    iComp.Enable();
            }

        // <---- ResetUI() ----

        // <---- Reset() ----

        // (re)create AIs, if needed

        foreach (var aiAgent in AIs)
            EM.Unsubscribe(Evt.AIMakeMove, aiAgent.MoveHandler);
        AIs.Clear();

        foreach (var chItm in chosenArr)
            if (chItm.OriginType == "AI")
            {
                var logic = chItm.RosterId == Game.Roster.AI_One ? AI.Logic.RNG : AI.Logic.Easy;
                var aiAgent = new AI(logic, chItm.RosterId);
                AIs.Add(aiAgent);

                EM.Subscribe(Evt.AIMakeMove, aiAgent.MoveHandler);
            }

        // start game, if necessary
        switch (prof.State)
        {
            case Game.State.Won:
                EM.Raise(Evt.GameOver, new { }, TurnWheel.CurPlayer);
                Game.GreyOutLostTiles(TurnWheel.CurPlayer);
                break;
            case Game.State.Tie:
                EM.Raise(Evt.GameTie, new { }, new EventArgs());
                break;
            case Game.State.Countdown:
                TurnWheel.Advance();
                break;
            case Game.State.Started:
                TurnWheel.AssertPlayer();
                break;
        }
    }

    EventHandler<Game.Roster> GameOverHandler() => (object? _, Game.Roster __) => ShowEndGameButton(true);
    EventHandler GameTieHandler() => (object? _, EventArgs e) => ShowEndGameButton(true);

    void SetupSubsAndCb()
    {
       // Menu.cs
       // EM.Subscribe(Evt.GStateChanged, GStateChangedHandler());

        EM.Subscribe(Evt.GameOver, GameOverHandler());
        EM.Subscribe(Evt.GameTie, GameTieHandler());

        EM.Subscribe(Evt.UpdateLabels, LabelManager.UpdateLabelsHandler);

        EM.Subscribe(Evt.SyncBoard, VBridge.SyncBoardHandler);
        EM.Subscribe(Evt.SyncBoardWin, VBridge.SyncBoardWinHandler);
        EM.Subscribe(Evt.SyncMoveLabels, VBridge.SyncMoveLabelsHandler);
        EM.Subscribe(Evt.GameOver, VBridge.GameOverHandler);
        EM.Subscribe(Evt.GameTie, VBridge.GameTieHandler);

        foreach (var cw in cellWrap)
        {
            EM.Subscribe(Evt.AIMoved, cw.AIMovedHandler);
            EM.Subscribe(Evt.SyncBoardUI, cw.SyncBoardUIHandler);
        }

        EM.Subscribe(Evt.PlayerMoved, TurnWheel.PlayerMovedHandler);

        TurnWheel.SetCallbacks(EnableUI, DisableUI);

        toolStripButton.Click += (object? _, EventArgs __) =>
        {
            SetupFormPopup();
            StartGame();
        };
    }

    void SetupBinds()
    {
        //info.DataBindings.Add(new Binding("BackColor", labMgr, "InfoBackBind"));
        //info.DataBindings.Add(new Binding("Text", labMgr, "InfoPanelBind"));
        //labelLeft.DataBindings.Add(new Binding("Text", labMgr, "LabelPlayer1Bind"));
        //labelLeft.DataBindings.Add(new Binding("ForeColor", labMgr, "Player1ForeBind"));
        //labelRight.DataBindings.Add(new Binding("Text", labMgr, "LabelPlayer2Bind"));
        //labelRight.DataBindings.Add(new Binding("ForeColor", labMgr, "Player2ForeBind"));
    }

    void Reset()
    {
        // rebuild visual bridge for translation between
        // Game <-> (CellWrapper, LabelManager)
        VBridge.Reset(chosen);

        // reset the game and the board
        Game.Reset(chosen.Select(chItm => chItm.RosterId).ToArray());
        // Game.SetTurns("random");

        TurnWheel.Reset();

        ResetUI();
    }

    void AssertPlayers(IEnumerable<ChoiceItem>? _chosen = null)
    {
        chosen = _chosen ?? SetupForm.roster.Where(itm => itm.chosen);
        chosenArr = chosen.ToArray();
        if (chosenArr.Length != 2)
            throw new Exception($"Form.AssertPlayers : wrong number of players '{chosenArr.Length}'");

        firstChosenIsLeft = chosenArr[0].side == ChoiceItem.Side.Left;
    }

    void CreateBackground()
    {
        KeyValuePair<Game.Roster, Game.Roster> leftRightBg = firstChosenIsLeft ?
            new(chosenArr[0].RosterId, chosenArr[1].RosterId) :
            new(chosenArr[1].RosterId, chosenArr[0].RosterId);

        foreach (var (_leftRightBg, bgImage) in mainBg)
            if (_leftRightBg.Equals(leftRightBg)) // cache exists
            {
                BackgroundImage = bgImage;
                return;
            }

        BackgroundImage = Resource.GameBackImg;

        int botPanelHeight = 206;
        var botPanelOff = new Point(0, Resource.GameBackImg.Height - botPanelHeight);
        var botPanelSize = new Size(Resource.GameBackImg.Width, botPanelHeight);

        var rect = new Rectangle(botPanelOff, botPanelSize);

        Color dimColor = Color.FromArgb(128, 0, 0, 0);
        using var brush = new SolidBrush(dimColor);
        using var g = Graphics.FromImage(BackgroundImage);

        g.FillRectangle(brush, rect);

        Image?[] headImage = chosen.Select(itm =>
            (Image?)Resource.ResourceManager.GetObject($"{itm.RosterId}_{itm.side}_Head")).ToArray();

        KeyValuePair<Image?, Image?> leftRightImage = firstChosenIsLeft ?
            new(headImage[0], headImage[1]) : new(headImage[1], headImage[0]);

        if (leftRightImage.Key != null && leftRightImage.Value != null)
        {
            leftRightImage.Key.GetOverlayOnBackground(
                BackgroundImage,
                botPanelOff,
                new Size(BackgroundImage.Width / 2, botPanelHeight),
                "left",
                "top");

            leftRightImage.Value.GetOverlayOnBackground(
                BackgroundImage,
                new Point(Resource.GameBackImg.Width / 2, botPanelOff.Y),
                new Size(BackgroundImage.Width / 2, botPanelHeight),
                "right",
                "top");

            mainBg.Add(leftRightBg, BackgroundImage);
        }

    }

    void SetupLabels()
    {
        labelLeft.BackColor = labelRight.BackColor = UIColors.Transparent;
        labelLeft.Font = labelRight.Font = UIFonts.regular;
        labelVS.Font = new Font("Arial", 24F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point);
        toolStripButton.Font = UIFonts.button;

        labelLeft.Anchor = labelRight.Anchor = AnchorStyles.None;

        int botPanelHeight = 206;
        var off = new Point(160, 50);
        labelLeft.Location = new Point(off.X, ClientSize.Height - botPanelHeight + off.Y);
        labelRight.Location = new Point(ClientSize.Width - labelRight.Width - off.X, ClientSize.Height - labelRight.Height - off.Y);

        RatioPosition.Add(labelLeft, this, RatioPosControl.Anchor.Left, RatioPosControl.Anchor.Top);
        RatioPosition.Add(labelRight, this, RatioPosControl.Anchor.Right, RatioPosControl.Anchor.Bottom);
        RatioPosition.Add(toolStripButton, this, RatioPosControl.Anchor.Middle, RatioPosControl.Anchor.Middle);

        info.Font = UIFonts.info;
        info.ForeColor = theme.Text;
    }

    void FormAspect_ClientSizeChanged(object? sender, EventArgs e)
    {
        // adjust components
        RatioPosition.Update(labelLeft);
        RatioPosition.Update(labelRight);
        RatioPosition.Update(toolStripButton);
    }

    void FormAspect_ControlAdded(object? sender, ControlEventArgs e)
    {
        if (e.Control == tLayout)
        {
            // --------- board cells ----------

            CellBg.CreateBgSet(new Size(142, 135));

            int tabInd = 0;
            cells = new Panel[3, 3];
            for (int row = 0; row < 3; row++)
                for (int col = 0; col < 3; col++)
                {
                    Panel p = cells[row, col] = new Panel();
                   // p.BackgroundImageLayout = ImageLayout.Stretch;
                   // p.Dock = DockStyle.Fill;
                   // p.Margin = new Padding(12);
                    p.Name = $"cell{row}{col}";
                    p.Size = new Size(109, 108);
                    p.TabIndex = tabInd++;

                  //  tLayout.Controls.Add(p, col, row);

                    ApplyDoubleBuffer(p);

                    cellWrap[row, col] = new CellWrapper(p, row, col);
                }
        }
    }

    void FormAspect_Load(object sender, EventArgs e)
    {
        var clHeight = ClientSize.Height - menuStrip1.Height;
        var clSize = new Size(ClientSize.Width, clHeight);
        clRatio = (double)clSize.Width / clSize.Height;
        ncSize = Size - clSize;

        minWndHeight = (int)(minWndWidth / clRatio);

        Label[] labs = new[] { info, labelLeft, labelRight, labelVS };
        foreach (var lab in labs)
            scalableLabels.Add(lab, lab.Font);
        scalableTSLabels.Add(toolStripButtonLabel, toolStripButtonLabel.Font);
        //scalableTSButtons.Add(new ScalableTSButton(
        //    toolStripButton,
        //    toolStripButton.Size,
        //    toolStripButtonLabel.Margin
        //));
        rcpClHeight = 1.0 / clHeight;
    }

    // ---------------   constant client aspect ratio   ---------------

    [StructLayout(LayoutKind.Sequential)]
    struct RECT
    {
        internal int Left;
        internal int Top;
        internal int Right;
        internal int Bottom;
    }
    private const int WM_SIZING = 0x214;
    enum WMSZ
    {
        LEFT = 1,
        RIGHT = 2,
        TOP = 3,
        TOPLEFT = 4,
        TOPRIGHT = 5,
        BOTTOM = 6,
        BOTTOMLEFT = 7,
        BOTTOMRIGHT = 8
    }

    // ----------

    // protected override
    void WndProc(ref Message m)
    {
        if (m.Msg == WM_SIZING)
        {
            var rc = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT))!;

            if (rc.Right - rc.Left < minWndWidth) rc.Right = rc.Left + minWndWidth;
            if (rc.Bottom - rc.Top < minWndHeight) rc.Bottom = rc.Top + minWndHeight;

            int clWidth, clHeight, newWidth, newHeight;
            clHeight = default;
            bool scale = false;

            switch ((WMSZ)m.WParam.ToInt32())
            {
                case WMSZ.LEFT:
                case WMSZ.RIGHT:
                    // width has changed, adjust height
                    clWidth = rc.Right - rc.Left - ncSize.Width;
                    clHeight = (int)(clWidth / clRatio);
                    newHeight = clHeight + ncSize.Height;
                    rc.Bottom = rc.Top + newHeight;
                    scale = true;
                    break;
                case WMSZ.TOP:
                case WMSZ.BOTTOM:
                case WMSZ.TOPLEFT:
                case WMSZ.TOPRIGHT:
                case WMSZ.BOTTOMLEFT:
                case WMSZ.BOTTOMRIGHT:
                    // height has changed, adjust width
                    clHeight = rc.Bottom - rc.Top - ncSize.Height;
                    newWidth = (int)(clHeight * clRatio) + ncSize.Width;
                    rc.Right = rc.Left + newWidth;
                    scale = true;
                    break;
            }
            Marshal.StructureToPtr(rc, m.LParam, true);

            if (scale)
            {
                var fact = (float)(clHeight * rcpClHeight);

                foreach (var (lab, font) in scalableLabels)
                    lab.Font = new Font(font.FontFamily, font.Size * fact, font.Style);
                foreach (var (lab, font) in scalableTSLabels)
                    lab.Font = new Font(font.FontFamily, font.Size * fact, font.Style);
                foreach (var rec in scalableTSButtons)
                {
                    var size = rec.size;
                    rec.control.Size = new Size((int)(size.Width * fact), (int)(size.Height * fact));
                    var pad = rec.padding;
                    //toolStripButtonLabel.Margin = new Padding(
                    //    (int)(pad.Left * fact),
                    //    (int)(pad.Top * fact),
                    //    (int)(pad.Right * fact),
                    //    (int)(pad.Bottom * fact)
                    //);
                }
            }
        }

        // base.WndProc(ref m);
    }

}