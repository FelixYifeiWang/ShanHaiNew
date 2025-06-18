using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ResourceRequirement
{
    public string resourceName;
    public int requiredQuantity;
    
    public ResourceRequirement(string name, int quantity)
    {
        resourceName = name;
        requiredQuantity = quantity;
    }
}

[System.Serializable]
public class LandUnlockRequirement
{
    public int landID;
    public List<ResourceRequirement> requiredResources = new List<ResourceRequirement>();
    
    public LandUnlockRequirement(int id)
    {
        landID = id;
    }
    
    public void AddRequirement(string resourceName, int quantity)
    {
        requiredResources.Add(new ResourceRequirement(resourceName, quantity));
    }
}

public class LandUnlockSystem : MonoBehaviour
{
    [SerializeField] private List<LandUnlockRequirement> landRequirements = new List<LandUnlockRequirement>();
    [SerializeField] private Button unlockButton;
    
    private ResourceManager resourceManager;
    private LandManager landManager;
    
    void Start()
    {
        resourceManager = FindObjectOfType<ResourceManager>();
        landManager = FindObjectOfType<LandManager>();
        InitializeLandRequirements();
        
        // Subscribe to land unlock events to unlock resources
        LandEvents.OnLandUnlocked += OnLandUnlocked;
        
        if (unlockButton == null)
        {
            CreateUnlockButton();
        }
        
        if (unlockButton != null)
        {
            unlockButton.onClick.AddListener(TryUnlockNextLand);
        }
        
        // Update button display initially and periodically
        UpdateButtonDisplay();
        InvokeRepeating("UpdateButtonDisplay", 0.5f, 0.5f);
    }
    
    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        LandEvents.OnLandUnlocked -= OnLandUnlocked;
    }
    
    private void OnLandUnlocked(int landID)
    {
        // Unlock resources when specific lands are unlocked
        switch (landID)
        {
            case 2:
                if (resourceManager != null)
                {
                    resourceManager.UnlockResource("crop1");
                }
                break;
            // crop2 and crop3 are now unlocked by storage building upgrades instead
        }
        
        // Update button display when land is unlocked
        UpdateButtonDisplay();
    }
    
    private void CreateUnlockButton()
    {
        // Find or create canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create button GameObject
        GameObject buttonObj = new GameObject("UnlockButton");
        buttonObj.transform.SetParent(canvas.transform);
        
        // Add Image component for button background
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.8f, 0.4f, 0.2f, 0.8f); // Orange
        
        // Add Button component
        unlockButton = buttonObj.AddComponent<Button>();
        
        // Position button in bottom right
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1, 0);
        buttonRect.anchorMax = new Vector2(1, 0);
        buttonRect.pivot = new Vector2(1, 0);
        buttonRect.anchoredPosition = new Vector2(-10, 10);
        buttonRect.sizeDelta = new Vector2(120, 40);
        
        // Create main button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = "Unlock Land";
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.fontSize = 14;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        // Position main text to upper portion
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0.4f);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Create cost text
        GameObject costTextObj = new GameObject("CostText");
        costTextObj.transform.SetParent(buttonObj.transform);
        
        Text costText = costTextObj.AddComponent<Text>();
        costText.text = "";
        costText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        costText.fontSize = 10;
        costText.color = new Color(0.8f, 0.8f, 0.8f, 1f); // Light gray
        costText.alignment = TextAnchor.MiddleCenter;
        
        // Position cost text in lower portion
        RectTransform costTextRect = costTextObj.GetComponent<RectTransform>();
        costTextRect.anchorMin = new Vector2(0, 0);
        costTextRect.anchorMax = new Vector2(1, 0.4f);
        costTextRect.offsetMin = Vector2.zero;
        costTextRect.offsetMax = Vector2.zero;
    }
    
    private void UpdateButtonDisplay()
    {
        if (unlockButton == null) return;
        
        Text buttonText = unlockButton.transform.Find("Text").GetComponent<Text>();
        Text costText = unlockButton.transform.Find("CostText").GetComponent<Text>();
        Image buttonImage = unlockButton.GetComponent<Image>();
        
        // Find the next locked land
        int nextLandID = GetNextLockedLandID();
        
        if (nextLandID == -1)
        {
            // All lands unlocked
            if (buttonText != null) buttonText.text = "All Unlocked";
            if (costText != null) costText.text = "";
            unlockButton.interactable = false;
            if (buttonImage != null) buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 0.6f); // Gray
            return;
        }
        
        // Check if we can unlock the next land
        bool canUnlock = CanUnlockLand(nextLandID);
        LandUnlockRequirement requirement = GetLandRequirement(nextLandID);
        
        // Update main text
        if (buttonText != null)
        {
            if (canUnlock)
            {
                buttonText.text = $"Unlock Land {nextLandID}";
            }
            else
            {
                buttonText.text = "Can't Afford";
            }
        }
        
        // Update cost text
        if (costText != null && requirement != null)
        {
            string costString = "";
            for (int i = 0; i < requirement.requiredResources.Count; i++)
            {
                costString += $"{requirement.requiredResources[i].requiredQuantity} {requirement.requiredResources[i].resourceName}";
                if (i < requirement.requiredResources.Count - 1)
                {
                    costString += ", ";
                }
            }
            costText.text = costString;
        }
        
        // Update button state
        unlockButton.interactable = canUnlock;
        if (buttonImage != null)
        {
            if (canUnlock)
            {
                buttonImage.color = new Color(0.8f, 0.4f, 0.2f, 0.8f); // Orange
            }
            else
            {
                buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 0.6f); // Gray
            }
        }
    }
    
    private int GetNextLockedLandID()
    {
        for (int landID = 2; landID <= 6; landID++)
        {
            if (landManager != null && !landManager.IsLandUnlocked(landID))
            {
                return landID;
            }
        }
        return -1; // All lands unlocked
    }
    
    private void TryUnlockNextLand()
    {
        // Find the next locked land
        for (int landID = 2; landID <= 6; landID++)
        {
            if (landManager != null && !landManager.IsLandUnlocked(landID))
            {
                if (TryUnlockLand(landID))
                {
                    return;
                }
                else
                {
                    LogMissingResources(landID);
                    return;
                }
            }
        }
        
        //Debug.Log("All lands are already unlocked!");
    }
    
    private void LogMissingResources(int landID)
    {
        List<ResourceRequirement> missing = GetMissingResources(landID);
        if (missing.Count > 0)
        {
            string missingText = "Missing: ";
            foreach (ResourceRequirement req in missing)
            {
                missingText += $"{req.requiredQuantity} {req.resourceName}, ";
            }
            //Debug.Log(missingText.TrimEnd(',', ' '));
        }
    }
    
    private void InitializeLandRequirements()
    {
        // Land 2: storage, tian1, tian2
        LandUnlockRequirement land2 = new LandUnlockRequirement(2);
        land2.AddRequirement("actpoint", 2);
        landRequirements.Add(land2);
        
        // Land 3: farmhouse, tian3-6
        LandUnlockRequirement land3 = new LandUnlockRequirement(3);
        land3.AddRequirement("actpoint", 2);
        landRequirements.Add(land3);
        
        // Land 4: zuofang
        LandUnlockRequirement land4 = new LandUnlockRequirement(4);
        land4.AddRequirement("actpoint", 2);
        landRequirements.Add(land4);
        
        // Land 5: brokenstone, tower
        LandUnlockRequirement land5 = new LandUnlockRequirement(5);

        land5.AddRequirement("actpoint", 2);
        landRequirements.Add(land5);
        
        // Land 6: entrance
        LandUnlockRequirement land6 = new LandUnlockRequirement(6);

        land6.AddRequirement("actpoint", 2);
        landRequirements.Add(land6);
    }
    
    public bool CanUnlockLand(int landID)
    {
        if (resourceManager == null) return false;
        
        LandUnlockRequirement requirement = GetLandRequirement(landID);
        if (requirement == null) return false;
        
        foreach (ResourceRequirement resourceReq in requirement.requiredResources)
        {
            Resource resource = resourceManager.GetResource(resourceReq.resourceName);
            if (resource == null || !resource.isUnlocked || resource.quantity < resourceReq.requiredQuantity)
            {
                return false;
            }
        }
        
        return true;
    }
    
    public bool TryUnlockLand(int landID)
    {
        if (!CanUnlockLand(landID)) return false;
        
        if (landManager != null && !landManager.IsLandUnlocked(landID))
        {
            // Consume resources
            ConsumeUnlockResources(landID);
            
            // Unlock the land
            landManager.UnlockLand(landID);
            
            //Debug.Log($"Land {landID} unlocked!");
            return true;
        }
        
        return false;
    }
    
    private void ConsumeUnlockResources(int landID)
    {
        LandUnlockRequirement requirement = GetLandRequirement(landID);
        if (requirement == null) return;
        
        foreach (ResourceRequirement resourceReq in requirement.requiredResources)
        {
            resourceManager.AddToResource(resourceReq.resourceName, -resourceReq.requiredQuantity);
        }
    }
    
    public LandUnlockRequirement GetLandRequirement(int landID)
    {
        foreach (LandUnlockRequirement requirement in landRequirements)
        {
            if (requirement.landID == landID)
            {
                return requirement;
            }
        }
        return null;
    }
    
    public List<ResourceRequirement> GetMissingResources(int landID)
    {
        List<ResourceRequirement> missing = new List<ResourceRequirement>();
        
        LandUnlockRequirement requirement = GetLandRequirement(landID);
        if (requirement == null) return missing;
        
        foreach (ResourceRequirement resourceReq in requirement.requiredResources)
        {
            Resource resource = resourceManager.GetResource(resourceReq.resourceName);
            if (resource == null || !resource.isUnlocked)
            {
                missing.Add(new ResourceRequirement(resourceReq.resourceName, resourceReq.requiredQuantity));
            }
            else if (resource.quantity < resourceReq.requiredQuantity)
            {
                int missingAmount = resourceReq.requiredQuantity - resource.quantity;
                missing.Add(new ResourceRequirement(resourceReq.resourceName, missingAmount));
            }
        }
        
        return missing;
    }
}