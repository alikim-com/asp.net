// https://www.w3.org/TR/png/

namespace imagex;

public class Image(Image.Format _format, int _width, int _height)
{
    public enum Format
    {
        JPEG,
        PNG,
    }

    public readonly Format format = _format;
    public readonly int width = _width;
    public readonly int height = _height;
}

public class PNG(int _width, int _height) : Image(Format.PNG, _width, _height)
{
    public enum ColorType {
        Greyscale = 0,
        Truecolour = 2,
        IndexedColour = 3,
        GreyscaleWithAlpha = 4,
        TruecolourWithAlpha = 6,
    }

    readonly Dictionary<ColorType, int[]> allowedBitDepths = new()
    {
        // each pixel is a greyscale sample
        { ColorType.Greyscale, [1, 2, 4, 8, 16] },
        // each pixel is an R,G,B triple
        { ColorType.Truecolour, [8, 16] },
        // each pixel is a palette index; a PLTE chunk shall appear
        { ColorType.IndexedColour, [1, 2, 4, 8] },  
        // each pixel is a greyscale sample followed by an alpha sample
        { ColorType.GreyscaleWithAlpha, [8, 16]},
        // each pixel is an R,G,B triple followed by an alpha sample
        { ColorType.TruecolourWithAlpha, [8, 16]},
    };

}

public class ByteFilter
{
    public enum Type
    {
        None = 0,
        Sub = 1,
        Up = 2,
        Average = 3,
        Paeth = 4,
    }


}

//0	None Filt(x) = Orig(x)   Recon(x) = Filt(x)
//1	Sub Filt(x) = Orig(x) - Orig(a) Recon(x) = Filt(x) + Recon(a)
//2	Up Filt(x) = Orig(x) - Orig(b) Recon(x) = Filt(x) + Recon(b)
//3	Average Filt(x) = Orig(x) - floor((Orig(a) + Orig(b)) / 2)	Recon(x) = Filt(x) + floor((Recon(a) + Recon(b)) / 2)
//4	Paeth Filt(x) = Orig(x) - PaethPredictor(Orig(a), Orig(b), Orig(c))   Recon(x) = Filt(x) + PaethPredictor(Recon(a), Recon(b), Recon(c))

