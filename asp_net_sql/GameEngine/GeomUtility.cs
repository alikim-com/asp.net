using System.Drawing;
using asp_net_sql.GameEnginePort;

namespace geomUtility;

public class GeomUtility
{
    /// <summary>
    /// Fit small rectangle into a large one,<br/>
    /// while preserving small box's aspect ratio and maximising its area
    /// </summary>
    /// <param name="large">Size of a larger rectangle</param>
    /// <param name="small">Size of a smaller rectangle</param>
    /// <returns>The new size of a scaled small box</returns>
    static public Size FitRect(Size large, Size small)
    {
        double widthRatio = (double)large.Width / small.Width;
        double heightRatio = (double)large.Height / small.Height;

        double scale = Math.Min(widthRatio, heightRatio);

        return new Size(
            (int)(small.Width * scale),
            (int)(small.Height * scale)
        );
    }

}

public class RatioPosControl
{
    public enum Anchor
    {
        Left, Top, Right, Bottom, Middle
    }

    internal readonly Anchor hor, ver;

    internal readonly Control control;
    internal readonly Control parent;

    internal readonly RectangleF ratioAnchors;

    internal RatioPosControl(Control _control, Control _parent, Anchor _hor, Anchor _ver)
    {
        control = _control;
        parent = _parent;
        hor = _hor;
        ver = _ver;

        var clSize = parent.ClientSize;
        ratioAnchors = new RectangleF(
            (float)control.Location.X / clSize.Width,
            (float)control.Location.Y / clSize.Height,
            (float)control.ClientSize.Width / clSize.Width,
            (float)control.ClientSize.Height / clSize.Height
        );
    }
}

/// <summary>
/// For keeping fractional position of a control inside its parent
/// </summary>
public class RatioPosition
{
    static readonly List<RatioPosControl> rpControls = new();

    static public void Add(Control ctrl, Control parent, RatioPosControl.Anchor hor, RatioPosControl.Anchor ver) =>
        rpControls.Add(new RatioPosControl(ctrl, parent, hor, ver));

    static public void Remove(Control control) => rpControls.RemoveAll(rpCtrl => rpCtrl.control == control);

    static public void Update(Control control, bool scale = false)
    {
        var rpCtrl = rpControls.Find(rpCtrl => rpCtrl.control == control);
        if (rpCtrl == null) return;

        var ctrl = rpCtrl.control;
        var rect = rpCtrl.ratioAnchors;
        var clSize = rpCtrl.parent.ClientSize;
        var hor = rpCtrl.hor;
        var ver = rpCtrl.ver;
        var x = hor switch
        {
            RatioPosControl.Anchor.Left => (int)(rect.Left * clSize.Width),
            RatioPosControl.Anchor.Right => (int)(rect.Right * clSize.Width) - ctrl.Width,
            RatioPosControl.Anchor.Middle => (int)((rect.Left + 0.5 * rect.Width) * clSize.Width) - ctrl.Width / 2,
            _ => throw new Exception($"RatioPosition.Update : wrong hor Anchor type '{hor}'"),
        };
        var y = ver switch
        {
            RatioPosControl.Anchor.Top => (int)(rect.Top * clSize.Height),
            RatioPosControl.Anchor.Bottom => (int)(rect.Bottom * clSize.Height) - ctrl.Height,
            RatioPosControl.Anchor.Middle => (int)((rect.Top + 0.5 * rect.Height) * clSize.Height) - ctrl.Height / 2,
            _ => throw new Exception($"RatioPosition.Update : wrong ver Anchor type '{ver}'"),
        };
        rpCtrl.control.Location = new Point(x, y);
    }
}
