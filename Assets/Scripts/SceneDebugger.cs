using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Debug helper to check scene setup
/// </summary>
public class SceneDebugger : MonoBehaviour
{
    private void Start()
    {
        LogSceneInfo();
    }
    
    [ContextMenu("Log Scene Info")]
    public void LogSceneInfo()
    {
        Debug.Log("=== SCENE DEBUG INFO ===");
        Debug.Log($"Current Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        Debug.Log($"Current Scene Index: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex}");
        Debug.Log($"Total Scenes in Build: {UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings}");
        
        Debug.Log("--- Scenes in Build Settings ---");
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Debug.Log($"Index {i}: {sceneName} ({scenePath})");
        }
        Debug.Log("=========================");
    }
}