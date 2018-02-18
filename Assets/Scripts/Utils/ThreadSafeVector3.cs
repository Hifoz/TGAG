using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public class ThreadSafeVector3 {
    private Vector3 vec;
    private Mutex mutex = new Mutex();

    public ThreadSafeVector3() {
        this.vec = new Vector3();
    }


    public ThreadSafeVector3(Vector3 vec) {
        this.vec = vec;
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
