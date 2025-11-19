using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{

    public static void LoadSelectedScene(string sceneName, bool additive)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            if (additive)
            {
                Debug.Log("Loading scene additively: " + sceneName);
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            }
            else
            {
                Debug.Log("Loading scene: " + sceneName);
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }    
        }
        else
        {
            Debug.LogError("No scene name provided");
        }        
    }

    // Inspector-friendly overload: non-additive (shows up in UnityEvent lists because apparently that only lets you have one param)
    public static void LoadSelectedScene(string sceneName)
    {
        LoadSelectedScene(sceneName, false);
    }

    // Inspector-friendly helper: additive (shows up in UnityEvent lists because apparently that only lets you have one param)
    public static void LoadSelectedSceneAdditive(string sceneName)
    {
        LoadSelectedScene(sceneName, true);
    }

    public static void UnloadSelectedScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            Debug.Log("Unloading scene: " + sceneName);
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
        }
        else
        {
            Debug.LogError("No scene name provided");
        }
    }

    public static void setActiveScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            Scene sceneToSet = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
            if (sceneToSet.IsValid())
            {
                Debug.Log("Setting active scene to: " + sceneName);
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(sceneToSet);
            }
            else
            {
                Debug.LogError("Scene not valid or not loaded: " + sceneName);
            }
        }
        else
        {
            Debug.LogError("No scene name provided");
        }
    }
}