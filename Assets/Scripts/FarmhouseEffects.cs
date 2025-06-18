using UnityEngine;

public class FarmhouseEffects : IBuildingEffects
{
    private static string selectedCropType = "crop1"; // Default crop type
    
    public void OnUpgrade(int newLevel)
    {
        ResourceManager resourceManager = Object.FindObjectOfType<ResourceManager>();
        if (resourceManager == null) return;
        
        Debug.Log($"Farmhouse upgraded to level {newLevel}");
        
        switch (newLevel)
        {
            case 1:
                // Unlock crop1 and crop2 at level 1
                Debug.Log("Unlocked crop1 and crop2");
                break;
                
            case 2:
                // Unlock crop3 at level 2
                Debug.Log("Unlocked crop3");
                break;
                
            case 3:
                // Additional population at level 3
                Debug.Log("Population increased by 1");
                break;
                
            case 4:
                // Additional population at level 4
                Debug.Log("Population increased by 1");
                break;
                
            case 5:
                // Additional population at level 5
                Debug.Log("Population increased by 1");
                break;
        }
    }
    
    public void OnStartWork(int buildingID)
    {
        // This is called when crop swap is confirmed
        Debug.Log($"Farmhouse crop type selection changed to {selectedCropType}");
        Debug.Log("This will affect future Tian cultivation only. Existing crops unchanged.");
    }
    
    public void OnCompleteWork()
    {
        // Farmhouse work completes immediately, nothing special needed
        Debug.Log("Farmhouse crop swap completed");
    }
    
    private void ShowCropSwapFeedback(int totalCrops)
    {
        // Create a simple feedback message (you could enhance this with proper UI)
        GameObject feedbackObj = new GameObject("CropSwapFeedback");
        
        // Position it in the center of the screen temporarily
        feedbackObj.transform.position = Vector3.zero;
        
        // Add a simple text component (this is basic - you might want to use Canvas)
        TextMesh textMesh = feedbackObj.AddComponent<TextMesh>();
        textMesh.text = $"Swapped {totalCrops} crops to {selectedCropType}!";
        textMesh.fontSize = 20;
        textMesh.color = Color.green;
        textMesh.anchor = TextAnchor.MiddleCenter;
        
        // Auto-destroy after 2 seconds
        Object.Destroy(feedbackObj, 2f);
        
        Debug.Log($"Crop swap feedback: {totalCrops} crops converted to {selectedCropType}");
    }
    
    // Static methods for external access
    public static void SetSelectedCropType(string cropType)
    {
        selectedCropType = cropType;
        Debug.Log($"Selected crop type set to: {cropType}");
    }
    
    public static string GetSelectedCropType()
    {
        return selectedCropType;
    }
    
    // Method for other systems to check farmhouse level requirements
    public static bool IsCropTypeAvailable(string cropType)
    {
        // int farmhouseLevel = GetFarmhouseLevel();
        
        // switch (cropType)
        // {
        //     case "crop1":
        //     case "crop2":
        //         return farmhouseLevel >= 1;
        //     case "crop3":
        //         return farmhouseLevel >= 2;
        //     default:
        //         return false;
        // }

        ResourceManager resourceManager = Object.FindObjectOfType<ResourceManager>();
        if (resourceManager == null) return false;
        
        Resource cropResource = resourceManager.GetResource(cropType);
        return cropResource != null && cropResource.isUnlocked;
    }
    
    private static int GetFarmhouseLevel()
    {
        BuildingComponent[] allBuildings = Object.FindObjectsOfType<BuildingComponent>();
        foreach (BuildingComponent building in allBuildings)
        {
            if (building.GetBuildingType().ToLower() == "farmhouse")
            {
                return building.GetLevel();
            }
        }
        return 0;
    }
}