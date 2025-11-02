using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{

    public void LoadSelectedScene(string sceneName)
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
    public void LoadSelectedScene(SceneAsset sceneName)
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
}
