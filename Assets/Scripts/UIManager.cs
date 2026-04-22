using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Main Menu Panel")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    [Header("Pause Panel")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseToMenuButton;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameOverHighScoreText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button gameOverToMenuButton;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Wire buttons
        if (playButton != null) playButton.onClick.AddListener(OnPlayPressed);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitPressed);
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumePressed);
        if (pauseToMenuButton != null) pauseToMenuButton.onClick.AddListener(OnMenuPressed);
        if (retryButton != null) retryButton.onClick.AddListener(OnRetryPressed);
        if (gameOverToMenuButton != null) gameOverToMenuButton.onClick.AddListener(OnMenuPressed);

        // Initial state: show main menu only
        ShowMainMenu(true);
        ShowPause(false);
        ShowGameOver(0f, 0f);
        // Make GameOver panel invisible but don't call with scores
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    // ── Main Menu ────────────────────────────────────────────────
    public void ShowMainMenu(bool show)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(show);
    }

    private void OnPlayPressed()
    {
        ShowMainMenu(false);
        if (GameManager.Instance != null) GameManager.Instance.StartGame();
    }

    private void OnQuitPressed()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ── Pause ────────────────────────────────────────────────────
    public void ShowPause(bool show)
    {
        if (pausePanel != null) pausePanel.SetActive(show);
    }

    private void OnResumePressed()
    {
        if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
    }

    private void OnMenuPressed()
    {
        if (GameManager.Instance != null) GameManager.Instance.GoToMainMenu();
    }

    // ── Game Over ────────────────────────────────────────────────
    public void ShowGameOver(float finalScore, float highScore)
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (gameOverScoreText != null)
            gameOverScoreText.text = $"PUNTAJE: {Mathf.RoundToInt(finalScore):D5}";
        if (gameOverHighScoreText != null)
            gameOverHighScoreText.text = $"MEJOR: {Mathf.RoundToInt(highScore):D5}";
    }

    private void OnRetryPressed()
    {
        if (GameManager.Instance != null) GameManager.Instance.RestartGame();
    }
}