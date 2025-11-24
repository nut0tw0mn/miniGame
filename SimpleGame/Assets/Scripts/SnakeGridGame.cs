using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SnakeGridGame : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 32;
    public int gridHeight = 32;

    public RectTransform gridParent;        // RectTransform ของ Panel ที่มี GridLayoutGroup
    public GridLayoutGroup gridLayoutGroup;
    public Image cellPrefab;                // Prefab ของ Image หนึ่งช่อง

    [Header("Colors")]
    public Color emptyColor = Color.black;
    public Color snakeColor = Color.green;
    public Color snakeHeadColor = Color.yellow;
    public Color foodColor = Color.red;

    [Header("Game Settings")]
    public float moveInterval = 0.1f;       // ความถี่การขยับ (วินาที)
    public int initialLength = 5;           // ความยาวเริ่มต้นของงู

    [Header("UI")]
    public Text scoreText;
    public Text statusText;

    private Image[,] cells;                 // ช่องในกริด [x, y]
    private List<Vector2Int> snake = new List<Vector2Int>();
    private Vector2Int direction = Vector2Int.right;
    private Vector2Int foodPosition;

    private float moveTimer = 0f;
    private bool isGameOver = false;
    private int score = 0;

    void Start()
    {
        GenerateGrid();
        NewGame();
    }

    void Update()
    {
        if (isGameOver)
        {
            // กด R เพื่อเริ่มใหม่
            if (Input.GetKeyDown(KeyCode.R))
            {
                NewGame();
            }
            return;
        }

        HandleInput();

        moveTimer += Time.deltaTime;
        if (moveTimer >= moveInterval)
        {
            moveTimer = 0f;
            Step();
        }
    }

    // ----------------- Setup -----------------

    void GenerateGrid()
    {
        if (gridParent == null || gridLayoutGroup == null || cellPrefab == null)
        {
            Debug.LogError("ยังไม่เซ็ต gridParent / gridLayoutGroup / cellPrefab ใน Inspector");
            return;
        }

        // ลบลูกเก่าออกก่อน (ถ้ามี)
        for (int i = gridParent.childCount - 1; i >= 0; i--)
        {
            Destroy(gridParent.GetChild(i).gameObject);
        }

        // เซ็ต GridLayout ให้เป็นจำนวนคอลัมน์ตามความกว้าง
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = gridWidth;

        // คำนวณขนาดช่องตามขนาด panel (ถ้าอยาก fix strict)
        Rect r = gridParent.rect;
        float cellWidth = r.width / gridWidth;
        float cellHeight = r.height / gridHeight;
        float size = Mathf.Min(cellWidth, cellHeight);
        gridLayoutGroup.cellSize = new Vector2(size, size);

        cells = new Image[gridWidth, gridHeight];

        // สร้างช่อง
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Image cell = Instantiate(cellPrefab, gridParent);
                cell.color = emptyColor;
                cells[x, y] = cell;
            }
        }
    }

    void NewGame()
    {
        // เคลียร์ข้อมูลงู
        snake.Clear();
        isGameOver = false;
        moveTimer = 0f;
        score = 0;
        UpdateScoreText();

        // ตั้งงูเริ่มกลางกระดาน
        Vector2Int startPos = new Vector2Int(gridWidth / 4, gridHeight / 2); // เอียงไปทางซ้ายหน่อย
        direction = Vector2Int.right;

        for (int i = 0; i < initialLength; i++)
        {
            // หัวอยู่ index 0 ทางขวา, หางอยู่ด้านซ้าย
            snake.Add(new Vector2Int(startPos.x - i, startPos.y));
        }

        SpawnFood();
        RenderBoard();

        if (statusText != null)
            statusText.text = "ใช้ W A S D หรือ ลูกศรบังคับงู\nกด R เพื่อเริ่มใหม่เมื่อ Game Over";
    }

    // ----------------- Input -----------------

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (direction != Vector2Int.down)
                direction = Vector2Int.up;
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (direction != Vector2Int.up)
                direction = Vector2Int.down;
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (direction != Vector2Int.right)
                direction = Vector2Int.left;
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (direction != Vector2Int.left)
                direction = Vector2Int.right;
        }
    }

    // ----------------- Game Step -----------------

    void Step()
    {
        if (snake.Count == 0) return;

        Vector2Int head = snake[0];
        Vector2Int newHead = head + direction;

        // ชนขอบ
        if (newHead.x < 0 || newHead.x >= gridWidth ||
            newHead.y < 0 || newHead.y >= gridHeight)
        {
            GameOver();
            return;
        }

        // ชนตัวเอง
        for (int i = 0; i < snake.Count; i++)
        {
            if (snake[i] == newHead)
            {
                GameOver();
                return;
            }
        }

        // ขยับงู: แทรกหัวใหม่
        snake.Insert(0, newHead);

        // กินอาหาร?
        if (newHead == foodPosition)
        {
            score += 10;
            UpdateScoreText();
            SpawnFood();
        }
        else
        {
            // ไม่ได้กิน ก็ลบหาง
            snake.RemoveAt(snake.Count - 1);
        }

        RenderBoard();
    }

    void GameOver()
    {
        isGameOver = true;
        if (statusText != null)
            statusText.text = "Game Over! กด R เพื่อเริ่มใหม่";
    }

    // ----------------- Rendering -----------------

    void ClearBoard()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (cells[x, y] != null)
                    cells[x, y].color = emptyColor;
            }
        }
    }

    void RenderBoard()
    {
        if (cells == null) return;

        ClearBoard();

        // วาดอาหาร
        if (IsInBounds(foodPosition) && cells[foodPosition.x, foodPosition.y] != null)
        {
            cells[foodPosition.x, foodPosition.y].color = foodColor;
        }

        // วาดงู
        for (int i = 0; i < snake.Count; i++)
        {
            Vector2Int pos = snake[i];
            if (!IsInBounds(pos)) continue;

            Image cell = cells[pos.x, pos.y];
            if (cell == null) continue;

            if (i == 0)
                cell.color = snakeHeadColor;
            else
                cell.color = snakeColor;
        }
    }

    bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridWidth &&
               pos.y >= 0 && pos.y < gridHeight;
    }

    // ----------------- Food -----------------

    void SpawnFood()
    {
        // ป้องกันเคสงูเต็มทั้งบอร์ด (ฮาดีถ้าถึง)
        if (snake.Count >= gridWidth * gridHeight)
        {
            Debug.Log("งูยาวจนเต็มบอร์ดแล้ว ไม่มีที่ให้วางอาหารละ");
            return;
        }

        // random ตำแหน่งที่ไม่ชนงู
        int guard = 0;
        while (true)
        {
            guard++;
            if (guard > 10000)
            {
                Debug.LogWarning("หาไม่เจอที่วางอาหาร (งูน่าจะยาวมาก)");
                break;
            }

            int x = Random.Range(0, gridWidth);
            int y = Random.Range(0, gridHeight);
            Vector2Int p = new Vector2Int(x, y);

            bool onSnake = false;
            for (int i = 0; i < snake.Count; i++)
            {
                if (snake[i] == p)
                {
                    onSnake = true;
                    break;
                }
            }

            if (!onSnake)
            {
                foodPosition = p;
                break;
            }
        }
    }

    // ----------------- UI helpers -----------------

    void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString();
    }

    public void RestartButton()
    {
        NewGame();
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
