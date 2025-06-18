using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CropSwapUI : MonoBehaviour
{
    private GameObject cropSwapPanel;
    private bool isShowing = false;
    private string selectedCropType = "crop1"; // Default selection
    private List<Button> cropButtons = new List<Button>();
    
    // Singleton pattern for easy access
    private static CropSwapUI instance;
    public static CropSwapUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CropSwapUI>();
                if (instance == null)
                {
                    // Create CropSwapUI automatically if it doesn't exist
                    GameObject cropSwapObj = new GameObject("CropSwapUI");
                    instance = cropSwapObj.AddComponent<CropSwapUI>();
                }
            }
            return instance;
        }
    }
    
    void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        CreateCropSwapUI();
    }
    
    private void CreateCropSwapUI()
    {
        // Find the main canvas (not a child canvas)
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Make sure we're using the root canvas, not a child canvas
        while (canvas.transform.parent != null && canvas.transform.parent.GetComponent<Canvas>() != null)
        {
            canvas = canvas.transform.parent.GetComponent<Canvas>();
        }
        
        // Create main crop swap panel
        GameObject panelObj = new GameObject("CropSwapPanel");
        panelObj.transform.SetParent(canvas.transform);
        cropSwapPanel = panelObj;
        
        // Add background image
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 1f); // Dark background
        
        // Add Canvas component to control sorting
        Canvas panelCanvas = panelObj.AddComponent<Canvas>();
        panelCanvas.overrideSorting = true;
        panelCanvas.sortingOrder = 100; // Higher than other UI elements
        
        // Add GraphicRaycaster for button interactions
        panelObj.AddComponent<GraphicRaycaster>();
        
        // Set panel size and position (centered, medium size)
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(350, 250);
        
        // Create title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "Select Crop Type";
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 18;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.8f);
        titleRect.anchorMax = new Vector2(1, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        // Create crop selection buttons area
        GameObject buttonAreaObj = new GameObject("ButtonArea");
        buttonAreaObj.transform.SetParent(panelObj.transform);
        
        // Add RectTransform component for UI objects
        RectTransform buttonAreaRect = buttonAreaObj.AddComponent<RectTransform>();
        buttonAreaRect.anchorMin = new Vector2(0.1f, 0.3f);
        buttonAreaRect.anchorMax = new Vector2(0.9f, 0.75f);
        buttonAreaRect.offsetMin = Vector2.zero;
        buttonAreaRect.offsetMax = Vector2.zero;
        
        // Create crop buttons
        CreateCropButton(buttonAreaObj, "crop1", "Crop 1", 0);
        CreateCropButton(buttonAreaObj, "crop2", "Crop 2", 1);
        CreateCropButton(buttonAreaObj, "crop3", "Crop 3", 2);
        
        // Create Confirm button
        GameObject confirmButtonObj = new GameObject("ConfirmButton");
        confirmButtonObj.transform.SetParent(panelObj.transform);
        
        Image confirmButtonImage = confirmButtonObj.AddComponent<Image>();
        confirmButtonImage.color = new Color(0.2f, 0.6f, 0.2f, 0.8f); // Green background
        
        Button confirmButton = confirmButtonObj.AddComponent<Button>();
        confirmButton.onClick.AddListener(OnConfirmClicked);
        
        RectTransform confirmButtonRect = confirmButtonObj.GetComponent<RectTransform>();
        confirmButtonRect.anchorMin = new Vector2(0.1f, 0.15f);
        confirmButtonRect.anchorMax = new Vector2(0.45f, 0.25f);
        confirmButtonRect.offsetMin = Vector2.zero;
        confirmButtonRect.offsetMax = Vector2.zero;
        
        // Create confirm button text
        GameObject confirmTextObj = new GameObject("ConfirmText");
        confirmTextObj.transform.SetParent(confirmButtonObj.transform);
        Text confirmText = confirmTextObj.AddComponent<Text>();
        confirmText.text = "Confirm";
        confirmText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        confirmText.fontSize = 14;
        confirmText.color = Color.white;
        confirmText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform confirmTextRect = confirmTextObj.GetComponent<RectTransform>();
        confirmTextRect.anchorMin = Vector2.zero;
        confirmTextRect.anchorMax = Vector2.one;
        confirmTextRect.offsetMin = Vector2.zero;
        confirmTextRect.offsetMax = Vector2.zero;
        
        // Create Close button
        GameObject closeButtonObj = new GameObject("CloseButton");
        closeButtonObj.transform.SetParent(panelObj.transform);
        
        Image closeButtonImage = closeButtonObj.AddComponent<Image>();
        closeButtonImage.color = new Color(0.6f, 0.2f, 0.2f, 0.8f); // Red background
        
        Button closeButton = closeButtonObj.AddComponent<Button>();
        closeButton.onClick.AddListener(HideCropSwapUI);
        
        RectTransform closeButtonRect = closeButtonObj.GetComponent<RectTransform>();
        closeButtonRect.anchorMin = new Vector2(0.55f, 0.15f);
        closeButtonRect.anchorMax = new Vector2(0.9f, 0.25f);
        closeButtonRect.offsetMin = Vector2.zero;
        closeButtonRect.offsetMax = Vector2.zero;
        
        // Create close button text
        GameObject closeTextObj = new GameObject("CloseText");
        closeTextObj.transform.SetParent(closeButtonObj.transform);
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
        
        // Start hidden
        cropSwapPanel.SetActive(false);
    }
    
    private void CreateCropButton(GameObject parent, string cropType, string displayName, int index)
    {
        GameObject buttonObj = new GameObject($"{cropType}Button");
        buttonObj.transform.SetParent(parent.transform);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f); // Default gray
        
        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(() => OnCropSelected(cropType));
        
        // Position buttons vertically
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        float buttonHeight = 0.25f;
        float yPos = 0.75f - (index * 0.3f);
        buttonRect.anchorMin = new Vector2(0f, yPos - buttonHeight/2);
        buttonRect.anchorMax = new Vector2(1f, yPos + buttonHeight/2);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        // Create button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = displayName;
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.fontSize = 16;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Store button reference
        cropButtons.Add(button);
        
        // Set crop1 as initially selected
        if (cropType == "crop1")
        {
            buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 0.8f); // Green for selected
        }
    }
    
    private void OnCropSelected(string cropType)
    {
        selectedCropType = cropType;
        UpdateButtonVisuals();
        Debug.Log($"Selected crop type: {cropType}");
    }
    
    private void UpdateButtonVisuals()
    {
        // Update button colors based on selection
        string[] cropTypes = { "crop1", "crop2", "crop3" };
        
        for (int i = 0; i < cropButtons.Count && i < cropTypes.Length; i++)
        {
            Image buttonImage = cropButtons[i].GetComponent<Image>();
            if (cropTypes[i] == selectedCropType)
            {
                buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 0.8f); // Green for selected
            }
            else
            {
                buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f); // Gray for unselected
            }
        }
    }
    
    private void OnConfirmClicked()
    {
        Debug.Log($"Confirmed crop swap to: {selectedCropType}");
        
        // Trigger the actual crop swap effect
        FarmhouseEffects.SetSelectedCropType(selectedCropType);
        
        // Apply the farmhouse crop swap effect
        BuildingEffectsSystem.Instance.ApplyStartWorkEffect("farmhouse", -1); // Use -1 as placeholder ID
        
        HideCropSwapUI();
    }
    
    public void ShowCropSwapUI()
    {
        if (cropSwapPanel != null)
        {
            UpdateAvailableCrops();
            cropSwapPanel.SetActive(true);
            isShowing = true;
            Debug.Log("Crop Swap UI opened");
        }
    }
    
    private void UpdateAvailableCrops()
    {
        // Find farmhouse level to determine available crops
        int farmhouseLevel = GetFarmhouseLevel();
        
        // Enable/disable buttons based on farmhouse level
        for (int i = 0; i < cropButtons.Count; i++)
        {
            Button button = cropButtons[i];
            bool isAvailable = false;
            
            if (i == 0) // crop1
                isAvailable = FarmhouseEffects.IsCropTypeAvailable("crop1");
            else if (i == 1) // crop2
                isAvailable = FarmhouseEffects.IsCropTypeAvailable("crop2");
            else if (i == 2) // crop3
                isAvailable = FarmhouseEffects.IsCropTypeAvailable("crop3");
            
            button.interactable = isAvailable;
            
            // Update visual feedback for disabled buttons
            Image buttonImage = button.GetComponent<Image>();
            if (!isAvailable)
            {
                buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f); // Dark gray for disabled
            }
            else if (i == 0 && selectedCropType == "crop1" ||
                     i == 1 && selectedCropType == "crop2" ||
                     i == 2 && selectedCropType == "crop3")
            {
                buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 0.8f); // Green for selected
            }
            else
            {
                buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f); // Gray for available but not selected
            }
        }
        
        // If current selection is not available, default to crop1
        if ((selectedCropType == "crop2" && farmhouseLevel < 1) ||
            (selectedCropType == "crop3" && farmhouseLevel < 2))
        {
            selectedCropType = "crop1";
            UpdateButtonVisuals();
        }
    }
    
    private int GetFarmhouseLevel()
    {
        // Find farmhouse building and get its level
        BuildingComponent[] allBuildings = FindObjectsOfType<BuildingComponent>();
        foreach (BuildingComponent building in allBuildings)
        {
            if (building.GetBuildingType().ToLower() == "farmhouse")
            {
                return building.GetLevel();
            }
        }
        return 0; // No farmhouse found
    }
    
    public void HideCropSwapUI()
    {
        if (cropSwapPanel != null)
        {
            cropSwapPanel.SetActive(false);
            isShowing = false;
            Debug.Log("Crop Swap UI closed");
        }
    }
    
    public bool IsShowing()
    {
        return isShowing;
    }
    
    public string GetSelectedCropType()
    {
        return selectedCropType;
    }
    
    public void SetSelectedCropType(string cropType)
    {
        selectedCropType = cropType;
        if (isShowing)
        {
            UpdateButtonVisuals();
        }
    }
}