using UnityEngine;
using TMPro;

public class HealthUI : MonoBehaviour
{
    [Tooltip("Attempts to find player if not specified")]
    public Health target;
    public TMP_Text text;

    void Update()
    {
        if (target && text)
        {
            text.text = $"HP: {Mathf.CeilToInt(target.currentHealth)}/{Mathf.CeilToInt(target.maxHealth)}";
        }
    }

    private void Start()
    {
        if (target == null)
        {
            try
            {
                target = GameObject.FindWithTag("Player").GetComponent<Health>();
            }
            catch
            {
                Debug.LogWarning("No Player with Health component found for HealthUI");
            }
        }
    }
}
