using UnityEngine;

public class ObstacleCar : ObstacleBase
{
    [Header("Car Config")]
    [SerializeField] private Color carColor = Color.blue;

    private void Awake()
    {
        speed = 15f;
        obstacleScale = new Vector3(2f, 1.2f, 3f);
        poolKey = "Car";
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        SetColor(carColor);
    }
}