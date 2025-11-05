using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject gameOverPanel;
    public Text gameOverText;
    public Text finalScoreText;
    public Button restartButton;
    public Button mainMenuButton;
    
    [Header("Cutscene Settings")]
    public float fadeInDuration = 2f;
    public float textDelay = 1f;
    public float buttonDelay = 3f;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip gameOverSound;
    
    private CanvasGroup panelCanvasGroup;
    
    void Start()
    {
        SetupUI();
        StartGameOverSequence();
    }
    
    void SetupUI()
    {
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
        if (finalScoreText != null) finalScoreText.color = new Color(finalScoreText.color.r, finalScoreText.color.g, finalScoreText.color.b, 0f);
        if (restartButton != null) restartButton.gameObject.SetActive(false);
        if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(false);
        
        // Setup button events
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
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
        
        // Fade in panel
        yield return StartCoroutine(FadeInPanel());
        
        // Show game over text
        yield return new WaitForSeconds(textDelay);
        yield return StartCoroutine(FadeInText(gameOverText));
        
        // Show final score
        yield return new WaitForSeconds(0.5f);
        if (finalScoreText != null)
        {
            // Get score from PlayerPrefs or GameManager
            int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
            finalScoreText.text = "Final Score: " + finalScore;
            yield return StartCoroutine(FadeInText(finalScoreText));
        }
        
        // Show buttons
        yield return new WaitForSeconds(buttonDelay);
        if (restartButton != null) 
        {
            restartButton.gameObject.SetActive(true);
            yield return StartCoroutine(FadeInButton(restartButton));
        }
        
        yield return new WaitForSeconds(0.3f);
        if (mainMenuButton != null)
        {
            mainMenuButton.gameObject.SetActive(true);
            yield return StartCoroutine(FadeInButton(mainMenuButton));
        }
    }
    
    IEnumerator FadeInPanel()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            panelCanvasGroup.alpha = Mathf.Lerp(0f, 0.8f, elapsedTime / fadeInDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        panelCanvasGroup.alpha = 0.8f;
    }
    
    IEnumerator FadeInText(Text textComponent)
    {
        float elapsedTime = 0f;
        float duration = 1f;
        Color originalColor = textComponent.color;
        
        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            textComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        textComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
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
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        buttonCanvasGroup.alpha = 1f;
    }
    
    public void RestartGame()
    {
        // Load the main game scene
        SceneManager.LoadScene("GameScene"); // Replace with your game scene name
    }
    
    public void GoToMainMenu()
    {
        // Load the main menu scene
        SceneManager.LoadScene("MainMenu"); // Replace with your main menu scene name
    }
}
