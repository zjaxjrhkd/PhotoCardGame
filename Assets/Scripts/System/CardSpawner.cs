using System.Collections.Generic;
using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    public CardListSO cardListSO; // 카드 프리팹 목록 SO 연결
    public BuffCardListSO buffCardListSO;
    public GameObject deckPrefab;

    public Vector3 deckSpawnPosition;

    public float minX = -3.65f;
    public float maxX = 5f;
    public float fixedY = -4f;
    public float fixedZ = -0.1f;

    public GameObject hoverUI; // 씬에서 드래그해 연결
    public TMPro.TextMeshProUGUI hoverText; // 씬에서 드래그해 연결

    public List<GameObject> SpawnCardsByID(List<int> cardIds)
    {
        List<GameObject> result = new List<GameObject>();

        foreach (int id in cardIds)
        {
            GameObject prefab = FindCardPrefabById(id);
            if (prefab == null)
            {
                Debug.LogWarning($"ID {id}에 해당하는 카드 프리팹을 찾을 수 없습니다.");
                continue;
            }

            Vector3 spawnPosition = new Vector3(1f, 1f, 1f); // 고정 위치

            GameObject newCard = Instantiate(prefab, spawnPosition, Quaternion.identity);

            // ID 설정
            CardData data = newCard.GetComponentInChildren<CardData>();
            if (data != null)
            {
                data.cardId = id;
            }

            // Hover UI 주입
            CardState state = newCard.GetComponentInChildren<CardState>();
            if (state != null)
            {
                state.hoverUI = hoverUI;
                state.hoverText = hoverText;
            }

            result.Add(newCard);
        }

        return result;
    }





    private GameObject FindCardPrefabById(int id)
    {
        foreach (var prefab in cardListSO.cards)
        {
            if (prefab == null)
            {
                Debug.LogWarning("CardListSO에 null 프리팹이 있습니다.");
                continue;
            }

            CardData data = prefab.GetComponentInChildren<CardData>(); // 자식 포함
            if (data == null)
            {
                Debug.LogWarning($"프리팹 {prefab.name}에 CardData 컴포넌트를 찾을 수 없습니다.");
                continue;
            }

            if (data.cardId == id)
            {
                //Debug.Log($"프리팹 {prefab.name}이 ID {id}와 일치");
                return prefab;
            }
        }

        Debug.LogWarning($"ID {id}에 해당하는 프리팹을 찾지 못했습니다.");
        return null;
    }

    public List<GameObject> SpawnBuffCardsByIndex(List<int> buffCardIndices)
    {
        Debug.Log($"버프카드 생성 요청(인덱스): {buffCardIndices.Count}개");
        List<GameObject> result = new List<GameObject>();

        foreach (int idx in buffCardIndices)
        {
            GameObject prefab = FindBuffCardPrefabByIndex(idx);
            if (prefab == null)
            {
                Debug.LogWarning($"인덱스 {idx}에 해당하는 버프카드 프리팹을 찾을 수 없습니다.");
                continue;
            }

            Vector3 spawnPosition = new Vector3(1f, 1f, 1f); // 고정 위치

            GameObject newCard = Instantiate(prefab, spawnPosition, Quaternion.identity);

            // Hover UI 주입
            CardState state = newCard.GetComponentInChildren<CardState>();
            if (state != null)
            {
                state.hoverUI = hoverUI;
                state.hoverText = hoverText;
            }

            result.Add(newCard);
        }

        return result;
    }

    private GameObject FindBuffCardPrefabByIndex(int idx)
    {
        if (buffCardListSO == null || buffCardListSO.buffCards == null)
        {
            Debug.LogWarning("BuffCardListSO 또는 buffCards 리스트가 null입니다.");
            return null;
        }
        if (idx < 0 || idx >= buffCardListSO.buffCards.Count)
        {
            Debug.LogWarning($"BuffCardListSO.buffCards에 인덱스 {idx}가 없습니다.");
            return null;
        }
        return buffCardListSO.buffCards[idx];
    }
}
