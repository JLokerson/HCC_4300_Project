using UnityEngine;
using System;

public class GameTimer : MonoBehaviour
{
    public float ElapsedSeconds { get; private set; }
    public bool IsRunning { get; private set; }

    // Events so UI doesn't need to poll every frame if you don't want to
    public event Action<string, float> OnTimerTick;   // formatted, rawSeconds
    public event Action OnTimerReset;

    [SerializeField] private HealthSystem health;

    void Start()
    {
        if (!health) health = GetComponent<HealthSystem>();
        StartTimer();

        if (health)
        {
            health.OnDied += HandleDeath;
        }
    }

    void Update()
    {
        if (!IsRunning) return;
        ElapsedSeconds += Time.deltaTime;
        OnTimerTick?.Invoke(Format(ElapsedSeconds), ElapsedSeconds);
    }

    public void StartTimer()
    {
        IsRunning = true;
    }

    public void StopTimer()
    {
        IsRunning = false;
    }

    public void ResetTimer()
    {
        ElapsedSeconds = 0f;
        OnTimerReset?.Invoke();
        OnTimerTick?.Invoke(Format(ElapsedSeconds), ElapsedSeconds);
    }

    public void RestartTimer()
    {
        ResetTimer();
        StartTimer();
    }

    private void HandleDeath()
    {
        StopTimer();
        // You asked: “continue counting until the player dies, then it resets.”
        // We'll reset on respawn (call RestartTimer from your respawn code).
    }

    private string Format(float seconds)
    {
        int total = Mathf.FloorToInt(seconds);
        int mins = total / 60;
        int secs = total % 60;
        return $"{mins:0}:{secs:00}";
    }
}
