using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject howToPlayWindow;

    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void RestartGame()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.currentLevel = 1;
            LevelManager.Instance.ResetMetrics();
        }
        SceneManager.LoadScene("GameScene");
    }

    public void OpenHowToPlay()
    {
        if (howToPlayWindow != null) howToPlayWindow.SetActive(true);
    }

    public void CloseHowToPlay()
    {
        if (howToPlayWindow != null) howToPlayWindow.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
