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

    [Header("Panels Options")]
    [SerializeField] GameObject OP_Video;
    [SerializeField] GameObject OP_Audio;

    [Header("Escena de Juego")]
    [SerializeField] string gameplaySceneName = "Game";

    void Start()
    {
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

    void ShowOnlyOption(GameObject target)
    {
        OP_Video.SetActive(false);
        OP_Audio.SetActive(false);
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
    public void OnOpenOpciones() {
        ShowOnly(panelOpciones);
        ShowOnlyOption(OP_Video);
    }  
    public void OnOpenSalir() => ShowOnly(panelSalir);

    public void OnOpenVideo() => ShowOnlyOption(OP_Video);
    public void OnOpenAudio() => ShowOnlyOption(OP_Audio);

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
