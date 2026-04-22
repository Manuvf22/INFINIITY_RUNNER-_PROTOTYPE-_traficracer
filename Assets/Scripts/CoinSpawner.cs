using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [SerializeField] private float coinSpawnChance = 0.10f;
    [SerializeField] private float spawnDistance = 80f;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float[] lanePositions = { -4.5f, -1.5f, 1.5f, 4.5f };
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int initialPoolSize = 10;

    private bool isSpawning = false;
    private Coroutine spawnCoroutine;

    private void Start()
    {
        if (ObjectPool.Instance != null && coinPrefab != null)
            ObjectPool.Instance.PreWarm("Coin", coinPrefab, initialPoolSize);
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
            if (isSpawning && Random.value < coinSpawnChance)
                SpawnCoin();
        }
    }

    private void SpawnCoin()
    {
        if (PlayerController.Instance == null || coinPrefab == null) return;

        float spawnZ = PlayerController.Instance.transform.position.z + spawnDistance;
        int laneIdx = Random.Range(0, lanePositions.Length);

        GameObject obj = ObjectPool.Instance.GetFromPool("Coin", coinPrefab);
        obj.transform.position = new Vector3(lanePositions[laneIdx], 0.5f, spawnZ);
        obj.transform.rotation = Quaternion.identity;
        obj.SetActive(true);
    }
}