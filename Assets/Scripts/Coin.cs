using UnityEngine;
using UnityEngine.SceneManagement;

public class Coin : MonoBehaviour
{
    [SerializeField] private Color coinColor = Color.yellow;
    [SerializeField] private float rotationSpeed = 90f;

    private void OnEnable()
    {
        var r = GetComponentInChildren<Renderer>();
        if (r != null) r.material.color = coinColor;
    }

    private void Update()
    {
        // Spin the coin for visual appeal
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Move toward -Z at the player's speed so it appears stationary
        if (PlayerController.Instance != null)
        {
            transform.position += Vector3.back * PlayerController.Instance.currentSpeed * Time.deltaTime;

            // Despawn check
            float playerZ = PlayerController.Instance.transform.position.z;
            if (transform.position.z < playerZ - 25f)
            {
                ObjectPool.Instance.ReturnToPool("Coin", gameObject);
            }
        }
    }

    public void Collect()
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.ActivateMultiplier();
        if (SoundManager.Instance != null) SoundManager.Instance.PlayCoinCollect();
        ObjectPool.Instance.ReturnToPool("Coin", gameObject);
    }
}