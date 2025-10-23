using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class XPUI : MonoBehaviour
{
    [SerializeField] private XPSystem playerXP;
    [SerializeField] private Slider xpBar;
    [SerializeField] private TMP_Text levelText;

    void OnEnable()
    {
        if (playerXP != null)
        {
            playerXP.OnXPChanged += UpdateXP;
            playerXP.OnLevelChanged += UpdateLevel;
        }
    }

    void OnDisable()
    {
        if (playerXP != null)
        {
            playerXP.OnXPChanged -= UpdateXP;
            playerXP.OnLevelChanged -= UpdateLevel;
        }
    }

    private void UpdateXP(float normalized, int current, int required)
    {
        if (xpBar) xpBar.value = normalized;
        if (levelText) levelText.text = $"Lv. {playerXP.Level}  ({current}/{required})";
    }

    private void UpdateLevel(int level, int newRequired)
    {
        if (levelText) levelText.text = $"Lv. {level}  (0/{newRequired})";
        // Optional: play level-up VFX/SFX
    }
}
