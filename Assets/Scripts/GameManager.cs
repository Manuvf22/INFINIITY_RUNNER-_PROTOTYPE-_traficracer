using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { MainMenu, Playing, Paused, GameOver }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private ObstacleSpawner obstacleSpawner;
    [SerializeField] private CoinSpawner coinSpawner;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private HUDManager hudManager;
    [SerializeField] private DifficultyManager difficultyManager;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        SetState(GameState.MainMenu);
    }

    public void StartGame()
    {
        SetState(GameState.Playing);
        Time.timeScale = 1f;
        if (playerController != null) playerController.EnableInput(true);
        if (obstacleSpawner != null) obstacleSpawner.SetSpawning(true);
        if (coinSpawner != null) coinSpawner.SetSpawning(true);
        if (scoreManager != null) scoreManager.ResetScore();
        if (difficultyManager != null) difficultyManager.ResetDifficulty();
        if (soundManager != null) soundManager.StartEngine();
        if (hudManager != null) hudManager.ShowHUD(true);
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;
        SetState(GameState.Paused);
        Time.timeScale = 0f;
        if (uiManager != null) uiManager.ShowPause(true);
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;
        SetState(GameState.Playing);
        Time.timeScale = 1f;
        if (uiManager != null) uiManager.ShowPause(false);
    }

    public void GameOver()
    {
        if (CurrentState == GameState.GameOver) return;
        SetState(GameState.GameOver);
        if (obstacleSpawner != null) obstacleSpawner.SetSpawning(false);
        if (coinSpawner != null) coinSpawner.SetSpawning(false);
        if (playerController != null) playerController.EnableInput(false);
        if (scoreManager != null) scoreManager.SaveHighScore();
        if (soundManager != null) soundManager.StopEngine();
        if (uiManager != null) uiManager.ShowGameOver(
            scoreManager != null ? scoreManager.CurrentScore : 0f,
            scoreManager != null ? scoreManager.HighScore : 0f);
        if (hudManager != null) hudManager.ShowHUD(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void SetState(GameState state)
    {
        CurrentState = state;
    }

    private void Update()
    {
        if (CurrentState == GameState.Playing && Input.GetKeyDown(KeyCode.Escape))
            PauseGame();
        else if (CurrentState == GameState.Paused && Input.GetKeyDown(KeyCode.Escape))
            ResumeGame();
    }
}