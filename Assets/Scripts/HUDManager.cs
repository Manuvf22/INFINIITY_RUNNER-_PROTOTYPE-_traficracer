using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("Speed Display")]
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private float speedDisplayMultiplier = 3.6f;

    [Header("Score Display")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Color multiplierColor = new Color(1f, 0.85f, 0f);
    [SerializeField] private float multiplierBlinkInterval = 0.5f;

    [Header("Lives Display")]
    [SerializeField] private GameObject[] lifeIcons; // Assign 3 heart/cube icons

    [Header("Ability Panels")]
    [SerializeField] private AbilityPanelUI[] abilityPanels; // 3 panels

    [Header("HUD Root")]
    [SerializeField] private GameObject hudRoot;

    // Internal state
    private bool multiplierBlinkState = false;
    private float multiplierBlinkTimer = 0f;
    private Coroutine multiplierBlinkCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ShowHUD(bool show)
    {
        if (hudRoot != null) hudRoot.SetActive(show);
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        UpdateSpeed();
        UpdateScore();
        UpdateLives();
        UpdateAbilityPanels();
    }

    private void UpdateSpeed()
    {
        if (speedText == null || PlayerController.Instance == null) return;
        float displaySpeed = PlayerController.Instance.currentSpeed * speedDisplayMultiplier;
        speedText.text = $"{Mathf.RoundToInt(displaySpeed)} km/h";
    }

    private void UpdateScore()
    {
        if (scoreText == null || ScoreManager.Instance == null) return;

        int score = Mathf.RoundToInt(ScoreManager.Instance.CurrentScore);

        if (ScoreManager.Instance.IsMultiplierActive)
        {
            // Blink logic
            multiplierBlinkTimer += Time.deltaTime;
            if (multiplierBlinkTimer >= multiplierBlinkInterval)
            {
                multiplierBlinkTimer = 0f;
                multiplierBlinkState = !multiplierBlinkState;
            }
            Color c = multiplierBlinkState ? multiplierColor : new Color(multiplierColor.r, multiplierColor.g, multiplierColor.b, 0.5f);
            scoreText.color = c;
            scoreText.text = $"SCORE: {score:D5}  \u2726 x2";
        }
        else
        {
            scoreText.color = Color.white;
            scoreText.text = $"SCORE: {score:D5}";
        }

        if (highScoreText != null)
        {
            int hs = Mathf.RoundToInt(ScoreManager.Instance.HighScore);
            highScoreText.text = $"BEST: {hs:D5}";
        }
    }

    private void UpdateLives()
    {
        if (lifeIcons == null || PlayerController.Instance == null) return;

        int lives = PlayerController.Instance.CurrentLives;
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] != null)
                lifeIcons[i].SetActive(i < lives);
        }
    }

    private void UpdateAbilityPanels()
    {
        if (abilityPanels == null || AbilityManager.Instance == null || ScoreManager.Instance == null) return;

        string[] names = { "1-INVENC.", "2-DISPARO", "3-SHRINK" };
        string[] keys = { "[1]", "[2]", "[3]" };

        for (int i = 0; i < abilityPanels.Length; i++)
        {
            if (abilityPanels[i] == null) continue;

            var ability = AbilityManager.Instance.GetAbility(i);
            if (ability == null) continue;

            int cost = ability.GetPointCost();
            float cd = ability.GetCooldownRemaining();
            float score = ScoreManager.Instance.CurrentScore;

            abilityPanels[i].SetName(names[i]);
            abilityPanels[i].SetKey(keys[i]);
            abilityPanels[i].SetCost(cost);

            if (cd > 0f)
            {
                // Cooldown
                abilityPanels[i].SetState(AbilityPanelState.Cooldown, $"Cd: {Mathf.CeilToInt(cd)}s");
            }
            else if (score < cost)
            {
                // Insufficient points
                abilityPanels[i].SetState(AbilityPanelState.NoPoints, "Sin puntos");
            }
            else
            {
                // Ready
                abilityPanels[i].SetState(AbilityPanelState.Ready, "LISTO");
            }
        }
    }
}

// ── Ability Panel State Enum ──────────────────────────────────────
public enum AbilityPanelState { Ready, Cooldown, NoPoints }

// ── Helper component for each ability panel UI ───────────────────
[System.Serializable]
public class AbilityPanelUI
{
    [Header("Panel References")]
    public Image panelBackground;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI keyText;

    [Header("Colors")]
    public Color readyColor = new Color(0.1f, 0.7f, 0.1f, 0.85f);
    public Color cooldownColor = new Color(0.7f, 0.1f, 0.1f, 0.85f);
    public Color noPointsColor = new Color(0.4f, 0.4f, 0.4f, 0.85f);

    public void SetName(string n)
    {
        if (nameText != null) nameText.text = n;
    }

    public void SetKey(string k)
    {
        if (keyText != null) keyText.text = k;
    }

    public void SetCost(int cost)
    {
        if (costText != null) costText.text = $"[{cost} pts]";
    }

    public void SetState(AbilityPanelState state, string statusLabel)
    {
        if (statusText != null) statusText.text = statusLabel;

        if (panelBackground == null) return;
        switch (state)
        {
            case AbilityPanelState.Ready: panelBackground.color = readyColor; break;
            case AbilityPanelState.Cooldown: panelBackground.color = cooldownColor; break;
            case AbilityPanelState.NoPoints: panelBackground.color = noPointsColor; break;
        }
    }
}