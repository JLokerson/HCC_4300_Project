using UnityEngine;
using System;

public class GameTimer : MonoBehaviour
{
    public float ElapsedSeconds { get; private set; }
    public bool IsRunning { get; private set; }

    // Events so UI doesn't need to poll every frame if you don't want to
    public event Action<string, float> OnTimerTick;   // formatted, rawSeconds
    public event Action OnTimerReset;

    [SerializeField] private Health health; // ⟵ was HealthSystem

    void Awake()
    {
        // Be flexible: allow assignment in Inspector or auto-find on this object or parent
        if (!health) health = GetComponent<Health>();
        if (!health) health = GetComponentInParent<Health>();
    }

    void OnEnable()
    {
        if (health) health.OnDeath += HandleDeath; // ⟵ was OnDied
        StartTimer();
        // Emit an initial tick so any listeners render 0:00 immediately
        OnTimerTick?.Invoke(Format(ElapsedSeconds), ElapsedSeconds);
    }

    void OnDisable()
    {
        if (health) health.OnDeath -= HandleDeath;
    }

    void Update()
    {
        if (!IsRunning) return;
        ElapsedSeconds += Time.deltaTime;
        OnTimerTick?.Invoke(Format(ElapsedSeconds), ElapsedSeconds);
    }

    public void StartTimer() => IsRunning = true;
    public void StopTimer() => IsRunning = false;

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
        // Reset/restart from your respawn code when appropriate
        // e.g., call RestartTimer() after you reload or respawn the player
    }

    private string Format(float seconds)
    {
        int total = Mathf.FloorToInt(seconds);
        int mins = total / 60;
        int secs = total % 60;
        return $"{mins:0}:{secs:00}";
    }
}
