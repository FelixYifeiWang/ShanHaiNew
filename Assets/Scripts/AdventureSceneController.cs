using UnityEngine;
using UnityEngine.UI;

public class AdventureSceneController : MonoBehaviour
{
    private Button returnButton;
    
    void Start()
    {
        // Ensure we have an EventSystem for UI interactions
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("Created EventSystem for UI interactions");
        }
        
        CreateReturnButton();
        
        // Start fade in effect when scene loads
        StartSceneFadeIn();
        
        Debug.Log("Adventure Scene loaded!");
    }
    
    private void StartSceneFadeIn()
    {
        // Scene starts with black screen, then fades in
        SceneFadeManager fadeManager = FindObjectOfType<SceneFadeManager>();
        if (fadeManager != null)
        {
            fadeManager.StartSceneFadeIn(1f);
        }
    }
    
    private void CreateReturnButton()
    {
        // Find current scene's canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create return button
        GameObject buttonObj = new GameObject("ReturnButton");
        buttonObj.transform.SetParent(canvas.transform, false);
        
        // Position button in top left with HIGHEST sorting order
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0, 1);
        buttonRect.anchorMax = new Vector2(0, 1);
        buttonRect.pivot = new Vector2(0, 1);
        buttonRect.anchoredPosition = new Vector2(20, -20);
        buttonRect.sizeDelta = new Vector2(150, 50);
        buttonRect.localScale = Vector3.one;
        
        // Add Image component with HIGHEST priority
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 1f); // Solid green
        buttonImage.raycastTarget = true;
        
        // Add Button component
        returnButton = buttonObj.AddComponent<Button>();
        returnButton.targetGraphic = buttonImage;
        returnButton.interactable = true;
        returnButton.onClick.AddListener(() => {
            Debug.Log("Return Home button clicked!");
            ReturnToMainScene();
        });
        
        // SIMPLE: Just add button to main canvas - no separate canvas needed
        // The button will naturally appear on top since it's added after fade panel
        
        // Create button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        textRect.localScale = Vector3.one;
        
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = "Return Home";
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.fontSize = 16;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.raycastTarget = false; // Text shouldn't block raycasts
        
        Debug.Log("Return button created successfully!");
    }
    
    private void ReturnToMainScene()
    {
        Debug.Log("Return button clicked!");
        
        // Check if SceneFadeManager exists before using it
        SceneFadeManager fadeManager = FindObjectOfType<SceneFadeManager>();
        if (fadeManager != null)
        {
            // Start fade out, then transition when complete
            fadeManager.StartSceneFadeOut(1f, () => {
                // This callback runs after fade out completes
                LoadMainSceneFallback();
            });
        }
        else
        {
            // Direct transition if no fade manager
            LoadMainSceneFallback();
        }
    }
    
    private void LoadMainSceneFallback()
    {
        if (GameSceneManager.Instance == null)
        {
            Debug.LogError("GameSceneManager.Instance is null!");
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
        else
        {
            GameSceneManager.Instance.ReturnToMainScene();
        }
    }
}