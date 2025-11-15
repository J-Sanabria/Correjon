using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button quitButton;

    [Header("Opciones")]
    public KeyCode pauseKey = KeyCode.Escape; // tecla rápida para PC opcional

    private bool isPaused;

    void Start()
    {
        if (pausePanel) pausePanel.SetActive(false);

        if (resumeButton)
            resumeButton.onClick.AddListener(ResumeGame);

        if (quitButton)
            quitButton.onClick.AddListener(QuitToGameOver);
    }

    void Update()
    {
        // Atajo para PC o debug
        if (Input.GetKeyDown(pauseKey))
        {
            if (!isPaused) PauseGame();
            else ResumeGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;

        if (pausePanel) pausePanel.SetActive(true);
        Time.timeScale = 0f; // pausa el tiempo del juego
        Debug.Log("Juego en pausa");
    }

    public void ResumeGame()
    {
        if (!isPaused) return;
        isPaused = false;

        if (pausePanel) pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Debug.Log("Juego reanudado");
    }
    public void QuitToGameOver()
    {
        pausePanel.gameObject.SetActive(false);
        Time.timeScale = 1f;
        Debug.Log("Forzando finalización inmediata de la partida...");

        if (LivesSystem.Instance != null)
            LivesSystem.Instance.ForceGameOver();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

}
