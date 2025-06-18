using UnityEngine;

public class TianEffects : IBuildingEffects
{
    public void OnUpgrade(int newLevel)
    {
        Debug.Log($"Tian upgraded to level {newLevel}");
        // Tian buildings could get efficiency bonuses at higher levels
        // For now, just log the upgrade
    }
    
    public void OnStartWork(int buildingID)
    {
        // Get the selected crop type from farmhouse (or default to crop1)
        string selectedCropType = GetSelectedCropType();
        //Debug.Log($"Tian {buildingID} starting {selectedCropType} cultivation");
    }
    
    public void OnCompleteWork()
    {
        // This is called when crop cultivation completes
        // The work assignment should tell us which crop was being grown
        // For now, we'll determine it based on the selected crop type
        
        Debug.Log("Tian work completed (crop reward handled by work assignment)");
    }
    
    private string GetSelectedCropType()
    {
        // Check if farmhouse exists and get selected crop type
        if (IsFarmhouseUnlocked())
        {
            return FarmhouseEffects.GetSelectedCropType();
        }
        else
        {
            // Default to crop1 if no farmhouse
            Debug.Log("No farmhouse found, defaulting to crop1");
            return "crop1";
        }
    }
    
    private bool IsFarmhouseUnlocked()
    {
        // Check if farmhouse building exists in the game
        BuildingComponent[] allBuildings = Object.FindObjectsOfType<BuildingComponent>();
        foreach (BuildingComponent building in allBuildings)
        {
            if (building.GetBuildingType().ToLower() == "farmhouse")
            {
                return true;
            }
        }
        return false;
    }
    
    private WorkRequirement GetCropWorkRequirement(string cropType)
    {
        // Create work requirements based on crop type
        WorkRequirement requirement;
        
        switch (cropType)
        {
            case "crop1":
                requirement = new WorkRequirement($"{cropType}_cultivation", 1); // 0 days
                requirement.AddResourceCost("actpoint", 1);
                break;
                
            case "crop2":
                requirement = new WorkRequirement($"{cropType}_cultivation", 2); // 1 day
                requirement.AddResourceCost("actpoint", 1);
                break;
                
            case "crop3":
                requirement = new WorkRequirement($"{cropType}_cultivation", 3); // 3 days
                requirement.AddResourceCost("actpoint", 1);
                break;
                
            default:
                // Fallback to crop1
                requirement = new WorkRequirement("crop1_cultivation", 0);
                requirement.AddResourceCost("actpoint", 1);
                Debug.LogWarning($"Unknown crop type {cropType}, defaulting to crop1");
                break;
        }
        
        return requirement;
    }
    
    // Static method to get crop cultivation duration (for UI display purposes)
    public static int GetCropDuration(string cropType)
    {
        switch (cropType)
        {
            case "crop1": return 1;
            case "crop2": return 2;
            case "crop3": return 3;
            default: return 0;
        }
    }
    
    // Static method to get crop cultivation description
    public static string GetCropDescription(string cropType)
    {
        int duration = GetCropDuration(cropType);
        if (duration == 0)
        {
            return $"Grow {cropType} (instant)";
        }
        else
        {
            return $"Grow {cropType} ({duration} day{(duration > 1 ? "s" : "")})";
        }
    }
}