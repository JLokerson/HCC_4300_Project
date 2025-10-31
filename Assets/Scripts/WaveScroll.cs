using UnityEngine;
using UnityEngine.UI;

public class WaveScroll : MonoBehaviour
{
    public RawImage waveImage;  // Assign in Inspector
    public float scrollSpeed = 0.2f;
    public bool verticalScroll = false; // toggle this for up/down scrolling

    void Start()
    {
        if (waveImage == null) return;

        // Ensure UV rect is properly sized for tiling
        Rect rect = waveImage.uvRect;
        rect.width = 1.0f;
        rect.height = 1.0f;
        waveImage.uvRect = rect;

        // Debug texture settings
        if (waveImage.texture != null)
        {
            Debug.Log($"Texture Wrap Mode: {waveImage.texture.wrapMode}");
            Debug.Log($"Initial UV Rect: {waveImage.uvRect}");
        }
        else
        {
            Debug.LogWarning("No texture assigned to RawImage!");
        }
    }

    void Update()
    {
        if (waveImage == null) return;

        Rect rect = waveImage.uvRect;

        if (verticalScroll)
            rect.y = (rect.y + scrollSpeed * Time.deltaTime) % 1.0f;  // wrap vertically
        else
            rect.x = (rect.x + scrollSpeed * Time.deltaTime) % 1.0f;  // wrap horizontally

        waveImage.uvRect = rect;
    }
}
