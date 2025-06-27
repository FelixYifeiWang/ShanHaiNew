using UnityEngine;

[CreateAssetMenu(fileName = "UmbrellaSkill", menuName = "Adventure/Skills/Umbrella")]
public class UmbrellaSkill : AdventureSkill
{
    private HexGridManager gridManager;
    
    public override void ExecuteSkill(int skillLevel, Vector2Int? targetTile = null)
    {
        if (!targetTile.HasValue) return;
        
        gridManager = FindObjectOfType<HexGridManager>();
        if (gridManager == null) return;
        
        int targetRow = targetTile.Value.x;
        int targetCol = targetTile.Value.y;
        int bombsRemoved = 0;
        
        // Remove bomb from target tile
        if (gridManager.GetTileType(targetRow, targetCol) == HexTileType.Bomb)
        {
            gridManager.SetTileType(targetRow, targetCol, HexTileType.Empty);
            bombsRemoved++;
        }
        
        // Remove bombs from neighbors
        int[] neighborRows, neighborCols;
        GetHexNeighbors(targetRow, targetCol, out neighborRows, out neighborCols);
        
        for (int i = 0; i < neighborRows.Length; i++)
        {
            int nr = neighborRows[i];
            int nc = neighborCols[i];
            
            if (gridManager.IsValidPosition(nr, nc) && 
                gridManager.GetTileType(nr, nc) == HexTileType.Bomb)
            {
                gridManager.SetTileType(nr, nc, HexTileType.Empty);
                bombsRemoved++;
            }
        }
        
        // Update indicators for all revealed tiles
        UpdateAllRevealedTileIndicators();
        Debug.Log($"Umbrella removed {bombsRemoved} bombs around tile ({targetRow}, {targetCol})");
    }
    
    public override bool IsValidTarget(int row, int col)
    {
        gridManager = FindObjectOfType<HexGridManager>();
        return gridManager != null && gridManager.IsClickable(row, col);
    }
    
    public override void ApplyTargetingVisuals()
    {
        gridManager = FindObjectOfType<HexGridManager>();
        if (gridManager == null) return;
        
        HexTileController[] allTiles = FindObjectsOfType<HexTileController>();
        foreach (HexTileController tile in allTiles)
        {
            if (gridManager.IsClickable(tile.row, tile.column))
            {
                SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(1f, 0.5f, 0.5f, 1f); // Red tint
            }
        }
    }
    
    public override void RemoveTargetingVisuals()
    {
        gridManager = FindObjectOfType<HexGridManager>();
        if (gridManager == null) return;
        
        HexTileController[] allTiles = FindObjectsOfType<HexTileController>();
        foreach (HexTileController tile in allTiles)
        {
            if (gridManager.IsClickable(tile.row, tile.column))
            {
                SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = Color.white; // Reset to normal
            }
        }
    }
    
    // Helper method to get hex neighbors (same logic as HexGridManager)
    private void GetHexNeighbors(int row, int col, out int[] neighborRows, out int[] neighborCols)
    {
        int[,] evenColOffsets = { {-1, -1}, {-1, 0}, {0, -1}, {0, 1}, {-1, 1}, {1, 0} };
        int[,] oddColOffsets = { {-1, 0}, {1, -1}, {0, -1}, {0, 1}, {1, 0}, {1, 1} };
        
        int[,] offsets = (col % 2 == 0) ? evenColOffsets : oddColOffsets;
        
        neighborRows = new int[6];
        neighborCols = new int[6];
        
        for (int i = 0; i < 6; i++)
        {
            neighborRows[i] = row + offsets[i, 0];
            neighborCols[i] = col + offsets[i, 1];
        }
    }
    
    // Update indicators for all revealed tiles (since bomb removal affects indicators globally)
    private void UpdateAllRevealedTileIndicators()
    {
        if (gridManager == null) return;
        
        HexTileController[] allTiles = FindObjectsOfType<HexTileController>();
        foreach (HexTileController tile in allTiles)
        {
            // Only update revealed empty tiles (they show indicators)
            if (gridManager.IsRevealed(tile.row, tile.column) && 
                gridManager.GetTileType(tile.row, tile.column) == HexTileType.Empty)
            {
                // Remove old indicators and add new ones
                RemoveAllIndicators(tile.gameObject);
                AddWarningIndicatorsForTile(tile.gameObject, tile.row, tile.column);
            }
        }
    }
    
