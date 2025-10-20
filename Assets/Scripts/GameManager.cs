using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles overall game state management and scene transitions
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameSceneName = "DemoScene";
    [SerializeField] private string creditsSceneName = "Credits";
    
    public static GameManager Instance { get; private set; }
    
    // Game state enumeration
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Credits,
        GameOver
    }
    
    [SerializeField] private GameState currentState = GameState.MainMenu;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Set initial game state based on current scene
        UpdateGameStateFromScene();
    }
    
    /// <summary>
    /// Update game state based on current scene
    /// </summary>
    private void UpdateGameStateFromScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        if (currentSceneName == mainMenuSceneName)
            SetGameState(GameState.MainMenu);
        else if (currentSceneName == gameSceneName)
            SetGameState(GameState.Playing);
        else if (currentSceneName == creditsSceneName)
            SetGameState(GameState.Credits);
    }
    
    /// <summary>
    /// Set the current game state
    /// </summary>
    public void SetGameState(GameState newState)
    {
        GameState previousState = currentState;
        currentState = newState;
        
        Debug.Log($"Game state changed from {previousState} to {newState}");
        
        // Handle state-specific logic
        switch (newState)
        {
            case GameState.MainMenu:
                HandleMainMenuState();
                break;
            case GameState.Playing:
                HandlePlayingState();
                break;
            case GameState.Paused:
                HandlePausedState();
                break;
            case GameState.Credits:
                HandleCreditsState();
                break;
            case GameState.GameOver:
                HandleGameOverState();
                break;
        }
    }
    
    /// <summary>
    /// Get the current game state
    /// </summary>
    public GameState GetGameState()
    {
        return currentState;
    }
    
    /// <summary>
    /// Handle main menu state
    /// </summary>
    private void HandleMainMenuState()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    /// <summary>
    /// Handle playing state
    /// </summary>
    private void HandlePlayingState()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    /// <summary>
    /// Handle paused state
    /// </summary>
    private void HandlePausedState()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    /// <summary>
    /// Handle credits state
    /// </summary>
    private void HandleCreditsState()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    /// <summary>
    /// Handle game over state
    /// </summary>
    private void HandleGameOverState()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    /// <summary>
    /// Load a scene by name
    /// </summary>
    public void LoadScene(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// Load the main menu scene
    /// </summary>
    public void LoadMainMenu()
    {
        LoadScene(mainMenuSceneName);
    }
    
    /// <summary>
    /// Load the game scene
    /// </summary>
    public void LoadGameScene()
    {
        LoadScene(gameSceneName);
    }
    
    /// <summary>
    /// Load the credits scene
    /// </summary>
    public void LoadCreditsScene()
    {
        LoadScene(creditsSceneName);
    }
    
    /// <summary>
    /// Quit the application
    /// </summary>
    public void QuitApplication()
    {
        Debug.Log("Quitting application...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    /// <summary>
    /// Called when a scene is loaded
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateGameStateFromScene();
    }
}