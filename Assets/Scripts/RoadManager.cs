using UnityEngine;

public class RoadManager : MonoBehaviour
{
    [SerializeField] private int segmentCount = 5;
    [SerializeField] private float segmentLength = 40f;
    [SerializeField] private GameObject roadSegmentPrefab;

    [Header("Reciclado")]
    [Tooltip("Cuántas unidades DETRÁS del jugador antes de reciclar el segmento. Aumentá si sigue desapareciendo por atrás.")]
    [SerializeField] private float despawnBehind = 60f;

    [Tooltip("Cuántas unidades ADELANTE del jugador se mantiene el camino generado.")]
    [SerializeField] private float spawnAhead = 80f;

    private GameObject[] segments;
    private float[] segmentZPositions;

    private void Start()
    {
        if (roadSegmentPrefab == null)
            roadSegmentPrefab = CreateDefaultRoadPrefab();

        segments = new GameObject[segmentCount];
        segmentZPositions = new float[segmentCount];

        float startZ = 0f;
        if (PlayerController.Instance != null)
            startZ = PlayerController.Instance.transform.position.z - segmentLength;

        for (int i = 0; i < segmentCount; i++)
        {
            segments[i] = Instantiate(roadSegmentPrefab, transform);
            segmentZPositions[i] = startZ + i * segmentLength;
            UpdateSegmentPosition(i);
        }
    }

    private void Update()
    {
        if (PlayerController.Instance == null) return;
        float playerZ = PlayerController.Instance.transform.position.z;

        for (int i = 0; i < segmentCount; i++)
        {
            float segEnd = segmentZPositions[i] + segmentLength;

            // Reciclar solo cuando el segmento entero quedó bien atrás del jugador
            if (segEnd < playerZ - despawnBehind)
            {
                // Moverlo adelante del segmento más lejano
                float maxZ = float.MinValue;
                for (int j = 0; j < segmentCount; j++)
                    if (segmentZPositions[j] > maxZ) maxZ = segmentZPositions[j];

                segmentZPositions[i] = maxZ + segmentLength;
                UpdateSegmentPosition(i);
            }
        }
    }

    private void UpdateSegmentPosition(int index)
    {
        segments[index].transform.position = new Vector3(0f, 0f, segmentZPositions[index]);
    }

    private GameObject CreateDefaultRoadPrefab()
    {
        GameObject road = new GameObject("RoadSegment");

        GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
        surface.transform.SetParent(road.transform);
        surface.transform.localPosition = new Vector3(0f, -0.05f, segmentLength / 2f);
        surface.transform.localScale = new Vector3(12f, 0.1f, segmentLength);
        var surfaceRenderer = surface.GetComponent<Renderer>();
        surfaceRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        surfaceRenderer.material.color = new Color(0.2f, 0.2f, 0.2f);
        Destroy(surface.GetComponent<Collider>());

        float[] laneLineXPositions = { -3f, 0f, 3f };
        foreach (float lx in laneLineXPositions)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.transform.SetParent(road.transform);
            line.transform.localPosition = new Vector3(lx, 0f, segmentLength / 2f);
            line.transform.localScale = new Vector3(0.1f, 0.01f, segmentLength);
            var lr = line.GetComponent<Renderer>();
            lr.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            lr.material.color = Color.white;
            Destroy(line.GetComponent<Collider>());
        }

        float[] barrierX = { -6.5f, 6.5f };
        foreach (float bx in barrierX)
        {
            GameObject barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrier.transform.SetParent(road.transform);
            barrier.transform.localPosition = new Vector3(bx, 0.5f, segmentLength / 2f);
            barrier.transform.localScale = new Vector3(0.5f, 1f, segmentLength);
            var br = barrier.GetComponent<Renderer>();
            br.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            br.material.color = new Color(0.8f, 0.8f, 0f);
            Destroy(barrier.GetComponent<Collider>());
        }

        return road;
    }
}