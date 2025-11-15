using UnityEngine;
using System.Collections;

public enum PowerupType
{
    None,
    Danta,
    Venado,
    Condor
}

public class PowerupSystem : MonoBehaviour
{
    public static PowerupSystem Instance { get; private set; }

    [Header("Referencias")]
    public PlayerController player;   // referencia al jugador
    public SpawnManager2D spawner;    // controla spawn de objetos
    public LivesSystem lives;         // para activar Danta

    [Header("Duraciones de poderes")]
    [Tooltip("Duración del efecto del Venado (segundos)")]
    public float venadoDuration = 10f;
    [Tooltip("Duración del vuelo del Cóndor (segundos)")]
    public float condorDuration = 10f;

    [Header("Venado - parámetros")]
    [Tooltip("Multiplicador para reducir espaciado entre spawns (0.5 = doble frecuencia)")]
    public float spawnBoostMultiplier = 0.8f;

    [Header("Cóndor - parámetros")]
    [Tooltip("Altura adicional de vuelo sobre el suelo")]
    public float flightHeight = 3f;

    public float VarFlapForce = 1.2f;
    [Tooltip("Radio del efecto magnético para recoger objetos")]
    public float magnetRadius = 4f;
    [Tooltip("Velocidad de atracción de pickups")]
    public float magnetSpeed = 8f;

    [Header("UI de Poderes")]
    public GameObject powerupPanel;
    public TMPro.TextMeshProUGUI powerupText;
    public float panelDisplayTime = 3f;
    public float pauseOnPowerupSeconds = 1.8f; // tiempo que se pausa el juego al mostrar mensaje

    private CanvasGroup powerupCanvasGroup;
    public float fadeDuration = 0.5f;

    [Header("Icono del poder")]
    public Animator animalAnimator;
    public Animator fondoAnimator;


    private PowerupType activePower = PowerupType.None;
    private Coroutine currentRoutine;
    private Coroutine panelRoutine; // <- NUEVO


    // --------------------------------------------------------------
    // Inicialización
    // --------------------------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (!lives) lives = LivesSystem.Instance;
        if (!spawner) spawner = FindAnyObjectByType<SpawnManager2D>();
        if (!player) player = FindAnyObjectByType<PlayerController>();


