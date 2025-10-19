using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("Refs")]
    public Transform target;        // jugador o cámara
    public GameObject coinPrefab;   // prefab con CoinPickup
    public GroundSpawner ground;
    public Camera cam;              // si es null usa Camera.main

    [Header("Spawn")]
    public float minInterval = 1.5f;
    public float maxInterval = 3.0f;
    public float minAheadFromPlayer = 8f;
    public float maxAheadFromPlayer = 16f;
    public float offscreenPadding = 2.0f;
    public float edgeSafety = 0.35f;
    public float spawnYOffset = 0.8f;

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
        if (!target || !coinPrefab || !ground) return;

        if (Time.time >= nextSpawnTime)
        {
            TrySpawnSafe();
            ScheduleNext();
        }

        foreach (Transform child in transform)
        {
            if (child.position.x < target.position.x - cullBehindDistance)
                Destroy(child.gameObject);
        }
    }
    void TrySpawnSafe()
    {
        float desiredX = target.position.x + Random.Range(minAheadFromPlayer, maxAheadFromPlayer);
        float camRight = GetCameraRightWorldX();
        desiredX = Mathf.Max(desiredX, camRight + offscreenPadding);

        if (!FindSolidSegmentAtOrAfterX(desiredX, edgeSafety, out Bounds seg))
            return;

        float minX = seg.min.x + edgeSafety;
        float maxX = seg.max.x - edgeSafety;
        if (maxX <= minX) return;

        float spawnX = Random.Range(minX, maxX);
        Vector3 pos = new Vector3(spawnX, seg.max.y + spawnYOffset, 0f);

        var go = Instantiate(coinPrefab, pos, Quaternion.identity, transform);

    }

    float GetCameraRightWorldX()
    {
        if (!cam) cam = Camera.main;
        if (!cam) return target ? target.position.x : 0f;
        Vector3 right = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, Mathf.Abs(cam.transform.position.z)));
        return right.x;
    }

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
