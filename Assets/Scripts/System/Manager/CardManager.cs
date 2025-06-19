using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardManager : MonoBehaviour
{
    public List<int> deckList = new List<int>();
    public List<int> drawCardList = new List<int>();
    public List<GameObject> playCardList = new List<GameObject>();
    public List<GameObject> checkCardList = new List<GameObject>();
    public List<GameObject> dropCardList = new List<GameObject>();
    public List<GameObject> selectCardList = new List<GameObject>();

    public CardSpawner cardSpawner;
    public ScoreManager scoreManager; // �ν����Ϳ��� ���� �Ǵ� FindObjectOfType ���
    public GameObject otherUIObjects; // ��Ȱ��ȭ/Ȱ��ȭ�� ������ UI ������Ʈ��(�ν����Ϳ��� �Ҵ�)

    public GameObject deckListParent; // DeckList ������Ʈ
    private bool isDeckListAtOrigin = false;


    // ī�� ��ġ ��ǥ
    public float minX = -4.5f;
    public float maxX = 5.9f;
    public float y = -3.2f;
    public float baseZ = -1f;
    public float zStep = 0.1f;

    public int defaultDrawCardEa = 10; // ���� ���� �� ��ο� ���
    public int drawCardEa = 1;

    // Hover UI ���� �ʵ�
    private CardState lastHoveredCard;
    private GameMaster gameMaster;

    private static readonly float[] selectCardXPositions = { -4.5f, -2.7f, -0.9f, 0.9f, 2.7f, 4.5f, 6.3f };
    private const float selectCardY = 0f;
    private const float selectCardZ = -0.1f;

    public int totalDeckCount; // �� ���(����)

    // �������� ���� ���� (public)
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

    /// <summary>
    /// �������� ���� ���� �ʱ�ȭ �� ���� ���������� �°� �缳��
    /// </summary>
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
        // �ʿ��ϴٸ� ī�� ���� �ٸ� Update �ڵ� �߰�
    }

    public void DrawInitialCards()
    {
        DrawCards(defaultDrawCardEa);
    }

    public void OnClickSortDrawnCardsByCharacterType()
    {
        // Idle ���°� �ƴ� ���� �������� ����
        if (gameMaster == null)
            gameMaster = FindObjectOfType<GameMaster>();

        var setProcessStateField = typeof(GameMaster).GetField("setProcessState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (setProcessStateField != null)
        {
            var setProcessStateValue = setProcessStateField.GetValue(gameMaster);
            if (setProcessStateValue == null || setProcessStateValue.ToString() != "Idle")
                return;
        }

        // SFX ���
        if (gameMaster != null && gameMaster.musicManager != null)
            gameMaster.musicManager.PlayUIClickSFX();

        // checkCardList�� �ִ� ��� ī�� ���� ���� �� ���� �ڸ��� �̵�
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

        // ���� �� characterType �� ����� ���
        foreach (var cardObj in playCardList)
        {
            var data = cardObj != null ? cardObj.GetComponentInChildren<CardData>() : null;
            Debug.Log(data != null ? data.characterType.ToString() : "No CardData");
        }

        // CardData.characterType(enum) �������� playCardList ����
        playCardList.Sort((a, b) =>
        {
            if (a == null || b == null) return 0;
            var dataA = a.GetComponentInChildren<CardData>();
            var dataB = b.GetComponentInChildren<CardData>();
            if (dataA == null || dataB == null) return 0;
            return dataA.characterType.CompareTo(dataB.characterType);
        });

        // ���� �� characterType �� ����� ���
        foreach (var cardObj in playCardList)
        {
            var data = cardObj != null ? cardObj.GetComponentInChildren<CardData>() : null;
            Debug.Log(data != null ? data.characterType.ToString() : "No CardData");
        }

        // ���ĵ� ������� ��ġ ���ġ (�ִϸ��̼� ����)
        int countAll = playCardList.Count;
        float spacing = countAll > 1 ? (maxX - minX) / (countAll - 1) : 0f;

        for (int i = 0; i < countAll; i++)
        {
            Debug.Log($"[CardManager] ���� �� ī�� {i} ��ġ: {minX + spacing * i}, {baseZ + zStep * i}");
            var cardObj = playCardList[i];
            if (cardObj == null) continue;
            float x = (countAll == 1) ? (minX + maxX) / 2f : minX + spacing * i;
            float z = baseZ + zStep * i;
            cardObj.transform.DOMove(new Vector3(x, y, z), 0.4f).SetEase(Ease.OutCubic);
        }
    }

    public void OnClickSortDrawnCards()
    {
        // Idle ���°� �ƴ� ���� �������� ����
        if (gameMaster == null)
            gameMaster = FindObjectOfType<GameMaster>();

        var setProcessStateField = typeof(GameMaster).GetField("setProcessState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (setProcessStateField != null)
        {
            var setProcessStateValue = setProcessStateField.GetValue(gameMaster);
            if (setProcessStateValue == null || setProcessStateValue.ToString() != "Idle")
                return;
        }

        // SFX ���
        if (gameMaster != null && gameMaster.musicManager != null)
            gameMaster.musicManager.PlayUIClickSFX();

        // checkCardList�� �ִ� ��� ī�� ���� ���� �� ���� �ڸ��� �̵�
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

        // cardID �������� playCardList ����
        playCardList.Sort((a, b) =>
        {
            if (a == null || b == null) return 0;
            var dataA = a.GetComponentInChildren<CardData>();
            var dataB = b.GetComponentInChildren<CardData>();
            int idA = dataA != null ? dataA.cardId : 0;
            int idB = dataB != null ? dataB.cardId : 0;
            return idA.CompareTo(idB);
        });

        // ���ĵ� ������� ��ġ ���ġ (�ִϸ��̼� ����)
        int countAll = playCardList.Count;
        float spacing = countAll > 1 ? (maxX - minX) / (countAll - 1) : 0f;

        for (int i = 0; i < countAll; i++)
        {
            var cardObj = playCardList[i];
            if (cardObj == null) continue;
            float x = (countAll == 1) ? (minX + maxX) / 2f : minX + spacing * i;
            float z = baseZ + zStep * i;
            cardObj.transform.DOMove(new Vector3(x, y, z), 0.4f).SetEase(Ease.OutCubic);
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

            // Noi ��������: 1/4 Ȯ���� 59�� ī��� ����
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

        // ī�� ��ġ (DOTween ����, y�� -3.2�� ����)
        int countAll = playCardList.Count;
        float spacing = countAll > 1 ? (maxX - minX) / (countAll - 1) : 0f;

        for (int i = 0; i < countAll; i++)
        {
            var cardObj = playCardList[i];
            if (cardObj == null) continue;
            float x = (countAll == 1) ? (minX + maxX) / 2f : minX + spacing * i;
            float z = baseZ + zStep * i;

            // ī�� ������ ���� ��ġ�� 7.8, -3.2, -1�� ����
            cardObj.transform.position = new Vector3(7.8f, -3.2f, -1f);
            cardObj.transform.DOMove(new Vector3(x, y, z), 0.4f).SetEase(Ease.OutCubic);
        }

        // Raz ��������: 1/2 Ȯ���� ī�� �޸� ��������Ʈ�� ����
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
                            cardState.cardInfo = "����������";
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("[CardManager] Vlup/CardBack ��������Ʈ�� ã�� �� �����ϴ�.");
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


    /// <summary>
    /// DeckList ������Ʈ�� �ڽ����� deckList�� ī�� ������Ʈ�� �����ϰ� X=-2.2~11.3�� �յ� ��ġ
    /// </summary>
    /// <summary>
    /// DeckList ������Ʈ�� �ڽ����� deckList�� ī�� ������Ʈ�� �����ϰ� X=-2.2~11.3�� �յ� ��ġ
    /// </summary>
    public void OnClickCreateDeckListObjects()
    {
        if (gameMaster != null && gameMaster.musicManager != null)
            gameMaster.musicManager.PlayUIClickSFX();

        if (deckListParent == null || cardSpawner == null || deckList == null || deckList.Count == 0)
            return;

        // ��ġ ��� (���� ��ǥ)
        if (!isDeckListAtOrigin)
        {
            deckListParent.SetActive(true);
            deckListParent.transform.position = new Vector3(0f, 0f, -2f);

            if (otherUIObjects != null)
                otherUIObjects.SetActive(false);
        }
        else
        {
            deckListParent.SetActive(false);
            deckListParent.transform.position = new Vector3(19f, 0f, 0f);

            if (otherUIObjects != null)
                otherUIObjects.SetActive(true);
        }

        isDeckListAtOrigin = !isDeckListAtOrigin;

        // "Clone"�� ���Ե� �ڽĸ� ����
        foreach (Transform child in deckListParent.transform)
        {
            if (child.name.Contains("Clone"))
                Destroy(child.gameObject);
        }

        // ī�� ���� (SpawnCardsByID ���, �θ�� deckListParent�� ���� ����)
        List<GameObject> spawnedCards = cardSpawner.SpawnCardsByID(deckList);

        // cardId �������� ����
        spawnedCards.Sort((a, b) =>
        {
            var dataA = a.GetComponentInChildren<CardData>();
            var dataB = b.GetComponentInChildren<CardData>();
            int idA = dataA != null ? dataA.cardId : 0;
            int idB = dataB != null ? dataB.cardId : 0;
            return idA.CompareTo(idB);
        });

        // �θ�(DeckList)�� �̵� �� �ݶ��̴� ��Ȱ��ȭ
        foreach (var cardObj in spawnedCards)
        {
            if (cardObj == null) continue;
            cardObj.transform.SetParent(deckListParent.transform, false);

            // ��� Collider, Collider2D ��Ȱ��ȭ
            foreach (var collider in cardObj.GetComponentsInChildren<Collider>(true))
                collider.enabled = false;
            foreach (var collider2D in cardObj.GetComponentsInChildren<Collider2D>(true))
                collider2D.enabled = false;
        }

        // X=-5.9 ~ 5.9, Y=-3.5 ~ 3.5, �� �ٿ� �ִ� 10��, �� �� �յ� ��ġ, Z�� -10���� +0.1�� ����
        float startX = -5.9f;
        float endX = 5.9f;
        float startY = 3.5f;
        float endY = -3.5f;
        int maxPerRow = 10;
        int count = spawnedCards.Count;

        // �յ� �й踦 ���� ��/�� ���
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
        // Iana ���������� 0�� ī��� 84�� ä��
        if (isIanaStage)
        {
            for (int i = 1; i <= 54; i++)
                deckList.Add(i);
            for (int i = 55; i < 85; i++)
                deckList.Add(58); // 0�� ī�� 84��
        }
        else
        {
            for (int i = 1; i <= 54; i++)
                deckList.Add(i);
        }

        totalDeckCount = deckList.Count; // ���� ���� �� ���� ����

        // ����
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
        SetDeckSuffle();

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
        selectCardList = new List<GameObject>(new GameObject[7]); // 7ĭ¥���� �ʱ�ȭ

        if (playCardList != null)
        {
            foreach (var card in playCardList)
            {
                if (card != null)
                    Destroy(card);
            }
        }
        playCardList = new List<GameObject>();

        if (gameMaster != null)
        {
            var ui = gameMaster.GetComponent<UIManager>();
            if (ui != null)
                ui.UpdateDeckCountUI(deckList.Count, TotalDeckCount);
        }
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
        // setProcessState�� Idle�� ���� ����
        if (gameMaster == null)
            gameMaster = FindObjectOfType<GameMaster>();

        // GameMaster.SetProcessState enum�� public�� �ƴϹǷ� string �� ���
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

            // Buff ī��� ����
            if (cardDataRead != null && cardDataRead.cardType == CardData.CardType.Buff)
            {
                Debug.Log($"[CardManager] ����ī�� Ŭ�� ���õ�: {cardDataRead.cardName}");
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

        // ����(Ŭ�� ����)�� �׻� ����ؾ� �ϹǷ�, ����(�߰�)�� ���� 7�� ����
        if (cardStateRead == null || cardDataRead == null) return;

        GameObject card = cardDataRead.gameObject;
        GameObject parentCard = card.transform.parent.gameObject;

        if (cardStateRead.isClick)
        {
            if (checkCardList.Count >= 7)
                return;

            if (!checkCardList.Contains(card))
                checkCardList.Add(card);

            // playCardList���� ���� (�ߺ� ���� ����)
            bool removed = false;
            if (playCardList.Contains(parentCard))
            {
                playCardList.Remove(parentCard);
                removed = true;
            }

            // ī�尡 playCardList���� ���ŵ� ���, ���� ī��� ��ġ ���ġ(DOTween)
            if (removed)
                ArrangeCardsWithinRange(true);

            if (!selectCardList.Contains(parentCard))
            {
                int emptyIdx = selectCardList.FindIndex(obj => obj == null);
                if (emptyIdx != -1)
                {
                    selectCardList[emptyIdx] = parentCard;
                    // DOTween���� ���� ��ġ�� �̵�
                    Vector3 targetPos = GetSelectCardPosition(emptyIdx);
                    parentCard.transform.DOMove(targetPos, 0.3f).SetEase(Ease.OutCubic);
                }
            }
        }
        else
        {
            RemoveCheckCardList(cardStateRead, cardDataRead);
        }

        // checkCardList ���� ���
        List<string> cardInfoList = new List<string>();
        foreach (var obj in checkCardList)
        {
            if (obj == null) continue;
            CardData cd = obj.GetComponent<CardData>();
            if (cd != null)
                cardInfoList.Add($"{cd.cardId}({obj.name})");
        }
        Debug.Log($"[CardManager] checkCardList: {string.Join(", ", cardInfoList)}");

        // ���� üũ �� UI ����
        var combos = GetCompletedCollectorCombos(scoreManager);
        if (combos.Count > 0)
        {
            foreach (var combo in combos)
            {

                Debug.Log($"[CardManager] �ݷ��� ���� �ϼ�: {combo.collectorName} (����: {combo.ownedCount}��, ����: {combo.score})");
            }
        }
        else
        {
            Debug.Log("[CardManager] ���� �ϼ��� �ݷ��� ���� ����");
        }

        if (gameMaster != null && gameMaster.uiManager != null)
        {
            gameMaster.uiManager.UpdateCollectorResultUI(combos);

            // ���� ��� UI ���� �߰�
            gameMaster.uiManager.UpdateScoreCalUI(
                scoreManager.rate, scoreManager.scoreYet, scoreManager.resultScore
            );
        }
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

        // playCardList�� �ٽ� �߰� (�ߺ� ����)
        if (!playCardList.Contains(parentCard))
            playCardList.Add(parentCard);

        int idx = selectCardList.IndexOf(parentCard);
        if (idx != -1)
        {
            selectCardList[idx] = null;
        }

        // playCardList ��ü ��ġ ���ġ (DOTween ����)
        ArrangeCardsWithinRange(true);

        // checkCardList ���� ���
        List<string> cardInfoList = new List<string>();
        foreach (var obj in checkCardList)
        {
            if (obj == null) continue;
            CardData cd = obj.GetComponent<CardData>();
            if (cd != null)
                cardInfoList.Add($"{cd.cardId}({obj.name})");
        }
        Debug.Log($"[CardManager] checkCardList: {string.Join(", ", cardInfoList)}");

        // ���� üũ �� UI ����
        var combos = GetCompletedCollectorCombos(scoreManager);
        if (combos.Count > 0)
        {
            foreach (var combo in combos)
            {
                Debug.Log($"[CardManager] �ݷ��� ���� �ϼ�: {combo.collectorName} (����: {combo.ownedCount}��, ����: {combo.score})");
            }
        }
        else
        {
            Debug.Log("[CardManager] ���� �ϼ��� �ݷ��� ���� ����");
        }

        if (gameMaster != null && gameMaster.uiManager != null)
            gameMaster.uiManager.UpdateCollectorResultUI(combos);
    }

    // DOTween �ִϸ��̼� ���� ����
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
            Vector3 targetPos = new Vector3(x, y, z);
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

            // DOTween���� X�� +10 �̵� �� Destroy
            Vector3 targetPos = parentCard.transform.position + new Vector3(10f, 0f, 0f);
            parentCard.transform.DOMove(targetPos, 0.3f).SetEase(Ease.InCubic)
                .OnComplete(() => Destroy(parentCard));
        }
        checkCardList.Clear();

        // Limi ���������� 1/2 Ȯ���θ� ��ο�
        int originalDrawEa = drawCardEa;
        drawCardEa = dropCount;

        if (!isLimiStage || (isLimiStage && Random.value < 0.5f))
        {
            DrawCards(drawCardEa);
            // DrawCards���� �̹� (7.8, -3.2, -1)���� DOTween �̵� ó����
        }

        drawCardEa = originalDrawEa;
        // ArrangeCardsWithinRange(); // ���ʿ�, DrawCards���� ��ġ �ִϸ��̼� ó����
    }

    public void InitDrawnCards()
    {
        // ���� �߰��� ī�� ��ü�� InitEffects ȣ��
        foreach (var cardObj in playCardList)
        {
            if (cardObj == null) continue;
            var cardData = cardObj.GetComponentInChildren<CardData>();
            if (cardData != null)
            {
                cardData.InitEffects(scoreManager, gameMaster, this);
                //Debug.Log($"[CardManager] InitEffects ȣ���: {cardData.cardName} (ID: {cardData.cardId}, ������Ʈ: {cardObj.name})");
            }
            else
            {
                Debug.LogWarning($"[CardManager] CardData�� ã�� �� ����: {cardObj.name}");
            }
        }
    }

    public Vector3 GetSelectCardPosition(int index)
    {
        if (index < 0 || index >= selectCardXPositions.Length)
            return Vector3.zero;
        return new Vector3(selectCardXPositions[index], selectCardY-1, selectCardZ);
    }

    /// <summary>
    /// ���õ� ī��� �ϼ��� �ݷ��� ���� ����Ʈ ��ȯ
    /// </summary>
    public List<CollectorComboResult> GetCompletedCollectorCombos(ScoreManager scoreManager)
    {
        List<CollectorComboResult> results = new List<CollectorComboResult>();
        if (checkCardList == null || checkCardList.Count == 0)
            return results;

        // üũ�� ī�� ID ��� ����
        List<int> ownedCardIds = new List<int>();
        foreach (var card in checkCardList)
        {
            if (card == null) continue;
            CardData cardData = card.GetComponent<CardData>();
            if (cardData != null)
                ownedCardIds.Add(cardData.cardId);
        }

        // �ݷ��ͺ� ��Ī ��� ���
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
}



// �ݷ��� ���� ��� ����ü
public struct CollectorComboResult
{
    public string collectorName;
    public int ownedCount;
    public int score;
}