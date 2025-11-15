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
    public JournalSystem journal; 

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
        if (!journal) journal = JournalSystem.Instance;
        if (panelRoot) panelRoot.SetActive(false);
    }

    void OnEnable()
    {
        // Escuchar el evento de Game Over
        if (LivesSystem.Instance != null)
            LivesSystem.Instance.OnGameOver += Show;

        // Asegurar listeners de botones
        if (retryButton)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnRetry);
        }

        if (menuButton)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(OnMenu);
        }

        // Si ya estás en GameOver por orden de inicialización
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

        MusicManager.Instance.StopMusic();

        if (shown) return;
        shown = true;

        if (panelRoot) panelRoot.SetActive(true);

        // Captura monedas ganadas en la carrera
        var cur = CurrencySystem.Instance;
        int runCoinsBefore = cur ? cur.RunCoins : 0;
        cur?.BankRunCoinsOnce(); // suma al total

        // Actualizar UI
        if (runCoinsText) runCoinsText.text = $"Monedas carrera: {runCoinsBefore}";
        if (totalCoinsText) totalCoinsText.text = $"Monedas totales: {cur?.Wallet ?? 0}";
        if (distanceText && runDistance != null)
            distanceText.text = $"Distancia: {runDistance.Distance:0.0} m";

        // Guardado final
        SaveFinalProgress();

        if (pauseOnShow) Time.timeScale = 0f;
    }

    void SaveFinalProgress()
    {
        if (currency != null)
            SaveManager.Data.wallet = currency.Wallet;

        if (journal != null)
            SaveManager.Data.journalLeaves = journal.TotalLeaves;

        SaveManager.Save();
        Debug.Log("[GameOverPanel] Progreso guardado");
    }

    public void OnRetry()
    {
        Time.timeScale = 1f;

        if (LivesSystem.Instance != null)
            LivesSystem.Instance.ResetLives();

        if (runDistance != null)
            runDistance.ResetDistance();

        // Recargar escena actual
        var current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    public void OnMenu()
    {
        Time.timeScale = 1f;
        if (string.IsNullOrEmpty(menuSceneName))
        {
            Debug.LogWarning("GameOverPanel: menuSceneName no está asignado.");
            return;
        }

        SceneManager.LoadScene(menuSceneName);
    }
}
