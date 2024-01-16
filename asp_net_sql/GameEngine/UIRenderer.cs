using System.Drawing;
using System.Drawing.Drawing2D;

using asp_net_sql.GameEnginePort;

namespace uiRenderer;

public class UIFonts
{
    static public readonly Font Default = new("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point);

    static public readonly Font header = new("Arial", 16F, FontStyle.Regular, GraphicsUnit.Point);
    static public readonly Font regular = new("Arial", 13.5F, FontStyle.Regular, GraphicsUnit.Point);
    static public readonly Font small = new("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point);
    static public readonly Font tiny = new("Arial", 10F, FontStyle.Regular, GraphicsUnit.Point);

    static public readonly Font menu = new("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
    static public readonly Font button = new("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
    static public readonly Font info = new("Segoe UI", 13F, FontStyle.Regular, GraphicsUnit.Point);

}

public class UIColors
{

    static public readonly Color Black = Color.FromArgb(0, 0, 0);
    static public readonly Color Transparent = Color.FromArgb(0, 0, 0, 0);

    static internal readonly Color TintLeft = Color.FromArgb(15 * 4, 200, 104, 34);
    static internal readonly Color TintRight = Color.FromArgb(20 * 4, 185, 36, 199);

    static internal readonly Color ForeLeft = ColorExtensions.BlendOver(TintLeft, Color.FromArgb(200, 200, 200));
    static internal readonly Color ForeRight = ColorExtensions.BlendOver(TintRight, Color.FromArgb(200, 200, 200));

    public struct ColorTheme
    {
        public Color Pitch;
        public Color Dark;
        public Color Prime;
        public Color Dawn;
        public Color Light;
        public Color Noon;
        public Color Accent;
        public Color Text;
    }

    static public readonly ColorTheme Steel = new()
    {
        Pitch = Color.FromArgb(23, 23, 26),
        Dark = Color.FromArgb(34, 34, 38),
        Prime = Color.FromArgb(46, 46, 51),
        Dawn = Color.FromArgb(57, 57, 64),
        Light = Color.FromArgb(69, 69, 77),
        Noon = Color.FromArgb(80, 80, 89),
        Accent = Color.FromArgb(115, 115, 128),
        Text = Color.FromArgb(200, 200, 200),
    };
}

class MenuColorTable : ProfessionalColorTable
{
    UIColors.ColorTheme theme;

    // menu item mouseOver border color
    //public override Color MenuItemBorder => theme.Dark;

    // menu items mouseOver bg color
    //public override Color MenuItemSelected => theme.Light;
   // public override Color MenuItemSelectedGradientBegin => theme.Light;
   // public override Color MenuItemSelectedGradientEnd => theme.Light;

    // menu items pressed (dropdown opened) color
    //public override Color MenuItemPressedGradientBegin => theme.Light;
    //public override Color MenuItemPressedGradientEnd => theme.Light;

    // menu dropdown bg color
  //  public override Color ToolStripDropDownBackground => theme.Dawn;

    // menu items pressed(dropdown opened) border color AND dropdown border color
   // public override Color MenuBorder => theme.Accent;

    public MenuColorTable(UIColors.ColorTheme _theme)
    {
        theme = _theme;
    }
}

public class MenuToolStripRenderer : ToolStripProfessionalRenderer
{
    UIColors.ColorTheme theme;

    //protected override
    void OnRenderArrow(ToolStripArrowRenderEventArgs e)
    {
        e.ArrowColor = theme.Text;
        base.OnRenderArrow(e);
    }

    //protected override 
    void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        ToolStripItem item = e.Item;
        Graphics g = e.Graphics;

        if (!item.IsOnDropDown || !item.Selected)
        {
          //  base.OnRenderMenuItemBackground(e);
            return;

        } else
        {
            using var brush = new SolidBrush(theme.Noon);
            Rectangle rc = new(Point.Empty, item.Size);
            g.FillRectangle(brush, rc);
            // g.DrawRectangle(Pens.Black, 1, 0, rc.Width - 2, rc.Height - 1);
        }
    }

    internal MenuToolStripRenderer(ProfessionalColorTable colorTable, UIColors.ColorTheme _theme) : base(colorTable)
    {
        theme = _theme;
    }
}

public class ButtonColors
{
    public struct ColorTheme
    {
        public Color gradTop;
        public Color gradBot;
        public Color gradBotOver;
        public Color gradTopOver;
        public Color gradTopDisabled;
        public Color gradBotDisabled;
    }

    static public readonly ColorTheme Sunrise = new()
    {
        gradTop = Color.FromArgb(52, 26, 79).ScaleRGB(1.25),
        gradBot = Color.FromArgb(110, 18, 0).ScaleRGB(1.25),
        gradBotOver = Color.FromArgb(52, 26, 79).ScaleRGB(1.4),
        gradTopOver = Color.FromArgb(110, 18, 0).ScaleRGB(1.4),
        gradTopDisabled = Color.FromArgb(200, 104, 34).ScaleRGB(0.10),
        gradBotDisabled = Color.FromArgb(185, 36, 199).ScaleRGB(0.10),
    };
}

public class ButtonToolStripRenderer : ToolStripProfessionalRenderer
{
    Color gTop, gBot;

    ButtonColors.ColorTheme theme;

    bool _disabled = false;
    internal bool Disabled
    {
        get => _disabled;
        set
        {
            if (value != _disabled)
            {
                _disabled = value;

                UpdateColors(false);
                parent.Invalidate();
            }
        }
    }

    readonly Control parent;

    internal ButtonToolStripRenderer(Control _parent, ButtonColors.ColorTheme _theme)
    {
        parent = _parent;
        RoundedEdges = false;
        theme = _theme;
    }

    void UpdateColors(bool state)
    {
        gTop = Disabled ? theme.gradTopDisabled : (state ? theme.gradTopOver : theme.gradTop);
        gBot = Disabled ? theme.gradBotDisabled : (state ? theme.gradBotOver : theme.gradBot);
    }

    internal void SetOverState(object sender, bool state)
    {
        UpdateColors(state);

        if (sender is Control ctrl)
        {
            ctrl.Cursor = (!Disabled && state) ? Cursors.Hand : Cursors.Default;
            ctrl.Invalidate();
        }
    }

    //protected override 
        void OnRenderToolStripBorder(ToolStripRenderEventArgs e) { }

    //protected override 
        void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        //base.OnRenderToolStripBackground(e);

        //using var brush = new LinearGradientBrush(
        //    e.AffectedBounds,
        //    gTop,
        //    gBot,
        //    LinearGradientMode.Vertical
        //);

        //e.Graphics.FillRectangle(brush, e.AffectedBounds);
    }
}

public class UIRenderer
{
    static public MenuToolStripRenderer MenuTSRenderer(UIColors.ColorTheme _theme, string colorTableName)
    {
        ProfessionalColorTable colorTable = colorTableName switch
        {
            "MenuColorTable" => new MenuColorTable(_theme),
            _ => throw new NotImplementedException($"UIRenderer.ToolStripRendererOverride : color table '{colorTableName}' does not exist"),
        };
        return new MenuToolStripRenderer(colorTable, _theme);
    }

    static public MenuToolStripRenderer MenuTSRenderer(UIColors.ColorTheme _theme, ProfessionalColorTable colorTable) => new(colorTable, _theme);

    static public ButtonToolStripRenderer ButtonTSRenderer(Control _parent, ButtonColors.ColorTheme _theme) => new(_parent, _theme);
}
