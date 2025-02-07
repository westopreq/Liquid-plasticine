using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public GameObject gameOverPanel; // Панель победы/поражения
    public Text titleText; // Заголовок
    public Text messageText; // Текст сообщения

    void Start()
    {
        gameOverPanel.SetActive(false); // Отключаем панель при старте
    }

    public void ShowWinScreen()
    {
        gameOverPanel.SetActive(true);
        titleText.text = "You Win!";
        messageText.text = "Congratulations!";
    }

    public void ShowLoseScreen()
    {
        gameOverPanel.SetActive(true);
        titleText.text = "You Lose!";
        messageText.text = "Try again!";
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
