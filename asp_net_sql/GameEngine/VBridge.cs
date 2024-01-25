
using System.Drawing;

namespace asp_net_sql.GameEngine;

/// <summary>
/// Provides bridges between game and UI states
/// </summary>
internal class VBridge
{
    // CellWrapper

    /// <summary>
    /// Auxiliary map
    /// </summary>
    static readonly Dictionary<Side, CellWrapper.BgMode> sideToBg = new()
    {
        { Side.None, CellWrapper.BgMode.Default },
        { Side.Left, CellWrapper.BgMode.Player1 },
        { Side.Right, CellWrapper.BgMode.Player2 },
    };

    // LabelManager

    /// <summary>
    /// Auxiliary map
    /// </summary>
    static readonly Dictionary<Side, LabelManager.Info> sideToPlayer = new()
    {
        { Side.Left, LabelManager.Info.Player1 },
        { Side.Right, LabelManager.Info.Player2 }
    };

    /// <summary>
    /// Memorise players' sides for mapping them into cell bgs & message labels
    /// </summary>
    static readonly Dictionary<Roster, Side> rosterToSide = new();

    // labels colors
    static readonly Color infoBackNone = Color.FromArgb(80, 0, 0, 0);

    static readonly Color infoBackLeft = ColorExtensions.BlendOver(
        ColorExtensions.Scale(UIColors.TintLeft, 0.8), infoBackNone);

    static readonly Color infoBackRight = ColorExtensions.BlendOver(
        ColorExtensions.Scale(UIColors.TintRight, 0.8), infoBackNone);

    static readonly Color dim = Color.FromArgb(96, 0, 0, 0);
    static readonly Color foreLeftDim = ColorExtensions.BlendOver(dim, UIColors.ForeLeft);
    static readonly Color foreRightDim = ColorExtensions.BlendOver(dim, UIColors.ForeRight);

    /// <summary>
    /// Fill in LabelManager.stateToString messages that depend on player names and their sides
    /// </summary>
    static internal void Reset(IEnumerable<ChoiceItem> chosen)
    {
        rosterToSide.Clear();
        rosterToSide.Add(Roster.None, Side.None);

        var enumInfo = new Dictionary<Enum, object>
        {
            { LabelManager.Bg.None, infoBackNone }
        };

        foreach (var chItem in chosen)
        {
            var side = chItem.SideId;

            rosterToSide.Add(chItem.RosterId, side);

            var state = Utils.SafeDictValue(sideToPlayer, side);
            var stateMove = Utils.SafeEnumFromStr<LabelManager.Info>($"{state}Move");
            var stateWon = Utils.SafeEnumFromStr<LabelManager.Info>($"{state}Won");

            var msgMove = chItem.OriginType == "AI" ?
                $"{chItem.IdentityName} is moving..." : $"Your move, {chItem.IdentityName}...";

            var msgWon = $"Player {chItem.IdentityName} is the winner! Congratulations!";

            enumInfo.Add(state, chItem.IdentityName);
            enumInfo.Add(stateMove, msgMove);
            enumInfo.Add(stateWon, msgWon);

            // enum Bg colors
            var stateBg = Utils.SafeEnumFromStr<LabelManager.Bg>($"{state}InfoBack");
            var stateFore = Utils.SafeEnumFromStr<LabelManager.Bg>($"{state}Fore");
            var stateForeDim = Utils.SafeEnumFromStr<LabelManager.Bg>($"{state}ForeDim");

            var sideIsLeft = side == Side.Left;
            var infoBackColor = sideIsLeft ? infoBackLeft : infoBackRight;
            var foreColor = sideIsLeft ? UIColors.ForeLeft : UIColors.ForeRight;
            var foreColorDim = sideIsLeft ? foreLeftDim : foreRightDim;

            enumInfo.Add(stateBg, infoBackColor);
            enumInfo.Add(stateFore, foreColor);
            enumInfo.Add(stateForeDim, foreColorDim);
        }

        LabelManager.Reset(enumInfo);

        EM.Raise(Evt.UpdateLabels, new { }, new Enum[] {
            LabelManager.Info.None,
            LabelManager.Info.Player1,
            LabelManager.Info.Player2,
            LabelManager.Bg.None,
            LabelManager.Bg.Player1Fore,
            LabelManager.Bg.Player2Fore,
        });
    }

