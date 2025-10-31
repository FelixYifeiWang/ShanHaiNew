using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class LandCoverInfoUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;
    public Transform requirementsContainer;
    public GameObject resourceItemPrefab;
    public Button unlockButton;
    
    [Header("Button Sprites")]
    public Sprite buttonCanAfford; // UI_v1_8 - when player can afford
    public Sprite buttonCannotAfford; // UI_v1_12 - when player cannot afford
    
    [Header("Resource Icon")]
    public Sprite placeholderResourceIcon; // Assign building_icons_15 in inspector
    
    [Header("Panel Background Images")]
    public Sprite land2BackgroundImage;
    public Sprite land3BackgroundImage;
    public Sprite land4BackgroundImage;
    public Sprite land5BackgroundImage;
    public Sprite land6BackgroundImage;
    public Sprite defaultBackgroundImage;
    
    [Header("Resource UI Settings")]
    public Vector2 iconSize = new Vector2(32, 32);
    public int textFontSize = 16;
    public Vector2 textSize = new Vector2(60, 32);
    public Color textColor = Color.white;
    public float spacingBetweenItems = 10f;
    public float spacingBetweenResources = 15f;
    public float resourcesVerticalOffset = 0f;
    
    [Header("Animation Settings")]
    public float fadeDuration = 0.3f;
    public float hideDelay = 0.2f; // Delay before hiding to allow mouse to move to panel
    
    [Header("Positioning Settings")]
    public Vector2 panelOffset = Vector2.zero; // Global offset applied to all positions
    [Header("Panel Positions (simple offset from center)")]
    public Vector2 land2Position = new Vector2(-300, 150); // Position for Land 2
    public Vector2 land3Position = new Vector2(0, 200);    // Position for Land 3  
    public Vector2 land4Position = new Vector2(300, 150);  // Position for Land 4
    public Vector2 land5Position = new Vector2(-200, 0);   // Position for Land 5
    public Vector2 land6Position = new Vector2(200, -50);  // Position for Land 6

    [Header("Resource Text Font")]
    public TMP_FontAsset resourceTextFont;
    
    private Coroutine currentFadeCoroutine;
    private Coroutine currentHideCoroutine;
    private LandUnlockSystem unlockSystem;
    private int currentLandID = -1;
    private bool isPanelHovered = false;
    
    void Start()
    {
        unlockSystem = FindObjectOfType<LandUnlockSystem>();
        
        if (resourceItemPrefab == null)
        {
            CreateResourceItemPrefab();
        }
        
        SetupButtonClickHandler();
        SetupPanelHoverDetection();
        
        if (panel != null)
        {
            StartCoroutine(DelayedHidePanel());
        }
    }
    
    public void ShowLandInfo(int landID)
    {
        if (IsAnyUIOpen())
        {
            return; // Don't show land info if any UI is open
        }

        currentLandID = landID;
        
        if (panel != null)
        {
            if (currentHideCoroutine != null)
            {
                StopCoroutine(currentHideCoroutine);
                currentHideCoroutine = null;
            }
            
            panel.SetActive(true);
            PositionPanelForLand(landID);
            UpdatePanelBackgroundImage(landID);
            UpdateRequirements(landID);
            
            if (!gameObject.activeInHierarchy)
            {
                CanvasGroup canvasGroup = GetOrAddCanvasGroup();
                canvasGroup.alpha = 1f;
                return;
            }
            
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
            }
            
            currentFadeCoroutine = StartCoroutine(FadeInPanel());
        }
    }
    
    public void HideLandInfo()
    {
        if (panel != null)
        {
            if (!gameObject.activeInHierarchy)
            {
                panel.SetActive(false);
                return;
            }
            
            if (currentHideCoroutine != null)
            {
                StopCoroutine(currentHideCoroutine);
            }
            
            currentHideCoroutine = StartCoroutine(DelayedHide());
        }
    }
    
    private System.Collections.IEnumerator DelayedHide()
    {
        yield return new WaitForSeconds(hideDelay);
        
        // Check if panel is still hovered before hiding
        if (!isPanelHovered)
        {
            if (!gameObject.activeInHierarchy)
            {
                panel.SetActive(false);
                currentHideCoroutine = null;
                yield break;
            }
            
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
            }
            
            currentFadeCoroutine = StartCoroutine(FadeOutPanel());
        }
        
        currentHideCoroutine = null;
    }
    
    private void SetupPanelHoverDetection()
    {
        if (panel == null) return;
        
        // Add EventTrigger component for hover detection
        UnityEngine.EventSystems.EventTrigger eventTrigger = panel.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = panel.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }
        
        // Clear existing entries to avoid duplicates
        eventTrigger.triggers.Clear();
        
        // Add mouse enter event
        UnityEngine.EventSystems.EventTrigger.Entry entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => OnPanelMouseEnter());
        eventTrigger.triggers.Add(entryEnter);
        
        // Add mouse exit event
        UnityEngine.EventSystems.EventTrigger.Entry entryExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => OnPanelMouseExit());
        eventTrigger.triggers.Add(entryExit);
        
        // Ensure EventTrigger doesn't block button clicks by setting the right properties
        if (panel.GetComponent<Image>() != null)
        {
            panel.GetComponent<Image>().raycastTarget = true;
        }
    }
    
    private void OnPanelMouseEnter()
    {
        if (IsAnyUIOpen())
        {
            isPanelHovered = false;
            if (currentHideCoroutine == null)
            {
                currentHideCoroutine = StartCoroutine(DelayedHide());
            }
            return;
        }

        isPanelHovered = true;
        
        // Cancel any pending hide operation
        if (currentHideCoroutine != null)
        {
            StopCoroutine(currentHideCoroutine);
            currentHideCoroutine = null;
        }
    }
    
    private void OnPanelMouseExit()
    {
        isPanelHovered = false;
        
        // Start hide process when mouse leaves panel
        if (currentHideCoroutine != null)
        {
            StopCoroutine(currentHideCoroutine);
        }
        currentHideCoroutine = StartCoroutine(DelayedHide());
    }
    
    private System.Collections.IEnumerator DelayedHidePanel()
    {
        yield return null;
        yield return null;
        yield return null;
        
        if (gameObject.activeInHierarchy && panel != null)
        {
            currentFadeCoroutine = StartCoroutine(FadeOutPanel());
        }
        else if (panel != null)
        {
            panel.SetActive(false);
        }
    }
    
    private System.Collections.IEnumerator FadeInPanel()
    {
        panel.SetActive(true);
        CanvasGroup canvasGroup = GetOrAddCanvasGroup();
        canvasGroup.alpha = 0f;
        
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        currentFadeCoroutine = null;
    }

    private bool IsAnyUIOpen()
    {
        if (UniversalPauseMenu.Instance != null && UniversalPauseMenu.Instance.IsPauseMenuShowing())
        {
            return true;
        }
        
        // Check BuildingPanelController
        BuildingPanelController panelController = FindObjectOfType<BuildingPanelController>();
        if (panelController != null && panelController.IsPanelVisible())
        {
            return true;
        }

        BuildingPanelController_Farmhouse farmhousePanel = FindObjectOfType<BuildingPanelController_Farmhouse>();
        if (farmhousePanel != null && farmhousePanel.IsPanelVisible())
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
    
    private System.Collections.IEnumerator FadeOutPanel()
    {
        CanvasGroup canvasGroup = GetOrAddCanvasGroup();
        float startAlpha = canvasGroup.alpha;
        
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        panel.SetActive(false);
        currentFadeCoroutine = null;
    }
    
    private CanvasGroup GetOrAddCanvasGroup()
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }
        return canvasGroup;
    }
    
    private void PositionPanelForLand(int landID)
    {
        if (panel == null) return;
        
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        if (panelRect == null) return;
        
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        
        Vector2 targetPosition = Vector2.zero;
        
        switch (landID)
        {
            case 2: targetPosition = land2Position; break;
            case 3: targetPosition = land3Position; break;
            case 4: targetPosition = land4Position; break;
            case 5: targetPosition = land5Position; break;
            case 6: targetPosition = land6Position; break;
            default: targetPosition = Vector2.zero; break;
        }
        
        targetPosition += panelOffset;
        panelRect.anchoredPosition = targetPosition;
    }
    
    private void UpdatePanelBackgroundImage(int landID)
    {
        if (panel == null) return;
        
        Image panelImage = panel.GetComponent<Image>();
        if (panelImage == null) return;
        
        Sprite backgroundSprite = GetBackgroundImageForLand(landID);
        if (backgroundSprite != null)
        {
            panelImage.sprite = backgroundSprite;
        }
    }
    
    private Sprite GetBackgroundImageForLand(int landID)
    {
        switch (landID)
        {
            case 2: return land2BackgroundImage;
            case 3: return land3BackgroundImage;
            case 4: return land4BackgroundImage;
            case 5: return land5BackgroundImage;
            case 6: return land6BackgroundImage;
            default: return defaultBackgroundImage;
        }
    }
    
    private void UpdateRequirements(int landID)
    {
        ClearRequirements();
        
        if (requirementsContainer != null)
        {
            RectTransform containerRect = requirementsContainer.GetComponent<RectTransform>();
            if (containerRect != null)
            {
                Vector3 pos = containerRect.anchoredPosition;
                pos.y = resourcesVerticalOffset;
                containerRect.anchoredPosition = pos;
            }
            
            HorizontalLayoutGroup containerLayout = requirementsContainer.GetComponent<HorizontalLayoutGroup>();
            if (containerLayout != null)
            {
                containerLayout.spacing = spacingBetweenResources;
            }
        }
        
        List<ResourceRequirement> requirements = GetLandRequirements(landID);
        
        foreach (ResourceRequirement req in requirements)
        {
            CreateResourceItem(req.resourceName, req.requiredQuantity);
        }
        
        UpdateButtonSprite(requirements);
    }
    
    private void ClearRequirements()
    {
        if (requirementsContainer == null) return;
        
        for (int i = requirementsContainer.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(requirementsContainer.GetChild(i).gameObject);
        }
    }
    
    private void CreateResourceItem(string resourceName, int quantity)
    {
        if (requirementsContainer == null || resourceItemPrefab == null) return;
        
        GameObject item = Instantiate(resourceItemPrefab, requirementsContainer);
        
        Image iconImage = item.transform.Find("Icon").GetComponent<Image>();
        if (iconImage != null && placeholderResourceIcon != null)
        {
            iconImage.sprite = placeholderResourceIcon;
            RectTransform iconRect = iconImage.GetComponent<RectTransform>();
            iconRect.sizeDelta = iconSize;
        }
        
        int currentAmount = GetCurrentResourceAmount(resourceName);
        
        TextMeshProUGUI quantityText = item.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        if (quantityText != null)
        {
            quantityText.text = $"{currentAmount}/{quantity}";
            quantityText.fontSize = textFontSize;
            quantityText.color = textColor;

            if (resourceTextFont != null)
                quantityText.font = resourceTextFont;

            RectTransform textRect = quantityText.GetComponent<RectTransform>();
            textRect.sizeDelta = textSize;
        }
        
        HorizontalLayoutGroup layout = item.GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
        {
            layout.spacing = spacingBetweenItems;
        }
    }
    
    private int GetCurrentResourceAmount(string resourceName)
    {
        ResourceManager resourceManager = FindObjectOfType<ResourceManager>();
        if (resourceManager == null) return 0;
        
        Resource resource = resourceManager.GetResource(resourceName);
        if (resource == null || !resource.isUnlocked)
        {
            return 0;
        }
        
        return resource.quantity;
    }
    
    private void CreateResourceItemPrefab()
    {
        resourceItemPrefab = new GameObject("ResourceItem");
        
        HorizontalLayoutGroup layout = resourceItemPrefab.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = spacingBetweenItems;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        
        ContentSizeFitter fitter = resourceItemPrefab.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(resourceItemPrefab.transform);
        Image iconImage = iconObj.AddComponent<Image>();
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.sizeDelta = iconSize;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(resourceItemPrefab.transform);
        TextMeshProUGUI quantityText = textObj.AddComponent<TextMeshProUGUI>();
        quantityText.font = null;
        quantityText.fontSize = textFontSize;
        quantityText.color = textColor;
        quantityText.alignment = TextAlignmentOptions.Center;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = textSize;
    }
    
    private void SetupButtonClickHandler()
    {
        if (unlockButton == null && panel != null)
        {
            unlockButton = panel.GetComponentInChildren<Button>();
        }
        
        if (unlockButton != null)
        {
            unlockButton.onClick.RemoveAllListeners();
            unlockButton.onClick.AddListener(OnUnlockButtonClicked);
            unlockButton.interactable = true;
        }
    }
    
    private void OnUnlockButtonClicked()
    {
        if (unlockSystem != null && currentLandID != -1)
        {
            bool success = unlockSystem.TryUnlockLand(currentLandID);
            if (success)
            {
                HideLandInfoImmediate();
            }
        }
    }
    
    private void HideLandInfoImmediate()
    {
        if (panel != null)
        {
            if (currentHideCoroutine != null)
            {
                StopCoroutine(currentHideCoroutine);
                currentHideCoroutine = null;
            }
            
            if (!gameObject.activeInHierarchy)
            {
                panel.SetActive(false);
                return;
            }
            
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
            }
            
            currentFadeCoroutine = StartCoroutine(FadeOutPanel());
        }
    }
    
    private List<ResourceRequirement> GetLandRequirements(int landID)
    {
        List<ResourceRequirement> requirements = new List<ResourceRequirement>();
        
        switch (landID)
        {
            case 2:
                requirements.Add(new ResourceRequirement("actpoint", 2));
                break;
            case 3:
                requirements.Add(new ResourceRequirement("actpoint", 2));
                break;
            case 4:
                requirements.Add(new ResourceRequirement("actpoint", 2));
                break;
            case 5:
                requirements.Add(new ResourceRequirement("gold", 1));
                break;
            case 6:
                requirements.Add(new ResourceRequirement("actpoint", 2));
                requirements.Add(new ResourceRequirement("crop2", 2));
                break;
        }
        
        return requirements;
    }
    
    private bool CanAffordRequirements(List<ResourceRequirement> requirements)
    {
        foreach (ResourceRequirement req in requirements)
        {
            int currentAmount = GetCurrentResourceAmount(req.resourceName);
            if (currentAmount < req.requiredQuantity)
            {
                return false;
            }
        }
        return true;
    }
    
    private void UpdateButtonSprite(List<ResourceRequirement> requirements)
    {
        if (unlockButton == null)
        {
            if (panel != null)
            {
                unlockButton = panel.GetComponentInChildren<Button>();
            }
        }
        
        if (unlockButton == null) return;
        
        Image buttonImage = unlockButton.GetComponent<Image>();
        if (buttonImage == null) return;
        
        bool canAfford = CanAffordRequirements(requirements);
        
        if (canAfford && buttonCanAfford != null)
        {
            buttonImage.sprite = buttonCanAfford;
        }
        else if (!canAfford && buttonCannotAfford != null)
        {
            buttonImage.sprite = buttonCannotAfford;
        }
    }
}