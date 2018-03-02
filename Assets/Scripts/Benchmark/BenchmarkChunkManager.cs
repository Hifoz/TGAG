using UnityEngine;
using System.Diagnostics;


public abstract class BenchmarkChunkManager : MonoBehaviour {

    protected Stopwatch stopwatch = new Stopwatch();   

    protected bool inProgress = false;
    protected string path = "";
    protected int currentThreads = 0;   

    public bool InProgress { get { return inProgress; } }
    public string Path { get { return path; } }
    public int CurrentThreads { get { return currentThreads; } }

    /// <summary>
    /// Returns current running time
    /// </summary>
    /// <returns></returns>
    public float getTime() {
        return (float)stopwatch.Elapsed.TotalSeconds;
    }

    /// <summary>
    /// Returns progress of benchmark
    /// </summary>
    /// <returns></returns>
    public abstract float getProgress();   

    
}
