using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(BuffCardListSO))]
public class BuffCardListSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BuffCardListSO buffListSO = (BuffCardListSO)target;

        if (GUILayout.Button("Prefep/BuffCard �������� �ڵ� ���"))
        {
            string folderPath = "Assets/Prefep/BuffCard";
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

            buffListSO.buffCards.Clear();

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab != null)
                {
                    buffListSO.buffCards.Add(prefab);
                }
            }

            EditorUtility.SetDirty(buffListSO);
            AssetDatabase.SaveAssets();

            Debug.Log($"{buffListSO.buffCards.Count}���� �������� BuffCardListSO�� ����߽��ϴ�.");
        }
    }
}
