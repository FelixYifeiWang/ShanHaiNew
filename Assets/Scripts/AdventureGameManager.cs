using UnityEngine;
using UnityEngine.UI;

public class AdventureGameManager : MonoBehaviour
{
    [Header("Adventure Stats")]
    [SerializeField] private int maxSteps = 60;
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int maxMP = 100;  // NEW: MP system
    
    private int currentSteps;
    private int currentHP;
    private int currentMP;  // NEW: Current MP
    
    [Header("UI Elements")]
    private Text stepsText;
    private Text hpText;
    private Text mpText;  // NEW: MP text
    private Text buffText;  // NEW: Buff text
    private GameObject uiPanel;
    private AdventureGameOverUI gameOverUI;
    private AdventureSuccessUI gameSuccessUI;
    
    void Start()
    {
        // Initialize stats
        currentSteps = maxSteps;
        currentHP = maxHP;
        currentMP = maxMP;  // NEW: Initialize MP
        
        CreateUI();
        UpdateUI();
        
        // Get reference to game over UI
        gameOverUI = FindObjectOfType<AdventureGameOverUI>();
        if (gameOverUI == null)
        {
            // Create game over UI if it doesn't exist
            GameObject gameOverObj = new GameObject("AdventureGameOverUI");
            gameOverUI = gameOverObj.AddComponent<AdventureGameOverUI>();
        }

        gameSuccessUI = FindObjectOfType<AdventureSuccessUI>();
        if (gameSuccessUI == null)
        {
            GameObject gameSuccessObj = new GameObject("AdventureSuccessUI");
            gameSuccessUI = gameSuccessObj.AddComponent<AdventureSuccessUI>();
        }
        
        // NEW: Initialize skill UI
        AdventureSkillUI skillUI = FindObjectOfType<AdventureSkillUI>();
        if (skillUI == null)
        {
            GameObject skillUIObj = new GameObject("AdventureSkillUI");
            skillUIObj.AddComponent<AdventureSkillUI>();
        }
    }
    
    private void CreateUI()
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
        
        // Create UI panel for stats
        GameObject panelObj = new GameObject("AdventureStatsPanel");
        panelObj.transform.SetParent(canvas.transform);
        uiPanel = panelObj;
        
