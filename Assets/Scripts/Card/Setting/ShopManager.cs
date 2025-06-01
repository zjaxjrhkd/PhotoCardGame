using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("카드 데이터")]
    public BuffCardListSO buffCardListSO;

    [Header("연결 대상")]
    public GameObject shopPanel; // 상점 전체 UI 패널
    public GameObject cardSpawnRoot; // 카드 생성 위치 부모
    public GameMaster gameMaster;

    [Header("버프 카드 위치 설정")]
    public float minX = -3f;
    public float maxX = 5f;
    public float y = 0f;
    public float z = 0f;

    [Header("카드 가격")]
    public int buffCardPrice = 3;

    public void OpenShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(true);

        ClearSpawnedCards();

        int total = buffCardListSO.buffCards.Count;
        int count = Mathf.Min(4, total);

        // 0~total-1 인덱스 중 랜덤하게 count개 뽑기 (중복 없이)
        List<int> indices = new List<int>();
        for (int i = 0; i < total; i++) indices.Add(i);
        for (int i = 0; i < count; i++)
        {
            int randIdx = Random.Range(i, total);
            // Swap
            int temp = indices[i];
            indices[i] = indices[randIdx];
            indices[randIdx] = temp;
        }

        float spacing = (count > 1) ? (maxX - minX) / (count - 1) : 0;

        for (int i = 0; i < count; i++)
        {
            int cardIdx = indices[i];
            GameObject prefab = buffCardListSO.buffCards[cardIdx];
            Vector3 spawnPos = new Vector3(minX + spacing * i, y, -0.1f);

            GameObject card = Instantiate(prefab, spawnPos, Quaternion.identity, cardSpawnRoot.transform);

            CardData data = card.GetComponent<CardData>();
            if (data != null)
                data.cardId = cardIdx;
        }
    }


    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }
    private void SetupShopItem(GameObject item, GameObject cardPrefab, int price)
    {
        item.SetActive(true);
        ShopItemUI itemUI = item.GetComponent<ShopItemUI>();
        itemUI.Setup(cardPrefab, price, gameMaster);
    }

    private void ClearSpawnedCards()
    {
        foreach (Transform child in cardSpawnRoot.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public bool IsShopClosed()
    {
        return !gameObject.activeSelf;
    }
}
