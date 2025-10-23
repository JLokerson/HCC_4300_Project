using UnityEngine;

public class MatchCursor : MonoBehaviour
{
    [Tooltip("The fixed Y coordinate for the object's position.")]
    public float fixedY = -1f;
    private void Start()
    {
        Cursor.visible = false;
    }
    

    // Update is called once per frame
    void Update()
    {
        //this is weird code but basically due to how the scene and camera are rotated I couldn't big brain the math to get the cursor position
        // to line up with the world position properly so I just cast a ray from the camera to a plane at y = -1 and set the object position to that
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0, fixedY, 0)); // Plane at y = fixedY

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 worldPosition = ray.GetPoint(distance);
            transform.position = worldPosition;
        }
    }
}
