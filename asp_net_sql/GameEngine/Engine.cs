using System.Collections.Concurrent;
using System.Diagnostics;

namespace asp_net_sql.GameEngine;

internal class Engine
{
    internal BlockingCollection<int> dataQueue;

    internal Engine(int capacity = 10)
    {
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
        Utils.Log("Producer start");

        for (int i = 0; i < 3; i++)
        {
            Utils.Log($"Producer iteration: {i}");
            dataQueue.Add(i);
            Utils.Log($"Produced: {i}");
            Utils.Log($"Producer asleep");
            Thread.Sleep(2000);
            Utils.Log($"Producer awake");
        }

        Utils.Log("Producer closed");
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
        Utils.Log("Consumer start");

        foreach (int item in dataQueue.GetConsumingEnumerable())
        {
            Utils.Log($"Consumed: {item}");
            Utils.Log($"Consumer asleep");
            Thread.Sleep(4000); Task.Delay(4000);
            Utils.Log($"Consumer awake");
        }

        Utils.Log("Consumer closed");
    }
}
