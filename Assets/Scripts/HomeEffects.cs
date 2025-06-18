using UnityEngine;

public class HomeEffects : IBuildingEffects
{
    public void OnUpgrade(int newLevel)
    {
        // Home upgrade increases population by 1
        ResourceManager resourceManager = Object.FindObjectOfType<ResourceManager>();
        if (resourceManager != null)
        {
            resourceManager.AddToResource("population", 1);
            Debug.Log($"Home upgraded to level {newLevel} - Population increased by 1! New population: {resourceManager.GetResource("population")?.quantity}");
        }
        else
        {
            Debug.LogWarning("ResourceManager not found - cannot increase population");
        }
    }
    
    public void OnStartWork(int buildingID)
    {
        // Homes don't have work assignments
        Debug.Log("Home buildings cannot be assigned work");
    }
    
    public void OnCompleteWork()
    {
        // Homes don't have work assignments
        Debug.Log("Home buildings don't perform work");
    }
}