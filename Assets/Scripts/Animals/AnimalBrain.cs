using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate bool Action();

public abstract class AnimalBrain {

    public AnimalState state;
    protected Dictionary<string, Action> actions = new Dictionary<string, Action>();
    
    virtual public float slowSpeed { get { return 1f; } }
    virtual public float fastSpeed { get { return 2f; } }

    /// <summary>
    /// Adds a named action to the brain
    /// </summary>
    /// <param name="name">Name of action</param>
    /// <param name="action">Action to do</param>
    public void addAction(string name, Action action) {
        actions.Add(name, action);
    }

    /// <summary>
    /// Spawns the animal at position
    /// </summary>
    /// <param name="pos">Vector3 pos</param>
    virtual public void Spawn(Vector3 pos) {
        state.transform.position = pos;
    }

    virtual public void OnCollisionEnter() {
        // override this in derived classes to handle the message
    }

    abstract public void move();
}
