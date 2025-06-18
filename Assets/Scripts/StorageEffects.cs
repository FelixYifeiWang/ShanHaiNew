using UnityEngine;

public class StorageEffects : IBuildingEffects
{
    public void OnUpgrade(int newLevel)
    {
        // Storage upgrades unlock new crops
        ResourceManager resourceManager = Object.FindObjectOfType<ResourceManager>();
        if (resourceManager != null)
        {
            switch (newLevel)
            {
                case 2:
                    resourceManager.UnlockResource("crop2");
                    Debug.Log($"Storage upgraded to level 2 - crop2 unlocked!");
                    break;
                    
                case 3:
                    resourceManager.UnlockResource("crop3");
                    Debug.Log($"Storage upgraded to level 3 - crop3 unlocked!");
                    break;
                    
                case 4:
                    Debug.Log($"Storage upgraded to level 4 - future storage capacity bonus could be applied here");
                    break;
                    
                case 5:
                    Debug.Log($"Storage upgraded to level 5 - maximum storage efficiency reached!");
                    break;
                    
                default:
                    Debug.Log($"Storage upgraded to level {newLevel}");
                    break;
            }
        }
        else
        {
            Debug.LogWarning("ResourceManager not found - cannot unlock crops");
        }
    }
    
    public void OnStartWork(int buildingID)
    {
        // Storage buildings don't have work assignments currently
        Debug.Log("Storage buildings cannot be assigned work currently");
    }
    
    public void OnCompleteWork()
    {
        // Storage buildings don't have work assignments currently
        Debug.Log("Storage buildings don't perform work currently");
    }
}