using UnityEngine;
using System.Collections;

public class CoverFadeController : MonoBehaviour
{
    private bool hasBeenClicked = false;
    
    void Start()
    {
        Debug.Log($"CoverFadeController started on {gameObject.name}");
        
        // Add a collider if it doesn't exist
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            Debug.Log("Added BoxCollider2D to cover");
        }
    }
    
    void OnMouseDown()
    {
        Debug.Log($"Cover {gameObject.name} was clicked!");
        
        if (!hasBeenClicked)
        {
            hasBeenClicked = true;
            StartCoroutine(FadeOutAndDisable());
        }
    }
    
    private System.Collections.IEnumerator FadeOutAndDisable()
    {
        Debug.Log("Starting cover fade out...");
        
        // Try SpriteRenderer first
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            yield return StartCoroutine(FadeSpriteRenderer(spriteRenderer));
        }
        else
        {
            // Try UI Image
            UnityEngine.UI.Image image = GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                yield return StartCoroutine(FadeUIImage(image));
            }
            else
            {
                Debug.LogError($"Cover {gameObject.name} has no SpriteRenderer or Image component!");
                gameObject.SetActive(false);
                yield break;
            }
        }
        
        // Disable the GameObject
        gameObject.SetActive(false);
        Debug.Log($"Cover {gameObject.name} faded out and disabled");
        
        // Trigger camera intro animation after cover fades out
        CameraController cameraController = FindObjectOfType<CameraController>();
        if (cameraController != null)
        {
            cameraController.StartIntroAnimation();
            Debug.Log("Triggered camera intro animation");
        }
        else
        {
            Debug.LogWarning("CameraController not found for intro animation");
        }
    }
    
    private System.Collections.IEnumerator FadeSpriteRenderer(SpriteRenderer spriteRenderer)
    {
        Color originalColor = spriteRenderer.color;
        float duration = 1f;
        float elapsed = 0f;
        
        Debug.Log($"Fading SpriteRenderer. Original color: {originalColor}");
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        
        // Ensure final alpha is 0
        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        Debug.Log("SpriteRenderer fade complete");
    }
    
    private System.Collections.IEnumerator FadeUIImage(UnityEngine.UI.Image image)
    {
        Color originalColor = image.color;
        float duration = 1f;
        float elapsed = 0f;
        
        Debug.Log($"Fading UI Image. Original color: {originalColor}");
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            image.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        
        // Ensure final alpha is 0
        image.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        Debug.Log("UI Image fade complete");
    }
}