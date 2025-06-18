using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class GameSaveData
{
    public List<ResourceSaveData> resources = new List<ResourceSaveData>();
    public List<LandSaveData> lands = new List<LandSaveData>();
    public List<BuildingSaveData> buildings = new List<BuildingSaveData>();
    public List<WorkAssignmentSaveData> workAssignments = new List<WorkAssignmentSaveData>();
    public List<WorkAssignmentSaveData> repairAssignments = new List<WorkAssignmentSaveData>(); // Add this
}

[System.Serializable]
public class ResourceSaveData
{
    public string resourceName;
    public int quantity;
    public bool isUnlocked;
    public bool isSpecialResource; // Add this
    
    public ResourceSaveData(string name, int qty, bool unlocked, bool isSpecial)
    {
        resourceName = name;
        quantity = qty;
        isUnlocked = unlocked;
        isSpecialResource = isSpecial; // Add this
    }
}

[System.Serializable]
public class LandSaveData
{
    public int landID;
    public bool isUnlocked;
    
    public LandSaveData(int id, bool unlocked)
    {
        landID = id;
        isUnlocked = unlocked;
    }
}

[System.Serializable]
public class BuildingSaveData
{
    public int buildingID;
    public string buildingType;
    public int level;
    public string status;
    public int landID;
    public bool needsRepair;     // Add this
    public bool isRepaired;
    
    public BuildingSaveData(int id, string type, int lvl, string stat, int land, bool repair, bool repaired)
    {
        buildingID = id;
        buildingType = type;
        level = lvl;
        status = stat;
        landID = land;
        needsRepair = repair;    // Add this
        isRepaired = repaired;
    }
}

[System.Serializable]
public class WorkAssignmentSaveData
{
    public int buildingID;
    public string buildingType;
    public string workType;
    public int startDay;
    public int duration;
    public int completionDay;
    public List<ResourceRequirement> resourceCosts;
    
    public WorkAssignmentSaveData(int id, string bType, string wType, int start, int dur, int completion, List<ResourceRequirement> costs)
    {
        buildingID = id;
        buildingType = bType;
        workType = wType;
        startDay = start;
        duration = dur;
        completionDay = completion;
        resourceCosts = costs ?? new List<ResourceRequirement>();
    }
}

public class SaveSystem : MonoBehaviour
{
    private string savePath;
    
    void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "gamesave.json");
    }
    
    void Start()
    {
        //InvokeRepeating("SaveGame", 10f, 10f);
        
        // Subscribe to events that should trigger saves
        LandEvents.OnLandUnlocked += OnLandUnlocked;
    }
    
    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        LandEvents.OnLandUnlocked -= OnLandUnlocked;
    }
    
    private void OnLandUnlocked(int landID)
    {
        // Save immediately when land is unlocked
        SaveGame();
    }
    
    public void TriggerSave(string reason = "Manual trigger")
    {
        SaveGame();
    }
    
    public void SaveGame()
    {
        GameSaveData saveData = new GameSaveData();
        
        // Save Resources
        SaveResources(saveData);
        
        // Save Lands
        SaveLands(saveData);
        
        // Save Buildings
        SaveBuildings(saveData);
        
        // Save Work Assignments
        SaveWorkAssignments(saveData);

        SaveRepairAssignments(saveData); 
        
        // Write to file
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);
    }
    
    public GameSaveData LoadGame()
    {
        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
                return saveData;
            }
            catch (System.Exception e)
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }
    
    private void SaveResources(GameSaveData saveData)
    {
        ResourceManager resourceManager = FindObjectOfType<ResourceManager>();
        if (resourceManager != null)
        {
            Dictionary<string, Resource> allResources = resourceManager.GetAllResources();
            
            foreach (var resourcePair in allResources)
            {
                string resourceName = resourcePair.Key;
                Resource resource = resourcePair.Value;
                saveData.resources.Add(new ResourceSaveData(resourceName, resource.quantity, resource.isUnlocked, resource.isSpecialResource));
            }
            
            //Debug.Log($"Saved {saveData.resources.Count} resources");
        }
        else
        {
            //Debug.LogWarning("ResourceManager not found!");
        }
    }
    
    private void SaveLands(GameSaveData saveData)
    {
        LandManager landManager = FindObjectOfType<LandManager>();
        if (landManager != null)
        {
            for (int i = 1; i <= 6; i++)
            {
                Land land = landManager.GetLand(i);
                if (land != null)
                {
                    saveData.lands.Add(new LandSaveData(land.landID, land.isUnlocked));
                }
            }
        }
    }
    
    private void SaveBuildings(GameSaveData saveData)
    {
        LandBuildingAssigner[] buildings = FindObjectsOfType<LandBuildingAssigner>();
        foreach (LandBuildingAssigner building in buildings)
        {
            BuildingComponent buildingComp = building.GetComponent<BuildingComponent>();
            if (buildingComp != null)
            {
                saveData.buildings.Add(new BuildingSaveData(
                    buildingComp.GetBuildingID(),
                    buildingComp.GetBuildingType(),
                    buildingComp.GetLevel(),
                    buildingComp.GetStatus().ToString(),
                    building.GetLandID(),
                    buildingComp.NeedsRepair(),    // Add this
                    buildingComp.IsRepaired()   
                ));
            }
        }
    }
    
    private void SaveWorkAssignments(GameSaveData saveData)
    {
        BuildingEffectsSystem effectsSystem = BuildingEffectsSystem.Instance;
        if (effectsSystem != null)
        {
            var activeWork = effectsSystem.GetActiveWorkAssignments();
            
            foreach (var work in activeWork)
            {
                saveData.workAssignments.Add(new WorkAssignmentSaveData(
                    work.buildingID,
                    work.buildingType,
                    work.workType,
                    work.startDay,
                    work.duration,
                    work.completionDay,
                    work.resourceCosts
                ));
            }
            
        }
    }

    private void SaveRepairAssignments(GameSaveData saveData)
    {
        BuildingEffectsSystem effectsSystem = BuildingEffectsSystem.Instance;
        if (effectsSystem != null)
        {
            var activeRepairs = effectsSystem.GetActiveRepairAssignments();
            
            foreach (var repair in activeRepairs)
            {
                saveData.repairAssignments.Add(new WorkAssignmentSaveData(
                    repair.buildingID,
                    repair.buildingType,
                    repair.workType,
                    repair.startDay,
                    repair.duration,
                    repair.completionDay,
                    repair.resourceCosts
                ));
            }
            
        }
    }
}