using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(CardListSO))]
public class CardListSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CardListSO cardList = (CardListSO)target;

        if (GUILayout.Button("ī�� ������ �ڵ� ���"))
        {
            cardList.cards = new List<GameObject>();

            // "Prefep/Card" ���� ��δ� Resources�� �ƴ�
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefep/Card" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                cardList.cards.Add(prefab);
            }

            EditorUtility.SetDirty(cardList);
            Debug.Log($"�� {cardList.cards.Count}���� ī�尡 ��ϵǾ����ϴ�.");
        }
    }
}
