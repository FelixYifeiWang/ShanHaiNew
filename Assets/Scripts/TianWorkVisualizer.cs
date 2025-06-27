using UnityEngine;

public class TianWorkVisualizer : MonoBehaviour
{
    private BuildingComponent buildingComponent;
    private GameObject currentActiveGroup;
    
    void Start()
    {
        buildingComponent = GetComponent<BuildingComponent>();
        
        // Hide all crop visual groups initially
        HideAllCropVisuals();
    }
    
    void Update()
    {
        if (buildingComponent == null) return;
        
        string buildingType = buildingComponent.GetBuildingType();
        
        // Only for Tian buildings
        if (buildingType.ToLower() != "tian") 
        {
            return;
        }
        
        // Show appropriate crop visual when working
        if (buildingComponent.GetStatus() == BuildingStatus.Working)
        {
            ShowCropVisualForLevel();
        }
        else
        {
            if (currentActiveGroup != null)
            {
                HideAllCropVisuals();
            }
        }
    }
    
    // Check if this Tian's work is ready to be collected
    public bool IsWorkReadyToCollect()
    {
        if (buildingComponent.GetBuildingType().ToLower() != "tian") return false;
        if (buildingComponent.GetStatus() != BuildingStatus.Working) return false;
        
        WorkAssignment workAssignment = BuildingEffectsSystem.Instance.GetWorkAssignment(buildingComponent.GetBuildingID());
        if (workAssignment == null) return false;
        
        // Get current day
        ResourceManager resourceManager = FindObjectOfType<ResourceManager>();
        if (resourceManager == null) return false;
        
        Resource dayCountResource = resourceManager.GetResource("daycount");
        if (dayCountResource == null) return false;
        
        int currentDay = dayCountResource.quantity;
        return currentDay >= workAssignment.completionDay;
    }
    
    // Manually complete the work and collect resources
    public void CollectTianWork()
    {
        if (!IsWorkReadyToCollect()) return;
        
        int buildingID = buildingComponent.GetBuildingID();
        WorkAssignment workAssignment = BuildingEffectsSystem.Instance.GetWorkAssignment(buildingID);
        
        if (workAssignment != null)
        {
            // Extract crop type and add resources
            string cropType = workAssignment.workType.Replace("_cultivation", "");
            int tianLevel = buildingComponent.GetLevel();
            int cropReward = 10 + 5 * (tianLevel - 1);
            
            ResourceManager resourceManager = FindObjectOfType<ResourceManager>();
            if (resourceManager != null)
            {
                resourceManager.AddToResource(cropType, cropReward);
                resourceManager.UnlockResource(cropType);
            }
            
            // Remove from active work assignments and set status to idle
            BuildingEffectsSystem.Instance.RemoveWorkAssignment(buildingID);
            buildingComponent.CompleteWork();
        }
    }
    
    private void ShowCropVisualForLevel()
    {
        int level = buildingComponent.GetLevel();
        int buildingID = buildingComponent.GetBuildingID();
        
        // Convert buildingID (3-8) to Tian number (1-6)
        int tianNumber = buildingID - 2;
        
        // Find the corresponding TianNEffects group
        string groupName = $"Tian{tianNumber}Effects";
        GameObject effectsGroup = GameObject.Find(groupName);
        
        // If not found, search through all GameObjects (including inactive ones)
        if (effectsGroup == null)
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == groupName && obj.scene.isLoaded)
                {
                    effectsGroup = obj;
                    break;
                }
            }
        }
        
        if (effectsGroup != null)
        {
            // Hide current active group if different
            if (currentActiveGroup != null && currentActiveGroup != effectsGroup)
            {
                HideAllCropVisuals();
            }
            
            // Show the group
            effectsGroup.SetActive(true);
            currentActiveGroup = effectsGroup;
            
            // Get the work assignment to determine days elapsed
            WorkAssignment workAssignment = BuildingEffectsSystem.Instance.GetWorkAssignment(buildingID);
            int daysElapsed = GetDaysElapsed(workAssignment);
            
            // Show grow objects based on level and update their sprites based on days elapsed
            for (int i = 1; i <= 4; i++)
            {
                Transform growObj = effectsGroup.transform.Find($"grow{i}");
                if (growObj != null)
                {
                    bool shouldShow = i <= level;
                    growObj.gameObject.SetActive(shouldShow);
                    
                    if (shouldShow)
                    {
                        UpdateGrowSprite(growObj.gameObject, daysElapsed);
                    }
                }
            }
        }
    }
    
    private int GetDaysElapsed(WorkAssignment workAssignment)
    {
        if (workAssignment == null) return 1;
        
        // Get current day
        ResourceManager resourceManager = FindObjectOfType<ResourceManager>();
        if (resourceManager == null) return 1;
        
        Resource dayCountResource = resourceManager.GetResource("daycount");
        if (dayCountResource == null) return 1;
        
        int currentDay = dayCountResource.quantity;
        int daysElapsed = currentDay - workAssignment.startDay + 1;
        
        // Clamp to valid range (1 to work duration)
        return Mathf.Clamp(daysElapsed, 1, workAssignment.duration);
    }
    
    private void UpdateGrowSprite(GameObject growObj, int daysElapsed)
    {
        SpriteRenderer spriteRenderer = growObj.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;
        
        // Check if work is completed and ready to collect
        bool isReadyToCollect = IsWorkReadyToCollect();
        
        if (isReadyToCollect)
        {
            // Show wheat animation when work is completed
            Animator animator = growObj.GetComponent<Animator>();
            if (animator == null)
            {
                animator = growObj.AddComponent<Animator>();
            }
            
            // Load the WheatAnimation controller from Resources
            RuntimeAnimatorController wheatController = Resources.Load<RuntimeAnimatorController>("WheatAnimation");
            if (wheatController != null)
            {
                animator.runtimeAnimatorController = wheatController;
            }
            else
            {
                // Fallback to static sprite
                Sprite wheatSprite = Resources.Load<Sprite>("wheat1");
                if (wheatSprite != null)
                {
                    spriteRenderer.sprite = wheatSprite;
                }
                Debug.LogWarning("WheatAnimation controller not found, using static wheat1 sprite");
            }
        }
        else
        {
            // Remove animator if it exists (for static sprites)
            Animator animator = growObj.GetComponent<Animator>();
            if (animator != null)
            {
                DestroyImmediate(animator);
            }
            
            // Show normal growth sprites
            string spriteName = daysElapsed == 1 ? "growday1" : "growday2";
            Sprite targetSprite = Resources.Load<Sprite>(spriteName);
            
            if (targetSprite != null)
            {
                spriteRenderer.sprite = targetSprite;
            }
            else
            {
                Debug.LogWarning($"Could not find sprite '{spriteName}' in Resources folder");
            }
        }
    }
    
    private void HideAllCropVisuals()
    {
        // Hide all TianNEffects groups (Tian1Effects to Tian6Effects)
        for (int i = 1; i <= 6; i++)
        {
            GameObject effectsGroup = GameObject.Find($"Tian{i}Effects");
            if (effectsGroup != null)
            {
                effectsGroup.SetActive(false);
            }
        }
        currentActiveGroup = null;
    }
}