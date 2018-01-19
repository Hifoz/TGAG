using System;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// A locking queue based on a blocking queue made by Stephen Toub (Microsoft).
/// https://blogs.msdn.microsoft.com/toub/2006/04/12/blocking-queues/
/// </summary>
/// <typeparam name="T"></typeparam>
class LockingQueue<T> {
    private int count = 0;
    private Queue<T> queue = new Queue<T>();

    /// <summary>
    /// Dequeues an item from the queue, will not fail if queue is empty.
    /// </summary>
    /// <returns>An item T from the queue</returns>
    public T Dequeue() {
        lock (queue) {
            try {
                T item = queue.Dequeue();
                count--;
                return queue.Dequeue();
            } catch (InvalidOperationException e) {
                throw e;
            }
        }
    }

    /// <summary>
    /// Enqueues the item T to the queue.
    /// </summary>
    /// <param name="data">The item to enqueue</param>
    public void Enqueue(T data) {
        if (data == null) throw new ArgumentNullException("data");

        lock (queue) {
            queue.Enqueue(data);
            count++;
        }
    }

    /// <summary>
    /// Returns the count of the queue.
    /// </summary>
    /// <returns>int count</returns>
    public int getCount() {
        lock (queue) {
            lock (count) {
                return count;
            }
        }
    }
}