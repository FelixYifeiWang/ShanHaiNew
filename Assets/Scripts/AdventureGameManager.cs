using UnityEngine;
using UnityEngine.UI;

public class AdventureGameManager : MonoBehaviour
{
    [Header("Adventure Stats")]
    [SerializeField] private int maxSteps = 60;
    [SerializeField] private int maxHP = 100;
    
    private int currentSteps;
    private int currentHP;
    
    [Header("UI Elements")]
    private Text stepsText;
    private Text hpText;
    private GameObject uiPanel;
    private AdventureGameOverUI gameOverUI;
    private AdventureSuccessUI gameSuccessUI;
    
    void Start()
    {
        // Initialize stats
        currentSteps = maxSteps;
        currentHP = maxHP;
        
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
        
        // Position panel in bottom right
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 0);
        panelRect.anchorMax = new Vector2(1, 0);
        panelRect.pivot = new Vector2(1, 0);
        panelRect.anchoredPosition = new Vector2(-10, 10);
        panelRect.sizeDelta = new Vector2(200, 60);
        
        // Create Steps text
        GameObject stepsObj = new GameObject("StepsText");
        stepsObj.transform.SetParent(panelObj.transform);
        stepsText = stepsObj.AddComponent<Text>();
        stepsText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        stepsText.fontSize = 16;
        stepsText.color = Color.black;
        stepsText.alignment = TextAnchor.MiddleRight;
        
        RectTransform stepsRect = stepsObj.GetComponent<RectTransform>();
        stepsRect.anchorMin = new Vector2(0, 0.5f);
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
        hpRect.anchorMin = new Vector2(0, 0);
        hpRect.anchorMax = new Vector2(1, 0.5f);
        hpRect.offsetMin = Vector2.zero;
        hpRect.offsetMax = Vector2.zero;
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
    
    public int GetCurrentSteps()
    {
        return currentSteps;
    }
    
    public int GetCurrentHP()
    {
        return currentHP;
    }
    
    public bool IsGameOver()
    {
        return currentSteps <= 0 || currentHP <= 0;
    }
}