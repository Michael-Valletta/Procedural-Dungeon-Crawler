using UnityEngine;
using TMPro;

public class UIScript : MonoBehaviour
{
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI enemyCountText;
    public TextMeshProUGUI scoreText; 

    private PlayerScript player;

    void Update()
    {
        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.GetComponent<PlayerScript>();
        }
        else
        {
            healthText.text = $"HP: {player.currentHealth} / {player.maxHealth}";
        }

        int count = GameObject.FindGameObjectsWithTag("Enemy").Length;
        enemyCountText.text = $"Enemies Left: {count}";

        if (LevelManager.Instance != null && scoreText != null)
        {
            scoreText.text = $"Dungeons Cleared: {LevelManager.Instance.dungeonsCleared}";
        }
    }
}