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
    public Text shopTitle;
    
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
        Debug.Log("=== ShopManager Start() ===");
        
        if (shopMenuPanel != null)
        {
            shopMenuPanel.SetActive(false);
            Debug.Log("Shop panel initialized (hidden)");
        }
        else
        {
            Debug.LogError("Shop Menu Panel is not assigned in Inspector!");
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShop);
            Debug.Log("Close button listener added");
        }
        else
        {
            Debug.LogWarning("Close Button is not assigned in Inspector");
        }
        
        if (shopTitle != null)
        {
            shopTitle.text = shopName;
            Debug.Log($"Shop title set to: {shopName}");
        }
        
        // Auto-find UpgradeManager on player
        if (upgradeManager == null)
        {
            Debug.Log("UpgradeManager not assigned, searching for Player...");
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                Debug.Log($"Found player: {player.name}");
                upgradeManager = player.GetComponent<UpgradeManager>();
                
                if (upgradeManager != null)
                {
                    Debug.Log("Found UpgradeManager on player!");
                }
                else
                {
                    Debug.LogError("Player found but has no UpgradeManager component!");
                }
            }
            else
            {
                Debug.LogError("No GameObject with tag 'Player' found in scene!");
            }
        }
        else
        {
            Debug.Log("UpgradeManager already assigned in Inspector");
        }
        
        // Validate all critical references
        if (upgradeContainer == null)
        {
            Debug.LogError("Upgrade Container is not assigned in Inspector!");
        }
        else
        {
            Debug.Log("Upgrade Container assigned");
        }
        
        if (upgradeItemPrefab == null)
        {
            Debug.LogError("Upgrade Item Prefab is not assigned in Inspector!");
        }
        else
        {
            Debug.Log($"Upgrade Item Prefab assigned: {upgradeItemPrefab.name}");
        }
        
        Debug.Log("=== ShopManager Start() complete ===");
    }
    
    public void OpenShop()
    {
        Debug.Log("=== OpenShop called ===");
        
        if (!isShopOpen)
        {
            isShopOpen = true;
            
            if (shopMenuPanel != null)
            {
                shopMenuPanel.SetActive(true);
                Debug.Log("Shop panel activated");
            }
            else
            {
                Debug.LogError("Shop Menu Panel is NULL!");
            }
            
            if (audioSource != null && openShopSound != null)
            {
                audioSource.PlayOneShot(openShopSound);
            }
            
            Debug.Log("About to call RefreshUpgradeDisplay()");
            RefreshUpgradeDisplay();
            
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            Debug.Log("Shop opened successfully");
        }
        else
        {
            Debug.Log("Shop already open, ignoring OpenShop call");
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
        Debug.Log("=== RefreshUpgradeDisplay called ===");
        
        if (upgradeManager == null)
        {
            Debug.LogError("UpgradeManager is NULL! Cannot display upgrades.");
            return;
        }
        
        if (upgradeContainer == null)
        {
            Debug.LogError("Upgrade Container is NULL! Cannot display upgrades.");
            return;
        }
        
        if (upgradeItemPrefab == null)
        {
            Debug.LogError("Upgrade Item Prefab is NULL! Cannot display upgrades.");
            return;
        }
        
        Debug.Log("All references valid, proceeding...");
        
        ClearUpgradeDisplay();
        
        Debug.Log($"Getting {numberOfUpgradeOptions} random upgrades from UpgradeManager...");
        List<UpgradeData> availableUpgrades = upgradeManager.GetRandomAvailableUpgrades(numberOfUpgradeOptions);
        
        Debug.Log($"Received {availableUpgrades.Count} upgrades from UpgradeManager");
        
        if (availableUpgrades.Count == 0)
        {
            Debug.LogWarning("No available upgrades returned from UpgradeManager!");
            return;
        }
        
        // Create UI for each upgrade
        foreach (UpgradeData upgrade in availableUpgrades)
        {
            Debug.Log($"Creating UI for upgrade: {upgrade.upgradeName}");
            
            GameObject itemObj = Instantiate(upgradeItemPrefab, upgradeContainer);
            Debug.Log($"Instantiated prefab, setting up...");
            
            SetupUpgradeItem(itemObj, upgrade);
            currentUpgradeItems.Add(itemObj);
            
            Debug.Log($"Upgrade item created for: {upgrade.upgradeName}");
        }
        
        Debug.Log($"=== RefreshUpgradeDisplay complete: {currentUpgradeItems.Count} items created ===");
    }
    
    /// <summary>
    /// Sets up a single upgrade item UI directly from UpgradeData
    /// </summary>
    private void SetupUpgradeItem(GameObject itemObj, UpgradeData upgrade)
    {
        Debug.Log($"=== SetupUpgradeItem called for: {upgrade.upgradeName} ===");
        
        // Find UI components by name (searches recursively through all children)
        Debug.Log("Searching for Icon component...");
        Image iconImage = FindChildComponent<Image>(itemObj.transform, "Icon");
        
        Debug.Log("Searching for NameText component...");
        Text nameText = FindChildComponent<Text>(itemObj.transform, "NameText");
        
        Debug.Log("Searching for DescriptionText component...");
        Text descriptionText = FindChildComponent<Text>(itemObj.transform, "DescriptionText");
        
        Debug.Log("Searching for StackCountText component...");
        Text stackText = FindChildComponent<Text>(itemObj.transform, "StackCountText");
        
        Debug.Log("Searching for PurchaseButton component...");
        Button button = FindChildComponent<Button>(itemObj.transform, "PurchaseButton");
        Text buttonText = button?.GetComponentInChildren<Text>();
        
        // Set icon from UpgradeData
        if (iconImage != null && upgrade.icon != null)
        {
            iconImage.sprite = upgrade.icon;
            Debug.Log($"Set icon for {upgrade.upgradeName}");
        }
        else if (iconImage == null)
        {
            Debug.LogWarning($"Could not find 'Icon' Image component in prefab for {upgrade.upgradeName}");
        }
        else if (upgrade.icon == null)
        {
            Debug.LogWarning($"Upgrade {upgrade.upgradeName} has no icon assigned in UpgradeData!");
        }
        
        // Set name from UpgradeData
        if (nameText != null)
        {
            nameText.text = upgrade.upgradeName;
            Debug.Log($"Set name: {upgrade.upgradeName}");
        }
        else
        {
            Debug.LogWarning($"Could not find 'NameText' Text component in prefab");
        }
        
        // Set description from UpgradeData
        if (descriptionText != null)
        {
            descriptionText.text = upgrade.description;
        }
        else
        {
            Debug.LogWarning($"Could not find 'DescriptionText' Text component in prefab");
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
    
    /// <summary>
    /// Helper method to find a component by name recursively through all children
    /// </summary>
    private T FindChildComponent<T>(Transform parent, string name) where T : Component
    {
        // Check direct children first
        Transform child = parent.Find(name);
        if (child != null)
        {
            T component = child.GetComponent<T>();
            if (component != null)
            {
                Debug.Log($"Found '{name}' as direct child with component {typeof(T).Name}");
                return component;
            }
        }
        
        // Search recursively through all descendants
        foreach (Transform t in parent)
        {
            T found = FindChildComponent<T>(t, name);
            if (found != null)
            {
                Debug.Log($"Found '{name}' in descendant: {t.name} with component {typeof(T).Name}");
                return found;
            }
        }
        
        Debug.LogWarning($"Could not find child named '{name}' with component {typeof(T).Name} under {parent.name}");
        return null;
    }
}