using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceDisplay : MonoBehaviour
{
    [SerializeField] private GameObject resourceItemPrefab;
    [SerializeField] private Transform resourceContainer;
    
    private ResourceManager resourceManager;
    private Dictionary<string, GameObject> resourceUIItems = new Dictionary<string, GameObject>();
    
    void Start()
    {
        resourceManager = FindObjectOfType<ResourceManager>();
        CreateResourceItemPrefab();
        UpdateDisplay();
        InvokeRepeating("UpdateDisplay", 0.1f, 0.1f);
    }
    
    private void CreateResourceItemPrefab()
    {
        if (resourceItemPrefab == null)
        {
            // Create prefab programmatically
            resourceItemPrefab = new GameObject("ResourceItem");
            
            // Add horizontal layout
            HorizontalLayoutGroup layout = resourceItemPrefab.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.spacing = 5f;
            
            // Add content size fitter
            ContentSizeFitter fitter = resourceItemPrefab.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Create resource name text
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(resourceItemPrefab.transform);
            Text nameText = nameObj.AddComponent<Text>();
            nameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            nameText.fontSize = 14;
            nameText.color = Color.white;
            
            // Create resource quantity text
            GameObject quantityObj = new GameObject("Quantity");
            quantityObj.transform.SetParent(resourceItemPrefab.transform);
            Text quantityText = quantityObj.AddComponent<Text>();
            quantityText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            quantityText.fontSize = 14;
            quantityText.color = Color.yellow;
        }
    }
    
    void UpdateDisplay()
    {
        if (resourceManager == null) return;
        
        // Use GetRegularResources instead of GetAllResources
        Dictionary<string, Resource> allResources = resourceManager.GetRegularResources();
        
        // Remove UI items for resources that are no longer unlocked
        List<string> toRemove = new List<string>();
        foreach (var item in resourceUIItems)
        {
            string resourceName = item.Key;
            if (!allResources.ContainsKey(resourceName) || !allResources[resourceName].isUnlocked)
            {
                Destroy(item.Value);
                toRemove.Add(resourceName);
            }
        }
        foreach (string name in toRemove)
        {
            resourceUIItems.Remove(name);
        }
        
        // Add or update UI items for unlocked regular resources
        foreach (var resourcePair in allResources)
        {
            string resourceName = resourcePair.Key;
            Resource resource = resourcePair.Value;
            
            if (resource.isUnlocked)
            {
                if (!resourceUIItems.ContainsKey(resourceName))
                {
                    // Create new UI item
                    GameObject newItem = Instantiate(resourceItemPrefab, resourceContainer);
                    newItem.name = resourceName;
                    resourceUIItems[resourceName] = newItem;
                }
                
                // Update the text
                GameObject item = resourceUIItems[resourceName];
                Text nameText = item.transform.Find("Name").GetComponent<Text>();
                Text quantityText = item.transform.Find("Quantity").GetComponent<Text>();
                
                nameText.text = resourceName + ":";
                quantityText.text = resource.quantity.ToString();
            }
        }
    }
}