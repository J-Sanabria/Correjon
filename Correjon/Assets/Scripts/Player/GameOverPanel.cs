using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    [Header("Refs")]
    public LivesSystem lives;
    public CurrencySystem currency;
    public RunDistance runDistance;

    [Header("UI")]
    public GameObject panelRoot;
    public TextMeshProUGUI runCoinsText;
    public TextMeshProUGUI totalCoinsText;
    public TextMeshProUGUI distanceText;
    public Button retryButton;
    public Button menuButton;

    [Header("Options")]
    public bool pauseOnShow = true;
    [Tooltip("Nombre de la escena del menú principal")]
    public string menuSceneName = "MainMenu";

    bool shown;

    void Awake()
    {
        if (!lives) lives = LivesSystem.Instance;
        if (!currency) currency = CurrencySystem.Instance;
        if (panelRoot) panelRoot.SetActive(false);
    }

    void OnEnable()
    {
        // Si usas el evento:
        if (LivesSystem.Instance != null)
            LivesSystem.Instance.OnGameOver += Show;

        // Si ya estabas en game over (por orden de inicialización), muéstrate
        if (LivesSystem.Instance != null && LivesSystem.Instance.CurrentLives <= 0)
            Show();
        // Asegurar listeners de botones
        if (retryButton) { retryButton.onClick.RemoveAllListeners(); retryButton.onClick.AddListener(OnRetry); }
        if (menuButton) { menuButton.onClick.RemoveAllListeners(); menuButton.onClick.AddListener(OnMenu); }

        // “Sticky”: si ya estabas en GameOver cuando este panel se habilita, muéstrate
        if (LivesSystem.Instance != null && LivesSystem.Instance.CurrentLives <= 0 && !shown)
            Show();
    }

    void OnDisable()
    {
        if (LivesSystem.Instance != null)
            LivesSystem.Instance.OnGameOver -= Show;
    }
    public void Show()
    {
        if (panelRoot) panelRoot.SetActive(true);

        var cur = CurrencySystem.Instance;
        int runCoinsBefore = cur ? cur.RunCoins : 0;   // 1) captura

        // 2) suma al total (esto deja RunCoins en 0)
        cur?.BankRunCoinsOnce();

        // 3) UI
        if (runCoinsText) runCoinsText.text = $"Monedas carrera: {runCoinsBefore}";
        if (totalCoinsText) totalCoinsText.text = $"Monedas totales: {cur?.Wallet ?? 0}";

        if (distanceText && runDistance != null)
            distanceText.text = $"Distancia: {runDistance.Distance:0.0} m";

        if (pauseOnShow) Time.timeScale = 0f;
    }
    public void OnRetry()
    {
        // Quitar pausa
        Time.timeScale = 1f;

        // RESET de sistemas persistentes
        if (LivesSystem.Instance != null)
            LivesSystem.Instance.ResetLives();       // <-- vuelve a startLives y limpia flags

        if (runDistance != null)
            runDistance.ResetDistance();             // <-- si tu RunDistance persiste

        // Recargar escena actual
        var current = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(current.buildIndex);
    }


    public void OnMenu()
    {
        if (string.IsNullOrEmpty(menuSceneName))
        {
            Debug.LogWarning("GameOverPanel: menuSceneName no está asignado.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
