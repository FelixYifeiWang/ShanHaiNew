using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AdventureSkillUI : MonoBehaviour
{
    [Header("UI References")]
    private GameObject skillPanel;
    private List<GameObject> skillButtons = new List<GameObject>();
    
    [Header("Skill Data")]
    private List<AdventureSkill> playerSkills = new List<AdventureSkill>();
    private List<int> skillLevels = new List<int>();
    
    [Header("Targeting System")]
    private bool isWaitingForTargetSelection = false;
    private int pendingSkillIndex = -1;
    private AdventureSkill pendingSkill;
    
    [Header("Cooldown System")]
    private Dictionary<int, int> skillCooldowns = new Dictionary<int, int>();
    private int lastPlayerSteps = 0;
    
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
        
        // Track player steps for cooldown system
        AdventureGameManager gameManager = FindObjectOfType<AdventureGameManager>();
        if (gameManager != null)
        {
            lastPlayerSteps = gameManager.GetCurrentSteps();
        }
    }
    
    void Start()
    {
        LoadPlayerSkills();
        CreateSkillUI();
        InvokeRepeating("CheckForStepChanges", 0.1f, 0.1f);
        SubscribeToTileClicks();
    }
    
    private void LoadPlayerSkills()
    {
        List<string> skillNames = AdventureDataManager.GetSelectedSkillNames();
        skillLevels = AdventureDataManager.GetSelectedSkillLevels();
        
        for (int i = 0; i < skillNames.Count; i++)
        {
            AdventureSkill skill = LoadSkillByName(skillNames[i]);
            if (skill != null)
            {
                playerSkills.Add(skill);
                skillCooldowns[i] = 0;
            }
        }
    }
    
    private AdventureSkill LoadSkillByName(string skillName)
    {
        // Try to load from Resources/Skills/ folder
        AdventureSkill skill = Resources.Load<AdventureSkill>($"Skills/{skillName}");
        
        // If not found, create a default placeholder skill for testing
        if (skill == null)
        {
            Debug.LogWarning($"Skill '{skillName}' not found in Resources/Skills/. Using default placeholder.");
            // For now, return null and skip this skill
            return null;
        }
        
        return skill;
    }
    
    private void CreateSkillUI()
    {
        if (playerSkills.Count == 0) return;
        
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
        int skillCount = playerSkills.Count;
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
        for (int i = 0; i < playerSkills.Count; i++)
        {
            CreateSkillButton(panelObj, i);
        }
    }
    
    private void CreateSkillButton(GameObject parent, int skillIndex)
    {
        AdventureSkill skill = playerSkills[skillIndex];
        
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
        
        // Add text elements
        CreateSkillButtonText(buttonObj, skill);
        skillButtons.Add(buttonObj);
    }
    
    private void CreateSkillButtonText(GameObject buttonObj, AdventureSkill skill)
    {
        // Create skill name text
        GameObject nameTextObj = new GameObject("SkillName");
        nameTextObj.transform.SetParent(buttonObj.transform, false);
        
        Text nameText = nameTextObj.AddComponent<Text>();
        nameText.text = skill.skillName;
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
        mpText.text = $"MP: {skill.mpCost}";
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
        cooldownText.text = $"CD: {skill.cooldownSteps}";
        cooldownText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        cooldownText.fontSize = 10;
        cooldownText.color = Color.yellow;
        cooldownText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform cooldownRect = cooldownTextObj.GetComponent<RectTransform>();
        cooldownRect.anchorMin = new Vector2(0, 0);
        cooldownRect.anchorMax = new Vector2(1, 0.3f);
        cooldownRect.offsetMin = new Vector2(5, 0);
        cooldownRect.offsetMax = new Vector2(-5, 0);
    }
    
    private void UseSkill(int skillIndex)
    {
        if (skillIndex >= playerSkills.Count) return;
        
        AdventureSkill skill = playerSkills[skillIndex];
        
        // Handle cancellation if same skill clicked while targeting
        if (isWaitingForTargetSelection && pendingSkillIndex == skillIndex)
        {
            CancelSkillTargeting();
            return;
        }
        
        // Cancel previous targeting if different skill
        if (isWaitingForTargetSelection)
        {
            CancelSkillTargeting();
        }
        
        // Check if skill is on cooldown
        if (skillCooldowns[skillIndex] > 0)
        {
            return;
        }
        
        // Check if player has enough MP
        AdventureGameManager gameManager = FindObjectOfType<AdventureGameManager>();
        if (gameManager == null || !gameManager.HasEnoughMP(skill.mpCost))
        {
            return;
        }
        
        // Handle targeting vs direct execution
        if (skill.requiresTargeting)
        {
            StartSkillTargeting(skillIndex, skill);
        }
        else
        {
            ExecuteSkillDirectly(skillIndex, skill);
        }
    }
    
    private void StartSkillTargeting(int skillIndex, AdventureSkill skill)
    {
        isWaitingForTargetSelection = true;
        pendingSkillIndex = skillIndex;
        pendingSkill = skill;
        
        skill.ApplyTargetingVisuals();
        UpdateSkillButtonVisuals();
    }
    
    private void CancelSkillTargeting()
    {
        if (pendingSkill != null)
        {
            pendingSkill.RemoveTargetingVisuals();
        }
        
        isWaitingForTargetSelection = false;
        pendingSkillIndex = -1;
        pendingSkill = null;
        
        UpdateSkillButtonVisuals();
    }
    
    private void ExecuteSkillDirectly(int skillIndex, AdventureSkill skill)
    {
        ConsumeSkill(skillIndex, skill);
        skill.ExecuteSkill(skillLevels[skillIndex]);
    }
    
    public void OnTileClicked(int row, int col)
    {
        if (!isWaitingForTargetSelection || pendingSkill == null) return;
        
        if (!pendingSkill.IsValidTarget(row, col))
        {
            return;
        }
        
        // Execute the skill with target
        ConsumeSkill(pendingSkillIndex, pendingSkill);
        pendingSkill.ExecuteSkill(skillLevels[pendingSkillIndex], new Vector2Int(row, col));
        
        // Clear targeting state
        CancelSkillTargeting();
    }
    
    private void ConsumeSkill(int skillIndex, AdventureSkill skill)
    {
        AdventureGameManager gameManager = FindObjectOfType<AdventureGameManager>();
        gameManager.UseMP(skill.mpCost);
        skillCooldowns[skillIndex] = skill.cooldownSteps;
        UpdateSkillButtonVisuals();
    }
    
    private void CheckForStepChanges()
    {
        AdventureGameManager gameManager = FindObjectOfType<AdventureGameManager>();
        if (gameManager == null) return;
        
        int currentSteps = gameManager.GetCurrentSteps();
        
        // If steps decreased (player used steps), reduce cooldowns
        if (currentSteps < lastPlayerSteps)
        {
            int stepsUsed = lastPlayerSteps - currentSteps;
            ReduceCooldowns(stepsUsed);
            lastPlayerSteps = currentSteps;
        }
        // Update lastPlayerSteps even if no change to keep it current
        else if (currentSteps != lastPlayerSteps)
        {
            lastPlayerSteps = currentSteps;
        }
    }
    
    private void ReduceCooldowns(int stepsUsed)
    {
        bool cooldownsChanged = false;
        
        for (int i = 0; i < playerSkills.Count; i++)
        {
            if (skillCooldowns[i] > 0)
            {
                skillCooldowns[i] -= stepsUsed;
                if (skillCooldowns[i] < 0) skillCooldowns[i] = 0;
                cooldownsChanged = true;
            }
        }
        
        if (cooldownsChanged)
        {
            UpdateSkillButtonVisuals();
        }
    }
    
    private void UpdateSkillButtonVisuals()
    {
        for (int i = 0; i < skillButtons.Count; i++)
        {
            GameObject buttonObj = skillButtons[i];
            if (buttonObj == null) continue;
            
            AdventureSkill skill = playerSkills[i];
            Button button = buttonObj.GetComponent<Button>();
            Image buttonImage = buttonObj.GetComponent<Image>();
            
            AdventureGameManager gameManager = FindObjectOfType<AdventureGameManager>();
            bool hasEnoughMP = gameManager != null && gameManager.HasEnoughMP(skill.mpCost);
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
                    cooldownText.text = $"CD: {skill.cooldownSteps}";
                    cooldownText.color = Color.yellow;
                }
            }
        }
    }
    
    private void SubscribeToTileClicks()
    {
        // Find all hex tiles and add skill targeting click handlers
        HexTileController[] allTiles = FindObjectsOfType<HexTileController>();
        foreach (HexTileController tile in allTiles)
        {
            // Add skill targeting click handler if it doesn't already exist
            if (tile.GetComponent<SkillTargetingClickHandler>() == null)
            {
                tile.gameObject.AddComponent<SkillTargetingClickHandler>();
            }
        }
    }
    
    public bool IsWaitingForTargetSelection()
    {
        return isWaitingForTargetSelection;
    }
}