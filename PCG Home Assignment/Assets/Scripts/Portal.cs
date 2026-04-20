using UnityEngine;

public class Portal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.CalculateDifficultyScore(); 
                LevelManager.Instance.currentLevel++;             
                LevelManager.Instance.ResetStatsForNewLevel();
                LevelManager.Instance.PortalEntered();
                Destroy(gameObject);
            }
        }
    }
}