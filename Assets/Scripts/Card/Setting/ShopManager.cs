using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("ī�� ������")]
    public BuffCardListSO buffCardListSO;

    [Header("���� ���")]
    public GameObject shopPanel; // ���� ��ü UI �г�
    public GameMaster gameMaster;
    public CardSpawner cardSpawner;

    [Header("ī�� ����")]
    public int buffCardPrice = 3;

    // OpenShop���� ������ ī�� ������
    private List<GameObject> spawnedShopCards = new List<GameObject>();

    public void OpenShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(true);

        ClearSpawnedCards();
        Debug.Log("[ShopManager] ���� ����");

        int total = buffCardListSO.buffCards.Count;
        int count = Mathf.Min(4, total);

        // 0~total-1 �ε��� �� �����ϰ� count�� �̱� (�ߺ� ����)
        List<int> indices = new List<int>();
        for (int i = 0; i < total; i++) indices.Add(i);
        for (int i = 0; i < count; i++)
        {
            int randIdx = Random.Range(i, total);
            int temp = indices[i];
            indices[i] = indices[randIdx];
            indices[randIdx] = temp;
        }

        // ���õ� �ε��� ����Ʈ ����
        List<int> selectedBuffCardIndices = new List<int>();
        for (int i = 0; i < count; i++)
        {
            selectedBuffCardIndices.Add(indices[i]);
        }

        if (cardSpawner == null)
        {
            Debug.LogError("ShopManager: cardSpawner�� null�Դϴ�.");
            return;
        }

        // ����ī�� ���� (index ���)
        List<GameObject> spawnedCards = cardSpawner.SpawnBuffCardsByIndex(selectedBuffCardIndices);

        Debug.Log($"[ShopManager] ������ ����ī�� ����: {spawnedCards.Count}");

        // x�� -3.5~5 �յ� �й�, y=0, z=-1 ����
        float startX = -3.5f;
        float endX = 5f;
        float spacing = (spawnedCards.Count > 1) ? (endX - startX) / (spawnedCards.Count - 1) : 0f;

        spawnedShopCards.Clear();
        for (int i = 0; i < spawnedCards.Count; i++)
        {
            GameObject card = spawnedCards[i];
            float x = (spawnedCards.Count == 1) ? (startX + endX) / 2f : startX + spacing * i;
            Vector3 spawnPos = new Vector3(x, 0f, -1f);
            card.transform.position = spawnPos; // �θ� ���� ��ġ�� ����

            spawnedShopCards.Add(card);

            var data = card.GetComponent<CardData>();
            int cardId = data != null ? data.cardId : -1;
            Debug.Log($"[ShopManager] ����ī�� ����: {card.name}, cardId: {cardId}, ��ġ: {spawnPos}");
        }
    }

    // ���� �� ShopItemUI ��� ȣ��
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