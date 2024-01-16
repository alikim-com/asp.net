using System.Diagnostics;
using System.Drawing;

namespace asp_net_sql.GameEnginePort;

// ENGINE PORT

public class _RectangleF(float _x, float _y, float _width, float _height)
{
    public float X = _x;
    public float Y = _y;
    public float Width = _width;
    public float Height = _height;

    public float Left => X;
    public float Right => X + Width;
    public float Top => Y;
    public float Bottom => Y + Height;
}

public class _Point(int _x, int _y)
{
    public int X = _x;
    public int Y = _y;
}
public class _Size(int _width, int _height)
{
    public int Width = _width;
    public int Height = _height;
}

public class Padding
{

}

public class ControlEventArgs : EventArgs
{
    public TableLayoutPanel Control;

    public ControlEventArgs()
    {
    }
}

public class ToolStripRenderEventArgs : EventArgs
{

}

public class ToolStripArrowRenderEventArgs : EventArgs
{
    public Color ArrowColor;
}
public class ToolStripItemRenderEventArgs : EventArgs
{
    public ToolStripItem Item {  get; set; }
    public Graphics Graphics { get; set; }
}

public enum Cursors
{
    Default,
    Hand,
}
public enum BorderStyle
{
    None
}

public enum AnchorStyles
{
    Bottom = 2,
    Left = 4,
    None = 0,
    Right = 8,
    Top = 1,
}

public class Control
{
    public string Name = "";
    public Size ClientSize = new(0, 0);
    public Size Size = new(0, 0);
    public Point Location = new(0, 0);
    public int Width;
    public int Height;
    public List<Label> Controls = [];
    public Image? BackgroundImage;
    public Color BackColor;
    public Color ForeColor;
    public Font? Font;
    public AnchorStyles Anchor { get; set; }
    public int TabIndex;
    public Cursors Cursor;

    public void Invalidate() { }
}

public class Label : Control
{
    public bool AutoSize;
    public string Text = "";
    public event EventHandler Click = delegate { };
}

public class ToolStripRenderer
{
    public bool RoundedEdges;
}

public class ToolStripProfessionalRenderer : ToolStripRenderer
{
    public ProfessionalColorTable? colorTable;

    public ToolStripProfessionalRenderer(ProfessionalColorTable _colorTable)
    {
        colorTable = _colorTable;
    }

    public ToolStripProfessionalRenderer() { }

    public void OnRenderArrow(ToolStripArrowRenderEventArgs e) { }
}

public class ToolStrip : Control
{
    public BorderStyle BorderStyle;
    public ToolStripRenderer? Renderer;
    public event EventHandler MouseEnter = delegate { };
    public event EventHandler MouseLeave = delegate { };
    public bool Enabled;
    public bool Visible;
    public event EventHandler Click = delegate { };
}
public class MenuStrip : ToolStrip
{
}
public class ToolStripItem : ToolStrip
{
    public bool IsOnDropDown;
    public bool Selected;
}
public class ToolStripTextBox : ToolStrip
{
}
public class ToolStripMenuItem : ToolStrip
{
}

public class ToolStripLabel : ToolStripItem
{
}

public class Panel : Control
{
    public event EventHandler MouseEnter = delegate { };
    public event EventHandler MouseLeave = delegate { };
    public event EventHandler Click = delegate { };
}

public class TableLayoutPanel : Panel
{
}

public class ProfessionalColorTable
{
}

public class MessageBox
{
    public static void Show(string message)
    {
        Debug.WriteLine(message);
    }
}

public struct Message
{
    public int Msg;
    public IntPtr LParam;
    public IntPtr WParam;
}

// --------------------

