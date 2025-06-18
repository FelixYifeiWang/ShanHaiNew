using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AdventureSpecialResourceInventory : MonoBehaviour
{
    private GameObject inventoryPanel;
    private GameObject inventoryButton;
    private bool isShowing = false;
    private AdventureResourceManager adventureResourceManager;
    private Transform inventoryContainer;
    private Dictionary<string, GameObject> inventoryUIItems = new Dictionary<string, GameObject>();
    
    // Singleton pattern
    private static AdventureSpecialResourceInventory instance;
    public static AdventureSpecialResourceInventory Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AdventureSpecialResourceInventory>();
                if (instance == null)
                {
                    GameObject inventoryObj = new GameObject("AdventureSpecialResourceInventory");
                    instance = inventoryObj.AddComponent<AdventureSpecialResourceInventory>();
                }
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        adventureResourceManager = AdventureResourceManager.Instance;
        Debug.Log($"Adventure Inventory: AdventureResourceManager found = {adventureResourceManager != null}");
        
        CreateInventoryButton();
        CreateInventoryPanel();
        
        Debug.Log($"Adventure Inventory: UI created - Button: {inventoryButton != null}, Panel: {inventoryPanel != null}, Container: {inventoryContainer != null}");
        
        // Start continuous updating when showing
        InvokeRepeating("UpdateInventoryIfShowing", 0.1f, 0.1f);
    }
    
    private void UpdateInventoryIfShowing()
    {
        if (isShowing && inventoryPanel != null && inventoryPanel.activeInHierarchy)
        {
            UpdateInventoryDisplay();
        }
    }
    
    private void CreateInventoryButton()
    {
        // Find current scene's canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create button GameObject
        GameObject buttonObj = new GameObject("AdventureInventoryButton");
        buttonObj.transform.SetParent(canvas.transform, false);
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
    }
    
    private void CreateInventoryPanel()
    {
        // Find current scene's canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        // Create main inventory panel (full screen overlay)
        GameObject panelObj = new GameObject("AdventureInventoryPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        inventoryPanel = panelObj;
        
        // Full screen background
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Add background image
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.0f, 0.0f, 0.0f, 0.7f);
        
        // Add Canvas component to control sorting
        Canvas panelCanvas = panelObj.AddComponent<Canvas>();
        panelCanvas.overrideSorting = true;
        panelCanvas.sortingOrder = 100;
        
        // Add GraphicRaycaster for button interactions
        panelObj.AddComponent<GraphicRaycaster>();
        
        // Create the content container
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
        contentImage.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        
        // Create content area
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(contentContainer.transform, false);
        
        // Add RectTransform component
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        inventoryContainer = contentObj.transform;
        
        // Configure the RectTransform
        contentRect.anchorMin = new Vector2(0.05f, 0.15f);
        contentRect.anchorMax = new Vector2(0.95f, 0.85f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;
        
        // Add layout
        VerticalLayoutGroup layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.spacing = 5f;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;
        
        // Create title and close button
        CreateTitle(contentContainer);
        CreateCloseButton(contentContainer);
        
        // Start hidden
        inventoryPanel.SetActive(false);
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
        if (inventoryContainer == null) return;
        
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
            default:
                if (string.IsNullOrEmpty(resourceName)) return resourceName;
                return char.ToUpper(resourceName[0]) + resourceName.Substring(1).ToLower();
        }
    }
    
    private void UpdateInventoryDisplay()
    {
        Debug.Log("UpdateInventoryDisplay called");
        
        if (adventureResourceManager == null) 
        {
            Debug.LogError("AdventureResourceManager is null in UpdateInventoryDisplay!");
            return;
        }
        
        if (inventoryContainer == null)
        {
            Debug.LogError("inventoryContainer is null in UpdateInventoryDisplay!");
            return;
        }
        
        Dictionary<string, Resource> specialResources = adventureResourceManager.GetSpecialResources();
        Debug.Log($"Found {specialResources.Count} special resources");
        
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
        
        // Remove items with 0 quantity
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
            
            Debug.Log($"Checking resource: {resourceName}, quantity: {resource.quantity}");
            
            if (resource.quantity >= 1)
            {
                if (!inventoryUIItems.ContainsKey(resourceName) || inventoryUIItems[resourceName] == null)
                {
                    Debug.Log($"Creating inventory item for {resourceName}");
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
        
        Debug.Log($"UpdateInventoryDisplay completed. UI items count: {inventoryUIItems.Count}");
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
        Debug.Log("ShowInventory called in adventure scene");
        
        if (adventureResourceManager == null)
        {
            adventureResourceManager = AdventureResourceManager.Instance;
            Debug.Log($"AdventureResourceManager search result: {adventureResourceManager != null}");
            if (adventureResourceManager == null) 
            {
                Debug.LogError("AdventureResourceManager not found in adventure scene!");
                return;
            }
        }
        
        if (inventoryPanel == null)
        {
            Debug.LogError("inventoryPanel is null!");
            return;
        }
        
        if (inventoryContainer == null)
        {
            Debug.LogError("inventoryContainer is null!");
            return;
        }
        
        Debug.Log("All components ready, showing inventory...");
        UpdateInventoryDisplay();
        inventoryPanel.SetActive(true);
        isShowing = true;
        Debug.Log("Adventure inventory panel activated");
    }
    
    private void HideInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            isShowing = false;
        }
    }
    
    public bool IsShowing()
    {
        return isShowing;
    }
}