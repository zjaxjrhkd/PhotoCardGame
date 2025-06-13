using System.Collections.Generic;
using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    public CardListSO cardListSO; // 카드 프리팹 목록 SO 연결
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

}
