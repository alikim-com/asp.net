using System.Drawing;

using System.ComponentModel;

namespace asp_net_sql.GameEngine;

/// <summary>
/// Updates UI labels based on subscribed events
/// </summary>
class LabelManager : INotifyPropertyChanged
{
    /// <summary>
    /// Middle & bottom info panel states
    /// </summary>
    internal enum Info
    {
        None,
        Tie,
        //
        Player1,
        Player2,
        Player1Move,
        Player2Move,
        Player1Won,
        Player2Won,
    }
    /// <summary>
    /// Pre-game countdown info panel states
    /// </summary>
    internal enum Countdown
    {
        Three,
        Two,
        One
    }
    /// <summary>
    /// Messages from AI to append to the current info
    /// </summary>
    internal enum AIMsg
    {
        Attack,
        Defend,
        Random,
    }
    /// <summary>
    /// Manages info bg color
    /// </summary>
    internal enum Bg
    {
        None,
        Player1InfoBack,
        Player2InfoBack,
        Player1Fore,
        Player2Fore,
        Player1ForeDim,
        Player2ForeDim,
    }

    static readonly Dictionary<Enum, string> stateToString = new()
    {
        { Info.None, "" },
        { Info.Tie, "It's a tie! No winner this time."},
        // the rest is filled by VBridge.Reset()

        { AIMsg.Attack, " (attacking)"},
        { AIMsg.Defend, " (defending)"},
        { AIMsg.Random, " (random choice)"},

        { Countdown.Three, "Game starts in 3..." },
        { Countdown.Two, "Game starts in 2..." },
        { Countdown.One, "Game starts in 1..." },
    };

    static readonly Dictionary<Enum, Color> stateToColor = new()
    {
        // enum Bg, filled by VBridge.Reset()
    };

    /// <summary>
    /// Subscribed to EM.EvtUpdateLabels event
    /// </summary>
    /// <param name="e">An array of states to set for each panel</param>
    static internal readonly EventHandler<Enum[]> UpdateLabelsHandler = (object? _, Enum[] e) =>
    {
        foreach (Enum state in e) SetLabel(state);
    };

    /// <summary>
    /// Called from VBridge.Reset
    /// defines Info labels when the game starts
    /// </summary>
    static internal void Reset(Dictionary<Enum, object> enumInfo)
    {
        foreach (var (enm, obj) in enumInfo)
        {
            if (obj is string msg)
            {
                if (!stateToString.ContainsKey(enm))
                    stateToString.Add(enm, msg);
                else
                    stateToString[enm] = msg;
            } else if (obj is Color backInfoColor)
            {
                if (!stateToColor.ContainsKey(enm))
                    stateToColor.Add(enm, backInfoColor);
                else
                    stateToColor[enm] = backInfoColor;
            }
        }
    }

    static void SetLabel(Enum state)
    {
        if (_this == null) return;
        switch (state)
        {
            case AIMsg:
                _this.InfoPanelBind += stateToString[state];
                RaiseEvtPropertyChanged(nameof(InfoPanelBind));
                break;
            case Info.Player1:
                _this.LabelPlayer1Bind = stateToString[state];
                RaiseEvtPropertyChanged(nameof(LabelPlayer1Bind));
                break;
            case Info.Player2:
                _this.LabelPlayer2Bind = stateToString[state];
                RaiseEvtPropertyChanged(nameof(LabelPlayer2Bind));
                break;
            case Info:
            case Countdown:
                _this.InfoPanelBind = stateToString[state];
                RaiseEvtPropertyChanged(nameof(InfoPanelBind));
                break;
            case Bg:
                if (state.ToString().Contains("Player1Fore"))
                {
                    _this.Player1ForeBind = stateToColor[state];
                    RaiseEvtPropertyChanged(nameof(Player1ForeBind));

                } else if (state.ToString().Contains("Player2Fore"))
                {
                    _this.Player2ForeBind = stateToColor[state];
                    RaiseEvtPropertyChanged(nameof(Player2ForeBind));
                } else
                {
                    _this.InfoBackBind = stateToColor[state];
                    RaiseEvtPropertyChanged(nameof(InfoBackBind));
                }
                break;
            default:
                throw new NotImplementedException($"LabelManager.SetLabels : state '{state}'");
        }
    }


    /* 
     * A workaround to implement the data binding interface that requires
     * PropertyChanged event and binding properties to be instanced
     * 
     */

    static void RaiseEvtPropertyChanged(string property)
    {
        var handler = _this?.PropertyChanged;
        handler?.Invoke(_this, new PropertyChangedEventArgs(property));
    }

    static LabelManager? _this;

    /// <summary>
    /// Data bindings
    /// </summary>
    public string LabelPlayer1Bind { get; set; } = "";
    public string LabelPlayer2Bind { get; set; } = "";
    public string InfoPanelBind { get; set; } = "";
    public Color InfoBackBind { get; set; } = UIColors.Transparent;
    public Color Player1ForeBind { get; set; } = UIColors.Transparent;
    public Color Player2ForeBind { get; set; } = UIColors.Transparent;

    public event PropertyChangedEventHandler? PropertyChanged;

    internal LabelManager()
    {
        _this = this;
    }
}
