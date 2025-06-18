using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuildingInfoUI : MonoBehaviour
{
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private Text infoText;
    [SerializeField] private GameObject expandedPanel;
    [SerializeField] private Button assignWorkButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Text expandedTitleText; // Reference to the title text in expanded panel
    
    private bool isExpanded = false;
    private BuildingComponent currentBuilding; // Reference to currently selected building
    private BuildingUpgradeSystem upgradeSystem; // Reference to upgrade system
    
    void Start()
    {
        // Get reference to upgrade system
        upgradeSystem = FindObjectOfType<BuildingUpgradeSystem>();
        
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
        
        if (expandedPanel == null)
        {
            CreateExpandedPanel();
        }
        
        if (expandedPanel != null)
        {
            expandedPanel.SetActive(false);
        }
        
        // Add button listeners
        if (assignWorkButton != null)
        {
            assignWorkButton.onClick.AddListener(OnAssignWorkClicked);
        }
        
        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }
    }
    
    // In BuildingInfoUI.cs, modify the OnAssignWorkClicked() method to handle farmhouse crop swap:

    private void OnAssignWorkClicked()
    {
        if (currentBuilding == null)
        {
            return;
        }
        
        string buildingType = currentBuilding.GetBuildingType().ToLower();

        if (currentBuilding.NeedsRepair() && !currentBuilding.IsRepaired())
        {
            // Handle repair logic
            if (currentBuilding.GetStatus() == BuildingStatus.Working)
            {
                return;
            }
            
            // Check repair resource requirements
            if (BuildingEffectsSystem.Instance.CanStartRepair(buildingType))
            {
                // Start the repair process
                if (BuildingEffectsSystem.Instance.StartRepair(currentBuilding.GetBuildingID(), buildingType))
                {
                    currentBuilding.StartRepair();
                }
            }
            else
            {
            }
            
            // Update the display after repair attempt
            UpdateExpandedDisplay();
            return;
        }
        
        // Special handling for farmhouse crop swap
        if (buildingType == "farmhouse")
        {
            // Use singleton pattern to get or create CropSwapUI
            CropSwapUI.Instance.ShowCropSwapUI();
            return;
        }
        
        // Handle workshop crafting
        if (buildingType == "workshop" || buildingType == "zuofang")
        {
            // For now, create CraftingUI on demand until we add singleton pattern to it
            CraftingUI craftingUI = FindObjectOfType<CraftingUI>();
            if (craftingUI == null)
            {
                GameObject craftingObj = new GameObject("CraftingUI");
                craftingUI = craftingObj.AddComponent<CraftingUI>();
            }
            craftingUI.ShowCraftingUI();
            return;
        }
        
        // Regular work handling for other buildings
        WorkRequirement requirement = BuildingEffectsSystem.Instance.GetDefaultWorkRequirement(buildingType);
        if (requirement != null)
        {
            if (BuildingEffectsSystem.Instance.CanStartWork(buildingType, requirement.workType))
            {
                // FIX: For Tian buildings, handle dynamic work assignment properly
                if (buildingType == "tian")
                {
                    // Get the selected crop type
                    string selectedCropType = "crop1"; // Default
                    if (FarmhouseEffects.IsCropTypeAvailable("crop1"))
                    {
                        selectedCropType = FarmhouseEffects.GetSelectedCropType();
                    }
                    
                    // Create dynamic work requirement for the selected crop
                    WorkRequirement tianRequirement = GetTianWorkRequirement(selectedCropType);
                    
                    // Start the work with the proper crop-specific assignment
                    bool success = BuildingEffectsSystem.Instance.StartTimedWork(
                        currentBuilding.GetBuildingID(),
                        "tian",
                        $"{selectedCropType}_cultivation",
                        tianRequirement.duration,
                        tianRequirement.resourceCosts
                    );
                    
                    if (success)
                    {
                        // Apply start work effects (but DON'T call StartTimedWork again)
                        // Just log that Tian work started
                        Debug.Log($"Tian starting {selectedCropType} cultivation (duration: {tianRequirement.duration} days)");
                    }
                }
                else
                {
                    // For all other buildings, use the standard work system
                    bool success = BuildingEffectsSystem.Instance.StartTimedWork(
                        currentBuilding.GetBuildingID(),
                        buildingType,
                        requirement.workType,
                        requirement.duration,
                        requirement.resourceCosts
                    );
                    
                    if (success)
                    {
                        // Apply any additional start work effects
                        BuildingEffectsSystem.Instance.ApplyStartWorkEffect(buildingType, currentBuilding.GetBuildingID());
                    }
                }
                
                // Update the display after starting work
                UpdateExpandedDisplay();
            }
            else
            {
            }
        }
        else
        {
            // Fallback to old system for buildings without work requirements
            currentBuilding.StartWork();
            BuildingEffectsSystem.Instance.ApplyStartWorkEffect(buildingType, currentBuilding.GetBuildingID());
        }
    }

    private WorkRequirement GetTianWorkRequirement(string cropType)
    {
        WorkRequirement requirement;
        
        switch (cropType)
        {
            case "crop1":
                requirement = new WorkRequirement($"{cropType}_cultivation", 1);
                requirement.AddResourceCost("actpoint", 1);
                break;
                
            case "crop2":
                requirement = new WorkRequirement($"{cropType}_cultivation", 2);
                requirement.AddResourceCost("actpoint", 1);
                break;
                
            case "crop3":
                requirement = new WorkRequirement($"{cropType}_cultivation", 3);
                requirement.AddResourceCost("actpoint", 1);
                break;
                
            default:
                // Fallback to crop1
                requirement = new WorkRequirement("crop1_cultivation", 1);
                requirement.AddResourceCost("actpoint", 1);
                Debug.LogWarning($"Unknown crop type {cropType}, defaulting to crop1");
                break;
        }
        
        return requirement;
    }
    
    private void OnUpgradeClicked()
    {
        if (currentBuilding == null)
        {
            return;
        }
        
        // Check if building is already upgrading
        if (currentBuilding.GetStatus() == BuildingStatus.Upgrading)
        {
            return;
        }
        
        string buildingType = currentBuilding.GetBuildingType();
        int currentLevel = currentBuilding.GetLevel();
        
        // Check if building is at max level
        if (currentLevel >= BuildingUpgradeSystem.MAX_BUILDING_LEVEL)
        {
            return;
        }
        
        // Check if upgrade system is available
        if (upgradeSystem == null)
        {
            return;
        }
        
        // Check if upgrade is possible (has sufficient resources)
        if (!upgradeSystem.CanUpgrade(buildingType, currentLevel))
        {
            // Log what resources are missing
            List<UpgradeRequirement> missingResources = upgradeSystem.GetMissingResources(buildingType, currentLevel);
            if (missingResources.Count > 0)
            {
                string missingText = "Cannot upgrade - Missing: ";
                for (int i = 0; i < missingResources.Count; i++)
                {
                    missingText += $"{missingResources[i].requiredQuantity} {missingResources[i].resourceName}";
                    if (i < missingResources.Count - 1)
                    {
                        missingText += ", ";
                    }
                }
            }
            else
            {

            }
            return;
        }
        
        // Get and log the upgrade cost
        BuildingUpgradeCost upgradeCost = upgradeSystem.GetUpgradeCost(buildingType, currentLevel);
        if (upgradeCost != null)
        {
            string costText = $"Upgrading {buildingType} from level {currentLevel} to {currentLevel + 1}. Cost: ";
            for (int i = 0; i < upgradeCost.requirements.Count; i++)
            {
                costText += $"{upgradeCost.requirements[i].requiredQuantity} {upgradeCost.requirements[i].resourceName}";
                if (i < upgradeCost.requirements.Count - 1)
                {
                    costText += ", ";
                }
            }
        }
        
        // Consume the required resources
        upgradeSystem.ConsumeUpgradeResources(buildingType, currentLevel);
        
        // Start the upgrade process
        currentBuilding.StartUpgrade();
        
        // Update the display
        UpdateExpandedDisplay();
        
    }
    
    private void UpdateExpandedDisplay()
    {
        if (currentBuilding != null && expandedTitleText != null)
        {
            string displayType = currentBuilding.GetBuildingType();
            
            // Capitalize first letter for display
            if (!string.IsNullOrEmpty(displayType))
            {
                displayType = char.ToUpper(displayType[0]) + displayType.Substring(1).ToLower();
            }
            
            string statusText = "";
            if (currentBuilding.NeedsRepair() && !currentBuilding.IsRepaired())
            {
                if (currentBuilding.GetStatus() == BuildingStatus.Working)
                {
                    statusText = " (Repairing...)";
                }
                else
                {
                    statusText = " (Needs Repair)";
                }
            }
            else if (currentBuilding.GetStatus() == BuildingStatus.Upgrading)
            {
                statusText = " (Upgrading...)";
            }
            else if (currentBuilding.GetStatus() == BuildingStatus.Working)
            {
                statusText = " (Working...)";
            }
            
            expandedTitleText.text = $"{displayType}, Level {currentBuilding.GetLevel()}{statusText}";
        }
        
        // Update button states
        if (currentBuilding != null)
        {
            ConfigureButtonsForBuilding(currentBuilding.GetBuildingType().ToLower());
        }
    }
    
    private void CreateExpandedPanel()
    {
        // Find or create canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        // Create expanded panel
        GameObject expandedObj = new GameObject("ExpandedBuildingPanel");
        expandedObj.transform.SetParent(canvas.transform);
        expandedPanel = expandedObj;
        
        // Add Image component for background
        Image panelImage = expandedObj.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.9f); // Dark background
        
        // Set panel size and position
        RectTransform panelRect = expandedObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(300, 200);
        
        // Create title text
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(expandedObj.transform);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "Building Info"; // Default text, will be updated when shown
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 18;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        
        // Store reference to title text for later updates
        expandedTitleText = titleText;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.7f);
        titleRect.anchorMax = new Vector2(1, 0.9f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        // Create Assign Work button
        assignWorkButton = CreateButton("AssignWorkButton", "Assign Work", expandedObj.transform);
        RectTransform assignRect = assignWorkButton.GetComponent<RectTransform>();
        assignRect.anchorMin = new Vector2(0.1f, 0.4f);
        assignRect.anchorMax = new Vector2(0.9f, 0.6f);
        assignRect.offsetMin = Vector2.zero;
        assignRect.offsetMax = Vector2.zero;
        
        // Create Upgrade button
        upgradeButton = CreateButton("UpgradeButton", "Upgrade", expandedObj.transform);
        RectTransform upgradeRect = upgradeButton.GetComponent<RectTransform>();
        upgradeRect.anchorMin = new Vector2(0.1f, 0.2f);
        upgradeRect.anchorMax = new Vector2(0.9f, 0.4f);
        upgradeRect.offsetMin = Vector2.zero;
        upgradeRect.offsetMax = Vector2.zero;
        
        // Create Close button
        Button closeButton = CreateButton("CloseButton", "Close", expandedObj.transform);
        closeButton.onClick.AddListener(HideExpandedInfo);
        RectTransform closeRect = closeButton.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.1f, 0.05f);
        closeRect.anchorMax = new Vector2(0.9f, 0.15f);
        closeRect.offsetMin = Vector2.zero;
        closeRect.offsetMax = Vector2.zero;
    }
    
    private Button CreateButton(string name, string text, Transform parent)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent);
        
        // Add Image for button background
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        
        // Add Button component
        Button button = buttonObj.AddComponent<Button>();
        
        // Create button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = text;
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.fontSize = 14;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        // Position text to fill button
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // If this is the assign work button, add a second text for cost display
        if (name == "AssignWorkButton")
        {
            GameObject costTextObj = new GameObject("CostText");
            costTextObj.transform.SetParent(buttonObj.transform);
            
            Text costText = costTextObj.AddComponent<Text>();
            costText.text = "";
            costText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            costText.fontSize = 10;
            costText.color = new Color(0.8f, 0.8f, 0.8f, 1f); // Light gray
            costText.alignment = TextAnchor.MiddleCenter;
            
            // Position cost text below main text
            RectTransform costTextRect = costTextObj.GetComponent<RectTransform>();
            costTextRect.anchorMin = new Vector2(0, 0);
            costTextRect.anchorMax = new Vector2(1, 0.4f);
            costTextRect.offsetMin = Vector2.zero;
            costTextRect.offsetMax = Vector2.zero;
            
            // Adjust main text to upper portion
            textRect.anchorMin = new Vector2(0, 0.4f);
            textRect.anchorMax = new Vector2(1, 1);
        }
        
        // If this is the upgrade button, add a second text for cost display
        if (name == "UpgradeButton")
        {
            GameObject costTextObj = new GameObject("CostText");
            costTextObj.transform.SetParent(buttonObj.transform);
            
            Text costText = costTextObj.AddComponent<Text>();
            costText.text = "";
            costText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            costText.fontSize = 10;
            costText.color = new Color(0.8f, 0.8f, 0.8f, 1f); // Light gray
            costText.alignment = TextAnchor.MiddleCenter;
            
            // Position cost text below main text
            RectTransform costTextRect = costTextObj.GetComponent<RectTransform>();
            costTextRect.anchorMin = new Vector2(0, 0);
            costTextRect.anchorMax = new Vector2(1, 0.4f);
            costTextRect.offsetMin = Vector2.zero;
            costTextRect.offsetMax = Vector2.zero;
            
            // Adjust main text to upper portion
            textRect.anchorMin = new Vector2(0, 0.4f);
            textRect.anchorMax = new Vector2(1, 1);
        }
        
        return button;
    }
    
    public void ShowInfo(string buildingType, int level, Vector3 worldPosition)
    {
        if (infoPanel != null && infoText != null && !isExpanded)
        {
            infoText.text = $"{buildingType}, Level {level}";
            
            // Auto-adjust panel width based on text content
            StartCoroutine(AdjustPanelWidth());
            
            infoPanel.SetActive(true);
            
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            infoPanel.transform.position = screenPos + new Vector3(0, 130, 0);
        }
    }
    
    private System.Collections.IEnumerator AdjustPanelWidth()
    {
        // Wait one frame for text to update
        yield return null;
        
        if (infoText != null && infoPanel != null)
        {
            // Force the text to calculate its preferred size
            infoText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1000f);
            Canvas.ForceUpdateCanvases();
            
            // Get the preferred width of the text
            float textWidth = infoText.preferredWidth;
            
            // Add generous padding
            float panelWidth = textWidth + 40f; // 20px padding on each side
            
            // Set minimum width
            panelWidth = Mathf.Max(panelWidth, 150f);
            
            // Get the panel's RectTransform
            RectTransform panelRect = infoPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                // Keep the height the same, just adjust width
                Vector2 currentSize = panelRect.sizeDelta;
                panelRect.sizeDelta = new Vector2(panelWidth, currentSize.y);
                
                // Also make sure the text rect can use the full width
                RectTransform textRect = infoText.rectTransform;
                if (textRect != null)
                {
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = new Vector2(10f, 0); // 10px left padding
                    textRect.offsetMax = new Vector2(-10f, 0); // 10px right padding
                }
            }
            
        }
    }
    
    public void ShowExpandedInfo(string buildingType, int level)
    {
        // This method needs to be called differently to pass the building component directly
        // For now, we'll try to find the correct building, but this should be refactored
        ShowExpandedInfoForBuilding(null, buildingType, level);
    }
    
    public void ShowExpandedInfoForBuilding(BuildingComponent building, string buildingType, int level)
    {
        // Use the passed building component directly, or find it as fallback
        if (building != null)
        {
            currentBuilding = building;
        }
        else
        {
            // Fallback - try to find building (this is not ideal but maintains compatibility)
            BuildingComponent[] buildings = FindObjectsOfType<BuildingComponent>();
            currentBuilding = null;
            
            foreach (BuildingComponent buildingComp in buildings)
            {
                if (buildingComp.GetBuildingType() == buildingType && buildingComp.GetLevel() == level)
                {
                    currentBuilding = buildingComp;
                    break;
                }
            }
        }
        
        if (expandedPanel != null)
        {
            // Hide hover info
            if (infoPanel != null)
            {
                infoPanel.SetActive(false);
            }
            
            // Show expanded panel
            expandedPanel.SetActive(true);
            isExpanded = true;
            
            // Update display
            UpdateExpandedDisplay();
            
            // Configure buttons based on building type
            if (currentBuilding != null)
            {
                ConfigureButtonsForBuilding(currentBuilding.GetBuildingType().ToLower());
            }
        }
    }
    
    private void ConfigureButtonsForBuilding(string buildingType)
    {
        bool showAssignWork = true;
        bool showUpgrade = true;
        string assignWorkText = "Start Work";
        string upgradeText = "Upgrade";
        bool upgradeInteractable = true;
        bool assignWorkInteractable = true;
        
        // Check if building is currently upgrading
        if (currentBuilding != null && currentBuilding.NeedsRepair() && !currentBuilding.IsRepaired())
        {
            assignWorkText = "Repair";
            assignWorkInteractable = currentBuilding.GetStatus() != BuildingStatus.Working;
            upgradeInteractable = false; // Can't upgrade if needs repair

            if (assignWorkInteractable && !BuildingEffectsSystem.Instance.CanStartRepair(buildingType))
            {
                assignWorkText = "Can't Afford";
                assignWorkInteractable = false;
            }
            
            if (currentBuilding.GetStatus() == BuildingStatus.Working)
            {
                assignWorkText = "Repairing...";
                assignWorkInteractable = false;
            }
        }
        else
        {
            // Original logic for non-repair buildings
            
            // Check if building is currently upgrading
            if (currentBuilding != null && currentBuilding.GetStatus() == BuildingStatus.Upgrading)
            {
                upgradeText = "Upgrading...";
                upgradeInteractable = false;
                assignWorkInteractable = false; // Disable work actions during upgrade
            }
            else if (currentBuilding != null && currentBuilding.GetStatus() == BuildingStatus.Working)
            {
                upgradeInteractable = false; // Disable upgrade during work
                assignWorkText = "Working...";
                assignWorkInteractable = false; // Disable work actions during work
            }
            
            // Check if building is at max level
            if (currentBuilding != null && currentBuilding.GetLevel() >= BuildingUpgradeSystem.MAX_BUILDING_LEVEL)
            {
                upgradeText = "Max Level";
                upgradeInteractable = false;
            }
            
            // Check if player can afford upgrade
            if (currentBuilding != null && upgradeSystem != null && upgradeInteractable)
            {
                if (!upgradeSystem.CanUpgrade(currentBuilding.GetBuildingType(), currentBuilding.GetLevel()))
                {
                    upgradeText = "Can't Afford";
                    upgradeInteractable = false;
                }
            }
            
            // Check if player can afford work
            if (currentBuilding != null && assignWorkInteractable)
            {
                WorkRequirement workReq = BuildingEffectsSystem.Instance.GetDefaultWorkRequirement(buildingType);
                if (workReq != null && !BuildingEffectsSystem.Instance.CanStartWork(buildingType, workReq.workType))
                {
                    assignWorkText = "Can't Afford";
                    assignWorkInteractable = false;
                }
            }
        }
        
        switch (buildingType.ToLower())
        {
            case "siheyuan":
            case "home":
                showAssignWork = false; // Home cannot be assigned work
                break;
                
            case "farmhouse":
                if (assignWorkText == "Start Work" || assignWorkText == "Working...")
                    assignWorkText = assignWorkInteractable ? "Crop Swap" : assignWorkText;
                break;
                
            case "tian":
                if (assignWorkText == "Start Work" && assignWorkInteractable)
                {
                    // Show dynamic crop type and duration
                    string selectedCrop = "crop1"; // Default
                    if (FarmhouseEffects.IsCropTypeAvailable("crop1")) // Check if farmhouse exists
                    {
                        selectedCrop = FarmhouseEffects.GetSelectedCropType();
                    }
                    assignWorkText = TianEffects.GetCropDescription(selectedCrop);
                }
                break;
                
            case "entrance":
                showUpgrade = false; // Entrance cannot be upgraded
                if (assignWorkText == "Start Work")
                    assignWorkText = "Begin Adventure";
                break;
                
            case "tower":
                // Tower-specific logic is handled by repair check above
                break;

            case "storage":
                showAssignWork = false;
                break;
                
            case "workshop":
            case "zuofang":
                if (assignWorkText == "Start Work")
                    assignWorkText = "Open Crafting";
                break;
        }
        
        // Configure Assign Work button
        if (assignWorkButton != null)
        {
            assignWorkButton.gameObject.SetActive(showAssignWork);
            assignWorkButton.interactable = assignWorkInteractable;
            
            if (showAssignWork)
            {
                Text buttonText = assignWorkButton.transform.Find("Text").GetComponent<Text>();
                Text costText = assignWorkButton.transform.Find("CostText").GetComponent<Text>();
                
                if (buttonText != null)
                {
                    buttonText.text = assignWorkText;
                }
                
                // Update cost text for work
                if (costText != null && currentBuilding != null)
                {
                    string costString = "";
                    
                    if (currentBuilding.GetStatus() == BuildingStatus.Working)
                    {
                        costString = "";
                    }
                    else if (currentBuilding.GetStatus() == BuildingStatus.Upgrading)
                    {
                        costString = "";
                    }
                    else if (currentBuilding.NeedsRepair() && !currentBuilding.IsRepaired())
                    {
                        // Show repair costs
                        RepairRequirement repairReq = BuildingEffectsSystem.Instance.GetRepairRequirement(buildingType);
                        if (repairReq != null && repairReq.resourceCosts.Count > 0)
                        {
                            for (int i = 0; i < repairReq.resourceCosts.Count; i++)
                            {
                                costString += $"{repairReq.resourceCosts[i].requiredQuantity} {repairReq.resourceCosts[i].resourceName}";
                                if (i < repairReq.resourceCosts.Count - 1)
                                {
                                    costString += ", ";
                                }
                            }
                        }
                    }
                    else
                    {
                        // Special handling for Tian buildings - show dynamic cost
                        if (buildingType == "tian")
                        {
                            costString = "1 actpoint"; // Tian always costs 1 actpoint
                        }
                        else
                        {
                            WorkRequirement workReq = BuildingEffectsSystem.Instance.GetDefaultWorkRequirement(buildingType);
                            if (workReq != null && workReq.resourceCosts.Count > 0)
                            {
                                for (int i = 0; i < workReq.resourceCosts.Count; i++)
                                {
                                    costString += $"{workReq.resourceCosts[i].requiredQuantity} {workReq.resourceCosts[i].resourceName}";
                                    if (i < workReq.resourceCosts.Count - 1)
                                    {
                                        costString += ", ";
                                    }
                                }
                            }
                        }
                    }
                    
                    costText.text = costString;
                }
                
                // Change button color based on interactability
                Image buttonImage = assignWorkButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    if (!assignWorkInteractable)
                    {
                        if (currentBuilding != null && currentBuilding.GetStatus() == BuildingStatus.Working)
                        {
                            buttonImage.color = new Color(0.6f, 0.4f, 0.2f, 0.8f); // Orange for working
                        }
                        else if (currentBuilding != null && currentBuilding.GetStatus() == BuildingStatus.Upgrading)
                        {
                            buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 0.6f); // Gray for disabled during upgrade
                        }
                        else
                        {
                            buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 0.6f); // Gray for can't afford
                        }
                    }
                    else
                    {
                        buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f); // Normal color
                    }
                }
            }
        }
        
        // Configure Upgrade button
        if (upgradeButton != null)
        {
            upgradeButton.gameObject.SetActive(showUpgrade);
            upgradeButton.interactable = upgradeInteractable;
            
            if (showUpgrade)
            {
                Text buttonText = upgradeButton.transform.Find("Text").GetComponent<Text>();
                Text costText = upgradeButton.transform.Find("CostText").GetComponent<Text>();
                
                if (buttonText != null)
                {
                    buttonText.text = upgradeText;
                }
                
                // Update cost text
                if (costText != null && currentBuilding != null && upgradeSystem != null)
                {
                    string costString = "";
                    
                    if (currentBuilding.GetLevel() >= BuildingUpgradeSystem.MAX_BUILDING_LEVEL)
                    {
                        costString = "";
                    }
                    else if (currentBuilding.GetStatus() == BuildingStatus.Upgrading)
                    {
                        costString = "";
                    }
                    else
                    {
                        BuildingUpgradeCost upgradeCost = upgradeSystem.GetUpgradeCost(currentBuilding.GetBuildingType(), currentBuilding.GetLevel());
                        if (upgradeCost != null && upgradeCost.requirements.Count > 0)
                        {
                            for (int i = 0; i < upgradeCost.requirements.Count; i++)
                            {
                                costString += $"{upgradeCost.requirements[i].requiredQuantity} {upgradeCost.requirements[i].resourceName}";
                                if (i < upgradeCost.requirements.Count - 1)
                                {
                                    costString += ", ";
                                }
                            }
                        }
                    }
                    
                    costText.text = costString;
                }
                
                // Change button color based on interactability
                Image buttonImage = upgradeButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    if (!upgradeInteractable)
                    {
                        if (currentBuilding != null && currentBuilding.GetStatus() == BuildingStatus.Upgrading)
                        {
                            buttonImage.color = new Color(0.6f, 0.4f, 0.2f, 0.8f); // Orange for upgrading
                        }
                        else
                        {
                            buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 0.6f); // Gray for can't afford/max level
                        }
                    }
                    else
                    {
                        buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 0.8f); // Normal color
                    }
                }
            }
        }
        
        // Adjust button positions if one is hidden
        RepositionButtons(showAssignWork, showUpgrade);
    }
    
    private void RepositionButtons(bool showAssignWork, bool showUpgrade)
    {
        if (showAssignWork && showUpgrade)
        {
            // Both buttons shown - normal positions
            if (assignWorkButton != null)
            {
                RectTransform assignRect = assignWorkButton.GetComponent<RectTransform>();
                assignRect.anchorMin = new Vector2(0.1f, 0.4f);
                assignRect.anchorMax = new Vector2(0.9f, 0.6f);
            }
            
            if (upgradeButton != null)
            {
                RectTransform upgradeRect = upgradeButton.GetComponent<RectTransform>();
                upgradeRect.anchorMin = new Vector2(0.1f, 0.2f);
                upgradeRect.anchorMax = new Vector2(0.9f, 0.4f);
            }
        }
        else if (showAssignWork && !showUpgrade)
        {
            // Only assign work button - center it
            if (assignWorkButton != null)
            {
                RectTransform assignRect = assignWorkButton.GetComponent<RectTransform>();
                assignRect.anchorMin = new Vector2(0.1f, 0.3f);
                assignRect.anchorMax = new Vector2(0.9f, 0.5f);
            }
        }
        else if (!showAssignWork && showUpgrade)
        {
            // Only upgrade button - center it
            if (upgradeButton != null)
            {
                RectTransform upgradeRect = upgradeButton.GetComponent<RectTransform>();
                upgradeRect.anchorMin = new Vector2(0.1f, 0.3f);
                upgradeRect.anchorMax = new Vector2(0.9f, 0.5f);
            }
        }
    }
    
    public void HideInfo()
    {
        if (infoPanel != null && !isExpanded)
        {
            infoPanel.SetActive(false);
        }
    }
    
    public void HideExpandedInfo()
    {
        if (expandedPanel != null)
        {
            expandedPanel.SetActive(false);
            isExpanded = false;
            currentBuilding = null;
        }
    }
    
    public bool IsExpanded()
    {
        return isExpanded;
    }
}