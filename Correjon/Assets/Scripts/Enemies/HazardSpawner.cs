using UnityEngine;

public class HazardSpawner : MonoBehaviour
{
    [Header("Refs")]
    public Transform target;
    public GameObject hazardPrefab;
    public GroundSpawner ground;
    public Camera cam; // si es null usa Camera.main

    [Header("Spawn")]
    public float minInterval = 2.5f;
    public float maxInterval = 4.5f;

    // distancia minima delante del jugador (ademas de salir fuera de camara)
    public float minAheadFromPlayer = 10f;
    public float maxAheadFromPlayer = 18f;

    // debe spawnear fuera del borde derecho de camara (para no "aparecer")
    public float offscreenPadding = 2f;

    // seguridad respecto a los bordes del bloque solido
    public float edgeSafety = 0.4f;

    // altura extra sobre el piso
    public float spawnYOffset = 0.5f;

    [Header("Limpiar atras")]
    public float cullBehindDistance = 40f;

    private float nextSpawnTime;

    void Start()
    {
        if (!cam) cam = Camera.main;
        ScheduleNext();
    }

    void Update()
    {
        if (target == null || hazardPrefab == null || ground == null) return;

        if (Time.time >= nextSpawnTime)
        {
            TrySpawnSafe();
            ScheduleNext();
        }

        // limpieza atras
        foreach (Transform child in transform)
        {
            if (child.position.x < target.position.x - cullBehindDistance)
                Destroy(child.gameObject);
        }
    }

    void TrySpawnSafe()
    {
        // punto de partida: delante del jugador
        float desiredX = target.position.x + Random.Range(minAheadFromPlayer, maxAheadFromPlayer);

        // tambien debe estar fuera de camara a la derecha
        float camRight = GetCameraRightWorldX();
        desiredX = Mathf.Max(desiredX, camRight + offscreenPadding);

        // buscar un bloque solido que empiece en o despues de desiredX y que sea suficientemente ancho
        if (!FindSolidSegmentAtOrAfterX(desiredX, edgeSafety, out Bounds seg))
            return; // no hay segmento adecuado -> no spawnea

        // elegir X dentro del bloque, lejos de bordes
        float minX = seg.min.x + edgeSafety;
        float maxX = seg.max.x - edgeSafety;

        // si el desiredX cae fuera de ese rango, clampear
        float spawnX = Mathf.Clamp(desiredX, minX, maxX);

        // si aun asi el bloque quedo demasiado fino, abortar
        if (maxX <= minX) return;

        Vector3 pos = new Vector3(spawnX, seg.max.y + spawnYOffset, 0f);
        var go = Instantiate(hazardPrefab, pos, Quaternion.identity, transform);

        var hz = go.GetComponent<Hazard>();
        if (hz != null) hz.Init(target);
    }

    float GetCameraRightWorldX()
    {
        if (!cam) cam = Camera.main;
        if (!cam) return target ? target.position.x : 0f;

        // convertir viewport (1,0.5) a mundo
        Vector3 right = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, Mathf.Abs(cam.transform.position.z)));
        return right.x;
    }

    // busca el primer bloque solido cuyo bounds contenga desiredX o venga despues,
    // y que tenga ancho util mayor que 2*edgeSafety
    bool FindSolidSegmentAtOrAfterX(float desiredX, float safety, out Bounds result)
    {
        result = new Bounds();
        float bestStart = float.PositiveInfinity;
        bool found = false;

        foreach (Transform child in ground.transform)
        {
            var col = child.GetComponent<Collider2D>();
            if (col == null || !col.enabled) continue; // ignorar vacios

            Bounds b = col.bounds;

            // si el segmento ya quedo atras, ignorar
            if (b.max.x < desiredX) continue;

            // preferir el primer bloque cuyo rango [min, max] incluya desiredX;
            // si no, tomar el bloque mas cercano por delante
            bool contains = desiredX >= b.min.x && desiredX <= b.max.x;
            bool ahead = b.min.x >= desiredX;

            if (contains)
            {
                // validar ancho util
                if (b.size.x >= safety * 2f)
                {
                    result = b;
                    return true; // mejor caso
                }
            }
            else if (ahead)
            {
                // candidato por delante; elegir el mas cercano
                if (b.min.x < bestStart && b.size.x >= safety * 2f)
                {
                    bestStart = b.min.x;
                    result = b;
                    found = true;
                }
            }
        }

        return found;
    }

    void ScheduleNext()
    {
        nextSpawnTime = Time.time + Random.Range(minInterval, maxInterval);
    }
}
