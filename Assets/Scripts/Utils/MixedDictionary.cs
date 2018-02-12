using System.Collections.Generic;

/// <summary>
/// A Type T Key, object Item dict.
/// </summary>
/// <typeparam name="T">T type of key</typeparam>
public class MixedDictionary<T> {

    private Dictionary<T, object> dict = new Dictionary<T, object>();

    /// <summary>
    /// Empty constructor
    /// </summary>
    public MixedDictionary() {
        
    }

    /// <summary>
    /// Constructor accepting a dict
    /// </summary>
    public MixedDictionary(Dictionary<T, object> dict) {
        this.dict = dict;
    }

    /// <summary>
    /// Adds an item of type U for key of type T
    /// </summary>
    /// <typeparam name="U">Type of item to add</typeparam>
    /// <param name="key">T key</param>
    /// <param name="item">U item</param>
    public void Add<U>(T key, U item) {
        dict.Add(key, item);
    }

    /// <summary>
    /// Gets the item for the key of type U
    /// </summary>
    /// <typeparam name="U">type of item to get</typeparam>
    /// <param name="key">T key</param>
    /// <returns>item of type U</returns>
    public U Get<U>(T key) {
        return (U)dict[key];
    }

    /// <summary>
    /// Adds object to dict
    /// </summary>
    /// <param name="key">T key</param>
    /// <param name="item">object item</param>
    public void Add(T key, object item) {
        dict.Add(key, item);
    }

    /// <summary>
    /// Gets object from dict
    /// </summary>
    /// <param name="key">T key</param>
    /// <returns>object item</returns>
    public object Get(T key) {
        return dict[key];
    }

    /// <summary>
    /// Returns internal dictionary
    /// </summary>
    /// <returns>Dictionary<T, object> dict</returns>
    public Dictionary<T, object> getDict() {
        return dict;
    }

    /// <summary>
    /// Clears the dictionary
    /// </summary>
    public void Clear() {
        dict.Clear();
    }
}
