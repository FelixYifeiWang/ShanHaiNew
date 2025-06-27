using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AdventureSkillUI : MonoBehaviour
{
    private GameObject skillPanel;
    private List<GameObject> skillButtons = new List<GameObject>();
    private List<string> skillNames = new List<string>();
    private List<int> skillLevels = new List<int>();
    
    // NEW: Cooldown tracking
    private Dictionary<int, int> skillCooldowns = new Dictionary<int, int>(); // skillIndex -> remaining cooldown steps
    private int lastPlayerSteps = 0;
    
    // NEW: Skill targeting system
    private bool isWaitingForTargetSelection = false;
    private int pendingSkillIndex = -1;
    private string pendingSkillName = "";
    
    // Skill data from AdventureSelectionUI - matching the MP costs and cooldowns
    private Dictionary<string, SkillInfo> skillDatabase = new Dictionary<string, SkillInfo>();
    
    [System.Serializable]
    public class SkillInfo
    {
        public int mpCost;
        public int cooldownSteps;
        
        public SkillInfo(int mp, int cooldown)
        {
            mpCost = mp;
            cooldownSteps = cooldown;
        }
    }
    
    // Singleton pattern
    private static AdventureSkillUI instance;
    public static AdventureSkillUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AdventureSkillUI>();
                if (instance == null)
                {
                    GameObject skillUIObj = new GameObject("AdventureSkillUI");
                    instance = skillUIObj.AddComponent<AdventureSkillUI>();
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
        
        InitializeSkillDatabase();
        
        // Track player steps for cooldown system
        AdventureGameManager gameManager = FindObjectOfType<AdventureGameManager>();
        if (gameManager != null)
        {
            lastPlayerSteps = gameManager.GetCurrentSteps();
            Debug.Log($"Initial player steps: {lastPlayerSteps}");
        }
    }
    
    void Start()
    {
        // Get selected skills from AdventureDataManager
        skillNames = AdventureDataManager.GetSelectedSkillNames();
        skillLevels = AdventureDataManager.GetSelectedSkillLevels();
        
        // Initialize cooldowns (all skills start ready)
        for (int i = 0; i < skillNames.Count; i++)
        {
            skillCooldowns[i] = 0;
        }
        
        CreateSkillUI();
        
        // Subscribe to step changes for cooldown updates
        InvokeRepeating("CheckForStepChanges", 0.1f, 0.1f);
        
        // Subscribe to tile click events for skill targeting
        SubscribeToTileClicks();
    }
    
    private void InitializeSkillDatabase()
    {
        // Initialize with the same data as AdventureSelectionUI
        skillDatabase["Umbrella"] = new SkillInfo(20, 2);
        
        // Add placeholder skills (matching AdventureSelectionUI pattern)
        for (int i = 1; i <= 30; i++)
        {
            skillDatabase[$"Skill {i}"] = new SkillInfo(10, 1);
        }
    }
    
    // NEW: Check for step changes and update cooldowns
    private void CheckForStepChanges()
    {
        AdventureGameManager gameManager = FindObjectOfType<AdventureGameManager>();
        if (gameManager == null) return;
        
        int currentSteps = gameManager.GetCurrentSteps();
        
        // If steps decreased (player used steps), reduce cooldowns
        if (currentSteps < lastPlayerSteps)
        {
            int stepsUsed = lastPlayerSteps - currentSteps;
            Debug.Log($"Steps used: {stepsUsed}, reducing cooldowns. Current steps: {currentSteps}, Last steps: {lastPlayerSteps}");
            ReduceCooldowns(stepsUsed);
            lastPlayerSteps = currentSteps;
        }
        // Update lastPlayerSteps even if no change to keep it current
        else if (currentSteps != lastPlayerSteps)
        {
            lastPlayerSteps = currentSteps;
        }
    }
    
    // NEW: Reduce cooldowns when steps are used
    private void ReduceCooldowns(int stepsUsed)
    {
        bool cooldownsChanged = false;
        
        for (int i = 0; i < skillNames.Count; i++)
        {
            if (skillCooldowns[i] > 0)
            {
                int oldCooldown = skillCooldowns[i];
                skillCooldowns[i] -= stepsUsed;
                if (skillCooldowns[i] < 0) skillCooldowns[i] = 0;
                Debug.Log($"Skill {skillNames[i]}: cooldown reduced from {oldCooldown} to {skillCooldowns[i]}");
                cooldownsChanged = true;
            }
        }
        
        if (cooldownsChanged)
        {
            UpdateSkillButtonVisuals();
        }
    }
    
    private void CreateSkillUI()
    {
        if (skillNames.Count == 0) return;
        
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
        
        // Create skill panel container
        GameObject panelObj = new GameObject("AdventureSkillPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        skillPanel = panelObj;
        
        // Position panel at bottom center
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0);
        panelRect.anchorMax = new Vector2(0.5f, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.anchoredPosition = new Vector2(0, 20);
        
        // Calculate total width needed
        int skillCount = skillNames.Count;
        float buttonWidth = 120f;
        float buttonGap = 15f;
        float totalWidth = (buttonWidth * skillCount) + (buttonGap * (skillCount - 1));
        
        panelRect.sizeDelta = new Vector2(totalWidth, 80);
        
        // Add HorizontalLayoutGroup for perfect centering
        HorizontalLayoutGroup layoutGroup = panelObj.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.spacing = buttonGap;
        layoutGroup.padding = new RectOffset(0, 0, 0, 0);
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        
        // Create skill buttons
        for (int i = 0; i < skillNames.Count; i++)
        {
            CreateSkillButton(panelObj, i);
        }
    }
    
    private void CreateSkillButton(GameObject parent, int skillIndex)
    {
        string skillName = skillNames[skillIndex];
        int skillLevel = skillLevels[skillIndex];
        
        // Get skill info from database
        SkillInfo skillInfo = skillDatabase.ContainsKey(skillName) ? 
            skillDatabase[skillName] : new SkillInfo(10, 1);
        
        // Create button GameObject
        GameObject buttonObj = new GameObject($"SkillButton_{skillIndex}");
        buttonObj.transform.SetParent(parent.transform, false);
        
        // Set fixed button size
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(120, 80);
        
        // Add LayoutElement to prevent layout group from changing size
        LayoutElement layoutElement = buttonObj.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 120;
        layoutElement.preferredHeight = 80;
        layoutElement.flexibleWidth = 0;
        layoutElement.flexibleHeight = 0;
        
        // Add button background
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.3f, 0.6f, 0.8f);
        
        // Add outline
        Outline outline = buttonObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, 2);
        
        // Add Button component
        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(() => UseSkill(skillIndex));
        
        // Create skill name text
        GameObject nameTextObj = new GameObject("SkillName");
        nameTextObj.transform.SetParent(buttonObj.transform, false);
        
        Text nameText = nameTextObj.AddComponent<Text>();
        nameText.text = skillName;
        nameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        nameText.fontSize = 12;
        nameText.color = Color.white;
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.fontStyle = FontStyle.Bold;
        
        RectTransform nameRect = nameTextObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.6f);
        nameRect.anchorMax = new Vector2(1, 0.9f);
        nameRect.offsetMin = new Vector2(5, 0);
        nameRect.offsetMax = new Vector2(-5, 0);
        
        // Create MP cost text
        GameObject mpTextObj = new GameObject("MPCost");
        mpTextObj.transform.SetParent(buttonObj.transform, false);
        
        Text mpText = mpTextObj.AddComponent<Text>();
        mpText.text = $"MP: {skillInfo.mpCost}";
        mpText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        mpText.fontSize = 10;
        mpText.color = Color.cyan;
        mpText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform mpRect = mpTextObj.GetComponent<RectTransform>();
        mpRect.anchorMin = new Vector2(0, 0.3f);
        mpRect.anchorMax = new Vector2(1, 0.6f);
        mpRect.offsetMin = new Vector2(5, 0);
        mpRect.offsetMax = new Vector2(-5, 0);
        
        // Create cooldown text
        GameObject cooldownTextObj = new GameObject("Cooldown");
        cooldownTextObj.transform.SetParent(buttonObj.transform, false);
        
        Text cooldownText = cooldownTextObj.AddComponent<Text>();
        cooldownText.text = $"CD: {skillInfo.cooldownSteps}";
        cooldownText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        cooldownText.fontSize = 10;
        cooldownText.color = Color.yellow;
        cooldownText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform cooldownRect = cooldownTextObj.GetComponent<RectTransform>();
        cooldownRect.anchorMin = new Vector2(0, 0);
        cooldownRect.anchorMax = new Vector2(1, 0.3f);
        cooldownRect.offsetMin = new Vector2(5, 0);
        cooldownRect.offsetMax = new Vector2(-5, 0);
        
        skillButtons.Add(buttonObj);
    }
    
    // NEW: Update button visuals based on cooldown status
    private void UpdateSkillButtonVisuals()
    {
        for (int i = 0; i < skillButtons.Count; i++)
        {
            GameObject buttonObj = skillButtons[i];
            if (buttonObj == null) continue;
            
            Button button = buttonObj.GetComponent<Button>();
            Image buttonImage = buttonObj.GetComponent<Image>();
            
            string skillName = skillNames[i];
            SkillInfo skillInfo = skillDatabase.ContainsKey(skillName) ? 
                skillDatabase[skillName] : new SkillInfo(10, 1);
            
            AdventureGameManager gameManager = FindObjectOfType<AdventureGameManager>();
            bool hasEnoughMP = gameManager != null && gameManager.HasEnoughMP(skillInfo.mpCost);
            bool onCooldown = skillCooldowns[i] > 0;
            
            // Update button state
            button.interactable = hasEnoughMP && !onCooldown;
            
            // Update visual appearance
            if (onCooldown)
            {
                buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 0.8f); // Gray when on cooldown
            }
            else if (!hasEnoughMP)
            {
                buttonImage.color = new Color(0.6f, 0.3f, 0.3f, 0.8f); // Red when not enough MP
            }
            else if (isWaitingForTargetSelection && pendingSkillIndex == i)
            {
                buttonImage.color = new Color(1f, 0.8f, 0.2f, 0.8f); // Golden when selected for targeting
            }
            else
            {
                buttonImage.color = new Color(0.3f, 0.3f, 0.6f, 0.8f); // Normal blue when ready
            }
            
            // Update cooldown text
            Text cooldownText = buttonObj.transform.Find("Cooldown").GetComponent<Text>();
            if (cooldownText != null)
            {
                if (onCooldown)
                {
                    cooldownText.text = $"CD: {skillCooldowns[i]}";
                    cooldownText.color = Color.red;
                }
                else
                {
                    cooldownText.text = $"CD: {skillInfo.cooldownSteps}";
                    cooldownText.color = Color.yellow;
                }
            }
        }
    }
    
    private void UseSkill(int skillIndex)
    {
        if (skillIndex >= skillNames.Count) return;
        
        string skillName = skillNames[skillIndex];
        SkillInfo skillInfo = skillDatabase.ContainsKey(skillName) ? 
            skillDatabase[skillName] : new SkillInfo(10, 1);
        
        // If we're waiting for target selection and player clicks same skill, cancel
        if (isWaitingForTargetSelection && pendingSkillIndex == skillIndex)
        {
            CancelSkillTargeting();
            return;
        }
        
        // If we're waiting for target selection for a different skill, cancel previous and start new
        if (isWaitingForTargetSelection)
        {
            CancelSkillTargeting();
        }
        
        // Check if skill is on cooldown
        if (skillCooldowns[skillIndex] > 0)
        {
            Debug.Log($"Skill {skillName} is on cooldown! {skillCooldowns[skillIndex]} steps remaining.");
            return;
        }
        
        // Check if player has enough MP
        AdventureGameManager gameManager = FindObjectOfType<AdventureGameManager>();
        if (gameManager == null || !gameManager.HasEnoughMP(skillInfo.mpCost))
        {
            Debug.Log($"Not enough MP to use {skillName}! Required: {skillInfo.mpCost}");
            return;
        }
        
        // Handle skills that need target selection
        if (RequiresTargetSelection(skillName))
        {
            StartSkillTargeting(skillIndex, skillName);
        }
        else
        {
            // Direct execution for skills that don't need targeting
            ExecuteSkillDirectly(skillIndex, skillName, skillInfo);
        }
    }
    
    // NEW: Check if skill requires target selection
    private bool RequiresTargetSelection(string skillName)
    {
        switch (skillName)
        {
            case "Umbrella":
                return true;
            // Add other targeting skills here
            default:
                return false;
        }
    }
    
    // NEW: Start skill targeting mode
    private void StartSkillTargeting(int skillIndex, string skillName)
    {
        isWaitingForTargetSelection = true;
        pendingSkillIndex = skillIndex;
        pendingSkillName = skillName;
        
        Debug.Log($"Skill {skillName} activated! Click on a tile to target, or click the skill again to cancel.");
        
        // Apply visual effects for this specific skill
        ApplySkillTargetingVisuals(skillName);
        
        // Update button visual to show it's selected
        UpdateSkillButtonVisuals();
    }
    
    // NEW: Cancel skill targeting
    private void CancelSkillTargeting()
    {
        Debug.Log($"Skill {pendingSkillName} targeting canceled.");
        
        string canceledSkillName = pendingSkillName;
        
        isWaitingForTargetSelection = false;
        pendingSkillIndex = -1;
        pendingSkillName = "";
        
        // Remove visual effects for the canceled skill
        RemoveSkillTargetingVisuals(canceledSkillName);
        
        // Update button visuals back to normal
        UpdateSkillButtonVisuals();
    }
    
    // NEW: Execute skill directly (for non-targeting skills)
    private void ExecuteSkillDirectly(int skillIndex, string skillName, SkillInfo skillInfo)
    {
        // Use MP and start cooldown
        AdventureGameManager gameManager = FindObjectOfType<AdventureGameManager>();
        gameManager.UseMP(skillInfo.mpCost);
        skillCooldowns[skillIndex] = skillInfo.cooldownSteps;
        UpdateSkillButtonVisuals();
        
        Debug.Log($"Used skill: {skillName} (MP Cost: {skillInfo.mpCost}, Cooldown: {skillInfo.cooldownSteps})");
        
        // Execute skill effect
        ExecuteSkillEffect(skillName, skillIndex);
    }
    
    // NEW: Subscribe to tile click events
    private void SubscribeToTileClicks()
    {
        // Find all hex tiles and add click listeners
        HexTileController[] allTiles = FindObjectsOfType<HexTileController>();
        foreach (HexTileController tile in allTiles)
        {
            // Add click listener if it doesn't already exist
            tile.gameObject.AddComponent<SkillTargetingClickHandler>();
        }
    }
    
    // NEW: Handle tile clicks for skill targeting
    public void OnTileClicked(int row, int col)
    {
        if (!isWaitingForTargetSelection) return;
        
        // Check if the clicked tile is valid for targeting
        HexGridManager gridManager = FindObjectOfType<HexGridManager>();
        if (gridManager == null) return;
        
        // For now, allow clicking on any clickable tile
        if (!gridManager.IsClickable(row, col))
        {
            Debug.Log("Cannot target non-clickable tiles!");
            return;
        }
        
        Debug.Log($"Tile ({row}, {col}) selected for skill {pendingSkillName}");
        
        // Execute the skill with the target
        ExecuteSkillWithTarget(pendingSkillIndex, pendingSkillName, row, col);
        
        // Clear targeting state and visuals
        string executedSkillName = pendingSkillName;
        CancelSkillTargeting(); // This will remove visuals for the executed skill
    }
    
    // NEW: Execute skill with target position
    private void ExecuteSkillWithTarget(int skillIndex, string skillName, int targetRow, int targetCol)
    {
        SkillInfo skillInfo = skillDatabase.ContainsKey(skillName) ? 
            skillDatabase[skillName] : new SkillInfo(10, 1);
        
        // Use MP and start cooldown
        AdventureGameManager gameManager = FindObjectOfType<AdventureGameManager>();
        gameManager.UseMP(skillInfo.mpCost);
        skillCooldowns[skillIndex] = skillInfo.cooldownSteps;
        UpdateSkillButtonVisuals();
        
        Debug.Log($"Used skill: {skillName} at ({targetRow}, {targetCol}) (MP Cost: {skillInfo.mpCost}, Cooldown: {skillInfo.cooldownSteps})");
        
        // Execute skill effect with target
        ExecuteSkillEffectWithTarget(skillName, skillIndex, targetRow, targetCol);
    }
    // NEW: Check if we're waiting for target selection (for click handler)
    public bool IsWaitingForTargetSelection()
    {
        return isWaitingForTargetSelection;
    }
    
    // NEW: Skill effect execution system (for non-targeting skills)
    private void ExecuteSkillEffect(string skillName, int skillIndex)
    {
        switch (skillName)
        {
            case "Skill 1":
                ExecuteSkill1();
                break;
                
            case "Skill 2":
                ExecuteSkill2();
                break;
                
            // Add more non-targeting skills here
            default:
                ExecuteDefaultSkill(skillName);
                break;
        }
    }
    
    // NEW: Skill effect execution system (for targeting skills)
    private void ExecuteSkillEffectWithTarget(string skillName, int skillIndex, int targetRow, int targetCol)
    {
        switch (skillName)
        {
            case "Umbrella":
                ExecuteUmbrellaSkill(targetRow, targetCol);
                break;
                
            // Add more targeting skills here
            default:
                Debug.Log($"No targeting implementation for skill: {skillName}");
                break;
        }
    }
    
    // Umbrella skill: Remove bombs around selected tile AND the tile itself
    private void ExecuteUmbrellaSkill(int targetRow, int targetCol)
    {
        Debug.Log($"Umbrella skill executed at tile ({targetRow}, {targetCol})!");
        
        HexGridManager gridManager = FindObjectOfType<HexGridManager>();
        if (gridManager == null) return;
        
        int bombsRemoved = 0;
        
        // First check the clicked tile itself
        if (gridManager.GetTileType(targetRow, targetCol) == HexTileType.Bomb)
        {
            gridManager.SetTileType(targetRow, targetCol, HexTileType.Empty);
            bombsRemoved++;
            Debug.Log($"Removed bomb at target tile ({targetRow}, {targetCol})");
        }
        
        // Then check all neighbors
        int[] neighborRows, neighborCols;
        GetHexNeighbors(targetRow, targetCol, out neighborRows, out neighborCols);
        
        for (int i = 0; i < neighborRows.Length; i++)
        {
            int neighborRow = neighborRows[i];
            int neighborCol = neighborCols[i];
            
            if (gridManager.IsValidPosition(neighborRow, neighborCol) && 
                gridManager.GetTileType(neighborRow, neighborCol) == HexTileType.Bomb)
            {
                gridManager.SetTileType(neighborRow, neighborCol, HexTileType.Empty);
                bombsRemoved++;
                Debug.Log($"Removed bomb at neighbor ({neighborRow}, {neighborCol})");
            }
        }
        
        // Update all revealed tile indicators
        UpdateAllRevealedTileIndicators();
        
        Debug.Log($"Umbrella skill removed {bombsRemoved} bombs around and including tile ({targetRow}, {targetCol})");
    }
    
    // Helper method to get hex neighbors (same logic as HexGridManager)
    private void GetHexNeighbors(int row, int col, out int[] neighborRows, out int[] neighborCols)
    {
        // Hex neighbor offsets - EXACTLY same as in HexGridManager
        // Different for even and odd COLUMNS (not rows!)
        int[,] evenColOffsets = { {-1, -1}, {-1, 0}, {0, -1}, {0, 1}, {-1, 1}, {1, 0} };
        int[,] oddColOffsets = { {-1, 0}, {1, -1}, {0, -1}, {0, 1}, {1, 0}, {1, 1} };
        
        int[,] offsets = (col % 2 == 0) ? evenColOffsets : oddColOffsets;
        
        neighborRows = new int[6];
        neighborCols = new int[6];
        
        for (int i = 0; i < 6; i++)
        {
            neighborRows[i] = row + offsets[i, 0];
            neighborCols[i] = col + offsets[i, 1];
        }
    }
    
    // Replace revealed tile sprite to show it's now empty (no longer a bomb)
    private void ReplaceRevealedTileSprite(int row, int col)
    {
        HexTileController[] allTiles = FindObjectsOfType<HexTileController>();
        foreach (HexTileController tile in allTiles)
        {
            if (tile.row == row && tile.column == col)
            {
                // Load revealed_tile sprite for empty tiles
                Sprite revealedSprite = Resources.Load<Sprite>("revealed_tile");
                if (revealedSprite != null)
                {
                    SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.sprite = revealedSprite;
                        sr.color = Color.white; // Full opacity
                    }
                }
                break;
            }
        }
    }
    
    // Update indicators for all revealed tiles (since bomb removal affects indicators globally)
    private void UpdateAllRevealedTileIndicators()
    {
        HexGridManager gridManager = FindObjectOfType<HexGridManager>();
        if (gridManager == null) return;
        
        HexTileController[] allTiles = FindObjectsOfType<HexTileController>();
        foreach (HexTileController tile in allTiles)
        {
            // Only update revealed empty tiles (they show indicators)
            if (gridManager.IsRevealed(tile.row, tile.column) && 
                gridManager.GetTileType(tile.row, tile.column) == HexTileType.Empty)
            {
                // Remove old indicators and add new ones
                RemoveAllIndicators(tile.gameObject);
                AddWarningIndicatorsForTile(tile.gameObject, tile.row, tile.column);
            }
        }
    }
    
    // Remove all indicator children from a tile
    private void RemoveAllIndicators(GameObject tileObject)
    {
        // Find and destroy all indicator objects
        for (int i = tileObject.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = tileObject.transform.GetChild(i);
            if (child.name.StartsWith("Indicator_"))
            {
                Destroy(child.gameObject);
            }
        }
    }
    
    // Add warning indicators for a specific tile (similar to HexGridManager logic)
    private void AddWarningIndicatorsForTile(GameObject tileObject, int row, int col)
    {
        HexGridManager gridManager = FindObjectOfType<HexGridManager>();
        if (gridManager == null) return;
        
        // Get neighbors and check for special tiles
        int[] neighborRows, neighborCols;
        GetHexNeighbors(row, col, out neighborRows, out neighborCols);
        
        bool hasBombNearby = false;
        bool hasTreasureNearby = false;
        bool hasDestinationNearby = false;
        
        for (int i = 0; i < neighborRows.Length; i++)
        {
            int nr = neighborRows[i];
            int nc = neighborCols[i];
            
            if (gridManager.IsValidPosition(nr, nc))
            {
                HexTileType neighborType = gridManager.GetTileType(nr, nc);
                
                if (neighborType == HexTileType.Bomb)
                {
                    hasBombNearby = true;
                }
                else if (neighborType == HexTileType.Treasure)
                {
                    hasTreasureNearby = true;
                }
                else if (neighborType == HexTileType.Destination)
                {
                    hasDestinationNearby = true;
                }
            }
        }
        
        // Create indicators if needed (same logic as HexGridManager)
        if (hasBombNearby || hasTreasureNearby || hasDestinationNearby)
        {
            CreateIndicatorsForTile(tileObject, hasBombNearby, hasTreasureNearby, hasDestinationNearby);
        }
    }
    
    // Create indicators for a tile (same logic as HexGridManager)
    private void CreateIndicatorsForTile(GameObject tileObject, bool showBomb, bool showTreasure, bool showDestination)
    {
        float indicatorSize = 1f;
        float gap = 0.2f;
        
        int indicatorCount = 0;
        if (showBomb) indicatorCount++;
        if (showTreasure) indicatorCount++;
        if (showDestination) indicatorCount++;
        
        if (indicatorCount == 0) return;
        
        float totalWidth = indicatorCount * indicatorSize + (indicatorCount - 1) * gap;
        float startX = -totalWidth / 2 + indicatorSize / 2;
        
        int currentIndex = 0;
        
        if (showBomb)
        {
            float xPos = startX + currentIndex * (indicatorSize + gap);
            CreateSingleIndicator(tileObject, "bomb_ind", new Vector3(xPos, 0.2f, -0.1f), indicatorSize);
            currentIndex++;
        }
        
        if (showTreasure)
        {
            float xPos = startX + currentIndex * (indicatorSize + gap);
            CreateSingleIndicator(tileObject, "chest_ind", new Vector3(xPos, 0.2f, -0.1f), indicatorSize);
            currentIndex++;
        }
        
        if (showDestination)
        {
            float xPos = startX + currentIndex * (indicatorSize + gap);
            CreateSingleIndicator(tileObject, "exit_ind", new Vector3(xPos, 0.2f, -0.1f), indicatorSize);
            currentIndex++;
        }
    }
    
    // Create a single indicator (same logic as HexGridManager)
    private void CreateSingleIndicator(GameObject parent, string spriteName, Vector3 localPosition, float size)
    {
        Sprite indicatorSprite = Resources.Load<Sprite>(spriteName);
        if (indicatorSprite == null)
        {
            Debug.LogError($"{spriteName} sprite not found in Resources folder");
            return;
        }
        
        GameObject indicator = new GameObject($"Indicator_{spriteName}");
        indicator.transform.SetParent(parent.transform);
        indicator.transform.localPosition = localPosition;
        
        SpriteRenderer sr = indicator.AddComponent<SpriteRenderer>();
        sr.sprite = indicatorSprite;
        sr.sortingOrder = 1;
        
        indicator.transform.localScale = Vector3.one * size;
        
        Color color = sr.color;
        color.a = 1f;
        sr.color = color;
    }
    
    // Placeholder for Skill 1
    private void ExecuteSkill1()
    {
        Debug.Log("Skill 1 effect executed!");
        // TODO: Implement Skill 1 effect
    }
    
    // Placeholder for Skill 2
    private void ExecuteSkill2()
    {
        Debug.Log("Skill 2 effect executed!");
        // TODO: Implement Skill 2 effect
    }
    
    // Default skill effect for unknown skills
    private void ExecuteDefaultSkill(string skillName)
    {
        Debug.Log($"Default skill effect for {skillName} - no specific implementation yet.");
        // TODO: Add default behavior or log warning for unimplemented skills
    }
    
    // NEW: Apply visual effects when targeting specific skills
    private void ApplySkillTargetingVisuals(string skillName)
    {
        switch (skillName)
        {
            case "Umbrella":
                ApplyUmbrellaTargetingVisuals();
                break;
            // Add other skills' targeting visuals here
            default:
                // No special visuals for this skill
                break;
        }
    }
    
    // NEW: Remove visual effects when targeting ends
    private void RemoveSkillTargetingVisuals(string skillName)
    {
        switch (skillName)
        {
            case "Umbrella":
                RemoveUmbrellaTargetingVisuals();
                break;
            // Add other skills' visual removal here
            default:
                // No special visuals to remove for this skill
                break;
        }
    }
    
    // NEW: Apply Umbrella-specific targeting visuals (red clickable tiles)
    private void ApplyUmbrellaTargetingVisuals()
    {
        HexGridManager gridManager = FindObjectOfType<HexGridManager>();
        if (gridManager == null) return;
        
        HexTileController[] allTiles = FindObjectsOfType<HexTileController>();
        foreach (HexTileController tile in allTiles)
        {
            // Use the existing clickability logic from HexGridManager
            if (gridManager.IsClickable(tile.row, tile.column))
            {
                SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    // Apply red tint to show this tile is targetable for Umbrella
                    spriteRenderer.color = new Color(1f, 0.5f, 0.5f, 1f); // Red tint
                }
            }
        }
    }
    
    // NEW: Remove Umbrella-specific targeting visuals
    private void RemoveUmbrellaTargetingVisuals()
    {
        HexGridManager gridManager = FindObjectOfType<HexGridManager>();
        if (gridManager == null) return;
        
        HexTileController[] allTiles = FindObjectsOfType<HexTileController>();
        foreach (HexTileController tile in allTiles)
        {
            // Reset clickable tiles back to their normal state
            if (gridManager.IsClickable(tile.row, tile.column))
            {
                SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    // Reset to the tile's normal clickable appearance (full opacity white)
                    spriteRenderer.color = Color.white;
                }
            }
        }
    }
    
    // Removed the custom IsTileClickable method since we're using the existing one
}