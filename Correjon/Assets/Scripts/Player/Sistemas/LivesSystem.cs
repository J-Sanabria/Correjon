// LivesSystem.cs
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;

public class LivesSystem : MonoBehaviour
{
    public static LivesSystem Instance { get; private set; }

    [Header("Config")]
    [SerializeField] int maxLives = 3;
    [SerializeField] int startLives = 3;
    [SerializeField] bool persistAcrossScenes = true;
    [SerializeField] float invulnerabilitySeconds = 1.0f;

    [Header("UI (opcional directo)")]
    [SerializeField] GameOverPanel gameOverPanel; // <-- arrástralo en el Inspector

    public int MaxLives => maxLives;
    public int CurrentLives { get; private set; }
    public bool IsInvulnerable { get; private set; }

    public UnityEvent<int, int> OnLivesChanged = new UnityEvent<int, int>();
    public UnityEvent<bool> OnInvulnerabilityChanged = new UnityEvent<bool>();

    public event Action OnGameOver;

    private bool gameOverFired = false;
    public bool HasGameOver => gameOverFired; // <-- “sticky” para late subscribers

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (persistAcrossScenes) DontDestroyOnLoad(gameObject);

        CurrentLives = Mathf.Clamp(startLives, 0, maxLives);
        gameOverFired = (CurrentLives <= 0);
        OnLivesChanged.Invoke(CurrentLives, MaxLives);
    }

    public void AddLife(int amount = 1)
    {
        if (amount <= 0 || gameOverFired) return;
        CurrentLives = Mathf.Clamp(CurrentLives + amount, 0, MaxLives);
        OnLivesChanged.Invoke(CurrentLives, MaxLives);
    }

    public void LoseLife(int amount = 1)
    {
        if (amount <= 0 || gameOverFired) return;
        if (IsInvulnerable) return;

        CurrentLives = Mathf.Max(0, CurrentLives - amount);
        OnLivesChanged.Invoke(CurrentLives, MaxLives);

        if (CurrentLives <= 0)
        {
            // Marca primero para evitar reentradas
            gameOverFired = true;

            // 1) Llamada directa al panel (si está asignado)
            if (gameOverPanel != null)
                gameOverPanel.Show();

            // 2) Y si alguien sigue el evento por código, también lo notificamos
            OnGameOver?.Invoke();
            return;
        }

        if (invulnerabilitySeconds > 0f)
            StartCoroutine(InvulnerabilityRoutine(invulnerabilitySeconds));
    }

    public void ResetLives()
    {
        CurrentLives = Mathf.Clamp(startLives, 0, MaxLives);
        gameOverFired = (CurrentLives <= 0);
        IsInvulnerable = false;
        OnInvulnerabilityChanged.Invoke(false);
        OnLivesChanged.Invoke(CurrentLives, MaxLives);
    }

    IEnumerator InvulnerabilityRoutine(float seconds)
    {
        IsInvulnerable = true;
        OnInvulnerabilityChanged.Invoke(true);
        float t = 0f;
        while (t < seconds) { t += Time.unscaledDeltaTime; yield return null; }
        IsInvulnerable = false;
        OnInvulnerabilityChanged.Invoke(false);
    }
}
