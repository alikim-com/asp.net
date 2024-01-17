
using System.Diagnostics;

namespace test_async;

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

/*
var res = test.Start();

0.000 -------------------Start: 1
0.023 -------------------Start2: 1
0.028 --------------------End3: 1
3.044 --------------------End2: 5
3.044 --------------------End: 5
10.041 --------------------End4: 5 res: Start() returned
10.041 --------------------Waiting: 5

var res = await test.Start();

0.000 -------------------Start: 1
0.025 -------------------Start2: 1
3.046 --------------------End2: 5
3.046 --------------------End: 5
3.046 --------------------End3: 5
13.056 --------------------End4: 5 res: Start() returned
13.057 --------------------Waiting: 5
*/

public class Program
{
   public static async Task Main(string[] args)
   {
      await Test();

      Utl.Log($"--------------------Waiting: {Environment.CurrentManagedThreadId}");
      Console.ReadLine();
   }

   public static async Task Test()
   {
      var test = new EventTest();

      var res = await test.Start();

      Utl.Log($"--------------------End3: {Environment.CurrentManagedThreadId}");

      await Task.Delay(10000);

      Utl.Log($"--------------------End4: {Environment.CurrentManagedThreadId} res: {res}");
   }

}

public class EventTest
{
   public async Task<string> Start()
   {
      Utl.Log($"-------------------Start: {Environment.CurrentManagedThreadId}");
      var a = 1;
      await Start2();
      a = 2;
      Utl.Log($"--------------------End: {Environment.CurrentManagedThreadId}");

      return "Start() returned";
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

