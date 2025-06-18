using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float closeLimit = 1f;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Auto-Pan Settings")]
    [SerializeField] private float panSpeed = 2f;
    [SerializeField] private bool enableAutoPan = true;
    
    [Header("Intro Animation")]
    [SerializeField] private float introAnimationDuration = 3f;
    [SerializeField] private Vector2 introTargetPosition = new Vector2(-0.9f, -0.265f);
    [SerializeField] private float introTargetZoom = 2f;
    
    private Camera cam;
    private float farLimit;
    private bool isOrthographic;
    private Bounds backgroundBounds;
    private Vector3 targetPosition;
    private bool isPanning = false;
    private bool isPlayingIntro = false;
    private bool introStarted = false; // NEW: Track if intro has been triggered
    private float introStartTime;
    private Vector3 introStartPosition;
    private float introStartZoom;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }
        
        if (cam == null)
        {
            Debug.LogError("No camera found for camera controller!");
            return;
        }
        
        // Store current camera settings as far limit
        isOrthographic = cam.orthographic;
        
        if (isOrthographic)
        {
            farLimit = cam.orthographicSize;
        }
        else
        {
            farLimit = Vector3.Distance(transform.position, Vector3.zero);
        }
        
        // Auto-detect background bounds
        CalculateBackgroundBounds();
        
        // Set camera position to background center at start (since we start fully zoomed out)
        Vector3 startPosition = new Vector3(backgroundBounds.center.x, backgroundBounds.center.y, transform.position.z);
        transform.position = startPosition;
        targetPosition = startPosition;
        
        // IMPORTANT: Don't start intro animation - wait for external trigger
        // Camera stays at background center, fully zoomed out until cover fades
        
        Debug.Log($"Camera controller initialized - Far limit: {farLimit}, Close limit: {closeLimit}");
        Debug.Log($"Background bounds: {backgroundBounds}");
        Debug.Log($"Camera positioned at background center: {startPosition}");
        Debug.Log("Camera waiting for intro trigger...");
    }
    
    void Update()
    {
        if (cam == null) return;
        
        // Handle intro animation first (only if it has been started)
        if (isPlayingIntro && introStarted)
        {
            HandleIntroAnimation();
            return; // Skip other input during intro
        }
        
        // Only allow camera controls if intro hasn't started or has completed
        if (!introStarted || !isPlayingIntro)
        {
            // Handle zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                ZoomCamera(scroll);
            }
            
            // Handle movement (only if not auto-panning)
            if (!isPanning)
            {
                HandleMovement();
            }
            
            // Handle auto-panning
            if (isPanning)
            {
                HandleAutoPan();
            }
        }
    }
    
    private void CalculateBackgroundBounds()
    {
        // Method 1: Try to find scroll1 GameObject
        GameObject scroll1 = GameObject.Find("scroll1");
        
        if (scroll1 != null)
        {
            // Try to get bounds from various components
            Renderer renderer = scroll1.GetComponent<Renderer>();
            if (renderer != null)
            {
                backgroundBounds = renderer.bounds;
                Debug.Log($"Found background bounds from Renderer: {backgroundBounds}");
                Debug.Log($"Background size: {backgroundBounds.size}, Center: {backgroundBounds.center}");
                return;
            }
            
            // Try SpriteRenderer
            SpriteRenderer spriteRenderer = scroll1.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                backgroundBounds = spriteRenderer.bounds;
                Debug.Log($"Found background bounds from SpriteRenderer: {backgroundBounds}");
                Debug.Log($"Background size: {backgroundBounds.size}, Center: {backgroundBounds.center}");
                Debug.Log($"Sprite size: {spriteRenderer.sprite.bounds.size}, Transform scale: {scroll1.transform.localScale}");
                return;
            }
            
            // Try Collider
            Collider2D collider2D = scroll1.GetComponent<Collider2D>();
            if (collider2D != null)
            {
                backgroundBounds = collider2D.bounds;
                Debug.Log($"Found background bounds from Collider2D: {backgroundBounds}");
                Debug.Log($"Background size: {backgroundBounds.size}, Center: {backgroundBounds.center}");
                return;
            }
        }
        
        // Method 2: Try to find HexGrid and calculate bounds from that
        GameObject hexGrid = GameObject.Find("HexGrid");
        if (hexGrid != null)
        {
            Bounds hexBounds = CalculateChildrenBounds(hexGrid);
            if (hexBounds.size != Vector3.zero)
            {
                // Expand hex bounds slightly to include some margin
                hexBounds.Expand(2f);
                backgroundBounds = hexBounds;
                Debug.Log($"Calculated background bounds from HexGrid: {backgroundBounds}");
                return;
            }
        }
        
        // Method 3: Try to find any GameObject with "scroll" in the name
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("scroll") || obj.name.ToLower().Contains("background"))
            {
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    backgroundBounds = sr.bounds;
                    Debug.Log($"Found background bounds from {obj.name}: {backgroundBounds}");
                    return;
                }
            }
        }
        
        // Fallback: use generous default bounds
        Debug.LogWarning("No background GameObject found. Using generous default bounds.");
        backgroundBounds = new Bounds(Vector3.zero, new Vector3(30f, 20f, 1f));
    }
    
    private Bounds CalculateChildrenBounds(GameObject parent)
    {
        Bounds bounds = new Bounds();
        bool hasBounds = false;
        
        Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }
        
        return bounds;
    }
    
    private void HandleMovement()
    {
        Vector3 moveDirection = Vector3.zero;
        
        // WASD movement
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveDirection.y += 1f;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveDirection.y -= 1f;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveDirection.x -= 1f;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveDirection.x += 1f;
        }
        
        // Apply movement with speed and time
        if (moveDirection != Vector3.zero)
        {
            Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
            Vector3 newPosition = transform.position + movement;
            
            // Calculate current zoom ratio and auto-limits
            float zoomRatio = GetZoomRatio();
            Vector4 autoLimits = CalculateAutoLimits(zoomRatio);
            
            // Clamp position to auto-calculated limits
            newPosition.x = Mathf.Clamp(newPosition.x, autoLimits.x, autoLimits.y); // left, right
            newPosition.y = Mathf.Clamp(newPosition.y, autoLimits.z, autoLimits.w); // bottom, top
            
            transform.position = newPosition;
        }
    }
    
    private Vector4 CalculateAutoLimits(float zoomRatio)
    {
        // Calculate what the movement limits should be at MAXIMUM zoom in (close limit)
        float maxZoomCameraHalfWidth, maxZoomCameraHalfHeight;
        
        if (isOrthographic)
        {
            maxZoomCameraHalfHeight = closeLimit; // Camera size at max zoom
            maxZoomCameraHalfWidth = maxZoomCameraHalfHeight * cam.aspect;
        }
        else
        {
            // For perspective camera at max zoom
            float distance = closeLimit;
            maxZoomCameraHalfHeight = distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            maxZoomCameraHalfWidth = maxZoomCameraHalfHeight * cam.aspect;
        }
        
        // Calculate maximum possible movement range (when fully zoomed in)
        float backgroundHalfWidth = backgroundBounds.size.x * 0.5f;
        float backgroundHalfHeight = backgroundBounds.size.y * 0.5f;
        
        // Maximum movement from center when fully zoomed in
        float maxPossibleMovementX = Mathf.Max(0f, backgroundHalfWidth - maxZoomCameraHalfWidth);
        float maxPossibleMovementY = Mathf.Max(0f, backgroundHalfHeight - maxZoomCameraHalfHeight);
        
        // Account for background center offset
        Vector3 backgroundCenter = backgroundBounds.center;
        
        // Scale the movement limits by zoom ratio
        // At zoom ratio 0: no movement
        // At zoom ratio 1: full movement range based on max zoom camera size
        float currentMaxMovementX = maxPossibleMovementX * zoomRatio;
        float currentMaxMovementY = maxPossibleMovementY * zoomRatio;
        
        float leftLimit = backgroundCenter.x - currentMaxMovementX;
        float rightLimit = backgroundCenter.x + currentMaxMovementX;
        float bottomLimit = backgroundCenter.y - currentMaxMovementY;
        float topLimit = backgroundCenter.y + currentMaxMovementY;
        
        return new Vector4(leftLimit, rightLimit, bottomLimit, topLimit);
    }
    
    private float GetZoomRatio()
    {
        float currentZoom = GetCurrentZoomLevel();
        // Invert the ratio: close limit = 1.0, far limit = 0.0
        float ratio = (farLimit - currentZoom) / (farLimit - closeLimit);
        return Mathf.Clamp01(ratio);
    }
    
    private void ZoomCamera(float scrollDelta)
    {
        if (isOrthographic)
        {
            // Orthographic camera - adjust orthographicSize around background center
            float newSize = cam.orthographicSize - scrollDelta * zoomSpeed;
            newSize = Mathf.Clamp(newSize, closeLimit, farLimit);
            
            // Calculate zoom factor change
            float zoomFactor = newSize / cam.orthographicSize;
            
            // Get world position of background center
            Vector3 backgroundCenter = backgroundBounds.center;
            
            // Calculate camera position offset from background center
            Vector3 offsetFromCenter = transform.position - backgroundCenter;
            
            // Scale the offset by zoom factor to maintain centering on background
            Vector3 newOffset = offsetFromCenter * zoomFactor;
            
            // Apply new size and position
            cam.orthographicSize = newSize;
            transform.position = backgroundCenter + newOffset;
        }
        else
        {
            // Perspective camera - move camera position toward/away from background center
            Vector3 backgroundCenter = backgroundBounds.center;
            Vector3 directionToCamera = (transform.position - backgroundCenter).normalized;
            
            float currentDistance = Vector3.Distance(transform.position, backgroundCenter);
            float newDistance = currentDistance - scrollDelta * zoomSpeed;
            newDistance = Mathf.Clamp(newDistance, closeLimit, farLimit);
            
            transform.position = backgroundCenter + directionToCamera * newDistance;
        }
    }
    
    // Public methods for runtime adjustment
    public void SetZoomSpeed(float speed)
    {
        zoomSpeed = speed;
    }
    
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
    
    public void SetCloseLimitDistance(float limit)
    {
        closeLimit = limit;
    }
    
    public void SetFarLimitDistance(float limit)
    {
        farLimit = limit;
    }
    
    public Bounds GetBackgroundBounds()
    {
        return backgroundBounds;
    }
    
    public float GetCurrentZoomLevel()
    {
        if (isOrthographic)
        {
            return cam.orthographicSize;
        }
        else
        {
            return transform.position.magnitude;
        }
    }
    
    public void PanToPosition(Vector3 worldPosition)
    {
        if (!enableAutoPan) return;
        
        // Set target position (keep current Z)
        targetPosition = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);
        
        // Clamp target position to current movement limits
        float zoomRatio = GetZoomRatio();
        Vector4 autoLimits = CalculateAutoLimits(zoomRatio);
        
        targetPosition.x = Mathf.Clamp(targetPosition.x, autoLimits.x, autoLimits.y);
        targetPosition.y = Mathf.Clamp(targetPosition.y, autoLimits.z, autoLimits.w);
        
        // Start panning if target is different from current position
        if (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            isPanning = true;
        }
    }
    
    private void HandleAutoPan()
    {
        // Smoothly move toward target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, panSpeed * Time.deltaTime);
        
        // Stop panning when close enough
        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            transform.position = targetPosition;
            isPanning = false;
        }
    }
    
    public void SetAutoPanEnabled(bool enabled)
    {
        enableAutoPan = enabled;
        if (!enabled)
        {
            isPanning = false;
        }
    }
    
    // PUBLIC: Called by CoverFadeController after cover fades out
    public void StartIntroAnimation()
    {
        if (introStarted) return; // Prevent multiple calls
        
        introStarted = true;
        isPlayingIntro = true;
        introStartTime = Time.time;
        introStartPosition = transform.position;
        
        if (isOrthographic)
        {
            introStartZoom = cam.orthographicSize;
        }
        else
        {
            introStartZoom = Vector3.Distance(transform.position, backgroundBounds.center);
        }
        
        Debug.Log($"Starting intro animation from position {introStartPosition} and zoom {introStartZoom}");
    }
    
    private void HandleIntroAnimation()
    {
        float elapsed = Time.time - introStartTime;
        float progress = Mathf.Clamp01(elapsed / introAnimationDuration);
        
        // Use smooth ease-in-out curve
        float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
        
        // Animate position
        Vector3 targetPos = new Vector3(introTargetPosition.x, introTargetPosition.y, transform.position.z);
        transform.position = Vector3.Lerp(introStartPosition, targetPos, smoothProgress);
        
        // Animate zoom
        if (isOrthographic)
        {
            float currentZoom = Mathf.Lerp(introStartZoom, introTargetZoom, smoothProgress);
            cam.orthographicSize = currentZoom;
        }
        else
        {
            Vector3 targetDirection = (targetPos - backgroundBounds.center).normalized;
            float targetDistance = Mathf.Lerp(introStartZoom, introTargetZoom, smoothProgress);
            transform.position = backgroundBounds.center + targetDirection * targetDistance;
        }
        
        // Check if animation is complete
        if (progress >= 1f)
        {
            isPlayingIntro = false;
            Debug.Log("Intro animation completed - camera controls now enabled");
        }
    }
}