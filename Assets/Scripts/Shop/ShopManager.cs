using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Simplified shop manager that displays UpgradeData directly
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }
    
    [Header("Shop UI References")]
    public GameObject shopMenuPanel;
    public Button closeButton;
    //public Text shopTitle;
    
    [Header("Upgrade Display")]
    [Tooltip("Container where upgrade items will be spawned")]
    public Transform upgradeContainer;
    [Tooltip("Prefab with Image, Text fields, and Button")]
    public GameObject upgradeItemPrefab;
    [Tooltip("Number of upgrade options to show at once")]
    public int numberOfUpgradeOptions = 3;
    
    [Header("References")]
    [Tooltip("Will auto-find player's UpgradeManager if not assigned")]
    public UpgradeManager upgradeManager;
    
    [Header("Shop Settings")]
    public string shopName = "Merchant Shop";
    
    [Header("Audio")]
    public AudioClip purchaseSound;
    public AudioClip openShopSound;
    public AudioClip closeShopSound;
    
    private bool isShopOpen = false;
    private List<GameObject> currentUpgradeItems = new List<GameObject>();
    private AudioSource audioSource;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }
    
    void Start()
    {
        if (shopMenuPanel != null)
        {
            shopMenuPanel.SetActive(false);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShop);
        }
        
        /*if (shopTitle != null)
        {
            shopTitle.text = shopName;
        }*/
        
        // Auto-find UpgradeManager on player
        if (upgradeManager == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                upgradeManager = player.GetComponent<UpgradeManager>();
            }
            
            if (upgradeManager == null)
            {
                Debug.LogWarning("ShopManager: No UpgradeManager found on player!");
            }
        }
    }
    
    public void OpenShop()
    {
        if (!isShopOpen)
        {
            isShopOpen = true;
            
            if (shopMenuPanel != null)
            {
                shopMenuPanel.SetActive(true);
            }
            
            if (audioSource != null && openShopSound != null)
            {
                audioSource.PlayOneShot(openShopSound);
            }
            
            RefreshUpgradeDisplay();
            
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    public void CloseShop()
    {
        if (isShopOpen)
        {
            isShopOpen = false;
            
            if (shopMenuPanel != null)
            {
                shopMenuPanel.SetActive(false);
            }
            
            if (audioSource != null && closeShopSound != null)
            {
                audioSource.PlayOneShot(closeShopSound);
            }
            
            ClearUpgradeDisplay();
            
            Time.timeScale = 1f;
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
    
    /// <summary>
    /// Populates the shop with available upgrades
    /// </summary>
    public void RefreshUpgradeDisplay()
    {
        if (upgradeManager == null || upgradeContainer == null || upgradeItemPrefab == null)
        {
            Debug.LogWarning("ShopManager: Missing references - cannot display upgrades");
            return;
        }
        
        ClearUpgradeDisplay();
        
        List<UpgradeData> availableUpgrades = upgradeManager.GetRandomAvailableUpgrades(numberOfUpgradeOptions);
        
        if (availableUpgrades.Count == 0)
        {
            Debug.Log("ShopManager: No available upgrades");
            return;
        }
        
        // Create UI for each upgrade
        foreach (UpgradeData upgrade in availableUpgrades)
        {
            GameObject itemObj = Instantiate(upgradeItemPrefab, upgradeContainer);
            SetupUpgradeItem(itemObj, upgrade);
            currentUpgradeItems.Add(itemObj);
        }
    }
    
    /// <summary>
    /// Sets up a single upgrade item UI directly from UpgradeData
    /// </summary>
    private void SetupUpgradeItem(GameObject itemObj, UpgradeData upgrade)
    {
        // Find UI components by name (standard naming in prefab)
        Image iconImage = itemObj.transform.Find("Icon")?.GetComponent<Image>();
        Text nameText = itemObj.transform.Find("NameText")?.GetComponent<Text>();
        Text descriptionText = itemObj.transform.Find("DescriptionText")?.GetComponent<Text>();
        Text stackText = itemObj.transform.Find("StackCountText")?.GetComponent<Text>();
        Button button = itemObj.transform.Find("PurchaseButton")?.GetComponent<Button>();
        Text buttonText = button?.GetComponentInChildren<Text>();
        
        // Set icon from UpgradeData
        if (iconImage != null && upgrade.icon != null)
        {
            iconImage.sprite = upgrade.icon;
        }
        
        // Set name from UpgradeData
        if (nameText != null)
        {
            nameText.text = upgrade.upgradeName;
        }
        
        // Set description from UpgradeData
        if (descriptionText != null)
        {
            descriptionText.text = upgrade.description;
        }
        
        // Set stack count
        int currentStack = upgradeManager.GetUpgradeStack(upgrade);
        if (stackText != null)
        {
            if (upgrade.isStackable && upgrade.maxStacks > 1)
            {
                stackText.text = $"Stack: {currentStack}/{upgrade.maxStacks}";
            }
            else
            {
                stackText.text = "";
            }
        }
        
        // Setup button
        if (button != null)
        {
            bool canAcquire = upgradeManager.CanAcquireUpgrade(upgrade);
            button.interactable = canAcquire;
            
            if (buttonText != null)
            {
                if (currentStack == 0)
                {
                    buttonText.text = "Acquire";
                }
                else if (canAcquire)
                {
                    buttonText.text = "Stack";
                }
                else
                {
                    buttonText.text = "Max";
                }
            }
            
            // Add click listener
            button.onClick.AddListener(() => PurchaseUpgrade(upgrade));
        }
    }
    
    /// <summary>
    /// Attempts to purchase an upgrade
    /// </summary>
    private void PurchaseUpgrade(UpgradeData upgrade)
    {
        if (upgradeManager == null || upgrade == null)
        {
            return;
        }
        
        bool success = upgradeManager.AcquireUpgrade(upgrade);
        
        if (success)
        {
            if (audioSource != null && purchaseSound != null)
            {
                audioSource.PlayOneShot(purchaseSound);
            }
            
            Debug.Log($"Purchased: {upgrade.upgradeName}");
            RefreshUpgradeDisplay();
        }
    }
    
    /// <summary>
    /// Clears all displayed upgrade items
    /// </summary>
    private void ClearUpgradeDisplay()
    {
        foreach (GameObject item in currentUpgradeItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        currentUpgradeItems.Clear();
    }
    
    void Update()
    {
        if (isShopOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }
    
    void OnDestroy()
    {
        ClearUpgradeDisplay();
    }
}
