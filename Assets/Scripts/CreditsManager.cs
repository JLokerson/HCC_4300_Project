using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the credits screen functionality
/// </summary>
public class CreditsManager : MonoBehaviour
{
    [Header("Credits UI")]
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private ScrollRect creditsScrollRect;
    [SerializeField] private Text creditsText;
    
    [Header("Navigation")]
    [SerializeField] private Button backButton;
    [SerializeField] private string returnSceneName = "MainMenu";
    
    [Header("Auto Scroll Settings")]
    [SerializeField] private bool autoScroll = true;
    [SerializeField] private float scrollSpeed = 0.5f;
    
    [Header("Credits Content")]
    [TextArea(10, 20)]
    [SerializeField] private string creditsContent = @"GAME CREDITS

Development Team:
- Game Designer: Your Name
- Programmer: Your Name
- Artist: Your Name

Special Thanks:
- Unity Technologies
- Community Contributors

Music:
- Background Music: Artist Name
- Sound Effects: Artist Name

Tools Used:
- Unity Engine
- Visual Studio
- Your favorite tools here

Thank you for playing!

© 2025 Your Game Studio";
    
    private void Start()
    {
        InitializeCredits();
        SetupButtonListeners();
    }
    
    private void Update()
    {
        if (autoScroll && creditsScrollRect != null)
        {
            AutoScrollCredits();
        }
        
        // Allow ESC key to return to main menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMainMenu();
        }
    }
    
    /// <summary>
    /// Initialize the credits screen
    /// </summary>
    private void InitializeCredits()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(true);
        
        if (creditsText != null)
            creditsText.text = creditsContent;
        
        // Start at the bottom of the credits
        if (creditsScrollRect != null)
            creditsScrollRect.normalizedPosition = new Vector2(0, 0);
    }
    
    /// <summary>
    /// Setup button listeners
    /// </summary>
    private void SetupButtonListeners()
    {
        if (backButton != null)
            backButton.onClick.AddListener(ReturnToMainMenu);
    }
    
    /// <summary>
    /// Auto scroll the credits upward
    /// </summary>
    private void AutoScrollCredits()
    {
        if (creditsScrollRect.normalizedPosition.y < 1f)
        {
            Vector2 currentPos = creditsScrollRect.normalizedPosition;
            currentPos.y += scrollSpeed * Time.deltaTime;
            creditsScrollRect.normalizedPosition = currentPos;
        }
        else
        {
            // Reset to bottom when reaching the top
            creditsScrollRect.normalizedPosition = new Vector2(0, 0);
        }
    }
    
    /// <summary>
    /// Return to the main menu
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("Returning to main menu from credits...");
        SceneManager.LoadScene(returnSceneName);
    }
    
    /// <summary>
    /// Set the credits content dynamically
    /// </summary>
    public void SetCreditsContent(string newContent)
    {
        creditsContent = newContent;
        if (creditsText != null)
            creditsText.text = creditsContent;
    }
    
    /// <summary>
    /// Toggle auto scroll
    /// </summary>
    public void ToggleAutoScroll()
    {
        autoScroll = !autoScroll;
    }
    
    /// <summary>
    /// Set auto scroll speed
    /// </summary>
    public void SetScrollSpeed(float newSpeed)
    {
        scrollSpeed = Mathf.Clamp(newSpeed, 0f, 2f);
    }
}