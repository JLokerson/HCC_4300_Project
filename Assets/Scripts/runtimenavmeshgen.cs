using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class runtimenavmeshgen : MonoBehaviour
{
    [Tooltip("Whether the specified navmesh surface is generated on start")]
    public bool generateOnStart = true;
    [Tooltip("The NavmeshSurface to bake. If left blank, will try to apply to the current object")]
    public NavMeshSurface surface;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {        
        if (generateOnStart)
        {
            Generate();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //call this function to bake the navmesh
    void Generate() {

        //if no surface was specified, try to get one from the current object
        if (surface == null && this.TryGetComponent<NavMeshSurface>(out NavMeshSurface tmp))
        {
            Debug.Log("Using navmesh from the current object");
            surface = tmp;
        }
        else
        {
            Debug.Log("Using navmesh from object " + gameObject.name);
        }

        //now it actually bakes the navmesh if it has the surface
        if (surface != null)
        {
            Debug.Log("Baking navmesh");
            surface.BuildNavMesh();
        }
        else
        {
            Debug.LogWarning("No NavMeshSurface specified to generate");
        }
        
    }
}
