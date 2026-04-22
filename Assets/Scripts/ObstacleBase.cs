using UnityEngine;

public abstract class ObstacleBase : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [SerializeField] public float speed = 10f;
    [SerializeField] public Vector3 obstacleScale = Vector3.one;

    [HideInInspector] public int laneIndex;
    [HideInInspector] public string poolKey;

    [SerializeField] private float despawnOffset = 20f;

    protected virtual void Update()
    {
        transform.position += Vector3.back * speed * Time.deltaTime;

        if (PlayerController.Instance != null)
        {
            float playerZ = PlayerController.Instance.transform.position.z;
            if (transform.position.z < playerZ - despawnOffset)
                ReturnToPool();
        }
    }

    public virtual void OnSpawn()
    {
        transform.localScale = obstacleScale;
        // Register with DifficultyManager so it can adjust speed without FindObjectsOfType
        if (DifficultyManager.Instance != null)
            DifficultyManager.Instance.RegisterObstacle(this);
    }

    public virtual void OnDespawn()
    {
        if (DifficultyManager.Instance != null)
            DifficultyManager.Instance.UnregisterObstacle(this);
    }

    public void ReturnToPool()
    {
        OnDespawn();
        if (!string.IsNullOrEmpty(poolKey))
            ObjectPool.Instance.ReturnToPool(poolKey, gameObject);
        else
            gameObject.SetActive(false);
    }

    // Helper para subclases: usa sharedMaterial para evitar leak de materiales en Edit mode
    protected void SetColor(Color color)
    {
        var r = GetComponentInChildren<Renderer>();
        if (r == null) return;

        // sharedMaterial en Edit mode, material en Play mode
        if (Application.isPlaying)
            r.material.color = color;
        else
            r.sharedMaterial.color = color;
    }
}