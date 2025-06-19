using System.Collections.Generic;
using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    public CardListSO cardListSO; // ī�� ������ ��� SO ����
    public BuffCardListSO buffCardListSO;
    public GameObject deckPrefab;

    public Vector3 deckSpawnPosition;

    public float minX = -3.65f;
    public float maxX = 5f;
    public float fixedY = -4f;
    public float fixedZ = -0.1f;

    public GameObject hoverUI; // ������ �巡���� ����
    public TMPro.TextMeshProUGUI hoverText; // ������ �巡���� ����

    public List<GameObject> SpawnCardsByID(List<int> cardIds)
    {
        List<GameObject> result = new List<GameObject>();

        foreach (int id in cardIds)
        {
            GameObject prefab = FindCardPrefabById(id);
            if (prefab == null)
            {
                Debug.LogWarning($"ID {id}�� �ش��ϴ� ī�� �������� ã�� �� �����ϴ�.");
                continue;
            }

            Vector3 spawnPosition = new Vector3(1f, 1f, 1f); // ���� ��ġ

            GameObject newCard = Instantiate(prefab, spawnPosition, Quaternion.identity);

            // ID ����
            CardData data = newCard.GetComponentInChildren<CardData>();
            if (data != null)
            {
                data.cardId = id;
            }

            // Hover UI ����
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
                Debug.LogWarning("CardListSO�� null �������� �ֽ��ϴ�.");
                continue;
            }

            CardData data = prefab.GetComponentInChildren<CardData>(); // �ڽ� ����
            if (data == null)
            {
                Debug.LogWarning($"������ {prefab.name}�� CardData ������Ʈ�� ã�� �� �����ϴ�.");
                continue;
            }

            if (data.cardId == id)
            {
                //Debug.Log($"������ {prefab.name}�� ID {id}�� ��ġ");
                return prefab;
            }
        }

        Debug.LogWarning($"ID {id}�� �ش��ϴ� �������� ã�� ���߽��ϴ�.");
        return null;
    }

    public List<GameObject> SpawnBuffCardsByIndex(List<int> buffCardIndices)
    {
        Debug.Log($"����ī�� ���� ��û(�ε���): {buffCardIndices.Count}��");
        List<GameObject> result = new List<GameObject>();

        foreach (int idx in buffCardIndices)
        {
            GameObject prefab = FindBuffCardPrefabByIndex(idx);
            if (prefab == null)
            {
                Debug.LogWarning($"�ε��� {idx}�� �ش��ϴ� ����ī�� �������� ã�� �� �����ϴ�.");
                continue;
            }

            Vector3 spawnPosition = new Vector3(1f, 1f, 1f); // ���� ��ġ

            GameObject newCard = Instantiate(prefab, spawnPosition, Quaternion.identity);

            // Hover UI ����
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
            Debug.LogWarning("BuffCardListSO �Ǵ� buffCards ����Ʈ�� null�Դϴ�.");
            return null;
        }
        if (idx < 0 || idx >= buffCardListSO.buffCards.Count)
        {
            Debug.LogWarning($"BuffCardListSO.buffCards�� �ε��� {idx}�� �����ϴ�.");
            return null;
        }
        return buffCardListSO.buffCards[idx];
    }
}
