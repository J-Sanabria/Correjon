using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager2D : MonoBehaviour
{
    public enum SpawnKind { Hazard, Coin, Heal, Leaf, Powerup }

    [Serializable]
    public class SpawnRule
    {
        public SpawnKind kind;
        public GameObject prefab;

        [Header("Rango por distancia (m) — al inicio de la carrera")]
        public float spacingMinStart = 8f;   // separación mínima entre spawns (m)
        public float spacingMaxStart = 14f;  // separación máxima entre spawns (m)

        [Header("Rango por distancia (m) — cuando aumenta la dificultad")]
        public float spacingMinEnd = 5f;     // se reduce para spawnear más seguido
        public float spacingMaxEnd = 10f;

        [Header("A qué distancia aplicar la transición (m)")]
        public float rampDistance = 500f;    // a esta distancia ya estamos en valores "End"

        [Header("Posición X relativa")]
        public float offscreenPadding = 2f;  // siempre fuera del borde derecho de la cámara

        [Header("En bloque sólido")]
        public float edgeSafety = 0.4f;      // margen a bordes del bloque
        public float spawnYOffset = 0.6f;    // altura extra sobre el piso

        [Header("Ocupación/solapamiento")]
        public float reserveWidthX = 1.2f;         // ancho reservado para evitar solapes
        public float minSeparationFromOthers = 0.6f;
        public int maxConcurrent = 6;

        // runtime
        [NonSerialized] public float nextSpawnAtX;
        [NonSerialized] public int aliveCount;
    }

    [Header("Refs")]
    public Transform target;      // player o cámara que avanza (usamos su X)
    public GroundSpawner ground;  // contenedor de segmentos con colliders
    public Camera cam;            // si es null se usa Camera.main
    public LivesSystem lives;     // para condicionar Heal

    [Header("Reglas")]
    public List<SpawnRule> rules = new List<SpawnRule>();

    [Header("Limpieza")]
    public float cullBehindDistance = 40f;

    // ---- internos ----
    private struct Booking
    {
        public Transform instance;
        public float minX, maxX;
        public int ruleIndex;  // para descontar aliveCount al liberar
    }

    private readonly List<Booking> bookings = new List<Booking>();
    private float startX; // X inicial para medir distancia recorrida



    void Start()
    {
        if (!cam) cam = Camera.main;
        if (!target || !ground) { enabled = false; return; }

        startX = target.position.x;

        // Inicializa el primer punto de spawn de cada regla
        for (int i = 0; i < rules.Count; i++)
        {
            var r = rules[i];
            float playerX = target.position.x;
            r.nextSpawnAtX = playerX + SampleSpacingMeters(r, playerX);
            r.aliveCount = 0;
            rules[i] = r;
        }

        if (lives == null) lives = LivesSystem.Instance;
    }

    void Update()
    {
        if (!target || rules.Count == 0) return;

        float playerX = target.position.x;

        // Recorremos por índice para poder escribir la regla modificada
        for (int i = 0; i < rules.Count; i++)
        {
            var r = rules[i];
            if (r.prefab == null) continue;
            if (r.aliveCount >= r.maxConcurrent) { rules[i] = r; continue; }

            // Heals sólo si no está a tope
            if (r.kind == SpawnKind.Heal && lives != null && lives.CurrentLives >= lives.MaxLives)
            {
                // Pospón ligeramente el próximo intento para no saturar el bucle
                r.nextSpawnAtX = Math.Max(r.nextSpawnAtX, playerX + 1f);
                rules[i] = r;
                continue;
            }

            // ¿Toca spawnear por distancia?
            if (playerX >= r.nextSpawnAtX)
            {
                TrySpawnByRule(i, ref r, playerX);
                rules[i] = r;
            }
        }

        // limpiar instancias atrás y liberar reservas
        CullAndRelease(playerX - cullBehindDistance);
    }

    // ------ núcleo de spawn ------

    void TrySpawnByRule(int ruleIndex, ref SpawnRule rule, float playerX)
    {
        // 1) punto candidato: al menos en nextSpawnAtX y fuera de cámara
        float desiredX = rule.nextSpawnAtX;
        float camRight = GetCameraRightWorldX();
        desiredX = Mathf.Max(desiredX, camRight + rule.offscreenPadding);

        // 2) encontrar bloque sólido que contenga o venga después
        if (!FindSolidSegmentAtOrAfterX(desiredX, rule.edgeSafety, out Bounds seg))
        {
            // si no hay bloque aún, intenta un poco más adelante en breve
            rule.nextSpawnAtX = desiredX + 1.0f;
            return;
        }

        // 3) rango útil dentro del bloque
        float minX = seg.min.x + rule.edgeSafety;
        float maxX = seg.max.x - rule.edgeSafety;
        if (maxX <= minX)
        {
            rule.nextSpawnAtX = desiredX + 1.0f;
            return;
        }

        // 4) intentar colocar sin solapes (muestreo estratificado para cubrir mejor el rango)
        const int MAX_TRIES = 10;
        float spawnX = desiredX;
        bool placed = false;

        for (int t = 0; t < MAX_TRIES; t++)
        {
            float u = (t + UnityEngine.Random.value) / MAX_TRIES; // 0..1 repartido
            spawnX = Mathf.Lerp(minX, maxX, u);

            if (!OverlapsExisting(spawnX, rule.reserveWidthX, rule.minSeparationFromOthers))
            {
                Vector3 pos = new Vector3(spawnX, seg.max.y + rule.spawnYOffset, 0f);
                var go = Instantiate(rule.prefab, pos, Quaternion.identity, transform);
                rule.aliveCount++;

                float half = Mathf.Max(0.01f, rule.reserveWidthX * 0.5f);
                bookings.Add(new Booking
                {
                    instance = go.transform,
                    minX = spawnX - half - rule.minSeparationFromOthers,
                    maxX = spawnX + half + rule.minSeparationFromOthers,
                    ruleIndex = ruleIndex
                });

                placed = true;
                break;
            }
        }


        // 5) programar el siguiente punto de spawn en metros (densidad escala con distancia)
        float nextSpacing = SampleSpacingMeters(rule, playerX);
        rule.nextSpawnAtX = (placed ? spawnX : desiredX) + nextSpacing;
    }

    // spacing (en metros) escalado con la distancia corrida
    float SampleSpacingMeters(SpawnRule r, float playerX)
    {
        float distance = Mathf.Max(0f, playerX - startX);
        float t = (r.rampDistance > 0f) ? Mathf.Clamp01(distance / r.rampDistance) : 1f;

        float min = Mathf.Lerp(r.spacingMinStart, r.spacingMinEnd, t);
        float max = Mathf.Lerp(r.spacingMaxStart, r.spacingMaxEnd, t);
        if (max < min) max = min;

        return UnityEngine.Random.Range(min, max);
    }

    // ------ utilidades ------

    bool OverlapsExisting(float x, float width, float pad)
    {
        float half = Mathf.Max(0.01f, width * 0.5f);
        float aMin = x - half - pad;
        float aMax = x + half + pad;

        for (int i = bookings.Count - 1; i >= 0; i--)
        {
            var b = bookings[i];
            if (b.instance == null)
            { // objeto destruido (recogido/muerto)
                DecreaseAliveCountForRuleIndex(b.ruleIndex);
                bookings.RemoveAt(i);
                continue;
            }
            bool overlap = !(aMax <= b.minX || aMin >= b.maxX);
            if (overlap) return true;
        }
        return false;
    }

    void CullAndRelease(float cullX)
    {
        for (int i = bookings.Count - 1; i >= 0; i--)
        {
            var bk = bookings[i];

            if (bk.instance == null)
            {
                DecreaseAliveCountForRuleIndex(bk.ruleIndex);
                bookings.RemoveAt(i);
                continue;
            }

            if (bk.instance.position.x < cullX)
            {
                DecreaseAliveCountForRuleIndex(bk.ruleIndex);
                Destroy(bk.instance.gameObject);
                bookings.RemoveAt(i);
            }
        }
    }

    void DecreaseAliveCountForRuleIndex(int idx)
    {
        if (idx < 0 || idx >= rules.Count) return;
        rules[idx].aliveCount = Mathf.Max(0, rules[idx].aliveCount - 1);
    }

    float GetCameraRightWorldX()
    {
        if (!cam) cam = Camera.main;
        if (!cam) return target ? target.position.x : 0f;

        Vector3 right = cam.ViewportToWorldPoint(
            new Vector3(1f, 0.5f, Mathf.Abs(cam.transform.position.z)));
        return right.x;
    }

    // primer bloque sólido que incluya desiredX o venga después
    bool FindSolidSegmentAtOrAfterX(float desiredX, float safety, out Bounds result)
    {
        result = new Bounds();
        float bestStart = float.PositiveInfinity;
        bool foundAhead = false;

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
                if (b.size.x >= safety * 2f) { result = b; return true; }
            }
            else if (ahead)
            {
                if (b.min.x < bestStart && b.size.x >= safety * 2f)
                {
                    bestStart = b.min.x;
                    result = b;
                    foundAhead = true;
                }
            }
        }
        return foundAhead;
    }

    private Dictionary<int, (float, float, float, float)> originalSpacings = new();

    public void BoostSpawnRates(float multiplier)
    {
        if (rules == null || rules.Count == 0) return;
        originalSpacings.Clear();

        float playerX = target ? target.position.x : 0f;
        bool anyAffected = false;

        for (int i = 0; i < rules.Count; i++)
        {
            var r = rules[i];

            // Solo monedas y hojas
            if (r.kind == SpawnKind.Coin || r.kind == SpawnKind.Leaf)
            {
                anyAffected = true;

                // Guardamos valores originales
                originalSpacings[i] = (r.spacingMinStart, r.spacingMaxStart, r.spacingMinEnd, r.spacingMaxEnd);

                // Reducimos espaciado (más frecuencia)
                r.spacingMinStart *= multiplier;
                r.spacingMaxStart *= multiplier;
                r.spacingMinEnd *= multiplier;
                r.spacingMaxEnd *= multiplier;

                // Reprogramar el siguiente spawn para que no quede lejísimos
                float newSpacing = SampleSpacingMeters(r, playerX);
                // Que el próximo spawn no esté más lejos de lo que dice el nuevo rango
                r.nextSpawnAtX = Mathf.Min(r.nextSpawnAtX, playerX + newSpacing);

                rules[i] = r;

                Debug.Log($"[Venado] Regla {i} ({r.kind}) boosteada. Nuevo rango start: {r.spacingMinStart:F1}-{r.spacingMaxStart:F1}");
            }
        }

        if (!anyAffected)
        {
            Debug.LogWarning("[Venado] BoostSpawnRates no encontró ninguna regla Coin/Leaf. Revisa el enum SpawnKind en el inspector.");
        }
        else
        {
            Debug.Log($"[Venado] Activado, multiplicador de aparición {multiplier}");
        }
    }


    public void RestoreSpawnRates()
    {
        foreach (var kv in originalSpacings)
        {
            int i = kv.Key;
            var orig = kv.Value;
            if (i >= 0 && i < rules.Count)
            {
                var r = rules[i];
                r.spacingMinStart = orig.Item1;
                r.spacingMaxStart = orig.Item2;
                r.spacingMinEnd = orig.Item3;
                r.spacingMaxEnd = orig.Item4;
                rules[i] = r;
            }
        }

        originalSpacings.Clear();
        Debug.Log("Venado terminó  tasas de aparición restauradas");
    }

}
