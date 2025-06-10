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

    private bool isFirstDraw = true; // 게임 시작 후 첫 드로우만 10장

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
                // 게임 종료 처리 (UI 등)
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

        // 목표점수 할당 (예: 스테이지별로 다르게)
        targetScore = stageManager.GetTargetScore(); // 또는 직접 할당

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
            Debug.LogWarning("Set 사용 횟수를 모두 사용했습니다.");
            return;
        }
        if (cardManager.checkCardList.Count == 0)
        {
            Debug.LogWarning("선택된 카드가 없습니다.");
            return;
        }

        /*// 결과 텍스트 초기화
        if (uiManager.resultText != null)
            uiManager.resultText.text = "";
        */
        currentSetCount--;
        uiManager.UpdateCountUI(currentSetCount, maxSetCount, currentDropCount, maxDropCount);

        //점수 계산 및 UI업데이트
        scoreManager.ApplyCardEffects(cardManager.checkCardList);
        buffManager.ApplyBuffCards(cardManager.checkCardList, this);
        scoreManager.ApplyCollectorCombos(cardManager.checkCardList);
        scoreManager.CalculateResultScore();
        uiManager.UpdateScoreUI(scoreManager.score, targetScore);

        // selectCardList에 뭐가 있는지 확인
        if (cardManager.selectCardList != null && cardManager.selectCardList.Count > 0)
        {
            Debug.Log("[GameMaster] OnSetButtonPressed() 호출 시 selectCardList 내용:");
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
            Debug.Log("[GameMaster] OnSetButtonPressed() 호출 시 selectCardList가 비어있음");
        }

        // selectCardList의 오브젝트도 Destroy 및 리스트 7칸짜리로 재초기화
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
            Debug.LogWarning("Drop 사용 횟수를 모두 사용했습니다.");
            return;
        }
        if (cardManager.checkCardList.Count == 0)
        {
            Debug.LogWarning("선택된 카드가 없습니다.");
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
        Debug.Log("게임 종료");
        // UI 등 게임 종료 처리
    }

    public void OpenShop()
    {
        shopManager.gameObject.SetActive(true);
        shopManager.OpenShop();

    }

    // 코인 관련 메서드
    public void AddCoin(int amount)
    {
        coin += amount;
        // 필요시 코인 UI 갱신
        uiManager.UpdateCoinUI(coin);
    }

    public void SpendCoin(int amount)
    {
        coin -= amount;
        // 필요시 코인 UI 갱신
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

    // 프록시/유틸 메서드
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