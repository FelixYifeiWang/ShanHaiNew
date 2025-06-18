using UnityEngine;
using System.Collections.Generic;

public class WorkshopEffects : IBuildingEffects
{
    private static List<CraftingRecipe> allRecipes = new List<CraftingRecipe>();
    private static bool recipesInitialized = false;
    
    // Dictionary to store active crafting assignments and their recipes
    private static Dictionary<int, CraftingRecipe> activeCraftingJobs = new Dictionary<int, CraftingRecipe>();
    
    public void OnUpgrade(int newLevel)
    {
        Debug.Log($"Workshop upgraded to level {newLevel} - new recipes may be available");
    }
    
    public void OnStartWork(int buildingID)
    {
        // This gets called when crafting starts - buildingID is passed for context
        // The actual crafting work is handled by BuildingEffectsSystem.StartTimedWork()
        // We just need to store which recipe is being crafted for this building
        
        // Find the active work assignment for this building to get the work type
        BuildingEffectsSystem effectsSystem = BuildingEffectsSystem.Instance;
        if (effectsSystem != null)
        {
            WorkAssignment currentWork = effectsSystem.GetWorkAssignment(buildingID);
            if (currentWork != null && currentWork.workType.StartsWith("craft_"))
            {
                // Extract recipe name from work type (e.g., "craft_Create White Gem" -> "Create White Gem")
                string recipeName = currentWork.workType.Substring(6); // Remove "craft_" prefix
                
                // Find the recipe and store it
                CraftingRecipe recipe = GetRecipeByName(recipeName);
                if (recipe != null)
                {
                    activeCraftingJobs[buildingID] = recipe;
                    Debug.Log($"Workshop {buildingID} started crafting: {recipe.recipeName}");
                }
            }
        }
    }
    
    public void OnCompleteWork()
    {
        // This is called when ANY workshop completes work, but we need to know which building
        // We'll handle the specific completion in CompleteWorkAssignment method
        Debug.Log("Workshop work completed - checking for crafting output");
    }
    
    // Called by BuildingEffectsSystem when a specific building completes work
    public static void CompleteCraftingWork(int buildingID)
    {
        if (activeCraftingJobs.ContainsKey(buildingID))
        {
            CraftingRecipe recipe = activeCraftingJobs[buildingID];
            
            // Produce the output resources
            ResourceManager resourceManager = Object.FindObjectOfType<ResourceManager>();
            if (resourceManager != null)
            {
                foreach (ResourceOutput output in recipe.outputs)
                {
                    resourceManager.AddToResource(output.resourceName, output.quantity);
                    
                    // Unlock the resource if it's not already unlocked (for special resources)
                    Resource resource = resourceManager.GetResource(output.resourceName);
                    if (resource != null && !resource.isUnlocked)
                    {
                        resourceManager.UnlockResource(output.resourceName);
                    }
                    
                    Debug.Log($"Crafting completed! Produced {output.quantity} {output.resourceName}");
                }
            }
            
            // Remove from active crafting jobs
            activeCraftingJobs.Remove(buildingID);
            Debug.Log($"Workshop {buildingID} completed crafting: {recipe.recipeName}");
        }
    }
    
    private static CraftingRecipe GetRecipeByName(string recipeName)
    {
        if (!recipesInitialized) InitializeRecipes();
        
        foreach (CraftingRecipe recipe in allRecipes)
        {
            if (recipe.recipeName == recipeName)
            {
                return recipe;
            }
        }
        
        Debug.LogWarning($"Recipe not found: {recipeName}");
        return null;
    }
    
    // NEW: Recipe management methods
    public static void InitializeRecipes()
    {
        if (recipesInitialized) return;
        
        allRecipes.Clear();
        
        // White Gem Creation (Level 1)
        CraftingRecipe createWhiteGem = new CraftingRecipe("Create White Gem", 2, 1);
        createWhiteGem.AddInput("crop2", 5);
        createWhiteGem.AddInput("actpoint", 2);
        createWhiteGem.AddOutput("whitegem", 1);
        allRecipes.Add(createWhiteGem);
        
        // Red Gem Creation (Level 2)
        CraftingRecipe createRedGem = new CraftingRecipe("Create Red Gem", 3, 2);
        createRedGem.AddInput("whitegem", 2);
        createRedGem.AddInput("actpoint", 2);
        createRedGem.AddOutput("redgem", 1);
        allRecipes.Add(createRedGem);
        
        // Blue Gem Creation (Level 3) - Example of higher level recipe
        CraftingRecipe createBlueGem = new CraftingRecipe("Create Blue Gem", 4, 3);
        createBlueGem.AddInput("redgem", 2);
        createBlueGem.AddInput("crop3", 10);
        createBlueGem.AddInput("actpoint", 3);
        createBlueGem.AddOutput("bluegem", 1);
        allRecipes.Add(createBlueGem);
        
        // Green Gem Creation (Level 4) - Another example
        CraftingRecipe createGreenGem = new CraftingRecipe("Create Green Gem", 5, 4);
        createGreenGem.AddInput("bluegem", 1);
        createGreenGem.AddInput("whitegem", 3);
        createGreenGem.AddInput("actpoint", 4);
        createGreenGem.AddOutput("greengem", 1);
        allRecipes.Add(createGreenGem);
        
        recipesInitialized = true;
        Debug.Log($"Initialized {allRecipes.Count} crafting recipes");
    }
    
    public static List<CraftingRecipe> GetAvailableRecipes(int workshopLevel)
    {
        if (!recipesInitialized) InitializeRecipes();
        
        List<CraftingRecipe> availableRecipes = new List<CraftingRecipe>();
        
        foreach (CraftingRecipe recipe in allRecipes)
        {
            if (recipe.requiredWorkshopLevel <= workshopLevel)
            {
                availableRecipes.Add(recipe);
            }
        }
        
        return availableRecipes;
    }
    
    public static List<CraftingRecipe> GetAllRecipes()
    {
        if (!recipesInitialized) InitializeRecipes();
        return allRecipes;
    }

    public static void RestoreActiveCraftingJob(int buildingID, string workType)
    {
        if (workType.StartsWith("craft_"))
        {
            string recipeName = workType.Substring(6); // Remove "craft_" prefix
            CraftingRecipe recipe = GetRecipeByName(recipeName);
            if (recipe != null)
            {
                activeCraftingJobs[buildingID] = recipe;
                Debug.Log($"Restored crafting job for building {buildingID}: {recipeName}");
            }
        }
    }
}