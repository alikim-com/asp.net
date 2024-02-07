
using System.Buffers.Binary;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace imagex;

internal class Program
{
    static void Main(string[] args)
    {
        var beData = Utils.ReadFileBytes("../../../testImages", "redDot_1x1.png");

        int offset = 8;

        Stopwatch watch = new ();
        watch.Start();

        var sp = new ReadOnlySpan<byte>(beData, offset, 4);
        int value = BinaryPrimitives.ReadInt32BigEndian(sp);
        
        watch.Stop();
        Console.WriteLine(watch.Elapsed);
        
        Console.WriteLine($"Value: {value:X}");
    }

}