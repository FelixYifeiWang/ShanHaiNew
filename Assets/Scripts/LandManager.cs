using System.Collections.Generic;
using UnityEngine;
using System;

public static class LandEvents
{
    public static event Action<int> OnLandUnlocked;
    
    public static void TriggerLandUnlocked(int landID)
    {
        OnLandUnlocked?.Invoke(landID);
    }
}

[System.Serializable]
public class Land
{
    public int landID;
    public bool isUnlocked;
    
    public Land(int id, bool unlocked)
    {
        landID = id;
        isUnlocked = unlocked;
    }
}

public class LandManager : MonoBehaviour
{
    [SerializeField] private List<Land> lands = new List<Land>();
    private bool isInitialized = false;
    
    void Awake()
    {
        // Try to load, but don't initialize if loading fails
        // We'll try again in Start() if needed
        if (!TryLoadLands())
        {
            Debug.Log("Failed to load lands in Awake, will try in Start");
        }
    }
    
    void Start()
    {
        // If we haven't initialized yet, try one more time then fall back to default
        if (!isInitialized)
        {
            if (!TryLoadLands())
            {
                Debug.Log("No save data found, initializing with defaults");
                InitializeLands();
            }
        }
    }
    
    private bool TryLoadLands()
    {
        SaveSystem saveSystem = FindObjectOfType<SaveSystem>();
        if (saveSystem == null)
        {
            Debug.LogWarning("SaveSystem not found");
            return false;
        }
        
        GameSaveData saveData = saveSystem.LoadGame();
        if (saveData == null || saveData.lands.Count == 0)
        {
            Debug.Log("No land save data available");
            return false;
        }
        
        // Clear existing lands
        lands.Clear();
        
        // Load from save data
        foreach (LandSaveData landData in saveData.lands)
        {
            lands.Add(new Land(landData.landID, landData.isUnlocked));
        }
        
        isInitialized = true;
        //Debug.Log($"Successfully loaded {lands.Count} lands from save data");
        
        // Verify we have all expected lands (1-6)
        for (int i = 1; i <= 6; i++)
        {
            if (GetLand(i) == null)
            {
                //Debug.LogWarning($"Land {i} missing from save data, adding with default state");
                AddLand(i, i == 1); // Land 1 defaults to unlocked
            }
        }
        
        return true;
    }
    
    private void InitializeLands()
    {
        lands.Clear();
        
        // Land 1: unlocked, siheyuan
        AddLand(1, true);
        
        // Land 2: locked, storage, tian1, tian2
        AddLand(2, false);
        
        // Land 3: locked, farmhouse, tian3, tian4, tian5, tian6
        AddLand(3, false);
        
        // Land 4: locked, zuofang
        AddLand(4, false);
        
        // Land 5: locked, brokenstone, tower
        AddLand(5, false);
        
        // Land 6: locked, entrance
        AddLand(6, false);
        
        isInitialized = true;
        //Debug.Log("Initialized lands with default values");
    }
    
    public void AddLand(int landID, bool isUnlocked)
    {
        // Check if land already exists
        Land existingLand = GetLand(landID);
        if (existingLand != null)
        {
            //Debug.LogWarning($"Land {landID} already exists, updating unlock status");
            existingLand.isUnlocked = isUnlocked;
            return;
        }
        
        Land newLand = new Land(landID, isUnlocked);
        lands.Add(newLand);
        //Debug.Log($"Added land {landID}, unlocked: {isUnlocked}");
    }
    
    public void UnlockLand(int landID)
    {
        Land land = GetLand(landID);
        if (land != null)
        {
            if (!land.isUnlocked)
            {
                land.isUnlocked = true;
                LandEvents.TriggerLandUnlocked(landID);
                //Debug.Log($"Land {landID} unlocked and event triggered!");
            }
            else
            {
                //Debug.Log($"Land {landID} is already unlocked");
            }
        }
        else
        {
           // Debug.LogError($"Cannot unlock land {landID} - land not found!");
        }
    }
    
    public Land GetLand(int landID)
    {
        foreach (Land land in lands)
        {
            if (land.landID == landID)
            {
                return land;
            }
        }
        return null;
    }
    
    public bool IsLandUnlocked(int landID)
    {
        Land land = GetLand(landID);
        return land != null && land.isUnlocked;
    }
    
    // Debug method to check current state
    public void DebugPrintLandStates()
    {
        //Debug.Log("=== Current Land States ===");
        foreach (Land land in lands)
        {
            //Debug.Log($"Land {land.landID}: {(land.isUnlocked ? "UNLOCKED" : "LOCKED")}");
        }
        //Debug.Log("=========================");
    }
}