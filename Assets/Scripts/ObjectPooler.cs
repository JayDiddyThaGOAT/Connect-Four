using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class ObjectPooler : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    private GameObject objectPrefab;

    [SerializeField]
    private int objectPoolSize;

    [SerializeField]
    private bool isExpandable = false;
#pragma warning restore 0649

    private List<GameObject> freeList;
    private List<GameObject> usedList;

    void Awake()
    {
        freeList = new List<GameObject>();
        usedList = new List<GameObject>();

        for (int i = 0; i < objectPoolSize; i++)
        {
            InstantiateNewObject();
        }
    }
    
    /// <summary>
    /// Generates new game object w/ objectPrefab then adds it to the object pool
    /// </summary>
    void InstantiateNewObject()
    {
        GameObject obj = Instantiate<GameObject>(objectPrefab, transform);
        obj.SetActive(false);
        freeList.Add(obj);
    }

    /// <summary>
    /// Gets object from the object pool
    /// </summary>
    public GameObject GetPooledObject()
    {
        if (freeList.Count == 0)
        {
            if (!isExpandable)
                return null;
            else
                InstantiateNewObject();
        }

        GameObject obj = freeList[freeList.Count - 1];
        obj.SetActive(true);
        
        freeList.RemoveAt(freeList.Count - 1);
        usedList.Add(obj);

        return obj;
    }

    /// <summary>
    /// Returns object to the object pool with default position and rotation
    /// </summary>
    public void ReturnPooledObject(GameObject obj)
    {
        Debug.Assert(usedList.Contains(obj));
        obj.SetActive(false);
        obj.transform.position = objectPrefab.transform.position;
        obj.transform.rotation = objectPrefab.transform.rotation;

        usedList.Remove(obj);
        freeList.Add(obj);
    }

}
