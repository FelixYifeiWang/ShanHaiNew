using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFadeManager : MonoBehaviour
{
    private static SceneFadeManager instance;
    private GameObject fadePanel;
    private Image fadeImage;
    private bool isFading = false;
    
    public static SceneFadeManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SceneFadeManager>();
                if (instance == null)
                {
                    GameObject fadeObj = new GameObject("SceneFadeManager");
                    instance = fadeObj.AddComponent<SceneFadeManager>();
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
            CreateFadePanel();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void CreateFadePanel()
    {
        // Find or create canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("FadeCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create fade panel
        GameObject panelObj = new GameObject("FadePanel");
        panelObj.transform.SetParent(canvas.transform, false);
        fadePanel = panelObj;
        
        // Add Image component for black overlay
        fadeImage = panelObj.AddComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 1f); // Start fully black
        fadeImage.raycastTarget = false; // Don't block clicks
        
        // Cover entire screen EXCEPT top-left corner where button is
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // NO separate canvas - use main canvas so button stays on top
        // fadeCanvas removed completely
        
        Debug.Log("Fade panel created");
    }
    
    public void StartSceneFadeIn(float duration = 1f)
    {
        if (isFading) return;
        StartCoroutine(FadeIn(duration));
    }
    
    public void StartSceneFadeOut(float duration = 1f, System.Action onComplete = null)
    {
        if (isFading) return;
        StartCoroutine(FadeOut(duration, onComplete));
    }
    
    private IEnumerator FadeIn(float duration)
    {
        isFading = true;
        
        if (fadePanel != null)
        {
            fadePanel.SetActive(true);
        }
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            
            if (fadeImage != null)
            {
                fadeImage.color = new Color(0f, 0f, 0f, alpha);
            }
            
            yield return null;
        }
        
        // Ensure final state
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
        }
        
        if (fadePanel != null)
        {
            fadePanel.SetActive(false);
        }
        
        isFading = false;
        Debug.Log("Scene fade in complete");
    }
    
    private IEnumerator FadeOut(float duration, System.Action onComplete)
    {
        isFading = true;
        
        if (fadePanel != null)
        {
            fadePanel.SetActive(true);
        }
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            
            if (fadeImage != null)
            {
                fadeImage.color = new Color(0f, 0f, 0f, alpha);
            }
            
            yield return null;
        }
        
        // Ensure final state
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0f, 0f, 0f, 1f);
        }
        
        isFading = false;
        Debug.Log("Scene fade out complete");
        
        // Call completion callback
        onComplete?.Invoke();
    }
}