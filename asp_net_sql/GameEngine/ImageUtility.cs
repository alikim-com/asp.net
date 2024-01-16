using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace imageUtility;

static public class ColorExtensions
{
    public struct Argb
    {
        public double a;
        public double r;
        public double g;
        public double b;

        public override readonly string ToString() => $"a: {a}, r: {r}, g: {g}, b: {b}";
    }

    static public Color ScaleRGB(this Color c, double scale) =>
        Color.FromArgb((int)(c.R * scale), (int)(c.G * scale), (int)(c.B * scale));

    static public Color Scale(this Color c, double scale) =>
        Color.FromArgb((int)(c.A * scale), (int)(c.R * scale), (int)(c.G * scale), (int)(c.B * scale));

    static public Argb Normalise(this Color c)
    {
        double k = 1.0 / 255;
        return new Argb { a = c.A * k, r = c.R * k, g = c.G * k, b = c.B * k };
    }

    static public Color FromNormalised(Argb nrm) => Color.FromArgb(
        (byte)(nrm.a * 255),
        (byte)(nrm.r * 255),
        (byte)(nrm.g * 255),
        (byte)(nrm.b * 255)
    );

    /// <summary>
    /// Blends top ARGB pixel over bottom ARGB pixel
    /// --- no pre ---
    /// a = a1 + a2 - a1 * a2 = a2 + a1 * (1 - a2)
    /// aR = R2 * a2 + R1 * a1 * (1 - a2)
    /// R = aR / a
    /// ---  pre  ---
    /// a = a2 + a1 * (1 - a2)
    /// r = r2 + r1 * (1 - a2)
    /// </summary>
    /// <param name="top">Transparent pixel on top</param>
    /// <param name="bot">Transparent pixel below</param>
    /// <param name="pre">Whether RGBs are premultiplied by alpha</param>
    /// <returns></returns>
    static public Color BlendOver(Color top, Color bot, bool pre = false)
    {
        var t = top.Normalise();
        var b = bot.Normalise();
        double f = 1 - t.a;

        double a = t.a + b.a * f;
        if (a == 0) return Color.FromArgb(0, 0, 0, 0);

        double ar = 1 / a;
        double _r = pre ? t.r + b.r * f : (t.r * t.a + b.r * b.a * f) * ar;
        double _g = pre ? t.g + b.g * f : (t.g * t.a + b.g * b.a * f) * ar;
        double _b = pre ? t.b + b.b * f : (t.b * t.a + b.b * b.a * f) * ar;

        return FromNormalised(new Argb() { a = a, r = _r, g = _g, b = _b });
    }
}

static public class ImageExtensions
{

    /// <summary>
    /// Best fit the current image into dstSize box while preserving its aspect ratio.<br/>
    /// Paint the image over dst background at the specified offDst.
    /// </summary>
    /// <param name="src">top image</param>
    /// <param name="dst">bottom image</param>
    /// <param name="dstOff">left-top offset on dst where to start painting fitted src</param>
    /// <param name="dstSize">a box into which to fit src while preserving its aspect ratio</param>
    /// <param name="hAlign">horizontal alignment after fitting scr into dstSize:</br/>
    /// "left" - left margin is zero, "right" - right margin is zero, "center" - left and right margins are equal</param>
    /// <param name="vAlign">vertical alignment after fitting scr into dstSize:</br/>
    /// "top" - top margin is zero, "bottom" - bottom margin is zero, "center" - top and bottom margins are equal</param>
    /// <param name="extraAlpha">extra opacity [0,1] applied to fitted src while blending</param>
    /// <param name="blendMode">"add" - simple RGBA color addition, "over" - A over B pixel blending</param>
    static public Image GetOverlayOnBackground(this Image src,
        Image dst,
        Point dstOff,
        Size dstSize,
        string hAlign,
        string vAlign,
        double extraAlpha = 1,
        string blendMode = "add")
    {
        Size scaledSrc = GeomUtility.FitRect(dstSize, src.Size);
        int offsetLeft = hAlign switch
        {
            "left" => 0,
            "right" => dstSize.Width - scaledSrc.Width,
            "center" => (dstSize.Width - scaledSrc.Width) / 2,
            _ => throw new NotImplementedException($"GetOverlayOnBackground : hAlign '{hAlign}'"),
        };
        int offsetTop = vAlign switch
        {
            "top" => 0,
            "bottom" => dstSize.Height - scaledSrc.Height,
            "center" => (dstSize.Height - scaledSrc.Height) / 2,
            _ => throw new NotImplementedException($"GetOverlayOnBackground : vAlign '{vAlign}'"),
        };

        using Graphics g = Graphics.FromImage(dst);

        // --- no pre ---
        // a = a2 + a1 * (1 - a2)
        // aR = R2 * a2 + R1 * a1 * (1 - a2)
        // R = aR / a
        if (blendMode == "over")
        {
            var dstBitmap = (Bitmap)dst;
            var scaledSrcImg = new Bitmap(scaledSrc.Width, scaledSrc.Height);
            using Graphics sg = Graphics.FromImage(scaledSrcImg);
            sg.InterpolationMode = InterpolationMode.HighQualityBicubic;
            sg.DrawImage(src, 0, 0, scaledSrc.Width, scaledSrc.Height);

            // blend
            double norm = 1.0 / 255;
            int dstLeft = offsetLeft + dstOff.X;
            int dstTop = offsetTop + dstOff.Y;
            for (int y = 0; y < scaledSrc.Height; y++)
            {
                int dstY = dstTop + y;
                for (int x = 0; x < scaledSrc.Width; x++)
                {
                    int dstX = dstLeft + x;
                    Color srcARGB = scaledSrcImg.GetPixel(x, y);
                    Color dstARGB = dstBitmap.GetPixel(dstX, dstY);
                    double a1 = dstARGB.A * norm;
                    double a2 = srcARGB.A * norm * extraAlpha;
                    double aFact = a1 * (1 - a2);
                    double a = a2 + aFact;
                    double ra = 1 / a;
                    Color blendOver = a == 0 ? Color.FromArgb(0, 0, 0, 0) :
                    Color.FromArgb(
                        (int)(255 * a),
                        (int)((srcARGB.R * a2 + dstARGB.R * aFact) * ra),
                        (int)((srcARGB.G * a2 + dstARGB.G * aFact) * ra),
                        (int)((srcARGB.B * a2 + dstARGB.B * aFact) * ra)
                    );
                    dstBitmap.SetPixel(dstX, dstY, blendOver);
                }
            }

        } else
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(src, offsetLeft + dstOff.X, offsetTop + dstOff.Y, scaledSrc.Width, scaledSrc.Height);
        }

