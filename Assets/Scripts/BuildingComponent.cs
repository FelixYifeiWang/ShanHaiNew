using UnityEngine;

public class BuildingComponent : MonoBehaviour
{
    [SerializeField] private int buildingID;
    [SerializeField] private string buildingType;
    [SerializeField] private int level = 1;
    [SerializeField] private BuildingStatus status = BuildingStatus.Idle;
    [SerializeField] private bool needsRepair = false;
    [SerializeField] private bool isRepaired = false;
    
    // VFX components for upgrade and working effects
    private GameObject upgradeVFX;
    private GameObject workingVFX;
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (buildingID == 0)
        {
            Debug.LogError($"Building {buildingType} on {gameObject.name} has invalid ID ({buildingID})! Please assign a unique ID in the inspector.");
            buildingID = GenerateUniqueID();
        }

        if (buildingType.ToLower() == "tower" && !LoadBuildingData())
        {
            needsRepair = true;
            isRepaired = false;
            Debug.Log($"Tower {buildingID} set to need repair");
        }
        
        if (!LoadBuildingData())
        {
            Debug.Log($"Building {buildingType} initialized with ID: {buildingID}");
            // Use default values if no save data
        }
        
        Debug.Log($"Building ID: {buildingID}");
        RegisterWithManager();
        
        // Subscribe to next day events to complete upgrades
        NextDayEvents.OnNextDay += OnNextDay;
        
        // Initialize VFX based on current status
        if (status == BuildingStatus.Upgrading)
        {
            CreateUpgradeVFX();
        }
        else if (status == BuildingStatus.Working)
        {
            CreateWorkingVFX();
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        NextDayEvents.OnNextDay -= OnNextDay;
    }
    
    private void OnNextDay()
    {
        if (status == BuildingStatus.Upgrading)
        {
            CompleteUpgrade();
        }
    }
    
    public void StartUpgrade()
    {
        if (needsRepair && !isRepaired)
        {
            Debug.Log($"Building {buildingType} needs repair before it can be upgraded!");
            return;
        }

        if (status != BuildingStatus.Upgrading)
        {
            status = BuildingStatus.Upgrading;
            CreateUpgradeVFX();
            Debug.Log($"Building {buildingType} started upgrading");
        }
    }
    
    private void CompleteUpgrade()
    {
        level++;
        status = BuildingStatus.Idle;
        DestroyUpgradeVFX();
        
        // Apply upgrade effects using the modular system
        BuildingEffectsSystem.Instance.ApplyUpgradeEffect(buildingType, level);
        
        // Show level up VFX
        StartCoroutine(ShowLevelUpVFX());
        
        Debug.Log($"Building {buildingType} upgrade completed! New level: {level}");
        
        // Trigger save after upgrade completion
        SaveSystem saveSystem = FindObjectOfType<SaveSystem>();
        if (saveSystem != null)
        {
            saveSystem.TriggerSave($"Building {buildingType} upgrade completed");
        }
    }
    
    // Future methods for work assignments
    public void StartWork()
    {
        if (needsRepair && !isRepaired)
        {
            Debug.Log($"Building {buildingType} needs repair before it can work!");
            return;
        }

        if (status == BuildingStatus.Idle)
        {
            status = BuildingStatus.Working;
            CreateWorkingVFX();
            Debug.Log($"Building {buildingType} started working with VFX");
        }
        else if (status == BuildingStatus.Working)
        {
            // Already working, just ensure VFX is visible
            if (workingVFX == null)
            {
                CreateWorkingVFX();
                Debug.Log($"Building {buildingType} was already working, added missing VFX");
            }
        }
    }
    
    public void CompleteWork()
    {
        if (status == BuildingStatus.Working)
        {
            status = BuildingStatus.Idle;
            DestroyWorkingVFX();
            StartCoroutine(ShowWorkCompleteVFX());
            BuildingEffectsSystem.Instance.ApplyCompleteWorkEffect(buildingType);
            Debug.Log($"Building {buildingType} completed work");
        }
    }
    
