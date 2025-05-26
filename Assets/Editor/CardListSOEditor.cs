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

        if (GUILayout.Button("카드 프리팹 자동 등록"))
        {
            cardList.cards = new List<GameObject>();

            // "Prefep/Card" 폴더 경로는 Resources가 아님
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefep/Card" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                cardList.cards.Add(prefab);
            }

            EditorUtility.SetDirty(cardList);
            Debug.Log($"총 {cardList.cards.Count}개의 카드가 등록되었습니다.");
        }
    }
}
