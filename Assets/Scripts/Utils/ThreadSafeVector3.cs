using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

// A minimal threadsafe wrapper for a vector3
public class ThreadSafeVector3 {
    private Vector3 vec;
    private Mutex mutex = new Mutex(true);

    public ThreadSafeVector3() {
        this.vec = new Vector3();
        mutex.ReleaseMutex();
    }


    public ThreadSafeVector3(Vector3 vec) {
        this.vec = vec;
        mutex.ReleaseMutex();
    }

    public Vector3 get() {
        mutex.WaitOne();
        Vector3 res = vec;
        mutex.ReleaseMutex();
        return res;
    }

    public void set(Vector3 v) {
        mutex.WaitOne();
        vec = v;
        mutex.ReleaseMutex();
    }
}