        if (powerupPanel)
            powerupCanvasGroup = powerupPanel.GetComponent<CanvasGroup>();
    }

    // --------------------------------------------------------------
    // Activación principal
    // --------------------------------------------------------------
    public void ActivatePower(PowerupType type)
    {
        // Si el mismo poder está activo, ignora
        if (activePower == type) return;

        // Si hay otro poder corriendo, lo cancela
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        activePower = type;

        switch (type)
        {
            case PowerupType.Danta:
                ActivateDanta();
                break;

            case PowerupType.Venado:
                currentRoutine = StartCoroutine(VenadoRoutine());
                break;

            case PowerupType.Condor:
                currentRoutine = StartCoroutine(CondorRoutine());
                break;
        }
    }

    // --------------------------------------------------------------
    //  DANTA — Protección de 3 impactos (manejada por LivesSystem)
    // --------------------------------------------------------------
    void ActivateDanta()
    {

        if (lives)
            lives.ActivateDantaProtection();

        StartCoroutine(ResetPowerAfterDelay(5f)); // 5 segundos o cuando termine la invulnerabilidad

        ShowPowerPanel(PowerupType.Danta);


        Debug.Log("Poder de la Danta activado: protección de golpes y powerups pausados.");
    }

    // --------------------------------------------------------------
    //  VENADO — Más objetos por tiempo limitado
    // --------------------------------------------------------------
    IEnumerator VenadoRoutine()
    {
        // Si hay otra corrutina corriendo, la detiene de forma segura (sin romper panel ni otras)
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        activePower = PowerupType.Venado;

        ShowPowerPanel(PowerupType.Venado);
        Debug.Log("Poder del Venado activado.");

        if (spawner != null)
            spawner.BoostSpawnRates(spawnBoostMultiplier); // más evidente

        float timer = 0f;

        // Espera manual que respeta pausas y no se ve interrumpida fácilmente
        while (timer < venadoDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // Restaurar condiciones normales
        if (spawner != null)
            spawner.RestoreSpawnRates();

        activePower = PowerupType.None;

        Debug.Log("Poder del Venado terminado y restaurado.");
    }


    // --------------------------------------------------------------
    //  CÓNDOR — Vuelo y magnetismo
    // --------------------------------------------------------------
    IEnumerator CondorRoutine()
    {
        activePower = PowerupType.Condor;
        ShowPowerPanel(PowerupType.Condor);
        Debug.Log("Poder del Condor activado.");

        // Desactiva el control normal del jugador
        player.SetControlEnabled(false);

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogWarning("CondorRoutine: el jugador no tiene Rigidbody2D.");
            yield break;
        }

        // Guardar parámetros originales
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 1.3f; // un poco más de peso para que no planee infinito
        rb.linearVelocity = Vector2.zero;

        // Altura base y límite máximo real
        float baseHeight = player.transform.position.y;
        float maxHeight = baseHeight + flightHeight; // flightHeight = 2 o 3 suele ir bien

        // Fuerza de aleteo (puedes bajar VarFlapForce a 0.7–0.9 si sigue muy alto)
        float flapForce = player.jumpForce * VarFlapForce;

        float timer = 0f;
        while (timer < condorDuration)
        {
            timer += Time.deltaTime;

            // Input de "aletear": un toque / clic rápido
            bool flapPressed =
                Input.GetMouseButtonDown(0) ||
                (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);

            if (flapPressed)
            {
                // Solo permitir nuevo impulso si aún no estamos pegados al límite
                if (player.transform.position.y <= maxHeight - 0.1f)
                {
                    // reset vertical antes del impulso
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                    rb.AddForce(Vector2.up * flapForce, ForceMode2D.Impulse);
                }
            }

            // Mantener avance horizontal igual que el PlayerController
            if (player.runDistance != null)
            {
                float currentSpeed = player.runDistance.GetSpeed();
                rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
            }
            else
            {
                float newX = Mathf.Min(
                    rb.linearVelocity.x + player.acceleration * Time.deltaTime,
                    player.maxSpeed
                );
                rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
            }

            // Clamp de altura máxima real
            Vector3 pos = player.transform.position;
            if (pos.y > maxHeight)
            {
                pos.y = maxHeight;
                player.transform.position = pos;

                // si venía subiendo muy fuerte, corta la velocidad hacia arriba
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Min(rb.linearVelocity.y, 0f));
            }

            // Magnetismo: recoger pickups cercanos
            var pickups = Physics2D.OverlapCircleAll(player.transform.position, magnetRadius);
            foreach (var p in pickups)
            {
                if (p.CompareTag("Pickup"))
                {
                    p.transform.position = Vector3.MoveTowards(
                        p.transform.position,
                        player.transform.position,
                        magnetSpeed * Time.deltaTime
                    );
                }
            }

            yield return null;
        }

        // Restaurar físicas y control normal
        rb.gravityScale = originalGravity;
        player.SetControlEnabled(true);

        activePower = PowerupType.None;

        Debug.Log("Poder del Condor terminado, modo normal restaurado.");
    }

    void ShowPowerPanel(PowerupType type)
    {
        if (!powerupPanel || !powerupText) return;

        string message = type switch
        {
            PowerupType.Danta => "Poder de la Danta: Invulnerable a 3 golpes.",
            PowerupType.Venado => "Poder del Venado: Aumenta la aparición de hojas y monedas.",
            PowerupType.Condor => "Poder del Cóndor: ¡Vuela y esquiva los obstáculos!",
            _ => ""
        };

        powerupText.text = message;
        powerupPanel.SetActive(true);

        if (powerupCanvasGroup)
            powerupCanvasGroup.alpha = 0f; // empieza invisible

        if (animalAnimator)
        {
            animalAnimator.ResetTrigger("ShowDanta");
            animalAnimator.ResetTrigger("ShowVenado");
            animalAnimator.ResetTrigger("ShowCondor");

            switch (type)
            {
                case PowerupType.Danta: animalAnimator.SetTrigger("ShowDanta"); break;
                case PowerupType.Venado: animalAnimator.SetTrigger("ShowVenado"); break;
                case PowerupType.Condor: animalAnimator.SetTrigger("ShowCondor"); break;
            }
        }


        if (fondoAnimator)
        {
            fondoAnimator.ResetTrigger("ShowFondoDanta");
            fondoAnimator.ResetTrigger("ShowFondoVenado");
            fondoAnimator.ResetTrigger("ShowFondoCondor");

            switch (type)
            {
                case PowerupType.Danta: fondoAnimator.SetTrigger("ShowFondoDanta"); break;
                case PowerupType.Venado: fondoAnimator.SetTrigger("ShowFondoVenado"); break;
                case PowerupType.Condor: fondoAnimator.SetTrigger("ShowFondoCondor"); break;
            }

         }

        if (pauseOnPowerupSeconds > 0f)
            StartCoroutine(SoftPauseRoutine());

        if (panelRoutine != null)
            StopCoroutine(panelRoutine);
        panelRoutine = StartCoroutine(PanelFadeRoutine());


    }
    IEnumerator PanelFadeRoutine()
    {
        if (powerupCanvasGroup == null)
            yield break;

        // Fade IN
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(t / fadeDuration);
            powerupCanvasGroup.alpha = Mathf.Lerp(0f, 1f, normalized);
            yield return null;
        }
        powerupCanvasGroup.alpha = 1f;

        // Permanecer visible panelDisplayTime (tiempo real)
        float hold = 0f;
        while (hold < panelDisplayTime)
        {
            hold += Time.unscaledDeltaTime;
            yield return null;
        }

        // Fade OUT
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(t / fadeDuration);
            powerupCanvasGroup.alpha = Mathf.Lerp(1f, 0f, normalized);
            yield return null;
        }
        powerupCanvasGroup.alpha = 0f;

        if (powerupPanel)
            powerupPanel.SetActive(false);

        panelRoutine = null;
    }

    // >>> Pausa "suave" usando tiempo NO escalado
    IEnumerator SoftPauseRoutine()
    {
        float previousScale = Time.timeScale;
        Time.timeScale = 0f;

        float t = 0f;
        while (t < pauseOnPowerupSeconds)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // Solo reanuda si nadie más cambió el timeScale mientras tanto
        if (Mathf.Approximately(Time.timeScale, 0f))
            Time.timeScale = previousScale;
    }


    IEnumerator HidePanelAfterDelay()
    {
        yield return new WaitForSeconds(panelDisplayTime);

        if (powerupCanvasGroup)
        {
            float t = 0f;
            float startAlpha = powerupCanvasGroup.alpha;

            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float normalized = t / fadeDuration;
                powerupCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, normalized);
                yield return null;
            }

            powerupCanvasGroup.alpha = 0f;
        }

        if (powerupPanel)
            powerupPanel.SetActive(false);
    }
    IEnumerator ResetPowerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        activePower = PowerupType.None;
    }
    public bool IsActive(PowerupType type) => activePower == type;
}