        // Position panel in bottom right - CHANGED: Made taller for MP and Buff
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 0);
        panelRect.anchorMax = new Vector2(1, 0);
        panelRect.pivot = new Vector2(1, 0);
        panelRect.anchoredPosition = new Vector2(-10, 10);
        panelRect.sizeDelta = new Vector2(220, 120);  // CHANGED: Width 220, Height 120
        
        // Create Steps text
        GameObject stepsObj = new GameObject("StepsText");
        stepsObj.transform.SetParent(panelObj.transform);
        stepsText = stepsObj.AddComponent<Text>();
        stepsText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        stepsText.fontSize = 16;
        stepsText.color = Color.black;
        stepsText.alignment = TextAnchor.MiddleRight;
        
        RectTransform stepsRect = stepsObj.GetComponent<RectTransform>();
        stepsRect.anchorMin = new Vector2(0, 0.75f);  // CHANGED: For 4 stats
        stepsRect.anchorMax = new Vector2(1, 1);
        stepsRect.offsetMin = Vector2.zero;
        stepsRect.offsetMax = Vector2.zero;
        
        // Create HP text
        GameObject hpObj = new GameObject("HPText");
        hpObj.transform.SetParent(panelObj.transform);
        hpText = hpObj.AddComponent<Text>();
        hpText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        hpText.fontSize = 16;
        hpText.color = Color.black;
        hpText.alignment = TextAnchor.MiddleRight;
        
        RectTransform hpRect = hpObj.GetComponent<RectTransform>();
        hpRect.anchorMin = new Vector2(0, 0.5f);  // CHANGED: For 4 stats
        hpRect.anchorMax = new Vector2(1, 0.75f);  // CHANGED: For 4 stats
        hpRect.offsetMin = Vector2.zero;
        hpRect.offsetMax = Vector2.zero;
        
        // NEW: Create MP text
        GameObject mpObj = new GameObject("MPText");
        mpObj.transform.SetParent(panelObj.transform);
        mpText = mpObj.AddComponent<Text>();
        mpText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        mpText.fontSize = 16;
        mpText.color = Color.black;
        mpText.alignment = TextAnchor.MiddleRight;
        
        RectTransform mpRect = mpObj.GetComponent<RectTransform>();
        mpRect.anchorMin = new Vector2(0, 0.25f);  // CHANGED: For 4 stats
        mpRect.anchorMax = new Vector2(1, 0.5f);   // CHANGED: For 4 stats
        mpRect.offsetMin = Vector2.zero;
        mpRect.offsetMax = Vector2.zero;
        
        // NEW: Create Buff text
        GameObject buffObj = new GameObject("BuffText");
        buffObj.transform.SetParent(panelObj.transform);
        buffText = buffObj.AddComponent<Text>();
        buffText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buffText.fontSize = 14;  // Slightly smaller font
        buffText.color = Color.blue;  // Blue color to distinguish from stats
        buffText.alignment = TextAnchor.MiddleRight;
        
        RectTransform buffRect = buffObj.GetComponent<RectTransform>();
        buffRect.anchorMin = new Vector2(0, 0);
        buffRect.anchorMax = new Vector2(1, 0.25f);
        buffRect.offsetMin = Vector2.zero;
        buffRect.offsetMax = Vector2.zero;
    }
    
    private void UpdateUI()
    {
        if (stepsText != null)
        {
            stepsText.text = $"Steps: {currentSteps}/{maxSteps}";
        }
        
        if (hpText != null)
        {
            hpText.text = $"HP: {currentHP}/{maxHP}";
        }
        
        // NEW: Update MP text
        if (mpText != null)
        {
            mpText.text = $"MP: {currentMP}/{maxMP}";
        }
        
        // NEW: Update Buff text
        if (buffText != null)
        {
            string selectedBuff = AdventureDataManager.GetSelectedBuff();
            buffText.text = !string.IsNullOrEmpty(selectedBuff) ? $"Buff: {selectedBuff}" : "Buff: None";
        }
    }
    
    public void UseStep()
    {
        if (currentSteps > 0)
        {
            currentSteps--;
            UpdateUI();
            CheckGameOver();
        }
    }
    
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        UpdateUI();
        CheckGameOver();
    }
    
    public void LoseSteps(int steps)
    {
        currentSteps -= steps;
        if (currentSteps < 0) currentSteps = 0;
        UpdateUI();
        CheckGameOver();
    }
    
    // NEW: MP management methods
    public void UseMP(int amount)
    {
        currentMP -= amount;
        if (currentMP < 0) currentMP = 0;
        UpdateUI();
    }
    
    public void RestoreMP(int amount)
    {
        currentMP += amount;
        if (currentMP > maxMP) currentMP = maxMP;
        UpdateUI();
    }
    
    public bool HasEnoughMP(int amount)
    {
        return currentMP >= amount;
    }
    
    private void CheckGameOver()
    {
        if (gameOverUI == null) return;
        
        if (currentHP <= 0)
        {
            gameOverUI.ShowGameOver("You ran out of health!\nThe adventure was too dangerous.");
        }
        else if (currentSteps <= 0)
        {
            gameOverUI.ShowGameOver("You ran out of steps!\nTime to head back home.");
        }
    }
    
    // Getter methods
    public int GetCurrentSteps()
    {
        return currentSteps;
    }
    
    public int GetCurrentHP()
    {
        return currentHP;
    }
    
    public int GetMaxSteps()
    {
        return maxSteps;
    }
    
    public int GetMaxHP()
    {
        return maxHP;
    }
    
    // NEW: MP getter methods
    public int GetCurrentMP()
    {
        return currentMP;
    }
    
    public int GetMaxMP()
    {
        return maxMP;
    }
    
    public bool IsGameOver()
    {
        return currentSteps <= 0 || currentHP <= 0;
    }
    
    // Setter methods for buff system
    public void SetMaxSteps(int newMaxSteps)
    {
        maxSteps = newMaxSteps;
        UpdateUI();
    }
    
    public void SetMaxHP(int newMaxHP)
    {
        maxHP = newMaxHP;
        UpdateUI();
    }
    
    public void SetCurrentSteps(int newCurrentSteps)
    {
        currentSteps = newCurrentSteps;
        UpdateUI();
    }
    
    public void SetCurrentHP(int newCurrentHP)
    {
        currentHP = newCurrentHP;
        UpdateUI();
    }
    
    // NEW: MP setter methods for buff system
    public void SetMaxMP(int newMaxMP)
    {
        maxMP = newMaxMP;
        UpdateUI();
    }
    
    public void SetCurrentMP(int newCurrentMP)
    {
        currentMP = newCurrentMP;
        UpdateUI();
    }
}