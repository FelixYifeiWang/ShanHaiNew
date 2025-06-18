using UnityEngine;
using UnityEngine.UI;
using System;

// Event system for next day notifications
public static class NextDayEvents
{
    public static event Action OnNextDay;
    
    public static void TriggerNextDay()
    {
        OnNextDay?.Invoke();
    }
}

public class NextDaySystem : MonoBehaviour
{
    [SerializeField] private Button nextDayButton;
    private ResourceManager resourceManager;
    
    void Start()
    {
        resourceManager = FindObjectOfType<ResourceManager>();
        
        if (nextDayButton == null)
        {
            CreateNextDayButton();
        }
        
        if (nextDayButton != null)
        {
            nextDayButton.onClick.AddListener(NextDay);
        }
    }
    
    private void CreateNextDayButton()
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
        
        // Create button GameObject
        GameObject buttonObj = new GameObject("NextDayButton");
        buttonObj.transform.SetParent(canvas.transform);
        
        // Add Image component for button background
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Dark gray
        
        // Add Button component
        nextDayButton = buttonObj.AddComponent<Button>();
        
        // Position button in top right
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1, 1);
        buttonRect.anchorMax = new Vector2(1, 1);
        buttonRect.pivot = new Vector2(1, 1);
        buttonRect.anchoredPosition = new Vector2(-10, -10);
        buttonRect.sizeDelta = new Vector2(120, 40);
        
        // Create button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = "Next Day";
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.fontSize = 16;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        // Position text to fill button
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    public void NextDay()
    {
        if (resourceManager != null)
        {
            // Increment day count
            Resource dayCountResource = resourceManager.GetResource("daycount");
            if (dayCountResource != null)
            {
                resourceManager.AddToResource("daycount", 1);
                Debug.Log($"Day incremented to: {dayCountResource.quantity}");
            }
            
            // Set actpoint = population
            Resource populationResource = resourceManager.GetResource("population");
            if (populationResource != null)
            {
                resourceManager.SetResourceQuantity("actpoint", populationResource.quantity);
            }
            
            // Consume crop1 based on population (population * 10)
            Resource populationResourceForConsumption = resourceManager.GetResource("population");
            Resource crop1Resource = resourceManager.GetResource("crop1");
            if (populationResourceForConsumption != null && crop1Resource != null && crop1Resource.isUnlocked)
            {
                int consumption = populationResourceForConsumption.quantity * 5;
                int currentCrop1 = crop1Resource.quantity;
                int newCrop1 = currentCrop1 - consumption;
                
                // Check for game over condition BEFORE setting the resource
                if (newCrop1 < 0)
                {
                    // Game over - not enough food
                    Debug.Log($"GAME OVER: Population needs {consumption} crop1, but only {currentCrop1} available!");
                    
                    // Trigger game over
                    GameOverSystem.Instance.TriggerGameOver($"Population starved! Needed {consumption} crop1, had {currentCrop1}");
                    return; // Don't continue with day progression
                }
                
                resourceManager.SetResourceQuantity("crop1", newCrop1);
                Debug.Log($"Population consumed {consumption} crop1. Had: {currentCrop1}, Now: {newCrop1}");
            }
                        
            // Trigger save after next day
            SaveSystem saveSystem = FindObjectOfType<SaveSystem>();
            if (saveSystem != null)
            {
                saveSystem.TriggerSave("Next Day");
            }
        }

        NextDayEvents.TriggerNextDay();

        Resource populationResourceUpdated = resourceManager.GetResource("population");
        if (populationResourceUpdated != null)
        {
            resourceManager.SetResourceQuantity("actpoint", populationResourceUpdated.quantity);
        }
    }
}