using System.Collections;
using UnityEngine;

public class Ability_Invincibility : MonoBehaviour, IAbility
{
    [SerializeField] private int pointCost = 200;
    [SerializeField] private float duration = 15f;
    [SerializeField] private float cooldown = 30f;
    [SerializeField] private Color activeColor = Color.yellow;

    public bool IsActive { get; private set; } = false;

    private float cooldownRemaining = 0f;
    private Coroutine activeCoroutine;

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
        activeCoroutine = StartCoroutine(InvincibilityCoroutine());
    }

    private IEnumerator InvincibilityCoroutine()
    {
        IsActive = true;
        var player = PlayerController.Instance;
        Color normal = Color.white;
        if (player != null)
        {
            normal = player.GetNormalColor();
            var r = player.GetPlayerRenderer();
            if (r != null) r.material.color = activeColor;
        }

        yield return new WaitForSeconds(duration);

        IsActive = false;
        if (player != null)
        {
            var r = player.GetPlayerRenderer();
            if (r != null) r.material.color = normal;
        }

        cooldownRemaining = cooldown;
    }
}