using UnityEngine;
using UnityEngine.Events;

[System.Serializable] public class TimeChangedEvent : UnityEvent<float> { } // seconds

public class RunTimer : MonoBehaviour
{
    public bool autoStart = true;
    public bool isRunning { get; private set; }

    public float Elapsed { get; private set; }
    public TimeChangedEvent OnTimeChanged = new TimeChangedEvent();

    void Start()
    {
        if (autoStart) StartTimer();
    }

    void Update()
    {
        if (!isRunning) return;

        Elapsed += Time.deltaTime;
        OnTimeChanged.Invoke(Elapsed);
    }

    public void StartTimer() { isRunning = true; }
    public void StopTimer() { isRunning = false; }
    public void ResetTimer()
    {
        Elapsed = 0f;
        OnTimeChanged.Invoke(Elapsed);
    }
}
