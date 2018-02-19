using System;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// A blocking List
/// </summary>
/// <typeparam name="T">Type of the list content</typeparam>
public class BlockingList<T> {
    private int count = 0;
    private List<T> list = new List<T>();
    
    /// <summary>
    /// Adds an item T to the list, and wakes up threads blocking on this list
    /// </summary>
    /// <param name="item">The item to add</param>
    public void Add(T item) {
        if(item == null) {
            throw new ArgumentNullException("item");
        }

        lock (list) {
            list.Add(item);
            count++;
            Monitor.Pulse(list);
        }
    }

    /// <summary>
    /// Adds a collection of T to the list, and wakes up threads blocking on this list
    /// </summary>
    /// <param name="item">The item to add</param>
    public void AddRange(IEnumerable<T> collection) {
        if(collection == null) {
            throw new ArgumentNullException("collection");

        }
        lock (list) {
            list.AddRange(collection);
            count = list.Count;
            Monitor.Pulse(list);
        }
    }

    /// <summary>
    /// Finds an element using a function and takes the item out of the list.
    /// </summary>
    /// <param name="func">Function to decide what element to take. Should return index item to take</param>
    /// <returns>An item from the list, based on result from func</returns>
    public T Take(Func<List<T>, int> func) {
        lock (list) {
            while(count <= 0) {
                Monitor.Wait(list);
            }

            int index = list.Count == 1 ? 0 : func(list);
            if (index < 0 || index > list.Count - 1)
                return default(T);

            T res = list[index];
            list.RemoveAt(index);
            count--;
            return res;
        }
    }

    /// <summary>
    /// Takes an item out of the list.
    /// </summary>
    /// <param name="func">Index of item to take</param>
    /// <returns>The item from the list</returns>
    public T Take(int index) {
        lock (list) {
            while (count <= 0) {
                Monitor.Wait(list);
            }

            T res = list[0];
            list.RemoveAt(0);
            count--;
            return res;
        }
    }


    public void RemoveAll(Predicate<T> match) {
        lock (list) {
            int invalidCount = list.RemoveAll(match);
            count -= invalidCount;
        }
    }


}
