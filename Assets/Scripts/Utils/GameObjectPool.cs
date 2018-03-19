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

    bool maintainActive = true;
    private List<GameObject> active;
    private Stack<GameObject> inactive = new Stack<GameObject>();


    /// <summary>
    /// Constructor taking a prefab
    /// </summary>
    /// <param name="prefab">The prefab that the pool pools</param>
    /// <param name="objParent">The transform parent of the obj</param>
    /// <param name="objName">The name of the objects</param>
    /// <param name="maintainActive">Should this pool keep track of the active objects?</param>
    public GameObjectPool(GameObject prefab, Transform objParent = null, string objName = null, bool maintainActive = true) {
        this.prefab = prefab;
        this.objParent = objParent;
        this.objName = objName;
        this.maintainActive = maintainActive;
        if (maintainActive) {
            active = new List<GameObject>();
        }
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
                obj.transform.parent = objParent;
            }
            if (objName != null) {
                obj.name = objName;
            }           
        } else {
            obj = inactive.Pop();            
            obj.SetActive(true);
        }
        if (maintainActive) {
            active.Add(obj);
        }
        return obj;
    }

    /// <summary>
    /// Returns a GameObject to the pool
    /// </summary>
    /// <param name="obj">GameObject to return</param>
    public void returnObject(GameObject obj) {
        if (maintainActive && !active.Remove(obj)) {
            Debug.LogWarning("The object returned to the pool was not part of the pools active list!");
        }
        inactive.Push(obj);
        obj.SetActive(false);
    }

    /// <summary>
    /// Destroys all gameobjects in the pool
    /// </summary>
    public void destroyAllGameObjects() {
        if (maintainActive) {
            foreach (GameObject obj in active) {
                MonoBehaviour.Destroy(obj);
            }
            active.Clear();
        }
        foreach (GameObject obj in inactive) {
            MonoBehaviour.Destroy(obj);
        }        
        inactive.Clear();
    }
}
