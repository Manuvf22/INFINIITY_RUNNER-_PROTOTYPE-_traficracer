using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [SerializeField] private float difficultyIncreaseInterval = 30f;
    [SerializeField] private float spawnIntervalDecrement = 0.1f;
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private float baseSpeedIncrement = 2f;
    [SerializeField] private float maxObstacleSpeedMultiplier = 2.5f;

    [SerializeField] private ObstacleSpawner obstacleSpawner;

    public int CurrentDifficultyLevel { get; private set; } = 0;

    private float timer = 0f;
    private float baseSpawnInterval;

    // Registry pattern — obstacles register themselves, no FindObjectsOfType needed
    private readonly List<ObstacleBase> activeObstacles = new List<ObstacleBase>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (obstacleSpawner != null)
            baseSpawnInterval = obstacleSpawner.spawnInterval;
    }

    public void RegisterObstacle(ObstacleBase obs)
    {
        if (obs != null && !activeObstacles.Contains(obs))
            activeObstacles.Add(obs);
    }

    public void UnregisterObstacle(ObstacleBase obs)
    {
        activeObstacles.Remove(obs);
    }

    public void ResetDifficulty()
    {
        CurrentDifficultyLevel = 0;
        timer = 0f;
        activeObstacles.Clear();
        if (obstacleSpawner != null)
            obstacleSpawner.SetSpawnInterval(baseSpawnInterval);
    }

    private void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        timer += Time.deltaTime;
        if (timer >= difficultyIncreaseInterval)
        {
            timer = 0f;
            IncreaseDifficulty();
        }
    }

    private void IncreaseDifficulty()
    {
        CurrentDifficultyLevel++;

        if (obstacleSpawner != null)
        {
            float newInterval = Mathf.Max(
                obstacleSpawner.spawnInterval - spawnIntervalDecrement,
                minSpawnInterval);
            obstacleSpawner.SetSpawnInterval(newInterval);
        }

        // Speed up active obstacles via registry — no deprecated API
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            if (activeObstacles[i] == null) { activeObstacles.RemoveAt(i); continue; }
            float cap = activeObstacles[i].speed * maxObstacleSpeedMultiplier;
            activeObstacles[i].speed = Mathf.Min(
                activeObstacles[i].speed + baseSpeedIncrement, cap);
        }
    }
}