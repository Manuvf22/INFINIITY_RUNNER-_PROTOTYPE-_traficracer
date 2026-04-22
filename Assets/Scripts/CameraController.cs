using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float offsetY = 4f;
    [SerializeField] private float offsetZ = -8f;
    [SerializeField] private float followSmoothSpeed = 10f;
    [SerializeField] private Vector3 cameraRotation = new Vector3(15f, 0f, 0f);

    private void Start()
    {
        if (player == null && PlayerController.Instance != null)
            player = PlayerController.Instance.transform;

        transform.rotation = Quaternion.Euler(cameraRotation);
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            if (PlayerController.Instance != null)
                player = PlayerController.Instance.transform;
            return;
        }

        Vector3 targetPosition = new Vector3(
            0f,
            player.position.y + offsetY,
            player.position.z + offsetZ
        );

        transform.position = Vector3.Lerp(transform.position, targetPosition, followSmoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(cameraRotation);
    }
}