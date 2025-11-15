using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject panelMenu;          // Principal
    [SerializeField] GameObject panelDiario;        // Diario del Páramo
    [SerializeField] GameObject panelTienda;        // Tienda
    [SerializeField] GameObject panelOpciones;      // Opciones (audio/video)
    [SerializeField] GameObject panelSalir;         // Confirmación salir

    [Header("Escena de Juego")]
    [SerializeField] string gameplaySceneName = "Game";
    void Start()
    {
        MusicManager.Instance.PlayMenuMusic();
        ShowOnly(panelMenu);
    }

    void Update()
    {
        // Botón "Atrás" en Android/ESC en PC:
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Si hay un panel secundario abierto, vuelve al menú.
            if (panelDiario.activeSelf || panelTienda.activeSelf || panelOpciones.activeSelf || panelSalir.activeSelf)
                ShowOnly(panelMenu);
            else
                ShowOnly(panelSalir); // desde menú principal abre confirmación de salida
        }
    }

    void ShowOnly(GameObject target)
    {
        panelMenu.SetActive(false);
        panelDiario.SetActive(false);
        panelTienda.SetActive(false);
        panelOpciones.SetActive(false);
        panelSalir.SetActive(false);
        target.SetActive(true);
    }


    // === Botones del menú principal ===
    public void OnPlay()
    {
        if (!string.IsNullOrEmpty(gameplaySceneName))
            SceneManager.LoadScene(gameplaySceneName);
    }

    public void OnOpenDiario() => ShowOnly(panelDiario);
    public void OnOpenTienda() => ShowOnly(panelTienda);

    public void OnOpenSalir() => ShowOnly(panelSalir);
    public void OnOpenOptions() => ShowOnly(panelOpciones);
    // === Botones comunes de "Volver" ===+
    public void OnBackToMenu() => ShowOnly(panelMenu);

    // === Confirmación de salida ===
    public void OnConfirmExitYes()
    {
        // En editor no cierra; en dispositivo sí.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void OnConfirmExitNo() => ShowOnly(panelMenu);

}
