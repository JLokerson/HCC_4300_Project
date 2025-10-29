using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{

    public void LoadSelectedScene(SceneAsset scene)
    {
        if (scene!=null)
        {
            SceneManager.LoadScene(scene.name);
        }
        else
        {
            Debug.LogError("No scene to load");
        }        
    }
}
