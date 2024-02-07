
using System.Diagnostics;

namespace utils;

public class Utils
{
    static public byte[] ReadFileBytes(string path, string name)
    {
        try
        {
            return File.ReadAllBytes(Path.Combine(path, name));
        }
        catch (Exception ex)
        {
            Log($"Utils.ReadFile : exception '{ex.Message}'");
            return [];
        }
    }

    static public void WriteFileBytes(string path, string name, byte[] outp)
    {
        try
        {
            File.WriteAllBytes(Path.Combine(path, name), outp);
        }
        catch (Exception ex)
        {
            Log($"Utils.WriteFile : exception '{ex.Message}'");
        }
    }

    public enum LogMode
    {
        Console,
        Debug,
    }

    static readonly LogMode logMode = LogMode.Console;
    static Stopwatch? stopwatch;
    static public void Log(string Log)
    {
        stopwatch ??= Stopwatch.StartNew();
        double elapsedSec = stopwatch.Elapsed.TotalSeconds;
        switch (logMode)
        {
            case LogMode.Console:
                Console.WriteLine(elapsedSec.ToString("0.00000") + " " + Log);
                break;
            case LogMode.Debug:
                Debug.WriteLine(elapsedSec.ToString("0.00000") + " " + Log);
                break;
            default:
                throw new NotImplementedException($"Utils.Log : logMode '{logMode}' is not supported");
        }

    }
}


