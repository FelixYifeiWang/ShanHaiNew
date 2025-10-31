using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BuildingPanelController_Farmhouse : MonoBehaviour
{
    [Header("Farmhouse UI References")]
    [SerializeField] private TextMeshProUGUI buildingNameText;
    [SerializeField] private TextMeshProUGUI buildingDescText;
    [SerializeField] private Image buildingIcon;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeButtonText;
    [SerializeField] private Button crop1Button;
    [SerializeField] private Button crop2Button;
    [SerializeField] private Button crop3Button;

    [Header("Building Data")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentBuildingID = 1;

    [Header("Panel Control")]
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private float fadeSpeed = 3f;
    [SerializeField] private bool isPanelVisible = false;

    [Header("Button States")]
    [SerializeField] private Sprite upgradeButtonAffordableSprite;
    [SerializeField] private Sprite upgradeButtonUnaffordableSprite;
    [SerializeField] private Sprite cropButtonSelectedSprite;
    [SerializeField] private Sprite cropButtonUnselectedSprite;

    private bool isFading = false;
    private string currentSelectedCrop = "crop1"; // Default to crop1

    private void Start()
    {
        Debug.Log("BuildingPanelController_Farmhouse Start() called");
        SetupPanelVisibility();
        SetupButtons();
        Debug.Log("BuildingPanelController_Farmhouse initialization complete");
    }

    private void Update()
    {
        if (isPanelVisible && !isFading && Input.GetMouseButtonDown(0))
        {
            CheckClickOutsidePanel();
        }
    }

    private void SetupPanelVisibility()
    {
        if (panelCanvasGroup == null)
            panelCanvasGroup = GetComponent<CanvasGroup>();
        
        if (panelCanvasGroup == null)
            panelCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        gameObject.SetActive(true);
        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;
        isPanelVisible = false;
        
        Debug.Log("Farmhouse panel setup complete - invisible but active");
    }

    private void SetupButtons()
    {
        // Setup upgrade button
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        }
        
        // Setup crop buttons
        if (crop1Button != null)
        {
            crop1Button.onClick.RemoveAllListeners();
            crop1Button.onClick.AddListener(() => OnCropButtonClicked("crop1"));
        }
        
        if (crop2Button != null)
        {
            crop2Button.onClick.RemoveAllListeners();
            crop2Button.onClick.AddListener(() => OnCropButtonClicked("crop2"));
        }
        
        if (crop3Button != null)
        {
            crop3Button.onClick.RemoveAllListeners();
            crop3Button.onClick.AddListener(() => OnCropButtonClicked("crop3"));
        }
    }

    private void OnUpgradeButtonClicked()
    {
        Debug.Log($"Farmhouse upgrade button clicked for building ID: {currentBuildingID}");
        
        BuildingComponent currentBuilding = FindBuildingByID(currentBuildingID);
        if (currentBuilding == null)
        {
            Debug.LogError($"Could not find BuildingComponent with ID: {currentBuildingID}");
            return;
        }
        
        BuildingInfoUI buildingInfoUI = FindObjectOfType<BuildingInfoUI>();
        if (buildingInfoUI == null)
        {
            Debug.LogError("BuildingInfoUI not found in scene!");
            return;
        }
        
        // Use existing BuildingInfoUI upgrade logic
        buildingInfoUI.ShowExpandedInfoForBuilding(currentBuilding, currentBuilding.GetBuildingType(), currentBuilding.GetLevel());
        buildingInfoUI.TriggerUpgrade();
        buildingInfoUI.HideExpandedInfo();
        
        // Update our panel content to reflect changes
        UpdatePanelContent();
    }

    private void OnCropButtonClicked(string cropType)
    {
        Debug.Log($"Crop button clicked: {cropType}");

        // If clicking the currently selected crop, do nothing
        if (currentSelectedCrop == cropType)
        {
            Debug.Log($"Crop {cropType} is already selected, no action needed");
            return;
        }

        // Find the current building component for context
        BuildingComponent currentBuilding = FindBuildingByID(currentBuildingID);
        if (currentBuilding == null)
        {
            Debug.LogError($"Could not find BuildingComponent with ID: {currentBuildingID}");
            return;
        }

        // Use the existing CropSwapUI system to perform the actual swap
        if (CropSwapUI.Instance != null)
        {
            Debug.Log($"Performing crop swap from {currentSelectedCrop} to {cropType}");
            
            // Set up BuildingInfoUI context (same pattern as upgrade/work buttons)
            BuildingInfoUI buildingInfoUI = FindObjectOfType<BuildingInfoUI>();
            if (buildingInfoUI != null)
            {
                // Set the building context
                buildingInfoUI.ShowExpandedInfoForBuilding(currentBuilding, currentBuilding.GetBuildingType(), currentBuilding.GetLevel());
                
                // Perform the crop swap using existing system
                bool swapSuccess = PerformCropSwapUsingExistingSystem(currentSelectedCrop, cropType);
                
                // Hide BuildingInfoUI panel immediately since we're using our own UI
                buildingInfoUI.HideExpandedInfo();
                
                if (swapSuccess)
                {
                    // Update our visual state
                    currentSelectedCrop = cropType;
                    UpdateCropButtonVisuals();
                    Debug.Log($"Crop swap successful: now using {cropType}");
                }
                else
                {
                    Debug.Log($"Crop swap failed from {currentSelectedCrop} to {cropType}");
                }
            }
            else
            {
                Debug.LogError("BuildingInfoUI not found for crop swap!");
            }
        }
        else
        {
            Debug.LogError("CropSwapUI.Instance not found!");
        }
    }

    private bool PerformCropSwapUsingExistingSystem(string fromCrop, string toCrop)
    {
        try
        {
            // Method 1: Try to use CropSwapUI's internal swap logic
            if (CropSwapUI.Instance != null)
            {
                // Check if CropSwapUI has a direct swap method
                var swapMethod = typeof(CropSwapUI).GetMethod("SwapToSelectedCrop", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                
                if (swapMethod != null)
                {
                    // First set the target crop
                    FarmhouseEffects.SetSelectedCropType(toCrop);
                    
                    // Then trigger the swap
                    swapMethod.Invoke(CropSwapUI.Instance, null);
                    return true;
                }
                
                // Method 2: Try alternative approach - directly set the crop type
                FarmhouseEffects.SetSelectedCropType(toCrop);
                Debug.Log($"Direct crop type change from {fromCrop} to {toCrop}");
                return true;
            }
            
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Crop swap failed: {e.Message}");
            return false;
        }
    }

    private void CheckClickOutsidePanel()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            GameObject clickedObject = EventSystem.current.currentSelectedGameObject;
            
            if (clickedObject != null && (clickedObject == gameObject || clickedObject.transform.IsChildOf(transform)))
            {
                Debug.Log("Clicked on farmhouse panel - keeping open");
                return;
            }
            
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            
            foreach (RaycastResult result in results)
            {
                if (result.gameObject == gameObject || result.gameObject.transform.IsChildOf(transform))
                {
                    Debug.Log("Clicked on farmhouse panel (via raycast) - keeping open");
                    return;
                }
            }
            
            Debug.Log("Clicked outside farmhouse panel - closing");
            HidePanel();
        }
        else
        {
            Debug.Log("Clicked outside UI - closing farmhouse panel");
            HidePanel();
        }
    }

    public void ShowPanelForBuilding(int buildingID, int level)
    {
        Debug.Log($"ShowPanelForBuilding called: ID={buildingID}, Level={level}");
        
        if (isFading) 
        {
            Debug.Log("Cannot show farmhouse panel - currently fading");
            return;
        }
        
        currentBuildingID = buildingID;
        currentLevel = level;
        
        // Try to get current selected crop from FarmhouseEffects
        try
        {
            if (FarmhouseEffects.IsCropTypeAvailable("crop1"))
            {
                currentSelectedCrop = FarmhouseEffects.GetSelectedCropType();
            }
        }
        catch
        {
            Debug.Log("Could not get selected crop from FarmhouseEffects, using default");
            currentSelectedCrop = "crop1";
        }
        
        UpdatePanelContent();
        ShowPanel();
    }

    public void ShowPanel()
    {
        Debug.Log($"ShowPanel called - isPanelVisible={isPanelVisible}, isFading={isFading}");
        
        if (isPanelVisible || isFading) return;
        
        isPanelVisible = true;
        Debug.Log("Starting farmhouse panel fade in");
        StartCoroutine(FadePanel(true));
    }

    public void HidePanel()
    {
        if (!isPanelVisible || isFading) return;
        
        isPanelVisible = false;
        StartCoroutine(FadePanel(false));
    }

    private System.Collections.IEnumerator FadePanel(bool fadeIn)
    {
        isFading = true;
        
        float startAlpha = fadeIn ? 0f : 1f;
        float targetAlpha = fadeIn ? 1f : 0f;
        float elapsed = 0f;
        float duration = 1f / fadeSpeed;
        
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = startAlpha;
            panelCanvasGroup.interactable = fadeIn;
            panelCanvasGroup.blocksRaycasts = fadeIn;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                panelCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
                yield return null;
            }
            
            panelCanvasGroup.alpha = targetAlpha;
        }
        
        isFading = false;
        Debug.Log($"Farmhouse panel fade {(fadeIn ? "in" : "out")} complete");
    }

    private void UpdatePanelContent()
    {
        BuildingComponent building = FindBuildingByID(currentBuildingID);
        if (building == null) 
        {
            Debug.LogWarning($"Could not find building with ID {currentBuildingID} for farmhouse panel content update");
            return;
        }

        Debug.Log($"Updating farmhouse panel content for building ID {currentBuildingID}, level {currentLevel}");

        // Update basic building info (content unchanged as requested)
        if (buildingNameText != null)
            buildingNameText.text = "Farm House";
        
        if (buildingDescText != null)
            buildingDescText.text = "A rustic farmhouse that serves as both home and agricultural center. Combines living space with farming efficiency.";
        
        if (levelText != null)
            levelText.text = $"Lv. {currentLevel}";

        // Handle crop button visibility based on level and resource unlock status
        HandleCropButtonVisibility();

        // Update crop button visuals
        UpdateCropButtonVisuals();

        // Update upgrade button state
        UpdateUpgradeButtonState(building);
    }

    private void HandleCropButtonVisibility()
    {
        ResourceManager resourceManager = FindObjectOfType<ResourceManager>();
        if (resourceManager == null)
        {
            Debug.LogWarning("ResourceManager not found - cannot check crop unlock status");
            return;
        }

        // Crop1 button is always visible (assumed to be always unlocked)
        if (crop1Button != null)
        {
            crop1Button.gameObject.SetActive(true);
        }

        // Crop2 button visibility - only if crop2 resource is unlocked
        if (crop2Button != null)
        {
            Resource crop2Resource = resourceManager.GetResource("crop2");
            bool shouldShowCrop2 = crop2Resource != null && crop2Resource.isUnlocked;
            crop2Button.gameObject.SetActive(shouldShowCrop2);
            
            Debug.Log($"Crop2 button visibility: Unlocked={shouldShowCrop2}");
        }

        // Crop3 button visibility - only if level >= 2 AND crop3 resource is unlocked
        if (crop3Button != null)
        {
            Resource crop3Resource = resourceManager.GetResource("crop3");
            bool isLevelSufficient = currentLevel >= 2;
            bool isResourceUnlocked = crop3Resource != null && crop3Resource.isUnlocked;
            bool shouldShowCrop3 = isLevelSufficient && isResourceUnlocked;
            
            crop3Button.gameObject.SetActive(shouldShowCrop3);
            
            Debug.Log($"Crop3 button visibility: Level={currentLevel}>=2: {isLevelSufficient}, Unlocked={isResourceUnlocked}, Final={shouldShowCrop3}");
        }
    }

    private void UpdateCropButtonVisuals()
    {
        // Ensure selected crop is still valid/visible
        ValidateSelectedCrop();
        
        // Update visual states for all crop buttons
        UpdateCropButtonSprite(crop1Button, "crop1");
        UpdateCropButtonSprite(crop2Button, "crop2");
        UpdateCropButtonSprite(crop3Button, "crop3");
        
        Debug.Log($"Current selected crop: {currentSelectedCrop}");
    }

    private void ValidateSelectedCrop()
    {
        ResourceManager resourceManager = FindObjectOfType<ResourceManager>();
        if (resourceManager == null) return;

        // Check if currently selected crop is still available
        bool isSelectedCropValid = true;

        if (currentSelectedCrop == "crop2")
        {
            Resource crop2Resource = resourceManager.GetResource("crop2");
            isSelectedCropValid = crop2Resource != null && crop2Resource.isUnlocked;
        }
        else if (currentSelectedCrop == "crop3")
        {
            Resource crop3Resource = resourceManager.GetResource("crop3");
            bool isResourceUnlocked = crop3Resource != null && crop3Resource.isUnlocked;
            bool isLevelSufficient = currentLevel >= 2;
            isSelectedCropValid = isResourceUnlocked && isLevelSufficient;
        }

        // If selected crop is no longer valid, fall back to crop1
        if (!isSelectedCropValid)
        {
            Debug.Log($"Selected crop {currentSelectedCrop} is no longer available, falling back to crop1");
            currentSelectedCrop = "crop1";
            
            // Update FarmhouseEffects to match
            try
            {
                FarmhouseEffects.SetSelectedCropType("crop1");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not update FarmhouseEffects: {e.Message}");
            }
        }
    }

    private void UpdateCropButtonSprite(Button cropButton, string cropType)
    {
        if (cropButton == null) return;

        Image buttonImage = cropButton.GetComponent<Image>();
        if (buttonImage == null) return;

        bool isSelected = currentSelectedCrop == cropType;
        
        if (isSelected && cropButtonSelectedSprite != null)
        {
            buttonImage.sprite = cropButtonSelectedSprite;
        }
        else if (!isSelected && cropButtonUnselectedSprite != null)
        {
            buttonImage.sprite = cropButtonUnselectedSprite;
        }
    }

    private void UpdateUpgradeButtonState(BuildingComponent building)
    {
        if (upgradeButton == null) return;

        BuildingUpgradeSystem upgradeSystem = FindObjectOfType<BuildingUpgradeSystem>();
        bool canAfford = upgradeSystem != null && upgradeSystem.CanUpgrade(building.GetBuildingType(), building.GetLevel());
        bool isUpgrading = building.GetStatus() == BuildingStatus.Upgrading;
        bool isAtMaxLevel = building.GetLevel() >= 5;
        bool isWorking = building.GetStatus() == BuildingStatus.Working;
        bool needsRepair = building.NeedsRepair() && !building.IsRepaired();

        string upgradeText = GetUpgradeButtonText(isAtMaxLevel, isUpgrading, canAfford, needsRepair);

        // Update button text with fallback methods
        if (upgradeButtonText != null)
        {
            upgradeButtonText.text = upgradeText;
        }
        else
        {
            TextMeshProUGUI textComponent = upgradeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = upgradeText;
            }
        }

        upgradeButton.interactable = !isUpgrading && !isAtMaxLevel && !needsRepair && canAfford;

        // Update sprite
        Image upgradeButtonImage = upgradeButton.GetComponent<Image>();
        if (upgradeButtonImage != null)
        {
            if (canAfford && upgradeButtonAffordableSprite != null)
                upgradeButtonImage.sprite = upgradeButtonAffordableSprite;
            else if (!canAfford && upgradeButtonUnaffordableSprite != null)
                upgradeButtonImage.sprite = upgradeButtonUnaffordableSprite;
        }

        Debug.Log($"Farmhouse upgrade button state: canAfford={canAfford}, isMaxLevel={isAtMaxLevel}, isUpgrading={isUpgrading}, text='{upgradeText}'");
    }

    private string GetUpgradeButtonText(bool isAtMaxLevel, bool isUpgrading, bool canAfford, bool needsRepair)
    {
        if (needsRepair)
        {
            return "Upgrade\n<size=70%>(Repair First)</size>";
        }

        if (isAtMaxLevel)
        {
            return "Max Level";
        }

        if (isUpgrading)
        {
            return "Upgrading...";
        }

        if (!canAfford)
        {
            return "Upgrade\n<size=70%>(Can't Afford)</size>";
        }

        return "Upgrade";
    }

    private BuildingComponent FindBuildingByID(int buildingID)
    {
        BuildingComponent[] allBuildings = FindObjectsOfType<BuildingComponent>();
        foreach (BuildingComponent building in allBuildings)
        {
            if (building.GetBuildingID() == buildingID)
                return building;
        }
        return null;
    }

    public bool IsPanelVisible()
    {
        return isPanelVisible;
    }

    public int GetCurrentBuildingID()
    {
        return currentBuildingID;
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public string GetCurrentSelectedCrop()
    {
        return currentSelectedCrop;
    }
}