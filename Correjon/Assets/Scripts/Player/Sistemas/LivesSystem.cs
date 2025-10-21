using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
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

    public int MaxLives => maxLives;
    public int CurrentLives { get; private set; }
    public bool IsInvulnerable { get; private set; }

    public UnityEvent<int,int> OnLivesChanged = new UnityEvent<int,int>();
    public UnityEvent<bool> OnInvulnerabilityChanged = new UnityEvent<bool>();
    public event Action OnGameOver;

    bool gameOverFired;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (persistAcrossScenes) DontDestroyOnLoad(gameObject);

        CurrentLives = Mathf.Clamp(startLives, 0, maxLives);
        gameOverFired = (CurrentLives <= 0);
        OnLivesChanged.Invoke(CurrentLives, MaxLives);
    }

    void OnEnable()  { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    // Cada vez que entras a la escena de juego, resetea estado de carrera
    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // si quieres filtrar por escena: if (s.name != "GameScene") return;
        ResetLives();
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
            if (!gameOverFired)
            {
                gameOverFired = true;
                OnGameOver?.Invoke();
            }
            return;
        }

        if (invulnerabilitySeconds > 0f)
            StartCoroutine(InvulRoutine(invulnerabilitySeconds));
    }

    public void ResetLives()
    {
        StopAllCoroutines(); // corta parpadeos antiguos
        IsInvulnerable = false;
        gameOverFired = false;

        CurrentLives = Mathf.Clamp(startLives, 0, MaxLives);
        OnInvulnerabilityChanged.Invoke(false);
        OnLivesChanged.Invoke(CurrentLives, MaxLives);
    }

    IEnumerator InvulRoutine(float sec)
    {
        IsInvulnerable = true;
        OnInvulnerabilityChanged.Invoke(true);
        float t = 0f;
        while (t < sec) { t += Time.unscaledDeltaTime; yield return null; }
        IsInvulnerable = false;
        OnInvulnerabilityChanged.Invoke(false);
    }
}
