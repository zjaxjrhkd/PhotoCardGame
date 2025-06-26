using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameMaster : MonoBehaviour
{
    public enum GameState { Draw, Select, Shop, ShopEnd, End, Option }
    public GameState gameState;
    private GameState prevGameState; // 옵션 진입 전 상태 저장

    public enum SetProcessState { None, Option,Idle, HandCardEffect, CardEffect, Buff, Collector, Calculate, Done }
    private SetProcessState setProcessState;
    private SetProcessState prevSetProcessState;

    public CardManager cardManager;
    public BuffManager buffManager;
    public ScoreManager scoreManager;
    public StageManager stageManager;
    public UIManager uiManager;
    public ShopManager shopManager;
    public CardSpawner cardSpawner;
    public MusicManager musicManager; // 음악 매니저 추가

    public GameObject optionUI;

    private int currentSetCount;
    private int currentDropCount;
    public int maxSetCount = 3;
    public int maxDropCount = 3;
    private string stageMessage;
    private int drawCount;

    private bool isFirstDraw = true; // 게임 시작 후 첫 드로우만 10장

    public int coin;
    public int winCoin = 3;
    private int targetScore = 2000;

    // 랜덤 스테이지 타입 저장
    private StageManager.StageType currentStageTypeX1;
    private StageManager.StageType currentStageTypeX2;

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
        musicManager = GetComponent<MusicManager>();
        cardSpawner = GetComponent<CardSpawner>();
        cardManager.deckCount = cardManager.defaltdeckCount;
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

        if (setProcessState != SetProcessState.None && setProcessState != SetProcessState.Idle)
        {
            HandleSetProcess();
        }

        if (gameState == GameState.Shop)
        {
            buffManager.DetectBuffCardClick(coin, SpendCoin, GetCoin);
            cardManager.DetectShopCardClick(ref coin); // 일반 카드 구매 처리
        }
    }

    public void StartGame()
    {
        uiManager.UpdateBackgroundUI();
        musicManager.Init(); // 음악 매니저 초기화s
        scoreManager.Init(this); // 점수 매니저 초기화
        uiManager.HideCollectorComboImages();

        maxSetCount = 3;
        maxDropCount = 3;
        stageManager.Initialize();

        var stagePair = stageManager.DecideStageTypeAndUpdateImage();
        if (stagePair.HasValue)
        {
            currentStageTypeX1 = stagePair.Value.x1;
            currentStageTypeX2 = stagePair.Value.x2;
            Debug.Log($"[GameMaster] 스테이지 타입 결정: x-1={currentStageTypeX1}, x-2={currentStageTypeX2}");

            // x-1 스테이지에 해당하는 음악 재생
            if (musicManager != null)
                musicManager.PlayStageMusic(currentStageTypeX1);
        }

        targetScore = stageManager.GetTargetScore();
        // 스테이지 효과 적용
        stageManager.ApplyStageEffect(scoreManager, cardManager, buffManager, ref targetScore, ref coin);

        buffManager.ApplyStartBuffs();

        currentSetCount = maxSetCount;
        currentDropCount = maxDropCount;

        scoreManager.SetScore();
        cardManager.SetCardList();

        AllUIUpdate();
        isFirstDraw = true;
        gameState = GameState.Draw;
        setProcessState = SetProcessState.Idle;
        shopManager.gameMaster = this;
        shopManager.cardSpawner = cardSpawner;
        buffManager.cardSpawner = cardSpawner;
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
            case SetProcessState.HandCardEffect:
                StartCoroutine(HandCardEffectCoroutine());
                setProcessState = SetProcessState.None; // 중복 실행 방지
                break;
            case SetProcessState.CardEffect:
                StartCoroutine(CardEffectCoroutine());
                setProcessState = SetProcessState.None; // 중복 실행 방지
                break;
            case SetProcessState.Collector:
                StartCoroutine(CollectorCoroutine());
                setProcessState = SetProcessState.None;
                break;
            case SetProcessState.Buff:
                if (!scoreManager.isSitryStage)
                {
                    StartCoroutine(BuffEffectCoroutine());
                    setProcessState = SetProcessState.None;
                }
                else setProcessState = SetProcessState.Calculate;
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
                    // 스테이지 클리어 시 클리어 타입 등록
                    stageManager.MarkStageTypeCleared(currentStageTypeX1);
                    stageManager.MarkStageTypeCleared(currentStageTypeX2);

                    AddCoin(winCoin);
                    ClearPlayCardList();
                    cardManager.SetDeckSuffle();
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
                setProcessState = SetProcessState.Idle;
                break;
        }
    }

    private IEnumerator HandCardEffectCoroutine()
    {
        // 기존: yield return StartCoroutine(scoreManager.ApplyHandTypeCardEffects(playCardList));
        yield return StartCoroutine(scoreManager.ApplyHandTypeCardEffects(cardManager.playCardList, cardManager.checkCardList));
        AllUIUpdate();
        setProcessState = SetProcessState.CardEffect;
    }

    private IEnumerator CardEffectCoroutine()
    {
        int cardCount = cardManager.checkCardList.Count;

        yield return StartCoroutine(scoreManager.ApplyCardEffects(cardManager.checkCardList));

        // 벨디르 스테이지면 카드 개수만큼 코인 감소
        if (scoreManager.isBeldirStage)
        {
            musicManager.PlayBuyBuffSFX();
            coin -= cardCount;
            if (uiManager != null)
                uiManager.UpdateCoinUI(coin);
        }

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

    public void OnOptionButtonPressed()
    {
        // SFX 재생
        musicManager.PlayUIClickSFX();
        if (optionUI != null)
            optionUI.SetActive(true);
    }

    public void OnExitButtonPressed()
    {
        musicManager.PlayUIClickSFX();
        if (optionUI != null)
            optionUI.SetActive(false);
    }

    public void OnMainMenuButtonPressed()
    {
        if (optionUI != null)
            optionUI.SetActive(false);
        // 메인 메뉴로 이동
        UnityEngine.SceneManagement.SceneManager.LoadScene("2.MainmenuScene");
    }

    public void ClearPlayCardList()
    {
        if (cardManager == null || cardManager.playCardList == null)
            return;

        foreach (var card in cardManager.playCardList)
        {
            if (card != null)
                Destroy(card);
        }
        cardManager.playCardList.Clear();
    }

    public void OnRestartButtonPressed()
    {
        musicManager.PlayUIClickSFX();
        // 옵션 UI가 열려 있으면 닫기
        if (optionUI != null)
            optionUI.SetActive(false);

        // 게임 재시작
        StartGame();
    }

    public void OnSetButtonPressed()
    {
        musicManager.PlayUIClickSFX();
        if (setProcessState == SetProcessState.Idle)
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

            // 세트 판정 및 배경 변경 ScoreManager에 위임
            scoreManager.CheckAndApplySetBackground(cardManager.checkCardList);

            currentSetCount--;
            uiManager.UpdateCountUI(currentSetCount, maxSetCount, currentDropCount, maxDropCount);

            // Set 처리 상태 시작
            setProcessState = SetProcessState.HandCardEffect;
        }
    }

    public void OnDropButtonPressed()
    {
        // SFX 재생
        musicManager.PlayDropCardSFX();


        if (setProcessState == SetProcessState.Idle)
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
    }

    public void OnNextStagePressed()
    {
        shopManager.CloseShop();
        scoreManager.SetScore();
        uiManager.UpdateScoreUI(scoreManager.score, targetScore);

        // Shop 스테이지면 다음 스테이지로 인덱스 증가
        if (stageManager.IsShopStage())
        {
            stageManager.GoToNextStage();
        }
        else
        {
            // 일반 스테이지면 기존대로 동작
            stageManager.UpdateStageUI();
        }

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

    public void GetCoin(int amount)
    {
        coin += amount;
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