    private System.Collections.IEnumerator ShowLevelUpVFX()
    {
        // Create level up VFX GameObject
        GameObject levelUpVFX = new GameObject("LevelUpVFX");
        levelUpVFX.transform.SetParent(transform);
        levelUpVFX.transform.localPosition = Vector3.zero;
        
        // Create a simple sparkle effect
        GameObject sparkleObj = new GameObject("LevelUpSparkle");
        sparkleObj.transform.SetParent(levelUpVFX.transform);
        sparkleObj.transform.localPosition = new Vector3(0, 0.5f, 0);
        
        SpriteRenderer sparkleSR = sparkleObj.AddComponent<SpriteRenderer>();
        
        // Create very simple sparkle - just a few pixels
        Texture2D sparkleTexture = new Texture2D(8, 8);
        Color[] pixels = new Color[8 * 8];
        
        // Simple cross pattern
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                pixels[y * 8 + x] = Color.clear;
                
                // Simple cross
                if ((x == 3 || x == 4) && (y >= 1 && y <= 6)) // Vertical line
                {
                    pixels[y * 8 + x] = new Color(1f, 1f, 0.8f, 1f); // Soft yellow
                }
                else if ((y == 3 || y == 4) && (x >= 1 && x <= 6)) // Horizontal line
                {
                    pixels[y * 8 + x] = new Color(1f, 1f, 0.8f, 1f); // Soft yellow
                }
            }
        }
        
        sparkleTexture.SetPixels(pixels);
        sparkleTexture.Apply();
        
        Sprite sparkleSprite = Sprite.Create(sparkleTexture, 
            new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8);
        sparkleSR.sprite = sparkleSprite;
        sparkleSR.sortingOrder = 15;
        
        // Very simple animation - just fade out while floating up
        float duration = 1f;
        float elapsed = 0f;
        Vector3 startPos = sparkleObj.transform.localPosition;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Float up slowly
            sparkleObj.transform.localPosition = startPos + new Vector3(0, progress * 0.5f, 0);
            
            // Fade out
            Color color = sparkleSR.color;
            color.a = 1f - progress;
            sparkleSR.color = color;
            
            yield return null;
        }
        
        // Clean up
        Destroy(levelUpVFX);
    }
    
    private void CreateUpgradeVFX()
    {
        if (upgradeVFX != null) return; // Already exists
        
        // Create a simple upgrade VFX GameObject
        upgradeVFX = new GameObject("UpgradeVFX");
        upgradeVFX.transform.SetParent(transform);
        upgradeVFX.transform.localPosition = Vector3.zero;
        
        // Add a subtle color tint to the building
        if (spriteRenderer != null)
        {
            UpgradeBuildingTint tint = upgradeVFX.AddComponent<UpgradeBuildingTint>();
            tint.Initialize(spriteRenderer);
        }
        
        // Add a small glowing light above the building
        CreateGlowingLight();
    }
    
    private void CreateGlowingLight()
    {
        GameObject light = new GameObject("GlowingLight");
        light.transform.SetParent(upgradeVFX.transform);
        light.transform.localPosition = new Vector3(0, 0, 0); // Exactly at building center
        
        SpriteRenderer lightSR = light.AddComponent<SpriteRenderer>();
        
        // Create a larger, more obvious glowing orb texture
        Texture2D lightTexture = new Texture2D(24, 24);
        Color[] pixels = new Color[24 * 24];
        Vector2 center = new Vector2(12, 12);
        
        for (int x = 0; x < 24; x++)
        {
            for (int y = 0; y < 24; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= 11f)
                {
                    float alpha = 1f - (distance / 11f);
                    alpha = alpha * alpha; // Make it more concentrated in center
                    pixels[y * 24 + x] = new Color(1f, 0.8f, 0.3f, alpha); // Brighter golden light
                }
                else
                {
                    pixels[y * 24 + x] = Color.clear;
                }
            }
        }
        
        lightTexture.SetPixels(pixels);
        lightTexture.Apply();
        
        Sprite lightSprite = Sprite.Create(lightTexture, 
            new Rect(0, 0, 24, 24), new Vector2(0.5f, 0.5f), 16);
        lightSR.sprite = lightSprite;
        lightSR.sortingOrder = 10; // Above everything
        
        // Add gentle pulsing animation to the light
        UpgradeLightAnimator lightAnimator = light.AddComponent<UpgradeLightAnimator>();
    }
    
    private void DestroyUpgradeVFX()
    {
        if (upgradeVFX != null)
        {
            Destroy(upgradeVFX);
            upgradeVFX = null;
        }
    }
    
    private void CreateWorkingVFX()
    {
        if (workingVFX != null) return; // Already exists
        
        // Create a simple working VFX GameObject
        workingVFX = new GameObject("WorkingVFX");
        workingVFX.transform.SetParent(transform);
        workingVFX.transform.localPosition = Vector3.zero;
        
        // Add a subtle blue tint to the building
        if (spriteRenderer != null)
        {
            WorkingBuildingTint tint = workingVFX.AddComponent<WorkingBuildingTint>();
            tint.Initialize(spriteRenderer);
        }
        
        // Add working indicator particles
        CreateWorkingParticles();
        Debug.Log($"WorkingVFX GameObject created: {workingVFX != null}");
    }
    
    private void CreateWorkingParticles()
    {
        GameObject particles = new GameObject("WorkingParticles");
        particles.transform.SetParent(workingVFX.transform);
        particles.transform.localPosition = new Vector3(0.3f, 0.3f, 0); // Offset from building center
        
        SpriteRenderer particleSR = particles.AddComponent<SpriteRenderer>();
        
        // Create a small working particle texture
        Texture2D particleTexture = new Texture2D(6, 6);
        Color[] pixels = new Color[6 * 6];
        Vector2 center = new Vector2(3, 3);
        
        for (int x = 0; x < 6; x++)
        {
            for (int y = 0; y < 6; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= 2.5f)
                {
                    float alpha = 1f - (distance / 2.5f);
                    pixels[y * 6 + x] = new Color(0.3f, 0.7f, 1f, alpha); // Blue working particle
                }
                else
                {
                    pixels[y * 6 + x] = Color.clear;
                }
            }
        }
        
        particleTexture.SetPixels(pixels);
        particleTexture.Apply();
        
        Sprite particleSprite = Sprite.Create(particleTexture, 
            new Rect(0, 0, 6, 6), new Vector2(0.5f, 0.5f), 8);
        particleSR.sprite = particleSprite;
        particleSR.sortingOrder = 12; // Above building but below upgrade
        
        // Add animation to the particles
        WorkingParticleAnimator particleAnimator = particles.AddComponent<WorkingParticleAnimator>();
    }
    
    private void DestroyWorkingVFX()
    {
        if (workingVFX != null)
        {
            Destroy(workingVFX);
            workingVFX = null;
        }
    }
    
    private System.Collections.IEnumerator ShowWorkCompleteVFX()
    {
        // Create work complete VFX GameObject
        GameObject workCompleteVFX = new GameObject("WorkCompleteVFX");
        workCompleteVFX.transform.SetParent(transform);
        workCompleteVFX.transform.localPosition = Vector3.zero;
        
        // Create a completion burst effect
        GameObject burstObj = new GameObject("CompletionBurst");
        burstObj.transform.SetParent(workCompleteVFX.transform);
        burstObj.transform.localPosition = new Vector3(0, 0.2f, 0);
        
        SpriteRenderer burstSR = burstObj.AddComponent<SpriteRenderer>();
        
        // Create burst texture - simple star pattern
        Texture2D burstTexture = new Texture2D(12, 12);
        Color[] pixels = new Color[12 * 12];
        Vector2 center = new Vector2(6, 6);
        
        for (int x = 0; x < 12; x++)
        {
            for (int y = 0; y < 12; y++)
            {
                pixels[y * 12 + x] = Color.clear;
                
                // Simple star burst pattern
                if ((x == 6 && (y >= 2 && y <= 10)) || // Vertical line
                    (y == 6 && (x >= 2 && x <= 10)) || // Horizontal line
                    ((x - y) == 0 && (x >= 3 && x <= 9)) || // Diagonal /
                    ((x + y) == 12 && (x >= 3 && x <= 9))) // Diagonal \
                {
                    pixels[y * 12 + x] = new Color(0.3f, 1f, 0.3f, 1f); // Green completion color
                }
            }
        }
        
        burstTexture.SetPixels(pixels);
        burstTexture.Apply();
        
        Sprite burstSprite = Sprite.Create(burstTexture, 
            new Rect(0, 0, 12, 12), new Vector2(0.5f, 0.5f), 12);
        burstSR.sprite = burstSprite;
        burstSR.sortingOrder = 16; // Above everything
        
        // Animate the burst - scale up and fade out
        float duration = 0.8f;
        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * 1.5f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Scale up
            burstObj.transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            
            // Fade out
            Color color = burstSR.color;
            color.a = 1f - progress;
            burstSR.color = color;
            
            yield return null;
        }
        
        // Clean up
        Destroy(workCompleteVFX);
    }
    
    private bool LoadBuildingData()
    {
        SaveSystem saveSystem = FindObjectOfType<SaveSystem>();
        if (saveSystem != null)
        {
            GameSaveData saveData = saveSystem.LoadGame();
            if (saveData != null && saveData.buildings.Count > 0)
            {
                LandBuildingAssigner landAssigner = GetComponent<LandBuildingAssigner>();
                if (landAssigner != null)
                {
                    int myLandID = landAssigner.GetLandID();
                    Debug.Log($"[{name}] Trying to load BuildingType={buildingType}, ID={buildingID}, LandID={myLandID}");

                    foreach (BuildingSaveData buildingData in saveData.buildings)
                    {
                        Debug.Log($"Checking saved: Type={buildingData.buildingType}, ID={buildingData.buildingID}, LandID={buildingData.landID}");
                        if (buildingData.landID == myLandID && buildingData.buildingID == buildingID)
                        {
                            level = buildingData.level;
                            status = (BuildingStatus)System.Enum.Parse(typeof(BuildingStatus), buildingData.status);
                            needsRepair = buildingData.needsRepair;      // Add this
                            isRepaired = buildingData.isRepaired; 
                            Debug.Log($"[{name}] Loaded building data successfully.");
                            return true;
                        }
                    }
                }
            }
        }
        Debug.LogWarning($"[{name}] Failed to load building data.");
        return false;
    }

    
    private int GenerateUniqueID()
    {
        return System.DateTime.Now.GetHashCode() + Random.Range(1000, 9999);
    }
    
    private void RegisterWithManager()
    {
        BuildingManager manager = FindObjectOfType<BuildingManager>();
        if (manager != null)
        {
            manager.AddBuilding(buildingType, buildingID);
        }
    }
    
    public int GetBuildingID()
    {
        return buildingID;
    }
    
    public string GetBuildingType()
    {
        return buildingType;
    }
    
    public int GetLevel()
    {
        return level;
    }
    
    public BuildingStatus GetStatus()
    {
        return status;
    }
    
    public void SetStatus(BuildingStatus newStatus)
    {
        status = newStatus;
    }
    
    public void UpgradeLevel()
    {
        level++;
    }

    public bool NeedsRepair()
    {
        return needsRepair;
    }

    public bool IsRepaired()
    {
        return isRepaired;
    }

    public void SetNeedsRepair(bool needs)
    {
        needsRepair = needs;
    }

    public void SetRepaired(bool repaired)
    {
        isRepaired = repaired;
    }

    // Add this method after CompleteWork()
    public void StartRepair()
    {
        if (needsRepair && !isRepaired && status == BuildingStatus.Idle)
        {
            status = BuildingStatus.Working; // Use Working status for repair
            CreateWorkingVFX(); // Reuse working VFX for repair
            Debug.Log($"Building {buildingType} started repair process");
        }
    }

    public void CompleteRepair()
    {
        if (needsRepair && status == BuildingStatus.Working)
        {
            isRepaired = true;
            status = BuildingStatus.Idle;
            DestroyWorkingVFX();
            StartCoroutine(ShowWorkCompleteVFX());
            Debug.Log($"Building {buildingType} repair completed - now functions normally");
        }
    }
    
    void OnMouseEnter()
    {
        // Don't show hover info if any UI is open
        if (IsAnyUIOpen())
        {
            return;
        }
        
        BuildingInfoUI infoUI = FindObjectOfType<BuildingInfoUI>();
        if (infoUI != null)
        {
            string displayText = buildingType;
            if (status == BuildingStatus.Upgrading)
            {
                displayText += " (Upgrading)";
            }
            infoUI.ShowInfo(displayText, level, transform.position);
        }
    }

    // REPLACE the existing OnMouseExit method in BuildingComponent.cs with this:

    void OnMouseExit()
    {
        // Always try to hide hover info when mouse exits, regardless of UI state
        // This ensures cleanup if UI was opened while hovering
        BuildingInfoUI infoUI = FindObjectOfType<BuildingInfoUI>();
        if (infoUI != null)
        {
            infoUI.HideInfo();
        }
    }
    
    void OnMouseDown()
    {
        // Check if ANY UI panel is open to prevent clicking through
        if (IsAnyUIOpen())
        {
            return; // Don't process building click if any UI is open
        }
        
        // Check if UI panel is already open to prevent clicking through
        BuildingInfoUI infoUI = FindObjectOfType<BuildingInfoUI>();
        if (infoUI != null && !infoUI.IsExpanded())
        {
            // Pass this specific building component to the UI
            infoUI.ShowExpandedInfoForBuilding(this, buildingType, level);
        }
    }

    // ADD this new helper method to BuildingComponent.cs
    private bool IsAnyUIOpen()
    {
        if (UniversalPauseMenu.Instance != null && UniversalPauseMenu.Instance.IsPauseMenuShowing())
        {
            return true;
        }
        
        // Check BuildingInfoUI
        BuildingInfoUI buildingUI = FindObjectOfType<BuildingInfoUI>();
        if (buildingUI != null && buildingUI.IsExpanded())
        {
            return true;
        }
        
        // Check SpecialResourceInventory
        if (SpecialResourceInventory.Instance != null && SpecialResourceInventory.Instance.IsShowing())
        {
            return true;
        }
        
        // Check CraftingUI
        CraftingUI craftingUI = FindObjectOfType<CraftingUI>();
        if (craftingUI != null && craftingUI.IsShowing())
        {
            return true;
        }
        
        // Check CropSwapUI
        if (CropSwapUI.Instance != null && CropSwapUI.Instance.IsShowing())
        {
            return true;
        }
        
        // Check GameOverSystem
        if (GameOverSystem.Instance != null && GameOverSystem.Instance.IsGameOver())
        {
            return true;
        }
        
        return false; // No UI is open
    }
}

