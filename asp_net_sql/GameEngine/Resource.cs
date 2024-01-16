using System.Drawing;

namespace asp_net_sql.GameEngine;

// ENGINE PORT

class Resource
{
    static public Image Star;
    static public Image TokenLeft;
    static public Image TokenRight;
    static public Image GameBackImg;

    static Resource()
    {
        Star = new Bitmap(1,1);
        TokenLeft = new Bitmap(1,1);
        TokenRight = new Bitmap(1,1);
        GameBackImg = new Bitmap(1, 1);
    }

    public Resource()
    {

    }

    public struct ResourceManager
    {
        public static Image GetObject(string str) { return Star; }
    }
}
