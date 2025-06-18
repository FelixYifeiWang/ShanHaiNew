using UnityEngine;
using System.Collections.Generic;

public class AdventureCheatController : MonoBehaviour
{
    private HexGridManager gridManager;
    private Dictionary<KeyCode, bool> keysPressed = new Dictionary<KeyCode, bool>();
    
    void Start()
    {
        gridManager = FindObjectOfType<HexGridManager>();
        
        if (gridManager == null)
        {
            Debug.LogError("HexGridManager not found for cheat controller!");
        }
        
        // Initialize key tracking
        keysPressed[KeyCode.C] = false;
        keysPressed[KeyCode.T] = false;
    }
    
    void Update()
    {
        // Track key states
        keysPressed[KeyCode.C] = Input.GetKey(KeyCode.C);
        keysPressed[KeyCode.T] = Input.GetKey(KeyCode.T);
        
        // Check for C + T combination
        if (keysPressed[KeyCode.C] && keysPressed[KeyCode.T])
        {
            // Only trigger once per key combination (not every frame)
            if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.T))
            {
                ToggleRevealAll();
            }
        }
    }
    
    private void ToggleRevealAll()
    {
        if (gridManager == null) return;
        
        Debug.Log("Cheat activated: Revealing all tiles!");
        
        // Reveal all tiles and apply appropriate sprites
        for (int r = 0; r < gridManager.GetRows(); r++)
        {
            for (int c = 0; c < gridManager.GetCols(); c++)
            {
                if (!gridManager.IsRevealed(r, c))
                {
                    // Reveal the tile
                    gridManager.RevealTile(r, c);
                    
                    // Apply the correct sprite based on tile type
                    HexTileType tileType = gridManager.GetTileType(r, c);
                    ApplyTileSprite(r, c, tileType);
                }
            }
        }
    }
    
    private void ApplyTileSprite(int row, int col, HexTileType tileType)
    {
        string spriteName = "";
        
        switch (tileType)
        {
            case HexTileType.Empty:
                // Empty tiles already handled by RevealTile -> ReplaceEmptyTileSprite
                return;
                
            case HexTileType.Start:
                spriteName = "start_tile";
                break;
                
            case HexTileType.Destination:
                spriteName = "end_tile";
                break;
                
            case HexTileType.Bomb:
                spriteName = "bomb";
                break;
                
            case HexTileType.Treasure:
                spriteName = "chest";
                break;
                
            case HexTileType.Trap:
                spriteName = "trap";
                break;
        }
        
        if (!string.IsNullOrEmpty(spriteName))
        {
            ReplaceTileSprite(row, col, spriteName);
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