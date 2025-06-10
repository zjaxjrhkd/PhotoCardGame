using UnityEngine;
using System.Collections.Generic;

public class GameMaster : MonoBehaviour
{
    public enum GameState { Draw, Select, Shop, ShopEnd, End }
    private GameState gameState;

    private CardManager cardManager;
    private BuffManager buffManager;
    private ScoreManager scoreManager;
    private StageManager stageManager;
    public UIManager uiManager;
    private ShopManager shopManager;
    public CardSpawner cardSpawner;

    private int currentSetCount;
    private int currentDropCount;
    private int maxSetCount = 3;
    private int maxDropCount = 99;
    private string stageMessage;
    private int drawCount;

    private bool isFirstDraw = true; // ���� ���� �� ù ��ο츸 10��

    public int coin;
    public int winCoin=3;
    private int targetScore = 2000;


    void Awake()
    {
        cardManager = GetComponent<CardManager>();
        buffManager = GetComponent<BuffManager>();
        scoreManager = GetComponent<ScoreManager>();
        stageManager = GetComponent<StageManager>();
        uiManager = GetComponent<UIManager>();
        shopManager = GetComponent<ShopManager>();
    }

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        switch (gameState)
        {
            case GameState.Draw:
                HandleDraw();
                break;
            case GameState.Select:
                cardManager.ClickCard();
                break;
            case GameState.Shop:
                HandleShop();
                break;
            case GameState.ShopEnd:
                StartGame();
                break;

            case GameState.End:
                // ���� ���� ó�� (UI ��)
                break;
        }
        if (gameState == GameState.Shop)
        {
            buffManager.DetectBuffCardClick(coin, SpendCoin);
        }
    }

    public void StartGame()
    {
        currentSetCount = maxSetCount;
        currentDropCount = maxDropCount;
        scoreManager.SetScore();
        cardManager.SetCardList();
        stageManager.Initialize();

        // ��ǥ���� �Ҵ� (��: ������������ �ٸ���)
        targetScore = stageManager.GetTargetScore(); // �Ǵ� ���� �Ҵ�

        uiManager.UpdateScoreUI(scoreManager.score, targetScore);
        uiManager.UpdateCountUI(currentSetCount, maxSetCount, currentDropCount, maxDropCount);
        uiManager.UpdateCoinUI(coin);
        stageManager.UpdateStageImage();
        uiManager.HideCollectorComboImages();
        isFirstDraw = true;
        gameState = GameState.Draw;
    }


    private void HandleDraw()
    {
        if (isFirstDraw)
        {
            cardManager.DrawInitialCards();
            isFirstDraw = false;
        }
        gameState = GameState.Select;
    }

    public void OnSetButtonPressed()
    {
        if (currentSetCount <= 0)
        {
            Debug.LogWarning("Set ��� Ƚ���� ��� ����߽��ϴ�.");
            return;
        }
        if (cardManager.checkCardList.Count == 0)
        {
            Debug.LogWarning("���õ� ī�尡 �����ϴ�.");
            return;
        }

        /*// ��� �ؽ�Ʈ �ʱ�ȭ
        if (uiManager.resultText != null)
            uiManager.resultText.text = "";
        */
        currentSetCount--;
        uiManager.UpdateCountUI(currentSetCount, maxSetCount, currentDropCount, maxDropCount);

        //���� ��� �� UI������Ʈ
        scoreManager.ApplyCardEffects(cardManager.checkCardList);
        buffManager.ApplyBuffCards(cardManager.checkCardList, this);
        scoreManager.ApplyCollectorCombos(cardManager.checkCardList);
        scoreManager.CalculateResultScore();
        uiManager.UpdateScoreUI(scoreManager.score, targetScore);

        // selectCardList�� ���� �ִ��� Ȯ��
        if (cardManager.selectCardList != null && cardManager.selectCardList.Count > 0)
        {
            Debug.Log("[GameMaster] OnSetButtonPressed() ȣ�� �� selectCardList ����:");
            foreach (var card in cardManager.selectCardList)
            {
                if (card != null)
                    Debug.Log($" - {card.name}");
                else
                    Debug.Log(" - null");
            }
        }
        else
        {
            Debug.Log("[GameMaster] OnSetButtonPressed() ȣ�� �� selectCardList�� �������");
        }

        // selectCardList�� ������Ʈ�� Destroy �� ����Ʈ 7ĭ¥���� ���ʱ�ȭ
        if (cardManager.selectCardList != null)
        {
            foreach (var card in cardManager.selectCardList)
            {
                if (card != null)
                    Destroy(card);
            }
            cardManager.selectCardList = new List<GameObject>(new GameObject[7]);
        }

        foreach (GameObject card in cardManager.checkCardList)
        {
            GameObject parentCard = card.transform.parent.gameObject;
            cardManager.playCardList.Remove(parentCard);
            Destroy(parentCard);
        }

        drawCount = cardManager.checkCardList.Count;
        cardManager.DrawCards(drawCount);
        cardManager.checkCardList.Clear();

        if (stageManager.CheckStageResult(scoreManager.score, out stageMessage))
        {
            AddCoin(winCoin);
            gameState = GameState.Shop;
            OpenShop();
        }
        else if (currentSetCount == 0)
        {
            gameState = GameState.End;
            HandleGameEnd();
        }
        else
        {
            gameState = GameState.Draw;
        }
    }

    public void OnDropButtonPressed()
    {
        if (currentDropCount <= 0)
        {
            Debug.LogWarning("Drop ��� Ƚ���� ��� ����߽��ϴ�.");
            return;
        }
        if (cardManager.checkCardList.Count == 0)
        {
            Debug.LogWarning("���õ� ī�尡 �����ϴ�.");
            return;
        }

        currentDropCount--;
        uiManager.UpdateCountUI(currentSetCount, maxSetCount, currentDropCount, maxDropCount);

        cardManager.DropAndDrawSelectedCards();

        uiManager.HideCollectorComboImages();
    }

    public void OnNextStagePressed()
    {
        shopManager.CloseShop();
        scoreManager.SetScore();
        uiManager.UpdateScoreUI(scoreManager.score, targetScore);
        gameState = GameState.ShopEnd;
    }

    private void HandleShop()
    {
        if (shopManager.IsShopClosed())
        {
            StartGame();
        }
    }

    private void HandleGameEnd()
    {
        Debug.Log("���� ����");
        // UI �� ���� ���� ó��
    }

    public void OpenShop()
    {
        shopManager.gameObject.SetActive(true);
        shopManager.OpenShop();

    }

    // ���� ���� �޼���
    public void AddCoin(int amount)
    {
        coin += amount;
        // �ʿ�� ���� UI ����
        uiManager.UpdateCoinUI(coin);
    }

    public void SpendCoin(int amount)
    {
        coin -= amount;
        // �ʿ�� ���� UI ����
        uiManager.UpdateCoinUI(coin);
    }

    public int Coin
    {
        get { return coin; }
        set
        {
            coin = value;
            uiManager.UpdateCoinUI(coin);
        }
    }

    // ���Ͻ�/��ƿ �޼���
    public bool IsInPlayCardList(GameObject card)
    {
        return cardManager != null && cardManager.IsInPlayCardList(card);
    }
    public bool IsDrawState()
    {
        return gameState == GameState.Draw;
    }
    public List<GameObject> buffCardList
    {
        get { return buffManager != null ? buffManager.buffCardList : null; }
    }
    public List<int> deckList
    {
        get { return cardManager != null ? cardManager.deckList : null; }
    }
}