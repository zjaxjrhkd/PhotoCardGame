using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public List<int> deckList = new List<int>();
    public List<int> drawCardList = new List<int>();
    public List<GameObject> playCardList = new List<GameObject>();
    public List<GameObject> checkCardList = new List<GameObject>();
    public List<GameObject> dropCardList = new List<GameObject>();
    public List<GameObject> selectCardList = new List<GameObject>();

    public CardSpawner cardSpawner;
    public ScoreManager scoreManager; // 인스펙터에서 연결 또는 FindObjectOfType 사용

    // 카드 배치 좌표
    public float minX = -3.65f;
    public float maxX = 5f;
    public float y = -3.2f;
    public float baseZ = -1f;
    public float zStep = 0.1f;

    public int defaultDrawCardEa = 10; // 게임 시작 시 드로우 장수
    public int drawCardEa = 1;

    // Hover UI 관련 필드
    private CardState lastHoveredCard;
    private GameMaster gameMaster;

    private static readonly float[] selectCardXPositions = { -4.5f, -2.7f, -0.9f, 0.9f, 2.7f, 4.5f, 6.3f };
    private const float selectCardY = 0f;
    private const float selectCardZ = -0.1f;

    public int totalDeckCount; // 총 장수(고정)

    void Start()
    {
        gameMaster = FindObjectOfType<GameMaster>();
        cardSpawner = GetComponent<CardSpawner>();
        if (scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>();
    }

    void Update()
    {
        HandleCardHover();
        // 필요하다면 카드 관련 다른 Update 코드 추가
    }

    public void DrawInitialCards()
    {
        DrawCards(defaultDrawCardEa);
    }

    public void DrawCards(int count)
    {
        drawCardList.Clear();
        int drawCount = Mathf.Min(count, deckList.Count);
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
                if (!state.isClick && gameMaster != null && IsInPlayCardList(state.gameObject))
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
        }

        if (lastHoveredCard != null)
        {
            lastHoveredCard.HideHoverUI();
            lastHoveredCard = null;
        }
    }

    public void SetDeckSuffle()
    {
        deckList.Clear();
        for (int i = 1; i <= 54; i++) deckList.Add(i);
        totalDeckCount = deckList.Count; // 셔플 직후 한 번만 저장
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
        selectCardList = new List<GameObject>(new GameObject[7]); // 7칸짜리로 초기화

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
        if (!Input.GetMouseButtonDown(0)) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            CardState cardStateRead = hit.collider.GetComponent<CardState>();
            CardData cardDataRead = hit.collider.GetComponent<CardData>();

            if (cardStateRead != null)
            {
                cardStateRead.CardStateClicked();
                AddCheckCardList(cardStateRead, cardDataRead);
            }
        }
    }

    public void AddCheckCardList(CardState cardStateRead, CardData cardDataRead)
    {
        if (checkCardList.Count >= 7)
            return;

        if (cardStateRead == null || cardDataRead == null) return;

        GameObject card = cardDataRead.gameObject;
        GameObject parentCard = card.transform.parent.gameObject;

        if (cardStateRead.isClick)
        {
            if (!checkCardList.Contains(card))
                checkCardList.Add(card);

            if (!selectCardList.Contains(parentCard))
            {
                int emptyIdx = selectCardList.FindIndex(obj => obj == null);
                if (emptyIdx != -1)
                {
                    selectCardList[emptyIdx] = parentCard;
                    parentCard.transform.position = GetSelectCardPosition(emptyIdx);
                }
            }
        }
        else
        {
            RemoveCheckCardList(cardStateRead, cardDataRead);
        }

        // checkCardList 정보 출력
        List<string> cardInfoList = new List<string>();
        foreach (var obj in checkCardList)
        {
            if (obj == null) continue;
            CardData cd = obj.GetComponent<CardData>();
            if (cd != null)
                cardInfoList.Add($"{cd.cardId}({obj.name})");
        }
        Debug.Log($"[CardManager] checkCardList: {string.Join(", ", cardInfoList)}");

        // 조합 체크 및 UI 갱신
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

    public void RemoveCheckCardList(CardState cardStateRead, CardData cardDataRead)
    {
        if (cardStateRead == null || cardDataRead == null) return;

        GameObject card = cardDataRead.gameObject;
        GameObject parentCard = card.transform.parent.gameObject;

        if (checkCardList.Contains(card))
            checkCardList.Remove(card);

        int idx = selectCardList.IndexOf(parentCard);
        if (idx != -1)
        {
            selectCardList[idx] = null;

            int playIdx = playCardList.IndexOf(parentCard);
            if (playIdx >= 0)
            {
                float spacing = (playCardList.Count > 1) ? (maxX - minX) / (playCardList.Count - 1) : 0f;
                float x = minX + spacing * playIdx;
                float z = baseZ + zStep * playIdx;
                parentCard.transform.position = new Vector3(x, y, z);
            }
        }

        // checkCardList 정보 출력
        List<string> cardInfoList = new List<string>();
        foreach (var obj in checkCardList)
        {
            if (obj == null) continue;
            CardData cd = obj.GetComponent<CardData>();
            if (cd != null)
                cardInfoList.Add($"{cd.cardId}({obj.name})");
        }
        Debug.Log($"[CardManager] checkCardList: {string.Join(", ", cardInfoList)}");

        // 조합 체크 및 UI 갱신
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


    public void DropAndDrawSelectedCards()
    {
        int dropCount = checkCardList.Count;
        if (dropCount == 0) return;

        foreach (GameObject card in checkCardList)
        {
            dropCardList.Add(card);
            GameObject parentCard = card.transform.parent.gameObject;
            playCardList.Remove(parentCard);
            Destroy(parentCard);
        }
        checkCardList.Clear();

        int originalDrawEa = drawCardEa;
        drawCardEa = dropCount;
        DrawCards(drawCardEa);
        drawCardEa = originalDrawEa;
        ArrangeCardsWithinRange();
    }

    public Vector3 GetSelectCardPosition(int index)
    {
        if (index < 0 || index >= selectCardXPositions.Length)
            return Vector3.zero;
        return new Vector3(selectCardXPositions[index], selectCardY, selectCardZ);
    }

    /// <summary>
    /// 선택된 카드로 완성된 콜렉터 조합 리스트 반환
    /// </summary>
    public List<CollectorComboResult> GetCompletedCollectorCombos(ScoreManager scoreManager)
    {
        List<CollectorComboResult> results = new List<CollectorComboResult>();
        if (checkCardList == null || checkCardList.Count == 0)
            return results;

        // 체크된 카드 ID 목록 생성
        List<int> ownedCardIds = new List<int>();
        foreach (var card in checkCardList)
        {
            if (card == null) continue;
            CardData cardData = card.GetComponent<CardData>();
            if (cardData != null)
                ownedCardIds.Add(cardData.cardId);
        }

        // 콜렉터별 매칭 결과 얻기
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

// 콜렉터 조합 결과 구조체
public struct CollectorComboResult
{
    public string collectorName;
    public int ownedCount;
    public int score;
}