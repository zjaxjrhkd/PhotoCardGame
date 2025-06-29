using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardManager : MonoBehaviour
{
    public List<int> deckList = new List<int>();
    public List<int> purchasedCardList = new List<int>(); // 구매 카드 리스트
    
    public List<int> drawCardList = new List<int>();
    public List<GameObject> playCardList = new List<GameObject>();
    public List<GameObject> checkCardList = new List<GameObject>();
    public List<GameObject> dropCardList = new List<GameObject>();
    public List<GameObject> selectCardList = new List<GameObject>();

    public CardSpawner cardSpawner;
    public ScoreManager scoreManager;
    public GameObject otherUIObjects;
    public GameObject deckListParent;
    public GameObject collectionList;
    private bool isDeckListAtOrigin = false;
    private bool isColListAtOrigin = false;
    public GameObject[] collectionGuideList;
    private int currentGuideIndex = -1;
    public GameObject nextButton;

    private GameMaster.GameState prevState;
    

    public int defaltdeckCount = 54;
    public int deckCount;

    public float minX = -4.5f;
    public float maxX = 5.9f;
    public float y = -3.2f;
    public float baseZ = -100f;
    public float zStep = 0.1f;

    public int defaultDrawCardEa = 10;
    public int drawCardEa = 1;

    private CardState lastHoveredCard;
    private GameMaster gameMaster;

    private static readonly float[] selectCardXPositions = { -4.5f, -2.7f, -0.9f, 0.9f, 2.7f, 4.5f, 6.3f };
    private const float selectCardY = 0f;
    private const float selectCardZ = -0.1f;

    public int totalDeckCount;

    public bool isLimiStage = false;
    public bool isNoiStage = false;
    public bool isRazStage = false;
    public bool isIanaStage = false;

    void Start()
    {
        gameMaster = FindObjectOfType<GameMaster>();
        cardSpawner = GetComponent<CardSpawner>();
        if (scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>();
    }

    public void InitStageVariables()
    {
        isLimiStage = false;
        isNoiStage = false;
        isRazStage = false;
        isIanaStage = false;
    }

    void Update()
    {
        HandleCardHover();
    }

    public void DrawInitialCards()
    {
        DrawCards(defaultDrawCardEa);
        Debug.Log($"[CardManager] 초기 카드 {defaultDrawCardEa}장 드로우 완료. 현재 플레이 카드 수: {playCardList.Count}");
    }

    public void DetectShopCardClick(ref int coin)
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider == null) return;

        CardData data = hit.collider.GetComponent<CardData>();
        if (data == null || data.cardType != CardData.CardType.Character) return;

        int cardId = data.cardId;
        int cost = data.CardCost;

        if (coin < cost)
        {
            Debug.Log("코인이 부족합니다.");
            return;
        }

        // 카드 생성
        List<GameObject> spawned = cardSpawner.SpawnCardsByID(new List<int> { cardId });
        if (spawned.Count == 0) return;

        GameObject cardObj = spawned[0];
        playCardList.Add(cardObj);

        // 코인 차감
        coin -= cost;

        // 카드 효과 초기화
        var cardData = cardObj.GetComponentInChildren<CardData>();
        
        if (cardData != null)
            cardData.InitEffects(scoreManager, gameMaster, this);
        // 1. 생성된 카드의 위치를 상점 카드의 위치로 맞춤
        
        //구매 후 이동
        data.gameObject.transform.position = data.transform.position;
        data.gameObject.transform.DOMove(new Vector3(10f, -3f, -50f), 0.3f).SetEase(Ease.OutCubic);
        gameMaster.musicManager.PlayBuyBuffSFX();


        //        Destroy(data.gameObject);

        // 구매 카드 리스트에만 추가
        purchasedCardList.Add(cardId);

        // 카드 구매 후 덱 갱신
        SetDeckSuffle();

        // 코인 UI 갱신
        var gm = FindObjectOfType<GameMaster>();
        if (gm != null && gm.uiManager != null)
            gm.uiManager.UpdateCoinUI(coin);

        var ui = gameMaster.GetComponent<UIManager>();
        if (ui != null)
            ui.UpdateDeckCountUI(deckList.Count, totalDeckCount);

        Debug.Log($"카드 구매 완료: {cardId} (구매카드리스트에 추가됨, 현재 구매카드: {string.Join(",", purchasedCardList)})");
    }

    public void OnClickSortAndArrangeBuffCardsFromGameMaster()
    {
        if (gameMaster != null && gameMaster.musicManager != null)
            gameMaster.musicManager.PlayUIClickSFX();

        if (gameMaster == null)
            gameMaster = FindObjectOfType<GameMaster>();

        if (gameMaster == null || gameMaster.buffManager == null)
            return;

        var buffManager = gameMaster.buffManager;
        var buffCardList = buffManager.buffCardList;
        if (buffCardList == null || buffCardList.Count == 0)
            return;

        buffCardList.Sort((a, b) =>
        {
            if (a == null || b == null) return 0;
            var dataA = a.GetComponentInChildren<CardData>();
            var dataB = b.GetComponentInChildren<CardData>();
            int idA = dataA != null ? dataA.cardId : 0;
            int idB = dataB != null ? dataB.cardId : 0;
            return idA.CompareTo(idB);
        });

        buffManager.RearrangeBuffCards();
    }

    public void OnClickSortDrawnCardsByCharacterType()
    {
        if (gameMaster == null)
            gameMaster = FindObjectOfType<GameMaster>();

        var setProcessStateField = typeof(GameMaster).GetField("setProcessState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (setProcessStateField != null)
        {
            var setProcessStateValue = setProcessStateField.GetValue(gameMaster);
            if (setProcessStateValue == null || setProcessStateValue.ToString() != "Idle")
                return;
        }

        if (gameMaster != null && gameMaster.musicManager != null)
            gameMaster.musicManager.PlayUIClickSFX();

        foreach (var card in new List<GameObject>(checkCardList))
        {
            if (card == null) continue;
            var cardState = card.GetComponent<CardState>();
            var cardData = card.GetComponent<CardData>();
            if (cardState != null && cardData != null)
            {
                cardState.ResetCardPosition();
                cardState.isClick = false;
                RemoveCheckCardList(cardState, cardData);
            }
        }
        checkCardList.Clear();

        foreach (var cardObj in playCardList)
        {
            var data = cardObj != null ? cardObj.GetComponentInChildren<CardData>() : null;
            Debug.Log(data != null ? data.characterType.ToString() : "No CardData");
        }

        playCardList.Sort((a, b) =>
        {
            if (a == null || b == null) return 0;
            var dataA = a.GetComponentInChildren<CardData>();
            var dataB = b.GetComponentInChildren<CardData>();
            if (dataA == null || dataB == null) return 0;
            return dataA.characterType.CompareTo(dataB.characterType);
        });

        foreach (var cardObj in playCardList)
        {
            var data = cardObj != null ? cardObj.GetComponentInChildren<CardData>() : null;
            Debug.Log(data != null ? data.characterType.ToString() : "No CardData");
        }

        int countAll = playCardList.Count;
        float spacing = countAll > 1 ? (maxX - minX) / (countAll - 1) : 0f;

        for (int i = 0; i < countAll; i++)
        {
            Debug.Log($"[CardManager] 정렬 후 카드 {i} 위치: {minX + spacing * i}, {baseZ + zStep * i}");
            var cardObj = playCardList[i];
            if (cardObj == null) continue;
            float x = (countAll == 1) ? (minX + maxX) / 2f : minX + spacing * i;
            float z = baseZ + zStep * i;
            cardObj.transform.DOMove(new Vector3(x, y, z), 0.1f).SetEase(Ease.OutCubic);
        }
    }

    public void OnClickSortDrawnCards()
    {
        if (gameMaster == null)
            gameMaster = FindObjectOfType<GameMaster>();

        var setProcessStateField = typeof(GameMaster).GetField("setProcessState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (setProcessStateField != null)
        {
            var setProcessStateValue = setProcessStateField.GetValue(gameMaster);
            if (setProcessStateValue == null || setProcessStateValue.ToString() != "Idle")
                return;
        }

        if (gameMaster != null && gameMaster.musicManager != null)
            gameMaster.musicManager.PlayUIClickSFX();

        foreach (var card in new List<GameObject>(checkCardList))
        {
            if (card == null) continue;
            var cardState = card.GetComponent<CardState>();
            var cardData = card.GetComponent<CardData>();
            if (cardState != null && cardData != null)
            {
                cardState.isClick = false;
                cardState.ResetCardPosition();
                RemoveCheckCardList(cardState, cardData);
            }
        }
        checkCardList.Clear();

        playCardList.Sort((a, b) =>
        {
            if (a == null || b == null) return 0;
            var dataA = a.GetComponentInChildren<CardData>();
            var dataB = b.GetComponentInChildren<CardData>();
            int idA = dataA != null ? dataA.cardId : 0;
            int idB = dataB != null ? dataB.cardId : 0;
            return idA.CompareTo(idB);
        });

        int countAll = playCardList.Count;
        float spacing = countAll > 1 ? (maxX - minX) / (countAll - 1) : 0f;

        for (int i = 0; i < countAll; i++)
        {
            var cardObj = playCardList[i];
            if (cardObj == null) continue;
            float x = (countAll == 1) ? (minX + maxX) / 2f : minX + spacing * i;
            float z = baseZ + zStep * i;
            cardObj.transform.DOMove(new Vector3(x, y, z), 0.1f).SetEase(Ease.OutCubic);
        }
    }

    public void DrawCards(int count)
    {
        if (gameMaster != null && gameMaster.musicManager != null)
            gameMaster.musicManager.PlayFlipCardSFX();

        drawCardList.Clear();
        int drawCount = Mathf.Min(count, deckList.Count);
        if (drawCount == 0) return;

        for (int i = 0; i < drawCount; i++)
        {
            int cardId = deckList[0];
            deckList.RemoveAt(0);

            if (isNoiStage && Random.value < 0.25f)
            {
                cardId = 59;
                if (gameMaster != null && gameMaster.musicManager != null)
                    gameMaster.musicManager.PlayFlipCardSFX();
            }

            drawCardList.Add(cardId);
        }

        List<GameObject> spawnedCards = cardSpawner.SpawnCardsByID(drawCardList);
        playCardList.AddRange(spawnedCards);

        int countAll = playCardList.Count;
        float spacing = countAll > 1 ? (maxX - minX) / (countAll - 1) : 0f;

        for (int i = 0; i < countAll; i++)
        {
            var cardObj = playCardList[i];
            if (cardObj == null) continue;
            float x = (countAll == 1) ? (minX + maxX) / 2f : minX + spacing * i;
            float z = baseZ + zStep * i;
            cardObj.transform.position = new Vector3(7.8f, -3.2f, -100f);
            cardObj.transform.DOMove(new Vector3(x, y, z), 0.1f).SetEase(Ease.OutCubic);
        }

        if (isRazStage)
        {
            Sprite backSprite = Resources.Load<Sprite>("Vlup/CardBack");
            if (backSprite != null)
            {
                foreach (var cardObj in spawnedCards)
                {
                    if (gameMaster != null && gameMaster.musicManager != null)
                        gameMaster.musicManager.PlayFlipCardSFX();

                    if (cardObj == null) continue;
                    if (Random.value < 0.5f)
                    {
                        Transform dataTr = cardObj.transform.Find("Data");
                        if (dataTr == null) continue;
                        Transform bgTr = dataTr.Find("Background");
                        if (bgTr == null) continue;
                        var sr = bgTr.GetComponent<SpriteRenderer>();
                        if (sr != null)
                            sr.sprite = backSprite;

                        var cardState = cardObj.GetComponentInChildren<CardState>();
                        if (cardState != null)
                        {
                            cardState.cardInfo = "ㅋㅋㅋㅋㅋ";
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("[CardManager] Vlup/CardBack 스프라이트를 찾을 수 없습니다.");
            }
        }
        InitDrawnCards();

        if (gameMaster != null)
        {
            var ui = gameMaster.GetComponent<UIManager>();
            if (ui != null)
                ui.UpdateDeckCountUI(deckList.Count, totalDeckCount);
        }
    }

    private void HandleCardHover()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            CardState state = hit.collider.GetComponent<CardState>();

            if (state != null)
            {
                if (lastHoveredCard != null && lastHoveredCard != state)
                {
                    lastHoveredCard.HideHoverUI();
                }

                state.ShowHoverUI();
                lastHoveredCard = state;
                return;
            }
        }

        if (lastHoveredCard != null)
        {
            lastHoveredCard.HideHoverUI();
            lastHoveredCard = null;
        }
    }

    public void OnclickCollectionGuide()
    {
        if (gameMaster != null && gameMaster.musicManager != null)
            gameMaster.musicManager.PlayUIClickSFX();

        // 모든 가이드 오브젝트 비활성화
        for (int i = 0; i < collectionGuideList.Length; i++)
        {
            if (collectionGuideList[i] != null)
            {
                collectionGuideList[i].SetActive(false);
                collectionGuideList[i].transform.position = new Vector3(19f, 0f, 0f);
            }
        }

        // 다음 가이드 인덱스 계산
        if (currentGuideIndex < collectionGuideList.Length - 1)
        {
            currentGuideIndex++;
            var guide = collectionGuideList[currentGuideIndex];
            if (guide != null)
            {
                guide.SetActive(true);
                guide.transform.position = new Vector3(0f, 0f, -150f);
            }

            // 첫 가이드 진입 시 상태 저장 및 Option 상태로 전환, otherUIObjects 비활성화
            if (currentGuideIndex == 0)
            {
                nextButton.SetActive(true);
                prevState = gameMaster.gameState;
                gameMaster.gameState = GameMaster.GameState.Option;
                if (otherUIObjects != null)
                    otherUIObjects.SetActive(false);
            }
        }
        else
        {
            // 마지막 가이드에서 버튼을 누르면 모두 비활성화 및 상태 복귀, otherUIObjects 활성화
            currentGuideIndex = -1;
            gameMaster.gameState = prevState;
            if (otherUIObjects != null)
                otherUIObjects.SetActive(true);
            nextButton.SetActive(false);
        }
    }


    public void OnClickCreateDeckListObjects()
    {
        if (gameMaster != null && gameMaster.musicManager != null)
            gameMaster.musicManager.PlayUIClickSFX();

        if (deckListParent == null || cardSpawner == null || deckList == null || deckList.Count == 0)
            return;

        // 덱리스트 열기
        if (!isDeckListAtOrigin)
        {
            prevState = gameMaster.gameState; // 현재 상태 저장
            gameMaster.gameState = GameMaster.GameState.Option;

            deckListParent.SetActive(true);
            deckListParent.transform.position = new Vector3(0f, 0f, -150f);
            if (otherUIObjects != null)
                otherUIObjects.SetActive(false);
        }
        else
        {
            gameMaster.gameState = prevState; // 이전 상태로 복귀

            deckListParent.SetActive(false);
            deckListParent.transform.position = new Vector3(19f, 0f, 0f);
            if (otherUIObjects != null)
                otherUIObjects.SetActive(true);
        }

        isDeckListAtOrigin = !isDeckListAtOrigin;
        foreach (Transform child in deckListParent.transform)
        {
            if (child.name.Contains("Clone"))
                Destroy(child.gameObject);
        }

        List<GameObject> spawnedCards = cardSpawner.SpawnCardsByID(deckList);
        Debug.Log($"[CardManager] DeckList 카드 생성: {spawnedCards.Count}장, deckList: {string.Join(",", deckList)}");
        spawnedCards.Sort((a, b) =>
        {
            var dataA = a.GetComponentInChildren<CardData>();
            var dataB = b.GetComponentInChildren<CardData>();
            int idA = dataA != null ? dataA.cardId : 0;
            int idB = dataB != null ? dataB.cardId : 0;
            return idA.CompareTo(idB);
        });

        foreach (var cardObj in spawnedCards)
        {
            if (cardObj == null) continue;
            cardObj.transform.SetParent(deckListParent.transform, false);

            foreach (var collider in cardObj.GetComponentsInChildren<Collider>(true))
                collider.enabled = false;
            foreach (var collider2D in cardObj.GetComponentsInChildren<Collider2D>(true))
                collider2D.enabled = false;
        }

        float startX = -5.9f;
        float endX = 5.9f;
        float startY = 3.5f;
        float endY = -3.5f;
        int maxPerRow = 10;
        int count = spawnedCards.Count;

        int rowCount = Mathf.CeilToInt((float)count / maxPerRow);
        int minCardsPerRow = count / rowCount;
        int extra = count % rowCount;

        float yStep = (rowCount > 1) ? (startY - endY) / (rowCount - 1) : 0f;
        float z = -1f;
        float zStep = -0.1f;
        int cardIdx = 0;

        for (int row = 0; row < rowCount; row++)
        {
            int cardsInThisRow = minCardsPerRow + (row < extra ? 1 : 0);
            float xStep = (cardsInThisRow > 1) ? (endX - startX) / (cardsInThisRow - 1) : 0f;
            float y = (rowCount == 1) ? (startY + endY) / 2f : startY - yStep * row;

            for (int col = 0; col < cardsInThisRow; col++)
            {
                if (cardIdx >= count) break;
                float x = (cardsInThisRow == 1) ? (startX + endX) / 2f : startX + xStep * col;
                Vector3 pos = new Vector3(x, y, z);
                spawnedCards[cardIdx].transform.localPosition = pos;
                z += zStep;
                cardIdx++;
            }
        }
    }
    public void SetDeckSuffle()
    {
        deckList.Clear();
        if (isIanaStage)
        {
            for (int i = 1; i <= deckCount; i++)
                deckList.Add(i);
            for (int i = deckCount + 1; i <= deckCount+84; i++)
                deckList.Add(58);
        }
        else
        {
            for (int i = 1; i <= deckCount; i++)
                deckList.Add(i);
        }

        // 구매한 카드 추가
        if (purchasedCardList != null && purchasedCardList.Count > 0)
        {
            deckList.AddRange(purchasedCardList);
        }

        totalDeckCount = deckList.Count;

        for (int i = 0; i < deckList.Count; i++)
        {
            int randIndex = Random.Range(i, deckList.Count);
            (deckList[i], deckList[randIndex]) = (deckList[randIndex], deckList[i]);
        }

        if (gameMaster != null)
        {
            var ui = gameMaster.GetComponent<UIManager>();
            if (ui != null)
                ui.UpdateDeckCountUI(deckList.Count, totalDeckCount);
        }
    }

    public void SetCardList()
    {
        Debug.Log("=== SetCardList 시작 ===");

        SetDeckSuffle();
        Debug.Log($"[SetCardList] deckList.Count(셔플 후): {deckList.Count}");

        checkCardList.Clear();
        dropCardList.Clear();
        drawCardList.Clear();

        if (selectCardList != null)
        {
            foreach (var card in selectCardList)
            {
                if (card != null)
                    Destroy(card);
            }
        }
        selectCardList = new List<GameObject>(new GameObject[7]);

        if (playCardList != null)
        {
            foreach (var card in playCardList)
            {
                if (card != null)
                    Destroy(card);
            }
        }
        playCardList = new List<GameObject>();

        Debug.Log($"[SetCardList] playCardList.Count(초기화 후): {playCardList.Count}");

        if (gameMaster != null)
        {
            var ui = gameMaster.GetComponent<UIManager>();
            if (ui != null)
                ui.UpdateDeckCountUI(deckList.Count, TotalDeckCount);
        }

        Debug.Log($"[SetCardList] drawCardList.Count: {drawCardList.Count}");
        Debug.Log($"[SetCardList] dropCardList.Count: {dropCardList.Count}");
        Debug.Log($"[SetCardList] checkCardList.Count: {checkCardList.Count}");
        Debug.Log($"[SetCardList] selectCardList.Count: {selectCardList.Count}");
        Debug.Log("=== SetCardList 끝 ===");
    }

    public int TotalDeckCount
    {
        get
        {
            return deckList.Count + playCardList.Count + drawCardList.Count + dropCardList.Count;
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
            float offsetZ = z - (i * 0.1f);
            Vector3 newPos = new Vector3(minX + spacing * i, y, offsetZ);
            activeCards[i].transform.position = newPos;
        }
    }

    public bool IsInPlayCardList(GameObject card)
    {
        if (card == null)
            return false;

        GameObject parent = card.transform.parent != null ? card.transform.parent.gameObject : card;
        return playCardList.Contains(parent);
    }

    public void ClickCard()
    {
        if (gameMaster == null)
            gameMaster = FindObjectOfType<GameMaster>();

        var setProcessStateField = typeof(GameMaster).GetField("setProcessState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (setProcessStateField != null)
        {
            var setProcessStateValue = setProcessStateField.GetValue(gameMaster);
            if (setProcessStateValue == null || setProcessStateValue.ToString() != "Idle")
                return;
        }

        if (!Input.GetMouseButtonDown(0)) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            CardState cardStateRead = hit.collider.GetComponent<CardState>();
            CardData cardDataRead = hit.collider.GetComponent<CardData>();

            if (cardDataRead != null && cardDataRead.cardType == CardData.CardType.Buff)
            {
                Debug.Log($"[CardManager] 버프카드 클릭 무시됨: {cardDataRead.cardName}");
                return;
            }

            if (cardStateRead != null)
            {
                cardStateRead.CardStateClicked();
                AddCheckCardList(cardStateRead, cardDataRead);
            }
        }
    }

    public void AddCheckCardList(CardState cardStateRead, CardData cardDataRead)
    {
        if (gameMaster != null && gameMaster.musicManager != null)
            gameMaster.musicManager.PlayCardSelectSFX();

        if (cardStateRead == null || cardDataRead == null) return;

        GameObject card = cardDataRead.gameObject;
        GameObject parentCard = card.transform.parent.gameObject;

        if (cardStateRead.isClick)
        {
            if (checkCardList.Count >= 7)
                return;

            if (!checkCardList.Contains(card))
                checkCardList.Add(card);

            bool removed = false;
            if (playCardList.Contains(parentCard))
            {
                playCardList.Remove(parentCard);
                removed = true;
            }

            if (removed)
                ArrangeCardsWithinRange(true);

            // === 카드와 Background의 y축을 0으로 이동 ===
            // 카드 오브젝트
            Vector3 cardPos = parentCard.transform.position;
            parentCard.transform.position = new Vector3(cardPos.x, 0f, cardPos.z);

            if (!selectCardList.Contains(parentCard))
            {
                int emptyIdx = selectCardList.FindIndex(obj => obj == null);
                if (emptyIdx != -1)
                {
                    // Background 오브젝트
                    Transform bg = parentCard.transform.Find("Data/Background");
                    if (bg != null)
                    {
                        Vector3 bgPos = bg.position;
                        bg.position = new Vector3(bgPos.x, 0f, bgPos.z);
                    }
                    // =========================================

                    selectCardList[emptyIdx] = parentCard;
                    Vector3 targetPos = GetSelectCardPosition(emptyIdx);
                    parentCard.transform.DOMove(targetPos, 0.3f).SetEase(Ease.OutCubic);
                }
            }
        }
        else
        {
            RemoveCheckCardList(cardStateRead, cardDataRead);
        }

        List<string> cardInfoList = new List<string>();
        foreach (var obj in checkCardList)
        {
            if (obj == null) continue;
            CardData cd = obj.GetComponent<CardData>();
            if (cd != null)
                cardInfoList.Add($"{cd.cardId}({obj.name})");
        }
        Debug.Log($"[CardManager] checkCardList: {string.Join(", ", cardInfoList)}");

        var combos = GetCompletedCollectorCombos(scoreManager);
        if (combos.Count > 0)
        {
            foreach (var combo in combos)
            {
                Debug.Log($"[CardManager] 콜렉터 조합 완성: {combo.collectorName} (소유: {combo.ownedCount}장, 점수: {combo.score})");
            }
        }
        else
        {
            Debug.Log("[CardManager] 현재 완성된 콜렉터 조합 없음");
        }

        if (gameMaster != null && gameMaster.uiManager != null)
        {
            gameMaster.uiManager.UpdateCollectorResultUI(combos);
            gameMaster.uiManager.UpdateScoreCalUI(
                scoreManager.rate, scoreManager.scoreYet, scoreManager.resultScore
            );
        }
    }

    // CardManager 클래스 내부에 추가
    private void SortCheckCardListById()
    {
        checkCardList.Sort((a, b) =>
        {
            var dataA = a != null ? a.GetComponent<CardData>() : null;
            var dataB = b != null ? b.GetComponent<CardData>() : null;
            int idA = dataA != null ? dataA.cardId : 0;
            int idB = dataB != null ? dataB.cardId : 0;
            return idA.CompareTo(idB);
        });
    }

    public void RemoveCheckCardList(CardState cardStateRead, CardData cardDataRead)
    {
        if (gameMaster != null && gameMaster.musicManager != null)
            gameMaster.musicManager.PlayCardRemoveSFX();

        if (cardStateRead == null || cardDataRead == null) return;

        GameObject card = cardDataRead.gameObject;
        GameObject parentCard = card.transform.parent.gameObject;

        if (checkCardList.Contains(card))
            checkCardList.Remove(card);

        if (!playCardList.Contains(parentCard))
            playCardList.Add(parentCard);

        int idx = selectCardList.IndexOf(parentCard);
        if (idx != -1)
        {
            selectCardList[idx] = null;
        }

        // === 카드 오브젝트의 y축을 -3.2로, Background는 0으로 이동 ===
        Vector3 cardPos = parentCard.transform.position;
        parentCard.transform.position = new Vector3(cardPos.x, -3.2f, cardPos.z);
        Transform bg = parentCard.transform.Find("Background");
        if (bg != null)
        {
            Vector3 bgPos = bg.position;
            bg.position = new Vector3(bgPos.x, 0f, bgPos.z);
        }
        // ================================================

        ArrangeCardsWithinRange(true);

        List<string> cardInfoList = new List<string>();
        foreach (var obj in checkCardList)
        {
            if (obj == null) continue;
            CardData cd = obj.GetComponent<CardData>();
            if (cd != null)
                cardInfoList.Add($"{cd.cardId}({obj.name})");
        }
        Debug.Log($"[CardManager] checkCardList: {string.Join(", ", cardInfoList)}");

        var combos = GetCompletedCollectorCombos(scoreManager);
        if (combos.Count > 0)
        {
            foreach (var combo in combos)
            {
                Debug.Log($"[CardManager] 콜렉터 조합 완성: {combo.collectorName} (소유: {combo.ownedCount}장, 점수: {combo.score})");
            }
        }
        else
        {
            Debug.Log("[CardManager] 현재 완성된 콜렉터 조합 없음");
        }

        if (gameMaster != null && gameMaster.uiManager != null)
            gameMaster.uiManager.UpdateCollectorResultUI(combos);
    }

    public void ArrangeCardsWithinRange(bool useTween = false)
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
            Vector3 targetPos = new Vector3(x, -3.2f, z);
            if (useTween)
                activeCards[i].transform.DOMove(targetPos, 0.3f).SetEase(Ease.OutCubic);
            else
                activeCards[i].transform.position = targetPos;
        }
    }

    public void DropAndDrawSelectedCards()
    {
        int dropCount = checkCardList.Count;
        if (dropCount == 0) return;

        foreach (GameObject card in checkCardList)
        {
            dropCardList.Add(card);
            GameObject parentCard = card.transform.parent.gameObject;
            playCardList.Remove(parentCard);

            Vector3 targetPos = parentCard.transform.position + new Vector3(10f, 0f, 0f);
            parentCard.transform.DOMove(targetPos, 0.3f).SetEase(Ease.InCubic)
                .OnComplete(() => Destroy(parentCard));
        }
        checkCardList.Clear();

        int originalDrawEa = drawCardEa;
        drawCardEa = dropCount;

        if (!isLimiStage || (isLimiStage && Random.value < 0.5f))
        {
            DrawCards(drawCardEa);
        }

        drawCardEa = originalDrawEa;
    }

    public void InitDrawnCards()
    {
        foreach (var cardObj in playCardList)
        {
            if (cardObj == null) continue;
            var cardData = cardObj.GetComponentInChildren<CardData>();
            if (cardData != null)
            {
                cardData.InitEffects(scoreManager, gameMaster, this);
            }
            else
            {
                Debug.LogWarning($"[CardManager] CardData를 찾을 수 없음: {cardObj.name}");
            }
        }
    }

    public Vector3 GetSelectCardPosition(int index)
    {
        if (index < 0 || index >= selectCardXPositions.Length)
            return Vector3.zero;
        return new Vector3(selectCardXPositions[index], 0, selectCardZ);
    }

    public List<CollectorComboResult> GetCompletedCollectorCombos(ScoreManager scoreManager)
    {
        List<CollectorComboResult> results = new List<CollectorComboResult>();
        if (checkCardList == null || checkCardList.Count == 0)
            return results;

        List<int> ownedCardIds = new List<int>();
        foreach (var card in checkCardList)
        {
            if (card == null) continue;
            CardData cardData = card.GetComponent<CardData>();
            if (cardData != null)
                ownedCardIds.Add(cardData.cardId);
        }

        var matchedScores = scoreManager.GetMatchedCollectorScores(ownedCardIds);

        foreach (var kv in matchedScores)
        {
            string collectorName = kv.Key;
            int score = kv.Value;
            int ownedCount = scoreManager.collectors[collectorName].FindAll(id => ownedCardIds.Contains(id)).Count;
            results.Add(new CollectorComboResult
            {
                collectorName = collectorName,
                ownedCount = ownedCount,
                score = score
            });
        }
        return results;
    }

    public bool DrawRandomTargetCard(int[] targetIds)
    {
        // 1/2 확률로만 동작
        if (Random.value >= 0.5f)
            return false;

        // deckList에서 targetIds에 해당하는 카드만 추출
        List<int> candidates = new List<int>();
        foreach (int id in deckList)
        {
            if (System.Array.Exists(targetIds, t => t == id))
                candidates.Add(id);
        }

        if (candidates.Count == 0)
            return false;

        // 랜덤으로 한 장 선택
        int selectedId = candidates[Random.Range(0, candidates.Count)];

        // 카드 생성 및 playCardList에 추가
        List<GameObject> spawned = cardSpawner.SpawnCardsByID(new List<int> { selectedId });
        if (spawned.Count > 0)
        {
            playCardList.Add(spawned[0]);
            // 카드 배치 (애니메이션 포함)
            int countAll = playCardList.Count;
            float spacing = countAll > 1 ? (maxX - minX) / (countAll - 1) : 0f;
            for (int i = 0; i < countAll; i++)
            {
                var cardObj = playCardList[i];
                if (cardObj == null) continue;
                float x = (countAll == 1) ? (minX + maxX) / 2f : minX + spacing * i;
                float z = baseZ + zStep * i;
                cardObj.transform.position = new Vector3(7.8f, -3.2f, -100f);
                cardObj.transform.DOMove(new Vector3(x, y, z), 0.1f).SetEase(Ease.OutCubic);
            }
            if (gameMaster != null && gameMaster.musicManager != null)
                gameMaster.musicManager.PlayFlipCardSFX();

            if (gameMaster != null)
            {
                var ui = gameMaster.GetComponent<UIManager>();
                if (ui != null)
                    ui.UpdateDeckCountUI(deckList.Count, totalDeckCount);
            }
            return true;
        }
        return false;
    }
}
public struct CollectorComboResult
{
    public string collectorName;
    public int ownedCount;
    public int score;
}