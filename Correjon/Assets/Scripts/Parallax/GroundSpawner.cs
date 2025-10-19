using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GroundSpawner : MonoBehaviour
{
    [Header("Configuración general")]
    public Transform target;                  // Jugador o cámara
    public GameObject normalGroundPrefab;     // Bloque de suelo sólido
    public GameObject emptyGroundPrefab;      // Bloque vacío (sin colisión)
    public int poolSize = 8;                  // Cuántos módulos mantener activos
    public float spawnOffset = 10f;           // Cuánto más allá de la cámara se generan nuevos módulos

    [Header("Control del hueco")]
    public int minNormalBeforeHole = 5;       // Mínimo de bloques normales antes del hueco
    public int maxNormalBeforeHole = 8;       // Máximo de bloques normales antes del hueco

    private GameObject[] pool;
    private float segmentLength;
    private int nextIndex;
    private float nextSpawnX;

    private bool holeSpawned = false;         // Controla si ya se generó el vacío
    private int blocksUntilHole;              // Contador de bloques normales antes del hueco

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("GroundSpawner: Falta asignar el target (jugador o cámara).");
            enabled = false;
            return;
        }

        SpriteRenderer sr = normalGroundPrefab.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("GroundSpawner: El prefab base no tiene SpriteRenderer.");
            return;
        }

        segmentLength = sr.sprite.bounds.size.x * normalGroundPrefab.transform.localScale.x;

        pool = new GameObject[poolSize];
        float startX = transform.position.x;

        // Configurar cuándo aparecerá el hueco
        blocksUntilHole = Random.Range(minNormalBeforeHole, maxNormalBeforeHole + 1);

        // Crear los bloques iniciales
        for (int i = 0; i < poolSize; i++)
        {
            GameObject prefab = GetNextPrefab();
            Vector3 spawnPos = new Vector3(startX + (segmentLength * i), transform.position.y, 0);
            pool[i] = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
        }

        nextSpawnX = startX + (segmentLength * poolSize);
    }

    void Update()
    {
        if (target == null) return;

        GameObject first = pool[nextIndex];

        // Reposicionar el bloque más viejo cuando el jugador avanza
        if (target.position.x - first.transform.position.x > segmentLength + spawnOffset)
        {
            first.transform.position = new Vector3(nextSpawnX, transform.position.y, 0);
            ReplaceModule(first);

            nextSpawnX += segmentLength;
            nextIndex = (nextIndex + 1) % poolSize;
        }
    }

    // --- Generador de un solo hueco ---
    GameObject GetNextPrefab()
    {
        // Si ya generamos el hueco, todo será normal
        if (holeSpawned)
            return normalGroundPrefab;

        // Aún no se generó el hueco
        if (blocksUntilHole > 0)
        {
            blocksUntilHole--;
            return normalGroundPrefab;
        }

        // Generar el único bloque vacío y marcarlo como ya hecho
        holeSpawned = true;
        return emptyGroundPrefab;
    }

    void ReplaceModule(GameObject obj)
    {
        if (obj == null) return;

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        GameObject prefab = GetNextPrefab();

        if (prefab != null && sr != null)
        {
            SpriteRenderer newSR = prefab.GetComponent<SpriteRenderer>();
            if (newSR != null)
                sr.sprite = newSR.sprite;

            // Activar o desactivar colisión según tipo
            Collider2D col = obj.GetComponent<Collider2D>();
            Collider2D prefabCol = prefab.GetComponent<Collider2D>();
            if (col != null && prefabCol != null)
                col.enabled = prefabCol.enabled;
        }
    }
}
