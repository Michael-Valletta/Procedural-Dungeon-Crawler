using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public int enemiesKilledThisLevel = 0;
    public float levelStartTime;

    void Start()
    {
        ResetStatsForNewLevel();
    }

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

    public void RegisterEnemy()
    {
        enemiesRemaining++; UpdateUI();
    }

    public void EnemyDied()
    {
        enemiesRemaining--;
        enemiesKilledThisLevel++;
        if (enemiesRemaining < 0) enemiesRemaining = 0;

        UpdateUI();

        GameObject[] enemiesInScene = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemiesRemaining <= 0 || enemiesInScene.Length == 0)
        {
            if (generator == null) generator = FindFirstObjectByType<DungeonGenerator>();

            if (generator != null)
            {
                generator.SpawnExitPortal();
                StartCoroutine(ShowPortalNotification());
            }
            else
            {
                Debug.LogError("Portal failed");
            }
        }
    }

    void UpdateUI()
    {
        if (enemyText != null) enemyText.text = "Enemies Left: " + enemiesRemaining;
    }

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
        if (victoryPanel == null)
        {
            GameObject go = GameObject.Find("VictoryFlash");
            if (go != null) victoryPanel = go;
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("VictoryFlash not found");
        }
        dungeonsCleared++;

        yield return new WaitForSeconds(2.0f);
        victoryPanel.SetActive(false);

        generator.settings.roomCount += 1;
        generator.settings.seed = Random.Range(0, 9999999);

        generator.Generate();
    }

    public void ResetStatsForNewLevel()
    {
        playerDamageTakenThisLevel = 0;
        enemiesKilledThisLevel = 0;
        levelStartTime = Time.time;
        enemiesRemaining = 0;
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
    }

    public void ResetMetrics()
    {
        enemiesRemaining = 0;
        enemiesKilledThisLevel = 0;
        playerDamageTakenThisLevel = 0;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == "VictoryFlash" && obj.scene == scene)
                {
                    victoryPanel = obj;
                    break;
                }
            }
            enemyText = GameObject.Find("EnemyCounterText")?.GetComponent<TextMeshProUGUI>();
            portalNotificationText = GameObject.Find("PortalNotificationText")?.GetComponent<TextMeshProUGUI>();

            generator = FindFirstObjectByType<DungeonGenerator>();

            Debug.Log(victoryPanel != null ? "Victory Panel FOund" : "Victory Panel not Found");
        }
    }
}