// Helper component for building tint - very simple effect with subtle glow
public class UpgradeBuildingTint : MonoBehaviour
{
    private SpriteRenderer buildingSpriteRenderer;
    private Color originalColor;
    private float glowSpeed = 1.5f;
    
    public void Initialize(SpriteRenderer buildingSR)
    {
        buildingSpriteRenderer = buildingSR;
        if (buildingSpriteRenderer != null)
        {
            originalColor = buildingSpriteRenderer.color;
        }
    }
    
    void Update()
    {
        if (buildingSpriteRenderer != null)
        {
            // Subtle golden glow that pulses gently
            float glowAmount = (Mathf.Sin(Time.time * glowSpeed) + 1f) / 2f;
            glowAmount = Mathf.Lerp(0.1f, 0.3f, glowAmount); // Gentle range
            
            Color goldenTint = new Color(1f, 0.9f, 0.7f, originalColor.a);
            Color glowedColor = Color.Lerp(originalColor, goldenTint, glowAmount);
            buildingSpriteRenderer.color = glowedColor;
        }
    }
    
    void OnDestroy()
    {
        // Restore original color when effect is removed
        if (buildingSpriteRenderer != null)
        {
            buildingSpriteRenderer.color = originalColor;
        }
    }
}

// Helper component for the glowing light animation
public class UpgradeLightAnimator : MonoBehaviour
{
    private SpriteRenderer lightSpriteRenderer;
    private float pulseSpeed = 2.5f;
    private float floatSpeed = 1f;
    private Vector3 basePosition;
    
