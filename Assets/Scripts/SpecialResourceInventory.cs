using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SpecialResourceInventory : MonoBehaviour
{
    private GameObject inventoryPanel;
    private GameObject inventoryButton;
    private bool isShowing = false;
    private ResourceManager resourceManager;
    private Transform inventoryContainer;
    private Dictionary<string, GameObject> inventoryUIItems = new Dictionary<string, GameObject>();
    private bool isInitialized = false;
    
    // Singleton pattern
    private static SpecialResourceInventory instance;
    public static SpecialResourceInventory Instance
    {
        get
        {
            if (instance == null)
            {
                // Look for existing instance first
                instance = FindObjectOfType<SpecialResourceInventory>();
                if (instance == null)
                {
                    // Create new instance
                    GameObject inventoryObj = new GameObject("SpecialResourceInventory");
                    instance = inventoryObj.AddComponent<SpecialResourceInventory>();
                    // REMOVED DontDestroyOnLoad to fix scene transition issues
                    Debug.Log("SpecialResourceInventory auto-created");
                }
            }
            return instance;
        }
    }
    
    void Awake()
    {
        Debug.Log($"SpecialResourceInventory Awake called on {gameObject.name}");
        
        // Simplified singleton pattern - no DontDestroyOnLoad
        if (instance == null)
        {
            instance = this;
            Debug.Log($"SpecialResourceInventory instance set to {gameObject.name}");
        }
        else if (instance != this)
        {
            Debug.Log($"Destroying duplicate SpecialResourceInventory on {gameObject.name}");
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Only proceed if this is the valid instance
        if (instance != this) 
        {
            Debug.Log("Skipping Start because this is not the instance");
            return;
        }
        
        if (isInitialized)
        {
            Debug.Log("Already initialized, skipping");
            return;
        }
        
        Debug.Log("Starting SpecialResourceInventory initialization");
        
        resourceManager = FindObjectOfType<ResourceManager>();
        
        // Initialize immediately instead of using coroutine
        InitializeUI();
        
        isInitialized = true;
        
        // Start continuous updating when showing
        InvokeRepeating("UpdateInventoryIfShowing", 0.1f, 0.1f);
    }
    
    // FIX 1: Add continuous updating method
    private void UpdateInventoryIfShowing()
    {
        if (isShowing && inventoryPanel != null && inventoryPanel.activeInHierarchy)
        {
            UpdateInventoryDisplay();
        }
    }
    
    private void InitializeUI()
    {
        Debug.Log("InitializeUI called");
        
        // Clean up any existing UI first to avoid duplicates
        if (inventoryButton != null)
        {
            Destroy(inventoryButton);
            inventoryButton = null;
        }
        if (inventoryPanel != null)
        {
            Destroy(inventoryPanel);
            inventoryPanel = null;
        }
        inventoryContainer = null;
        
        try
        {
            CreateInventoryButton();
            CreateInventoryPanel();
            
            // Immediate verification
            if (inventoryContainer == null)
            {
                Debug.LogError("CRITICAL: inventoryContainer is null after InitializeUI!");
            }
            else
            {
                Debug.Log($"SUCCESS: inventoryContainer created: {inventoryContainer.name}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Exception in InitializeUI: {e.Message}");
        }
    }
    
    private void CreateInventoryButton()
    {
        Debug.Log("Creating inventory button");
        
        // Find current scene's canvas (important for scene transitions)
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("Created new Canvas");
        }
        
        // Create button GameObject
        GameObject buttonObj = new GameObject("InventoryButton");
        buttonObj.transform.SetParent(canvas.transform, false); // FALSE is critical for proper scaling
        inventoryButton = buttonObj;
        
        // Add Image component for button background
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 1.0f);
        
        // Add Button component
        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(ToggleInventory);
        
        // Position button in bottom left
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0, 0);
        buttonRect.anchorMax = new Vector2(0, 0);
        buttonRect.pivot = new Vector2(0, 0);
        buttonRect.anchoredPosition = new Vector2(10, 10);
        buttonRect.sizeDelta = new Vector2(100, 40);
        
        // Create button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = "Inventory";
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.fontSize = 14;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        Debug.Log("Inventory button created successfully");
    }
    
    private void CreateInventoryPanel()
    {
        Debug.Log("Creating inventory panel");
        
        // Find current scene's canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) 
        {
            Debug.LogError("No Canvas found for inventory panel!");
            return;
        }
        
        try
        {
            // Create main inventory panel (following the pattern from CraftingUI and CropSwapUI)
            GameObject panelObj = new GameObject("InventoryPanel");
            panelObj.transform.SetParent(canvas.transform, false);
            inventoryPanel = panelObj;
            
            // Make the panel itself cover the full screen (like GameOverSystem)
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // Add background image that covers the entire screen
            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.0f, 0.0f, 0.0f, 0.7f); // Semi-transparent background
            
            // Add Canvas component to control sorting (like other UIs)
            Canvas panelCanvas = panelObj.AddComponent<Canvas>();
            panelCanvas.overrideSorting = true;
            panelCanvas.sortingOrder = 100; // Higher than other UI elements
            
            // Add GraphicRaycaster for button interactions (essential for click blocking)
            panelObj.AddComponent<GraphicRaycaster>();
            
            // Create the actual content container
            GameObject contentContainer = new GameObject("ContentContainer");
            contentContainer.transform.SetParent(panelObj.transform, false);
            
            RectTransform containerRect = contentContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.anchoredPosition = Vector2.zero;
            containerRect.sizeDelta = new Vector2(300, 400);
            
            // Add background for the content area
            Image contentImage = contentContainer.AddComponent<Image>();
            contentImage.color = new Color(0.0f, 0.0f, 0.0f, 1.0f); // Solid black for content area
            
            Debug.Log("Panel base created, creating content...");
            
            // SIMPLIFIED: Create content area with direct setup
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(contentContainer.transform, false);
            
            // Add RectTransform component FIRST
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            
            // CRITICAL: Set inventoryContainer IMMEDIATELY after RectTransform
            inventoryContainer = contentObj.transform;
            
            Debug.Log($"inventoryContainer SET: {inventoryContainer != null}");
            Debug.Log($"inventoryContainer name: {(inventoryContainer != null ? inventoryContainer.name : "NULL")}");
            Debug.Log($"inventoryContainer gameObject: {(inventoryContainer != null ? inventoryContainer.gameObject != null : false)}");
            
            // Now configure the RectTransform
            contentRect.anchorMin = new Vector2(0.05f, 0.15f);
            contentRect.anchorMax = new Vector2(0.95f, 0.85f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            // Add layout AFTER setting container reference
            VerticalLayoutGroup layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.spacing = 5f;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            
            Debug.Log("Content setup complete");
            
            // Create title and close button
            CreateTitle(contentContainer);
            CreateCloseButton(contentContainer);
            
            // Start hidden
            inventoryPanel.SetActive(false);
            
            // FINAL verification
            Debug.Log($"FINAL CHECK - inventoryContainer: {inventoryContainer != null}");
            if (inventoryContainer != null)
            {
                Debug.Log($"SUCCESS: Container name: {inventoryContainer.name}, gameObject active: {inventoryContainer.gameObject.activeInHierarchy}");
            }
            else
            {
                Debug.LogError("CRITICAL: inventoryContainer is null after setup!");
            }
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Exception in CreateInventoryPanel: {e.Message}\n{e.StackTrace}");
            inventoryContainer = null;
            inventoryPanel = null;
        }
    }
    
    private void CreateTitle(GameObject parent)
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(parent.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "Special Resources";
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 18;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.fontStyle = FontStyle.Bold;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.9f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
    }
    
    private void CreateCloseButton(GameObject parent)
    {
        GameObject closeButtonObj = new GameObject("CloseButton");
        closeButtonObj.transform.SetParent(parent.transform, false);
        
        Image closeButtonImage = closeButtonObj.AddComponent<Image>();
        closeButtonImage.color = new Color(0.6f, 0.2f, 0.2f, 0.8f);
        
        Button closeButton = closeButtonObj.AddComponent<Button>();
        closeButton.onClick.AddListener(HideInventory);
        
        RectTransform closeButtonRect = closeButtonObj.GetComponent<RectTransform>();
        closeButtonRect.anchorMin = new Vector2(0.2f, 0.02f);
        closeButtonRect.anchorMax = new Vector2(0.8f, 0.12f);
        closeButtonRect.offsetMin = Vector2.zero;
        closeButtonRect.offsetMax = Vector2.zero;
        
        GameObject closeTextObj = new GameObject("CloseText");
        closeTextObj.transform.SetParent(closeButtonObj.transform, false);
        Text closeText = closeTextObj.AddComponent<Text>();
        closeText.text = "Close";
        closeText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        closeText.fontSize = 14;
        closeText.color = Color.white;
        closeText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform closeTextRect = closeTextObj.GetComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;
    }
    
    private void CreateInventoryItem(string resourceName, int quantity)
    {
        if (inventoryContainer == null)
        {
            Debug.LogError($"Cannot create inventory item for {resourceName}: inventoryContainer is null!");
            return;
        }
        
        Debug.Log($"Creating inventory item for {resourceName} with quantity {quantity}");
        
        GameObject itemObj = new GameObject($"Item_{resourceName}");
        itemObj.transform.SetParent(inventoryContainer, false);
        
        RectTransform itemRect = itemObj.AddComponent<RectTransform>();
        itemRect.sizeDelta = new Vector2(0, 35);
        itemRect.localScale = Vector3.one;
        
        LayoutElement layoutElement = itemObj.AddComponent<LayoutElement>();
        layoutElement.minHeight = 35f;
        layoutElement.preferredHeight = 35f;
        layoutElement.flexibleWidth = 1f;
        
        Image bg = itemObj.AddComponent<Image>();
        bg.color = new Color(0.3f, 0.3f, 0.3f, 1.0f);
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(itemObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, 0);
        
        Text text = textObj.AddComponent<Text>();
        text.text = $"{GetDisplayName(resourceName)}: {quantity}";
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 14;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;
        
        inventoryUIItems[resourceName] = itemObj;
        
        Debug.Log($"Inventory item created successfully for {resourceName}");
    }
    
    private string GetDisplayName(string resourceName)
    {
        switch (resourceName.ToLower())
        {
            case "whitegem": return "White Gem";
            case "redgem": return "Red Gem";
            case "bluegem": return "Blue Gem";
            case "greengem": return "Green Gem";
            case "gold": return "Gold";
            case "divineseal": return "Divine Seal";
            default:
                if (string.IsNullOrEmpty(resourceName)) return resourceName;
                return char.ToUpper(resourceName[0]) + resourceName.Substring(1).ToLower();
        }
    }
    
    private void UpdateInventoryDisplay()
    {
        if (resourceManager == null) 
        {
            Debug.LogError("ResourceManager is null in UpdateInventoryDisplay!");
            return;
        }
        
        if (inventoryContainer == null)
        {
            Debug.LogError("inventoryContainer is null in UpdateInventoryDisplay!");
            // Try to recreate just the content part
            if (inventoryPanel != null)
            {
                // Look for ContentContainer first, then Content inside it
                Transform contentContainerTransform = inventoryPanel.transform.Find("ContentContainer");
                if (contentContainerTransform != null)
                {
                    Transform contentTransform = contentContainerTransform.Find("Content");
                    if (contentTransform != null)
                    {
                        inventoryContainer = contentTransform;
                        Debug.Log("Recovered inventoryContainer from existing panel");
                    }
                    else
                    {
                        Debug.LogError("Could not find Content in ContentContainer, recreating panel...");
                        CreateInventoryPanel();
                        return;
                    }
                }
                else
                {
                    Debug.LogError("Could not find ContentContainer in panel, recreating panel...");
                    CreateInventoryPanel();
                    return;
                }
            }
            else
            {
                Debug.LogError("Could not recover inventoryContainer - recreating panel");
                CreateInventoryPanel();
                return;
            }
        }
        
        Dictionary<string, Resource> specialResources = resourceManager.GetSpecialResources();
        
        // Clean up destroyed items
        List<string> toRemove = new List<string>();
        foreach (var item in inventoryUIItems)
        {
            if (item.Value == null)
            {
                toRemove.Add(item.Key);
            }
        }
        foreach (string name in toRemove)
        {
            inventoryUIItems.Remove(name);
        }
        
        // FIX 1: Remove items with 0 quantity
        List<string> toRemoveZero = new List<string>();
        foreach (var item in inventoryUIItems)
        {
            string resourceName = item.Key;
            if (!specialResources.ContainsKey(resourceName) || specialResources[resourceName].quantity < 1)
            {
                if (item.Value != null)
                {
                    Destroy(item.Value);
                }
                toRemoveZero.Add(resourceName);
            }
        }
        foreach (string name in toRemoveZero)
        {
            inventoryUIItems.Remove(name);
        }
        
        // Add or update items
        foreach (var resourcePair in specialResources)
        {
            string resourceName = resourcePair.Key;
            Resource resource = resourcePair.Value;
            
            if (resource.quantity >= 1)
            {
                if (!inventoryUIItems.ContainsKey(resourceName) || inventoryUIItems[resourceName] == null)
                {
                    CreateInventoryItem(resourceName, resource.quantity);
                }
                else
                {
                    // Update existing item
                    GameObject item = inventoryUIItems[resourceName];
                    Text text = item.GetComponentInChildren<Text>();
                    if (text != null)
                    {
                        text.text = $"{GetDisplayName(resourceName)}: {resource.quantity}";
                    }
                }
            }
        }
    }
    
    private void ToggleInventory()
    {
        if (isShowing)
        {
            HideInventory();
        }
        else
        {
            ShowInventory();
        }
    }
    
    private void ShowInventory()
    {
        Debug.Log("ShowInventory called");
        
        // SAFETY: Always try to ensure we have working UI
        bool needsRecreation = (inventoryButton == null || inventoryPanel == null || inventoryContainer == null);
        
        if (needsRecreation)
        {
            Debug.Log("UI missing, recreating...");
            InitializeUI();
            
            // Double-check after recreation
            if (inventoryContainer == null)
            {
                Debug.LogError("FAILED to create inventoryContainer! Cannot show inventory.");
                return;
            }
        }
        
        // Ensure ResourceManager
        if (resourceManager == null)
        {
            resourceManager = FindObjectOfType<ResourceManager>();
            if (resourceManager == null)
            {
                Debug.LogError("ResourceManager not found! Cannot show inventory.");
                return;
            }
            Debug.Log($"ResourceManager found: {resourceManager != null}");
        }
        
        // Final safety check before showing
        if (inventoryPanel != null && inventoryContainer != null)
        {
            Debug.Log("All components ready, showing inventory...");
            try
            {
                UpdateInventoryDisplay();
                inventoryPanel.SetActive(true);
                isShowing = true;
                Debug.Log("Special Resource Inventory opened successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error in ShowInventory: {e.Message}");
                isShowing = false;
            }
        }
        else
        {
            Debug.LogError($"Cannot show inventory - panel: {inventoryPanel != null}, container: {inventoryContainer != null}");
        }
    }
    
    private void HideInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            isShowing = false;
            Debug.Log("Special Resource Inventory closed");
        }
    }
    
    public bool IsShowing()
    {
        return isShowing;
    }
}