using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;

public class GameHUD : MonoBehaviour
{
    [Header("Referencias de datos")]
    [SerializeField] LivesSystem livesSystem;
    [SerializeField] RunTimer runTimer;

    [Header("UI - Vidas")]
    //// Opción A: numérica "x3"
    //[SerializeField] TextMeshPro livesText; // reemplaza por TextMeshProUGUI si usas TMP
    //// Opción B: íconos (corazones) — opcional
    [SerializeField] Transform heartsContainer;
    [SerializeField] Image heartPrefab; // simple sprite de corazón

    [Header("UI - Tiempo")]
    [SerializeField] TextMeshProUGUI timeText; // o TMP

    void OnEnable()
    {
        if (livesSystem != null)
            livesSystem.OnLivesChanged.AddListener(UpdateLives);

        if (runTimer != null)
            runTimer.OnTimeChanged.AddListener(UpdateTime);
    }

    void OnDisable()
    {
        if (livesSystem != null)
            livesSystem.OnLivesChanged.RemoveListener(UpdateLives);
        if (runTimer != null)
            runTimer.OnTimeChanged.RemoveListener(UpdateTime);
    }

    void Start()
    {
        // Forzar actualización al inicio
        if (livesSystem != null)
            UpdateLives(livesSystem.CurrentLives, livesSystem.MaxLives);
        if (runTimer != null)
            UpdateTime(runTimer.Elapsed);
    }
    void UpdateLives(int current, int max)
    {
        if (heartsContainer == null || heartPrefab == null) return;

        int existing = heartsContainer.childCount;

        // Si faltan corazones, crea los que falten
        for (int i = existing; i < max; i++)
        {
            var img = Instantiate(heartPrefab, heartsContainer);
            img.gameObject.SetActive(true);
        }

        // Actualiza visibilidad/opacidad de todos los corazones
        for (int i = 0; i < heartsContainer.childCount; i++)
        {
            var img = heartsContainer.GetChild(i).GetComponent<Image>();
            if (img != null)
                img.color = new Color(1f, 1f, 1f, (i < current) ? 1f : 0.25f);

            // Si hay más de las necesarias, ocúltalas
            img.gameObject.SetActive(i < max);
        }
    }


    void UpdateTime(float seconds)
    {
        if (timeText != null)
            timeText.text = $"Tiempo: {FormatTime(seconds)}";
    }

    string FormatTime(float s)
    {
        int total = Mathf.FloorToInt(s);
        int min = total / 60;
        int sec = total % 60;
        int ms = Mathf.FloorToInt((s - Mathf.Floor(s)) * 1000f) / 10; // centésimas
        // mm:ss:cc
        return $"{min:00}:{sec:00}:{ms:00}";
    }
}
