using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public DungeonGenerator generator;
    public TextMeshProUGUI enemyText;

    [Header("Victory Visuals")]
    public GameObject victoryPanel; 

    private int enemiesRemaining;
    private int currentLevel = 1;

    void Awake() { Instance = this; }

    public void RegisterEnemy() { enemiesRemaining++; UpdateUI(); }

    public void EnemyDied()
    {
        enemiesRemaining--;
        UpdateUI();
        if (enemiesRemaining <= 0) StartCoroutine(VictorySequence());
    }

    void UpdateUI() { if (enemyText != null) enemyText.text = "Enemies Left: " + enemiesRemaining; }

    IEnumerator VictorySequence()
    {
        victoryPanel.SetActive(true);

        yield return new WaitForSeconds(2.0f);
        victoryPanel.SetActive(false);
        currentLevel++;

        generator.settings.roomCount += 1; 
        generator.settings.seed = Random.Range(0, 9999); 

        generator.Generate();
    }
}