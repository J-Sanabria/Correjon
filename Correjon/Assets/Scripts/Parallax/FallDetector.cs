using System.Collections;
using UnityEngine;

public class FallDetector : MonoBehaviour
{
    [Header("Refs")]
    public PlayerController player;
    public LivesSystem livesSystem;
    public GroundSpawner groundSpawner;

    [Header("Config")]
    public float fallThreshold = -5f;   // Y a partir de la cual consideramos que cayo
    public float respawnYOffset = 1.2f; // reaparece un poco sobre el suelo
    public float searchAheadPadding = 0.5f; // busca piso hacia adelante

    [Header("FX")]
    public float fadeOutTime = 0.2f;
    public float fadeInTime = 0.2f;
    public float invulnTime = 0.8f;   // titileo

    private bool isRespawning = false;
    private SpriteRenderer[] rends;
    private Vector3 cachedScale;

    void Awake()
    {
        if (player != null)
            rends = player.GetComponentsInChildren<SpriteRenderer>(true);
    }

    void Update()
    {
        if (player == null || livesSystem == null || groundSpawner == null) return;

        if (!isRespawning && player.transform.position.y < fallThreshold)
            StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        // cachear escala original por si acaso
        cachedScale = player.transform.localScale;

        // quitar vida y pausar control
        livesSystem.LoseLife();
        player.SetControlEnabled(false);

        // fade out
        yield return StartCoroutine(FadeTo(0f, fadeOutTime));

        // buscar piso solido hacia adelante y recolocar
        Vector3 safe = GetNextSafePosition(player.transform.position.x + searchAheadPadding);
        player.transform.position = safe + Vector3.up * respawnYOffset;
        player.transform.rotation = Quaternion.identity;
        player.transform.localScale = cachedScale; // asegurar que no cambie de tamano
        player.ResetVerticalVelocity();

        // fade in
        yield return StartCoroutine(FadeTo(1f, fadeInTime));

        // titileo de invulnerabilidad visual
        yield return StartCoroutine(Blink(invulnTime));

        player.SetControlEnabled(true);
        isRespawning = false;
    }

    Vector3 GetNextSafePosition(float fromX)
    {
        float bestX = float.PositiveInfinity;
        float y = transform.position.y;

        foreach (Transform child in groundSpawner.transform)
        {
            var col = child.GetComponent<Collider2D>();
            if (col != null && col.enabled && child.position.x > fromX && child.position.x < bestX)
            {
                bestX = child.position.x;
                y = child.position.y;
            }
        }

        if (float.IsPositiveInfinity(bestX))
            bestX = fromX + 3f; // fallback prudente

        return new Vector3(bestX, y, 0f);
    }

    IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (rends == null || rends.Length == 0) yield break;

        // leer alpha inicial del primer renderer
        float startA = rends[0].color.a;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = duration > 0f ? Mathf.Clamp01(t / duration) : 1f;
            float a = Mathf.Lerp(startA, targetAlpha, k);
            SetAlpha(a);
            yield return null;
        }
        SetAlpha(targetAlpha);
    }

    IEnumerator Blink(float totalTime)
    {
        if (rends == null || rends.Length == 0) yield break;

        float elapsed = 0f;
        float period = 0.1f;
        bool on = true;

        while (elapsed < totalTime)
        {
            elapsed += period;
            on = !on;
            SetAlpha(on ? 1f : 0.35f);
            yield return new WaitForSeconds(period);
        }
        SetAlpha(1f);
    }

    void SetAlpha(float a)
    {
        if (rends == null) return;
        for (int i = 0; i < rends.Length; i++)
        {
            var c = rends[i].color;
            c.a = a;
            rends[i].color = c;
        }
    }

    // ayuda visual opcional en la escena
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        float x = (player != null) ? player.transform.position.x : transform.position.x;
        Vector3 p1 = new Vector3(x - 20f, fallThreshold, 0f);
        Vector3 p2 = new Vector3(x + 20f, fallThreshold, 0f);
        Gizmos.DrawLine(p1, p2);
    }
}
