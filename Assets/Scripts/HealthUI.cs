using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUI : MonoBehaviour
{
    [Header("Assign in Inspector (auto-finds if left empty)")]
    [SerializeField] private HealthSystem playerHealth;
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text healthText;

    private void Awake()
    {
        if (!healthBar) healthBar = GetComponent<Slider>();
        if (!healthText) healthText = GetComponentInChildren<TMP_Text>();
        if (!playerHealth) playerHealth = FindObjectOfType<HealthSystem>();

        if (healthBar)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = 1f;
            healthBar.interactable = false;
        }
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateUI;
            playerHealth.OnDied += HandleDeath;

            // Initial draw
            float norm = playerHealth.MaxHealth > 0
                ? (float)playerHealth.CurrentHealth / playerHealth.MaxHealth
                : 0f;
            UpdateUI(norm, playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateUI;
            playerHealth.OnDied -= HandleDeath;
        }
    }

    private void UpdateUI(float normalized, int current, int max)
    {
        if (healthBar) healthBar.value = Mathf.Clamp01(normalized);
        if (healthText) healthText.text = $"{current} / {max}";
    }

    private void HandleDeath()
    {
        // Optional: flash red, show skull, etc.
    }
}