    void Start()
    {
        lightSpriteRenderer = GetComponent<SpriteRenderer>();
        basePosition = transform.localPosition;
    }
    
    void Update()
    {
        if (lightSpriteRenderer != null)
        {
            // More obvious pulsing alpha effect
            float alpha = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            alpha = Mathf.Lerp(0.6f, 1f, alpha); // More visible range
            
            Color color = lightSpriteRenderer.color;
            color.a = alpha;
            lightSpriteRenderer.color = color;
            
            // Gentle floating motion
            float yOffset = Mathf.Sin(Time.time * floatSpeed) * 0.08f;
            transform.localPosition = basePosition + new Vector3(0, yOffset, 0);
        }
    }
}

// Helper component for working building tint
public class WorkingBuildingTint : MonoBehaviour
{
    private SpriteRenderer buildingSpriteRenderer;
    private Color originalColor;
    private float pulseSpeed = 2f;
    
    public void Initialize(SpriteRenderer buildingSR)
    {
        buildingSpriteRenderer = buildingSR;
        if (buildingSpriteRenderer != null)
        {
            originalColor = buildingSpriteRenderer.color;
        }
    }
    
    void Update()
    {
        if (buildingSpriteRenderer != null)
        {
            // Subtle blue tint that pulses gently
            float pulseAmount = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            pulseAmount = Mathf.Lerp(0.05f, 0.2f, pulseAmount); // Gentle range
            
            Color blueTint = new Color(0.7f, 0.9f, 1f, originalColor.a);
            Color pulsedColor = Color.Lerp(originalColor, blueTint, pulseAmount);
            buildingSpriteRenderer.color = pulsedColor;
        }
    }
    
    void OnDestroy()
    {
        // Restore original color when effect is removed
        if (buildingSpriteRenderer != null)
        {
            buildingSpriteRenderer.color = originalColor;
        }
    }
}

// Helper component for working particle animation
public class WorkingParticleAnimator : MonoBehaviour
{
    private SpriteRenderer particleSpriteRenderer;
    private float bobSpeed = 1.5f;
    private float fadeSpeed = 3f;
    private Vector3 basePosition;
    
    void Start()
    {
        particleSpriteRenderer = GetComponent<SpriteRenderer>();
        basePosition = transform.localPosition;
    }
    
    void Update()
    {
        if (particleSpriteRenderer != null)
        {
            // Gentle bobbing motion
            float yOffset = Mathf.Sin(Time.time * bobSpeed) * 0.1f;
            transform.localPosition = basePosition + new Vector3(0, yOffset, 0);
            
            // Gentle fading in and out
            float alpha = (Mathf.Sin(Time.time * fadeSpeed) + 1f) / 2f;
            alpha = Mathf.Lerp(0.4f, 0.8f, alpha); // Visible range
            
            Color color = particleSpriteRenderer.color;
            color.a = alpha;
            particleSpriteRenderer.color = color;
        }
    }
}