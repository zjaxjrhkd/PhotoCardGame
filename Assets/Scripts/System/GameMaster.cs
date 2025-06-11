using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GameMaster : MonoBehaviour
{
    public enum GameState { Draw, Select, Shop, ShopEnd, End }
    private GameState gameState;

    public enum SetProcessState { None, CardEffect, Buff, Collector, Calculate, Done }
    private SetProcessState setProcessState;

    private CardManager cardManager;
    private BuffManager buffManager;
    private ScoreManager scoreManager;
    private StageManager stageManager;
    public UIManager uiManager;
    private ShopManager shopManager;
    public CardSpawner cardSpawner;

    private int currentSetCount;
    private int currentDropCount;
    public int maxSetCount = 3;
    public int maxDropCount = 3;
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
        scoreManager.uiManager = uiManager;
        buffManager.uiManager = uiManager;
        buffManager.gameMaster = this;
        buffManager.scoreManager = scoreManager;
        buffManager.cardManager = cardManager;

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

        // Set 처리 상태 머신
        if (setProcessState != SetProcessState.None && setProcessState != SetProcessState.Done)
        {
            HandleSetProcess();
        }

        if (gameState == GameState.Shop)
        {
            buffManager.DetectBuffCardClick(coin, SpendCoin);
        }
    }

    public void StartGame()
    {
        maxSetCount = 3; // 초기 Set 사용 횟수 설정
        maxDropCount = 3;

        buffManager.ApplyStartBuffs(); // 게임 시작 시 버프 적용

        currentSetCount = maxSetCount;
        currentDropCount = maxDropCount;

        scoreManager.SetScore();
        cardManager.SetCardList();
        stageManager.Initialize();
        

        // 목표점수 할당 (예: 스테이지별로 다르게)
        targetScore = stageManager.GetTargetScore(); // 또는 직접 할당

        AllUIUpdate();
        stageManager.UpdateStageImage();
        isFirstDraw = true;
        gameState = GameState.Draw;
        setProcessState = SetProcessState.None; // Set 처리 상태 초기화
    }

    public void AllUIUpdate()
    {
        uiManager.UpdateScoreUI(scoreManager.score, targetScore);
        uiManager.UpdateCountUI(currentSetCount, maxSetCount, currentDropCount, maxDropCount);
        uiManager.UpdateCoinUI(coin);
        if (setProcessState == SetProcessState.Calculate)
        {
            uiManager.HideCollectorComboImages();
        }
        
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

    private void HandleSetProcess()
    {
        switch (setProcessState)
        {
            case SetProcessState.CardEffect:
                StartCoroutine(CardEffectCoroutine());
                setProcessState = SetProcessState.None; // 중복 실행 방지
                break;
            case SetProcessState.Collector:
                StartCoroutine(CollectorCoroutine());
                setProcessState = SetProcessState.None;
                break;
            case SetProcessState.Buff:
                StartCoroutine(BuffEffectCoroutine());
                setProcessState = SetProcessState.None;
                break;
            case SetProcessState.Calculate:
                scoreManager.CalculateResultScore();
                AllUIUpdate();
                scoreManager.SetCalculateResutScore();

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

                setProcessState = SetProcessState.Done;
                break;
        }
    }

    // 코루틴에서 카드 이펙트 적용 후 상태 전환
    private IEnumerator CardEffectCoroutine()
    {
        yield return StartCoroutine(scoreManager.ApplyCardEffects(cardManager.checkCardList));
        AllUIUpdate();
        setProcessState = SetProcessState.Collector;
    }

    private IEnumerator BuffEffectCoroutine()
    {
        Debug.Log("[GameMaster] BuffEffectCoroutine 시작");
        yield return StartCoroutine(buffManager.ApplyBuffCards(buffManager.buffCardList));
        Debug.Log("[GameMaster] BuffEffectCoroutine 종료, Collector 단계로 이동");
        setProcessState = SetProcessState.Calculate;
    }

    private IEnumerator CollectorCoroutine()
    {
        Debug.Log("[GameMaster] CollectorCoroutine 시작");
        yield return StartCoroutine(scoreManager.ApplyCollectorCombosCoroutine(cardManager.checkCardList));
        Debug.Log("[GameMaster] CollectorCoroutine 종료, Calculate 단계로 이동");
        setProcessState = SetProcessState.Buff;
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

        currentSetCount--;
        uiManager.UpdateCountUI(currentSetCount, maxSetCount, currentDropCount, maxDropCount);

        // Set 처리 상태 시작
        setProcessState = SetProcessState.CardEffect;
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