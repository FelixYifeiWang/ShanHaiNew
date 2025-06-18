using UnityEngine;
using System.Collections.Generic;

// Base interface for all building effects
public interface IBuildingEffects
{
    void OnUpgrade(int newLevel);
    void OnStartWork(int buildingID); // Now passes buildingID for context
    void OnCompleteWork(); // For future work completion
}

// Work assignment data
[System.Serializable]
public class WorkAssignment
{
    public int buildingID;
    public string buildingType;
    public string workType; // What specific work is being done
    public int startDay;
    public int duration;
    public int completionDay;
    public List<ResourceRequirement> resourceCosts; // NEW: Resource costs for this work
    
    public WorkAssignment(int id, string bType, string wType, int start, int dur, List<ResourceRequirement> costs = null)
    {
        buildingID = id;
        buildingType = bType;
        workType = wType;
        startDay = start;
        duration = dur;
        completionDay = start + duration;
        resourceCosts = costs ?? new List<ResourceRequirement>();
    }
    
    public bool IsComplete(int currentDay)
    {
        return currentDay >= startDay + duration;
    }
}

// Work requirement data
[System.Serializable]
public class WorkRequirement
{
    public string workType;
    public int duration;
    public List<ResourceRequirement> resourceCosts;
    
    public WorkRequirement(string type, int dur)
    {
        workType = type;
        duration = dur;
        resourceCosts = new List<ResourceRequirement>();
    }
    
    public void AddResourceCost(string resourceName, int quantity)
    {
        resourceCosts.Add(new ResourceRequirement(resourceName, quantity));
    }
}

// Repair requirement data
[System.Serializable]
public class RepairRequirement
{
    public string buildingType;
    public int duration;
    public List<ResourceRequirement> resourceCosts;
    
    public RepairRequirement(string type, int dur)
    {
        buildingType = type;
        duration = dur;
        resourceCosts = new List<ResourceRequirement>();
    }
    
    public void AddResourceCost(string resourceName, int quantity)
    {
        resourceCosts.Add(new ResourceRequirement(resourceName, quantity));
    }
}

// Building effects manager
public class BuildingEffectsSystem : MonoBehaviour
{
    private static BuildingEffectsSystem instance;
    private Dictionary<string, IBuildingEffects> buildingEffects = new Dictionary<string, IBuildingEffects>();
    private List<WorkAssignment> activeWorkAssignments = new List<WorkAssignment>();
    private Dictionary<string, List<WorkRequirement>> workRequirements = new Dictionary<string, List<WorkRequirement>>();
    private Dictionary<string, RepairRequirement> repairRequirements = new Dictionary<string, RepairRequirement>();
    private List<WorkAssignment> activeRepairAssignments = new List<WorkAssignment>();
    private bool hasLoadedFromSave = false; // Add this flag
    
