using UnityEngine;

/// <summary>
/// Simple script to apply tiled floor material and adjust tiling parameters
/// </summary>
public class FloorTileController : MonoBehaviour
{
    [Header("Floor Tile Settings")]
    [SerializeField] private Material floorTileMaterial;
    [SerializeField] private Vector2 tileScale = new Vector2(10, 10);
    [SerializeField] private Vector2 tileOffset = Vector2.zero;
    
    private Renderer floorRenderer;
    
    void Start()
    {
        SetupFloorTiling();
    }
    
    void OnValidate()
    {
        // Update tiling in real-time when values change in inspector
        if (Application.isPlaying)
        {
            UpdateTiling();
        }
    }
    
    [ContextMenu("Setup Floor Tiling")]
    public void SetupFloorTiling()
    {
        // Get the renderer component
        floorRenderer = GetComponent<Renderer>();
        if (floorRenderer == null)
        {
            Debug.LogError("No Renderer component found on " + gameObject.name);
            return;
        }
        
        // Apply the floor tile material if assigned
        if (floorTileMaterial != null)
        {
            floorRenderer.material = floorTileMaterial;
            Debug.Log("Floor tile material applied to " + gameObject.name);
        }
        else
        {
            Debug.LogWarning("No floor tile material assigned!");
        }
        
        UpdateTiling();
    }
    
    [ContextMenu("Update Tiling")]
    public void UpdateTiling()
    {
        if (floorRenderer != null && floorRenderer.material != null)
        {
            // Update the tiling and offset on the main texture
            floorRenderer.material.mainTextureScale = tileScale;
            floorRenderer.material.mainTextureOffset = tileOffset;
            
            Debug.Log($"Floor tiling updated - Scale: {tileScale}, Offset: {tileOffset}");
        }
    }
    
    /// <summary>
    /// Set tiling scale programmatically
    /// </summary>
    public void SetTileScale(Vector2 scale)
    {
        tileScale = scale;
        UpdateTiling();
    }
    
    /// <summary>
    /// Set tiling offset programmatically
    /// </summary>
    public void SetTileOffset(Vector2 offset)
    {
        tileOffset = offset;
        UpdateTiling();
    }
    
    /// <summary>
    /// Calculate optimal tiling based on world size
    /// </summary>
    [ContextMenu("Auto Calculate Tiling")]
    public void AutoCalculateTiling()
    {
        if (floorRenderer == null) return;
        
        // Get the world size of the floor object
        Vector3 worldSize = floorRenderer.bounds.size;
        
        // Calculate tiling based on desired tile size (you can adjust this)
        float desiredTileSize = 1.0f; // 1 unit per tile
        
        tileScale.x = worldSize.x / desiredTileSize;
        tileScale.y = worldSize.z / desiredTileSize; // Use Z for the other horizontal axis
        
        UpdateTiling();
        
        Debug.Log($"Auto-calculated tiling: {tileScale} based on world size: {worldSize}");
    }
}