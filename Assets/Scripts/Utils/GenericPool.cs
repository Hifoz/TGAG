using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// A class for pooling generic objects
/// ! the objects need to have a parameter less constructor
/// </summary>
/// <typeparam name="T">Type fo object to pool</typeparam>
public class GenericPool<T> {
    private List<T> active = new List<T>();
    private Stack<T> inactive = new Stack<T>();

    public List<T> activeList { get { return active; } }
    public Stack<T> inactiveStack { get { return inactive; } }

    /// <summary>
    /// Constructor that throws exception if type T is not a class
    /// </summary>
    public GenericPool(){
        if (!typeof(T).IsClass && typeof(T).GetConstructor(Type.EmptyTypes) != null) {
            throw new Exception("GenericPool: Type does not derive from object!");
        }    
    }

    /// <summary>
    /// Gets an object from the pool
    /// </summary>
    /// <returns>T instance</returns>
    public T getObject() {
        T obj;
        if (inactive.Count == 0) {
            obj = Activator.CreateInstance<T>();
            active.Add(obj);
        } else {
            obj = inactive.Pop();
            active.Add(obj);
        }
        return obj;
    }

    /// <summary>
    /// Returns a T to the pool
    /// </summary>
    /// <param name="obj">T to return</param>
    public void returnObject(T obj) {
        if (!active.Remove(obj)) {
            Debug.LogWarning("The object returned to the pool was not part of the pools active list!");
        }
        inactive.Push(obj);
    }
}
