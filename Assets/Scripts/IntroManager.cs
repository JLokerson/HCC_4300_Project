using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [Header("Intro Panels")]
    public GameObject[] introPanels;
    
    [Header("Timing Settings")]
    public float fadeInDuration = 1.5f;
    public float fadeOutDuration = 1f;
    public float panelDisplayDuration = 3f;
    public float delayBetweenPanels = 0.5f;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip introSound;
    
    private CanvasGroup[] panelCanvasGroups;
    
    void Start()
    {
        SetupPanels();
        StartIntroSequence();
    }
    
    void SetupPanels()
    {
        if (introPanels == null || introPanels.Length == 0) return;
        
        panelCanvasGroups = new CanvasGroup[introPanels.Length];
        
        for (int i = 0; i < introPanels.Length; i++)
        {
            if (introPanels[i] != null)
            {
                panelCanvasGroups[i] = introPanels[i].GetComponent<CanvasGroup>();
                if (panelCanvasGroups[i] == null)
                    panelCanvasGroups[i] = introPanels[i].AddComponent<CanvasGroup>();
                
                panelCanvasGroups[i].alpha = 0f;
                introPanels[i].SetActive(true);
            }
        }
    }
    
    void StartIntroSequence()
    {
        StartCoroutine(IntroCutscene());
    }
    
    IEnumerator IntroCutscene()
    {
        // Play intro sound
        if (audioSource != null && introSound != null)
        {
            audioSource.PlayOneShot(introSound);
        }
        
        // Show each panel in sequence
        for (int i = 0; i < introPanels.Length; i++)
        {
            if (introPanels[i] != null && panelCanvasGroups[i] != null)
            {
                // Fade in panel
                yield return StartCoroutine(FadeInPanel(panelCanvasGroups[i]));
                
                // Display panel for set duration
                yield return new WaitForSeconds(panelDisplayDuration);
                
                // Fade out panel
                yield return StartCoroutine(FadeOutPanel(panelCanvasGroups[i]));
                
                // Delay before next panel
                if (i < introPanels.Length - 1)
                {
                    yield return new WaitForSeconds(delayBetweenPanels);
                }
            }
        }
        
        // Hide all panels after cutscene
        HideAllPanels();
    }
    
    IEnumerator FadeInPanel(CanvasGroup canvasGroup)
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }
    
    IEnumerator FadeOutPanel(CanvasGroup canvasGroup)
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeOutDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }
    
    void HideAllPanels()
    {
        for (int i = 0; i < introPanels.Length; i++)
        {
            if (introPanels[i] != null)
            {
                introPanels[i].SetActive(false);
            }
        }
    }
    
    public void SkipIntro()
    {
        StopAllCoroutines();
        HideAllPanels();
    }
}
