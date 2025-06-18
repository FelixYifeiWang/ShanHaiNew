using UnityEngine;

public class EntranceEffects : IBuildingEffects
{
    public void OnUpgrade(int newLevel)
    {
        // Entrance doesn't upgrade according to BuildingInfoUI.cs
        Debug.Log($"Entrance doesn't support upgrades");
    }
    
    public void OnStartWork(int buildingID)
    {
        Debug.Log("Starting adventure from entrance!");
        
        // CRITICAL: Save the game state before scene transition
        SaveSystem saveSystem = Object.FindObjectOfType<SaveSystem>();
        if (saveSystem != null)
        {
            saveSystem.TriggerSave("Before Adventure");
            Debug.Log("Game saved before adventure");
        }
        
        // Check if SceneFadeManager exists before using it
        SceneFadeManager fadeManager = Object.FindObjectOfType<SceneFadeManager>();
        if (fadeManager != null)
        {
            // Start fade out before transitioning
            fadeManager.StartSceneFadeOut(1f, () => {
                // This callback runs after fade completes
                Debug.Log("Transitioning to adventure scene...");
                GameSceneManager.Instance.LoadAdventureScene();
            });
        }
        else
        {
            // Fallback: direct transition without fade
            Debug.Log("No fade manager found, direct transition");
            GameSceneManager.Instance.LoadAdventureScene();
        }
    }
    
    public void OnCompleteWork()
    {
        // Adventure work completes immediately (duration 0)
        Debug.Log("Adventure completed from entrance");
    }
}

// Simple helper class for delayed scene transition (no longer needed with fade system)
public class AdventureTransitionHelper : MonoBehaviour
{
    // This class is kept for compatibility but no longer used
}