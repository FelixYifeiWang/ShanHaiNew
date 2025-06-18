using UnityEngine;

public enum HexTileType
{
    Empty = 0,
    Start = 1,
    Destination = 2,
    Bomb = 3,
    Treasure = 4,
    Trap = 5
}

public class HexGridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int rows = 12;
    [SerializeField] private int cols = 25;
    [SerializeField] private int startRow = 6; // Middle row
    [SerializeField] private int startCol = 0; // Left side
    
    [Header("Special Tile Counts")]
    [SerializeField] private int bombCount = 25;      // Proportionally reduced from 30
    [SerializeField] private int treasureCount = 16;  // Proportionally reduced from 20  
    [SerializeField] private int trapCount = 8;       // Proportionally reduced from 10
    
    // Grid data arrays
    private HexTileType[,] tileTypes;
    private bool[,] revealedStatus;
    private bool[,] clickableStatus;
    
    void Start()
    {
        InitializeGrid();
        GenerateLevel();
    }
    
    private void InitializeGrid()
    {
        tileTypes = new HexTileType[rows, cols];
        revealedStatus = new bool[rows, cols];
        clickableStatus = new bool[rows, cols];
        
        // Initialize all tiles as empty, unrevealed, and not clickable
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                tileTypes[r, c] = HexTileType.Empty;
                revealedStatus[r, c] = false;
                clickableStatus[r, c] = false;
            }
        }
    }
    
    private void GenerateLevel()
    {
        // Place start tile
        tileTypes[startRow, startCol] = HexTileType.Start;
        
        // Reveal start area (1-tile radius) FIRST
        RevealArea(startRow, startCol, 1);
        
        // THEN replace start tile sprite (after reveal status is set)
        ReplaceStartTileSprite();
        
        // Create safe zone based on ACTUALLY revealed tiles (not hex distance)
        bool[,] safeZone = CreateSafeZoneFromRevealed();
        
        // Place 2 destinations in rightmost 5 columns
        PlaceDestinations(safeZone);
        
        // Place hazards and treasures using adjustable parameters
        PlaceRandomTiles(HexTileType.Bomb, bombCount, safeZone);
        PlaceRandomTiles(HexTileType.Treasure, treasureCount, safeZone);
        PlaceRandomTiles(HexTileType.Trap, trapCount, safeZone);
        
        // NOW add indicators to all revealed empty tiles (after all tiles are placed)
        AddIndicatorsToRevealedTiles();
        
        Debug.Log("Level generated successfully");
        Debug.Log($"Start tile ({startRow}, {startCol}) revealed: {revealedStatus[startRow, startCol]}");
    }
    
    private bool[,] CreateSafeZoneFromRevealed()
    {
        bool[,] safeZone = new bool[rows, cols];
        
        // Mark all currently revealed tiles as safe zone
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (revealedStatus[r, c])
                {
                    safeZone[r, c] = true;
                    Debug.Log($"Safe zone: ({r}, {c})");
                }
            }
        }
        
        return safeZone;
    }
    
    private void AddIndicatorsToRevealedTiles()
    {
        // Go through all revealed empty tiles and add indicators
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (revealedStatus[r, c] && tileTypes[r, c] == HexTileType.Empty)
                {
                    // Find the tile object and add indicators
                    HexTileController[] allTiles = FindObjectsOfType<HexTileController>();
                    foreach (HexTileController tile in allTiles)
                    {
                        if (tile.row == r && tile.column == c)
                        {
                            AddWarningIndicators(tile.gameObject, r, c);
                            break;
                        }
                    }
                }
            }
        }
    }
    
    private void ReplaceStartTileSprite()
    {
        // Find the hex tile at start position and replace its sprite
        HexTileController[] allTiles = FindObjectsOfType<HexTileController>();
        foreach (HexTileController tile in allTiles)
        {
            if (tile.row == startRow && tile.column == startCol)
            {
                // Load start_tile sprite from Resources folder
                Sprite startSprite = Resources.Load<Sprite>("start_tile");
                if (startSprite != null)
                {
                    SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.sprite = startSprite;
                        // Set opacity to 100%
                        Color color = sr.color;
                        color.a = 1f;
                        sr.color = color;
                        Debug.Log("Start tile sprite replaced successfully");
                    }
                }
                else
                {
                    Debug.LogError("start_tile sprite not found in Resources folder");
                }
                break;
            }
        }
    }
    
    private void RevealArea(int centerRow, int centerCol, int distance)
    {
        Debug.Log($"RevealArea called: center=({centerRow},{centerCol}), distance={distance}");
        
        // For distance 1, just reveal center + direct neighbors (like JS version)
        if (distance == 1)
        {
            // Reveal center
            Debug.Log($"Revealing center tile ({centerRow},{centerCol})");
            RevealTile(centerRow, centerCol);
            
            // Reveal direct neighbors
            int[] neighborRows, neighborCols;
            GetHexNeighbors(centerRow, centerCol, out neighborRows, out neighborCols);
            
            for (int i = 0; i < neighborRows.Length; i++)
            {
                int nr = neighborRows[i];
                int nc = neighborCols[i];
                
                if (IsValidPosition(nr, nc))
                {
                    Debug.Log($"Revealing neighbor tile ({nr},{nc})");
                    RevealTile(nr, nc);
                }
            }
        }
        else
        {
            // For other distances, use the original hex distance method
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int hexDistance = GetHexDistance(centerRow, centerCol, r, c);
                    if (hexDistance <= distance)
                    {
                        Debug.Log($"Revealing tile ({r},{c}) - distance from start: {hexDistance}");
                        RevealTile(r, c);
                    }
                }
            }
        }
    }
    
    private void MarkSafeZone(int centerRow, int centerCol, int distance, bool[,] safeZone)
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (GetHexDistance(centerRow, centerCol, r, c) <= distance)
                {
                    safeZone[r, c] = true;
                }
            }
        }
    }
    
    private void PlaceDestinations(bool[,] safeZone)
    {
        int destinationsPlaced = 0;
        int attempts = 0;
        
        while (destinationsPlaced < 2 && attempts < 1000)
        {
            int r = Random.Range(0, rows);
            int c = Random.Range(cols - 5, cols); // Rightmost 5 columns
            
            if (tileTypes[r, c] == HexTileType.Empty && !safeZone[r, c])
            {
                tileTypes[r, c] = HexTileType.Destination;
                destinationsPlaced++;
            }
            attempts++;
        }
    }
    
    private void PlaceRandomTiles(HexTileType tileType, int count, bool[,] safeZone)
    {
        int placed = 0;
        int attempts = 0;
        
        while (placed < count && attempts < count * 100)
        {
            int r = Random.Range(0, rows);
            int c = Random.Range(0, cols);
            
            if (tileTypes[r, c] == HexTileType.Empty && !safeZone[r, c])
            {
                tileTypes[r, c] = tileType;
                placed++;
            }
            attempts++;
        }
        
        Debug.Log($"Placed {placed}/{count} {tileType} tiles");
    }
    
    private int GetHexDistance(int r1, int c1, int r2, int c2)
    {
        // Convert hex coordinates to cube coordinates for distance calculation
        int x1 = c1 - (r1 - (r1 & 1)) / 2;
        int z1 = r1;
        int y1 = -x1 - z1;
        
        int x2 = c2 - (r2 - (r2 & 1)) / 2;
        int z2 = r2;
        int y2 = -x2 - z2;
        
        return (Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2) + Mathf.Abs(z1 - z2)) / 2;
    }
    
    // Public methods for accessing grid data
    public HexTileType GetTileType(int row, int col)
    {
        if (IsValidPosition(row, col))
            return tileTypes[row, col];
        return HexTileType.Empty;
    }
    
    public bool IsRevealed(int row, int col)
    {
        if (IsValidPosition(row, col))
            return revealedStatus[row, col];
        return false;
    }
    
    public void RevealTile(int row, int col)
    {
        if (IsValidPosition(row, col))
        {
            revealedStatus[row, col] = true;
            Debug.Log($"Revealed tile ({row}, {col}) - Type: {tileTypes[row, col]}");
            
            // If it's an empty tile, replace sprite and add indicators
            if (tileTypes[row, col] == HexTileType.Empty)
            {
                ReplaceEmptyTileSprite(row, col);
            }
            
            // Update clickable status for adjacent tiles
            UpdateClickableStatus();
        }
    }
    
    private void UpdateClickableStatus()
    {
        // Reset all clickable status
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                clickableStatus[r, c] = false;
            }
        }
        
        // Set tiles as clickable if they are unrevealed and adjacent to revealed tiles
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                // Debug specific problematic tile
                if (r == 5 && c == 1)
                {
                    Debug.Log($"Checking tile (5,1): revealed={revealedStatus[r, c]}, !revealed={!revealedStatus[r, c]}, adjacent={IsAdjacentToRevealed(r, c)}");
                }
                
                // MUST be unrevealed first, then check if adjacent to revealed
                if (!revealedStatus[r, c] && IsAdjacentToRevealed(r, c))
                {
                    clickableStatus[r, c] = true;
                    ReplaceClickableTileSprite(r, c);
                    Debug.Log($"Made tile ({r}, {c}) clickable - unrevealed: {!revealedStatus[r, c]}, adjacent: {IsAdjacentToRevealed(r, c)}");
                }
            }
        }
        
        Debug.Log($"Start tile ({startRow}, {startCol}) - revealed: {revealedStatus[startRow, startCol]}, clickable: {clickableStatus[startRow, startCol]}");
    }
    
    private void ReplaceClickableTileSprite(int row, int col)
    {
        // Find the hex tile at the specified position
        HexTileController[] allTiles = FindObjectsOfType<HexTileController>();
        foreach (HexTileController tile in allTiles)
        {
            if (tile.row == row && tile.column == col)
            {
                // Load clickable_tile sprite from Resources folder
                Sprite clickableSprite = Resources.Load<Sprite>("clickable_tile");
                if (clickableSprite != null)
                {
                    SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.sprite = clickableSprite;
                        // Set opacity to 100%
                        Color color = sr.color;
                        color.a = 1f;
                        sr.color = color;
                    }
                }
                else
                {
                    Debug.LogError("clickable_tile sprite not found in Resources folder");
                }
                break;
            }
        }
    }
    
    private bool IsAdjacentToRevealed(int row, int col)
    {
        // Get all neighbors and check if any are revealed
        int[] neighborRows, neighborCols;
        GetHexNeighbors(row, col, out neighborRows, out neighborCols);
        
        for (int i = 0; i < neighborRows.Length; i++)
        {
            int nr = neighborRows[i];
            int nc = neighborCols[i];
            
            if (IsValidPosition(nr, nc) && revealedStatus[nr, nc])
            {
                return true;
            }
        }
        
        return false;
    }
    
    private void GetHexNeighbors(int row, int col, out int[] neighborRows, out int[] neighborCols)
    {
        // Hex neighbor offsets from the JavaScript code - CORRECTED
        // Different for even and odd columns
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
    
    private void ReplaceEmptyTileSprite(int row, int col)
    {
        // Find the hex tile at the specified position
        HexTileController[] allTiles = FindObjectsOfType<HexTileController>();
        foreach (HexTileController tile in allTiles)
        {
            if (tile.row == row && tile.column == col)
            {
                // Load revealed_tile sprite from Resources folder
                Sprite revealedSprite = Resources.Load<Sprite>("revealed_tile");
                if (revealedSprite != null)
                {
                    SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.sprite = revealedSprite;
                        // Set opacity to 100%
                        Color color = sr.color;
                        color.a = 1f;
                        sr.color = color;
                    }
                }
                else
                {
                    Debug.LogError("revealed_tile sprite not found in Resources folder");
                }
                
                // Check for adjacent special tiles and add indicators
                AddWarningIndicators(tile.gameObject, row, col);
                break;
            }
        }
    }
    
    private void AddWarningIndicators(GameObject tileObject, int row, int col)
    {
        // Get neighbors and check for special tiles
        int[] neighborRows, neighborCols;
        GetHexNeighbors(row, col, out neighborRows, out neighborCols);
        
        bool hasBombNearby = false;
        bool hasTreasureNearby = false;
        bool hasDestinationNearby = false;
        
        Debug.Log($"Checking indicators for tile ({row}, {col}):");
        
        for (int i = 0; i < neighborRows.Length; i++)
        {
            int nr = neighborRows[i];
            int nc = neighborCols[i];
            
            if (IsValidPosition(nr, nc))
            {
                HexTileType neighborType = tileTypes[nr, nc];
                Debug.Log($"  Neighbor ({nr}, {nc}): {neighborType}");
                
                if (neighborType == HexTileType.Bomb)
                {
                    hasBombNearby = true;
                }
                else if (neighborType == HexTileType.Treasure)
                {
                    hasTreasureNearby = true;
                }
                else if (neighborType == HexTileType.Destination)
                {
                    hasDestinationNearby = true;
                }
            }
            else
            {
                Debug.Log($"  Neighbor ({nr}, {nc}): OUT OF BOUNDS");
            }
        }
        
        Debug.Log($"  Result: Bomb nearby={hasBombNearby}, Treasure nearby={hasTreasureNearby}, Destination nearby={hasDestinationNearby}");
        
        // Create indicators if needed
        if (hasBombNearby || hasTreasureNearby || hasDestinationNearby)
        {
            CreateIndicators(tileObject, hasBombNearby, hasTreasureNearby, hasDestinationNearby);
        }
    }
    
    private void CreateIndicators(GameObject tileObject, bool showBomb, bool showTreasure, bool showDestination)
    {
        float indicatorSize = 1f; // Full size
        float gap = 0.2f; // Gap between indicators
        
        // Count how many indicators we need
        int indicatorCount = 0;
        if (showBomb) indicatorCount++;
        if (showTreasure) indicatorCount++;
        if (showDestination) indicatorCount++;
        
        if (indicatorCount == 0) return;
        
        // Calculate positions to center the group
        float totalWidth = indicatorCount * indicatorSize + (indicatorCount - 1) * gap;
        float startX = -totalWidth / 2 + indicatorSize / 2;
        
        int currentIndex = 0;
        
        if (showBomb)
        {
            float xPos = startX + currentIndex * (indicatorSize + gap);
            CreateIndicator(tileObject, "bomb_ind", new Vector3(xPos, 0.2f, -0.1f), indicatorSize);
            currentIndex++;
        }
        
        if (showTreasure)
        {
            float xPos = startX + currentIndex * (indicatorSize + gap);
            CreateIndicator(tileObject, "chest_ind", new Vector3(xPos, 0.2f, -0.1f), indicatorSize);
            currentIndex++;
        }
        
        if (showDestination)
        {
            float xPos = startX + currentIndex * (indicatorSize + gap);
            CreateIndicator(tileObject, "exit_ind", new Vector3(xPos, 0.2f, -0.1f), indicatorSize);
            currentIndex++;
        }
    }
    
    private void CreateIndicator(GameObject parent, string spriteName, Vector3 localPosition, float size)
    {
        // Load indicator sprite
        Sprite indicatorSprite = Resources.Load<Sprite>(spriteName);
        if (indicatorSprite == null)
        {
            Debug.LogError($"{spriteName} sprite not found in Resources folder");
            return;
        }
        
        // Create indicator GameObject
        GameObject indicator = new GameObject($"Indicator_{spriteName}");
        indicator.transform.SetParent(parent.transform);
        indicator.transform.localPosition = localPosition;
        
        // Add SpriteRenderer
        SpriteRenderer sr = indicator.AddComponent<SpriteRenderer>();
        sr.sprite = indicatorSprite;
        sr.sortingOrder = 1; // Above the tile sprite
        
        // Set size
        indicator.transform.localScale = Vector3.one * size;
        
        // Set opacity to 100%
        Color color = sr.color;
        color.a = 1f;
        sr.color = color;
    }
    
    public bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < rows && col >= 0 && col < cols;
    }
    
    public int GetRows()
    {
        return rows;
    }
    
    public int GetCols()
    {
        return cols;
    }
    
    public int GetStartRow()
    {
        return startRow;
    }
    
    public int GetStartCol()
    {
        return startCol;
    }
    
    public bool IsClickable(int row, int col)
    {
        if (IsValidPosition(row, col))
            return clickableStatus[row, col];
        return false;
    }
}