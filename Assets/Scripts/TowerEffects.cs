using UnityEngine;

public class TowerEffects : IBuildingEffects
{
    public void OnUpgrade(int newLevel)
    {
        Debug.Log($"Tower upgraded to level {newLevel} - future worship bonus could be applied here");
        
        // Example future effects:
        // - Level 2: -10% worship resource cost
        // - Level 3: -20% worship resource cost
        // - Level 4: -30% worship resource cost  
        // - Level 5: Unlock special worship benefits
    }
    
    public void OnStartWork(int buildingID)
    {
        Debug.Log("Tower work started - worship initiated");
        
        // Use the new resource-aware system
        bool success = BuildingEffectsSystem.Instance.StartTimedWork(buildingID, "tower", "worship", 0);
        
        if (!success)
        {
            Debug.Log("Could not start tower worship - insufficient resources");
        }
    }
    
    public void OnCompleteWork()
    {
        Debug.Log("Tower worship completed - divine blessings received");
        // Future: Generate worship rewards based on tower level
    }
}