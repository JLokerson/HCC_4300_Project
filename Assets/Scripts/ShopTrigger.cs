using UnityEngine;
using UnityEngine.UI;

public class ShopTrigger : MonoBehaviour
{
    [Header("Interaction Settings")]
    public KeyCode interactionKey = KeyCode.E;
    public string playerTag = "Player";
    
    [Header("Interaction UI")]
    public GameObject interactionPrompt;
    public Text promptText;
    
    [Header("Shop Settings")]
    public string promptMessage = "Press E to open shop";
    
    private bool playerInRange = false;
    private GameObject player;
    
    void Start()
    {
        // Hide interaction prompt initially
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        // Set prompt text
        if (promptText != null)
        {
            promptText.text = promptMessage;
        }
    }
    
    void Update()
    {
        // Check for interaction input when player is in range
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            OpenShop();
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player entered trigger
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            player = other.gameObject;
            ShowInteractionPrompt();
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        // Check if player left trigger
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            player = null;
            HideInteractionPrompt();
        }
    }
    
    // For 3D colliders
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            player = other.gameObject;
            ShowInteractionPrompt();
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            player = null;
            HideInteractionPrompt();
        }
    }
    
    void ShowInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }
    }
    
    void HideInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    void OpenShop()
    {
        // Only open shop if ShopManager exists and shop is not already open
        if (ShopManager.Instance != null && !ShopManager.Instance.IsShopOpen())
        {
            ShopManager.Instance.OpenShop();
            HideInteractionPrompt(); // Hide prompt when shop opens
        }
    }
    
    void OnDisable()
    {
        // Clean up when NPC is disabled
        HideInteractionPrompt();
        playerInRange = false;
        player = null;
    }
}
