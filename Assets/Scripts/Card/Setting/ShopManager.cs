using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("카드 데이터")]
    public BuffCardListSO buffCardListSO;

    [Header("연결 대상")]
    public GameObject shopPanel; // 상점 전체 UI 패널
    public GameMaster gameMaster;
    public CardSpawner cardSpawner;

    [Header("카드 가격")]
    public int buffCardPrice = 3;

    // OpenShop에서 생성한 카드 추적용
    private List<GameObject> spawnedShopCards = new List<GameObject>();

    public void OpenShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(true);

        ClearSpawnedCards();
        Debug.Log("[ShopManager] 상점 열림");

        int total = buffCardListSO.buffCards.Count;
        int count = Mathf.Min(4, total);

        // 0~total-1 인덱스 중 랜덤하게 count개 뽑기 (중복 없이)
        List<int> indices = new List<int>();
        for (int i = 0; i < total; i++) indices.Add(i);
        for (int i = 0; i < count; i++)
        {
            int randIdx = Random.Range(i, total);
            int temp = indices[i];
            indices[i] = indices[randIdx];
            indices[randIdx] = temp;
        }

        // 선택된 인덱스 리스트 생성
        List<int> selectedBuffCardIndices = new List<int>();
        for (int i = 0; i < count; i++)
        {
            selectedBuffCardIndices.Add(indices[i]);
        }

        if (cardSpawner == null)
        {
            Debug.LogError("ShopManager: cardSpawner가 null입니다.");
            return;
        }

        // 버프카드 생성 (index 기반)
        List<GameObject> spawnedCards = cardSpawner.SpawnBuffCardsByIndex(selectedBuffCardIndices);

        Debug.Log($"[ShopManager] 생성된 버프카드 개수: {spawnedCards.Count}");

        // x축 -3.5~5 균등 분배, y=0, z=-1 고정
        float startX = -3.5f;
        float endX = 5f;
        float spacing = (spawnedCards.Count > 1) ? (endX - startX) / (spawnedCards.Count - 1) : 0f;

        spawnedShopCards.Clear();
        for (int i = 0; i < spawnedCards.Count; i++)
        {
            GameObject card = spawnedCards[i];
            float x = (spawnedCards.Count == 1) ? (startX + endX) / 2f : startX + spacing * i;
            Vector3 spawnPos = new Vector3(x, 0f, -1f);
            card.transform.position = spawnPos; // 부모 없이 위치만 지정

            spawnedShopCards.Add(card);

            var data = card.GetComponent<CardData>();
            int cardId = data != null ? data.cardId : -1;
            Debug.Log($"[ShopManager] 버프카드 생성: {card.name}, cardId: {cardId}, 위치: {spawnPos}");
        }
    }

    // 구매 시 ShopItemUI 등에서 호출
    public void NotifyCardPurchased(GameObject card)
    {
        spawnedShopCards.Remove(card);
    }

    private void ClearSpawnedCards()
    {
        foreach (var card in spawnedShopCards)
        {
            if (card != null)
                Destroy(card);
        }
        spawnedShopCards.Clear();
    }

    public void CloseShop()
    {
        ClearSpawnedCards();
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    private void SetupShopItem(GameObject item, int cardId, int price)
    {
        if (item == null) return;

        item.SetActive(true);
        ShopItemUI itemUI = item.GetComponent<ShopItemUI>();
        if (itemUI != null)
            itemUI.Setup(cardId, price, gameMaster, buffCardListSO);
    }

    public bool IsShopClosed()
    {
        return !gameObject.activeSelf;
    }
}