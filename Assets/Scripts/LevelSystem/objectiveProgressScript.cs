using TMPro;
using UnityEngine;

public class objectiveProgressScript : MonoBehaviour
{

    public TMP_Text text;
    public LevelManager levelManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (levelManager == null)
        {
            levelManager = GameObject.FindFirstObjectByType<LevelManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        text.text="Current level: " + LevelManager.currentLevel + " Enemies killed: " + levelManager.currentObjective.enemiesDefeated + "/" + levelManager.currentObjective.enemiesToKill;
    }
}
