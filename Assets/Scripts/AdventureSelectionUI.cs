using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AdventureSelectionUI : MonoBehaviour
{
    private GameObject selectionPanel;
    private GameObject buffArea;
    private GameObject skillArea;
    private GameObject buffSelectionArea;
    private bool isShowing = false;
    private bool isBuffExpanded = false;
    private string selectedBuff = "Buff_0";
    private List<Button> buffButtons = new List<Button>();
    private List<int> selectedSkills = new List<int>();
    private int maxSelectedSkills = 3;
    
    [System.Serializable]
    public class BuffData
    {
        public string name;
        public string description;
        public bool isUnlocked;
        
        public BuffData(string buffName, string buffDesc, bool unlocked = false)
        {
            name = buffName;
            description = buffDesc;
            isUnlocked = unlocked;
        }
    }
    
    [System.Serializable]
    public class SkillData
    {
        public string name;
        public string description;
        public int level;
        public int mpCost;
        public int cooldownSteps;
        
        public SkillData(string skillName, string skillDesc, int skillLevel, int mp, int cooldown)
        {
            name = skillName;
            description = skillDesc;
            level = skillLevel;
            mpCost = mp;
            cooldownSteps = cooldown;
        }
    }
    
    private List<BuffData> allBuffs = new List<BuffData>();
    private List<SkillData> allSkills = new List<SkillData>();
    
    // Singleton pattern
    private static AdventureSelectionUI instance;
    public static AdventureSelectionUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AdventureSelectionUI>();
                if (instance == null)
                {
                    GameObject selectionObj = new GameObject("AdventureSelectionUI");
                    instance = selectionObj.AddComponent<AdventureSelectionUI>();
                }
            }
            return instance;
        }
    }
    
    void Awake()
    {
        instance = this;
        selectedBuff = "Buff_0";
        InitializeBuffData();
        InitializeSkillData();
    }
    
    void Start()
    {
        CreateAdventureSelectionUI();
    }
    
    private void InitializeBuffData()
    {
        allBuffs.Clear();
        allBuffs.Add(new BuffData("Blue Bird青鸟", "Increase HP and steps by 15%", true));
        allBuffs.Add(new BuffData("Red Phoenix红凤", "Increase attack damage by 20%", false));
        allBuffs.Add(new BuffData("Green Turtle绿龟", "Increase defense by 25%", false));
        allBuffs.Add(new BuffData("Golden Dragon金龙", "Double gold collection rate", false));
        allBuffs.Add(new BuffData("Silver Wolf银狼", "Increase movement speed by 30%", false));
        allBuffs.Add(new BuffData("Purple Snake紫蛇", "Poison enemies on contact", false));
        allBuffs.Add(new BuffData("White Tiger白虎", "Critical hit chance +15%", false));
        allBuffs.Add(new BuffData("Black Bear黑熊", "Reduce all damage by 10%", false));
    }
    
    private void InitializeSkillData()
    {
        allSkills.Clear();
        allSkills.Add(new SkillData("Umbrella", "Remove the bombs around the selected tile", 1, 20, 2));
        
        for (int i = 1; i <= 30; i++)
        {
            allSkills.Add(new SkillData($"Skill {i}", $"Placeholder skill {i} description", 0, 10, 1));
        }
    }
    
    private void CreateAdventureSelectionUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        GameObject panelObj = new GameObject("AdventureSelectionPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        selectionPanel = panelObj;
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.8f, 0.7f, 0.6f, 1f);
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        Canvas panelCanvas = panelObj.AddComponent<Canvas>();
        panelCanvas.overrideSorting = true;
        panelCanvas.sortingOrder = 200;
        panelObj.AddComponent<GraphicRaycaster>();
        
        CreateBuffArea(panelObj);
        CreateSkillArea(panelObj);
        CreateStartAdventureButton(panelObj);
        CreateCloseButton(panelObj);
        CreateSkillDescriptionArea(panelObj);
        
        selectionPanel.SetActive(false);
    }
    
    private void CreateBuffArea(GameObject parent)
    {
        GameObject buffAreaObj = new GameObject("BuffArea");
        buffAreaObj.transform.SetParent(parent.transform, false);
        buffArea = buffAreaObj;
        
        RectTransform buffRect = buffAreaObj.AddComponent<RectTransform>();
        buffRect.anchorMin = new Vector2(0.05f, 0.25f);
        buffRect.anchorMax = new Vector2(0.35f, 0.9f);
        buffRect.offsetMin = Vector2.zero;
        buffRect.offsetMax = Vector2.zero;
        
        Image buffBg = buffAreaObj.AddComponent<Image>();
        buffBg.color = new Color(0.9f, 0.8f, 0.7f, 1f);
        
        Outline buffOutline = buffAreaObj.AddComponent<Outline>();
        buffOutline.effectColor = Color.black;
        buffOutline.effectDistance = new Vector2(2, 2);
        
        Button buffAreaButton = buffAreaObj.AddComponent<Button>();
        buffAreaButton.onClick.AddListener(ExpandBuffArea);
        
        CreateSingleBuffDisplay(buffAreaObj);
        CreateBuffSelectionArea(parent);
    }
    
    private void CreateSingleBuffDisplay(GameObject parent)
    {
        GameObject buffDisplayObj = new GameObject("BuffDisplay");
        buffDisplayObj.transform.SetParent(parent.transform, false);
        
        RectTransform displayRect = buffDisplayObj.AddComponent<RectTransform>();
        displayRect.anchorMin = new Vector2(0.1f, 0.1f);
        displayRect.anchorMax = new Vector2(0.9f, 0.9f);
        displayRect.offsetMin = Vector2.zero;
        displayRect.offsetMax = Vector2.zero;
        
        Image buffImage = buffDisplayObj.AddComponent<Image>();
        buffImage.color = new Color(1f, 0.8f, 0.4f, 1f);
        
        Outline outline = buffDisplayObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, 2);
        
        GameObject textObj = new GameObject("BuffText");
        textObj.transform.SetParent(buffDisplayObj.transform, false);
        
        Text buffText = textObj.AddComponent<Text>();
        buffText.text = $"Selected: {allBuffs[0].name}";
        buffText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buffText.fontSize = 18;
        buffText.color = Color.black;
        buffText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    private void CreateBuffSelectionArea(GameObject parent)
    {
        GameObject buffSelectionObj = new GameObject("BuffSelectionArea");
        buffSelectionObj.transform.SetParent(parent.transform, false);
        buffSelectionArea = buffSelectionObj;
        
        RectTransform selectionRect = buffSelectionObj.AddComponent<RectTransform>();
        selectionRect.anchorMin = new Vector2(0.05f, 0.25f);
        selectionRect.anchorMax = new Vector2(0.95f, 0.9f);
        selectionRect.offsetMin = Vector2.zero;
        selectionRect.offsetMax = Vector2.zero;
        
        Image selectionBg = buffSelectionObj.AddComponent<Image>();
        selectionBg.color = new Color(0.9f, 0.8f, 0.7f, 1f);
        
        Outline outline = buffSelectionObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, 2);
        
        CreateExpandedBuffGrid(buffSelectionObj);
        buffSelectionArea.SetActive(false);
    }
    
    private void CreateExpandedBuffGrid(GameObject parent)
    {
        GameObject buffContentObj = new GameObject("BuffContent");
        buffContentObj.transform.SetParent(parent.transform, false);
        
        RectTransform contentRect = buffContentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = new Vector2(20, 20);
        contentRect.offsetMax = new Vector2(-20, -20);
        
        GridLayoutGroup gridLayout = buffContentObj.AddComponent<GridLayoutGroup>();
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 4;
        gridLayout.cellSize = new Vector2(140, 140);
        gridLayout.spacing = new Vector2(15, 15);
        gridLayout.padding = new RectOffset(15, 15, 15, 15);
        
        buffButtons.Clear();
        for (int i = 0; i < 8; i++)
        {
            CreateExpandedBuffSlot(buffContentObj, i);
        }
    }
    
    private void CreateExpandedBuffSlot(GameObject parent, int index)
    {
        GameObject buffSlotObj = new GameObject($"BuffSlot_{index}");
        buffSlotObj.transform.SetParent(parent.transform, false);
        
        BuffData buffData = allBuffs[index];
        
        Button buffButton = buffSlotObj.AddComponent<Button>();
        
        if (buffData.isUnlocked)
        {
            buffButton.onClick.AddListener(() => SelectBuff($"Buff_{index}"));
        }
        buffButton.interactable = buffData.isUnlocked;
        buffButtons.Add(buffButton);
        
        Image buffImage = buffSlotObj.AddComponent<Image>();
        if (!buffData.isUnlocked)
        {
            buffImage.color = new Color(0.4f, 0.4f, 0.4f, 0.7f);
        }
        else if (index == 0)
        {
            buffImage.color = new Color(1f, 0.8f, 0.4f, 1f);
        }
        else
        {
            buffImage.color = new Color(0.7f, 0.6f, 0.5f, 1f);
        }
        
        Outline outline = buffSlotObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, 2);
        
        GameObject nameTextObj = new GameObject("BuffName");
        nameTextObj.transform.SetParent(buffSlotObj.transform, false);
        
        Text nameText = nameTextObj.AddComponent<Text>();
        nameText.text = buffData.name;
        nameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        nameText.fontSize = 14;
        nameText.color = buffData.isUnlocked ? Color.white : new Color(0.6f, 0.6f, 0.6f);
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.fontStyle = FontStyle.Bold;
        
        RectTransform nameRect = nameTextObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.7f);
        nameRect.anchorMax = new Vector2(1, 0.9f);
        nameRect.offsetMin = new Vector2(5, 0);
        nameRect.offsetMax = new Vector2(-5, 0);
        
        if (!buffData.isUnlocked)
        {
            GameObject lockObj = new GameObject("LockIcon");
            lockObj.transform.SetParent(buffSlotObj.transform, false);
            
            Text lockText = lockObj.AddComponent<Text>();
            lockText.text = "🔒";
            lockText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            lockText.fontSize = 28;
            lockText.color = new Color(0.8f, 0.8f, 0.8f);
            lockText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform lockRect = lockObj.GetComponent<RectTransform>();
            lockRect.anchorMin = new Vector2(0, 0.2f);
            lockRect.anchorMax = new Vector2(1, 0.6f);
            lockRect.offsetMin = Vector2.zero;
            lockRect.offsetMax = Vector2.zero;
        }
    }
    
    private void CreateSkillArea(GameObject parent)
    {
        GameObject skillAreaObj = new GameObject("SkillArea");
        skillAreaObj.transform.SetParent(parent.transform, false);
        skillArea = skillAreaObj;
        
        RectTransform skillRect = skillAreaObj.AddComponent<RectTransform>();
        skillRect.anchorMin = new Vector2(0.4f, 0.25f);
        skillRect.anchorMax = new Vector2(0.95f, 0.9f);
        skillRect.offsetMin = Vector2.zero;
        skillRect.offsetMax = Vector2.zero;
        
        Image skillBg = skillAreaObj.AddComponent<Image>();
        skillBg.color = new Color(0.9f, 0.8f, 0.7f, 1f);
        
        Outline skillOutline = skillAreaObj.AddComponent<Outline>();
        skillOutline.effectColor = Color.black;
        skillOutline.effectDistance = new Vector2(2, 2);
        
        CreateSkillGrid(skillAreaObj);
    }
    
    private void CreateSkillGrid(GameObject parent)
    {
        GameObject skillContentObj = new GameObject("SkillContent");
        skillContentObj.transform.SetParent(parent.transform, false);
        
        RectTransform contentRect = skillContentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.offsetMin = new Vector2(15, -999);
        contentRect.offsetMax = new Vector2(-15, -15);
        
        GridLayoutGroup gridLayout = skillContentObj.AddComponent<GridLayoutGroup>();
        gridLayout.constraint = GridLayoutGroup.Constraint.Flexible;
        gridLayout.cellSize = new Vector2(120, 120);
        gridLayout.spacing = new Vector2(10, 10);
        gridLayout.padding = new RectOffset(10, 10, 10, 10);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
        
        ContentSizeFitter fitter = skillContentObj.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        int numberOfSkills = allSkills.Count;
        for (int i = 0; i < numberOfSkills; i++)
        {
            // Only create slots for skills with level > 0
            if (allSkills[i].level > 0)
            {
                CreateSkillSlot(skillContentObj, i);
            }
        }
    }
    
    private void CreateSkillSlot(GameObject parent, int index)
    {
        GameObject skillSlotObj = new GameObject($"SkillSlot_{index}");
        skillSlotObj.transform.SetParent(parent.transform, false);
        
        SkillData skillData = allSkills[index];
        
        Image skillImage = skillSlotObj.AddComponent<Image>();
        UpdateSkillVisual(skillSlotObj, index);
        
        Outline outline = skillSlotObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, 1);
        
        GameObject nameTextObj = new GameObject("SkillName");
        nameTextObj.transform.SetParent(skillSlotObj.transform, false);
        
        Text nameText = nameTextObj.AddComponent<Text>();
        nameText.text = skillData.name;
        nameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        nameText.fontSize = 14;
        nameText.color = Color.black;
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.fontStyle = FontStyle.Bold;
        nameText.raycastTarget = false;
        
        RectTransform nameRect = nameTextObj.GetComponent<RectTransform>();
        nameRect.anchorMin = Vector2.zero;
        nameRect.anchorMax = Vector2.one;
        nameRect.offsetMin = new Vector2(5, 5);
        nameRect.offsetMax = new Vector2(-5, -5);
        
        Button skillButton = skillSlotObj.AddComponent<Button>();
        skillButton.onClick.AddListener(() => ToggleSkillSelection(index));
        
        UnityEngine.EventSystems.EventTrigger trigger = skillSlotObj.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        
        UnityEngine.EventSystems.EventTrigger.Entry enterEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        enterEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { ShowSkillTooltip(skillData, skillSlotObj.transform.position); });
        trigger.triggers.Add(enterEntry);
        
        UnityEngine.EventSystems.EventTrigger.Entry exitEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
        exitEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { HideSkillTooltip(); });
        trigger.triggers.Add(exitEntry);
    }
    
    private GameObject skillTooltip;
    
    private void ShowSkillTooltip(SkillData skill, Vector3 position)
    {
        HideSkillTooltip();
        
        Canvas canvas = FindObjectOfType<Canvas>();
        skillTooltip = new GameObject("SkillTooltip");
        skillTooltip.transform.SetParent(canvas.transform, false);
        
        Image tooltipBg = skillTooltip.AddComponent<Image>();
        tooltipBg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        RectTransform tooltipRect = skillTooltip.GetComponent<RectTransform>();
        tooltipRect.sizeDelta = new Vector2(250, 120);
        
        Vector3 tooltipPosition = position + new Vector3(60 + 10 + 125, 0, 0);
        
        float screenWidth = Screen.width;
        if (tooltipPosition.x + 125 > screenWidth)
        {
            tooltipPosition = position + new Vector3(-60 - 10 - 125, 0, 0);
        }
        
        tooltipRect.position = tooltipPosition;
        
        GameObject textObj = new GameObject("TooltipText");
        textObj.transform.SetParent(skillTooltip.transform, false);
        
        Text tooltipText = textObj.AddComponent<Text>();
        tooltipText.text = $"{skill.name}\n{skill.description}\nLevel: {skill.level}\nMP: {skill.mpCost}\nCooldown: {skill.cooldownSteps} steps";
        tooltipText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        tooltipText.fontSize = 14;
        tooltipText.color = Color.white;
        tooltipText.alignment = TextAnchor.UpperLeft;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
    }
    
    private void HideSkillTooltip()
    {
        if (skillTooltip != null)
        {
            Destroy(skillTooltip);
            skillTooltip = null;
        }
    }
    
    private void CreateSkillDescriptionArea(GameObject parent)
    {
        GameObject descAreaObj = new GameObject("SkillDescriptionArea");
        descAreaObj.transform.SetParent(parent.transform, false);
        
        RectTransform descRect = descAreaObj.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0.05f, 0.05f);
        descRect.anchorMax = new Vector2(0.35f, 0.2f);
        descRect.offsetMin = Vector2.zero;
        descRect.offsetMax = Vector2.zero;
        
        Image descBg = descAreaObj.AddComponent<Image>();
        descBg.color = new Color(0.95f, 0.9f, 0.85f, 1f);
        
        Outline outline = descAreaObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, 1);
        
        GameObject textObj = new GameObject("DescriptionText");
        textObj.transform.SetParent(descAreaObj.transform, false);
        
        Text descText = textObj.AddComponent<Text>();
        descText.text = $"{allBuffs[0].name}\n{allBuffs[0].description}";
        descText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        descText.fontSize = 16;
        descText.color = Color.black;
        descText.alignment = TextAnchor.UpperLeft;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
    }
    
    private void CreateStartAdventureButton(GameObject parent)
    {
        GameObject buttonObj = new GameObject("StartAdventureButton");
        buttonObj.transform.SetParent(parent.transform, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.4f, 0.05f);
        buttonRect.anchorMax = new Vector2(0.95f, 0.15f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        Button startButton = buttonObj.AddComponent<Button>();
        startButton.onClick.AddListener(StartAdventure);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.6f, 0.4f, 0.2f, 1f);
        
        Outline outline = buttonObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, 2);
        
        GameObject textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = "start adventure";
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.fontSize = 20;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    private void CreateCloseButton(GameObject parent)
    {
        GameObject closeButtonObj = new GameObject("CloseButton");
        closeButtonObj.transform.SetParent(parent.transform, false);
        
        RectTransform closeButtonRect = closeButtonObj.AddComponent<RectTransform>();
        closeButtonRect.anchorMin = new Vector2(0.92f, 0.92f);
        closeButtonRect.anchorMax = new Vector2(0.97f, 0.97f);
        closeButtonRect.offsetMin = Vector2.zero;
        closeButtonRect.offsetMax = Vector2.zero;
        
        Button closeButton = closeButtonObj.AddComponent<Button>();
        closeButton.onClick.AddListener(CloseSelectionUI);
        
        Image closeButtonImage = closeButtonObj.AddComponent<Image>();
        closeButtonImage.color = new Color(0.6f, 0.5f, 0.4f, 0.8f);
        
        Outline outline = closeButtonObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, 1);
        
        GameObject textObj = new GameObject("CloseText");
        textObj.transform.SetParent(closeButtonObj.transform, false);
        
        Text closeText = textObj.AddComponent<Text>();
        closeText.text = "X";
        closeText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        closeText.fontSize = 16;
        closeText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        closeText.alignment = TextAnchor.MiddleCenter;
        closeText.fontStyle = FontStyle.Normal;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    private void ExpandBuffArea()
    {
        if (!isBuffExpanded)
        {
            buffArea.SetActive(false);
            buffSelectionArea.SetActive(true);
            isBuffExpanded = true;
        }
    }
    
    private void SelectBuff(string buffName)
    {
        selectedBuff = buffName;
        int buffIndex = int.Parse(buffName.Split('_')[1]);
        BuffData selectedBuffData = allBuffs[buffIndex];
        
        for (int i = 0; i < buffButtons.Count; i++)
        {
            if (allBuffs[i].isUnlocked)
            {
                Image buttonImage = buffButtons[i].GetComponent<Image>();
                if (buffName == $"Buff_{i}")
                {
                    buttonImage.color = new Color(1f, 0.8f, 0.4f, 1f);
                }
                else
                {
                    buttonImage.color = new Color(0.7f, 0.6f, 0.5f, 1f);
                }
            }
        }
        
        buffSelectionArea.SetActive(false);
        buffArea.SetActive(true);
        isBuffExpanded = false;
        
        UpdateSingleBuffDisplay(selectedBuffData);
        UpdateSkillDescription(selectedBuffData);
    }
    
    private void UpdateSingleBuffDisplay(BuffData buffData)
    {
        GameObject buffDisplay = buffArea.transform.Find("BuffDisplay").gameObject;
        if (buffDisplay != null)
        {
            Text buffText = buffDisplay.GetComponentInChildren<Text>();
            if (buffText != null)
            {
                buffText.text = $"Selected: {buffData.name}";
            }
        }
    }
    
    private void UpdateSkillDescription(BuffData buffData)
    {
        GameObject descArea = selectionPanel.transform.Find("SkillDescriptionArea").gameObject;
        if (descArea != null)
        {
            Text descText = descArea.GetComponentInChildren<Text>();
            if (descText != null)
            {
                descText.text = $"{buffData.name}\n{buffData.description}";
            }
        }
    }
    
    private void ToggleSkillSelection(int skillIndex)
    {
        if (selectedSkills.Contains(skillIndex))
        {
            // Unselect skill
            selectedSkills.Remove(skillIndex);
        }
        else if (selectedSkills.Count < maxSelectedSkills)
        {
            // Select skill if under limit
            selectedSkills.Add(skillIndex);
        }
        // If at limit and skill not selected, do nothing
        
        UpdateSkillVisual(GameObject.Find($"SkillSlot_{skillIndex}"), skillIndex);
    }
    
    private void UpdateSkillVisual(GameObject skillSlot, int skillIndex)
    {
        SkillData skillData = allSkills[skillIndex];
        Image skillImage = skillSlot.GetComponent<Image>();
        
        if (selectedSkills.Contains(skillIndex))
        {
            // Selected skill - bright blue
            skillImage.color = new Color(0.2f, 0.6f, 1f, 1f);
        }
        else if (skillData.level > 0)
        {
            // Learned but not selected - golden
            skillImage.color = new Color(1f, 0.8f, 0.4f, 1f);
        }
        else
        {
            // Unlearned - brown
            skillImage.color = new Color(0.7f, 0.6f, 0.5f, 1f);
        }
    }
    
    private void CloseSelectionUI()
    {
        HideSelectionUI();
    }
    
    private void StartAdventure()
    {
        // Prepare data to pass to adventure scene
        string buffName = allBuffs[int.Parse(selectedBuff.Split('_')[1])].name;
        
        List<string> skillNames = new List<string>();
        List<int> skillLevels = new List<int>();
        
        foreach (int skillIndex in selectedSkills)
        {
            skillNames.Add(allSkills[skillIndex].name);
            skillLevels.Add(allSkills[skillIndex].level);
        }
        
        // Pass data to adventure data manager
        AdventureDataManager.SetAdventureData(buffName, skillNames, skillLevels);
        
        HideSelectionUI();
        EntranceEffects.ConsumeActPointAndStartAdventure();
    }
    
    public void ShowSelectionUI()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(true);
            isShowing = true;
            
            isBuffExpanded = false;
            buffArea.SetActive(true);
            buffSelectionArea.SetActive(false);
        }
    }
    
    public void HideSelectionUI()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
            isShowing = false;
        }
    }
    
    public bool IsShowing()
    {
        return isShowing;
    }
}