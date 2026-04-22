using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Lane Settings")]
    [SerializeField] public float[] lanePositions = { -4.5f, -1.5f, 1.5f, 4.5f };

    [Header("Spawn Settings")]
    [SerializeField] public float spawnDistance = 80f;
    [SerializeField] public float spawnInterval = 2f;

    [Header("Prefabs")]
    [SerializeField] private GameObject truckPrefab;
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private GameObject motoPrefab;

    [Header("Weights")]
    [SerializeField] public float truckWeight = 0.25f;
    [SerializeField] public float carWeight = 0.50f;
    [SerializeField] public float motoWeight = 0.25f;

    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 10;

    private bool isSpawning = false;
    private Coroutine spawnCoroutine;

    private void Start()
    {
        // Pre-warm pools
        if (ObjectPool.Instance != null)
        {
            if (truckPrefab != null) ObjectPool.Instance.PreWarm("Truck", truckPrefab, initialPoolSize);
            if (carPrefab != null) ObjectPool.Instance.PreWarm("Car", carPrefab, initialPoolSize);
            if (motoPrefab != null) ObjectPool.Instance.PreWarm("Moto", motoPrefab, initialPoolSize);
        }
    }

    public void SetSpawning(bool active)
    {
        isSpawning = active;
        if (active && spawnCoroutine == null)
            spawnCoroutine = StartCoroutine(SpawnLoop());
        else if (!active && spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (isSpawning)
        {
            yield return new WaitForSeconds(spawnInterval);
            if (isSpawning) SpawnWave();
        }
    }

    private void SpawnWave()
    {
        if (PlayerController.Instance == null) return;

        float spawnZ = PlayerController.Instance.transform.position.z + spawnDistance;

        // Block 1 or 2 lanes
        int lanesToBlock = Random.value < 0.5f ? 1 : 2;
        List<int> availableLanes = new List<int> { 0, 1, 2, 3 };
        List<int> blockedLanes = new List<int>();

        for (int i = 0; i < lanesToBlock; i++)
        {
            int pick = Random.Range(0, availableLanes.Count);
            blockedLanes.Add(availableLanes[pick]);
            availableLanes.RemoveAt(pick);
        }

        foreach (int laneIdx in blockedLanes)
        {
            SpawnObstacleInLane(laneIdx, spawnZ);
        }
    }

    private void SpawnObstacleInLane(int laneIndex, float spawnZ)
    {
        string key;
        GameObject prefab;
        PickObstacleType(out key, out prefab);
        if (prefab == null) return;

        GameObject obj = ObjectPool.Instance.GetFromPool(key, prefab);
        obj.transform.position = new Vector3(lanePositions[laneIndex], 0.5f, spawnZ);
        obj.transform.rotation = Quaternion.identity;

        var obstacle = obj.GetComponent<ObstacleBase>();
        if (obstacle != null)
        {
            obstacle.laneIndex = laneIndex;
            obstacle.poolKey = key;
            obstacle.OnSpawn();
        }
    }

    private void PickObstacleType(out string key, out GameObject prefab)
    {
        float total = truckWeight + carWeight + motoWeight;
        float r = Random.value * total;

        if (r < truckWeight)
        { key = "Truck"; prefab = truckPrefab; }
        else if (r < truckWeight + carWeight)
        { key = "Car"; prefab = carPrefab; }
        else
        { key = "Moto"; prefab = motoPrefab; }
    }

    // Called by DifficultyManager
    public void SetSpawnInterval(float interval)
    {
        spawnInterval = interval;
    }
}