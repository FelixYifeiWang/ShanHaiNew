using UnityEngine;
using UnityEngine.UI;

public class GameOverSystem : MonoBehaviour
{
    private static GameOverSystem instance;
    private GameObject gameOverPanel;
    private bool gameIsOver = false;
    
    public static GameOverSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameOverSystem>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GameOverSystem");
                    instance = go.AddComponent<GameOverSystem>();
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
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        CreateGameOverUI();
    }
    
    private void CreateGameOverUI()
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
        
        // Create game over panel
        GameObject panelObj = new GameObject("GameOverPanel");
        panelObj.transform.SetParent(canvas.transform);
        gameOverPanel = panelObj;
        
        // Add background image
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.9f); // Dark background
        
        // Add Canvas component to control sorting (highest priority)
        Canvas panelCanvas = panelObj.AddComponent<Canvas>();
        panelCanvas.overrideSorting = true;
        panelCanvas.sortingOrder = 1000; // Highest priority
        
        // Add GraphicRaycaster for button interactions
        panelObj.AddComponent<GraphicRaycaster>();
        
        // Set panel to cover entire screen
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Create title text
        GameObject titleObj = new GameObject("GameOverTitle");
        titleObj.transform.SetParent(panelObj.transform);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "GAME OVER";
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 48;
        titleText.color = Color.red;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.fontStyle = FontStyle.Bold;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.6f);
        titleRect.anchorMax = new Vector2(1, 0.8f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        // Create reason text
        GameObject reasonObj = new GameObject("GameOverReason");
        reasonObj.transform.SetParent(panelObj.transform);
        Text reasonText = reasonObj.AddComponent<Text>();
        reasonText.text = "Your population starved!\nNot enough food to sustain them.";
        reasonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        reasonText.fontSize = 20;
        reasonText.color = Color.white;
        reasonText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform reasonRect = reasonObj.GetComponent<RectTransform>();
        reasonRect.anchorMin = new Vector2(0, 0.4f);
        reasonRect.anchorMax = new Vector2(1, 0.6f);
        reasonRect.offsetMin = Vector2.zero;
        reasonRect.offsetMax = Vector2.zero;
        
        // Create restart button
        GameObject restartButtonObj = new GameObject("RestartButton");
        restartButtonObj.transform.SetParent(panelObj.transform);
        
        Image restartButtonImage = restartButtonObj.AddComponent<Image>();
        restartButtonImage.color = new Color(0.6f, 0.2f, 0.2f, 0.8f); // Red background
        
        Button restartButton = restartButtonObj.AddComponent<Button>();
        restartButton.onClick.AddListener(RestartGame);
        
        RectTransform restartButtonRect = restartButtonObj.GetComponent<RectTransform>();
        restartButtonRect.anchorMin = new Vector2(0.3f, 0.2f);
        restartButtonRect.anchorMax = new Vector2(0.7f, 0.3f);
        restartButtonRect.offsetMin = Vector2.zero;
        restartButtonRect.offsetMax = Vector2.zero;
        
        // Create restart button text
        GameObject restartTextObj = new GameObject("RestartText");
        restartTextObj.transform.SetParent(restartButtonObj.transform);
        Text restartText = restartTextObj.AddComponent<Text>();
        restartText.text = "Restart Game";
        restartText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        restartText.fontSize = 18;
        restartText.color = Color.white;
        restartText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform restartTextRect = restartTextObj.GetComponent<RectTransform>();
        restartTextRect.anchorMin = Vector2.zero;
        restartTextRect.anchorMax = Vector2.one;
        restartTextRect.offsetMin = Vector2.zero;
        restartTextRect.offsetMax = Vector2.zero;
        
        // Start hidden
        gameOverPanel.SetActive(false);
    }
    
    public void TriggerGameOver(string reason = "Population starved!")
    {
        if (gameIsOver) return; // Prevent multiple triggers
        
        gameIsOver = true;
        Debug.Log($"GAME OVER: {reason}");
        
        // Show the game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // Disable all other UI interactions
        DisableGameplayUI();
    }
    
    private void DisableGameplayUI()
    {
        // Disable next day button
        NextDaySystem nextDaySystem = FindObjectOfType<NextDaySystem>();
        if (nextDaySystem != null)
        {
            Button nextDayButton = nextDaySystem.GetComponent<Button>();
            if (nextDayButton == null)
            {
                nextDayButton = nextDaySystem.GetComponentInChildren<Button>();
            }
            if (nextDayButton != null)
            {
                nextDayButton.interactable = false;
            }
        }
        
        // Disable land unlock button
        LandUnlockSystem landUnlockSystem = FindObjectOfType<LandUnlockSystem>();
        if (landUnlockSystem != null)
        {
            Button unlockButton = landUnlockSystem.GetComponentInChildren<Button>();
            if (unlockButton != null)
            {
                unlockButton.interactable = false;
            }
        }
        
        // Disable building interactions
        BuildingComponent[] allBuildings = FindObjectsOfType<BuildingComponent>();
        foreach (BuildingComponent building in allBuildings)
        {
            Collider2D col = building.GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
            }
        }
    }
    
    private void RestartGame()
    {
        Debug.Log("Restarting game...");
        
        // Delete save file
        SaveSystem saveSystem = FindObjectOfType<SaveSystem>();
        if (saveSystem != null)
        {
            string savePath = System.IO.Path.Combine(Application.persistentDataPath, "gamesave.json");
            if (System.IO.File.Exists(savePath))
            {
                System.IO.File.Delete(savePath);
                Debug.Log("Save file deleted");
            }
        }
        
        // Reload the scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    public bool IsGameOver()
    {
        return gameIsOver;
    }
}