using UnityEngine;
using TMPro;

public class UITimer : MonoBehaviour
{
    [Header("Assign your TMP text (or leave blank to auto-find)")]
    [SerializeField] private TMP_Text label;

    [Header("Behavior")]
    [SerializeField] private bool autoStart = true;       // starts on enable
    [SerializeField] private bool useUnscaledTime = false; // ignore Time.timeScale (e.g., pause)

    private float elapsed;
    private bool running;

    void Awake()
    {
        // Auto-find the TMP text on this object if not assigned
        if (!label) label = GetComponent<TMP_Text>();
        if (label) label.text = "0:00";
    }

    void OnEnable()
    {
        if (autoStart) StartTimer();
    }

    void Update()
    {
        if (!running) return;

        elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (label) label.text = Format(elapsed);
    }

    // Public controls you can call from other scripts or buttons
    public void StartTimer() { running = true; }
    public void StopTimer() { running = false; }
    public void ResetTimer() { elapsed = 0f; if (label) label.text = "0:00"; }
    public void RestartTimer() { ResetTimer(); StartTimer(); }

    public float ElapsedSeconds => elapsed;

    private string Format(float seconds)
    {
        int total = Mathf.FloorToInt(seconds);
        int mins = total / 60;
        int secs = total % 60;
        return $"{mins:0}:{secs:00}";
    }
}
