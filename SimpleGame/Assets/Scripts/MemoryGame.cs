using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MemoryGame : MonoBehaviour
{
    [SerializeField]private Button[] cards;      
    [SerializeField]private Sprite[] cardImages;  
    [SerializeField] private Sprite cardBack;      
    [SerializeField] private Text statusText;

    [SerializeField] private Button backButton;
    [SerializeField] private Button restartButton;

    private int[] cardIds;       
    private bool[] matched;
    private int firstIndex = -1;
    private int secondIndex = -1;
    private bool canClick = true;
    private int pairsFound = 0;

    void Start()
    {
        
        for (int i = 0; i < cards.Length; i++)
        {
            int index = i; 
            if (cards[i] == null)
            {
                Debug.LogError("Card button at index " + i + " is NULL");
                continue;
            }

            cards[i].onClick.RemoveAllListeners();                    
            cards[i].onClick.AddListener(() => OnCardClicked(index)); 
        }
        backButton.onClick.AddListener(BackToMenu);
        restartButton.onClick.AddListener(RestartGame);

        SetupGame();
    }

    void SetupGame()
    {
        int cardCount = cards.Length;

        if (cardCount % 2 != 0)
        {
            Debug.LogError("จำนวนการ์ดต้องเป็นเลขคู่");
            return;
        }
        if (cardImages.Length * 2 < cardCount)
        {
            Debug.LogError("จำนวนรูปไม่พอ (ต้องการอย่างน้อย cardCount/2 รูป)");
            return;
        }

        cardIds = new int[cardCount];
        matched = new bool[cardCount];

        // สร้างคู่ข้อมูล 0,0,1,1,2,2,...
        int imageIndex = 0;
        for (int i = 0; i < cardCount; i += 2)
        {
            cardIds[i] = imageIndex;
            cardIds[i + 1] = imageIndex;
            imageIndex++;
        }

        // สุ่มไพ่ (Fisher–Yates)
        for (int i = 0; i < cardCount; i++)
        {
            int rand = Random.Range(i, cardCount);
            int tmp = cardIds[i];
            cardIds[i] = cardIds[rand];
            cardIds[rand] = tmp;
        }

        // รีเซ็ต UI
        for (int i = 0; i < cardCount; i++)
        {
            if (cards[i] == null) continue;

            cards[i].interactable = true;
            Image img = cards[i].GetComponent<Image>();
            if (img != null)
                img.sprite = cardBack;
        }

        firstIndex = -1;
        secondIndex = -1;
        pairsFound = 0;
        canClick = true;

        if (statusText != null)
            statusText.text = "หาให้ครบทุกคู่!";

        Debug.Log("MemoryGame.SetupGame done. cardCount = " + cardCount);
    }

    public void OnCardClicked(int index)
    {
        Debug.Log("OnCardClicked index = " + index);

        if (!canClick) return;
        if (index < 0 || index >= cards.Length) return;
        if (matched[index]) return;
        if (index == firstIndex) return;

        ShowCard(index);

        if (firstIndex == -1)
        {
            firstIndex = index;
        }
        else
        {
            secondIndex = index;
            canClick = false;
            StartCoroutine(CheckMatch());
        }
    }

    void ShowCard(int index)
    {
        Image img = cards[index].GetComponent<Image>();
        if (img == null)
        {
            Debug.LogError("ไม่พบ Image บนการ์ด index " + index);
            return;
        }

        int id = cardIds[index];
        if (id < 0 || id >= cardImages.Length)
        {
            Debug.LogError("cardImages out of range: id = " + id);
            return;
        }

        img.sprite = cardImages[id];
    }

    void HideCard(int index)
    {
        Image img = cards[index].GetComponent<Image>();
        if (img != null)
            img.sprite = cardBack;
    }

    IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(0.5f);

        if (cardIds[firstIndex] == cardIds[secondIndex])
        {
            matched[firstIndex] = true;
            matched[secondIndex] = true;

            cards[firstIndex].interactable = false;
            cards[secondIndex].interactable = false;

            pairsFound++;

            if (statusText != null)
                statusText.text = "เจอคู่แล้ว! (" + pairsFound + ")";

            if (pairsFound * 2 >= cards.Length)
            {
                if (statusText != null)
                    statusText.text = "ครบทุกคู่แล้ว เยี่ยม!";
            }
        }
        else
        {
            HideCard(firstIndex);
            HideCard(secondIndex);
        }

        firstIndex = -1;
        secondIndex = -1;
        canClick = true;
    }

    public void RestartGame()
    {
        SetupGame();
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
