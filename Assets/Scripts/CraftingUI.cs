using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CraftingUI : MonoBehaviour
{
    private GameObject craftingPanel;
    private bool isShowing = false;
    private Transform recipeContainer;
    private int currentWorkshopLevel = 1;
    private BuildingComponent currentWorkshop;
    
    void Start()
    {
        // UI creation is handled when needed
    }
    
    private void CreateCraftingUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create main crafting panel (full screen overlay)
        GameObject panelObj = new GameObject("CraftingPanel");
        panelObj.transform.SetParent(canvas.transform);
        craftingPanel = panelObj;
        
        // Full screen background
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.9f);
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // Add canvas for proper sorting
        Canvas panelCanvas = panelObj.AddComponent<Canvas>();
        panelCanvas.overrideSorting = true;
        panelCanvas.sortingOrder = 100;
        panelObj.AddComponent<GraphicRaycaster>();
        
        // Create content container (centered)
        GameObject contentContainer = new GameObject("ContentContainer");
        contentContainer.transform.SetParent(panelObj.transform);
        
        RectTransform containerRect = contentContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(500, 400);
        
        Image contentBg = contentContainer.AddComponent<Image>();
        contentBg.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        
        // Create title
        CreateTitle(contentContainer);
        
        // Create scrollable recipe area
        CreateScrollableRecipeArea(contentContainer);
        
        // Create close button
        CreateCloseButton(contentContainer);
        
        craftingPanel.SetActive(false);
    }
    
    private void CreateTitle(GameObject parent)
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(parent.transform);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "Workshop Crafting";
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 20;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.85f);
        titleRect.anchorMax = new Vector2(1, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
    }
    
    private void CreateScrollableRecipeArea(GameObject parent)
    {
        // Create scroll view container
        GameObject scrollViewObj = new GameObject("ScrollView");
        scrollViewObj.transform.SetParent(parent.transform);
        
        RectTransform scrollViewRect = scrollViewObj.AddComponent<RectTransform>();
        scrollViewRect.anchorMin = new Vector2(0.05f, 0.2f);
        scrollViewRect.anchorMax = new Vector2(0.95f, 0.8f);
        scrollViewRect.offsetMin = Vector2.zero;
        scrollViewRect.offsetMax = Vector2.zero;
        
        // Add ScrollRect component
        ScrollRect scrollRect = scrollViewObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        
        // Create viewport
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollViewObj.transform);
        
        RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        
        Image viewportImage = viewportObj.AddComponent<Image>();
        viewportImage.color = new Color(0.05f, 0.05f, 0.05f, 1f);
        
        Mask viewportMask = viewportObj.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        
        // Create content area (this will expand with recipes)
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform);
        
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 0); // Will be auto-sized by layout
        
        // Add layout group for automatic spacing
        VerticalLayoutGroup layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 10f;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        
        // Add content size fitter to auto-resize content area
        ContentSizeFitter contentFitter = contentObj.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Connect ScrollRect to its components
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        
        // Set recipeContainer reference
        recipeContainer = contentRect;
        
        // Create scrollbar (optional but helpful)
        CreateScrollbar(scrollViewObj, scrollRect);
    }
    
    private void CreateScrollbar(GameObject scrollView, ScrollRect scrollRect)
    {
        GameObject scrollbarObj = new GameObject("Scrollbar");
        scrollbarObj.transform.SetParent(scrollView.transform);
        
        RectTransform scrollbarRect = scrollbarObj.AddComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1, 0);
        scrollbarRect.anchorMax = new Vector2(1, 1);
        scrollbarRect.pivot = new Vector2(1, 0.5f);
        scrollbarRect.anchoredPosition = Vector2.zero;
        scrollbarRect.sizeDelta = new Vector2(20, 0);
        
        Image scrollbarBg = scrollbarObj.AddComponent<Image>();
        scrollbarBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        Scrollbar scrollbar = scrollbarObj.AddComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        
        // Create handle
        GameObject handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(scrollbarObj.transform);
        
        RectTransform handleRect = handleObj.AddComponent<RectTransform>();
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = Vector2.one;
        handleRect.offsetMin = Vector2.zero;
        handleRect.offsetMax = Vector2.zero;
        
        Image handleImage = handleObj.AddComponent<Image>();
        handleImage.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        
        scrollbar.handleRect = handleRect;
        scrollbar.targetGraphic = handleImage;
        
        // Connect scrollbar to ScrollRect
        scrollRect.verticalScrollbar = scrollbar;
    }
    
    private void CreateCloseButton(GameObject parent)
    {
        GameObject closeButtonObj = new GameObject("CloseButton");
        closeButtonObj.transform.SetParent(parent.transform);
        
        Image closeButtonImage = closeButtonObj.AddComponent<Image>();
        closeButtonImage.color = new Color(0.6f, 0.2f, 0.2f, 0.8f);
        
        Button closeButton = closeButtonObj.AddComponent<Button>();
        closeButton.onClick.AddListener(HideCraftingUI);
        
        RectTransform closeButtonRect = closeButtonObj.GetComponent<RectTransform>();
        closeButtonRect.anchorMin = new Vector2(0.3f, 0.05f);
        closeButtonRect.anchorMax = new Vector2(0.7f, 0.15f);
        closeButtonRect.offsetMin = Vector2.zero;
        closeButtonRect.offsetMax = Vector2.zero;
        
        GameObject closeTextObj = new GameObject("CloseText");
        closeTextObj.transform.SetParent(closeButtonObj.transform);
        Text closeText = closeTextObj.AddComponent<Text>();
        closeText.text = "Close";
        closeText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        closeText.fontSize = 16;
        closeText.color = Color.white;
        closeText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform closeTextRect = closeTextObj.GetComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;
    }
    
    private void CreateRecipeButton(CraftingRecipe recipe, bool canCraft, bool levelLocked)
    {
        GameObject recipeObj = new GameObject($"Recipe_{recipe.recipeName}");
        recipeObj.transform.SetParent(recipeContainer);
        
        RectTransform recipeRect = recipeObj.AddComponent<RectTransform>();
        recipeRect.sizeDelta = new Vector2(0, 100);
        
        LayoutElement layoutElement = recipeObj.AddComponent<LayoutElement>();
        layoutElement.minHeight = 100f;
        layoutElement.preferredHeight = 100f;
        layoutElement.flexibleWidth = 1f;
        
        // Background color based on state
        Image bgImage = recipeObj.AddComponent<Image>();
        if (levelLocked)
        {
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Dark gray for locked
        }
        else if (canCraft)
        {
            bgImage.color = new Color(0.2f, 0.4f, 0.2f, 0.8f); // Green for available
        }
        else
        {
            bgImage.color = new Color(0.4f, 0.2f, 0.2f, 0.8f); // Red for can't afford
        }
        
        // Recipe name
        CreateRecipeText(recipeObj, "RecipeName", 
            levelLocked ? $"{recipe.recipeName} (Level {recipe.requiredWorkshopLevel} Required)" : recipe.recipeName,
            new Vector2(0.05f, 0.7f), new Vector2(0.7f, 0.9f), 14, FontStyle.Bold, levelLocked ? Color.gray : Color.white);
        
        // Input requirements
        string inputString = "Requires: ";
        for (int i = 0; i < recipe.inputResources.Count; i++)
        {
            inputString += $"{recipe.inputResources[i].requiredQuantity} {recipe.inputResources[i].resourceName}";
            if (i < recipe.inputResources.Count - 1)
                inputString += ", ";
        }
        
        CreateRecipeText(recipeObj, "Inputs", inputString,
            new Vector2(0.05f, 0.45f), new Vector2(0.7f, 0.65f), 11, FontStyle.Normal, 
            levelLocked ? Color.gray : new Color(0.9f, 0.9f, 0.9f));
        
        // Output and duration
        string outputString = "Produces: ";
        for (int i = 0; i < recipe.outputs.Count; i++)
        {
            outputString += $"{recipe.outputs[i].quantity} {recipe.outputs[i].resourceName}";
            if (i < recipe.outputs.Count - 1)
                outputString += ", ";
        }
        outputString += $" ({recipe.workDuration} days)";
        
        CreateRecipeText(recipeObj, "Output", outputString,
            new Vector2(0.05f, 0.2f), new Vector2(0.7f, 0.4f), 11, FontStyle.Normal,
            levelLocked ? Color.gray : new Color(0.8f, 1f, 0.8f));
        
        // Craft button (only if not level locked)
        if (!levelLocked)
        {
            CreateCraftButton(recipeObj, recipe, canCraft);
        }
    }
    
    private void CreateRecipeText(GameObject parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, int fontSize, FontStyle fontStyle, Color color)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform);
        
        Text textComponent = textObj.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.fontSize = fontSize;
        textComponent.color = color;
        textComponent.alignment = TextAnchor.MiddleLeft;
        textComponent.fontStyle = fontStyle;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = anchorMin;
        textRect.anchorMax = anchorMax;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    private void CreateCraftButton(GameObject parent, CraftingRecipe recipe, bool canCraft)
    {
        GameObject craftButtonObj = new GameObject("CraftButton");
        craftButtonObj.transform.SetParent(parent.transform);
        
        Image craftButtonImage = craftButtonObj.AddComponent<Image>();
        craftButtonImage.color = canCraft ? new Color(0.3f, 0.6f, 0.3f, 0.9f) : new Color(0.5f, 0.5f, 0.5f, 0.7f);
        
        Button craftButton = craftButtonObj.AddComponent<Button>();
        craftButton.interactable = canCraft;
        craftButton.onClick.AddListener(() => StartCrafting(recipe));
        
        RectTransform craftButtonRect = craftButtonObj.GetComponent<RectTransform>();
        craftButtonRect.anchorMin = new Vector2(0.75f, 0.3f);
        craftButtonRect.anchorMax = new Vector2(0.95f, 0.7f);
        craftButtonRect.offsetMin = Vector2.zero;
        craftButtonRect.offsetMax = Vector2.zero;
        
        GameObject craftTextObj = new GameObject("CraftText");
        craftTextObj.transform.SetParent(craftButtonObj.transform);
        Text craftText = craftTextObj.AddComponent<Text>();
        craftText.text = "Craft";
        craftText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        craftText.fontSize = 12;
        craftText.color = Color.white;
        craftText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform craftTextRect = craftTextObj.GetComponent<RectTransform>();
        craftTextRect.anchorMin = Vector2.zero;
        craftTextRect.anchorMax = Vector2.one;
        craftTextRect.offsetMin = Vector2.zero;
        craftTextRect.offsetMax = Vector2.zero;
    }
    
    private void StartCrafting(CraftingRecipe recipe)
    {
        if (currentWorkshop == null)
        {
            return;
        }
        
        List<ResourceRequirement> workCosts = new List<ResourceRequirement>();
        foreach (var input in recipe.inputResources)
        {
            workCosts.Add(new ResourceRequirement(input.resourceName, input.requiredQuantity));
        }
        
        bool success = BuildingEffectsSystem.Instance.StartTimedWork(
            currentWorkshop.GetBuildingID(),
            "workshop",
            $"craft_{recipe.recipeName}",
            recipe.workDuration,
            workCosts
        );
        
        if (success)
        {
            HideCraftingUI();
        }
    }
    
    private bool CanCraftRecipe(CraftingRecipe recipe)
    {
        ResourceManager resourceManager = FindObjectOfType<ResourceManager>();
        if (resourceManager == null) return false;
        
        foreach (var input in recipe.inputResources)
        {
            Resource resource = resourceManager.GetResource(input.resourceName);
            if (resource == null || !resource.isUnlocked || resource.quantity < input.requiredQuantity)
            {
                return false;
            }
        }
        
        return true;
    }
    
    private void UpdateRecipeDisplay()
    {
        if (recipeContainer == null)
        {
            CreateCraftingUI();
            if (recipeContainer == null) return;
        }
        
        // Clear existing recipes
        foreach (Transform child in recipeContainer)
        {
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }
        
        FindCurrentWorkshop();
        
        WorkshopEffects.InitializeRecipes();
        List<CraftingRecipe> allRecipes = WorkshopEffects.GetAllRecipes();
        
        foreach (CraftingRecipe recipe in allRecipes)
        {
            bool levelLocked = recipe.requiredWorkshopLevel > currentWorkshopLevel;
            bool canCraft = !levelLocked && CanCraftRecipe(recipe);
            
            CreateRecipeButton(recipe, canCraft, levelLocked);
        }
    }
    
    private void FindCurrentWorkshop()
    {
        BuildingComponent[] allBuildings = FindObjectsOfType<BuildingComponent>();
        foreach (BuildingComponent building in allBuildings)
        {
            if (building.GetBuildingType().ToLower() == "workshop" || building.GetBuildingType().ToLower() == "zuofang")
            {
                currentWorkshop = building;
                currentWorkshopLevel = building.GetLevel();
                break;
            }
        }
        
        if (currentWorkshop == null)
        {
            currentWorkshopLevel = 1;
        }
    }
    
    public void ShowCraftingUI()
    {
        if (craftingPanel == null || recipeContainer == null)
        {
            CreateCraftingUI();
        }
        
        if (craftingPanel != null && recipeContainer != null)
        {
            UpdateRecipeDisplay();
            craftingPanel.SetActive(true);
            isShowing = true;
        }
    }
    
    public void HideCraftingUI()
    {
        if (craftingPanel != null)
        {
            craftingPanel.SetActive(false);
            isShowing = false;
        }
    }
    
    public bool IsShowing()
    {
        return isShowing;
    }
}