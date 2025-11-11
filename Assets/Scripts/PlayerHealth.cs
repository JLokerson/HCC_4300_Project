using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Health))]
public class PlayerHealth : MonoBehaviour
{
    private Health health;
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // fallback option
    [SerializeField] private int mainMenuBuildIndex = 0;            // fallback by build index

    void Awake()
    {
        health = GetComponent<Health>();
    }

    void OnEnable()
    {
        health.OnDeath += HandleDeath;
        health.OnDamaged += HandleDamaged;
        Debug.Log($"[PlayerHealth] Event handlers subscribed for {gameObject.name}");
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
        Debug.Log("[PlayerHealth] Player died. Triggering Game Over...");

        // unlock cursor so menus work
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Save the final score before triggering game over
        // You might want to get this from a ScoreManager or similar
        PlayerPrefs.SetInt("FinalScore", 0); // Replace with actual score

        // Try to use GameOverManager first
        GameOverManager.TriggerGameOver();
        
        // Note: The old scene loading code is removed since GameOverManager handles everything
    }
}
