namespace asp_net_sql.GameEngine;

/// <summary>
/// Defines players roster, evaluates the board and winning conditions
/// </summary>
class Game
{
    /// <summary>
    /// Pattern for human player names Human*, for AI players - AI*
    /// </summary>

    static internal readonly Dictionary<Roster, string> rosterIdentity = new()
    {
        { Roster.Human_1, "Ironheart" },
        { Roster.Human_2, "Silverlight" },
        { Roster.AI_1, "Quantum" },
        { Roster.AI_2, "Syncstorm" }
    };

    static Roster[] _turnList = Array.Empty<Roster>();

    internal enum State
    {
        Countdown,
        Started,
        Won,
        Tie
    }

    static internal State _state = State.Countdown;
    static internal State GState
    {
        get => _state;
        set
        {
            _state = value;
            EM.Raise(Evt.GStateChanged, new { }, _state);
        }
    }

    /// <summary>
    /// Players from Roster in the order of their turns;<br/>
    /// can be overwritten by SetTurns()
    /// </summary>
    static internal Roster[] TurnList
    {
        get => _turnList;
        set => _turnList = value;
    }

    static internal readonly Board board = new(3, 3, Roster.None);

    static internal readonly Line[] lines = new Line[]
    {
        // rows
        new(board, new Tile[] { new(0,0), new(0,1), new(0,2) }),
        new(board, new Tile[] { new(1,0), new(1,1), new(1,2) }),
        new(board, new Tile[] { new(2,0), new(2,1), new(2,2) }),
        // columns
        new(board, new Tile[] { new(0,0), new(1,0), new(2,0) }),
        new(board, new Tile[] { new(0,1), new(1,1), new(2,1) }),
        new(board, new Tile[] { new(0,2), new(1,2), new(2,2) }),
        // diagonals
        new(board, new Tile[] { new(2,0), new(1,1), new(0,2) }), // Fwd
        new(board, new Tile[] { new(0,0), new(1,1), new(2,2) }), // Bwd

    };

    static internal readonly ArraySegment<Line> rows = new(lines, 0, 3);
    static internal readonly ArraySegment<Line> cols = new(lines, 3, 3);
    static internal readonly ArraySegment<Line> diags = new(lines, 6, 2);

    static internal bool CanTakeBoardTile(int index, out Roster owner)
    {
        owner = board[index];
        return owner == Roster.None;
    }
    static internal bool CanTakeBoardTile(Tile pnt, out Roster owner)
    {
        owner = board[pnt.row, pnt.col];
        return owner == Roster.None;
    }

    static internal bool CanTakeLineTile(Line line, int index, out Roster owner)
    {
        owner = line[index];
        return owner == Roster.None;
    }

    static internal List<LineInfo> ExamineLines()
    {
        var linesInfo = new List<LineInfo>();

        foreach (var line in lines)
        {
            var info = new LineInfo(line);

            for (int i = 0; i < line.Length; i++)
            {
                var canTake = CanTakeLineTile(line, i, out Roster player);

                if (canTake) info.canTake.Add(i);

                if (TurnList.Contains(player)) // exclude Roster.None
                {
                    if (!info.takenStats.ContainsKey(player)) info.takenStats.Add(player, 0);
                    info.takenStats[player]++;
                }
            }

            if (info.takenStats.Count > 0) info.dominant = info.takenStats.MaxBy(rec => rec.Value);

            linesInfo.Add(info);
        }

        return linesInfo;
    }

    static internal void Reset(Roster[] turnlist, Roster[]? _board = null)
    {
        TurnList = turnlist;

        ResetBoard(_board);

        // add all the board cells to the update
        var update = new Dictionary<Tile, Roster>();

        for (int i = 0; i < board.width; i++)
            for (int j = 0; j < board.height; j++)
                update.Add(new Tile(i, j), board[i, j]);

        // sync the board
        EM.Raise(Evt.SyncBoard, new { }, update);
    }

    /// <summary>
    /// Sets the order of players turns
    /// </summary>
    static internal void SetTurns(string mode)
    {
        switch (mode)
        {
            case "random":
                // random shuffle
                Array.Sort(_turnList, (x, y) => Guid.NewGuid().CompareTo(Guid.NewGuid()));
                break;
            default:
                throw new NotImplementedException($"Game.SetTurns : mode '{mode}'");
        }
    }

    static void ResetBoard(Roster[]? _board)
    {
        if (_board != null)
            for (int i = 0; i < board.Length; i++) board[i] = _board[i];
        else
            for (int i = 0; i < board.Length; i++) board[i] = Roster.None;
    }

    /// <summary>
    /// Called by TurnWheel.PlayerMovedHandler.<br/>
    /// Assert the game state, execute game over or<br/>
    /// advance TurnWheel otherwise
    /// </summary>
    static internal void Update(Roster curPlayer, Tile rc)
    {
        board[rc.row, rc.col] = curPlayer;

        // add the cell to the update & sync cells via VBridge
        var update = new Dictionary<Tile, Roster>() { { rc, curPlayer } };
        EM.Raise(Evt.SyncBoard, new { }, update);

        var linesInfo = ExamineLines();

        var dominant = linesInfo.MaxBy(rec => rec.dominant.Value)?.dominant;

        var gameWon = dominant?.Value == 3;

        if (gameWon)
        {
            GState = State.Won;
            EM.Raise(Evt.GameOver, new { }, curPlayer);

            GreyOutLostTiles(curPlayer);

            return;
        }

        var maxCanTake = linesInfo.MaxBy(rec => rec.canTake.Count)?.canTake.Count;

        if (maxCanTake > 0)
        {
            TurnWheel.Advance();

        } else
        {
            GState = State.Tie;
            EM.Raise(Evt.GameTie, new { }, new EventArgs());
        }
    }

    static internal void GreyOutLostTiles(Roster curPlayer)
    {
        var lostTiles = new Dictionary<Tile, Roster>();
        for (var i = 0; i < board.Length; i++)
        {
            var bi = board[i];
            if (bi != curPlayer && bi != Roster.None) lostTiles.Add(board.GetTile(i), bi);
        }

        EM.Raise(Evt.SyncBoardWin, new { }, lostTiles);
    }
}
