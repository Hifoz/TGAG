using System;
using System.Collections.Generic;

/// <summary>
/// Struct representing a range
/// </summary>
/// <typeparam name="T">The type of number in range</typeparam>
public struct Range<T> {
    private static HashSet<Type> numericTypes = new HashSet<Type> {
       typeof(float), typeof(int), typeof(double), typeof(long), typeof(uint)
    };

    private T min;
    private T max;

    /// <summary>
    /// Exception for invalid range
    /// </summary>
    public class InvalidRangeException : Exception {
        public InvalidRangeException (string message) : base(message) {

        }
    }

    /// <summary>
    /// Constructor taking two numbers, min and max, min cant be greater then max, but it can equal max
    /// </summary>
    /// <param name="min">minimum number in range</param>
    /// <param name="max">maximum number in range</param>
    public Range(T min, T max) {
        this.min = default(T);
        this.max = default(T);

        if (numericTypes.Contains(min.GetType())) {
            if (Comparer<T>.Default.Compare(min, max) > 0) {                
                throw new InvalidRangeException("min is greater then max!");
            }
            this.min = min;
            this.max = max;
        } else {
            throw new InvalidRangeException(string.Format("Invalid typing, T is not a number or a supported number! T is: {0}", min.GetType().ToString()));
        }
    }

    /// <summary>
    /// Gets minimum
    /// </summary>
    public T Min { get { return min; } }
    /// <summary>
    /// Gets maximum
    /// </summary>
    public T Max { get { return max; } }
}