    public static BuildingEffectsSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<BuildingEffectsSystem>();
                if (instance == null)
                {
                    GameObject go = new GameObject("BuildingEffectsSystem");
                    instance = go.AddComponent<BuildingEffectsSystem>();
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
            DontDestroyOnLoad(gameObject);
            InitializeBuildingEffects();
            InitializeWorkRequirements();
            InitializeRepairRequirements();
            
            // Subscribe to next day events to check work completion
            NextDayEvents.OnNextDay += OnNextDay;

            LoadWorkAssignmentsFromSave();
            hasLoadedFromSave = true;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Move save loading to Start() to ensure SaveSystem is ready
        if (!hasLoadedFromSave)
        {
            LoadWorkAssignmentsFromSave();
            hasLoadedFromSave = true;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        NextDayEvents.OnNextDay -= OnNextDay;
    }
    
    private void OnNextDay()
    {
        Debug.Log($"OnNextDay called - checking {activeWorkAssignments.Count} work assignments and {activeRepairAssignments.Count} repair assignments");
        CheckWorkCompletions();
    }
    
    private void CheckWorkCompletions()
    {
        ResourceManager resourceManager = FindObjectOfType<ResourceManager>();
        if (resourceManager == null) 
        {
            Debug.LogError("ResourceManager not found in CheckWorkCompletions!");
            return;
        }
        
        Resource dayCountResource = resourceManager.GetResource("daycount");
        if (dayCountResource == null) 
        {
            Debug.LogError("daycount resource not found in CheckWorkCompletions!");
            return;
        }
        
        int currentDay = dayCountResource.quantity;
        Debug.Log($"Current day: {currentDay}, checking work completions...");
        
        // Check all active work assignments for completion
        for (int i = activeWorkAssignments.Count - 1; i >= 0; i--)
        {
            WorkAssignment work = activeWorkAssignments[i];
            Debug.Log($"Checking work assignment: {work.workType} for building {work.buildingID}, completion day: {work.completionDay}");
            
            if (work.IsComplete(currentDay))
            {
                Debug.Log($"Work assignment {work.workType} for building {work.buildingID} is complete!");
                // Complete the work
                CompleteWorkAssignment(work);
                activeWorkAssignments.RemoveAt(i);
            }
        }

        for (int i = activeRepairAssignments.Count - 1; i >= 0; i--)
        {
            WorkAssignment repair = activeRepairAssignments[i];
            Debug.Log($"Checking repair assignment: {repair.workType} for building {repair.buildingID}, completion day: {repair.completionDay}");
            
            if (repair.IsComplete(currentDay))
            {
                Debug.Log($"Repair assignment {repair.workType} for building {repair.buildingID} is complete!");
                CompleteRepairAssignment(repair);
                activeRepairAssignments.RemoveAt(i);
            }
        }
        
        Debug.Log($"CheckWorkCompletions finished. Remaining work assignments: {activeWorkAssignments.Count}, repair assignments: {activeRepairAssignments.Count}");
    }
    
    private void CompleteWorkAssignment(WorkAssignment work)
    {
        Debug.Log($"Work '{work.workType}' completed for {work.buildingType} (ID: {work.buildingID}) after {work.duration} days");
        
        // Handle specific work completion rewards
        if (work.buildingType.ToLower() == "tian" && work.workType.Contains("_cultivation"))
        {
            // Extract crop type from work type (e.g., "crop1_cultivation" -> "crop1")
            string cropType = work.workType.Replace("_cultivation", "");
            HandleTianCropReward(cropType, work.buildingID);
        }
        else if (work.buildingType.ToLower() == "workshop" && work.workType.StartsWith("craft_"))
        {
            // Handle crafting completion - delegate to WorkshopEffects
            WorkshopEffects.CompleteCraftingWork(work.buildingID);
        }
        
        // Apply general completion effects
        if (buildingEffects.ContainsKey(work.buildingType.ToLower()))
        {
            buildingEffects[work.buildingType.ToLower()].OnCompleteWork();
        }
        
        // Set building status back to idle and trigger VFX
        BuildingComponent building = FindBuildingByID(work.buildingID);
        if (building != null)
        {
            building.CompleteWork(); // This will handle VFX and status change
        }
    }

    private void CompleteRepairAssignment(WorkAssignment repair)
    {
        Debug.Log($"Repair completed for {repair.buildingType} (ID: {repair.buildingID}) after {repair.duration} days");
        
        // Set building as repaired and complete the repair
        BuildingComponent building = FindBuildingByID(repair.buildingID);
        if (building != null)
        {
            building.CompleteRepair();
            
            // TODO: Change visual appearance after repair
            // This could include:
            // - Swapping sprite from broken stone to repaired tower
            // - Changing material/shader
            // - Adding particle effects
            // - Playing repair completion animation
            // Example placeholder:
            // ChangeRepairVisuals(building);
        }
    }

    // Add placeholder method for future visual changes
    // TODO: Implement visual changes after repair completion
    /*
    private void ChangeRepairVisuals(BuildingComponent building)
    {
        // Future implementation:
        // 1. Get the SpriteRenderer component
        // 2. Load new sprite for repaired state
        // 3. Swap the sprite
        // 4. Optionally add completion effects
        
        // Example:
        // SpriteRenderer sr = building.GetComponent<SpriteRenderer>();
        // if (sr != null)
        // {
        //     Sprite repairedSprite = Resources.Load<Sprite>("Sprites/TowerRepaired");
        //     sr.sprite = repairedSprite;
        // }
        
        Debug.Log($"Visual appearance changed for repaired {building.GetBuildingType()}");
    }
    */

    private void HandleTianCropReward(string cropType, int buildingID)
    {
        ResourceManager resourceManager = FindObjectOfType<ResourceManager>();
        if (resourceManager != null)
        {
            // Get the Tian building level to calculate reward
            BuildingComponent tianBuilding = FindBuildingByID(buildingID);
            int tianLevel = tianBuilding != null ? tianBuilding.GetLevel() : 1;
            
            // Calculate crop reward: 10 + 2 * (level - 1)
            int cropReward = 10 + 5 * (tianLevel - 1);
            
            // Add the calculated amount of the specific crop that was being grown
            resourceManager.AddToResource(cropType, cropReward);
            Debug.Log($"Level {tianLevel} Tian cultivation completed! Added {cropReward} {cropType}");
            
            // Unlock the crop resource if it's not already unlocked
            resourceManager.UnlockResource(cropType);
        }
    }
    
    private BuildingComponent FindBuildingByID(int buildingID)
    {
        BuildingComponent[] allBuildings = FindObjectsOfType<BuildingComponent>();
        foreach (BuildingComponent building in allBuildings)
        {
            if (building.GetBuildingID() == buildingID)
            {
                return building;
            }
        }
        return null;
    }
    
    private void InitializeBuildingEffects()
    {
        // Register all building effect handlers
        buildingEffects["home"] = new HomeEffects();
        buildingEffects["siheyuan"] = new HomeEffects(); // Same as home
        buildingEffects["storage"] = new StorageEffects();
        buildingEffects["farmhouse"] = new FarmhouseEffects();
        buildingEffects["tian"] = new TianEffects(); // Tian buildings for crop production
        buildingEffects["tower"] = new TowerEffects();
        buildingEffects["workshop"] = new WorkshopEffects();
        buildingEffects["entrance"] = new EntranceEffects();
    }
    
    private void InitializeWorkRequirements()
    {
        // Tower work requirements
        workRequirements["tower"] = new List<WorkRequirement>();
        WorkRequirement worship = new WorkRequirement("worship", 5);
        worship.AddResourceCost("actpoint", 3);
        worship.AddResourceCost("crop1", 0);
        workRequirements["tower"].Add(worship);

        workRequirements["tian"] = new List<WorkRequirement>();
        WorkRequirement tianWork = new WorkRequirement("cultivation", 1);
        tianWork.AddResourceCost("actpoint", 1);
        workRequirements["tian"].Add(tianWork);

        workRequirements["entrance"] = new List<WorkRequirement>();
        WorkRequirement adventureWork = new WorkRequirement("adventure", 0); // Instant completion
        adventureWork.AddResourceCost("actpoint", 3);
        workRequirements["entrance"].Add(adventureWork);
    }

    private void InitializeRepairRequirements()
    {
        // Tower repair requirements: 3 actpoint + 100 crop3 + 1 white gem, takes 3 days
        RepairRequirement towerRepair = new RepairRequirement("tower", 3);
        towerRepair.AddResourceCost("actpoint", 3);
        towerRepair.AddResourceCost("crop1", 2);
        towerRepair.AddResourceCost("whitegem", 1);
        repairRequirements["tower"] = towerRepair;
    }
    
    public RepairRequirement GetRepairRequirement(string buildingType)
    {
        string key = buildingType.ToLower();
        if (repairRequirements.ContainsKey(key))
        {
            return repairRequirements[key];
        }
        return null;
    }

    // Add this method to expose active work assignments for saving
    public List<WorkAssignment> GetActiveWorkAssignments()
    {
        return activeWorkAssignments;
    }
    
    // Add this method to load work assignments from save data
    public void LoadWorkAssignments(List<WorkAssignmentSaveData> savedWorkAssignments)
    {
        activeWorkAssignments.Clear();
        
        foreach (var savedWork in savedWorkAssignments)
        {
            WorkAssignment work = new WorkAssignment(
                savedWork.buildingID,
                savedWork.buildingType,
                savedWork.workType,
                savedWork.startDay,
                savedWork.duration,
                savedWork.resourceCosts
            );
            work.completionDay = savedWork.completionDay; // Restore completion day
            activeWorkAssignments.Add(work);

            if (work.buildingType.ToLower() == "workshop" && work.workType.StartsWith("craft_"))
            {
                WorkshopEffects.RestoreActiveCraftingJob(work.buildingID, work.workType);
            }
            
            Debug.Log($"Loaded work assignment: {savedWork.workType} for {savedWork.buildingType} (ID: {savedWork.buildingID}), completes day {savedWork.completionDay}");
        }
        
        Debug.Log($"Loaded {activeWorkAssignments.Count} work assignments");
    }

    public List<WorkAssignment> GetActiveRepairAssignments()
    {
        return activeRepairAssignments;
    }

    // Add this method to load repair assignments from save data
    public void LoadRepairAssignments(List<WorkAssignmentSaveData> savedRepairAssignments)
    {
        activeRepairAssignments.Clear();
        
        foreach (var savedRepair in savedRepairAssignments)
        {
            WorkAssignment repair = new WorkAssignment(
                savedRepair.buildingID,
                savedRepair.buildingType,
                savedRepair.workType,
                savedRepair.startDay,
                savedRepair.duration,
                savedRepair.resourceCosts
            );
            repair.completionDay = savedRepair.completionDay; // Restore completion day
            activeRepairAssignments.Add(repair);
            
            Debug.Log($"Loaded repair assignment: {savedRepair.workType} for {savedRepair.buildingType} (ID: {savedRepair.buildingID}), completes day {savedRepair.completionDay}");
        }
        
        Debug.Log($"Loaded {activeRepairAssignments.Count} repair assignments");
    }
    
    // Improve this method with better error handling and logging
    private void LoadWorkAssignmentsFromSave()
    {
        Debug.Log("LoadWorkAssignmentsFromSave called");
        
        SaveSystem saveSystem = FindObjectOfType<SaveSystem>();
        if (saveSystem == null)
        {
            Debug.LogWarning("SaveSystem not found - cannot load work assignments");
            return;
        }
        
        GameSaveData saveData = saveSystem.LoadGame();
        if (saveData == null)
        {
            Debug.Log("No save data found - starting with empty work assignments");
            return;
        }
        
        if (saveData.workAssignments != null && saveData.workAssignments.Count > 0)
        {
            Debug.Log($"Loading {saveData.workAssignments.Count} work assignments from save");
            LoadWorkAssignments(saveData.workAssignments);
        }
        else
        {
            Debug.Log("No work assignments in save data");
            Debug.Log($"{saveData.workAssignments != null}");
        }

        if (saveData.repairAssignments != null && saveData.repairAssignments.Count > 0)
        {
            Debug.Log($"Loading {saveData.repairAssignments.Count} repair assignments from save");
            LoadRepairAssignments(saveData.repairAssignments);
        }
        else
        {
            Debug.Log("No repair assignments in save data");
        }
        
        // NEW: Restore building states after loading work assignments
        RestoreBuildingStatesFromWorkAssignments();
        
        Debug.Log($"LoadWorkAssignmentsFromSave completed. Active work: {activeWorkAssignments.Count}, Active repairs: {activeRepairAssignments.Count}");
    }

    private void RestoreBuildingStatesFromWorkAssignments()
    {
        Debug.Log("Restoring building states from active work assignments...");
        
        // Restore working state for buildings with active work assignments
        foreach (WorkAssignment work in activeWorkAssignments)
        {
            BuildingComponent building = FindBuildingByID(work.buildingID);
            if (building != null)
            {
                building.SetStatus(BuildingStatus.Working);
                building.StartWork(); // This will create the working VFX
                Debug.Log($"Restored working state for building {work.buildingID} ({work.buildingType})");
            }
        }
        
        // Restore working state for buildings with active repair assignments
        foreach (WorkAssignment repair in activeRepairAssignments)
        {
            BuildingComponent building = FindBuildingByID(repair.buildingID);
            if (building != null)
            {
                building.SetStatus(BuildingStatus.Working);
                building.StartRepair(); // This will create the working VFX for repair
                Debug.Log($"Restored repair state for building {repair.buildingID} ({repair.buildingType})");
            }
        }
    }
    
    public void ApplyUpgradeEffect(string buildingType, int newLevel)
    {
        string key = buildingType.ToLower();
        if (buildingEffects.ContainsKey(key))
        {
            buildingEffects[key].OnUpgrade(newLevel);
        }
        else
        {
            Debug.Log($"No upgrade effects defined for building type: {buildingType}");
        }
    }
    
    public void ApplyStartWorkEffect(string buildingType, int buildingID)
    {
        string key = buildingType.ToLower();
        if (buildingEffects.ContainsKey(key))
        {
            buildingEffects[key].OnStartWork(buildingID);
        }
    }
    
    // NEW: Check if work can be started (has sufficient resources)
    public bool CanStartWork(string buildingType, string workType)
    {
        WorkRequirement requirement = GetWorkRequirement(buildingType, workType);
        if (requirement == null) return false;
        
        ResourceManager resourceManager = FindObjectOfType<ResourceManager>();
        if (resourceManager == null) return false;
        
        foreach (ResourceRequirement resourceReq in requirement.resourceCosts)
        {
            Resource resource = resourceManager.GetResource(resourceReq.resourceName);
            if (resource == null || !resource.isUnlocked || resource.quantity < resourceReq.requiredQuantity)
            {
                return false;
            }
        }
        
        return true;
    }
    
    // NEW: Get work requirement for a specific building and work type
    public WorkRequirement GetWorkRequirement(string buildingType, string workType)
    {
        string key = buildingType.ToLower();
        if (!workRequirements.ContainsKey(key)) return null;
        
        foreach (WorkRequirement req in workRequirements[key])
        {
            if (req.workType == workType)
            {
                return req;
            }
        }
        
        return null;
    }
    
    // NEW: Get the default work requirement for a building type
    public WorkRequirement GetDefaultWorkRequirement(string buildingType)
    {
        string key = buildingType.ToLower();
        if (!workRequirements.ContainsKey(key) || workRequirements[key].Count == 0) return null;
        
        return workRequirements[key][0]; // Return first/default work type
    }
    
    // Updated method with resource cost checking and consumption
    // Replace the StartTimedWork method in BuildingEffectsSystem.cs with this corrected version:

    public bool StartTimedWork(int buildingID, string buildingType, string workType, int duration, List<ResourceRequirement> resourceCosts = null)
    {
        Debug.Log($"StartTimedWork called for building {buildingID} ({buildingType}), work: {workType}");
        
        // If no costs provided, try to get from requirements
        if (resourceCosts == null)
        {
            WorkRequirement requirement = GetWorkRequirement(buildingType, workType);
            if (requirement != null)
            {
                resourceCosts = requirement.resourceCosts;
                duration = requirement.duration; // Use duration from requirement
            }
            else
            {
                resourceCosts = new List<ResourceRequirement>();
            }
        }
        
        // Check if we can afford the resource costs
        ResourceManager resourceManager = FindObjectOfType<ResourceManager>();
        if (resourceManager == null)
        {
            Debug.LogError("ResourceManager not found - cannot start timed work");
            return false;
        }
        
        foreach (ResourceRequirement resourceReq in resourceCosts)
        {
            Resource resource = resourceManager.GetResource(resourceReq.resourceName);
            if (resource == null || !resource.isUnlocked || resource.quantity < resourceReq.requiredQuantity)
            {
                Debug.Log($"Cannot start work - insufficient {resourceReq.resourceName} (need {resourceReq.requiredQuantity}, have {resource?.quantity ?? 0})");
                return false;
            }
        }
        
        // Consume the resources
        foreach (ResourceRequirement resourceReq in resourceCosts)
        {
            resourceManager.AddToResource(resourceReq.resourceName, -resourceReq.requiredQuantity);
        }
        
        // Get current day
        Resource dayCountResource = resourceManager.GetResource("daycount");
        if (dayCountResource == null)
        {
            Debug.LogError("daycount resource not found - cannot start timed work");
            return false;
        }
        
        int currentDay = dayCountResource.quantity;
        
        // IMPORTANT: Start the building work (for VFX) BEFORE creating work assignment
        BuildingComponent building = FindBuildingByID(buildingID);
        if (building != null)
        {
            building.StartWork(); // This creates the working VFX
        }
        
        // Create work assignment
        WorkAssignment newWork = new WorkAssignment(buildingID, buildingType, workType, currentDay, duration, resourceCosts);
        if (duration == 0)
        {
            Debug.Log($"Instant work '{workType}' for {buildingType} (ID: {buildingID}) - completing immediately");
            
            // For instant work, we need to be careful about execution order
            // 1. First apply start work effects (this might trigger scene changes)
            ApplyStartWorkEffect(buildingType, buildingID);
            
            // 2. Then apply completion effects
            ApplyCompleteWorkEffect(buildingType);
            
            // 3. Reset building status immediately (no VFX for instant work)
            if (building != null)
            {
                building.SetStatus(BuildingStatus.Idle);
            }
            
            // Don't add to active assignments since it's already complete
        }
        else
        {
            // Add to active work assignments for multi-day work
            activeWorkAssignments.Add(newWork);
            
            // Apply start work effects (for data/logic)
            ApplyStartWorkEffect(buildingType, buildingID);
            
            Debug.Log($"Started {duration}-day work '{workType}' for {buildingType} (ID: {buildingID}) on day {currentDay}, will complete on day {newWork.completionDay}");
        }
        
        //Debug.Log($"Started {duration}-day work '{workType}' for {buildingType} (ID: {buildingID}) on day {currentDay}, will complete on day {newWork.completionDay}");
        Debug.Log($"Total active work assignments: {activeWorkAssignments.Count}");
        return true;
    }

    public bool CanStartRepair(string buildingType)
    {
        RepairRequirement requirement = GetRepairRequirement(buildingType);
        if (requirement == null) return false;
        
        ResourceManager resourceManager = FindObjectOfType<ResourceManager>();
        if (resourceManager == null) return false;
        
        foreach (ResourceRequirement resourceReq in requirement.resourceCosts)
        {
            Resource resource = resourceManager.GetResource(resourceReq.resourceName);
            if (resource == null || !resource.isUnlocked || resource.quantity < resourceReq.requiredQuantity)
            {
                return false;
            }
        }
        
        return true;
    }

    public bool StartRepair(int buildingID, string buildingType)
    {
        RepairRequirement requirement = GetRepairRequirement(buildingType);
        if (requirement == null)
        {
            Debug.LogError($"No repair requirement found for {buildingType}");
            return false;
        }
        
        // Check if we can afford the repair costs
        ResourceManager resourceManager = FindObjectOfType<ResourceManager>();
        if (resourceManager == null)
        {
            Debug.LogError("ResourceManager not found - cannot start repair");
            return false;
        }
        
        foreach (ResourceRequirement resourceReq in requirement.resourceCosts)
        {
            Resource resource = resourceManager.GetResource(resourceReq.resourceName);
            if (resource == null || !resource.isUnlocked || resource.quantity < resourceReq.requiredQuantity)
            {
                Debug.Log($"Cannot start repair - insufficient {resourceReq.resourceName} (need {resourceReq.requiredQuantity}, have {resource?.quantity ?? 0})");
                return false;
            }
        }
        
        // Consume the resources
        foreach (ResourceRequirement resourceReq in requirement.resourceCosts)
        {
            resourceManager.AddToResource(resourceReq.resourceName, -resourceReq.requiredQuantity);
        }
        
        // Get current day
        Resource dayCountResource = resourceManager.GetResource("daycount");
        if (dayCountResource == null)
        {
            Debug.LogError("daycount resource not found - cannot start repair");
            return false;
        }
        
        int currentDay = dayCountResource.quantity;
        
        // Create repair assignment (reuse WorkAssignment but mark as repair)
        WorkAssignment repairWork = new WorkAssignment(buildingID, buildingType, "repair", currentDay, requirement.duration, requirement.resourceCosts);
        activeRepairAssignments.Add(repairWork);
        
        Debug.Log($"Started {requirement.duration}-day repair for {buildingType} (ID: {buildingID}) on day {currentDay}, will complete on day {repairWork.completionDay}");
        return true;
    }
    
    public void ApplyCompleteWorkEffect(string buildingType)
    {
        string key = buildingType.ToLower();
        if (buildingEffects.ContainsKey(key))
        {
            buildingEffects[key].OnCompleteWork();
        }
    }
    
    public bool IsBuildingWorking(int buildingID)
    {
        foreach (WorkAssignment work in activeWorkAssignments)
        {
            if (work.buildingID == buildingID)
            {
                return true;
            }
        }
        return false;
    }
    
    public WorkAssignment GetWorkAssignment(int buildingID)
    {
        foreach (WorkAssignment work in activeWorkAssignments)
        {
            if (work.buildingID == buildingID)
            {
                return work;
            }
        }
        return null;
    }
}

[System.Serializable]
public class CraftingRecipe
{
    public string recipeName;
    public List<ResourceRequirement> inputResources;
    public int workDuration; // in days
    public List<ResourceOutput> outputs;
    public int requiredWorkshopLevel;
    
    public CraftingRecipe(string name, int duration, int minLevel = 1)
    {
        recipeName = name;
        workDuration = duration;
        requiredWorkshopLevel = minLevel;
        inputResources = new List<ResourceRequirement>();
        outputs = new List<ResourceOutput>();
    }
    
    public void AddInput(string resourceName, int quantity)
    {
        inputResources.Add(new ResourceRequirement(resourceName, quantity));
    }
    
    public void AddOutput(string resourceName, int quantity)
    {
        outputs.Add(new ResourceOutput(resourceName, quantity));
    }
}

[System.Serializable]
public class ResourceOutput
{
    public string resourceName;
    public int quantity;
    
    public ResourceOutput(string name, int qty)
    {
        resourceName = name;
        quantity = qty;
    }
}