using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema de explosiones 100% por codigo. Sin ParticleSystem del editor.
/// Genera cubos pequeños que salen disparados, rotan, encogen y desaparecen.
///
/// USO:
///   ExplosionEffect.Spawn(position, color);             // explosion normal
///   ExplosionEffect.Spawn(position, color, count: 20);  // mas particulas
///
/// Colocar este script en un GameObject vacio en la escena (ej: "ExplosionManager").
/// </summary>
public class ExplosionEffect : MonoBehaviour
{
    public static ExplosionEffect Instance { get; private set; }

    // ─── Config de particulas ─────────────────────────────────────────────────
    [Header("Particle Settings")]
    [Tooltip("Cantidad de cubos por explosion (default)")]
    [SerializeField] private int defaultParticleCount = 14;
    [Tooltip("Tamaño inicial de cada particula")]
    [SerializeField] private float particleSize = 0.22f;
    [Tooltip("Velocidad inicial de salida")]
    [SerializeField] private float particleSpeed = 7f;
    [Tooltip("Velocidad adicional aleatoria (spread)")]
    [SerializeField] private float particleSpeedVariance = 3f;
    [Tooltip("Duracion de vida de cada particula en segundos")]
    [SerializeField] private float particleLifetime = 0.55f;
    [Tooltip("Gravedad aplicada a las particulas (negativo = caen)")]
    [SerializeField] private float particleGravity = -12f;
    [Tooltip("Velocidad de rotacion aleatoria maxima (grados/seg)")]
    [SerializeField] private float maxRotationSpeed = 400f;

    // ─── Pool ─────────────────────────────────────────────────────────────────
    [Header("Pool")]
    [Tooltip("Particulas pre-instanciadas al inicio. Minimo: defaultParticleCount * 5")]
    [SerializeField] private int poolWarmSize = 80;

    private Queue<GameObject> pool = new Queue<GameObject>();

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        WarmPool();
    }

    // ─── Pool management ──────────────────────────────────────────────────────
    private void WarmPool()
    {
        for (int i = 0; i < poolWarmSize; i++)
            pool.Enqueue(CreateParticle());
    }

    private GameObject CreateParticle()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "ExplosionParticle";
        go.transform.SetParent(transform);
        go.transform.localScale = Vector3.one * particleSize;

        // Sin colision: no queremos que las particulas afecten gameplay
        Destroy(go.GetComponent<Collider>());

        // Material propio para poder cambiar color por instancia
        var rend = go.GetComponent<Renderer>();
        if (rend != null)
            rend.material = new Material(Shader.Find("Standard"));

        go.SetActive(false);
        return go;
    }

    private GameObject GetFromPool()
    {
        // Limpiar entradas nulas (por si algo se destruyo)
        while (pool.Count > 0 && pool.Peek() == null)
            pool.Dequeue();

        if (pool.Count > 0)
            return pool.Dequeue();

        return CreateParticle(); // expandir pool si hace falta
    }

    private void ReturnToPool(GameObject p)
    {
        if (p == null) return;
        p.SetActive(false);
        p.transform.SetParent(transform);
        pool.Enqueue(p);
    }

    // ─── API Publica ──────────────────────────────────────────────────────────

    /// <summary>
    /// Genera una explosion en <paramref name="position"/> con el <paramref name="color"/> indicado.
    /// </summary>
    /// <param name="position">Centro de la explosion en world space.</param>
    /// <param name="color">Color de las particulas.</param>
    /// <param name="count">Cantidad de cubos (-1 usa defaultParticleCount).</param>
    public static void Spawn(Vector3 position, Color color, int count = -1)
    {
        if (Instance == null)
        {
            Debug.LogWarning("[ExplosionEffect] No hay instancia en la escena. Agrega un GameObject con ExplosionEffect.");
            return;
        }
        Instance.DoSpawn(position, color, count < 0 ? Instance.defaultParticleCount : count);
    }

    // ─── Logica interna ───────────────────────────────────────────────────────
    private void DoSpawn(Vector3 position, Color color, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject p = GetFromPool();
            if (p == null) continue;

            // Posicionar en el centro de la explosion
            p.transform.position = position;
            p.transform.SetParent(null); // soltar del padre para moverse libremente
            p.transform.localScale = Vector3.one * particleSize;

            // Aplicar color
            var rend = p.GetComponent<Renderer>();
            if (rend != null && rend.material != null)
                rend.material.color = color;

            // Direccion de salida aleatoria (hemisferio superior para que vuelen hacia arriba)
            Vector3 dir = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(0.3f, 1f),   // siempre con componente hacia arriba
                Random.Range(-1f, 1f)
            ).normalized;

            float speed = particleSpeed + Random.Range(0f, particleSpeedVariance);
            Vector3 velocity = dir * speed;

            // Rotacion aleatoria
            Vector3 rotAxis = Random.insideUnitSphere.normalized;
            float rotSpeed = Random.Range(maxRotationSpeed * 0.4f, maxRotationSpeed);

            p.SetActive(true);
            StartCoroutine(AnimateParticle(p, velocity, rotAxis, rotSpeed));
        }
    }

    private IEnumerator AnimateParticle(GameObject p, Vector3 velocity, Vector3 rotAxis, float rotSpeed)
    {
        float elapsed = 0f;
        Vector3 startScale = Vector3.one * particleSize;

        while (elapsed < particleLifetime)
        {
            if (p == null) yield break;

            float t = elapsed / particleLifetime;   // 0 → 1 a lo largo de la vida

            // ── Movimiento ──────────────────────────────────────────────
            velocity.y += particleGravity * Time.deltaTime;
            p.transform.position += velocity * Time.deltaTime;

            // ── Escala: encoge progresivamente hasta desaparecer ────────
            p.transform.localScale = startScale * (1f - t);

            // ── Rotacion libre ──────────────────────────────────────────
            p.transform.Rotate(rotAxis, rotSpeed * Time.deltaTime, Space.World);

            elapsed += Time.deltaTime;
            yield return null;
        }

        ReturnToPool(p);
    }
}