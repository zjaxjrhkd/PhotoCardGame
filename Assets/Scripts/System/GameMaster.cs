// GameMaster.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class GameMaster : MonoBehaviour
{
    enum GameState { Set, Draw, Select, Check, Drop, Shop, End }

    public int score;
    private GameState gameState;

    private int currentSetCount;
    private int currentDropCount;
    private int maxSetCount = 3;
    private int maxDropCount = 3;

    public int drawCardEa;
    public List<int> deckList;
    public List<int> drawCardList;
    public List<GameObject> playCardList;
    public List<GameObject> checkCardList;
    public List<GameObject> dropCardList;
    public List<GameObject> selectCardList;
    public List<GameObject> buffCardList;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI setCountText;
    public TextMeshProUGUI dropCountText;

    public CardSpawner cardSpawner;
    private ScoreCalculator scoreCalculator;
    private StageManager stageManager;

    private CardState cardStateRead;
    private CardData cardDataRead;

    private string stageMessage;
    private bool isStageCleared;
    private int stageIndex;

    float minX = -3.65f;
    float maxX = 5f;
    float y = -3.2f;
    float baseZ = -1f;
    float zStep = 0.1f;

    public SpriteListSO stageImageListSO;
    public UnityEngine.UI.Image stageUIImage;

    public int coin;
    public TextMeshProUGUI coinText;
    public ShopManager shopManager;

    public BuffCardListSO buffCardListSO;

    void Start()
    {
        SetGameState();
        SetCardSpawner();
        SetCardList();
        SetDeckSuffle();
        SetScore();
        buffCardList = new List<GameObject>();
        StartGame();
    }

    void Update()
    {
        RunGameFlow();
        if (gameState == GameState.Shop)
        {
            DetectBuffCardClick();
        }
    }

    public void SetGameState()
    {
        gameState = GameState.Set;
        Debug.Log("게임 상태: " + gameState);
    }

    public void SetScore()
    {
        score = 0;
        UpdateScoreUI();
    }

    public void StartGame()
    {
        gameState = GameState.Draw;
        score = 0;
        InitStageCount();
        SetCardList();
        
        UpdateScoreUI();
        UpdateCountUI();
        UpdateStageImage();

        Debug.Log("게임 시작, 상태: " + gameState);
    }

    private void DetectBuffCardClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider == null) return;

        CardData data = hit.collider.GetComponent<CardData>();
        if (data == null || data.cardType != CardData.CardType.Buff) return;

        if (coin < 3)
        {
            Debug.Log("코인이 부족합니다.");
            return;
        }

        SpendCoin(3);

        GameObject clickedCard = data.gameObject;
        clickedCard.SetActive(false); // 상점에서 제거

        GameObject newCard = Instantiate(clickedCard); // 복사본 생성
        buffCardList.Add(newCard);
        RearrangeBuffCards();

        Debug.Log($"버프 카드 구매 완료: {newCard.name}");
    }

    private void RearrangeBuffCards()
    {
        float minX = -3f;
        float maxX = 5f;
        float y = 3.19f;
        float z = -0.1f;

        int count = buffCardList.Count;
        if (count == 0) return;

        float spacing = (count > 1) ? (maxX - minX) / (count - 1) : 0f;

        for (int i = 0; i < count; i++)
        {
            GameObject card = buffCardList[i];
            card.SetActive(true);
            card.transform.position = new Vector3(minX + spacing * i, y, z);
        }
    }


    public void BuyBuffCardById(int cardId)
    {
        if (buffCardListSO == null || cardId < 0 || cardId >= buffCardListSO.buffCards.Count)
        {
            Debug.LogWarning("잘못된 카드 ID");
            return;
        }

        if (coin < 3)
        {
            Debug.Log("코인이 부족합니다.");
            return;
        }

        SpendCoin(3);

        GameObject prefab = buffCardListSO.buffCards[cardId];
        buffCardList.Add(prefab);

        Debug.Log($"버프 카드 {prefab.name} 구매 완료");
        RearrangeBuffCards();
    }

    public void InitStageCount()
    {
        currentSetCount = maxSetCount;
        currentDropCount = maxDropCount;
        UpdateCountUI();
    }

    public void SetCardSpawner()
    {
        cardSpawner = GetComponent<CardSpawner>();
        cardSpawner.SetDeckSpawnPosition();
        scoreCalculator = GetComponent<ScoreCalculator>();
        stageManager = GetComponent<StageManager>();
        stageManager.Initialize();
        shopManager = GetComponent<ShopManager>();
    }

    public void SetCardList()
    {
        SetDeckSuffle();

        checkCardList = new List<GameObject>();
        dropCardList = new List<GameObject>();
        selectCardList = new List<GameObject>();
        drawCardList = new List<int>();

        if (playCardList != null)
        {
            foreach (var card in playCardList)
            {
                if (card != null)
                    Destroy(card);
            }
        }
        playCardList = new List<GameObject>();
    }

    public void SetDeckSuffle()
    {
        deckList = new List<int>();
        for (int i = 1; i <= 54; i++) deckList.Add(i);
        for (int i = 0; i < deckList.Count; i++)
        {
            int randIndex = Random.Range(i, deckList.Count);
            (deckList[i], deckList[randIndex]) = (deckList[randIndex], deckList[i]);
        }
        Debug.Log("덱 섞기 완료, 카드 수: " + deckList.Count);
    }

    private void UpdateStageImage()
    {
        int stageIndex = stageManager.GetCurrentStageIndex();

        if (stageImageListSO != null && stageImageListSO.sprites.Count > stageIndex)
        {
            stageUIImage.sprite = stageImageListSO.sprites[stageIndex];
            Debug.Log($"스테이지 이미지 변경: {stageIndex}");
        }
        else
        {
            Debug.LogWarning("스테이지 이미지 리스트에 해당 인덱스가 없습니다.");
        }
    }

    public bool IsDrawState()
    {
        return gameState == GameState.Draw;
    }

    public void OnNextStageButtonPressed()
    {
        Debug.Log("다음 스테이지 버튼 클릭됨");
        shopManager.CloseShop();
        StartGame();
    }

    void RunGameFlow()
    {
        switch (gameState)
        {
            case GameState.Draw:
                DrawCard();
                gameState = GameState.Select;
                break;

            case GameState.Select:
                ClickCard();
                break;

            case GameState.Shop:
                break;

            case GameState.End:
                Debug.Log("게임 종료 상태");
                break;
        }
    }

    public void DrawCard()
    {
        drawCardList = new List<int>();
        int drawCount = Mathf.Min(drawCardEa, deckList.Count);
        if (drawCount == 0) return;

        for (int i = 0; i < drawCount; i++)
        {
            int cardId = deckList[0];
            drawCardList.Add(cardId);
            deckList.RemoveAt(0);
        }

        List<GameObject> spawnedCards = cardSpawner.SpawnCardsByID(drawCardList);
        playCardList.AddRange(spawnedCards);
        ArrangeCardsWithinRange();
    }

    public void OnSetButtonPressed()
    {
        if (checkCardList.Count == 0)
        {
            Debug.LogWarning("선택된 카드가 없습니다.");
            return;
        }

        if (!TryUseSet())
        {
            Debug.LogWarning("Set 사용 횟수를 모두 사용했습니다.");
            return;
        }

        ArrangeCardsInRange(checkCardList, -3.65f, 5f, 0f, -0.1f);

        foreach (GameObject card in checkCardList)
        {
            CardData cardData = card.GetComponentInChildren<CardData>();
            if (cardData == null)
            {
                Debug.LogWarning($"{card.name}에서 CardData를 찾을 수 없습니다.");
                continue;
            }

            string scriptName = cardData.cardName;

            System.Type type = System.Type.GetType(scriptName);
            if (type == null)
            {
                Debug.LogWarning($"{scriptName} 타입을 찾을 수 없습니다.");
                continue;
            }

            Component effectScript = cardData.GetComponent(type);
            if (effectScript == null)
            {
                Debug.LogWarning($"{scriptName} 스크립트를 찾을 수 없습니다.");
                continue;
            }

            var method = type.GetMethod("Effect");
            if (method != null)
            {
                method.Invoke(effectScript, null);
                Debug.Log($"{scriptName}.Effect() 호출 완료");
            }
            else
            {
                Debug.LogWarning($"{scriptName}에 Effect() 메서드가 없습니다.");
            }
        }

        ApplyBuffCards();
        ApplyCardEffects();
        ApplyCollectorCombos();

        UpdateScoreUI();
        StartCoroutine(DropAndEvaluateAfterDelay(2f));
    }

    private void ApplyBuffCards()
    {
        foreach (GameObject card in checkCardList)
        {
            var buffs = card.GetComponentsInChildren<IBuffCard>();
            foreach (var buff in buffs)
            {
                buff.ApplyBuff(this);
            }
        }
    }

    public interface IBuffCard
    {
        void ApplyBuff(GameMaster game);
    }

    private void ApplyCardEffects()
    {
        foreach (GameObject card in checkCardList)
        {
            CardData cardData = card.GetComponentInChildren<CardData>();
            if (cardData == null) continue;

            string scriptName = cardData.cardName;
            System.Type type = System.Type.GetType(scriptName);
            if (type == null) continue;

            Component script = cardData.GetComponent(type);
            System.Reflection.MethodInfo method = type.GetMethod("Effect");

            if (script != null && method != null)
            {
                method.Invoke(script, null);
            }
        }
    }

    private void ApplyCollectorCombos()
    {
        List<int> selectedCardIds = checkCardList
            .Select(card => card.GetComponent<CardData>()?.cardId ?? -1)
            .Where(id => id > 0)
            .ToList();

        if (scoreCalculator != null)
        {
            var matchedScores = scoreCalculator.GetMatchedCollectorScores(selectedCardIds);

            foreach (var entry in matchedScores)
            {
                score += entry.Value;
            }

            if (matchedScores.Count > 0)
            {
                string names = string.Join(", ", matchedScores.Keys);
                resultText.text = $"조합 성공! → {names}";
            }
            else
            {
                resultText.text = "일치하는 조합 없음";
                score += 10;
            }
        }
    }

    public void AddCoin(int amount)
    {
        coin += amount;
        UpdateCoinUI();
    }

    public void SpendCoin(int amount)
    {
        coin -= amount;
        UpdateCoinUI();
    }

    private void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = $"Coin: {coin}";
    }

    public void OpenShop()
    {
        shopManager.gameObject.SetActive(true);
        shopManager.OpenShop();
    }

    private IEnumerator DropAndEvaluateAfterDelay(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        DropCardWithoutCost();
        yield return new WaitForSeconds(0.5f);

        if (stageManager.CheckStageResult(score, out stageMessage))
        {
            AddCoin(5);
            resultText.text += "\n" + stageMessage;

            if (stageManager.IsLastStage())
            {
                GameClear();
            }
            else
            {
                gameState = GameState.Shop;
                OpenShop();
            }
        }
        else if (currentSetCount == 0)
        {
            HandleStageEnd();
        }
    }

    public void DropCardByPlayer()
    {
        if (checkCardList.Count == 0)
        {
            Debug.LogWarning("선택된 카드가 없습니다. 드롭 불가.");
            return;
        }

        if (!TryUseDrop())
        {
            Debug.LogWarning("Drop 사용 횟수를 모두 소진했습니다.");
            return;
        }

        ExecuteDropAndDraw();
    }

    private void DropCardWithoutCost() => ExecuteDropAndDraw();

    private void ExecuteDropAndDraw()
    {
        if (checkCardList.Count == 0) return;
        int dropCount = checkCardList.Count;

        foreach (GameObject card in checkCardList)
        {
            dropCardList.Add(card);
            GameObject parentCard = card.transform.parent.gameObject;
            playCardList.Remove(parentCard);
            Destroy(parentCard);
        }

        checkCardList.Clear();
        int originalDrawCount = drawCardEa;
        drawCardEa = dropCount;
        DrawCard();
        drawCardEa = originalDrawCount;
        ArrangeCardsWithinRange();
    }

    private void HandleStageEnd()
    {
        isStageCleared = stageManager.CheckStageResult(score, out stageMessage);
        resultText.text += "\n" + stageMessage;
        if (!isStageCleared) GameOver();
        else if (stageManager.IsLastStage()) GameClear();
        else StartCoroutine(DropCardAfterDelay(2f));
    }

    private IEnumerator DropCardAfterDelay(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        DropCardWithoutCost();
    }

    public void GameOver()
    {
        gameState = GameState.End;
        resultText.text += "\n게임 오버";
    }

    public void GameClear()
    {
        gameState = GameState.End;
        resultText.text += "\n게임 클리어!";
    }

    public bool TryUseSet()
    {
        if (currentSetCount > 0) { currentSetCount--; UpdateCountUI(); return true; }
        return false;
    }

    public bool TryUseDrop()
    {
        if (currentDropCount > 0) { currentDropCount--; UpdateCountUI(); return true; }
        return false;
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Score : " + score.ToString();
    }

    private void UpdateCountUI()
    {
        if (setCountText != null) setCountText.text = $"Set: {currentSetCount}/{maxSetCount}";
        if (dropCountText != null) dropCountText.text = $"Drop: {currentDropCount}/{maxDropCount}";
    }

    public void IncreaseSetCountByItem(int amount)
    {
        maxSetCount += amount;
        currentSetCount += amount;
        UpdateCountUI();
    }

    public void IncreaseDropCountByItem(int amount)
    {
        maxDropCount += amount;
        currentDropCount += amount;
        UpdateCountUI();
    }
    public void ClickCard()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            cardStateRead = hit.collider.GetComponent<CardState>();
            cardDataRead = hit.collider.GetComponent<CardData>();

            if (cardStateRead != null)
            {
                cardStateRead.CardStateClicked(); // ← 반드시 호출되어야 함
                AddCheckCardList();
            }
        }
    }

    public void AddCheckCardList()
    {
        GameObject card = cardDataRead.gameObject;
        GameObject parentCard = card.transform.parent.gameObject;

        if (cardStateRead.isClick)
        {
            if (!checkCardList.Contains(card))
                checkCardList.Add(card);

            if (!selectCardList.Contains(parentCard))
            {
                selectCardList.Add(parentCard);
                Vector3 pos = parentCard.transform.position;
                parentCard.transform.position = new Vector3(pos.x, pos.y + 3.2f, pos.z);
            }
        }
        else
        {
            RemoveCheckCardList();
        }
    }

    public void RemoveCheckCardList()
    {
        GameObject card = cardDataRead.gameObject;
        GameObject parentCard = card.transform.parent.gameObject;

        if (checkCardList.Contains(card))
            checkCardList.Remove(card);

        if (selectCardList.Contains(parentCard))
        {
            selectCardList.Remove(parentCard);
            Vector3 pos = parentCard.transform.position;
            parentCard.transform.position = new Vector3(pos.x, pos.y - 3.2f, pos.z);
        }
    }


    public void ArrangeCardsWithinRange()
    {
        if (playCardList == null || playCardList.Count == 0) return;
        List<GameObject> activeCards = playCardList.FindAll(card => card != null && card.activeSelf);
        int count = activeCards.Count;
        if (count == 0) return;

        float spacing = count > 1 ? (maxX - minX) / (count - 1) : 0f;

        for (int i = 0; i < count; i++)
        {
            float x = minX + spacing * i;
            float z = baseZ + zStep * i;
            activeCards[i].transform.position = new Vector3(x, y, z);
        }
    }

    public void ArrangeCardsInRange(List<GameObject> cardList, float minX, float maxX, float y, float z)
    {
        if (cardList == null || cardList.Count == 0) return;
        List<GameObject> activeCards = cardList.FindAll(card => card != null && card.activeSelf);
        int count = activeCards.Count;
        if (count == 0) return;

        float spacing = count > 1 ? (maxX - minX) / (count - 1) : 0f;

        for (int i = 0; i < count; i++)
        {
            float offsetZ = z - (i * 0.1f); // 오른쪽으로 갈수록 z 감소
            Vector3 newPos = new Vector3(minX + spacing * i, y, offsetZ);
            activeCards[i].transform.position = newPos;
        }
    }

    public bool IsInPlayCardList(GameObject card)
    {
        return playCardList.Contains(card.transform.parent.gameObject); // 카드 자체가 아니라 카드의 부모(슬롯)를 비교
    }


}
