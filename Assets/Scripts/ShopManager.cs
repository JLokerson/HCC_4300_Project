using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }
    
    [Header("Shop UI References")]
    public GameObject shopMenuPanel;
    public Button closeButton;
    public Text shopTitle;
    
    [Header("Shop Settings")]
    public string shopName = "Merchant Shop";
    
    private bool isShopOpen = false;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Initialize shop UI
        if (shopMenuPanel != null)
        {
            shopMenuPanel.SetActive(false);
        }
        
        // Setup close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShop);
        }
        
        // Set shop title
        if (shopTitle != null)
        {
            shopTitle.text = shopName;
        }
    }
    
    public void OpenShop()
    {
        if (!isShopOpen)
        {
            isShopOpen = true;
            shopMenuPanel.SetActive(true);
            
            // Pause game time for shop interaction
            Time.timeScale = 0f;
            
            // Enable cursor for UI interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    public void CloseShop()
    {
        if (isShopOpen)
        {
            isShopOpen = false;
            shopMenuPanel.SetActive(false);
            
            // Resume game time
            Time.timeScale = 1f;
            
            // Hide cursor and lock for gameplay
            //Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    public void ToggleShop()
    {
        if (isShopOpen)
        {
            CloseShop();
        }
        else
        {
            OpenShop();
        }
    }
    
    public bool IsShopOpen()
    {
        return isShopOpen;
    }
    
    void Update()
    {
        // Close shop with Escape key
        if (isShopOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }
}
