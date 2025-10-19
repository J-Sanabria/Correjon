using UnityEngine;
using UnityEngine.Events;

[System.Serializable] public class DistanceChangedEvent : UnityEvent<float> { } // metros

public class RunDistance : MonoBehaviour
{
    [Header("Configuración")]
    public bool autoStart = true;
    public bool isRunning { get; private set; }

    [Tooltip("Velocidad horizontal del jugador en m/s (se puede actualizar desde PlayerController).")]
    public float speed = 4.2f;

    public float Distance { get; private set; }  // en metros
    public DistanceChangedEvent OnDistanceChanged = new DistanceChangedEvent();

    void Start()
    {
        if (autoStart) StartRun();
    }

    void Update()
    {
        if (!isRunning) return;

        // Aumenta distancia por frame
        Distance += speed * Time.deltaTime;
        OnDistanceChanged.Invoke(Distance);
    }

    public void StartRun() => isRunning = true;
    public void StopRun() => isRunning = false;

    public void ResetDistance()
    {
        Distance = 0f;
        OnDistanceChanged.Invoke(Distance);
    }

    // Permite actualizar velocidad desde PlayerController
    public void SetSpeed(float newSpeed) => speed = newSpeed;
}
