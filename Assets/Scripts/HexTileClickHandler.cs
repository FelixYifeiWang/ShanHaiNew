using UnityEngine;

public class HexTileClickHandler : MonoBehaviour
{
    private HexGridManager gridManager;
    private AdventureGameManager gameManager;
    private CameraController cameraController;
    private Vector3 originalScale;
    private bool isHovering = false;
    
    void Start()
    {
        gridManager = FindObjectOfType<HexGridManager>();
        gameManager = FindObjectOfType<AdventureGameManager>();
        cameraController = FindObjectOfType<CameraController>();
        
        // Store original scale for hover effect
        originalScale = transform.localScale;
        
        if (gridManager == null)
        {
            Debug.LogError("HexGridManager not found!");
        }
        
        if (gameManager == null)
        {
            Debug.LogError("AdventureGameManager not found!");
        }
        
        if (cameraController == null)
        {
            Debug.LogError("CameraController not found!");
        }
    }
    
    void OnMouseEnter()
    {
        // Only show hover effect if tile is clickable
        HexTileController tileController = GetComponent<HexTileController>();
        if (tileController != null && gridManager != null)
        {
            int row = tileController.row;
            int col = tileController.column;
            
            if (gridManager.IsClickable(row, col))
            {
                isHovering = true;
                // Enlarge tile by 10%
                transform.localScale = originalScale * 1.1f;
            }
        }
    }
    
    void OnMouseExit()
    {
        if (isHovering)
        {
            isHovering = false;
            // Restore original scale
            transform.localScale = originalScale;
        }
    }
    
    void OnMouseDown()
    {
        if (gridManager == null || gameManager == null) return;
        
        // Check if game is over - disable all clicks
        AdventureGameOverUI gameOverUI = FindObjectOfType<AdventureGameOverUI>();
        if (gameOverUI != null && gameOverUI.IsGameOver())
        {
            return; // Ignore clicks when game is over
        }

        AdventureSuccessUI gameDoneUI = FindObjectOfType<AdventureSuccessUI>();
        if (gameDoneUI != null && gameDoneUI.IsSuccessShown())
        {
            return; // Ignore clicks when game is over
        }

        if (UniversalPauseMenu.Instance != null && 
            UniversalPauseMenu.Instance.IsPauseMenuShowing())
        {
            return; // Ignore clicks when pause menu is open
        }
            
        if (AdventureSpecialResourceInventory.Instance != null && 
            AdventureSpecialResourceInventory.Instance.IsShowing())
        {
            return; // Ignore clicks when adventure inventory is open
        }
        
        // Get this tile's coordinates
        HexTileController tileController = GetComponent<HexTileController>();
        if (tileController == null) return;
        
        int row = tileController.row;
        int col = tileController.column;
        
        // Check if this tile is clickable
        if (!gridManager.IsClickable(row, col))
        {
            Debug.Log($"Tile ({row}, {col}) is not clickable");
            return;
        }
        
        // Use a step
        gameManager.UseStep();
        
        // Reveal the tile
        gridManager.RevealTile(row, col);
        
        // Pan camera to center this tile
        if (cameraController != null)
        {
            cameraController.PanToPosition(transform.position);
        }
        
        // Handle tile effects based on type
        HexTileType tileType = gridManager.GetTileType(row, col);
        HandleTileEffect(row, col, tileType);
        
        Debug.Log($"Clicked tile ({row}, {col}) - Type: {tileType}");
    }
    
    private void HandleTileEffect(int row, int col, HexTileType tileType)
    {
        switch (tileType)
        {
            case HexTileType.Empty:
                // Already handled in RevealTile - becomes revealed_tile
                break;
                
            case HexTileType.Bomb:
                ReplaceTileSprite(row, col, "bomb");
                gameManager.TakeDamage(60);
                Debug.Log($"Hit bomb at ({row}, {col})! Lost 60 HP");
                break;
                
            case HexTileType.Destination:
                ReplaceTileSprite(row, col, "end_tile");
                Debug.Log($"Reached destination at ({row}, {col})!");
            
                // Show success UI using singleton pattern
                AdventureSuccessUI.Instance.ShowSuccess();
                break;
                
            case HexTileType.Treasure:
                ReplaceTileSprite(row, col, "chest");
                int goldReward = Random.Range(1, 4); // 1 to 3 inclusive
                AdventureResourceManager.Instance.FindTreasure("gold", goldReward);
                Debug.Log($"Found treasure at ({row}, {col})!");
                break;
                
            case HexTileType.Trap:
                ReplaceTileSprite(row, col, "trap");
                gameManager.LoseSteps(10);
                Debug.Log($"Hit trap at ({row}, {col})! Lost 10 steps");
                break;
        }
    }
    
    private void ReplaceTileSprite(int row, int col, string spriteName)
    {
        // Find the hex tile at the specified position
        HexTileController[] allTiles = FindObjectsOfType<HexTileController>();
        foreach (HexTileController tile in allTiles)
        {
            if (tile.row == row && tile.column == col)
            {
                // Load sprite from Resources folder
                Sprite newSprite = Resources.Load<Sprite>(spriteName);
                if (newSprite != null)
                {
                    SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.sprite = newSprite;
                        // Set opacity to 100%
                        Color color = sr.color;
                        color.a = 1f;
                        sr.color = color;
                    }
                }
                else
                {
                    Debug.LogError($"{spriteName} sprite not found in Resources folder");
                }
                break;
            }
        }
    }
}