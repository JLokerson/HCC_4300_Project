using UnityEngine;
using TMPro;

public class HealthUI : MonoBehaviour
{
    public Health target;
    public TMP_Text text;

    void Update()
    {
        if (target && text)
        {
            text.text = $"HP: {Mathf.CeilToInt(target.currentHealth)}/{Mathf.CeilToInt(target.maxHealth)}";
        }
    }
}
