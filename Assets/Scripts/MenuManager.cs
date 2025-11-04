using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Handles the main menu functionality including navigation between different menu screens
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject creditsPanel;
    
    [Header("Main Menu Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;
    
    [Header("Credits Buttons")]
    [SerializeField] private Button backFromCreditsButton;
    
    [Header("Scene Management")]
    [SerializeField] private string gameSceneName = null; // The name of your main game scene
    
    private void Start()
    {
        InitializeMenu();
        SetupButtonListeners();
    }
    
    /// <summary>
    /// Initialize the menu to show the main menu panel
    /// </summary>
    private void InitializeMenu()
    {
        ShowMainMenu();
    }
    
    /// <summary>
    /// Setup all button click listeners
    /// </summary>
    private void SetupButtonListeners()
    {
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);
        
        if (creditsButton != null)
            creditsButton.onClick.AddListener(ShowCredits);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
        
        if (backFromCreditsButton != null)
            backFromCreditsButton.onClick.AddListener(ShowMainMenu);
    }
    
    /// <summary>
    /// Show the main menu panel and hide others
    /// </summary>
    public void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }
    
    /// <summary>
    /// Show the credits panel and hide others
    /// </summary>
    public void ShowCredits()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        
        if (creditsPanel != null)
            creditsPanel.SetActive(true);
    }

    /// <summary>
    /// Start the game by loading the main game scene
    /// </summary>
    public void StartGame()
    {
        Debug.Log("Starting game...");
        if (gameSceneName != null)
        {
            LoadScene.LoadSelectedScene(gameSceneName);
        }
        else
        {
            Debug.LogError("Game scene name is not set!");
        }
    }
    
    /// <summary>
    /// Quit the game application
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}