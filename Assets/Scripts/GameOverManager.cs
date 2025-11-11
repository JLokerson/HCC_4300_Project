using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button restartButton;
    public Button mainMenuButton;
    
    [Header("Cutscene Settings")]
    public float fadeToBlackDuration = 2f;
    public float fadeInDuration = 2f;
    public float textDelay = 1f;
    public float buttonDelay = 3f;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip gameOverSound;
    
    [Header("Scene Names")]
    public string gameSceneName = "SampleScene"; // Change to your actual game scene name
    public string mainMenuSceneName = "MainMenu"; // Change to your actual main menu scene name
    
    private CanvasGroup panelCanvasGroup;
    private CanvasGroup blackFadeCanvasGroup;
    private GameObject blackFadePanel;
    private static GameOverManager instance;
    private PlayerInput playerInput;
    
    void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        SetupUI();
        // Find player input component
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerInput = player.GetComponent<PlayerInput>();
        }
    }
    
    public static void TriggerGameOver()
    {
        if (instance != null)
        {
            instance.HandleGameOver();
        }
    }
    
    private void HandleGameOver()
    {
        // Stop all time (like pause menu)
        Time.timeScale = 0f;
        
        // Disable player input completely
        DisablePlayerInput();
        
        // Stop player movement
        StopPlayerMovement();
        
        // Enable cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        StartGameOverSequence();
    }
    
    private void DisablePlayerInput()
    {
        if (playerInput != null)
        {
            playerInput.enabled = false;
            Debug.Log("[GameOverManager] Disabled PlayerInput component");
        }
        
        // Also try to find and disable any other input components
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Hide the reticle
            GameObject reticle = GameObject.Find("reticle");
            if (reticle != null)
            {
                reticle.SetActive(false);
                Debug.Log("[GameOverManager] Disabled reticle");
            }
            
            // Disable common player control scripts
            MonoBehaviour[] allComponents = player.GetComponents<MonoBehaviour>();
            foreach (var component in allComponents)
            {
                string componentName = component.GetType().Name;
                if (componentName.Contains("Character") || 
                    componentName.Contains("Player") || 
                    componentName.Contains("Movement") ||
                    componentName.Contains("Controller") ||
                    componentName.Contains("Core"))  // Added "Core" to catch CharacterCore
                {
                    component.enabled = false;
                    Debug.Log($"[GameOverManager] Disabled {componentName}");
                }
            }
        }
    }
    
    private void StopPlayerMovement()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Stop rigidbody movement
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null) 
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            Rigidbody2D rb2d = player.GetComponent<Rigidbody2D>();
            if (rb2d != null) 
            {
                rb2d.linearVelocity = Vector2.zero;
                rb2d.angularVelocity = 0f;
            }
            
            // Stop character controller movement
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                // Character controller doesn't have velocity property, but stopping input will stop movement
                Debug.Log("[GameOverManager] Found CharacterController - movement stopped via input disable");
            }
        }
    }
    
    void SetupUI()
    {
        // Create black fade panel
        CreateBlackFadePanel();
        
        if (gameOverPanel != null)
        {
            panelCanvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
                panelCanvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
            
            panelCanvasGroup.alpha = 0f;
            gameOverPanel.SetActive(true);
        }
        
        // Initially hide UI elements
        if (gameOverText != null) gameOverText.color = new Color(gameOverText.color.r, gameOverText.color.g, gameOverText.color.b, 0f);
        if (restartButton != null) restartButton.gameObject.SetActive(false);
        if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(false);
        
        // Setup button events
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
    }
    
    void CreateBlackFadePanel()
    {
        // Find the main Canvas in the scene (not the GameOverPanel's canvas)
        Canvas mainCanvas = FindMainCanvas();
        
        if (mainCanvas == null)
        {
            Debug.LogError("[GameOverManager] Could not find main Canvas for black fade panel");
            return;
        }
        
        blackFadePanel = new GameObject("BlackFadePanel");
        blackFadePanel.transform.SetParent(mainCanvas.transform, false);
        
        // Add RectTransform for full screen coverage
        RectTransform rect = blackFadePanel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        
        // Add Image component for black color
        Image image = blackFadePanel.AddComponent<Image>();
        image.color = Color.black;
        
        // Add CanvasGroup for alpha control
        blackFadeCanvasGroup = blackFadePanel.AddComponent<CanvasGroup>();
        blackFadeCanvasGroup.alpha = 0f;
        
        // Set as highest priority to render on top
        blackFadePanel.transform.SetAsLastSibling();
    }
    
    private Canvas FindMainCanvas()
    {
        // Look for a Canvas that's not part of the GameOverPanel
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        
        foreach (Canvas canvas in allCanvases)
        {
            // Skip the GameOverPanel's canvas
            if (canvas.gameObject == gameOverPanel)
                continue;
                
            // Prefer a canvas that's rendering to screen space overlay
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return canvas;
        }
        
        // If no overlay canvas found, return the first canvas that's not the game over panel
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.gameObject != gameOverPanel)
                return canvas;
        }
        
        // If still nothing found, create a new canvas
        GameObject canvasObj = new GameObject("MainCanvas");
        Canvas newCanvas = canvasObj.AddComponent<Canvas>();
        newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        return newCanvas;
    }
    
    void StartGameOverSequence()
    {
        StartCoroutine(GameOverCutscene());
    }
    
    IEnumerator GameOverCutscene()
    {
        // Play game over sound
        if (audioSource != null && gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }
        
        // Fade to black first - use unscaled time since we paused the game
        yield return StartCoroutine(FadeToBlack());
        
        // Wait a moment in black
        yield return new WaitForSecondsRealtime(0.5f);
        
        // Make sure the GameOverPanel's canvas is on top
        if (gameOverPanel != null)
        {
            Canvas gameOverCanvas = gameOverPanel.GetComponent<Canvas>();
            if (gameOverCanvas != null)
            {
                gameOverCanvas.sortingOrder = 100; // High sorting order to be on top
            }
        }
        
        // Fade in panel (this will be over the black background)
        yield return StartCoroutine(FadeInPanel());
        
        // Show game over text
        yield return new WaitForSecondsRealtime(textDelay);
        yield return StartCoroutine(FadeInTextMeshPro(gameOverText));
        
        // Show buttons
        yield return new WaitForSecondsRealtime(buttonDelay);
        if (restartButton != null) 
        {
            restartButton.gameObject.SetActive(true);
            yield return StartCoroutine(FadeInButton(restartButton));
        }
        
        yield return new WaitForSecondsRealtime(0.3f);
        if (mainMenuButton != null)
        {
            mainMenuButton.gameObject.SetActive(true);
            yield return StartCoroutine(FadeInButton(mainMenuButton));
        }
        
        Debug.Log("[GameOverManager] Game over sequence complete - buttons should be interactable now");
    }
    
    IEnumerator FadeInTextMeshPro(TextMeshProUGUI textComponent)
    {
        float elapsedTime = 0f;
        float duration = 1f;
        Color originalColor = textComponent.color;
        
        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            textComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time
            yield return null;
        }
        textComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
    }
    
    IEnumerator FadeToBlack()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeToBlackDuration)
        {
            blackFadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeToBlackDuration);
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time
            yield return null;
        }
        blackFadeCanvasGroup.alpha = 1f;
    }
    
    IEnumerator FadeInPanel()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            panelCanvasGroup.alpha = Mathf.Lerp(0f, 0.8f, elapsedTime / fadeInDuration);
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time
            yield return null;
        }
        panelCanvasGroup.alpha = 0.8f;
    }
    
    IEnumerator FadeInButton(Button button)
    {
        CanvasGroup buttonCanvasGroup = button.GetComponent<CanvasGroup>();
        if (buttonCanvasGroup == null)
            buttonCanvasGroup = button.gameObject.AddComponent<CanvasGroup>();
        
        buttonCanvasGroup.alpha = 0f;
        float elapsedTime = 0f;
        float duration = 0.5f;
        
        while (elapsedTime < duration)
        {
            buttonCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time
            yield return null;
        }
        buttonCanvasGroup.alpha = 1f;
    }
    
    public void RestartGame()
    {
        Debug.Log($"[GameOverManager] Attempting to restart game. Loading scene: {gameSceneName}");
        
        // Reset time scale before loading new scene
        Time.timeScale = 1f;
        
        // Try to load the specified game scene
        try
        {
            SceneManager.LoadScene(gameSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameOverManager] Failed to load scene '{gameSceneName}': {e.Message}");
            // Fallback: reload current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    public void GoToMainMenu()
    {
        Debug.Log($"[GameOverManager] Attempting to go to main menu. Loading scene: {mainMenuSceneName}");
        
        // Reset time scale before loading new scene
        Time.timeScale = 1f;
        
        // Try to load the main menu scene
        try
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameOverManager] Failed to load scene '{mainMenuSceneName}': {e.Message}");
            // Fallback: try build index 0 (usually main menu)
            SceneManager.LoadScene(0);
        }
    }
}
