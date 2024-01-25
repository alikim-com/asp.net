using System.Drawing;

namespace asp_net_sql.GameEngine;

class AI
{
    internal enum Logic
    {
        None,
        RNG,
        Easy
    }

    readonly Logic logic;
    readonly Roster selfId;
    internal EventHandler<Roster> MoveHandler;

    internal AI(Logic _logic, Roster _selfId)
    {
        logic = _logic;
        selfId = _selfId;
        MoveHandler = AIMakeMoveHandler();
    }

    /// <summary>
    /// Choose a board cell
    /// </summary>
    /// <param name="count">The number of remaining UI elements to click</param>
    EventHandler<Roster> AIMakeMoveHandler()
    {
        return logic switch
        {
            Logic.RNG =>
            (object? _, Roster curPlayer) =>
            {
                if (curPlayer != selfId) return;

                Thread thread = new(() =>
                {
                    Thread.Sleep(400);

                    var tile = LogicRNG();

                    Thread.Sleep(1200);

                    EM.InvokeFromMainThread(() => EM.Raise(Evt.AIMoved, new { }, new Point(tile.row, tile.col)));
                });

                thread.Start();
            }
            ,
            Logic.Easy =>
            (object? _, Roster curPlayer) =>
            {
                if (curPlayer != selfId) return;

                Thread thread = new(() =>
                {
                    Thread.Sleep(400);

                    var tile = LogicEasy();

                    Thread.Sleep(1200);

                    EM.InvokeFromMainThread(() => EM.Raise(Evt.AIMoved, new { }, new Point(tile.row, tile.col)));
                });

                thread.Start();
            }
            ,
            _ => throw new NotImplementedException($"AI.AIMakeMoveHandler : logic '{logic}' not supported"),
        };

    }

    static Tile LogicRNG()
    {
        var canTake = new List<int>();

        for (int i = 0; i < Game.board.Length; i++)
            if (Game.CanTakeBoardTile(i, out Roster _)) canTake.Add(i);

        if (canTake.Count == 0)
            throw new Exception("AI.LogicRNG : run on full board");

        var rng = new Random();
        var choice = rng.Next(canTake.Count);

        EM.InvokeFromMainThread(() => EM.Raise(Evt.UpdateLabels, new { }, new Enum[] { LabelManager.AIMsg.Random }));

        var tile = Game.board.GetTile(canTake[choice]);

        return tile;
    }

    /// <summary>
    /// Simple AI logic (easy mode) for playing the game<br/>
    /// Each line is examined to determine the highest presence (cells taken) by a player<br/>
    /// AI participates in the first available (has cells that can be taken) line with the highest presence,<br/>
    /// whether it's its own (to win the game) or a foe's (to stop them from winning)
    /// </summary>
    Tile LogicEasy()
    {
        var linesInfo = Game.ExamineLines();

        var playableLines = linesInfo.Where(rec => rec.canTake.Count > 0).ToArray();

        var playLine = playableLines.MaxBy(rec => rec.dominant.Value) ??
            throw new Exception($"AI.LogicEasy : run on full board");

        var tile = playLine.line.GetTile(playLine.canTake.First());

        var msg = playLine.dominant.Key == selfId ? LabelManager.AIMsg.Attack : LabelManager.AIMsg.Defend;

        EM.InvokeFromMainThread(() => EM.Raise(Evt.UpdateLabels, new { }, new Enum[] { msg }));

        return tile;
    }
}
