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
                // Debug.Log($"[CardManager] 마우스 감지됨 → {state.name}");

                if (!state.isClick && gameMaster != null && IsInPlayCardList(state.gameObject))
                {
                    if (lastHoveredCard != null && lastHoveredCard != state)
                    {
                        lastHoveredCard.HideHoverUI();
                        Debug.Log($"[CardManager] 이전 카드 {lastHoveredCard.name} UI 숨김");
                    }

                    state.ShowHoverUI();
                    lastHoveredCard = state;

                    // Debug.Log($"[CardManager] 카드 {state.name} UI 표시");
                    return;
                }
                // else
                // {
                //     Debug.Log("[CardManager] 상태 조건 불일치 → UI 표시 안함");
                // }
            }
            // else
            // {
            //     Debug.Log("[CardManager] 카드 상태 스크립트(CardState) 없음");
            // }
        }

        // 마우스가 카드에서 벗어난 경우
        if (lastHoveredCard != null)
        {
            lastHoveredCard.HideHoverUI();
            // Debug.Log($"[CardManager] 카드 {lastHoveredCard.name} 에서 마우스 벗어남 → UI 숨김");
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
        Debug.Log("덱 섞기 완료, 카드 수: " + deckList.Count);

        // 덱 장수 UI 갱신
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

        // selectCardList에 있는 오브젝트도 Destroy
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
        // 덱 장수 UI 갱신
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
            // 필요에 따라 dropCardList, playCardList 등도 포함할 수 있음
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
        if (cardStateRead == null || cardDataRead == null) return;

        GameObject card = cardDataRead.gameObject;
        GameObject parentCard = card.transform.parent.gameObject;

        if (cardStateRead.isClick)
        {
            if (!checkCardList.Contains(card))
                checkCardList.Add(card);

            // selectCardList에 비어있는 자리(null)를 찾아서 추가
            if (!selectCardList.Contains(parentCard))
            {
                // 최대 7장까지만
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

            // playCardList에서의 인덱스 구하기
            int playIdx = playCardList.IndexOf(parentCard);
            if (playIdx >= 0)
            {
                float spacing = (playCardList.Count > 1) ? (maxX - minX) / (playCardList.Count - 1) : 0f;
                float x = minX + spacing * playIdx;
                float z = baseZ + zStep * playIdx;
                parentCard.transform.position = new Vector3(x, y, z);
            }
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
            Destroy(parentCard);
        }
        checkCardList.Clear();

        int originalDrawEa = drawCardEa;
        drawCardEa = dropCount;
        DrawCards(drawCardEa); // DrawCard() → DrawCards(drawCardEa)로 변경
        drawCardEa = originalDrawEa;
        ArrangeCardsWithinRange();
    }

    public Vector3 GetSelectCardPosition(int index)
    {
        if (index < 0 || index >= selectCardXPositions.Length)
            return Vector3.zero;
        return new Vector3(selectCardXPositions[index], selectCardY, selectCardZ);
    }

    // 카드 선택/체크/드롭 관련 메서드는 GameMaster와의 연결이 필요하므로, 
    // GameMaster에서 호출하거나 이벤트로 연결하는 것이 좋습니다.
}