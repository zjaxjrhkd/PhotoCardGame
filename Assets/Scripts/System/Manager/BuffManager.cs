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

    public GameObject sellBackImage;
    public GameObject buyBackImage;

    public bool buyBuffCard = true;

    public UIManager uiManager;

    // GameMaster, ScoreManager, CardManager 참조 필요
    public GameMaster gameMaster;
    public ScoreManager scoreManager;
    public CardManager cardManager;

    public CardSpawner cardSpawner; // 필드 선언 필요

    public void DetectBuffCardClick(int coin, Action<int> spendCoin, Action<int> GetCoin)
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider == null) return;

        CardData data = hit.collider.GetComponentInChildren<CardData>();
        if (data == null || data.cardType != CardData.CardType.Buff) return;

        if (buyBuffCard)
        {
            if (Mathf.Approximately(data.transform.position.y, 3.19f))
            {
                Debug.Log($"이미 구매된 버프카드입니다 (y=3.19f): {data.cardName}");
                return;
            }

            int cardCost = data.CardCost;
            if (coin < cardCost)
            {
                Debug.Log($"코인이 부족합니다. 필요 코인: {cardCost}, 보유 코인: {coin}");
                return;
            }

            spendCoin(cardCost);
            gameMaster.musicManager.PlayBuyBuffSFX();
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

            CardData cardData = newCard.GetComponentInChildren<CardData>();
            if (cardData != null)
            {
                cardData.InitEffects(scoreManager, gameMaster, cardManager);
            }

            Debug.Log($"버프 카드 구매 완료: {newCard.name}");
            RearrangeBuffCards();
        }
        else
        {
            if (!Mathf.Approximately(data.transform.position.y, 3.19f))
            {
                Debug.Log("상단에 배치된(구매된) 버프카드만 판매할 수 있습니다.");
                return;
            }

            int sellValue = Mathf.FloorToInt(data.CardCost * 0.5f);
            GetCoin(sellValue);

            Debug.Log("삭제 전 buffCardList 상태:");
            foreach (var card in buffCardList)
            {
                if (card == null)
                {
                    Debug.Log(" - null");
                }
                else
                {
                    CardData cd = card.GetComponentInChildren<CardData>();
                    if (cd != null)
                        Debug.Log($" - {cd.cardName} (ID: {cd.cardId})");
                    else
                        Debug.Log(" - GameObject 있음, CardData 없음");
                }
            }

            GameObject target = buffCardList.Find(card =>
            {
                if (card == null) return false;
                CardData cardData = card.GetComponentInChildren<CardData>();
                return cardData != null && cardData.cardId == data.cardId;
            });

            if (target != null)
            {
                buffCardList.Remove(target);
                Destroy(target);
            }
            else
            {
                Debug.LogWarning("판매 대상 카드를 buffCardList에서 찾지 못했습니다.");
            }

            buffCardList.RemoveAll(card => card == null);

            Debug.Log("삭제 후 buffCardList 상태:");
            foreach (var card in buffCardList)
            {
                if (card == null)
                {
                    Debug.Log(" - null");
                }
                else
                {
                    CardData cd = card.GetComponentInChildren<CardData>();
                    if (cd != null)
                        Debug.Log($" - {cd.cardName} (ID: {cd.cardId})");
                    else
                        Debug.Log(" - GameObject 있음, CardData 없음");
                }
            }
            gameMaster.musicManager.PlayGetCoinSFX();

            Debug.Log($"버프 카드 판매 완료: {data.cardName}, 반환 코인: {sellValue}");

            RearrangeBuffCards();
        }
    }



    public void OnbuyButten()
    {
        buyBuffCard = true;
        sellBackImage.SetActive(false); 
        buyBackImage.SetActive(true);
        gameMaster.musicManager.PlayUIClickSFX();
    }
    public void OnSellButten()
    {
        buyBuffCard = false;
        sellBackImage.SetActive(true);  
        buyBackImage.SetActive(false);
        gameMaster.musicManager.PlayUIClickSFX();

    }

    public void RearrangeBuffCards()
    {
        // 1. null 오브젝트 정리
        buffCardList.RemoveAll(card => card == null);
        Debug.Log("buffCardList에서 null 오브젝트 제거 완료. 현재 카드 수: " + buffCardList.Count);
        int count = buffCardList.Count;
        if (count == 0) return;

        // 2. cardId 기준 오름차순 정렬
        buffCardList.Sort((a, b) =>
        {
            if (a == null || b == null) return 0;
            var dataA = a.GetComponentInChildren<CardData>();
            var dataB = b.GetComponentInChildren<CardData>();
            int idA = dataA != null ? dataA.cardId : 0;
            int idB = dataB != null ? dataB.cardId : 0;
            return idA.CompareTo(idB);
        });

        float startX = -3.5f; // 항상 -3.5에서 시작
        float maxX = 4f;
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

        // 3. 정렬된 순서대로 위치 재배치
        for (int i = 0; i < count; i++)
        {
            GameObject card = buffCardList[i];
            float x = startX + spacing * i;
            float z = baseZ - zStep * i;
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