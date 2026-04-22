using UnityEngine;

public class ObstacleMoto : ObstacleBase
{
    [Header("Moto Config")]
    [SerializeField] private Color motoColor = new Color(1f, 0.5f, 0f);

    private void Awake()
    {
        speed = 25f;
        obstacleScale = new Vector3(0.8f, 1f, 1.5f);
        poolKey = "Moto";
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        SetColor(motoColor);
    }
}