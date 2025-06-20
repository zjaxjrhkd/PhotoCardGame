using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("카드 데이터")]
    public BuffCardListSO buffCardListSO;
    public CardListSO CardListSO;

    [Header("연결 대상")]
    public GameObject shopPanel; // 상점 전체 UI 패널
    public GameObject setPanel;  // 세트 UI 패널

    public GameMaster gameMaster;
    public CardSpawner cardSpawner;

    // OpenShop에서 생성한 카드 추적용
    private List<GameObject> spawnedShopCards = new List<GameObject>();

    public void OpenShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(true);
        if (setPanel != null)
            setPanel.SetActive(false);
        ClearSpawnedCards();
        Debug.Log("[ShopManager] 상점 열림");

        // 1. 버프카드 4장 생성
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
            Debug.Log($"[ShopManager] 버프카드 생성: {spawnedBuffCards[i].name}, cardId: {cardId}, 위치: {pos}");
        }

        // 2. 캐릭터 카드 6장 생성 (index: 1~57)
        int charMinIndex = 1;
        int charMaxIndex = 57;
        int charCount = 6;
        List<int> charIndices = Enumerable.Range(charMinIndex, charMaxIndex).OrderBy(x => Random.value).Take(charCount).ToList();
        Debug.Log($"[ShopManager] 랜덤 선택된 캐릭터 카드 인덱스(1~57): {string.Join(",", charIndices)}");

        List<int> charCardIds = charIndices
            .Select(idx => {
                // CardListSO.cards는 0부터 시작하므로 idx-1
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
            Debug.Log($"[ShopManager] 캐릭터카드 생성: {spawnedCharCards[i].name}, 인덱스: {charIndices[i]}, 위치: {pos}");
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
        if (setPanel != null)
            setPanel.SetActive(true);
    }

    public void OnClickRefreshShopItems()
    {
        // 코인 부족 시 바로 리턴
        if (gameMaster == null || gameMaster.coin < 1)
        {
            Debug.LogWarning("[ShopManager] 코인이 부족하여 새로고침 불가");
            return;
        }

        // 기존 카드 모두 제거
        ClearSpawnedCards();
        gameMaster.SpendCoin(1); // 코인 차감

        // --- 버프카드 새로고침 ---
        int totalBuff = buffCardListSO != null && buffCardListSO.buffCards != null ? buffCardListSO.buffCards.Count : 0;
        int buffCount = Mathf.Min(4, totalBuff);

        List<int> buffIndices = Enumerable.Range(0, totalBuff).OrderBy(x => Random.value).Take(buffCount).ToList();

        if (cardSpawner == null)
        {
            Debug.LogError("ShopManager: cardSpawner가 null입니다.");
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
            Debug.Log($"[ShopManager] 새로고침 버프카드: {spawnedBuffCards[i].name}, cardId: {cardId}, 위치: {pos}");
        }

        // --- 캐릭터카드 새로고침 (index: 1~57) ---
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
            Debug.Log($"[ShopManager] 새로고침 캐릭터카드: {spawnedCharCards[i].name}, 인덱스: {charIndices[i]}, 위치: {pos}");
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