    // Remove all indicator children from a tile
    private void RemoveAllIndicators(GameObject tileObject)
    {
        for (int i = tileObject.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = tileObject.transform.GetChild(i);
            if (child.name.StartsWith("Indicator_"))
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
    
    // Add warning indicators for a specific tile
    private void AddWarningIndicatorsForTile(GameObject tileObject, int row, int col)
    {
        // Get neighbors and check for special tiles
        int[] neighborRows, neighborCols;
        GetHexNeighbors(row, col, out neighborRows, out neighborCols);
        
        bool hasBombNearby = false;
        bool hasTreasureNearby = false;
        bool hasDestinationNearby = false;
        
        for (int i = 0; i < neighborRows.Length; i++)
        {
            int nr = neighborRows[i];
            int nc = neighborCols[i];
            
            if (gridManager.IsValidPosition(nr, nc))
            {
                HexTileType neighborType = gridManager.GetTileType(nr, nc);
                
                if (neighborType == HexTileType.Bomb)
                    hasBombNearby = true;
                else if (neighborType == HexTileType.Treasure)
                    hasTreasureNearby = true;
                else if (neighborType == HexTileType.Destination)
                    hasDestinationNearby = true;
            }
        }
        
        // Create indicators if needed
        if (hasBombNearby || hasTreasureNearby || hasDestinationNearby)
        {
            CreateIndicatorsForTile(tileObject, hasBombNearby, hasTreasureNearby, hasDestinationNearby);
        }
    }
    
    // Create indicators for a tile (same logic as HexGridManager)
    private void CreateIndicatorsForTile(GameObject tileObject, bool showBomb, bool showTreasure, bool showDestination)
    {
        float indicatorSize = 1f;
        float gap = 0.2f;
        
        int indicatorCount = 0;
        if (showBomb) indicatorCount++;
        if (showTreasure) indicatorCount++;
        if (showDestination) indicatorCount++;
        
        if (indicatorCount == 0) return;
        
        float totalWidth = indicatorCount * indicatorSize + (indicatorCount - 1) * gap;
        float startX = -totalWidth / 2 + indicatorSize / 2;
        
        int currentIndex = 0;
        
        if (showBomb)
        {
            float xPos = startX + currentIndex * (indicatorSize + gap);
            CreateSingleIndicator(tileObject, "bomb_ind", new Vector3(xPos, 0.2f, -0.1f), indicatorSize);
            currentIndex++;
        }
        
        if (showTreasure)
        {
            float xPos = startX + currentIndex * (indicatorSize + gap);
            CreateSingleIndicator(tileObject, "chest_ind", new Vector3(xPos, 0.2f, -0.1f), indicatorSize);
            currentIndex++;
        }
        
        if (showDestination)
        {
            float xPos = startX + currentIndex * (indicatorSize + gap);
            CreateSingleIndicator(tileObject, "exit_ind", new Vector3(xPos, 0.2f, -0.1f), indicatorSize);
            currentIndex++;
        }
    }
    
    // Create a single indicator (same logic as HexGridManager)
    private void CreateSingleIndicator(GameObject parent, string spriteName, Vector3 localPosition, float size)
    {
        Sprite indicatorSprite = Resources.Load<Sprite>(spriteName);
        if (indicatorSprite == null) return;
        
        GameObject indicator = new GameObject($"Indicator_{spriteName}");
        indicator.transform.SetParent(parent.transform);
        indicator.transform.localPosition = localPosition;
        
        SpriteRenderer sr = indicator.AddComponent<SpriteRenderer>();
        sr.sprite = indicatorSprite;
        sr.sortingOrder = 1;
        
        indicator.transform.localScale = Vector3.one * size;
        
        Color color = sr.color;
        color.a = 1f;
        sr.color = color;
    }
}