/// <summary>
/// Simple pair class
/// </summary>
/// <typeparam name="T">Type of first value</typeparam>
/// <typeparam name="T">Type of second value</typeparam>
public class Pair<T, U> {

    /// <summary>
    /// Empty constructor
    /// </summary>
    public Pair() {

    }

    /// <summary>
    /// Constructor accepting a pair
    /// </summary>
    /// <param name="first">First item</param>
    /// <param name="second">Second item</param>
    public Pair(T first, U second) {
        this.first = first;
        this.second = second;
    }

    public T first;
    public U second;
}
