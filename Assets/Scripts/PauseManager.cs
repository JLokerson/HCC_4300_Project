using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Handles the pause menu functionality during gameplay
/// </summary>
public class PauseManager : MonoBehaviour
{
    [Header("Pause Menu UI")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private AudioClip pauseSound = null;

    [Header("Pause Menu Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    
    [Header("Scene Management")]
#if UNITY_EDITOR
    [SerializeField] private SceneAsset mainMenuSceneName = null;
#endif
    [SerializeField] private string mainMenuSceneNameString = ""; // Fallback for builds

    private bool isPaused = false;
    private InputAction pauseAction;
    
    // Removed singleton pattern - each scene has its own PauseManager
    
    private void Start()
    {
        InitializePauseMenu();
        SetupButtonListeners();
        SetupInputActions();
    }
    
    private void OnDestroy()
    {
        // Clean up input actions
        if (pauseAction != null)
        {
            pauseAction.performed -= OnPausePressed;
            pauseAction.Disable();
        }
    }
    
    /// <summary>
    /// Initialize the pause menu
    /// </summary>
    private void InitializePauseMenu()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        
        isPaused = false;
        Time.timeScale = 1f; // Ensure time scale is normal
    }
    

    
    /// <summary>
    /// Setup button listeners
    /// </summary>
    private void SetupButtonListeners()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }
    
    /// <summary>
    /// Setup input actions for pause functionality
    /// </summary>
    private void SetupInputActions()
    {
        // Try to find the pause action from the input system
        pauseAction = InputSystem.actions.FindAction("Pause");
        
        if (pauseAction != null)
        {
            pauseAction.performed += OnPausePressed;
            pauseAction.Enable();
        }
        else
        {
            Debug.LogWarning("Pause action not found in Input Actions. Make sure to add a 'Pause' action to your Input Actions asset.");
        }
    }
    
    /// <summary>
    /// Handle pause input
    /// </summary>
    private void OnPausePressed(InputAction.CallbackContext context)
    {
        TogglePause();
    }
    
    /// <summary>
    /// Toggle pause state
    /// </summary>
    public void TogglePause()
    {
        AudioSource.PlayClipAtPoint(pauseSound, Camera.main.transform.position);

        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    
    /// <summary>
    /// Pause the game
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Freeze game time
        
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
        
        // Enable cursor for menu navigation
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("Game paused");
    }
    
    /// <summary>
    /// Resume the game
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Resume game time
        
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        
        
        Cursor.visible = false;
        
        Debug.Log("Game resumed");
    }
    
    /// <summary>
    /// Return to main menu
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Make sure time scale is reset
        Debug.Log("Returning to main menu...");
#if UNITY_EDITOR
        if (mainMenuSceneName != null)
        {
            LoadScene.LoadSelectedScene(mainMenuSceneName);
        }
        else
        {
            LoadScene.LoadSelectedScene(mainMenuSceneNameString);
        }
#else
        LoadScene.LoadSelectedScene(mainMenuSceneNameString);
#endif
    }
    
    /// <summary>
    /// Quit the game
    /// </summary>
    public void QuitGame()
    {
        Time.timeScale = 1f; // Make sure time scale is reset
        Debug.Log("Quitting game...");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
    /// <summary>
    /// Get current pause state
    /// </summary>
    public bool IsPaused()
    {
        return isPaused;
    }
    
    private void Update()
    {
        
    }
}