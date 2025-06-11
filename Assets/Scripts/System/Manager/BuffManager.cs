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

    // GameMaster, ScoreManager, CardManager ���� �ʿ�
    public GameMaster gameMaster;
    public ScoreManager scoreManager;
    public CardManager cardManager;

    public void DetectBuffCardClick(int coin, Action<int> spendCoin)
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider == null) return;

        CardData data = hit.collider.GetComponent<CardData>();
        if (data == null || data.cardType != CardData.CardType.Buff) return;

        if (coin < 3)
        {
            Debug.Log("������ �����մϴ�.");
            return;
        }

        spendCoin(3);

        GameObject clickedCard = data.gameObject;

        // ���� ����
        GameObject newCard = Instantiate(clickedCard);
        buffCardList.Add(newCard);

        // �� ������ ���� ��Ȱ��ȭ
        clickedCard.SetActive(false);

        // Init ����� ȣ��
        foreach (var effect in newCard.GetComponents<ICardEffect>())
        {
            effect.Init(scoreManager, gameMaster, cardManager);
        }

        Debug.Log($"���� ī�� ���� �Ϸ�: {newCard.name}");
        RearrangeBuffCards();
    }

    public void BuyBuffCardById(int cardId, int coin, Action<int> spendCoin)
    {
        if (buffCardListSO == null || cardId < 0 || cardId >= buffCardListSO.buffCards.Count)
        {
            Debug.LogWarning("�߸��� ī�� ID");
            return;
        }

        if (coin < 3)
        {
            Debug.Log("������ �����մϴ�.");
            return;
        }

        spendCoin(3);

        GameObject prefab = buffCardListSO.buffCards[cardId];
        GameObject newCard = Instantiate(prefab);
        buffCardList.Add(newCard);

        // Init ����� ȣ��
        foreach (var effect in newCard.GetComponents<ICardEffect>())
        {
            effect.Init(scoreManager, gameMaster, cardManager);
        }

        Debug.Log($"���� ī�� {newCard.name} ���� �Ϸ�");
        RearrangeBuffCards();
    }

    public void RearrangeBuffCards()
    {
        int count = buffCardList.Count;
        if (count == 0) return;

        float startX = -3.5f;
        float endX = 4f;
        float totalWidth = endX - startX;

        float spacing = (count > 1) ? totalWidth / (count - 1) : 0f;
        float y = 3.19f;
        float baseZ = -0.1f;
        float zStep = 0.1f;

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
        Debug.Log($"���� ���� ����: ���� {maxBuffSlotCount}ĭ");
    }

    public IEnumerator ApplyBuffCards(List<GameObject> buffCardList)
    {
        var buffListCopy = new List<GameObject>(buffCardList);

        foreach (var card in buffListCopy)
        {
            if (card == null) continue;
            var data = card.GetComponent<CardData>();
            if (data == null || data.buffType != CardData.BuffType.Always) continue;

            var effects = card.GetComponents<ICardEffect>();
            foreach (var effect in effects)
            {
                effect.Effect();
            }

            if (scoreManager != null && scoreManager.uiManager != null)
            {
                scoreManager.uiManager.UpdateScoreCalUI(
                    scoreManager.rate,
                    scoreManager.scoreYet,
                    scoreManager.resultScore
                );
            }

            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("[BuffManager] ApplyBuffCards �Ϸ�");
    }

    public void ApplyStartBuffs()
    {
        foreach (var card in buffCardList)
        {
            var data = card.GetComponent<CardData>();
            if (data != null && data.buffType == CardData.BuffType.OnStart)
            {
                foreach (var effect in card.GetComponents<ICardEffect>())
                {
                    effect.Effect();
                }
            }
        }
    }
}