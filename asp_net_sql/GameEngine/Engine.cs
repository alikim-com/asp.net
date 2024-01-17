using System.Collections.Concurrent;
using System.Diagnostics;

namespace asp_net_sql.GameEngine;

internal class Engine
{
    static Stopwatch? stopwatch;
    static internal void Log(string msg)
    {
        double elapsedSec = (stopwatch == null) ? 0 : stopwatch.Elapsed.TotalSeconds;
        Debug.WriteLine(elapsedSec.ToString("0.000") + " " + msg);
    }

    internal BlockingCollection<int> dataQueue;

    internal Engine(int capacity = 10)
    {
        stopwatch = Stopwatch.StartNew();

        dataQueue = new(boundedCapacity: capacity);
    }

    internal void Start()
    {
        var messageLoop = new MessageLoop(dataQueue);
        Task.Run(messageLoop.Run);
        var producer = new Producer(dataQueue);
        Task.Run(producer.Loop);
        Task.WaitAll();
    }
}

internal class Producer
{
    internal BlockingCollection<int> dataQueue;

    internal Producer(BlockingCollection<int> _dataQueue)
    {
        dataQueue = _dataQueue;
    }

    internal void Loop()
    {
        Engine.Log("Producer start");

        for (int i = 0; i < 3; i++)
        {
            Engine.Log($"Producer iteration: {i}");
            dataQueue.Add(i);
            Engine.Log($"Produced: {i}");
            Engine.Log($"Producer asleep");
            Thread.Sleep(2000);
            Engine.Log($"Producer awake");
        }

        Engine.Log("Producer closed");
        dataQueue.CompleteAdding();
    }
}

internal class MessageLoop
{
    internal BlockingCollection<int> dataQueue;

    internal MessageLoop(BlockingCollection<int> _dataQueue)
    {
        dataQueue = _dataQueue;
    }

    internal void Run()
    {
        Engine.Log("Consumer start");

        foreach (int item in dataQueue.GetConsumingEnumerable())
        {
            Engine.Log($"Consumed: {item}");
            Engine.Log($"Consumer asleep");
            Thread.Sleep(4000); Task.Delay(4000);
            Engine.Log($"Consumer awake");
        }

        Engine.Log("Consumer closed");
    }
}
