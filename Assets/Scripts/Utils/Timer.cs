using System.Diagnostics;

/// <summary>
/// Little wrapper class for the stopwatch, used for debugging / profiling
/// </summary>
public class Timer {
    Stopwatch stopwatch = new Stopwatch();

    /// <summary>
    /// Resets watch and returns time
    /// </summary>
    /// <returns></returns>
    public void reset() {
        stopwatch.Reset();
        stopwatch.Start();
    }

    /// <summary>
    /// Gets current elapsed time in milliseconds
    /// </summary>
    /// <returns>elapsed time in milliseconds</returns>
    public double get() {
        return stopwatch.ElapsedMilliseconds;
    } 
}
