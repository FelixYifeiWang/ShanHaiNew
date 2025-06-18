using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpgradeRequirement
{
    public string resourceName;
    public int requiredQuantity;
    
    public UpgradeRequirement(string name, int quantity)
    {
        resourceName = name;
        requiredQuantity = quantity;
    }
}

[System.Serializable]
public class BuildingUpgradeCost
{
    public string buildingType;
    public int fromLevel;
    public List<UpgradeRequirement> requirements = new List<UpgradeRequirement>();
    
    public BuildingUpgradeCost(string type, int level)
    {
        buildingType = type;
        fromLevel = level;
    }
    
    public void AddRequirement(string resourceName, int quantity)
    {
        requirements.Add(new UpgradeRequirement(resourceName, quantity));
    }
}

public class BuildingUpgradeSystem : MonoBehaviour
{
    [SerializeField] private List<BuildingUpgradeCost> upgradeCosts = new List<BuildingUpgradeCost>();
    private Dictionary<string, BuildingUpgradeCost> costLookup = new Dictionary<string, BuildingUpgradeCost>();
    
    public const int MAX_BUILDING_LEVEL = 5;
    
    void Start()
    {
        InitializeUpgradeCosts();
        BuildCostLookup();
        
        // Debug: Print all available upgrade costs
        Debug.Log("=== Available Upgrade Costs ===");
        foreach (var cost in upgradeCosts)
        {
            Debug.Log($"Building: {cost.buildingType}, Level: {cost.fromLevel}");
        }
        Debug.Log("===============================");
    }
    
    private void InitializeUpgradeCosts()
    {
        // Define upgrade costs for each building type and level
        // Format: BuildingType from Level X to Level X+1
        
        // Siheyuan/Home upgrades (levels 1->2, 2->3, 3->4, 4->5)
        AddUpgradeCost("siheyuan", 1, "actpoint", 2);
        AddUpgradeCost("siheyuan", 2, "actpoint", 2);
        AddUpgradeCost("siheyuan", 3, "actpoint", 2);
        AddUpgradeCost("siheyuan", 4, "actpoint", 2);
        
        AddUpgradeCost("home", 1, "actpoint", 2);
        AddUpgradeCost("home", 2, "actpoint", 2);
        AddUpgradeCost("home", 3, "actpoint", 2);
        AddUpgradeCost("home", 4, "actpoint", 2);

        AddUpgradeCost("tian", 1, "actpoint", 2);
        AddUpgradeCost("tian", 2, "actpoint", 2);
        AddUpgradeCost("tian", 3, "actpoint", 2);
        AddUpgradeCost("tian", 4, "actpoint", 2);
        
        // Farmhouse upgrades
        AddUpgradeCost("farmhouse", 1, "actpoint", 2);
        AddUpgradeCost("farmhouse", 2, "actpoint", 99999);
        AddUpgradeCost("farmhouse", 3, "actpoint", 99999);
        AddUpgradeCost("farmhouse", 4, "actpoint", 99999);
        
        // Tower upgrades
        AddUpgradeCost("tower", 1, "actpoint", 2);
        AddUpgradeCost("tower", 2, "actpoint", 2);
        AddUpgradeCost("tower", 3, "actpoint", 2);
        AddUpgradeCost("tower", 4, "actpoint", 2);
        
        // Storage upgrades
        AddUpgradeCost("storage", 1, "actpoint", 2);
        AddUpgradeCost("storage", 2, "actpoint", 2);
        AddUpgradeCost("storage", 3, "actpoint", 99999);
        AddUpgradeCost("storage", 4, "actpoint", 99999);
        
        // Workshop upgrades (ADDED)
        AddUpgradeCost("workshop", 1, "actpoint", 2);
        AddUpgradeCost("workshop", 2, "actpoint", 2);
        AddUpgradeCost("workshop", 3, "actpoint", 2);
        AddUpgradeCost("workshop", 4, "actpoint", 2);
        
        // Note: Entrance doesn't upgrade according to BuildingInfoUI.cs configuration
    }
    
    private void AddUpgradeCost(string buildingType, int fromLevel, string resourceName, int quantity)
    {
        BuildingUpgradeCost cost = new BuildingUpgradeCost(buildingType, fromLevel);
        cost.AddRequirement(resourceName, quantity);
        upgradeCosts.Add(cost);
    }
    
