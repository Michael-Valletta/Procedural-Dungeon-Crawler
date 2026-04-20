using UnityEngine;
using TMPro;

public class GameOverScript : MonoBehaviour
{
    public TextMeshProUGUI finalScoreText;

    void Start() 
    {
        if (LevelManager.Instance != null)
        {
            finalScoreText.text = "Num of Dungeons Cleared: " + LevelManager.Instance.dungeonsCleared;
        }
        else
        {
            finalScoreText.text = "Num of Dungeons Cleared: 0";
        }
    }
}