using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper class for creating consistent UI elements across menus
/// </summary>
public class UIHelpers : MonoBehaviour
{
    [Header("UI Style Settings")]
    public Color primaryButtonColor = new Color(0.2f, 0.6f, 1f, 1f); // Blue
    public Color secondaryButtonColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray
    public Color dangerButtonColor = new Color(1f, 0.3f, 0.3f, 1f); // Red
    public Color textColor = Color.white;
    public Font defaultFont;
    
    /// <summary>
    /// Style a button with primary colors
    /// </summary>
    public void StylePrimaryButton(Button button)
    {
        if (button != null)
        {
            var colors = button.colors;
            colors.normalColor = primaryButtonColor;
            colors.highlightedColor = Color.Lerp(primaryButtonColor, Color.white, 0.2f);
            colors.pressedColor = Color.Lerp(primaryButtonColor, Color.black, 0.2f);
            button.colors = colors;
        }
    }
    
    /// <summary>
    /// Style a button with secondary colors
    /// </summary>
    public void StyleSecondaryButton(Button button)
    {
        if (button != null)
        {
            var colors = button.colors;
            colors.normalColor = secondaryButtonColor;
            colors.highlightedColor = Color.Lerp(secondaryButtonColor, Color.white, 0.2f);
            colors.pressedColor = Color.Lerp(secondaryButtonColor, Color.black, 0.2f);
            button.colors = colors;
        }
    }
    
    /// <summary>
    /// Style a button with danger colors (for quit/delete actions)
    /// </summary>
    public void StyleDangerButton(Button button)
    {
        if (button != null)
        {
            var colors = button.colors;
            colors.normalColor = dangerButtonColor;
            colors.highlightedColor = Color.Lerp(dangerButtonColor, Color.white, 0.2f);
            colors.pressedColor = Color.Lerp(dangerButtonColor, Color.black, 0.2f);
            button.colors = colors;
        }
    }
    
    /// <summary>
    /// Style text with consistent settings
    /// </summary>
    public void StyleText(Text text, int fontSize = 16, TextAnchor alignment = TextAnchor.MiddleCenter)
    {
        if (text != null)
        {
            text.color = textColor;
            text.fontSize = fontSize;
            text.alignment = alignment;
            
            if (defaultFont != null)
                text.font = defaultFont;
        }
    }
    
    /// <summary>
    /// Add hover sound effects to a button
    /// </summary>
    public void AddButtonSounds(Button button, AudioClip hoverSound = null, AudioClip clickSound = null)
    {
        if (button == null) return;
        
        // Add audio source if it doesn't exist
        AudioSource audioSource = button.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = button.gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // You can expand this to add event triggers for hover and click sounds
        // This would require UnityEngine.EventSystems
    }
    
    /// <summary>
    /// Animate a UI element fade in
    /// </summary>
    public void FadeIn(CanvasGroup canvasGroup, float duration = 0.5f)
    {
        if (canvasGroup != null)
        {
            StartCoroutine(FadeCoroutine(canvasGroup, 0f, 1f, duration));
        }
    }
    
    /// <summary>
    /// Animate a UI element fade out
    /// </summary>
    public void FadeOut(CanvasGroup canvasGroup, float duration = 0.5f)
    {
        if (canvasGroup != null)
        {
            StartCoroutine(FadeCoroutine(canvasGroup, 1f, 0f, duration));
        }
    }
    
    /// <summary>
    /// Coroutine for fade animations
    /// </summary>
    private System.Collections.IEnumerator FadeCoroutine(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time so it works when paused
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }
        
        canvasGroup.alpha = endAlpha;
    }
}