using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public DungeonGenerator generator;
    public TextMeshProUGUI enemyText;
    public TextMeshProUGUI portalNotificationText;

    [Header("Victory Visuals")]
    public GameObject victoryPanel;

    private int enemiesRemaining;
    public int currentLevel = 1;
    public int dungeonsCleared = 0;

    public float difficultyScore = 1.0f; 
    public int playerDamageTakenThisLevel = 0;
    public float levelStartTime;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterEnemy() { enemiesRemaining++; UpdateUI(); }

    public void EnemyDied()
    {
        enemiesRemaining--;
        if (enemiesRemaining < 0) enemiesRemaining = 0;

        UpdateUI();
        bool noEnemiesLeft = (enemiesRemaining <= 0) || (GameObject.FindGameObjectsWithTag("Enemy").Length == 0);

        if (noEnemiesLeft)
        {
            Debug.Log("LEVEL CLEAR: Triggering Portal Spawn.");
            if (generator != null)
            {
                generator.SpawnExitPortal();
            }
            StartCoroutine(ShowPortalNotification());
        }
    }

    void UpdateUI() { if (enemyText != null) enemyText.text = "Enemies Left: " + enemiesRemaining; }

    IEnumerator ShowPortalNotification()
    {
        if (portalNotificationText != null)
        {
            portalNotificationText.gameObject.SetActive(true); 
            portalNotificationText.text = "A Portal has appeared!";
            yield return new WaitForSeconds(5f);
            portalNotificationText.gameObject.SetActive(false);
        }
    }

    public void PortalEntered()
    {
        StartCoroutine(VictorySequence());
    }

    IEnumerator VictorySequence()
    {
        victoryPanel.SetActive(true);
        dungeonsCleared++;

        yield return new WaitForSeconds(2.0f);
        victoryPanel.SetActive(false);

        generator.settings.roomCount += 1;
        generator.settings.seed = Random.Range(0, 9999);

        generator.Generate();
    }

    public void ResetStatsForNewLevel()
    {
        playerDamageTakenThisLevel = 0;
        levelStartTime = Time.time;
    }

    public void CalculateDifficultyScore()
    {
        float timeTaken = Time.time - levelStartTime;

        if (playerDamageTakenThisLevel < 20 && timeTaken < 60f)
        {
            difficultyScore += 0.2f;
        }
        else if (playerDamageTakenThisLevel > 50)
        {
            difficultyScore -= 0.2f;
        }

        difficultyScore = Mathf.Clamp(difficultyScore, 0.5f, 3.0f);
        Debug.Log("New Difficulty Score: " + difficultyScore);
    }
}