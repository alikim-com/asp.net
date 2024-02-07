
namespace imagex;

public enum Format
{
    JPEG,
    PNG,
}

public class Image(Format _format, int _width, int _height)
{
    public readonly Format format = _format;
    public readonly int width = _width;
    public readonly int height = _height;
}

public class PNG(Format _format, int _width, int _height) : Image(_format, _width, _height)
{
    
}