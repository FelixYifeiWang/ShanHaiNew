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
        Debug.Log("Opening adventure selection UI!");
        
        // CRITICAL: Save the game state before showing selection UI
        SaveSystem saveSystem = Object.FindObjectOfType<SaveSystem>();
        if (saveSystem != null)
        {
            saveSystem.TriggerSave("Before Adventure Selection");
            Debug.Log("Game saved before adventure selection");
        }
        
        // Show the adventure selection UI instead of immediately transitioning
        // NOTE: ActPoint is NOT consumed here - only when "Start Adventure" is clicked
        AdventureSelectionUI.Instance.ShowSelectionUI();
        
        // IMPORTANT: Set building status back to idle immediately since we're not doing real work
        BuildingComponent building = Object.FindObjectOfType<BuildingComponent>();
        BuildingComponent[] allBuildings = Object.FindObjectsOfType<BuildingComponent>();
        foreach (BuildingComponent buildingComp in allBuildings)
        {
            if (buildingComp.GetBuildingID() == buildingID)
            {
                buildingComp.SetStatus(BuildingStatus.Idle);
                break;
            }
        }
    }
    
    public void OnCompleteWork()
    {
        // Adventure work completes immediately (duration 0)
        Debug.Log("Adventure selection completed from entrance");
    }
    
    // Static method to actually consume actpoint and start adventure
    // This is called from the "Start Adventure" button, not from building work assignment
    public static void ConsumeActPointAndStartAdventure()
    {
        ResourceManager resourceManager = Object.FindObjectOfType<ResourceManager>();
        if (resourceManager != null)
        {
            // Check if player has enough actpoint
            Resource actpointResource = resourceManager.GetResource("actpoint");
            if (actpointResource != null && actpointResource.quantity >= 3)
            {
                // Consume 3 actpoint
                resourceManager.AddToResource("actpoint", -3);
                Debug.Log("Consumed 3 actpoint for adventure");
                
                // Now start the actual adventure
                SceneFadeManager fadeManager = Object.FindObjectOfType<SceneFadeManager>();
                if (fadeManager != null)
                {
                    fadeManager.StartSceneFadeOut(1f, () => {
                        GameSceneManager.Instance.LoadAdventureScene();
                    });
                }
                else
                {
                    GameSceneManager.Instance.LoadAdventureScene();
                }
            }
            else
            {
                Debug.Log("Not enough actpoint for adventure!");
                // Could show error message here
            }
        }
    }
}