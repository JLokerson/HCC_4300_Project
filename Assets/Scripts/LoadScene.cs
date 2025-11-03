#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{

    public static void LoadSelectedScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("No scene name provided");
        }        
    }
    
#if UNITY_EDITOR
    public static void LoadSelectedScene(SceneAsset sceneName)
    {
        if (sceneName != null)
        {
            LoadSelectedScene(sceneName.name);
        }
        else
        {
            Debug.LogError("No scene asset provided");
        }
    }
#endif
}