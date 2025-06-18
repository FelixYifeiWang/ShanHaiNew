using UnityEngine;
using UnityEngine.UI;

public class UniversalPauseMenu : MonoBehaviour
{
    private GameObject pauseMenuPanel;
    private bool isPauseMenuShowing = false;
    
    // Singleton pattern for easy access
    private static UniversalPauseMenu instance;
    public static UniversalPauseMenu Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UniversalPauseMenu>();
                if (instance == null)
                {
                    GameObject pauseMenuObj = new GameObject("UniversalPauseMenu");
                    instance = pauseMenuObj.AddComponent<UniversalPauseMenu>();
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
            // Don't destroy on load so it works in both scenes
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        CreatePauseMenu();
    }
    
    void Update()
    {
        // Check for ESC key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC key pressed");
            
            // Ensure we have a valid pause menu
            if (pauseMenuPanel == null)
            {
                Debug.LogWarning("Pause menu panel is null, recreating...");
                CreatePauseMenu();
            }
            
            TogglePauseMenu();
        }
    }
    
    private void CreatePauseMenu()
    {
        Debug.Log("Creating pause menu...");
        
        // Clean up existing panel first
        if (pauseMenuPanel != null)
        {
            Destroy(pauseMenuPanel);
            pauseMenuPanel = null;
        }
        
        // Find or create canvas in current scene
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.Log("No canvas found, creating new one");
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        Debug.Log($"Using canvas: {canvas.name}");
        
        // Create pause menu panel (full screen overlay)
        GameObject panelObj = new GameObject("PauseMenuPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        pauseMenuPanel = panelObj;
        
        // Add background image with high alpha to block clicks
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.8f);
        panelImage.raycastTarget = true;
        
        // Add Canvas component to control sorting (highest priority)
        Canvas panelCanvas = panelObj.AddComponent<Canvas>();
        panelCanvas.overrideSorting = true;
        panelCanvas.sortingOrder = 2000; // Higher than other UIs
        
        // Add GraphicRaycaster for button interactions
        panelObj.AddComponent<GraphicRaycaster>();
        
        // Set panel to cover entire screen
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Create content container (centered)
        GameObject contentContainer = new GameObject("ContentContainer");
        contentContainer.transform.SetParent(panelObj.transform, false);
        
        RectTransform containerRect = contentContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(300, 200);
        
        // Add background for content container
        Image contentBg = contentContainer.AddComponent<Image>();
        contentBg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        
        // Create title text
        GameObject titleObj = new GameObject("PauseTitle");
        titleObj.transform.SetParent(contentContainer.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "PAUSED";
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 24;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.fontStyle = FontStyle.Bold;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.7f);
        titleRect.anchorMax = new Vector2(1, 0.9f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        // Create Quit Game button
        GameObject quitButtonObj = new GameObject("QuitGameButton");
        quitButtonObj.transform.SetParent(contentContainer.transform, false);
        
        Image quitButtonImage = quitButtonObj.AddComponent<Image>();
        quitButtonImage.color = new Color(0.6f, 0.2f, 0.2f, 0.9f); // Red background
        
        Button quitButton = quitButtonObj.AddComponent<Button>();
        quitButton.onClick.AddListener(QuitGame);
        
        RectTransform quitButtonRect = quitButtonObj.GetComponent<RectTransform>();
        quitButtonRect.anchorMin = new Vector2(0.2f, 0.4f);
        quitButtonRect.anchorMax = new Vector2(0.8f, 0.6f);
        quitButtonRect.offsetMin = Vector2.zero;
        quitButtonRect.offsetMax = Vector2.zero;
        
        // Create quit button text
        GameObject quitTextObj = new GameObject("QuitText");
        quitTextObj.transform.SetParent(quitButtonObj.transform, false);
        Text quitText = quitTextObj.AddComponent<Text>();
        quitText.text = "Quit Game";
        quitText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        quitText.fontSize = 16;
        quitText.color = Color.white;
        quitText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform quitTextRect = quitTextObj.GetComponent<RectTransform>();
        quitTextRect.anchorMin = Vector2.zero;
        quitTextRect.anchorMax = Vector2.one;
        quitTextRect.offsetMin = Vector2.zero;
        quitTextRect.offsetMax = Vector2.zero;
        
        // Create warning message
        GameObject warningObj = new GameObject("WarningMessage");
        warningObj.transform.SetParent(contentContainer.transform, false);
        Text warningText = warningObj.AddComponent<Text>();
        warningText.text = "Unsaved game will be lost";
        warningText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        warningText.fontSize = 12;
        warningText.color = new Color(1f, 0.8f, 0.8f, 1f); // Light red color
        warningText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform warningRect = warningObj.GetComponent<RectTransform>();
        warningRect.anchorMin = new Vector2(0, 0.25f);
        warningRect.anchorMax = new Vector2(1, 0.35f);
        warningRect.offsetMin = Vector2.zero;
        warningRect.offsetMax = Vector2.zero;
        
        // Create Resume button
        GameObject resumeButtonObj = new GameObject("ResumeButton");
        resumeButtonObj.transform.SetParent(contentContainer.transform, false);
        
        Image resumeButtonImage = resumeButtonObj.AddComponent<Image>();
        resumeButtonImage.color = new Color(0.2f, 0.6f, 0.2f, 0.9f); // Green background
        
        Button resumeButton = resumeButtonObj.AddComponent<Button>();
        resumeButton.onClick.AddListener(HidePauseMenu);
        
        RectTransform resumeButtonRect = resumeButtonObj.GetComponent<RectTransform>();
        resumeButtonRect.anchorMin = new Vector2(0.2f, 0.15f);
        resumeButtonRect.anchorMax = new Vector2(0.8f, 0.35f);
        resumeButtonRect.offsetMin = Vector2.zero;
        resumeButtonRect.offsetMax = Vector2.zero;
        
        // Create resume button text
        GameObject resumeTextObj = new GameObject("ResumeText");
        resumeTextObj.transform.SetParent(resumeButtonObj.transform, false);
        Text resumeText = resumeTextObj.AddComponent<Text>();
        resumeText.text = "Resume";
        resumeText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        resumeText.fontSize = 16;
        resumeText.color = Color.white;
        resumeText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform resumeTextRect = resumeTextObj.GetComponent<RectTransform>();
        resumeTextRect.anchorMin = Vector2.zero;
        resumeTextRect.anchorMax = Vector2.one;
        resumeTextRect.offsetMin = Vector2.zero;
        resumeTextRect.offsetMax = Vector2.zero;
        
        // Start hidden
        pauseMenuPanel.SetActive(false);
        
        Debug.Log("Pause menu created successfully");
    }
    
    private void TogglePauseMenu()
    {
        if (isPauseMenuShowing)
        {
            HidePauseMenu();
        }
        else
        {
            ShowPauseMenu();
        }
    }
    
    private void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            isPauseMenuShowing = true;
            
            // Pause the game time
            Time.timeScale = 0f;
            
            Debug.Log("Pause menu opened");
        }
    }
    
    private void HidePauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
            isPauseMenuShowing = false;
            
            // Resume the game time
            Time.timeScale = 1f;
            
            Debug.Log("Pause menu closed");
        }
    }
    
    private void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        // Quit the application
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public bool IsPauseMenuShowing()
    {
        return isPauseMenuShowing;
    }
    
    void OnDestroy()
    {
        // Reset time scale when destroyed
        Time.timeScale = 1f;
    }
}