    private void BuildCostLookup()
    {
        costLookup.Clear();
        foreach (BuildingUpgradeCost cost in upgradeCosts)
        {
            string key = GetLookupKey(cost.buildingType, cost.fromLevel);
            costLookup[key] = cost;
        }
        
        // Debug: Print all lookup keys
        Debug.Log("=== Lookup Keys ===");
        foreach (var kvp in costLookup)
        {
            Debug.Log($"Key: '{kvp.Key}' -> {kvp.Value.buildingType} level {kvp.Value.fromLevel}");
        }
        Debug.Log("==================");
    }
    
    private string GetLookupKey(string buildingType, int level)
    {
        return $"{buildingType.ToLower()}_{level}";
    }
    
    public BuildingUpgradeCost GetUpgradeCost(string buildingType, int currentLevel)
    {
        string key = GetLookupKey(buildingType, currentLevel);
        Debug.Log($"Looking up upgrade cost for: '{key}' (buildingType='{buildingType}', level={currentLevel})");
        
        if (costLookup.ContainsKey(key))
        {
            Debug.Log($"Found upgrade cost for '{key}'");
            return costLookup[key];
        }
        
        Debug.LogWarning($"No upgrade cost found for '{key}'");
        return null;
    }
    
    public bool CanUpgrade(string buildingType, int currentLevel)
    {
        Debug.Log($"Checking if can upgrade: {buildingType} at level {currentLevel}");
        
        // Check if building is at max level
        if (currentLevel >= MAX_BUILDING_LEVEL)
        {
            Debug.Log($"Cannot upgrade: at max level ({MAX_BUILDING_LEVEL})");
            return false;
        }
        
        BuildingUpgradeCost cost = GetUpgradeCost(buildingType, currentLevel);
        if (cost == null)
        {
            Debug.Log($"Cannot upgrade: no cost definition found");
            return false;
        }
        
        // Check if player has sufficient resources
        ResourceManager resourceManager = FindObjectOfType<ResourceManager>();
        if (resourceManager == null)
        {
            Debug.LogError("ResourceManager not found!");
            return false;
        }
        
        foreach (UpgradeRequirement requirement in cost.requirements)
        {
            Resource resource = resourceManager.GetResource(requirement.resourceName);
            Debug.Log($"Checking resource '{requirement.resourceName}': need {requirement.requiredQuantity}, have {(resource?.quantity ?? 0)}, unlocked: {(resource?.isUnlocked ?? false)}");
            
            if (resource == null || !resource.isUnlocked || resource.quantity < requirement.requiredQuantity)
            {
                Debug.Log($"Cannot upgrade: insufficient {requirement.resourceName}");
                return false;
            }
        }
        
        Debug.Log("Can upgrade: all requirements met");
        return true;
    }
    
    public List<UpgradeRequirement> GetMissingResources(string buildingType, int currentLevel)
    {
        List<UpgradeRequirement> missing = new List<UpgradeRequirement>();
        
        BuildingUpgradeCost cost = GetUpgradeCost(buildingType, currentLevel);
        if (cost == null)
        {
            return missing;
        }
        
        ResourceManager resourceManager = FindObjectOfType<ResourceManager>();
        if (resourceManager == null)
        {
            return missing;
        }
        
        foreach (UpgradeRequirement requirement in cost.requirements)
        {
            Resource resource = resourceManager.GetResource(requirement.resourceName);
            if (resource == null || !resource.isUnlocked)
            {
                missing.Add(new UpgradeRequirement(requirement.resourceName, requirement.requiredQuantity));
            }
            else if (resource.quantity < requirement.requiredQuantity)
            {
                int missingAmount = requirement.requiredQuantity - resource.quantity;
                missing.Add(new UpgradeRequirement(requirement.resourceName, missingAmount));
            }
        }
        
        return missing;
    }
    
    public void ConsumeUpgradeResources(string buildingType, int currentLevel)
    {
        BuildingUpgradeCost cost = GetUpgradeCost(buildingType, currentLevel);
        if (cost == null)
        {
            return;
        }
        
        ResourceManager resourceManager = FindObjectOfType<ResourceManager>();
        if (resourceManager == null)
        {
            return;
        }
        
        foreach (UpgradeRequirement requirement in cost.requirements)
        {
            resourceManager.AddToResource(requirement.resourceName, -requirement.requiredQuantity);
        }
    }
}