using System.Collections;
using UnityEngine;

public class Ability_Shrink : MonoBehaviour, IAbility
{
    [SerializeField] private int pointCost = 400;
    [SerializeField] private float duration = 10f;
    [SerializeField] private float cooldown = 45f;
    [SerializeField] private float shrinkScale = 0.5f;
    [SerializeField] private float bonusMaxSpeed = 120f;
    [SerializeField] private float bonusAcceleration = 15f;

    public bool IsActive { get; private set; } = false;

    private float cooldownRemaining = 0f;
    private Coroutine activeCoroutine;
    private Vector3 originalScale;
    private float originalMaxSpeed;
    private float originalAccelRate;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        if (cooldownRemaining > 0f)
            cooldownRemaining -= Time.deltaTime;
        if (cooldownRemaining < 0f) cooldownRemaining = 0f;
    }

    public bool IsReady() => !IsActive && cooldownRemaining <= 0f;
    public int GetPointCost() => pointCost;
    public float GetCooldownRemaining() => cooldownRemaining;

    public void Activate()
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(ShrinkCoroutine());
    }

    private IEnumerator ShrinkCoroutine()
    {
        IsActive = true;

        var player = PlayerController.Instance;
        if (player != null)
        {
            originalMaxSpeed = player.maxSpeed;
            originalAccelRate = player.accelerationRate;
            player.SetSpeedOverride(bonusMaxSpeed, bonusAcceleration);
        }

        originalScale = transform.localScale;
        transform.localScale = originalScale * shrinkScale;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayAbility3Music();

        yield return new WaitForSeconds(duration);

        // Restore
        transform.localScale = originalScale;
        if (player != null)
            player.RestoreSpeedDefaults(originalMaxSpeed, originalAccelRate);

        if (SoundManager.Instance != null)
            SoundManager.Instance.StopAbility3Music();

        IsActive = false;
        cooldownRemaining = cooldown;
    }
}