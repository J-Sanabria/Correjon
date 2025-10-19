using UnityEngine;
using TMPro;

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

    [Header("Options")]
    public bool pauseOnShow = true;

    void Awake()
    {
        if (!lives) lives = LivesSystem.Instance;
        if (!currency) currency = CurrencySystem.Instance;
        if (panelRoot) panelRoot.SetActive(false);
    }

    void OnEnable()
    {
        if (LivesSystem.Instance != null)
            LivesSystem.Instance.OnGameOver += Show;
        Debug.Log("GAME OVER fired en game over panel");// <-- C# event
    }

    void OnDisable()
    {
        if (LivesSystem.Instance != null)
            LivesSystem.Instance.OnGameOver -= Show;
        Debug.Log("GAME OVER se apago");// <-- desuscribir
    }

    public void Show()
    {
        Debug.Log("si entro a show");
        if (panelRoot) panelRoot.SetActive(true);

        if (runCoinsText && currency != null)
            runCoinsText.text = $"Monedas carrera: {currency.RunCoins}";

        if (totalCoinsText && currency != null)
            totalCoinsText.text = $"Monedas totales: {currency.Wallet}";

        if (distanceText && runDistance != null)
            distanceText.text = $"Distancia: {runDistance.Distance:0.0} m";

        if (pauseOnShow) Time.timeScale = 0f;
    }

    public void OnRetry()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void OnMenu(string sceneName)
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
