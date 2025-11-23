using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button ticTacToeButton;
    [SerializeField] private Button snakeButton;
    [SerializeField] private Button memoryButton;

    [SerializeField] private Button quitButton;

    void Start()
    {
        ticTacToeButton.onClick.AddListener(OpenTicTacToe);
        snakeButton.onClick.AddListener(OpenSnake);
        memoryButton.onClick.AddListener(OpenMemory);

        quitButton.onClick.AddListener(QuitGame);
    }


    public void OpenTicTacToe()
    {
        SceneManager.LoadScene("TicTacToe");
    }

    public void OpenSnake()
    {
        SceneManager.LoadScene("Snake");
    }

    public void OpenMemory()
    {
        SceneManager.LoadScene("Memory");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
