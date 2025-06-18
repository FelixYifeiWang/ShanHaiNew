using UnityEngine;

public class HexGridGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    public int rows = 12;
    public int columns = 25;
    public string spriteName = "clickable_tile";
    public string cornerSpriteName = "border4";
    public string oddColumnTopBorderSprite = "border3";
    public string evenColumnTopBorderSprite = "border1";
    public string sideBorderSprite = "border2";
    
    [Header("Hex Size Control")]
    public float hexSize = 0.1f;
    public float customWidth = 0.12f;
    public float customHeight = 0.12f;
    public bool useCustomSize = true;
    
    [Header("Spacing Control")]
    public float horizontalOverlap = 0.866f;
    public float verticalOverlap = 0.75f;
    
    [Header("Grid Position")]
    public Vector3 gridOffset = Vector3.zero;
    
    void Start()
    {
        GenerateHexGrid();
    }
    
    void GenerateHexGrid()
    {
        // Load sprites
        Sprite hexSprite = Resources.Load<Sprite>(spriteName);
        Sprite cornerSprite = Resources.Load<Sprite>(cornerSpriteName);
        Sprite oddTopBorderSprite = Resources.Load<Sprite>(oddColumnTopBorderSprite);
        Sprite evenTopBorderSprite = Resources.Load<Sprite>(evenColumnTopBorderSprite);
        Sprite sideBorderSpriteLoaded = Resources.Load<Sprite>(sideBorderSprite);
        
        if (hexSprite == null)
        {
            return;
        }
        
        if (cornerSprite == null)
        {
            cornerSprite = hexSprite;
        }
        
        if (oddTopBorderSprite == null)
        {
            oddTopBorderSprite = hexSprite;
        }
        
        if (evenTopBorderSprite == null)
        {
            evenTopBorderSprite = hexSprite;
        }
        
        if (sideBorderSpriteLoaded == null)
        {
            sideBorderSpriteLoaded = hexSprite;
        }
        
        // Calculate hex dimensions
        float hexWidth, hexHeight;
        
        if (useCustomSize)
        {
            hexWidth = customWidth;
            hexHeight = customHeight;
        }
        else
        {
            hexWidth = hexSprite.bounds.size.x * hexSize;
            hexHeight = hexSprite.bounds.size.y * hexSize;
        }
        
        // Calculate spacing
        float horizontalSpacing = hexWidth * horizontalOverlap;
        float verticalSpacing = hexHeight * verticalOverlap;
        
        // Create parent object
        GameObject gridParent = new GameObject("HexGrid");
        gridParent.transform.SetParent(transform);
        
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // Create hex tile
                GameObject hexTile = new GameObject($"HexTile_{row}_{col}");
                hexTile.transform.SetParent(gridParent.transform);
                
                // Add SpriteRenderer
                SpriteRenderer spriteRenderer = hexTile.AddComponent<SpriteRenderer>();
                
                // Set sprite
                spriteRenderer.sprite = GetHexSprite(row, col, hexSprite, cornerSprite, oddTopBorderSprite, evenTopBorderSprite, sideBorderSpriteLoaded);
                
                // Apply flips
                ApplyHexFlips(spriteRenderer, row, col);
                
                // Apply size scaling
                if (useCustomSize)
                {
                    Vector3 scale = new Vector3(
                        customWidth / hexSprite.bounds.size.x,
                        customHeight / hexSprite.bounds.size.y,
                        1f
                    );
                    hexTile.transform.localScale = scale;
                }
                else
                {
                    hexTile.transform.localScale = Vector3.one * hexSize;
                }
                
                // Set opacity
                bool isBorder = IsBorderHex(row, col);
                Color hexColor = spriteRenderer.color;
                hexColor.a = isBorder ? 0.5f : 0f;
                spriteRenderer.color = hexColor;
                
                // Set position
                Vector3 position = CalculateHexPosition(row, col, horizontalSpacing, verticalSpacing);
                position.z = -1f;
                hexTile.transform.localPosition = position;
                
                // Add collider
                // Add collider - use PolygonCollider2D for perfect hex shape
                CircleCollider2D collider = hexTile.AddComponent<CircleCollider2D>();
                collider.radius = Mathf.Max(hexWidth, hexHeight) * 2.5f;                 
                // Add controller
                HexTileController tileController = hexTile.AddComponent<HexTileController>();
                tileController.Initialize(row, col);

                // Add click handler
                hexTile.AddComponent<HexTileClickHandler>();
                
                // Set sorting
                spriteRenderer.sortingLayerName = "Default";
                spriteRenderer.sortingOrder = 0;
            }
        }
        
        // Center grid
        CenterGrid(gridParent, hexWidth, hexHeight);
    }
    
    Sprite GetHexSprite(int row, int col, Sprite defaultSprite, Sprite cornerSprite, Sprite oddTopBorderSprite, Sprite evenTopBorderSprite, Sprite sideBorderSprite)
    {
        if (IsCornerHex(row, col))
        {
            // Bottom corners use border3 instead of border4
            if (row == rows - 1)
            {
                return oddTopBorderSprite; // border3 for bottom corners
            }
            return cornerSprite; // border4 for top corners
        }
        
        if (row == 0 && col > 0 && col < columns - 1)
        {
            return (col % 2 == 1) ? evenTopBorderSprite : oddTopBorderSprite;
        }
        
        if (row == rows - 1 && col > 0 && col < columns - 1)
        {
            return (col % 2 == 1) ? oddTopBorderSprite : evenTopBorderSprite;
        }
        
        // Left border (excluding corners)
        if (col == 0 && row > 0 && row < rows - 1)
        {
            return sideBorderSprite;
        }
        
        // Right border (excluding corners)
        if (col == columns - 1 && row > 0 && row < rows - 1)
        {
            return sideBorderSprite;
        }
        
        return defaultSprite;
    }
    
    void ApplyHexFlips(SpriteRenderer spriteRenderer, int row, int col)
    {
        if (IsCornerHex(row, col))
        {
            ApplyCornerFlip(spriteRenderer, row, col);
            return;
        }
        
        if (row == 0 && col > 0 && col < columns - 1)
        {
            spriteRenderer.flipX = false;
            spriteRenderer.flipY = false;
            return;
        }
        
        if (row == rows - 1 && col > 0 && col < columns - 1)
        {
            spriteRenderer.flipX = false;
            spriteRenderer.flipY = true;
            return;
        }
        
        if (col == 0 && row > 0 && row < rows - 1)
        {
            spriteRenderer.flipX = false;
            spriteRenderer.flipY = false;
            return;
        }
        
        if (col == columns - 1 && row > 0 && row < rows - 1)
        {
            spriteRenderer.flipX = true;
            spriteRenderer.flipY = false;
            return;
        }
        
        spriteRenderer.flipX = false;
        spriteRenderer.flipY = false;
    }
    
    bool IsCornerHex(int row, int col)
    {
        bool isTopLeft = (row == 0 && col == 0);
        bool isTopRight = (row == 0 && col == columns - 1);
        bool isBottomLeft = (row == rows - 1 && col == 0);
        bool isBottomRight = (row == rows - 1 && col == columns - 1);
        
        return isTopLeft || isTopRight || isBottomLeft || isBottomRight;
    }
    
    void ApplyCornerFlip(SpriteRenderer spriteRenderer, int row, int col)
    {
        bool isTop = (row == 0);
        bool isLeft = (col == 0);
        
        if (isTop && isLeft)
        {
            // Top-left: border4, no rotation
            spriteRenderer.flipX = false;
            spriteRenderer.flipY = false;
            spriteRenderer.transform.rotation = Quaternion.identity;
        }
        else if (isTop && !isLeft)
        {
            // Top-right: border4, horizontal flip
            spriteRenderer.flipX = true;
            spriteRenderer.flipY = false;
            spriteRenderer.transform.rotation = Quaternion.identity;
        }
        else if (!isTop && isLeft)
        {
            // Bottom-left: border3, 120° counter-clockwise (240° clockwise)
            spriteRenderer.flipX = false;
            spriteRenderer.flipY = false;
            spriteRenderer.transform.rotation = Quaternion.Euler(0, 0, 120f);
        }
        else
        {
            // Bottom-right: border3, 120° clockwise
            spriteRenderer.flipX = false;
            spriteRenderer.flipY = false;
            spriteRenderer.transform.rotation = Quaternion.Euler(0, 0, -120f);
        }
    }
    
    bool IsBorderHex(int row, int col)
    {
        bool isTopBorder = (row == 0);
        bool isBottomBorder = (row == rows - 1);
        bool isLeftBorder = (col == 0);
        bool isRightBorder = (col == columns - 1);
        
        return isTopBorder || isBottomBorder || isLeftBorder || isRightBorder;
    }
    
    Vector3 CalculateHexPosition(int row, int col, float horizontalSpacing, float verticalSpacing)
    {
        float x = col * horizontalSpacing;
        float y = row * verticalSpacing;
        
        if (col % 2 == 1)
        {
            y += verticalSpacing * 0.5f;
        }
        
        y = -y;
        
        return new Vector3(x, y, 0);
    }
    
    void CenterGrid(GameObject gridParent, float hexWidth, float hexHeight)
    {
        float totalWidth = (columns - 1) * hexWidth * horizontalOverlap;
        float totalHeight = (rows - 1) * hexHeight * verticalOverlap;
        
        Vector3 centerOffset = new Vector3(-totalWidth * 0.5f, totalHeight * 0.5f, 0);
        gridParent.transform.localPosition = centerOffset + gridOffset;
    }
}

public class HexTileController : MonoBehaviour
{
    public int row; // Changed from private to public
    public int column; // Changed from private to public
    private bool isSelected = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    public void Initialize(int r, int c)
    {
        row = r;
        column = c;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }
}