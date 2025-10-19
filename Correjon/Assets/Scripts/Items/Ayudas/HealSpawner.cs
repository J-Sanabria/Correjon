using UnityEngine;

public class HealSpawner : MonoBehaviour
{
    [Header("Refs")]
    public Transform target;            // jugador o camara
    public GameObject healPrefab;       // prefab con isTrigger + HealPickup
    public GroundSpawner ground;        // para leer segmentos solidos
    public Camera cam;                  // si es null usa Camera.main
    public LivesSystem lives;           // para evitar spawn si ya esta al maximo

    [Header("Spawn")]
    public float minInterval = 6f;
    public float maxInterval = 10f;

    // distancia minima delante del jugador (y fuera de camara)
    public float minAheadFromPlayer = 12f;
    public float maxAheadFromPlayer = 20f;

    public float offscreenPadding = 2.5f;  // mas alla del borde derecho de camara
    public float edgeSafety = 0.5f;        // margen lejos de bordes del bloque

    public float spawnYOffset = 0.75f;     // un poco sobre el piso

    [Header("Opciones")]
    public bool skipIfAtMaxLives = true;   // no spawnear si vidas al maximo

    [Header("Cleanup")]
    public float cullBehindDistance = 40f;

    private float nextSpawnTime;

    void Start()
    {
        if (!cam) cam = Camera.main;
        ScheduleNext();
    }

    void Update()
    {
        if (!target || !healPrefab || !ground) return;

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
        // opcional: no generar si ya esta al maximo
        if (skipIfAtMaxLives && lives != null && lives.CurrentLives >= lives.MaxLives)
            return;

        float desiredX = target.position.x + Random.Range(minAheadFromPlayer, maxAheadFromPlayer);

        // forzar fuera de la camara a la derecha
        float camRight = GetCameraRightWorldX();
        desiredX = Mathf.Max(desiredX, camRight + offscreenPadding);

        // buscar segmento solido adecuado
        if (!FindSolidSegmentAtOrAfterX(desiredX, edgeSafety, out Bounds seg))
            return;

        float minX = seg.min.x + edgeSafety;
        float maxX = seg.max.x - edgeSafety;
        if (maxX <= minX) return;

        // elige una X aleatoria segura dentro del bloque
        float spawnX = Random.Range(minX, maxX);

        Vector3 pos = new Vector3(spawnX, seg.max.y + spawnYOffset, 0f);
        var go = Instantiate(healPrefab, pos, Quaternion.identity, transform);

   
    }

    float GetCameraRightWorldX()
    {
        if (!cam) cam = Camera.main;
        if (!cam) return target ? target.position.x : 0f;

        Vector3 right = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, Mathf.Abs(cam.transform.position.z)));
        return right.x;
    }

    // igual que en tu HazardSpawner: primer bloque solido que incluya desiredX o venga despues
    bool FindSolidSegmentAtOrAfterX(float desiredX, float safety, out Bounds result)
    {
        result = new Bounds();
        float bestStart = float.PositiveInfinity;
        bool found = false;

        foreach (Transform child in ground.transform)
        {
            var col = child.GetComponent<Collider2D>();
            if (col == null || !col.enabled) continue;

            Bounds b = col.bounds;

            if (b.max.x < desiredX) continue;

            bool contains = desiredX >= b.min.x && desiredX <= b.max.x;
            bool ahead = b.min.x >= desiredX;

            if (contains)
            {
                if (b.size.x >= safety * 2f)
                {
                    result = b;
                    return true;
                }
            }
            else if (ahead)
            {
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
