using UnityEngine;
using System.Collections.Generic;

//This class does not provide any max size constraints, we may or may not want that.
//I'm thinking that it would ideally just grow to its needed size and stay there.

/// <summary>
/// A class for pooling gameobjects
/// </summary>
public class GameObjectPool {
    private GameObject prefab; //The gameobject that this pool handles
    private Transform objParent;
    private string objName;

    private List<GameObject> active = new List<GameObject>();
    private Stack<GameObject> inactive = new Stack<GameObject>();

    /// <summary>
    /// Constructor taking a prefab
    /// </summary>
    /// <param name="prefab">The prefab that the pool pools</param>
    /// <param name="objParent">The transform parent of the obj</param>
    /// <param name="objName">The name of the objects</param>
    public GameObjectPool(GameObject prefab, Transform objParent = null, string objName = null) {
        this.prefab = prefab;
        this.objParent = objParent;
        this.objName = objName;
    }

    /// <summary>
    /// Destructor that clears the pool
    /// </summary>
    ~GameObjectPool() {
        foreach(GameObject obj in active) {
            MonoBehaviour.Destroy(obj);
        }
        foreach (GameObject obj in inactive) {
            MonoBehaviour.Destroy(obj);
        }
        active.Clear();
        inactive.Clear();
    }

    public List<GameObject> activeList { get { return active; } }
    public Stack<GameObject> inactiveStack { get { return inactive; } }

    /// <summary>
    /// Gets an object from the pool
    /// </summary>
    /// <returns>GameObject instance</returns>
    public GameObject getObject() {
        GameObject obj;
        if (inactive.Count == 0) {
            obj = MonoBehaviour.Instantiate(prefab);
            if (objParent != null) {
                obj.transform.SetParent(objParent);
            }
            if (objName != null) {
                obj.name = objName;
            }
            active.Add(obj);            
        } else {
            obj = inactive.Pop();
            active.Add(obj);
            obj.SetActive(true);
        }
        return obj;
    }

    /// <summary>
    /// Returns a GameObject to the pool
    /// </summary>
    /// <param name="obj">GameObject to return</param>
    public void returnObject(GameObject obj) {
        if (!active.Remove(obj)) {
            Debug.LogWarning("The object returned to the pool was not part of the pools active list!");
        }
        inactive.Push(obj);
        obj.SetActive(true);
    }
}
