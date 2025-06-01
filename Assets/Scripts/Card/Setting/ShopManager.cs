using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("ī�� ������")]
    public BuffCardListSO buffCardListSO;

    [Header("���� ���")]
    public GameObject shopPanel; // ���� ��ü UI �г�
    public GameObject cardSpawnRoot; // ī�� ���� ��ġ �θ�
    public GameMaster gameMaster;

    [Header("���� ī�� ��ġ ����")]
    public float minX = -3f;
    public float maxX = 5f;
    public float y = 0f;
    public float z = 0f;

    [Header("ī�� ����")]
    public int buffCardPrice = 3;

    public void OpenShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(true);

        ClearSpawnedCards();

        int total = buffCardListSO.buffCards.Count;
        int count = Mathf.Min(4, total);

        // 0~total-1 �ε��� �� �����ϰ� count�� �̱� (�ߺ� ����)
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
