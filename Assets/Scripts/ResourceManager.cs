using System.Collections.Generic;
using UnityEngine;
using System.Collections;

[System.Serializable]
public class Resource
{
    public int quantity;
    public bool isUnlocked;
    public bool isSpecialResource; // Add this new parameter
    
    public Resource(int initialQuantity, bool initialUnlocked, bool isSpecial = false)
    {
        quantity = initialQuantity;
        isUnlocked = initialUnlocked;
        isSpecialResource = isSpecial; // Add this
    }
}

public class ResourceManager : MonoBehaviour
{
    [SerializeField] private Dictionary<string, Resource> resources = new Dictionary<string, Resource>();
    private bool isInitialized = false;
    
    void Awake()
    {
        // Make ResourceManager persist between scenes
        if (FindObjectsOfType<ResourceManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
        
        // Try to load in Awake, but don't initialize if loading fails
        if (!TryLoadResources())
        {
            Debug.Log("Failed to load resources in Awake, will try in Start");
        }
    }
    
    void Start()
    {
        // If we haven't initialized yet, try one more time then fall back to default
        if (!isInitialized)
        {
            if (!TryLoadResources())
            {
                Debug.Log("No resource save data found, initializing with defaults");
                InitializeResources();
            }
        }
        //SpecialResourceInventory.Instance.gameObject.SetActive(true);
        StartCoroutine(InitializeSpecialResourceInventory());
    }
    
    private System.Collections.IEnumerator InitializeSpecialResourceInventory()
    {
        // Wait a frame to ensure canvas is ready
        yield return null;
        
        // Access the instance to trigger creation
        var inventory = SpecialResourceInventory.Instance;
        Debug.Log("Special Resource Inventory initialized from ResourceManager");
    }

    private bool TryLoadResources()
    {
        SaveSystem saveSystem = FindObjectOfType<SaveSystem>();
        if (saveSystem == null)
        {
            Debug.LogWarning("SaveSystem not found");
            return false;
        }
        
        GameSaveData saveData = saveSystem.LoadGame();
        if (saveData == null || saveData.resources.Count == 0)
        {
            Debug.Log("No resource save data available");
            return false;
        }
        
        // Clear existing resources
        resources.Clear();
        
        // Load from save data
        foreach (ResourceSaveData resourceData in saveData.resources)
        {
            // FIX: Use the 3-parameter constructor that includes isSpecialResource
            resources[resourceData.resourceName] = new Resource(resourceData.quantity, resourceData.isUnlocked, resourceData.isSpecialResource);
        }
        
        isInitialized = true;
        Debug.Log($"Successfully loaded {resources.Count} resources from save data");
        
        // Verify we have all expected resources
        EnsureRequiredResources();
        
        return true;
    }
    
    private void EnsureRequiredResources()
    {
        // Make sure all required resources exist, add defaults if missing
        if (!resources.ContainsKey("daycount"))
            resources["daycount"] = new Resource(1, true, false);
        if (!resources.ContainsKey("population"))
            resources["population"] = new Resource(2, true, false);
        if (!resources.ContainsKey("actpoint"))
            resources["actpoint"] = new Resource(2, true, false);
        if (!resources.ContainsKey("crop1"))
            resources["crop1"] = new Resource(0, false, false);
        if (!resources.ContainsKey("crop2"))
            resources["crop2"] = new Resource(0, false, false);
        if (!resources.ContainsKey("crop3"))
            resources["crop3"] = new Resource(0, false, false);
        if (!resources.ContainsKey("money"))
            resources["money"] = new Resource(0, false, false);
        
        if (!resources.ContainsKey("whitegem"))
            resources["whitegem"] = new Resource(0, true, true);
        if (!resources.ContainsKey("redgem"))
            resources["redgem"] = new Resource(0, true, true);
        if (!resources.ContainsKey("bluegem"))  // Add blue gem
            resources["bluegem"] = new Resource(0, true, true);
        if (!resources.ContainsKey("greengem"))  // Add green gem
            resources["greengem"] = new Resource(0, true, true);
        if (!resources.ContainsKey("gold"))  // Add green gem
            resources["gold"] = new Resource(0, true, true);
        if (!resources.ContainsKey("divineseal"))  // Add green gem
            resources["divineseal"] = new Resource(0, true, true);
    }
    
    private void InitializeResources()
    {
        resources.Clear();
        resources["daycount"] = new Resource(1, true, false);
        resources["population"] = new Resource(2, true, false);
        resources["actpoint"] = new Resource(2, true, false);
        resources["crop1"] = new Resource(200, false, false);
        resources["crop2"] = new Resource(0, false, false);
        resources["crop3"] = new Resource(0, false, false);
        resources["money"] = new Resource(0, false, false);
        
        resources["whitegem"] = new Resource(0, true, true);
        resources["redgem"] = new Resource(0, true, true); 
        resources["bluegem"] = new Resource(0, true, true);
        resources["greengem"] = new Resource(0, true, true);
        resources["gold"] = new Resource(0, true, true);
        resources["divineseal"] = new Resource(0, true, true);
        
        isInitialized = true;
        Debug.Log("Initialized resources with default values");
    }
    
    public Resource GetResource(string resourceName)
    {
        if (resources.ContainsKey(resourceName))
        {
            return resources[resourceName];
        }
        return null;
    }

    public Dictionary<string, Resource> GetRegularResources()
    {
        Dictionary<string, Resource> regularResources = new Dictionary<string, Resource>();
        
        foreach (var resourcePair in resources)
        {
            if (!resourcePair.Value.isSpecialResource)
            {
                regularResources[resourcePair.Key] = resourcePair.Value;
            }
        }
        
        return regularResources;
    }

    // Add this new method to get only special resources
    public Dictionary<string, Resource> GetSpecialResources()
    {
        Dictionary<string, Resource> specialResources = new Dictionary<string, Resource>();
        
        foreach (var resourcePair in resources)
        {
            if (resourcePair.Value.isSpecialResource)
            {
                specialResources[resourcePair.Key] = resourcePair.Value;
            }
        }
        
        return specialResources;
    }
    
    public void SetResourceQuantity(string resourceName, int quantity)
    {
        if (resources.ContainsKey(resourceName))
        {
            resources[resourceName].quantity = quantity;
        }
    }
    
    public void AddToResource(string resourceName, int amount)
    {
        if (resources.ContainsKey(resourceName))
        {
            resources[resourceName].quantity += amount;
        }
    }
    
    public void UnlockResource(string resourceName)
    {
        if (resources.ContainsKey(resourceName))
        {
            resources[resourceName].isUnlocked = true;
        }
    }
    
    public void AddNewResource(string resourceName, int initialQuantity, bool initialUnlocked, bool isSpecial = false)
    {
        if (!resources.ContainsKey(resourceName))
        {
            resources[resourceName] = new Resource(initialQuantity, initialUnlocked, isSpecial);
        }
    }

    // Add method specifically for adding special resources
    public void AddSpecialResource(string resourceName, int initialQuantity = 0)
    {
        if (!resources.ContainsKey(resourceName))
        {
            resources[resourceName] = new Resource(initialQuantity, false, true); // Special resources start locked
            Debug.Log($"Added special resource: {resourceName}");
        }
    }
    
    public Dictionary<string, Resource> GetAllResources()
    {
        return resources;
    }
}