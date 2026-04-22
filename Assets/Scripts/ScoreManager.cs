using System.Collections;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private float pointsPerMeter = 1f;
    [SerializeField] private float multiplierDuration = 10f;

    public float CurrentScore { get; private set; } = 0f;
    public bool IsMultiplierActive { get; private set; } = false;
    public float MultiplierTimeRemaining { get; private set; } = 0f;
    public float HighScore => PlayerPrefs.GetFloat("HighScore", 0f);

    private float lastPlayerZ = 0f;
    private Coroutine multiplierCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (PlayerController.Instance != null)
            lastPlayerZ = PlayerController.Instance.transform.position.z;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (PlayerController.Instance == null) return;

        float currentZ = PlayerController.Instance.transform.position.z;
        float delta = currentZ - lastPlayerZ;
        lastPlayerZ = currentZ;

        if (delta > 0f)
        {
            float multiplier = IsMultiplierActive ? 2f : 1f;
            CurrentScore += delta * pointsPerMeter * multiplier;
        }

        if (IsMultiplierActive)
        {
            MultiplierTimeRemaining -= Time.deltaTime;
            if (MultiplierTimeRemaining <= 0f)
            {
                IsMultiplierActive = false;
                MultiplierTimeRemaining = 0f;
            }
        }
    }

    public void ActivateMultiplier()
    {
        IsMultiplierActive = true;
        MultiplierTimeRemaining = multiplierDuration;
    }

    public void SpendPoints(int amount)
    {
        CurrentScore -= amount;
        if (CurrentScore < 0f) CurrentScore = 0f;
    }

    public void ResetScore()
    {
        CurrentScore = 0f;
        IsMultiplierActive = false;
        MultiplierTimeRemaining = 0f;
        if (PlayerController.Instance != null)
            lastPlayerZ = PlayerController.Instance.transform.position.z;
    }

    public void SaveHighScore()
    {
        if (CurrentScore > HighScore)
            PlayerPrefs.SetFloat("HighScore", CurrentScore);
        PlayerPrefs.Save();
    }
}