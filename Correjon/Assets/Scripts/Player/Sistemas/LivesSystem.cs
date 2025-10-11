using UnityEngine;
using UnityEngine.Events;

[System.Serializable] public class LivesChangedEvent : UnityEvent<int, int> { } // (current, max)

public class LivesSystem : MonoBehaviour
{
    [SerializeField] int maxLives = 3;
    [SerializeField] int startLives = 3;

    public int MaxLives => maxLives;
    public int CurrentLives { get; private set; }

    public LivesChangedEvent OnLivesChanged = new LivesChangedEvent();

    void Awake()
    {
        CurrentLives = Mathf.Clamp(startLives, 0, maxLives);
    }

    void Start()
    {
        OnLivesChanged.Invoke(CurrentLives, MaxLives);
    }

    public void AddLife(int amount = 1)
    {
        CurrentLives = Mathf.Clamp(CurrentLives + amount, 0, MaxLives);
        OnLivesChanged.Invoke(CurrentLives, MaxLives);
    }

    public void LoseLife(int amount = 1)
    {
        CurrentLives = Mathf.Max(0, CurrentLives - amount);
        OnLivesChanged.Invoke(CurrentLives, MaxLives);

        if (CurrentLives <= 0)
        {
            // Aquí puedes notificar KO / fin de carrera si quieres
            // SendMessage("OnPlayerKO", SendMessageOptions.DontRequireReceiver);
        }
    }

    public void ResetLives()
    {
        CurrentLives = Mathf.Clamp(startLives, 0, MaxLives);
        OnLivesChanged.Invoke(CurrentLives, MaxLives);
    }
}