    /// <summary>
    /// Subscribed to EvtSyncBoard event<br/>
    /// Translates game board state into UI states
    /// </summary>
    static internal EventHandler<Dictionary<Tile, Roster>> SyncBoardHandler =
    (object? s, Dictionary<Tile, Roster> e) =>
    {
        Dictionary<Point, CellWrapper.BgMode> cellBgs = new();

        foreach (var (rc, rostId) in e)
        {
            var side = Utils.SafeDictValue(rosterToSide, rostId);
            var bg = Utils.SafeDictValue(sideToBg, side);
            cellBgs.Add(new Point(rc.row, rc.col), bg);
        }

        EM.Raise(Evt.SyncBoardUI, s ?? new { }, cellBgs);
    };

    /// <summary>
    /// Subscribed to EvtSyncBoardWin event<br/>
    /// Translates game board state into UI states;<br/>
    /// applies greyed bgs to cells owned by the winner
    /// </summary>
    static internal EventHandler<Dictionary<Tile, Roster>> SyncBoardWinHandler =
    (object? s, Dictionary<Tile, Roster> e) =>
    {
        Dictionary<Point, CellWrapper.BgMode> cellBgs = new();

        foreach (var (rc, rId) in e)
        {
            var side = Utils.SafeDictValue(rosterToSide, rId);
            var bgFullColor = Utils.SafeDictValue(sideToBg, side);
            var bgGreyedOut = Utils.SafeEnumFromStr<CellWrapper.BgMode>($"{bgFullColor}Lost");
            cellBgs.Add(new Point(rc.row, rc.col), bgGreyedOut);
        }

        EM.Raise(Evt.SyncBoardUI, s ?? new { }, cellBgs);
    };

    /// <summary>
    /// Subscribed to EM.SyncMoveLabels event raised by TurnWheel to update labels on player move
    /// </summary>
    static internal EventHandler<Roster> SyncMoveLabelsHandler =
    (object? _, Roster rostId) =>
    {
        var side = Utils.SafeDictValue(rosterToSide, rostId);
        var state = Utils.SafeDictValue(sideToPlayer, side);
        var stateMove = Utils.SafeEnumFromStr<LabelManager.Info>($"{state}Move");
        var stateBg = Utils.SafeEnumFromStr<LabelManager.Bg>($"{state}InfoBack");

        EM.Raise(Evt.UpdateLabels, new { }, new Enum[] { stateMove, stateBg });
    };

    static internal EventHandler<Roster> GameOverHandler =
    (object? _, Roster winner) =>
    {
        var side = Utils.SafeDictValue(rosterToSide, winner);
        var state = Utils.SafeDictValue(sideToPlayer, side);
        var stateWon = Utils.SafeEnumFromStr<LabelManager.Info>($"{state}Won");
        var stateBg = Utils.SafeEnumFromStr<LabelManager.Bg>($"{state}InfoBack");

        EM.Raise(Evt.UpdateLabels, new { }, new Enum[] {
            stateWon,
            stateBg,
            LabelManager.Bg.Player1ForeDim,
            LabelManager.Bg.Player2ForeDim,
        });
    };

    static internal EventHandler GameTieHandler =
    (object? _, EventArgs __) =>
    {
        EM.Raise(Evt.UpdateLabels, new { }, new Enum[] {
            LabelManager.Info.Tie,
            LabelManager.Bg.None,
            LabelManager.Bg.Player1ForeDim,
            LabelManager.Bg.Player2ForeDim,
        });
    };
}
