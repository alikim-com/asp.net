
using System.Diagnostics;

public class Utl
{
   static Stopwatch? stopwatch;

   static Utl()
   {
      stopwatch = new Stopwatch();
      stopwatch.Start();
   }

   static public void Log(string msg)
   {
      double elapsedSec = (stopwatch == null) ? 0 : stopwatch.Elapsed.TotalSeconds;
      Console.WriteLine(elapsedSec.ToString("0.000") + " " + msg);
   }
}

public class Program
{
   public static async Task Main(string[] args)
   {
      await Test();

      Console.WriteLine("Waiting...");
      Console.ReadLine();
   }

   public static async Task Test()
   {
      var test = new EventTest();
      var res = test.Start();

      await Task.Delay(10000);

      Utl.Log($"--------------------End3: {res}");
   }

}

public class EventTest
{
   public async Task<int> Start()
   {
      Utl.Log($"-------------------Start: {Environment.CurrentManagedThreadId}");
      var a = 1;
      await Start2();
      a = 2;
      Utl.Log($"--------------------End: {Environment.CurrentManagedThreadId}");

      return 1;
   }

   public async Task Start2()
   {
      Utl.Log($"-------------------Start2: {Environment.CurrentManagedThreadId}");
      var a = 1;
      await Task.Delay(3000);
      a = 2;
      Utl.Log($"--------------------End2: {Environment.CurrentManagedThreadId}");
   }
}

