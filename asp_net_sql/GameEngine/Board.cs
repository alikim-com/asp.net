
using System.Collections;

namespace asp_net_sql.GameEngine;

struct Tile
{
    internal int row;
    internal int col;

    internal Tile(int _row, int _col)
    {
        row = _row;
        col = _col;
    }
}

class Board : IEnumerable<Roster>
{

    readonly Roster[] board;

    internal readonly int width;
    internal readonly int height;
    internal readonly int Length;

    internal Board(int _width, int _height, Roster _def)
    {
        width = _width;
        height = _height;
        Length = _width * _height;
        board = new Roster[Length];

        Array.Fill(board, _def);
    }

    internal Tile GetTile(int index)
    {
        if (index < 0 || index >= Length)
            throw new IndexOutOfRangeException("Board.GetTile : index is out of range");

        var row = index / width;
        var col = index % width;

        return new Tile(row, col);
    }

    internal Roster this[int index]
    {
        get
        {
            if (index < 0 || index >= board.Length)
                throw new IndexOutOfRangeException("Board.get[] : index is out of range");

            return board[index];
        }
        set
        {
            if (index < 0 || index >= board.Length)
                throw new IndexOutOfRangeException("Board.set[] : index is out of range");

            board[index] = value;
        }
    }

    internal Roster this[int row, int col]
    {
        get
        {
            if (row < 0 || row >= height)
                throw new IndexOutOfRangeException("Board.get[,] : row index is out of range");
            if (col < 0 || col >= width)
                throw new IndexOutOfRangeException("Board.get[,] : column index is out of range");
            var index = row * width + col;
            return board[index];
        }
        set
        {
            if (row < 0 || row >= height)
                throw new IndexOutOfRangeException("Board.get[,] : row index is out of range");
            if (col < 0 || col >= width)
                throw new IndexOutOfRangeException("Board.get[,] : column index is out of range");
            var index = row * width + col;

            board[index] = value;
        }
    }

    public IEnumerator<Roster> GetEnumerator() => new BoardEtor(board);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

class BoardEtor : IEnumerator<Roster>
{
    readonly Roster[] list;
    int head;

    internal BoardEtor(Roster[] _list)
    {
        list = _list;
        head = -1;
    }

    public Roster Current => list[head];

    object IEnumerator.Current => Current;

    public void Dispose() { }

    public bool MoveNext()
    {
        head++;
        return head != list.Length;
    }

    public void Reset() { head = -1; }
}

class Line : IEnumerable<Roster>
{
    internal readonly int Length;
    readonly Tile[] rc;
    readonly Board board;

    internal Line(Board _board, Tile[] _rc)
    {
        rc = _rc;
        Length = _rc.Length;
        board = _board;
    }

    internal Tile GetTile(int index)
    {
        if (index < 0 || index >= Length)
            throw new IndexOutOfRangeException("Line.GetTile : index is out of range");

        return rc[index];
    }

    internal Roster this[int index]
    {
        get
        {
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException("Line.get[] : index is out of range");

            return board[rc[index].row, rc[index].col];
        }
        set
        {
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException("Line.set[] : index is out of range");

            board[rc[index].row, rc[index].col] = value;
        }
    }

    public IEnumerator<Roster> GetEnumerator() => new LineEtor(board, rc);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

class LineEtor : IEnumerator<Roster>
{
    readonly Board list;
    readonly Tile[] rc;
    int head;

    internal LineEtor(Board _list, Tile[] _rc)
    {
        list = _list;
        rc = _rc;
        head = -1;
    }

    public Roster Current => list[rc[head].row, rc[head].col];

    object IEnumerator.Current => Current;

    public void Dispose() { }

    public bool MoveNext()
    {
        head++;
        return head != rc.Length;
    }

    public void Reset() { head = -1; }
}

/// <summary>
/// For gathering statistics about each line
/// </summary>
class LineInfo
{
    internal readonly Line line;
    internal readonly List<int> canTake = new();
    internal readonly Dictionary<Roster, int> takenStats = new();
    internal KeyValuePair<Roster, int> dominant = new(Roster.None, 0);

    internal LineInfo(Line _line)
    {
        line = _line;
    }
}