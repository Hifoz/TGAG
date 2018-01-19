using System;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// A blocking queue made by Stephen Toub (Microsoft).
/// https://blogs.msdn.microsoft.com/toub/2006/04/12/blocking-queues/
/// </summary>
/// <typeparam name="T"></typeparam>
class BlockingQueue<T> {
    private int count = 0;
    private Queue<T> queue = new Queue<T>();

    /// <summary>
    /// Tries to dequeue an item from the queue, blocks if it's empty.
    /// </summary>
    /// <returns>An item T from the queue</returns>
    public T Dequeue() {
        lock (queue) {
            while (count <= 0) {
                Monitor.Wait(queue);
            }

            count--;
            return queue.Dequeue();
        }
    }

    /// <summary>
    /// Enqueues an item T to the queue, and wakes up threads blocking on this queue.
    /// </summary>
    /// <param name="data">The item T to enqueue</param>
    public void Enqueue(T data) {
        if (data == null) throw new ArgumentNullException("data");

        lock (queue) {
            queue.Enqueue(data);
            count++;
            Monitor.Pulse(queue);
        }
    }
}