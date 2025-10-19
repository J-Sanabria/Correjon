using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [Header("Referencias de datos")]
    [SerializeField] LivesSystem livesSystem;
    [SerializeField] RunDistance runDistance;  // Cambiado

    [Header("UI - Vidas")]
    [SerializeField] Transform heartsContainer;
    [SerializeField] Image heartPrefab;

    [Header("UI - Distancia")]
    [SerializeField] TextMeshProUGUI distanceText;  // Cambiado

    void OnEnable()
    {
        if (livesSystem != null)
            livesSystem.OnLivesChanged.AddListener(UpdateLives);

        if (runDistance != null)
            runDistance.OnDistanceChanged.AddListener(UpdateDistance);
    }

    void OnDisable()
    {
        if (livesSystem != null)
            livesSystem.OnLivesChanged.RemoveListener(UpdateLives);
        if (runDistance != null)
            runDistance.OnDistanceChanged.RemoveListener(UpdateDistance);
    }

    void Start()
    {
        if (livesSystem != null)
            UpdateLives(livesSystem.CurrentLives, livesSystem.MaxLives);
        if (runDistance != null)
            UpdateDistance(runDistance.Distance);
    }

    // === VIDAS ===
    void UpdateLives(int current, int max)
    {
        if (heartsContainer == null || heartPrefab == null) return;

        int existing = heartsContainer.childCount;

        for (int i = existing; i < max; i++)
        {
            var img = Instantiate(heartPrefab, heartsContainer);
            img.gameObject.SetActive(true);
        }

        for (int i = 0; i < heartsContainer.childCount; i++)
        {
            var img = heartsContainer.GetChild(i).GetComponent<Image>();
            if (img != null)
                img.color = new Color(1f, 1f, 1f, (i < current) ? 1f : 0.25f);

            img.gameObject.SetActive(i < max);
        }
    }

    // === DISTANCIA ===
    void UpdateDistance(float meters)
    {
        if (distanceText != null)
            distanceText.text = $"Distancia: {meters:0.0} m";
    }
}
