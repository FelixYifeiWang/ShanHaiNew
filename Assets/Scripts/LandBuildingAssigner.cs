using UnityEngine;
using System.Collections;

public class LandBuildingAssigner : MonoBehaviour
{
    [SerializeField] private int landID;
    private LandManager landManager;
    private SpriteRenderer spriteRenderer;
    private bool isAnimating = false;
    
    // void Start()
    // {
    //     landManager = FindObjectOfType<LandManager>();
    //     spriteRenderer = GetComponent<SpriteRenderer>();
    //     //Debug.Log($"[{gameObject.name}] LandManager found: {landManager != null}");
        
    //     // Subscribe to land unlock events
    //     LandEvents.OnLandUnlocked += OnLandUnlocked;
        
    //     UpdateVisibility();
    // }
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;  // Hide visual

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false; // Disable interaction
    }


    IEnumerator Start()
    {
        landManager = FindObjectOfType<LandManager>();
        LandEvents.OnLandUnlocked += OnLandUnlocked;

        yield return new WaitForEndOfFrame(); // Wait for LandManager to load state

        UpdateVisibility(); // Will show if needed
    }


    
    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        LandEvents.OnLandUnlocked -= OnLandUnlocked;
    }
    
    private void OnLandUnlocked(int unlockedLandID)
    {
        if (unlockedLandID == landID)
        {
            Debug.Log($"[{gameObject.name}] Received unlock event for land {landID}");

            if (spriteRenderer != null)
                spriteRenderer.enabled = true;

            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
                col.enabled = true;

            // No need to load save data — this is a new building instance

            StartCoroutine(FadeInEffect());
        }
    }

    private IEnumerator FadeInEffect()
    {
        if (spriteRenderer == null || isAnimating) yield break;
        
        isAnimating = true;
        
        // Start with transparent
        Color originalColor = spriteRenderer.color;
        Color transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        spriteRenderer.color = transparentColor;
        
        // Fade in over 1 second
        float duration = 1f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, originalColor.a, elapsed / duration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        
        // Ensure final color is set
        spriteRenderer.color = originalColor;
        isAnimating = false;
        
        //Debug.Log($"[{gameObject.name}] Fade-in complete!");
    }
    
    void Update()
    {
        // Remove the constant Update() check - now event-driven
    }
    
    private void UpdateVisibility()
    {
        bool shouldBeVisible = landManager != null && landManager.IsLandUnlocked(landID);

        if (shouldBeVisible)
        {
            if (spriteRenderer != null){
                spriteRenderer.enabled = true;
                StartCoroutine(FadeInEffect());
            }

            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
                col.enabled = true;
        }
        else
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = false;

            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;
        }
    }

    
    public void SetLandID(int id)
    {
        landID = id;
        //Debug.Log($"[{gameObject.name}] Land ID set to: {id}");
        UpdateVisibility();
    }
    
    public int GetLandID()
    {
        return landID;
    }
}