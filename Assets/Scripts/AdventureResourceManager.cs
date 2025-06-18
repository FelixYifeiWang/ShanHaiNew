using System.Collections.Generic;
using UnityEngine;

public class AdventureResourceManager : MonoBehaviour
{
    [SerializeField] private Dictionary<string, Resource> adventureResources = new Dictionary<string, Resource>();
    
    // Singleton pattern
    private static AdventureResourceManager instance;
    public static AdventureResourceManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AdventureResourceManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AdventureResourceManager");
                    instance = go.AddComponent<AdventureResourceManager>();
                }
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        InitializeAdventureResources();
    }
    
    private void InitializeAdventureResources()
    {
        // Initialize adventure-only special resources (start with 0)
        adventureResources["whitegem"] = new Resource(0, true, true);
        adventureResources["redgem"] = new Resource(0, true, true);
        adventureResources["bluegem"] = new Resource(0, true, true);
        adventureResources["greengem"] = new Resource(0, true, true);
        adventureResources["gold"] = new Resource(0, true, true);
        
        Debug.Log("Adventure resources initialized - all gems and gold start at 0");
    }
    
    public Resource GetResource(string resourceName)
    {
        if (adventureResources.ContainsKey(resourceName))
        {
            return adventureResources[resourceName];
        }
        return null;
    }
    
    public Dictionary<string, Resource> GetSpecialResources()
    {
        Dictionary<string, Resource> specialResources = new Dictionary<string, Resource>();
        
        foreach (var resourcePair in adventureResources)
        {
            if (resourcePair.Value.isSpecialResource)
            {
                specialResources[resourcePair.Key] = resourcePair.Value;
            }
        }
        
        return specialResources;
    }
    
    public void AddToResource(string resourceName, int amount)
    {
        if (adventureResources.ContainsKey(resourceName))
        {
            adventureResources[resourceName].quantity += amount;
            Debug.Log($"Adventure: Added {amount} {resourceName}, new total: {adventureResources[resourceName].quantity}");
        }
    }
    
    public void SetResourceQuantity(string resourceName, int quantity)
    {
        if (adventureResources.ContainsKey(resourceName))
        {
            adventureResources[resourceName].quantity = quantity;
        }
    }
    
    public Dictionary<string, Resource> GetAllAdventureResources()
    {
        return adventureResources;
    }
    
    // Method to add a found resource (for treasure rewards)
    public void FindTreasure(string gemType, int amount = 1)
    {
        AddToResource(gemType, amount);
        Debug.Log($"Found treasure: {amount} {gemType}!");
    }
}