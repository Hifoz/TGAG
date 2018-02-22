using System.Diagnostics;

/// <summary>
/// Little wrapper class for the stopwatch, used for debugging / profiling
/// </summary>
public class StopWatch {

    Stopwatch stopwatch = new Stopwatch();

    /// <summary>
    /// Starts the timer
    /// </summary>
    public void start() {
        stopwatch.Start();
    }

    /// <summary>
    /// Resets watch and returns time
    /// </summary>
    /// <returns></returns>
    public float reset() {
        float time = stopwatch.ElapsedMilliseconds;
        stopwatch.Reset();
        return time;
    }

    /// <summary>
    /// Stops and outputs results to console
    /// </summary>
    /// <param name="message">message to add to the output</param>
    public void done(string message) {
        stopwatch.Stop();
        UnityEngine.Debug.Log(string.Format("Stopwatch time: {0}ms __ Custom message: {1}", stopwatch.ElapsedMilliseconds, message));
        stopwatch.Reset();
    }
}
