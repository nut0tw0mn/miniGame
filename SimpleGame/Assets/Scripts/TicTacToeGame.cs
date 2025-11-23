using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TicTacToeGame : MonoBehaviour
{
    [SerializeField] private Button[] cells;     
    [SerializeField] private Text statusText;
    [SerializeField] private Button backButton;
    [SerializeField] private Button resetButton;

    private int[] board = new int[9]; // 0 = empty, 1 = X, 2 = O
    private int currentPlayer = 1;
    private bool gameOver = false;

    void Start()
    {

        for (int i = 0; i < cells.Length; i++)
        {
            int index = i; 
            cells[i].onClick.RemoveAllListeners();
            cells[i].onClick.AddListener(() => OnCellClicked(index));
        }

        backButton.onClick.AddListener(BackToMenu);
        resetButton.onClick.AddListener(ResetGame);

        ResetGame();
    }

    public void OnCellClicked(int index)
    {
        Debug.Log("Click cell index = " + index); 

        if (gameOver) return;
        if (index < 0 || index >= board.Length) return;
        if (board[index] != 0) return;

        board[index] = currentPlayer;

        Text cellText = cells[index].GetComponentInChildren<Text>();
        if (cellText == null)
        {
            Debug.LogError("ไม่เจอ UI Text ในปุ่ม index = " + index);
        }
        else
        {
            cellText.text = currentPlayer == 1 ? "X" : "O";
        }
        cells[index].interactable = false;

        int winner = CheckWinner();
        if (winner != 0)
        {
            gameOver = true;
            if (statusText != null)
                statusText.text = "Player " + (winner == 1 ? "X" : "O") + " ชนะ!";
        }
        else if (IsBoardFull())
        {
            gameOver = true;
            if (statusText != null)
                statusText.text = "เสมอจ้า";
        }
        else
        {
            currentPlayer = (currentPlayer == 1) ? 2 : 1;
            if (statusText != null)
                statusText.text = "ตา Player " + (currentPlayer == 1 ? "X" : "O");
        }
    }

    public void ResetGame()
    {
        for (int i = 0; i < board.Length; i++)
        {
            board[i] = 0;
            if (cells != null && i < cells.Length && cells[i] != null)
            {
                Text cellText = cells[i].GetComponentInChildren<Text>();
                if (cellText != null)
                    cellText.text = "";
                cells[i].interactable = true;
            }
        }
        currentPlayer = 1;
        gameOver = false;
        if (statusText != null)
            statusText.text = "ตา Player X";
    }

    int CheckWinner()
    {
        int[,] wins = new int[,]
        {
            {0,1,2},
            {3,4,5},
            {6,7,8},
            {0,3,6},
            {1,4,7},
            {2,5,8},
            {0,4,8},
            {2,4,6}
        };

        for (int i = 0; i < wins.GetLength(0); i++)
        {
            int a = wins[i, 0];
            int b = wins[i, 1];
            int c = wins[i, 2];

            if (board[a] != 0 &&
                board[a] == board[b] &&
                board[b] == board[c])
            {
                return board[a];
            }
        }

        return 0;
    }

    bool IsBoardFull()
    {
        for (int i = 0; i < board.Length; i++)
        {
            if (board[i] == 0) return false;
        }
        return true;
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
