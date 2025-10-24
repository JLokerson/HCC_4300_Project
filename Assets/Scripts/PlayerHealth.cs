using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Health))]
public class PlayerHealth : MonoBehaviour
{
    private Health health;
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // change if your title has a different name
    [SerializeField] private int mainMenuBuildIndex = 0;            // fallback by build index

    void Awake()
    {
        health = GetComponent<Health>();
    }

    void OnEnable()
    {
        health.OnDeath += HandleDeath;
        health.OnDamaged += HandleDamaged;
    }

    void OnDisable()
    {
        health.OnDeath -= HandleDeath;
        health.OnDamaged -= HandleDamaged;
    }

    void HandleDamaged(float amount)
    {
        // Optional VFX/SFX hook
        Debug.Log($"[PlayerHealth] Took {amount} damage. {health.currentHealth}/{health.maxHealth}");
    }

    void HandleDeath()
    {
        Debug.Log("[PlayerHealth] Player died. Loading title screen...");

        // unlock cursor so menus work
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Use GameManager if present; otherwise try by scene name; finally by build index
        if (TryLoadViaGameManager()) return;
        if (TryLoadByName(mainMenuSceneName)) return;

        Debug.LogWarning($"[PlayerHealth] Could not load '{mainMenuSceneName}'. Falling back to build index {mainMenuBuildIndex}.");
        SceneManager.LoadScene(mainMenuBuildIndex);
    }

    bool TryLoadViaGameManager()
    {
        // Safe call: only if an instance is alive
        var gmType = System.Type.GetType("GameManager");
        // If you have a GameManager Singleton with static Instance, use it:
        // Replace the below reflection block with: if (GameManager.Instance != null) { GameManager.Instance.SetGameState(GameManager.GameState.GameOver); GameManager.Instance.LoadMainMenu(); return true; }
        // Using reflection here avoids compile errors if your GameManager script name/namespace changes.
        try
        {
            var prop = gmType?.GetProperty("Instance");
            var inst = prop?.GetValue(null, null);
            if (inst != null)
            {
                var setState = gmType.GetMethod("SetGameState");
                var loadMain = gmType.GetMethod("LoadMainMenu");
                var enumType = gmType.GetNestedType("GameState");
                var gameOverValue = System.Enum.Parse(enumType, "GameOver");

                setState?.Invoke(inst, new object[] { gameOverValue });
                loadMain?.Invoke(inst, null);
                return true;
            }
        }
        catch { /* ignore and fall through */ }
        return false;
    }

    bool TryLoadByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;

        // verify it exists in build settings before trying
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
            {
                SceneManager.LoadScene(sceneName);
                return true;
            }
        }
        return false;
    }
}
