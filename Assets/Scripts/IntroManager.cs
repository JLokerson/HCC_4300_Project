using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IntroManager : MonoBehaviour
{
    public static bool IsIntroPlaying { get; private set; } = false;
    
    [Header("Cutscene Canvases")]
    public Canvas[] cutsceneCanvases;
    
    [Header("Timing Settings")]
    public float fadeInDuration = 1.5f;
    public float canvasDisplayTime = 3f;
    public float fadeOutDuration = 1f;
    public float delayBetweenCanvases = 0.5f;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] canvasSounds;
    
    private CanvasGroup[] canvasGroups;
    private PauseManager pauseManager;
    
    void Start()
    {
        IsIntroPlaying = true;
        
        // Find and disable PauseManager during intro
        pauseManager = FindObjectOfType<PauseManager>();
        if (pauseManager != null)
        {
            pauseManager.enabled = false;
        }
        
        // Pause the game world during intro
        Time.timeScale = 0f;
        
        SetupCutsceneCanvases();
        StartIntroSequence();
    }
    
    void SetupCutsceneCanvases()
    {
        if (cutsceneCanvases == null || cutsceneCanvases.Length == 0) 
        {
            Debug.LogWarning("No cutscene canvases assigned to IntroManager!");
            return;
        }
        
        canvasGroups = new CanvasGroup[cutsceneCanvases.Length];
        
        for (int i = 0; i < cutsceneCanvases.Length; i++)
        {
            if (cutsceneCanvases[i] != null)
            {
                // Ensure canvas has CanvasGroup component
                canvasGroups[i] = cutsceneCanvases[i].GetComponent<CanvasGroup>();
                if (canvasGroups[i] == null)
                    canvasGroups[i] = cutsceneCanvases[i].gameObject.AddComponent<CanvasGroup>();
                
                // Setup canvas properties
                cutsceneCanvases[i].sortingOrder = 100 + i; // Ensure proper layering
                cutsceneCanvases[i].gameObject.SetActive(true);
                
                // Initialize canvas group state
                canvasGroups[i].alpha = 0f;
                canvasGroups[i].interactable = false;
                canvasGroups[i].blocksRaycasts = false;
            }
        }
    }
    
    void StartIntroSequence()
    {
        StartCoroutine(PlayIntroSequence());
    }
    
    IEnumerator PlayIntroSequence()
    {
        for (int i = 0; i < cutsceneCanvases.Length; i++)
        {
            if (cutsceneCanvases[i] != null && canvasGroups[i] != null)
            {
                // Hide previous canvas before showing current one (except for first canvas)
                if (i > 0 && canvasGroups[i - 1] != null)
                {
                    canvasGroups[i - 1].alpha = 0f;
                }
                
                // Play sound if available
                if (canvasSounds != null && i < canvasSounds.Length && canvasSounds[i] != null && audioSource != null)
                {
                    audioSource.PlayOneShot(canvasSounds[i]);
                }
                
                // Show current canvas immediately
                canvasGroups[i].alpha = 1f;
                
                // Display canvas (use unscaled time since game is paused)
                yield return new WaitForSecondsRealtime(canvasDisplayTime);
                
                // Delay before next canvas (except for last canvas)
                if (i < cutsceneCanvases.Length - 1)
                {
                    yield return new WaitForSecondsRealtime(delayBetweenCanvases);
                }
            }
        }
        
        // Intro sequence complete - hide all canvases and start game world
        HideAllCanvases();
        StartGameWorld();
    }
    
    void HideAllCanvases()
    {
        for (int i = 0; i < cutsceneCanvases.Length; i++)
        {
            if (cutsceneCanvases[i] != null && canvasGroups[i] != null)
            {
                canvasGroups[i].alpha = 0f;
                cutsceneCanvases[i].gameObject.SetActive(false);
            }
        }
    }
    
    public void SkipIntro()
    {
        StopAllCoroutines();
        HideAllCanvases();
        StartGameWorld();
    }
    
    void StartGameWorld()
    {
        IsIntroPlaying = false;
        
        // Re-enable PauseManager
        if (pauseManager != null)
        {
            pauseManager.enabled = true;
        }
        
        // Unpause the game world
        Time.timeScale = 1f;
        Debug.Log("Game world started");
    }
    
    void Update()
    {
        // Allow skipping intro with Escape or Space key
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
        {
            SkipIntro();
        }
    }
}