        return dst;
    }

    /// <summary>
    /// Create new image of dstSize and best fit the current image into it while preserving its aspect ratio
    /// </summary>
    static public Image GetOverlayOnBackground(this Image src,
        Size dstSize,
        Color? bg,
        string hAlign,
        string vAlign)
    {
        Size scaledSrc = GeomUtility.FitRect(dstSize, src.Size);
        int offsetLeft = hAlign switch
        {
            "left" => 0,
            "right" => dstSize.Width - scaledSrc.Width,
            "center" => (dstSize.Width - scaledSrc.Width) / 2,
            _ => throw new NotImplementedException($"GetOverlayOnBackground : hAlign '{hAlign}'"),
        };
        int offsetTop = vAlign switch
        {
            "top" => 0,
            "bottom" => dstSize.Height - scaledSrc.Height,
            "center" => (dstSize.Height - scaledSrc.Height) / 2,
            _ => throw new NotImplementedException($"GetOverlayOnBackground : vAlign '{vAlign}'"),
        };

        var dst = new Bitmap(dstSize.Width, dstSize.Height);
        using Graphics g = Graphics.FromImage(dst);

        g.Clear(bg ?? Color.Transparent);
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.DrawImage(src, offsetLeft, offsetTop, scaledSrc.Width, scaledSrc.Height);

        return dst;
    }

    static public Image GetImageCopyWithAlpha(this Image src, float opacity)
    {
        Bitmap dst = new(src.Width, src.Height);
        using (Graphics g = Graphics.FromImage(dst))
        {
            ColorMatrix matrix = new()
            {
                Matrix33 = opacity
            };
            ImageAttributes attributes = new();
            attributes.SetColorMatrix(
                matrix,
                ColorMatrixFlag.Default,
                ColorAdjustType.Bitmap
            );
            g.DrawImage(
                src,
                new Rectangle(0, 0, dst.Width, dst.Height),
                0, 0, src.Width, src.Height,
                GraphicsUnit.Pixel,
                attributes
            );
        }
        return dst;
    }

    /// <summary>
    /// Makes a black & white version of an image
    /// </summary>
    /// <param name="mode">"desaturate" - formula ised un Photoshop</param>
    /// <param name="factor">in "desaturate" mode - the desired progression towards B&W image E[0,1]</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    static public Image Desaturate(this Image src, string mode, double factor = 1)
    {
        if (mode != "PS")
            throw new NotImplementedException($"Image.Desaturate : mode '{mode}'");

        int w = src.Width, h = src.Height;
        var srcBmp = new Bitmap(src);
        var dst = new Bitmap(w, h);

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                Color sRGB = srcBmp.GetPixel(x, y);
                // photoshop desaturate
                var min = Math.Min(sRGB.R, Math.Min(sRGB.G, sRGB.B));
                var max = Math.Max(sRGB.R, Math.Max(sRGB.G, sRGB.B));
                int avr = (min + max) / 2;
                dst.SetPixel(x, y, Color.FromArgb(
                    sRGB.A,
                    Lerp(sRGB.R, avr, factor),
                    Lerp(sRGB.G, avr, factor),
                    Lerp(sRGB.B, avr, factor)
                ));
            }

        return dst;
    }

    static public int Lerp(int beg, int end, double f) => beg + (int)((end - beg) * f);
    static public double Lerp(double beg, double end, double f) => beg + (end - beg) * f;
}
