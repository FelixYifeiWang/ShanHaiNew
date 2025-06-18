using UnityEngine;
using UnityEngine.UI;

public class AdventureGameOverUI : MonoBehaviour
{
    private GameObject gameOverPanel;
    private bool isGameOver = false;
    
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
        
        // Create game over panel (full screen overlay)
        GameObject panelObj = new GameObject("AdventureGameOverPanel");
        panelObj.transform.SetParent(canvas.transform);
        gameOverPanel = panelObj;
        
        // Add background image with high alpha to block clicks
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.8f); // Dark semi-transparent background
        panelImage.raycastTarget = true; // Blocks clicks to objects behind it
        
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
        
        // Create content container (centered)
        GameObject contentContainer = new GameObject("ContentContainer");
        contentContainer.transform.SetParent(panelObj.transform);
        
        RectTransform containerRect = contentContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(400, 300);
        
        // Add background for content container
        Image contentBg = contentContainer.AddComponent<Image>();
        contentBg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        // Create title text
        GameObject titleObj = new GameObject("GameOverTitle");
        titleObj.transform.SetParent(contentContainer.transform);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "ADVENTURE FAILED";
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 28;
        titleText.color = Color.red;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.fontStyle = FontStyle.Bold;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.7f);
        titleRect.anchorMax = new Vector2(1, 0.9f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        // Create reason text
        GameObject reasonObj = new GameObject("GameOverReason");
        reasonObj.transform.SetParent(contentContainer.transform);
        Text reasonText = reasonObj.AddComponent<Text>();
        reasonText.text = "You have died in the adventure!\nBetter luck next time.";
        reasonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        reasonText.fontSize = 18;
        reasonText.color = Color.white;
        reasonText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform reasonRect = reasonObj.GetComponent<RectTransform>();
        reasonRect.anchorMin = new Vector2(0, 0.4f);
        reasonRect.anchorMax = new Vector2(1, 0.7f);
        reasonRect.offsetMin = Vector2.zero;
        reasonRect.offsetMax = Vector2.zero;
        
        // Create return home button
        GameObject returnButtonObj = new GameObject("ReturnHomeButton");
        returnButtonObj.transform.SetParent(contentContainer.transform);
        
        Image returnButtonImage = returnButtonObj.AddComponent<Image>();
        returnButtonImage.color = new Color(0.2f, 0.6f, 0.2f, 0.8f); // Green background
        
        Button returnButton = returnButtonObj.AddComponent<Button>();
        returnButton.onClick.AddListener(ReturnHome);
        
        RectTransform returnButtonRect = returnButtonObj.GetComponent<RectTransform>();
        returnButtonRect.anchorMin = new Vector2(0.2f, 0.1f);
        returnButtonRect.anchorMax = new Vector2(0.8f, 0.3f);
        returnButtonRect.offsetMin = Vector2.zero;
        returnButtonRect.offsetMax = Vector2.zero;
        
        // Create return button text
        GameObject returnTextObj = new GameObject("ReturnText");
        returnTextObj.transform.SetParent(returnButtonObj.transform);
        Text returnText = returnTextObj.AddComponent<Text>();
        returnText.text = "Return Home";
        returnText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        returnText.fontSize = 20;
        returnText.color = Color.white;
        returnText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform returnTextRect = returnTextObj.GetComponent<RectTransform>();
        returnTextRect.anchorMin = Vector2.zero;
        returnTextRect.anchorMax = Vector2.one;
        returnTextRect.offsetMin = Vector2.zero;
        returnTextRect.offsetMax = Vector2.zero;
        
        // Start hidden
        gameOverPanel.SetActive(false);
    }
    
    public void ShowGameOver(string reason = "You have died in the adventure!")
    {
        if (isGameOver) return; // Prevent multiple triggers
        
        isGameOver = true;
        
        // Update reason text
        Text reasonText = gameOverPanel.transform.Find("ContentContainer/GameOverReason").GetComponent<Text>();
        if (reasonText != null)
        {
            reasonText.text = reason;
        }
        
        // Show the game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        Debug.Log($"Adventure Game Over: {reason}");
    }
    
    private void ReturnHome()
    {
        Debug.Log("Returning to main scene from adventure...");
        
        // Use the existing scene management system
        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.ReturnToMainScene();
        }
        else
        {
            // Fallback - try to load scene directly
            try
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
            }
            catch
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }
    }
    
    public bool IsGameOver()
    {
        return isGameOver;
    }
}