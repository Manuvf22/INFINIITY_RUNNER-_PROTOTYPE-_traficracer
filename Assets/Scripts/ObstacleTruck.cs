using UnityEngine;

public class ObstacleTruck : ObstacleBase
{
    [Header("Truck Config")]
    [SerializeField] private Color truckColor = new Color(0.3f, 0.3f, 0.3f);

    private void Awake()
    {
        speed = 8f;
        obstacleScale = new Vector3(3f, 2f, 5f);
        poolKey = "Truck";
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        SetColor(truckColor);
    }
}