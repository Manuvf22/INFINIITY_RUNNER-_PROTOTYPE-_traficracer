using System.Collections;
using UnityEngine;

public class Ability_Projectile : MonoBehaviour, IAbility
{
    [SerializeField] private int pointCost = 100;
    [SerializeField] private float cooldown = 15f;
    [SerializeField] public float projectileSpeed = 60f;
    [SerializeField] public float projectileMaxRange = 80f;
    [SerializeField] private GameObject projectilePrefab;

    public bool IsActive { get; private set; } = false;
    private float cooldownRemaining = 0f;

    private void Update()
    {
        if (cooldownRemaining > 0f)
            cooldownRemaining -= Time.deltaTime;
        if (cooldownRemaining < 0f) cooldownRemaining = 0f;
    }

    public bool IsReady() => cooldownRemaining <= 0f;
    public int GetPointCost() => pointCost;
    public float GetCooldownRemaining() => cooldownRemaining;

    public void Activate()
    {
        if (projectilePrefab == null) return;

        GameObject proj = ObjectPool.Instance.GetFromPool("Projectile", projectilePrefab);
        proj.transform.position = transform.position + Vector3.up * 0.5f;
        proj.transform.rotation = Quaternion.identity;

        var projComponent = proj.GetComponent<Projectile>();
        if (projComponent == null) return; // si no está en el prefab, no dispara

        // La velocidad del proyectil = velocidad propia + velocidad actual del jugador
        // así nunca se queda atrás sin importar qué tan rápido vaya el auto
        float playerSpeed = 0f;
        if (PlayerController.Instance != null)
            playerSpeed = PlayerController.Instance.currentSpeed;

        projComponent.Initialize(projectileSpeed + playerSpeed, projectileMaxRange, proj.transform.position);

        proj.SetActive(true);
        cooldownRemaining = cooldown;
    }
}

public class Projectile : MonoBehaviour
{
    private float speed;
    private float maxRange;
    private Vector3 startPosition;

    public void Initialize(float spd, float range, Vector3 startPos)
    {
        speed = spd;
        maxRange = range;
        startPosition = startPos;
    }

    private void Update()
    {
        transform.position += Vector3.forward * speed * Time.deltaTime;

        if (Vector3.Distance(startPosition, transform.position) >= maxRange)
            ReturnToPool();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Intenta obtener ObstacleBase tanto del objeto como de sus padres
        // Esto evita el problema de que el tag no esté asignado
        ObstacleBase obstacle = other.GetComponent<ObstacleBase>();
        if (obstacle == null)
            obstacle = other.GetComponentInParent<ObstacleBase>();

        if (obstacle != null)
        {
            // ── EXPLOSIÓN DEL OBSTÁCULO ───────────────────────────────────
            ExplosionEffect.Spawn(other.transform.position, new Color(1f, 0.55f, 0f), count: 16);
            // ─────────────────────────────────────────────────────────────
            obstacle.ReturnToPool();
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayObstacleDestroy();
            ReturnToPool();
            return;
        }

        // Fallback: si tiene el tag Obstacle pero no el componente
        if (other.CompareTag("Obstacle"))
        {
            other.gameObject.SetActive(false);
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        ObjectPool.Instance.ReturnToPool("Projectile", gameObject);
    }
}