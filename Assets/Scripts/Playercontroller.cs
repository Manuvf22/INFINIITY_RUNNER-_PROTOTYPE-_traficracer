using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Lane Settings")]
    [SerializeField] public float[] lanePositions = { -4.5f, -1.5f, 1.5f, 4.5f };

    [Header("Speed Settings")]
    [SerializeField] public float minSpeed = 10f;
    [SerializeField] public float maxSpeed = 80f;
    [SerializeField] public float currentSpeed = 20f;
    [SerializeField] public float accelerationRate = 5f;
    [SerializeField] public float decelerationRate = 8f;

    [Header("Lane Change")]
    [SerializeField] public float laneChangeSpeed = 8f;

    [Header("Lives")]
    [SerializeField] public int maxLives = 3;
    [SerializeField] public float hitInvincibilityDuration = 2f;

    [Header("Visual")]
    [SerializeField] private Renderer playerRenderer;
    [SerializeField] private Color normalColor = Color.cyan;

    // Public state
    public int CurrentLives { get; private set; }
    public bool IsInputEnabled { get; private set; } = false;
    public bool IsHitInvincible { get; private set; } = false;

    private int currentLaneIndex = 1;
    private float targetX;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        CurrentLives = maxLives;
        currentLaneIndex = 1;
        targetX = lanePositions[currentLaneIndex];
        Vector3 pos = transform.position;
        pos.x = targetX;
        transform.position = pos;

        if (playerRenderer == null)
            playerRenderer = GetComponentInChildren<Renderer>();
        if (playerRenderer != null)
            playerRenderer.material.color = normalColor;
    }

    private void Update()
    {
        if (!IsInputEnabled) return;

        HandleMovement();
        HandleLaneChange();
        HandleAbilityInput();
    }

    private void HandleMovement()
    {
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            currentSpeed += accelerationRate * Time.deltaTime;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            currentSpeed -= decelerationRate * Time.deltaTime;

        currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxSpeed);

        Vector3 pos = transform.position;
        pos.z += currentSpeed * Time.deltaTime;
        pos.x = Mathf.Lerp(pos.x, targetX, laneChangeSpeed * Time.deltaTime);
        transform.position = pos;
    }

    private void HandleLaneChange()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (currentLaneIndex > 0)
            {
                currentLaneIndex--;
                targetX = lanePositions[currentLaneIndex];
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (currentLaneIndex < lanePositions.Length - 1)
            {
                currentLaneIndex++;
                targetX = lanePositions[currentLaneIndex];
            }
        }
    }

    private void HandleAbilityInput()
    {
        var abilityManager = AbilityManager.Instance;
        if (abilityManager == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) abilityManager.TryActivateAbility(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) abilityManager.TryActivateAbility(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) abilityManager.TryActivateAbility(2);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            HandleObstacleCollision();
        }
        else if (other.CompareTag("Coin"))
        {
            var coin = other.GetComponent<Coin>();
            if (coin != null) coin.Collect();
        }
    }

    private void HandleObstacleCollision()
    {
        if (IsHitInvincible) return;

        var invincibility = GetComponent<Ability_Invincibility>();
        if (invincibility != null && invincibility.IsActive) return;

        CurrentLives--;
        if (SoundManager.Instance != null) SoundManager.Instance.PlayCrash();

        if (CurrentLives <= 0)
        {
            CurrentLives = 0;
            GameManager.Instance.GameOver();
        }
        else
        {
            StartCoroutine(HitInvincibilityCoroutine());
        }
    }

    private IEnumerator HitInvincibilityCoroutine()
    {
        IsHitInvincible = true;
        float elapsed = 0f;
        while (elapsed < hitInvincibilityDuration)
        {
            if (playerRenderer != null)
                playerRenderer.enabled = !playerRenderer.enabled;
            yield return new WaitForSeconds(0.15f);
            elapsed += 0.15f;
        }
        if (playerRenderer != null) playerRenderer.enabled = true;
        IsHitInvincible = false;
    }

    public void EnableInput(bool enabled)
    {
        IsInputEnabled = enabled;
    }

    public void SetNormalColor(Color c)
    {
        normalColor = c;
        if (playerRenderer != null) playerRenderer.material.color = c;
    }

    public Color GetNormalColor() => normalColor;

    public Renderer GetPlayerRenderer() => playerRenderer;

    // Allow abilities to override maxSpeed and accelerationRate temporarily
    public void SetSpeedOverride(float newMax, float newAccel)
    {
        maxSpeed = newMax;
        accelerationRate = newAccel;
    }

    public void RestoreSpeedDefaults(float origMax, float origAccel)
    {
        maxSpeed = origMax;
        accelerationRate = origAccel;
    }
}