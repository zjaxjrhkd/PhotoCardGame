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

    // GameMaster, ScoreManager, CardManager ���� �ʿ�
    public GameMaster gameMaster;
    public ScoreManager scoreManager;
    public CardManager cardManager;

    public CardSpawner cardSpawner; // �ʵ� ���� �ʿ�

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
                Debug.Log($"�̹� ���ŵ� ����ī���Դϴ� (y=3.19f): {data.cardName}");
                return;
            }

            int cardCost = data.CardCost;
            if (coin < cardCost)
            {
                Debug.Log($"������ �����մϴ�. �ʿ� ����: {cardCost}, ���� ����: {coin}");
                return;
            }

            spendCoin(cardCost);
            gameMaster.musicManager.PlayBuyBuffSFX();
            int index = data.cardId - 101;
            if (index < 0 || index >= buffCardListSO.buffCards.Count)
            {
                Debug.LogWarning($"�߸��� cardId: {data.cardId} (index: {index}, buffCardListSO count: {buffCardListSO.buffCards.Count})");
                return;
            }

            if (cardSpawner == null)
            {
                Debug.LogError("BuffManager: cardSpawner�� null�Դϴ�.");
                return;
            }

            var spawned = cardSpawner.SpawnBuffCardsByIndex(new List<int> { index });
            if (spawned.Count == 0)
            {
                Debug.LogWarning("BuffManager: SpawnBuffCardsByIndex�� ����ī�� ���� ����");
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

            Debug.Log($"���� ī�� ���� �Ϸ�: {newCard.name}");
            RearrangeBuffCards();
        }
        else
        {
            if (!Mathf.Approximately(data.transform.position.y, 3.19f))
            {
                Debug.Log("��ܿ� ��ġ��(���ŵ�) ����ī�常 �Ǹ��� �� �ֽ��ϴ�.");
                return;
            }

            int sellValue = Mathf.FloorToInt(data.CardCost * 0.5f);
            GetCoin(sellValue);

            Debug.Log("���� �� buffCardList ����:");
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
                        Debug.Log(" - GameObject ����, CardData ����");
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
                Debug.LogWarning("�Ǹ� ��� ī�带 buffCardList���� ã�� ���߽��ϴ�.");
            }

            buffCardList.RemoveAll(card => card == null);

            Debug.Log("���� �� buffCardList ����:");
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
                        Debug.Log(" - GameObject ����, CardData ����");
                }
            }
            gameMaster.musicManager.PlayGetCoinSFX();

            Debug.Log($"���� ī�� �Ǹ� �Ϸ�: {data.cardName}, ��ȯ ����: {sellValue}");

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
        // 1. null ������Ʈ ����
        buffCardList.RemoveAll(card => card == null);
        Debug.Log("buffCardList���� null ������Ʈ ���� �Ϸ�. ���� ī�� ��: " + buffCardList.Count);
        int count = buffCardList.Count;
        if (count == 0) return;

        // 2. cardId ���� �������� ����
        buffCardList.Sort((a, b) =>
        {
            if (a == null || b == null) return 0;
            var dataA = a.GetComponentInChildren<CardData>();
            var dataB = b.GetComponentInChildren<CardData>();
            int idA = dataA != null ? dataA.cardId : 0;
            int idB = dataB != null ? dataB.cardId : 0;
            return idA.CompareTo(idB);
        });

        float startX = -3.5f; // �׻� -3.5���� ����
        float maxX = 4f;
        float y = 3.19f;
        float baseZ = -0.1f;
        float zStep = 0.1f;
        float defaultSpacing = 2.5f;
        float minSpacing = buffMinSpacing;

        // �⺻ �������� ��ġ���� �� ������ ī���� x�� ���
        float spacing = defaultSpacing;
        float lastX = startX + spacing * (count - 1);

        // ���� ������ ī�尡 maxX�� ������ spacing�� �ڵ����� ����
        if (lastX > maxX && count > 1)
        {
            spacing = (maxX - startX) / (count - 1);
            if (spacing < minSpacing) spacing = minSpacing;
        }

        // 3. ���ĵ� ������� ��ġ ���ġ
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
        Debug.Log($"���� ���� ����: ���� {maxBuffSlotCount}ĭ");
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
                Debug.Log("[ApplyBuffCards] UI ����");
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