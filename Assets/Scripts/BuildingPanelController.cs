using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BuildingPanelController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI buildingNameText;
    [SerializeField] private TextMeshProUGUI buildingDescText;
    [SerializeField] private Image buildingIcon;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button workButton;
    [SerializeField] private TextMeshProUGUI workButtonText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI upgradeButtonText;

    [Header("Building Data")]
    [SerializeField] private BuildingType currentBuildingType = BuildingType.Home;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentBuildingID = 1;

    public enum BuildingType
    {
        Home,
        Storage,
        Tian,
        FarmHouse,
        Workshop,
        Tower,
        Entrance,
        // Add new building types here for future expansion
    }

    [System.Serializable]
    public class BuildingData
    {
        public string buildingName;
        public string buildingDescription;
        public Sprite buildingIcon;
        public string workButtonText;
        public bool hasWorkFunction;
        public bool canUpgrade;
    }

    [Header("Building Configurations")]
    [SerializeField] private List<BuildingData> buildingConfigs = new List<BuildingData>();

    [Header("Panel Positioning")]
    [SerializeField] private List<Vector3> buildingPositions = new List<Vector3>();
    [SerializeField] private int maxBuildingID = 13;

    [Header("Button States")]
    [SerializeField] private Sprite workButtonAffordableSprite;
    [SerializeField] private Sprite workButtonUnaffordableSprite;
    [SerializeField] private Sprite upgradeButtonAffordableSprite;
    [SerializeField] private Sprite upgradeButtonUnaffordableSprite;

    [Header("Panel Control")]
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private float fadeSpeed = 3f;
    [SerializeField] private bool isPanelVisible = false;

    // Store original button positions
    private Vector3 workButtonOriginalPos;
    private Vector3 upgradeButtonOriginalPos;
    private bool positionsStored = false;
    private bool isFading = false;

    private void Start()
    {
        Debug.Log("BuildingPanelController Start() called");
        StoreOriginalButtonPositions();
        InitializeBuildingData();
        InitializeBuildingPositions();
        SetupPanelVisibility();
        UpdatePanelContent();
        SetupWorkButton();
        SetPanelPosition();
        Debug.Log("BuildingPanelController initialization complete");
    }

    private void StoreOriginalButtonPositions()
    {
        if (!positionsStored)
        {
            if (workButton != null)
                workButtonOriginalPos = workButton.transform.localPosition;
            if (upgradeButton != null)
                upgradeButtonOriginalPos = upgradeButton.transform.localPosition;
            positionsStored = true;
        }
    }

    private void InitializeBuildingPositions()
    {
        // Initialize position list if empty or insufficient
        while (buildingPositions.Count < maxBuildingID)
        {
            // Add default positions - customize these in inspector
            buildingPositions.Add(Vector3.zero);
        }
    }

    private void SetupPanelVisibility()
    {
        // Get or add CanvasGroup for fade effects
        if (panelCanvasGroup == null)
            panelCanvasGroup = GetComponent<CanvasGroup>();
        
        if (panelCanvasGroup == null)
            panelCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        // Keep GameObject active but make it invisible and non-interactive
        gameObject.SetActive(true);
        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;
        isPanelVisible = false;
        
        Debug.Log("Panel setup complete - invisible but active");
    }

    private void Update()
    {
        if (isPanelVisible && !isFading && Input.GetMouseButtonDown(0))
        {
            CheckClickOutsidePanel();
        }
    }

    private void CheckClickOutsidePanel()
    {
        // Use EventSystem to check if we clicked on any UI element
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            // We clicked on a UI element, check if it's part of our panel
            GameObject clickedObject = EventSystem.current.currentSelectedGameObject;
            
            // Check if the clicked object is this panel or a child of this panel
            if (clickedObject != null && (clickedObject == gameObject || clickedObject.transform.IsChildOf(transform)))
            {
                Debug.Log("Clicked on panel - keeping open");
                return; // Don't close panel
            }
            
            // Also check using raycast for more accurate detection
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
                    Debug.Log("Clicked on panel (via raycast) - keeping open");
                    return; // Don't close panel
                }
            }
            
            Debug.Log("Clicked on other UI element - closing panel");
            HidePanel();
        }
        else
        {
            // We clicked outside any UI element (on the game world)
            Debug.Log("Clicked outside UI - closing panel");
            HidePanel();
        }
    }

    private void InitializeBuildingData()
    {
        // Initialize building configs if empty
        if (buildingConfigs.Count == 0)
        {
            buildingConfigs.Add(new BuildingData // Home
            {
                buildingName = "Home",
                buildingDescription = "A comfortable dwelling where residents rest and recover. Provides basic shelter and increases population capacity.",
                workButtonText = "Rest",
                hasWorkFunction = false,
                canUpgrade = true
            });

            buildingConfigs.Add(new BuildingData // Storage
            {
                buildingName = "Storage",
                buildingDescription = "A large facility for storing resources and materials. Increases storage capacity for all resource types.",
                workButtonText = "Organize",
                hasWorkFunction = false,
                canUpgrade = true
            });

            buildingConfigs.Add(new BuildingData // Tian
            {
                buildingName = "Farmland",
                buildingDescription = "Agricultural fields for growing crops. Produces food resources over time when worked by farmers.",
                workButtonText = "Harvest",
                hasWorkFunction = true,
                canUpgrade = true
            });

            buildingConfigs.Add(new BuildingData // FarmHouse
            {
                buildingName = "Farm House",
                buildingDescription = "A rustic farmhouse that serves as both home and agricultural center. Combines living space with farming efficiency.",
                workButtonText = "Harvest",
                hasWorkFunction = true,
                canUpgrade = true
            });

            buildingConfigs.Add(new BuildingData // Workshop
            {
                buildingName = "Workshop",
                buildingDescription = "A specialized facility for creating tools, weapons, and other crafted items. Requires materials and skilled workers.",
                workButtonText = "Craft",
                hasWorkFunction = true,
                canUpgrade = true
            });

            buildingConfigs.Add(new BuildingData // Tower
            {
                buildingName = "Tower",
                buildingDescription = "A tall defensive structure that provides surveillance and protection. Increases defense and early warning capabilities.",
                workButtonText = "Worship",
                hasWorkFunction = true,
                canUpgrade = true
            });

            buildingConfigs.Add(new BuildingData // Entrance
            {
                buildingName = "Entrance",
                buildingDescription = "The primary gateway to your settlement. Controls access and serves as the first line of defense.",
                workButtonText = "Adventure",
                hasWorkFunction = true,
                canUpgrade = false
            });
        }
    }

    public void SetBuildingType(BuildingType newType)
    {
        currentBuildingType = newType;
        UpdatePanelContent();
    }

    public void SetBuildingLevel(int newLevel)
    {
        currentLevel = Mathf.Max(1, newLevel);
        UpdatePanelContent();
    }

    public void SetBuildingID(int newBuildingID)
    {
        currentBuildingID = Mathf.Max(1, newBuildingID);
        SetPanelPosition();
    }

    public void SetBuildingData(BuildingType type, int level, int buildingID)
    {
        currentBuildingType = type;
        currentLevel = Mathf.Max(1, level);
        currentBuildingID = Mathf.Max(1, buildingID);
        UpdatePanelContent();
        SetPanelPosition();
    }

    private void UpdatePanelContent()
    {
        if (buildingConfigs == null || buildingConfigs.Count == 0)
            return;

        int typeIndex = (int)currentBuildingType;
        if (typeIndex >= 0 && typeIndex < buildingConfigs.Count)
        {
            BuildingData data = buildingConfigs[typeIndex];
            
            // Update UI elements
            if (buildingNameText != null)
                buildingNameText.text = data.buildingName;
            
            if (buildingDescText != null)
                buildingDescText.text = data.buildingDescription;
            
            if (buildingIcon != null && data.buildingIcon != null)
                buildingIcon.sprite = data.buildingIcon;
            
            if (levelText != null)
                levelText.text = $"Lv. {currentLevel}";
            
            // Handle button visibility and positioning
            HandleButtonVisibility(data.hasWorkFunction, data.canUpgrade);
            
            if (workButtonText != null)
                workButtonText.text = data.workButtonText;
            
            // NEW: Update button states based on resource availability
            UpdateButtonStates();
        }
    }

    private void SetPanelPosition()
    {
        int positionIndex = currentBuildingID - 1; // Convert to 0-based index
        if (positionIndex >= 0 && positionIndex < buildingPositions.Count)
        {
            transform.localPosition = buildingPositions[positionIndex];
        }
    }

    private void HandleButtonVisibility(bool hasWorkFunction, bool canUpgrade)
    {
        // Handle work button visibility
        if (workButton != null)
        {
            workButton.gameObject.SetActive(hasWorkFunction);
        }

        // Handle upgrade button visibility and positioning
        if (upgradeButton != null)
        {
            upgradeButton.gameObject.SetActive(canUpgrade);
            
            if (canUpgrade && positionsStored)
            {
                if (hasWorkFunction)
                {
                    // Both buttons visible - restore upgrade button to original position
                    upgradeButton.transform.localPosition = upgradeButtonOriginalPos;
                }
                else
                {
                    // Only upgrade button visible - move it to work button position
                    upgradeButton.transform.localPosition = workButtonOriginalPos;
                }
            }
        }
    }

    private void SetupWorkButton()
    {
        if (workButton != null)
        {
            workButton.onClick.RemoveAllListeners();
            workButton.onClick.AddListener(OnWorkButtonClicked);
        }
        
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        }
    }

    private void OnWorkButtonClicked()
    {
        Debug.Log($"Work button clicked for {currentBuildingType} (ID: {currentBuildingID})");
        
        // Find the current building component
        BuildingComponent currentBuilding = FindBuildingByID(currentBuildingID);
        if (currentBuilding == null)
        {
            Debug.LogError($"Could not find BuildingComponent with ID: {currentBuildingID}");
            return;
        }
        
        // Find BuildingInfoUI and use its existing work assignment logic
        BuildingInfoUI buildingInfoUI = FindObjectOfType<BuildingInfoUI>();
        if (buildingInfoUI == null)
        {
            Debug.LogError("BuildingInfoUI not found in scene!");
            return;
        }
        
        // Set the current building in BuildingInfoUI (simulate showing expanded info)
        buildingInfoUI.ShowExpandedInfoForBuilding(currentBuilding, currentBuilding.GetBuildingType(), currentBuilding.GetLevel());
        
        // Trigger the work assignment using the existing method
        buildingInfoUI.TriggerAssignWork();
        
        // Hide BuildingInfoUI expanded panel immediately since we're using our own UI
        buildingInfoUI.HideExpandedInfo();
        
        // Update our panel content to reflect any changes
        UpdatePanelContent();
    }

    private void OnUpgradeButtonClicked()
    {
        Debug.Log($"Upgrade button clicked for {currentBuildingType} (ID: {currentBuildingID})");
        
        // Find the current building component
        BuildingComponent currentBuilding = FindBuildingByID(currentBuildingID);
        if (currentBuilding == null)
        {
            Debug.LogError($"Could not find BuildingComponent with ID: {currentBuildingID}");
            return;
        }
        
        // Find BuildingInfoUI and use its existing upgrade logic
        BuildingInfoUI buildingInfoUI = FindObjectOfType<BuildingInfoUI>();
        if (buildingInfoUI == null)
        {
            Debug.LogError("BuildingInfoUI not found in scene!");
            return;
        }
        
        // Set the current building in BuildingInfoUI 
        buildingInfoUI.ShowExpandedInfoForBuilding(currentBuilding, currentBuilding.GetBuildingType(), currentBuilding.GetLevel());
        
        // Use the new public method to trigger upgrade
        TriggerUpgradeFromBuildingInfoUI(buildingInfoUI);
        
        // Hide BuildingInfoUI expanded panel immediately since we're using our own UI
        buildingInfoUI.HideExpandedInfo();
        
        // Update our panel content to reflect any changes
        UpdatePanelContent();
    }

    private void TriggerUpgradeFromBuildingInfoUI(BuildingInfoUI buildingInfoUI)
    {
        // Use the new public method instead of reflection
        buildingInfoUI.TriggerUpgrade();
    }

    private BuildingComponent FindBuildingByID(int buildingID)
    {
        BuildingComponent[] allBuildings = FindObjectsOfType<BuildingComponent>();
        foreach (BuildingComponent building in allBuildings)
        {
            if (building.GetBuildingID() == buildingID)
            {
                return building;
            }
        }
        return null;
    }

    private void UpdateButtonStates()
    {
        BuildingComponent currentBuilding = FindBuildingByID(currentBuildingID);
        if (currentBuilding == null) 
        {
            Debug.LogWarning($"Could not find building with ID {currentBuildingID} for button state update");
            return;
        }

        Debug.Log($"Updating button states for building ID {currentBuildingID}, type {currentBuilding.GetBuildingType()}, status {currentBuilding.GetStatus()}");

        // Update work button state
        UpdateWorkButtonState(currentBuilding);
        
        // Update upgrade button state  
        UpdateUpgradeButtonState(currentBuilding);
    }

    private void UpdateWorkButtonState(BuildingComponent building)
    {
        if (workButton == null || workButtonText == null) return;

        string buildingType = building.GetBuildingType().ToLower();
        bool canAffordWork = CanAffordWork(building, buildingType);
        bool isWorking = building.GetStatus() == BuildingStatus.Working;
        bool isUpgrading = building.GetStatus() == BuildingStatus.Upgrading;
        bool needsRepair = building.NeedsRepair() && !building.IsRepaired();

        // Get base work text
        string baseWorkText = GetBaseWorkText(buildingType, needsRepair, isWorking);
        
        // Update button text and interactability
        bool isInteractable = !isWorking && !isUpgrading && (needsRepair || canAffordWork);
        
        string finalWorkText;
        
        if (needsRepair && !canAffordWork && !isWorking)
        {
            // Can't afford repair
            finalWorkText = baseWorkText + "\n<size=70%>(Can't Afford)</size>";
        }
        else if (!canAffordWork && !isWorking && !isUpgrading && !needsRepair)
        {
            // Can't afford regular work
            finalWorkText = baseWorkText + "\n<size=70%>(Can't Afford)</size>";
        }
        else
        {
            finalWorkText = baseWorkText;
        }
        
        workButtonText.text = finalWorkText;
        workButton.interactable = isInteractable;
        
        // Update button sprite
        Image workButtonImage = workButton.GetComponent<Image>();
        if (workButtonImage != null)
        {
            if (canAffordWork && workButtonAffordableSprite != null)
            {
                workButtonImage.sprite = workButtonAffordableSprite;
            }
            else if (!canAffordWork && workButtonUnaffordableSprite != null)
            {
                workButtonImage.sprite = workButtonUnaffordableSprite;
            }
        }
        
        // Debug logging
        Debug.Log($"Work button state: buildingType={buildingType}, canAfford={canAffordWork}, isWorking={isWorking}, isUpgrading={isUpgrading}, needsRepair={needsRepair}, finalText='{finalWorkText}'");
    }

    private void UpdateUpgradeButtonState(BuildingComponent building)
    {
        if (upgradeButton == null) return;

        bool canAffordUpgrade = CanAffordUpgrade(building);
        bool isAtMaxLevel = building.GetLevel() >= 5; // Assuming max level is 5
        bool isWorking = building.GetStatus() == BuildingStatus.Working;
        bool isUpgrading = building.GetStatus() == BuildingStatus.Upgrading;
        bool needsRepair = building.NeedsRepair() && !building.IsRepaired();

        // Get button text
        string upgradeText = GetUpgradeButtonText(isAtMaxLevel, isUpgrading, canAffordUpgrade, needsRepair);
        
        // Update button interactability
        bool isInteractable = !isWorking && !isUpgrading && !isAtMaxLevel && !needsRepair && canAffordUpgrade;
        
        // Update upgrade button text - try multiple methods to ensure it works
        if (upgradeButtonText != null)
        {
            upgradeButtonText.text = upgradeText;
        }
        else
        {
            // Fallback: try to find text component in children
            TextMeshProUGUI textComponent = upgradeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = upgradeText;
                Debug.LogWarning("UpgradeButtonText reference was null, using fallback method");
            }
            else
            {
                // Last resort: try regular Text component
                Text legacyTextComponent = upgradeButton.GetComponentInChildren<Text>();
                if (legacyTextComponent != null)
                {
                    legacyTextComponent.text = upgradeText;
                    Debug.LogWarning("Using legacy Text component for upgrade button");
                }
            }
        }
        
        upgradeButton.interactable = isInteractable;
        
        // Update button sprite
        Image upgradeButtonImage = upgradeButton.GetComponent<Image>();
        if (upgradeButtonImage != null)
        {
            if (canAffordUpgrade && upgradeButtonAffordableSprite != null)
            {
                upgradeButtonImage.sprite = upgradeButtonAffordableSprite;
            }
            else if (!canAffordUpgrade && upgradeButtonUnaffordableSprite != null)
            {
                upgradeButtonImage.sprite = upgradeButtonUnaffordableSprite;
            }
        }
        
        // Debug logging
        Debug.Log($"Upgrade button state: canAfford={canAffordUpgrade}, isMaxLevel={isAtMaxLevel}, isWorking={isWorking}, isUpgrading={isUpgrading}, needsRepair={needsRepair}, text='{upgradeText}'");
    }

    private bool CanAffordWork(BuildingComponent building, string buildingType)
    {
        // Handle repair case
        if (building.NeedsRepair() && !building.IsRepaired())
        {
            return BuildingEffectsSystem.Instance.CanStartRepair(buildingType);
        }

        // Handle normal work case - reuse BuildingEffectsSystem logic
        WorkRequirement workReq = BuildingEffectsSystem.Instance.GetDefaultWorkRequirement(buildingType);
        if (workReq != null)
        {
            return BuildingEffectsSystem.Instance.CanStartWork(buildingType, workReq.workType);
        }

        return true; // Default to true if no requirements found
    }

    private bool CanAffordUpgrade(BuildingComponent building)
    {
        // Reuse BuildingUpgradeSystem logic
        BuildingUpgradeSystem upgradeSystem = FindObjectOfType<BuildingUpgradeSystem>();
        if (upgradeSystem != null)
        {
            return upgradeSystem.CanUpgrade(building.GetBuildingType(), building.GetLevel());
        }
        
        return false;
    }

    private string GetBaseWorkText(string buildingType, bool needsRepair, bool isWorking)
    {
        if (needsRepair)
        {
            return isWorking ? "Repairing..." : "Repair";
        }

        if (isWorking)
        {
            // Return working state text based on building type
            switch (buildingType)
            {
                case "farmhouse": return "Swapping...";
                case "tian": return "Cultivating...";
                case "entrance": return "Adventuring...";
                case "workshop": case "zuofang": return "Crafting...";
                case "tower": return "Worshipping...";
                default: return "Working...";
            }
        }

        // Get building-specific work text for idle state
        switch (buildingType)
        {
            case "farmhouse": return "Crop Swap";
            case "tian": return "Cultivate";
            case "entrance": return "Adventure";
            case "workshop": case "zuofang": return "Craft";
            case "tower": return "Worship";
            default: return "Work";
        }
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

    // Panel visibility control methods
    public void ShowPanelForBuilding(int buildingID, BuildingType buildingType, int level)
    {
        Debug.Log($"ShowPanelForBuilding called: ID={buildingID}, Type={buildingType}, Level={level}");
        
        if (isFading) 
        {
            Debug.Log("Cannot show panel - currently fading");
            return;
        }
        
        // Set building data
        SetBuildingData(buildingType, level, buildingID);
        
        // Show panel with fade
        ShowPanel();
    }

    public void ShowPanel()
    {
        Debug.Log($"ShowPanel called - isPanelVisible={isPanelVisible}, isFading={isFading}");
        
        if (isPanelVisible || isFading) return;
        
        isPanelVisible = true;
        Debug.Log("Starting fade in");
        StartCoroutine(FadePanel(true));
    }

    public void HidePanel()
    {
        if (!isPanelVisible || isFading) return;
        
        isPanelVisible = false;
        StartCoroutine(FadePanel(false));
    }

    public void HidePanelImmediate()
    {
        isPanelVisible = false;
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
        }
        // Don't deactivate GameObject - keep it active for FindObjectOfType
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
            
            // Don't deactivate GameObject - keep it active for FindObjectOfType
            // if (!fadeIn)
            // {
            //     gameObject.SetActive(false);
            // }
        }
        
        isFading = false;
        Debug.Log($"Fade {(fadeIn ? "in" : "out")} complete");
    }

    public bool IsPanelVisible()
    {
        return isPanelVisible;
    }

    // Public methods for external access
    public BuildingType GetCurrentBuildingType()
    {
        return currentBuildingType;
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public int GetCurrentBuildingID()
    {
        return currentBuildingID;
    }

    // Position management methods
    public void SetBuildingPosition(int buildingID, Vector3 position)
    {
        int index = buildingID - 1; // Convert to 0-based index
        
        // Expand list if necessary
        while (buildingPositions.Count <= index)
        {
            buildingPositions.Add(Vector3.zero);
        }
        
        buildingPositions[index] = position;
        
        // Update max building ID if needed
        if (buildingID > maxBuildingID)
            maxBuildingID = buildingID;
        
        // Update position if this is the current building
        if (currentBuildingID == buildingID)
            SetPanelPosition();
    }

    public Vector3 GetBuildingPosition(int buildingID)
    {
        int index = buildingID - 1;
        if (index >= 0 && index < buildingPositions.Count)
            return buildingPositions[index];
        return Vector3.zero;
    }

    // Building configuration management
    public void AddNewBuildingType(BuildingData newBuildingData)
    {
        buildingConfigs.Add(newBuildingData);
    }

    public void UpdateBuildingData(BuildingType type, string name, string description, Sprite icon, string workText, bool hasWorkFunc = true, bool canUpgradeBuilding = true)
    {
        int index = (int)type;
        
        // Expand list if necessary
        while (buildingConfigs.Count <= index)
        {
            buildingConfigs.Add(new BuildingData());
        }
        
        if (index >= 0 && index < buildingConfigs.Count)
        {
            buildingConfigs[index].buildingName = name;
            buildingConfigs[index].buildingDescription = description;
            buildingConfigs[index].buildingIcon = icon;
            buildingConfigs[index].workButtonText = workText;
            buildingConfigs[index].hasWorkFunction = hasWorkFunc;
            buildingConfigs[index].canUpgrade = canUpgradeBuilding;
            
            if (currentBuildingType == type)
                UpdatePanelContent();
        }
    }
}