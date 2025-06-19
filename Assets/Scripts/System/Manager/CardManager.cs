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
    public ScoreManager scoreManager; // 인스펙터에서 연결 또는 FindObjectOfType 사용
    public GameObject otherUIObjects; // 비활성화/활성화할 나머지 UI 오브젝트들(인스펙터에서 할당)

    public GameObject deckListParent; // DeckList 오브젝트
    private bool isDeckListAtOrigin = false;


    // 카드 배치 좌표
    public float minX = -4.5f;
    public float maxX = 5.9f;
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

    // 스테이지 관련 변수 (public)
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
    /// 스테이지 관련 변수 초기화 및 현재 스테이지에 맞게 재설정
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
        // 필요하다면 카드 관련 다른 Update 코드 추가
    }

    public void DrawInitialCards()
    {
        DrawCards(defaultDrawCardEa);
    }

    public void OnClickSortDrawnCardsByCharacterType()
    {
        // Idle 상태가 아닐 때는 동작하지 않음
        if (gameMaster == null)
            gameMaster = FindObjectOfType<GameMaster>();

        var setProcessStateField = typeof(GameMaster).GetField("setProcessState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (setProcessStateField != null)
        {
            var setProcessStateValue = setProcessStateField.GetValue(gameMaster);
            if (setProcessStateValue == null || setProcessStateValue.ToString() != "Idle")
                return;
        }

        // SFX 재생
        if (gameMaster != null && gameMaster.musicManager != null)
            gameMaster.musicManager.PlayUIClickSFX();

        // checkCardList에 있는 모든 카드 선택 해제 및 원래 자리로 이동
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

        // 정렬 전 characterType 값 디버그 출력
        foreach (var cardObj in playCardList)
        {
            var data = cardObj != null ? cardObj.GetComponentInChildren<CardData>() : null;
            Debug.Log(data != null ? data.characterType.ToString() : "No CardData");
        }

        // CardData.characterType(enum) 기준으로 playCardList 정렬
        playCardList.Sort((a, b) =>
        {
            if (a == null || b == null) return 0;
            var dataA = a.GetComponentInChildren<CardData>();
            var dataB = b.GetComponentInChildren<CardData>();
            if (dataA == null || dataB == null) return 0;
            return dataA.characterType.CompareTo(dataB.characterType);
        });

        // 정렬 후 characterType 값 디버그 출력
        foreach (var cardObj in playCardList)
        {
            var data = cardObj != null ? cardObj.GetComponentInChildren<CardData>() : null;
            Debug.Log(data != null ? data.characterType.ToString() : "No CardData");
        }

        // 정렬된 순서대로 위치 재배치 (애니메이션 적용)
        int countAll = playCardList.Count;
        float spacing = countAll > 1 ? (maxX - minX) / (countAll - 1) : 0f;

        for (int i = 0; i < countAll; i++)
        {
            Debug.Log($"[CardManager] 정렬 후 카드 {i} 위치: {minX + spacing * i}, {baseZ + zStep * i}");
            var cardObj = playCardList[i];
            if (cardObj == null) continue;
            float x = (countAll == 1) ? (minX + maxX) / 2f : minX + spacing * i;
            float z = baseZ + zStep * i;
            cardObj.transform.DOMove(new Vector3(x, y, z), 0.4f).SetEase(Ease.OutCubic);
        }
    }

    public void OnClickSortDrawnCards()
    {
        // Idle 상태가 아닐 때는 동작하지 않음
        if (gameMaster == null)
            gameMaster = FindObjectOfType<GameMaster>();

        var setProcessStateField = typeof(GameMaster).GetField("setProcessState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (setProcessStateField != null)
        {
            var setProcessStateValue = setProcessStateField.GetValue(gameMaster);
            if (setProcessStateValue == null || setProcessStateValue.ToString() != "Idle")
                return;
        }

        // SFX 재생
        if (gameMaster != null && gameMaster.musicManager != null)
            gameMaster.musicManager.PlayUIClickSFX();

        // checkCardList에 있는 모든 카드 선택 해제 및 원래 자리로 이동
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

        // cardID 기준으로 playCardList 정렬
        playCardList.Sort((a, b) =>
        {
            if (a == null || b == null) return 0;
            var dataA = a.GetComponentInChildren<CardData>();
            var dataB = b.GetComponentInChildren<CardData>();
            int idA = dataA != null ? dataA.cardId : 0;
            int idB = dataB != null ? dataB.cardId : 0;
            return idA.CompareTo(idB);
        });

        // 정렬된 순서대로 위치 재배치 (애니메이션 적용)
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

            // Noi 스테이지: 1/4 확률로 59번 카드로 변경
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

        // 카드 배치 (DOTween 적용, y는 -3.2로 고정)
        int countAll = playCardList.Count;
        float spacing = countAll > 1 ? (maxX - minX) / (countAll - 1) : 0f;

        for (int i = 0; i < countAll; i++)
        {
            var cardObj = playCardList[i];
            if (cardObj == null) continue;
            float x = (countAll == 1) ? (minX + maxX) / 2f : minX + spacing * i;
            float z = baseZ + zStep * i;

            // 카드 움직임 시작 위치를 7.8, -3.2, -1로 지정
            cardObj.transform.position = new Vector3(7.8f, -3.2f, -1f);
            cardObj.transform.DOMove(new Vector3(x, y, z), 0.4f).SetEase(Ease.OutCubic);
        }

        // Raz 스테이지: 1/2 확률로 카드 뒷면 스프라이트로 변경
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


    /// <summary>
    /// DeckList 오브젝트의 자식으로 deckList의 카드 오브젝트를 생성하고 X=-2.2~11.3에 균등 배치
    /// </summary>
    /// <summary>
    /// DeckList 오브젝트의 자식으로 deckList의 카드 오브젝트를 생성하고 X=-2.2~11.3에 균등 배치
    /// </summary>
    public void OnClickCreateDeckListObjects()
    {
        if (gameMaster != null && gameMaster.musicManager != null)
            gameMaster.musicManager.PlayUIClickSFX();

        if (deckListParent == null || cardSpawner == null || deckList == null || deckList.Count == 0)
            return;

        // 위치 토글 (월드 좌표)
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

        // "Clone"이 포함된 자식만 삭제
        foreach (Transform child in deckListParent.transform)
        {
            if (child.name.Contains("Clone"))
                Destroy(child.gameObject);
        }

        // 카드 생성 (SpawnCardsByID 사용, 부모는 deckListParent로 직접 지정)
        List<GameObject> spawnedCards = cardSpawner.SpawnCardsByID(deckList);

        // cardId 기준으로 정렬
        spawnedCards.Sort((a, b) =>
        {
            var dataA = a.GetComponentInChildren<CardData>();
            var dataB = b.GetComponentInChildren<CardData>();
            int idA = dataA != null ? dataA.cardId : 0;
            int idB = dataB != null ? dataB.cardId : 0;
            return idA.CompareTo(idB);
        });

        // 부모(DeckList)로 이동 및 콜라이더 비활성화
        foreach (var cardObj in spawnedCards)
        {
            if (cardObj == null) continue;
            cardObj.transform.SetParent(deckListParent.transform, false);

            // 모든 Collider, Collider2D 비활성화
            foreach (var collider in cardObj.GetComponentsInChildren<Collider>(true))
                collider.enabled = false;
            foreach (var collider2D in cardObj.GetComponentsInChildren<Collider2D>(true))
                collider2D.enabled = false;
        }

        // X=-5.9 ~ 5.9, Y=-3.5 ~ 3.5, 한 줄에 최대 10개, 각 줄 균등 배치, Z는 -10부터 +0.1씩 증가
        float startX = -5.9f;
        float endX = 5.9f;
        float startY = 3.5f;
        float endY = -3.5f;
        int maxPerRow = 10;
        int count = spawnedCards.Count;

        // 균등 분배를 위한 행/열 계산
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
        // Iana 스테이지면 0번 카드로 84장 채움
        if (isIanaStage)
        {
            for (int i = 1; i <= 54; i++)
                deckList.Add(i);
            for (int i = 55; i < 85; i++)
                deckList.Add(58); // 0번 카드 84장
        }
        else
        {
            for (int i = 1; i <= 54; i++)
                deckList.Add(i);
        }

        totalDeckCount = deckList.Count; // 셔플 직후 한 번만 저장

        // 셔플
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
        // setProcessState가 Idle일 때만 동작
        if (gameMaster == null)
            gameMaster = FindObjectOfType<GameMaster>();

        // GameMaster.SetProcessState enum이 public이 아니므로 string 비교 사용
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

            // Buff 카드는 무시
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

        // 해제(클릭 해제)는 항상 허용해야 하므로, 선택(추가)일 때만 7개 제한
        if (cardStateRead == null || cardDataRead == null) return;

        GameObject card = cardDataRead.gameObject;
        GameObject parentCard = card.transform.parent.gameObject;

        if (cardStateRead.isClick)
        {
            if (checkCardList.Count >= 7)
                return;

            if (!checkCardList.Contains(card))
                checkCardList.Add(card);

            // playCardList에서 제거 (중복 제거 방지)
            bool removed = false;
            if (playCardList.Contains(parentCard))
            {
                playCardList.Remove(parentCard);
                removed = true;
            }

            // 카드가 playCardList에서 제거된 경우, 남은 카드들 위치 재배치(DOTween)
            if (removed)
                ArrangeCardsWithinRange(true);

            if (!selectCardList.Contains(parentCard))
            {
                int emptyIdx = selectCardList.FindIndex(obj => obj == null);
                if (emptyIdx != -1)
                {
                    selectCardList[emptyIdx] = parentCard;
                    // DOTween으로 지정 위치로 이동
                    Vector3 targetPos = GetSelectCardPosition(emptyIdx);
                    parentCard.transform.DOMove(targetPos, 0.3f).SetEase(Ease.OutCubic);
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
        {
            gameMaster.uiManager.UpdateCollectorResultUI(combos);

            // 점수 계산 UI 갱신 추가
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

        // playCardList에 다시 추가 (중복 방지)
        if (!playCardList.Contains(parentCard))
            playCardList.Add(parentCard);

        int idx = selectCardList.IndexOf(parentCard);
        if (idx != -1)
        {
            selectCardList[idx] = null;
        }

        // playCardList 전체 위치 재배치 (DOTween 적용)
        ArrangeCardsWithinRange(true);

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

    // DOTween 애니메이션 적용 버전
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

            // DOTween으로 X값 +10 이동 후 Destroy
            Vector3 targetPos = parentCard.transform.position + new Vector3(10f, 0f, 0f);
            parentCard.transform.DOMove(targetPos, 0.3f).SetEase(Ease.InCubic)
                .OnComplete(() => Destroy(parentCard));
        }
        checkCardList.Clear();

        // Limi 스테이지면 1/2 확률로만 드로우
        int originalDrawEa = drawCardEa;
        drawCardEa = dropCount;

        if (!isLimiStage || (isLimiStage && Random.value < 0.5f))
        {
            DrawCards(drawCardEa);
            // DrawCards에서 이미 (7.8, -3.2, -1)에서 DOTween 이동 처리됨
        }

        drawCardEa = originalDrawEa;
        // ArrangeCardsWithinRange(); // 불필요, DrawCards에서 위치 애니메이션 처리됨
    }

    public void InitDrawnCards()
    {
        // 새로 추가된 카드 전체에 InitEffects 호출
        foreach (var cardObj in playCardList)
        {
            if (cardObj == null) continue;
            var cardData = cardObj.GetComponentInChildren<CardData>();
            if (cardData != null)
            {
                cardData.InitEffects(scoreManager, gameMaster, this);
                //Debug.Log($"[CardManager] InitEffects 호출됨: {cardData.cardName} (ID: {cardData.cardId}, 오브젝트: {cardObj.name})");
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
        return new Vector3(selectCardXPositions[index], selectCardY-1, selectCardZ);
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