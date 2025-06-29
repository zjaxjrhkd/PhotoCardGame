using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

public class GameMaster : MonoBehaviour
{
    public enum GameState { Draw, Select, Shop, ShopEnd, End, Option }
    public GameState gameState;
    private GameState prevGameState; // �ɼ� ���� �� ���� ����

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
    public MusicManager musicManager; // ���� �Ŵ��� �߰�
    public GameObject tutorialSystem; // Ʃ�丮�� �ý��� �߰�

    public GameObject optionUI;

    private int currentSetCount;
    private int currentDropCount;
    public int maxSetCount = 3;
    public int maxDropCount = 3;
    private string stageMessage;
    private int drawCount;

    private bool isFirstDraw = true; // ���� ���� �� ù ��ο츸 10��

    public int coin;
    public int winCoin = 3;
    private int targetScore = 2000;

    // ���� �������� Ÿ�� ����
    private StageManager.StageType currentStageTypeX1;
    private StageManager.StageType currentStageTypeX2;

    void Awake()
    {
        Screen.SetResolution(1920, 1080, false);

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
                // ���� ���� ó�� (UI ��)
                break;
        }

        if (setProcessState != SetProcessState.None && setProcessState != SetProcessState.Idle)
        {
            HandleSetProcess();
        }

        if (gameState == GameState.Shop)
        {
            buffManager.DetectBuffCardClick(coin, SpendCoin, GetCoin);
            cardManager.DetectShopCardClick(ref coin); // �Ϲ� ī�� ���� ó��
        }
    }

    public void StartGame()
    {
        if (musicManager.gameData == null || musicManager.gameData.isTutorial == false)
        {
            uiManager.UpdateBackgroundUI();
            musicManager.Init(); // ���� �Ŵ��� �ʱ�ȭs
            scoreManager.Init(this); // ���� �Ŵ��� �ʱ�ȭ
            uiManager.HideCollectorComboImages();

            maxSetCount = 3;
            maxDropCount = 3;
            stageManager.Initialize();

            var stagePair = stageManager.DecideStageTypeAndUpdateImage();
            if (stagePair.HasValue)
            {
                currentStageTypeX1 = stagePair.Value.x1;
                currentStageTypeX2 = stagePair.Value.x2;
                Debug.Log($"[GameMaster] �������� Ÿ�� ����: x-1={currentStageTypeX1}, x-2={currentStageTypeX2}");

                // x-1 ���������� �ش��ϴ� ���� ���
                if (musicManager != null)
                    musicManager.PlayStageMusic(currentStageTypeX1);
            }

            targetScore = stageManager.GetTargetScore();
            // �������� ȿ�� ����
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
        else if (musicManager.gameData.isTutorial == true)
        {
            TutorialSet(); // Ʃ�丮�� ��Ʈ�� ����
            return;
        }
    }

    public void TutorialSet()
    {
        // Ʃ�丮�� ���������� ����
        tutorialSystem.SetActive(true);
        targetScore = 100;
        stageManager.SetTutorialStage(); // Ʃ�丮�� �������� Ÿ�� ����
        uiManager.UpdateBackgroundUI();
        musicManager.Init();
        scoreManager.Init(this);
        uiManager.HideCollectorComboImages();

        maxSetCount = 3;
        maxDropCount = 3;
        stageManager.InitializeTutorial(); // Ʃ�丮��� �������� ���� �ʱ�ȭ

        // Ʃ�丮�� �������� ȿ�� ����
        stageManager.ApplyStageEffect(scoreManager, cardManager, buffManager, ref targetScore, ref coin);

        buffManager.ApplyStartBuffs();

        currentSetCount = maxSetCount;
        currentDropCount = maxDropCount;
        musicManager.PlayStageMusic(stageManager.stageType);
        scoreManager.SetScore();
        cardManager.SetCardList();

        AllUIUpdate();
        isFirstDraw = true;
        gameState = GameState.Draw;
        setProcessState = SetProcessState.Idle;
        shopManager.gameMaster = this;
        shopManager.cardSpawner = cardSpawner;
        buffManager.cardSpawner = cardSpawner;
        return;
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
                setProcessState = SetProcessState.None; // �ߺ� ���� ����
                break;
            case SetProcessState.CardEffect:
                StartCoroutine(CardEffectCoroutine());
                setProcessState = SetProcessState.None; // �ߺ� ���� ����
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
                    cardManager.selectCardList.ForEach(card => {
                        if (card != null)
                        {
                            Vector3 targetPos = card.transform.position + new Vector3(10f, 0f, -1f);
                            card.transform.DOMove(targetPos, 0.3f).SetEase(Ease.InCubic)
                                .OnComplete(() => Destroy(card));
                        }
                    });
                    cardManager.selectCardList = new List<GameObject>(new GameObject[7]);
                }
                drawCount = cardManager.checkCardList.Count;
                cardManager.DrawCards(drawCount);
                cardManager.checkCardList.Clear();

                if (stageManager.CheckStageResult(scoreManager.score, currentSetCount, out string message))
                {
                    // �������� Ŭ���� �� Ŭ���� Ÿ�� ���
                    stageManager.MarkStageTypeCleared(currentStageTypeX1);
                    stageManager.MarkStageTypeCleared(currentStageTypeX2);

                    musicManager.PlayStageClearSFX();
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
        // ����: yield return StartCoroutine(scoreManager.ApplyHandTypeCardEffects(playCardList));
        yield return StartCoroutine(scoreManager.ApplyHandTypeCardEffects(cardManager.playCardList, cardManager.checkCardList));
        AllUIUpdate();
        setProcessState = SetProcessState.CardEffect;
    }

    private IEnumerator CardEffectCoroutine()
    {
        int cardCount = cardManager.checkCardList.Count;

        yield return StartCoroutine(scoreManager.ApplyCardEffects(cardManager.checkCardList));

        // ���� ���������� ī�� ������ŭ ���� ����
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
        Debug.Log("[GameMaster] BuffEffectCoroutine ����");
        yield return StartCoroutine(buffManager.ApplyBuffCards(buffManager.buffCardList));
        Debug.Log("[GameMaster] BuffEffectCoroutine ����, Collector �ܰ�� �̵�");
        setProcessState = SetProcessState.Calculate;
    }

    private IEnumerator CollectorCoroutine()
    {
        Debug.Log("[GameMaster] CollectorCoroutine ����");
        yield return StartCoroutine(scoreManager.ApplyCollectorCombosCoroutine(cardManager.checkCardList));
        Debug.Log("[GameMaster] CollectorCoroutine ����, Calculate �ܰ�� �̵�");
        setProcessState = SetProcessState.Buff;
    }

    public void OnOptionButtonPressed()
    {
        // SFX ���
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
        // ���� �޴��� �̵�
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
        // �ɼ� UI�� ���� ������ �ݱ�
        if (optionUI != null)
            optionUI.SetActive(false);

        // �������� Ŭ���� ���� �� �ε��� �ʱ�ȭ
        if (stageManager != null)
        {
            // �������� Ŭ���� Ÿ�� �ʱ�ȭ
            var clearedTypesField = stageManager.GetType().GetField("clearedTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (clearedTypesField != null)
            {
                var clearedTypes = clearedTypesField.GetValue(stageManager) as HashSet<StageManager.StageType>;
                clearedTypes?.Clear();
            }
            // �������� �ε��� �ʱ�ȭ
            var currentStageIndexField = stageManager.GetType().GetField("currentStageIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (currentStageIndexField != null)
                currentStageIndexField.SetValue(stageManager, 0);
        }
        // ������ ĳ���� ī�� �ʱ�ȭ
        if (cardManager != null && cardManager.purchasedCardList != null)
            cardManager.purchasedCardList.Clear();
        for(int i = 0; i < buffManager.buffCardList.Count; i++)
        {
            Destroy(buffManager.buffCardList[i]);
        }
        buffManager.buffCardList.Clear();
           
        // ���� �����
        StartGame();
    }
    public void PlusSetCount()
    {
        currentSetCount ++;
        uiManager.UpdateCountUI(currentSetCount, maxSetCount, currentDropCount, maxDropCount);
    }

    public void OnSetButtonPressed()
    {
        musicManager.PlayUIClickSFX();
        if (setProcessState == SetProcessState.Idle)
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

            // ��Ʈ ���� �� ��� ���� ScoreManager�� ����
            scoreManager.CheckAndApplySetBackground(cardManager.checkCardList);

            currentSetCount--;
            uiManager.UpdateCountUI(currentSetCount, maxSetCount, currentDropCount, maxDropCount);

            // Set ó�� ���� ����
            setProcessState = SetProcessState.HandCardEffect;
        }
    }

    public void OnDropButtonPressed()
    {
        // SFX ���
        musicManager.PlayDropCardSFX();


        if (setProcessState == SetProcessState.Idle)
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
    }

    public void OnNextStagePressed()
    {
        if (stageManager.stageType == StageManager.StageType.Tutorial)
        {
             musicManager.gameData.isTutorial = false;
        }
        shopManager.CloseShop();
        scoreManager.SetScore();
        uiManager.UpdateScoreUI(scoreManager.score, targetScore);

        // Shop ���������� ���� ���������� �ε��� ����
        if (stageManager.IsShopStage())
        {
            stageManager.GoToNextStage();
        }
        else
        {
            // �Ϲ� ���������� ������� ����
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
        if (stageManager.stageType == StageManager.StageType.Tutorial)
        {
            return;
        }
        else
        {
            coin += amount;
            // �ʿ�� ���� UI ����
        }

        uiManager.UpdateCoinUI(coin);
    }

    public void SpendCoin(int amount)
    {
        coin -= amount;
        // �ʿ�� ���� UI ����
        uiManager.UpdateCoinUI(coin);
    }

    public void GetCoin(int amount)
    {
        if (stageManager.stageType == StageManager.StageType.Tutorial)
        {
            return;
        }
        else
        {
            coin += amount;
            // �ʿ�� ���� UI ����
        }

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