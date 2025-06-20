using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    public List<GameObject> buffCardList = new List<GameObject>();
    public BuffCardListSO buffCardListSO;
    public int maxBuffSlotCount = 4;

    public float buffStartX = -3f;
    public float buffEndX = 4f;
    public float buffY = 3.19f;
    public float buffZ = -0.1f;
    public float buffDefaultSpacing = 1f;
    public float buffMinSpacing = 0.3f;

    public UIManager uiManager;

    // GameMaster, ScoreManager, CardManager 참조 필요
    public GameMaster gameMaster;
    public ScoreManager scoreManager;
    public CardManager cardManager;

    public CardSpawner cardSpawner; // 필드 선언 필요

    public void DetectBuffCardClick(int coin, Action<int> spendCoin)
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider == null) return;

        CardData data = hit.collider.GetComponent<CardData>();
        if (data == null || data.cardType != CardData.CardType.Buff) return;

        // 이미 상단 배치된 카드인지 위치로 판단 (y == 3.19f)
        if (Mathf.Approximately(data.transform.position.y, 3.19f))
        {
            Debug.Log($"이미 구매된 버프카드입니다 (y=3.19f): {data.cardName}");
            return;
        }

        int cardCost = data.CardCost; // CardData에 정의된 비용 사용
        if (coin < cardCost)
        {
            Debug.Log($"코인이 부족합니다. 필요 코인: {cardCost}, 보유 코인: {coin}");
            return;
        }

        spendCoin(cardCost);

        int index = data.cardId - 101;
        if (index < 0 || index >= buffCardListSO.buffCards.Count)
        {
            Debug.LogWarning($"잘못된 cardId: {data.cardId} (index: {index}, buffCardListSO count: {buffCardListSO.buffCards.Count})");
            return;
        }

        if (cardSpawner == null)
        {
            Debug.LogError("BuffManager: cardSpawner가 null입니다.");
            return;
        }

        var spawned = cardSpawner.SpawnBuffCardsByIndex(new List<int> { index });
        if (spawned.Count == 0)
        {
            Debug.LogWarning("BuffManager: SpawnBuffCardsByIndex로 버프카드 생성 실패");
            return;
        }

        GameObject newCard = spawned[0];
        buffCardList.Add(newCard);
        data.gameObject.SetActive(false);

        CardData cardData = newCard.GetComponent<CardData>();
        if (cardData != null)
        {
            cardData.InitEffects(scoreManager, gameMaster, cardManager);
        }

        Debug.Log($"버프 카드 구매 완료: {newCard.name}");
        RearrangeBuffCards();
    }

    public void RearrangeBuffCards()
    {
        int count = buffCardList.Count;
        if (count == 0) return;

        float startX = -3.5f; // 좌측 끝
        float maxX = 4f;      // 우측 최대 x값
        float y = 3.19f;
        float baseZ = -0.1f;
        float zStep = 0.1f;
        float defaultSpacing = 2.5f;
        float minSpacing = buffMinSpacing;

        // 기본 간격으로 배치했을 때 마지막 카드의 x값 계산
        float spacing = defaultSpacing;
        float lastX = startX + spacing * (count - 1);

        // 만약 마지막 카드가 maxX를 넘으면 spacing을 자동으로 줄임
        if (lastX > maxX && count > 1)
        {
            spacing = (maxX - startX) / (count - 1);
            if (spacing < minSpacing) spacing = minSpacing;
        }

        for (int i = 0; i < count; i++)
        {
            float x = startX + spacing * i;
            float z = baseZ - zStep * i;

            GameObject card = buffCardList[i];
            card.SetActive(true);
            card.transform.position = new Vector3(x, y, z);
        }
    }

    public void IncreaseBuffSlotCount(int amount)
    {
        maxBuffSlotCount += amount;
        RearrangeBuffCards();
        Debug.Log($"버프 슬롯 증가: 현재 {maxBuffSlotCount}칸");
    }

    public IEnumerator ApplyBuffCards(List<GameObject> buffCardList)
    {
        var buffListCopy = new List<GameObject>(buffCardList);

        foreach (var card in buffListCopy)
        {
            if (card == null)
                continue;

            CardData cardData = card.GetComponentInChildren<CardData>();
            if (cardData != null)
            {
                cardData.InitEffects(scoreManager, gameMaster, cardManager);
                cardData.UseEffect();
            }
            if (scoreManager != null && scoreManager.uiManager != null)
            {
                Debug.Log("[ApplyBuffCards] UI 갱신");
                scoreManager.uiManager.UpdateScoreCalUI(
                    scoreManager.rate,
                    scoreManager.scoreYet,
                    scoreManager.resultScore
                );
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

public void ApplyStartBuffs()
    {
        foreach (var card in buffCardList)
        {
            var data = card.GetComponent<CardData>();
            if (data != null && data.buffType == CardData.BuffType.OnStart)
            {
                foreach (var effect in card.GetComponents<CardData>())
                {
                    effect.UseEffect();
                    scoreManager.uiManager.UpdateScoreCalUI(scoreManager.rate, scoreManager.scoreYet, scoreManager.resultScore);
                }
            }
        }
    }
}