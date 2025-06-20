using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("ī�� ������")]
    public BuffCardListSO buffCardListSO;
    public CardListSO CardListSO;

    [Header("���� ���")]
    public GameObject shopPanel; // ���� ��ü UI �г�
    public GameObject setPanel;  // ��Ʈ UI �г�

    public GameMaster gameMaster;
    public CardSpawner cardSpawner;

    // OpenShop���� ������ ī�� ������
    private List<GameObject> spawnedShopCards = new List<GameObject>();

    public void OpenShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(true);
        if (setPanel != null)
            setPanel.SetActive(false);
        ClearSpawnedCards();
        Debug.Log("[ShopManager] ���� ����");

        // 1. ����ī�� 4�� ����
        int totalBuff = buffCardListSO != null && buffCardListSO.buffCards != null ? buffCardListSO.buffCards.Count : 0;
        int buffCount = Mathf.Min(4, totalBuff);
        List<int> buffIndices = Enumerable.Range(0, totalBuff).OrderBy(x => Random.value).Take(buffCount).ToList();
        List<GameObject> spawnedBuffCards = (cardSpawner != null && buffIndices.Count > 0)
            ? cardSpawner.SpawnBuffCardsByIndex(buffIndices)
            : new List<GameObject>();

        float buffStartX = -3.5f;
        float buffEndX = 5f;
        float buffY = 0f;
        float buffZ = -1f;
        float buffSpacing = (spawnedBuffCards.Count > 1) ? (buffEndX - buffStartX) / (spawnedBuffCards.Count - 1) : 0f;

        for (int i = 0; i < spawnedBuffCards.Count; i++)
        {
            float x = (spawnedBuffCards.Count == 1) ? (buffStartX + buffEndX) / 2f : buffStartX + buffSpacing * i;
            Vector3 pos = new Vector3(x, buffY, buffZ);
            spawnedBuffCards[i].transform.position = pos;
            spawnedShopCards.Add(spawnedBuffCards[i]);
            var data = spawnedBuffCards[i].GetComponent<CardData>();
            int cardId = data != null ? data.cardId : -1;
            Debug.Log($"[ShopManager] ����ī�� ����: {spawnedBuffCards[i].name}, cardId: {cardId}, ��ġ: {pos}");
        }

        // 2. ĳ���� ī�� 6�� ���� (index: 1~57)
        int charMinIndex = 1;
        int charMaxIndex = 57;
        int charCount = 6;
        List<int> charIndices = Enumerable.Range(charMinIndex, charMaxIndex).OrderBy(x => Random.value).Take(charCount).ToList();
        Debug.Log($"[ShopManager] ���� ���õ� ĳ���� ī�� �ε���(1~57): {string.Join(",", charIndices)}");

        List<int> charCardIds = charIndices
            .Select(idx => {
                // CardListSO.cards�� 0���� �����ϹǷ� idx-1
                var prefab = (idx - 1 >= 0 && idx - 1 < CardListSO.cards.Count) ? CardListSO.cards[idx - 1] : null;
                var data = prefab != null ? prefab.GetComponentInChildren<CardData>() : null;
                return data != null ? data.cardId : -1;
            })
            .Where(id => id != -1)
            .ToList();

        List<GameObject> spawnedCharCards = (cardSpawner != null && charCardIds.Count > 0)
            ? cardSpawner.SpawnCardsByID(charCardIds)
            : new List<GameObject>();

        float charStartX = -3.85f;
        float charEndX = 5.2f;
        float charY = -3.15f;
        float charZ = -1f;
        float charSpacing = (spawnedCharCards.Count > 1) ? (charEndX - charStartX) / (spawnedCharCards.Count - 1) : 0f;

        for (int i = 0; i < spawnedCharCards.Count; i++)
        {
            float x = (spawnedCharCards.Count == 1) ? (charStartX + charEndX) / 2f : charStartX + charSpacing * i;
            Vector3 pos = new Vector3(x, charY, charZ);
            spawnedCharCards[i].transform.position = pos;
            spawnedShopCards.Add(spawnedCharCards[i]);
            Debug.Log($"[ShopManager] ĳ����ī�� ����: {spawnedCharCards[i].name}, �ε���: {charIndices[i]}, ��ġ: {pos}");
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
        if (setPanel != null)
            setPanel.SetActive(true);
    }

    public void OnClickRefreshShopItems()
    {
        // ���� ���� �� �ٷ� ����
        if (gameMaster == null || gameMaster.coin < 1)
        {
            Debug.LogWarning("[ShopManager] ������ �����Ͽ� ���ΰ�ħ �Ұ�");
            return;
        }

        // ���� ī�� ��� ����
        ClearSpawnedCards();
        gameMaster.SpendCoin(1); // ���� ����

        // --- ����ī�� ���ΰ�ħ ---
        int totalBuff = buffCardListSO != null && buffCardListSO.buffCards != null ? buffCardListSO.buffCards.Count : 0;
        int buffCount = Mathf.Min(4, totalBuff);

        List<int> buffIndices = Enumerable.Range(0, totalBuff).OrderBy(x => Random.value).Take(buffCount).ToList();

        if (cardSpawner == null)
        {
            Debug.LogError("ShopManager: cardSpawner�� null�Դϴ�.");
            return;
        }

        List<GameObject> spawnedBuffCards = (buffIndices.Count > 0)
            ? cardSpawner.SpawnBuffCardsByIndex(buffIndices)
            : new List<GameObject>();

        float buffStartX = -3.5f;
        float buffEndX = 5f;
        float buffY = 0f;
        float buffZ = -1f;
        float buffSpacing = (spawnedBuffCards.Count > 1) ? (buffEndX - buffStartX) / (spawnedBuffCards.Count - 1) : 0f;

        for (int i = 0; i < spawnedBuffCards.Count; i++)
        {
            float x = (spawnedBuffCards.Count == 1) ? (buffStartX + buffEndX) / 2f : buffStartX + buffSpacing * i;
            Vector3 pos = new Vector3(x, buffY, buffZ);
            spawnedBuffCards[i].transform.position = pos;
            spawnedShopCards.Add(spawnedBuffCards[i]);
            var data = spawnedBuffCards[i].GetComponent<CardData>();
            int cardId = data != null ? data.cardId : -1;
            Debug.Log($"[ShopManager] ���ΰ�ħ ����ī��: {spawnedBuffCards[i].name}, cardId: {cardId}, ��ġ: {pos}");
        }

        // --- ĳ����ī�� ���ΰ�ħ (index: 1~57) ---
        int charMinIndex = 1;
        int charMaxIndex = 57;
        int charCount = 6;
        List<int> charIndices = Enumerable.Range(charMinIndex, charMaxIndex).OrderBy(x => Random.value).Take(charCount).ToList();

        List<int> charCardIds = charIndices
            .Select(idx => {
                var prefab = (idx - 1 >= 0 && idx - 1 < CardListSO.cards.Count) ? CardListSO.cards[idx - 1] : null;
                var data = prefab != null ? prefab.GetComponentInChildren<CardData>() : null;
                return data != null ? data.cardId : -1;
            })
            .Where(id => id != -1)
            .ToList();

        List<GameObject> spawnedCharCards = (cardSpawner != null && charCardIds.Count > 0)
            ? cardSpawner.SpawnCardsByID(charCardIds)
            : new List<GameObject>();

        float charStartX = -3.85f;
        float charEndX = 5.2f;
        float charY = -3.15f;
        float charZ = -1f;
        float charSpacing = (spawnedCharCards.Count > 1) ? (charEndX - charStartX) / (spawnedCharCards.Count - 1) : 0f;

        for (int i = 0; i < spawnedCharCards.Count; i++)
        {
            float x = (spawnedCharCards.Count == 1) ? (charStartX + charEndX) / 2f : charStartX + charSpacing * i;
            Vector3 pos = new Vector3(x, charY, charZ);
            spawnedCharCards[i].transform.position = pos;
            spawnedShopCards.Add(spawnedCharCards[i]);
            Debug.Log($"[ShopManager] ���ΰ�ħ ĳ����ī��: {spawnedCharCards[i].name}, �ε���: {charIndices[i]}, ��ġ: {pos}");
        }
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