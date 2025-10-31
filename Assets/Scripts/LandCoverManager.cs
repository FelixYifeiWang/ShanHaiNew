using UnityEngine;
using UnityEngine.UI;

public class LandCoverManager : MonoBehaviour
{
    [SerializeField] private GameObject landcover1; // Land 2
    [SerializeField] private GameObject landcover2; // Land 3
    [SerializeField] private GameObject landcover3; // Land 4
    [SerializeField] private GameObject landcover4; // Land 5
    [SerializeField] private GameObject landcover5; // Land 6
    
    private LandManager landManager;
    
    void Start()
    {
        landManager = FindObjectOfType<LandManager>();
        
        // Subscribe to land unlock events
        LandEvents.OnLandUnlocked += OnLandUnlocked;
        
        // Delay initial setup to ensure LandManager has loaded
        StartCoroutine(DelayedInitialization());
    }
    
    private System.Collections.IEnumerator DelayedInitialization()
    {
        // Wait two frames to ensure LandManager has finished initialization
        yield return null;
        yield return null;
        
        // Initial setup
        UpdateLandCoverVisibility();
        SetupHoverHandlers();
    }

    // Add this to LandCoverManager.cs Start() method after SetupClickHandlers():
    // Add these methods to LandCoverManager.cs:
    private void SetupHoverHandlers()
    {
        SetupCoverHover(landcover1, 2);
        SetupCoverHover(landcover2, 3);
        SetupCoverHover(landcover3, 4);
        SetupCoverHover(landcover4, 5);
        SetupCoverHover(landcover5, 6);
    }

    private void SetupCoverHover(GameObject cover, int landID)
    {
        if (cover == null) return;
        
        // Add hover handler component
        LandCoverHoverHandler hoverHandler = cover.GetComponent<LandCoverHoverHandler>();
        if (hoverHandler == null)
        {
            hoverHandler = cover.AddComponent<LandCoverHoverHandler>();
        }
        hoverHandler.Initialize(landID);
    }
    
    void OnDestroy()
    {
        LandEvents.OnLandUnlocked -= OnLandUnlocked;
    }
    
    private void OnLandUnlocked(int landID)
    {
        UpdateLandCoverVisibility();
    }
    
    private void UpdateLandCoverVisibility()
    {
        if (landManager == null) return;
        
        // Landcover1 (Land 2): Show when land 1 unlocked but land 2 locked
        SetCoverActive(landcover1, landManager.IsLandUnlocked(1) && !landManager.IsLandUnlocked(2));
        
        // Landcover2 (Land 3): Show after land 2 unlocked, hide when land 3 unlocked
        SetCoverActive(landcover2, landManager.IsLandUnlocked(2) && !landManager.IsLandUnlocked(3));
        
        // Landcover3 (Land 4): Show after land 2 unlocked, hide when land 4 unlocked
        SetCoverActive(landcover3, landManager.IsLandUnlocked(2) && !landManager.IsLandUnlocked(4));
        
        // Landcover4 (Land 5): Show after land 3 OR 4 unlocked, hide when land 5 unlocked
        SetCoverActive(landcover4, (landManager.IsLandUnlocked(3) || landManager.IsLandUnlocked(4)) && !landManager.IsLandUnlocked(5));
        
        // Landcover5 (Land 6): Show after land 2 unlocked, hide when land 6 unlocked
        SetCoverActive(landcover5, landManager.IsLandUnlocked(2) && !landManager.IsLandUnlocked(6));
    }
    
    private void SetCoverActive(GameObject cover, bool active)
    {
        if (cover == null) return;
        
        if (active && !cover.activeInHierarchy)
        {
            cover.SetActive(true);
            StartCoroutine(FadeIn(cover));
        }
        else if (!active && cover.activeInHierarchy)
        {
            StartCoroutine(FadeOut(cover));
        }
    }
    
    private System.Collections.IEnumerator FadeIn(GameObject cover)
    {
        SpriteRenderer sr = cover.GetComponent<SpriteRenderer>();
        Image img = cover.GetComponent<Image>();
        
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = elapsed / duration;
            
            if (sr != null)
            {
                Color color = sr.color;
                color.a = alpha;
                sr.color = color;
            }
            else if (img != null)
            {
                Color color = img.color;
                color.a = alpha;
                img.color = color;
            }
            
            yield return null;
        }
        
        // Ensure full alpha
        if (sr != null)
        {
            Color color = sr.color;
            color.a = 1f;
            sr.color = color;
        }
        else if (img != null)
        {
            Color color = img.color;
            color.a = 1f;
            img.color = color;
        }
    }
    
    private System.Collections.IEnumerator FadeOut(GameObject cover)
    {
        SpriteRenderer sr = cover.GetComponent<SpriteRenderer>();
        Image img = cover.GetComponent<Image>();
        
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / duration);
            
            if (sr != null)
            {
                Color color = sr.color;
                color.a = alpha;
                sr.color = color;
            }
            else if (img != null)
            {
                Color color = img.color;
                color.a = alpha;
                img.color = color;
            }
            
            yield return null;
        }
        
        cover.SetActive(false);
        
        // Reset alpha for next time
        if (sr != null)
        {
            Color color = sr.color;
            color.a = 1f;
            sr.color = color;
        }
        else if (img != null)
        {
            Color color = img.color;
            color.a = 1f;
            img.color = color;
        }
    }
}

public class LandCoverHoverHandler : MonoBehaviour
{
    private int targetLandID;
    private LandCoverInfoUI infoUI;
    
    public void Initialize(int landID)
    {
        targetLandID = landID;
        infoUI = FindObjectOfType<LandCoverInfoUI>();
    }
    
    void OnMouseEnter()
    {
        // Check if mouse is over UI - if so, don't trigger hover
        if (UnityEngine.EventSystems.EventSystem.current != null && 
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        
        Debug.Log("Hover begins");
        if (infoUI != null)
        {
            infoUI.ShowLandInfo(targetLandID);
        }
    }
    
    void OnMouseExit()
    {
        // Check if mouse is over UI - if so, don't trigger exit
        if (UnityEngine.EventSystems.EventSystem.current != null && 
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        
        if (infoUI != null)
        {
            infoUI.HideLandInfo();
        }
    }
}