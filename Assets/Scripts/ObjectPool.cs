using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObject> prefabRegistry = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void PreWarm(string key, GameObject prefab, int count)
    {
        if (!pools.ContainsKey(key))
            pools[key] = new Queue<GameObject>();
        if (!prefabRegistry.ContainsKey(key))
            prefabRegistry[key] = prefab;

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pools[key].Enqueue(obj);
        }
    }

    public GameObject GetFromPool(string key, GameObject prefab)
    {
        if (!pools.ContainsKey(key))
            pools[key] = new Queue<GameObject>();
        if (!prefabRegistry.ContainsKey(key))
            prefabRegistry[key] = prefab;

        if (pools[key].Count > 0)
        {
            GameObject obj = pools[key].Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // Expand pool
            GameObject obj = Instantiate(prefab, transform);
            return obj;
        }
    }

    public void ReturnToPool(string key, GameObject obj)
    {
        if (obj == null) return;
        obj.SetActive(false);
        if (!pools.ContainsKey(key))
            pools[key] = new Queue<GameObject>();
        pools[key].Enqueue(obj);
    }
}