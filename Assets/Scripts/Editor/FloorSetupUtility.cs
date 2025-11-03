using UnityEngine;
using UnityEditor;

/// <summary>
/// Simple editor utility to quickly set up floor tiling
/// </summary>
public class FloorSetupUtility : EditorWindow
{
    private GameObject floorObject;
    private Material floorMaterial;
    private Vector2 tileScale = new Vector2(10, 10);
    
    [MenuItem("Tools/Floor Setup Utility")]
    public static void ShowWindow()
    {
        GetWindow<FloorSetupUtility>("Floor Setup");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Floor Tiling Setup", EditorStyles.boldLabel);
        
        floorObject = (GameObject)EditorGUILayout.ObjectField("Floor Object", floorObject, typeof(GameObject), true);
        floorMaterial = (Material)EditorGUILayout.ObjectField("Floor Material", floorMaterial, typeof(Material), false);
        tileScale = EditorGUILayout.Vector2Field("Tile Scale", tileScale);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Setup Floor Tiling"))
        {
            SetupFloorTiling();
        }
        
        if (GUILayout.Button("Find Floor Object in Scene"))
        {
            FindFloorObject();
        }
        
        if (GUILayout.Button("Load Floor Tile Material"))
        {
            LoadFloorMaterial();
        }
    }
    
    void SetupFloorTiling()
    {
        if (floorObject == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a floor object!", "OK");
            return;
        }
        
        // Add FloorTileController if it doesn't exist
        FloorTileController controller = floorObject.GetComponent<FloorTileController>();
        if (controller == null)
        {
            controller = floorObject.AddComponent<FloorTileController>();
        }
        
        // Apply material if available
        if (floorMaterial != null)
        {
            Renderer renderer = floorObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = floorMaterial;
                renderer.material.mainTextureScale = tileScale;
            }
        }
        
        // Update the controller settings
        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("tileScale").vector2Value = tileScale;
        if (floorMaterial != null)
        {
            so.FindProperty("floorTileMaterial").objectReferenceValue = floorMaterial;
        }
        so.ApplyModifiedProperties();
        
        controller.SetupFloorTiling();
        
        Debug.Log("Floor tiling setup complete!");
    }
    
    void FindFloorObject()
    {
        // Look for objects with "floor" in the name
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("floor") || obj.name.ToLower().Contains("ground"))
            {
                floorObject = obj;
                Debug.Log("Found floor object: " + obj.name);
                break;
            }
        }
        
        if (floorObject == null)
        {
            Debug.Log("No floor object found. Please assign manually.");
        }
    }
    
    void LoadFloorMaterial()
    {
        // Try to find the floor tile material
        string[] materialGUIDs = AssetDatabase.FindAssets("FloorTileMaterial t:Material");
        if (materialGUIDs.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(materialGUIDs[0]);
            floorMaterial = AssetDatabase.LoadAssetAtPath<Material>(path);
            Debug.Log("Loaded floor material: " + floorMaterial.name);
        }
        else
        {
            Debug.Log("Floor tile material not found. Make sure it exists in the project.");
        }
    }
}