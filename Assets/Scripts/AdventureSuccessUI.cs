using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AdventureSuccessUI : MonoBehaviour
{
    private GameObject successPanel;
    private bool isSuccessShown = false;
    
    // Singleton pattern like AdventureGameOverUI
    private static AdventureSuccessUI instance;
    public static AdventureSuccessUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AdventureSuccessUI>();
                if (instance == null)
                {
                    GameObject successObj = new GameObject("AdventureSuccessUI");
                    instance = successObj.AddComponent<AdventureSuccessUI>();
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
        CreateSuccessUI();
    }
    
    private void CreateSuccessUI()
    {
        // Find or create canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create success panel (full screen overlay)
        GameObject panelObj = new GameObject("AdventureSuccessPanel");
        panelObj.transform.SetParent(canvas.transform);
        successPanel = panelObj;
        
        // Add background image with high alpha to block clicks
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.8f);
        panelImage.raycastTarget = true;
        
        // Add Canvas component to control sorting (highest priority)
        Canvas panelCanvas = panelObj.AddComponent<Canvas>();
        panelCanvas.overrideSorting = true;
        panelCanvas.sortingOrder = 1000;
        
        // Add GraphicRaycaster for button interactions
        panelObj.AddComponent<GraphicRaycaster>();
        
        // Set panel to cover entire screen
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Create content container (centered)
        GameObject contentContainer = new GameObject("ContentContainer");
        contentContainer.transform.SetParent(panelObj.transform);
        
        RectTransform containerRect = contentContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(400, 300);
        
        // Add background for content container
        Image contentBg = contentContainer.AddComponent<Image>();
        contentBg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        // Create title text
        GameObject titleObj = new GameObject("SuccessTitle");
        titleObj.transform.SetParent(contentContainer.transform);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "ADVENTURE COMPLETE";
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 28;
        titleText.color = Color.green;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.fontStyle = FontStyle.Bold;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.7f);
        titleRect.anchorMax = new Vector2(1, 0.9f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        // Create success text
        GameObject successObj = new GameObject("SuccessText");
        successObj.transform.SetParent(contentContainer.transform);
        Text successText = successObj.AddComponent<Text>();
        successText.text = "You successfully reached the destination!\nWell done, adventurer.";
        successText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        successText.fontSize = 18;
        successText.color = Color.white;
        successText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform successRect = successObj.GetComponent<RectTransform>();
        successRect.anchorMin = new Vector2(0, 0.4f);
        successRect.anchorMax = new Vector2(1, 0.7f);
        successRect.offsetMin = Vector2.zero;
        successRect.offsetMax = Vector2.zero;
        
        // Create return home button
        GameObject returnButtonObj = new GameObject("ReturnHomeButton");
        returnButtonObj.transform.SetParent(contentContainer.transform);
        
        Image returnButtonImage = returnButtonObj.AddComponent<Image>();
        returnButtonImage.color = new Color(0.2f, 0.6f, 0.2f, 0.8f);
        
        Button returnButton = returnButtonObj.AddComponent<Button>();
        returnButton.onClick.AddListener(ReturnHome);
        
        RectTransform returnButtonRect = returnButtonObj.GetComponent<RectTransform>();
        returnButtonRect.anchorMin = new Vector2(0.2f, 0.1f);
        returnButtonRect.anchorMax = new Vector2(0.8f, 0.3f);
        returnButtonRect.offsetMin = Vector2.zero;
        returnButtonRect.offsetMax = Vector2.zero;
        
        // Create return button text
        GameObject returnTextObj = new GameObject("ReturnText");
        returnTextObj.transform.SetParent(returnButtonObj.transform);
        Text returnText = returnTextObj.AddComponent<Text>();
        returnText.text = "Return Home";
        returnText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        returnText.fontSize = 20;
        returnText.color = Color.white;
        returnText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform returnTextRect = returnTextObj.GetComponent<RectTransform>();
        returnTextRect.anchorMin = Vector2.zero;
        returnTextRect.anchorMax = Vector2.one;
        returnTextRect.offsetMin = Vector2.zero;
        returnTextRect.offsetMax = Vector2.zero;
        
        // Start hidden
        successPanel.SetActive(false);
    }
    
    public void ShowSuccess()
    {
        if (isSuccessShown) return;
        
        isSuccessShown = true;
        
        Debug.Log($"ShowSuccess called - successPanel exists: {successPanel != null}");
        
        // Show the success panel
        if (successPanel != null)
        {
            Debug.Log($"Activating success panel - was active: {successPanel.activeInHierarchy}");
            successPanel.SetActive(true);
            Debug.Log($"Success panel activated - now active: {successPanel.activeInHierarchy}");
            
            // Force canvas update
            Canvas.ForceUpdateCanvases();
            
            // Check if panel has proper Canvas component
            Canvas panelCanvas = successPanel.GetComponent<Canvas>();
            Debug.Log($"Panel canvas exists: {panelCanvas != null}, sorting order: {(panelCanvas != null ? panelCanvas.sortingOrder : -1)}");
        }
        else
        {
            Debug.LogError("successPanel is null in ShowSuccess!");
        }
        
        Debug.Log("Adventure Success: Player reached destination!");
    }
    
    private void ReturnHome()
    {
        Debug.Log("Returning to main scene from successful adventure...");
        
        // Transfer adventure resources to main game ResourceManager
        TransferAdventureResources();
        
        // Use the existing scene management system
        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.ReturnToMainScene();
        }
        else
        {
            // Fallback - try to load scene directly
            try
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
            }
            catch
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }
    }
    
    private void TransferAdventureResources()
    {
        // Get adventure resources
        if (AdventureResourceManager.Instance == null)
        {
            Debug.LogWarning("AdventureResourceManager not found - cannot transfer resources");
            return;
        }
        
        // Get main game ResourceManager (should exist due to DontDestroyOnLoad)
        ResourceManager mainResourceManager = FindObjectOfType<ResourceManager>();
        if (mainResourceManager == null)
        {
            Debug.LogWarning("Main ResourceManager not found - cannot transfer resources");
            return;
        }
        
        Dictionary<string, Resource> adventureResources = AdventureResourceManager.Instance.GetAllAdventureResources();
        
        foreach (var resourcePair in adventureResources)
        {
            string resourceName = resourcePair.Key;
            Resource adventureResource = resourcePair.Value;
            
            if (adventureResource.quantity > 0)
            {
                // Add adventure resources to main game inventory
                mainResourceManager.AddToResource(resourceName, adventureResource.quantity);
                Debug.Log($"Transferred {adventureResource.quantity} {resourceName} to main inventory");
            }
        }
        
        Debug.Log("Adventure resources transferred to main game inventory!");
    }
    
    public bool IsSuccessShown()
    {
        return isSuccessShown;
    }
}