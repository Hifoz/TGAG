using System;

/// <summary>
/// High memory performance implementation of a queue, using an array
/// </summary>
/// <typeparam name="T">Type to store in queue</typeparam>
public class ArrayQueue<T> {
    T[] buffer;
    int dequeue;
    int enqueue;
    int size;

    public ArrayQueue(int size){
        this.buffer = new T[size];
        dequeue = 0;
        enqueue = 0;
        size = 0;
    }

    /// <summary>
    /// Enqueues an item
    /// </summary>
    /// <param name="item"></param>
    public void Enqueue(T item) {
        if (size == buffer.Length) {
            throw new ArrayQueueException("Tried to enqueue with a full buffer!");
        }
        size++;
        enqueue = (enqueue + 1) % buffer.Length;
        buffer[enqueue] = item;
    }

    /// <summary>
    /// Dequeues an item
    /// </summary>
    /// <returns>item T</returns>
    public T Dequeue() {
        if (size == 0) {
            throw new ArrayQueueException("Tried to dequeue with a size of 0!");
        }
        size--;
        dequeue = (dequeue + 1) % buffer.Length;
        return buffer[dequeue];     
    }

    /// <summary>
    /// The currently used size of the buffer (as a count)
    /// </summary>
    /// <returns>int size</returns>
    public int getSize() {
        return size;
    }

    public bool Any() {
        return size > 0;
    }

    /// <summary>
    /// Exception class for this ArrayQueues
    /// </summary>
    class ArrayQueueException : Exception {
        public ArrayQueueException(string message) : base(message) { }
    }
}
