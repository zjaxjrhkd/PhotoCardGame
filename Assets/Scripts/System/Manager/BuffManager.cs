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

    public void DetectBuffCardClick(int coin, System.Action<int> spendCoin)
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

        spendCoin(3);

        GameObject clickedCard = data.gameObject;
        clickedCard.SetActive(false);

        GameObject newCard = Instantiate(clickedCard);
        buffCardList.Add(newCard);
        RearrangeBuffCards();

        Debug.Log($"버프 카드 구매 완료: {newCard.name}");
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
        Debug.Log($"버프 슬롯 증가: 현재 {maxBuffSlotCount}칸");
    }

    public void BuyBuffCardById(int cardId, int coin, System.Action<int> spendCoin)
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

        spendCoin(3);

        GameObject prefab = buffCardListSO.buffCards[cardId];
        buffCardList.Add(prefab);

        Debug.Log($"버프 카드 {prefab.name} 구매 완료");
        RearrangeBuffCards();
    }

    public interface IBuffCard
    {
        void ApplyBuff(GameMaster game);
    }

    public void ApplyBuffCards(List<GameObject> checkCardList, GameMaster gameMaster)
    {
        foreach (GameObject card in checkCardList)
        {
            var buffs = card.GetComponentsInChildren<IBuffCard>();
            foreach (var buff in buffs)
            {
                buff.ApplyBuff(gameMaster);
            }
        }
    }
}
