using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SpriteListGenerator
{
    [MenuItem("Tools/Build SpriteList from Resources/Image/Stage")]
    public static void GenerateSpriteList()
    {
        // ���� ���� ���
        string savePath = "Assets/Resources/Data/SpriteList.asset";

        // ScriptableObject ����
        SpriteListSO spriteList = ScriptableObject.CreateInstance<SpriteListSO>();
        spriteList.sprites = new List<Sprite>();

        // Resources/Image/Stage ��� ���� ��� �̹��� ���� ã��
        string resourcePath = "Resources/Image/Stage";
        string fullPath = Path.Combine(Application.dataPath, resourcePath);

        if (!Directory.Exists(fullPath))
        {
            Debug.LogError("������ �������� �ʽ��ϴ�: " + fullPath);
            return;
        }

        string[] files = Directory.GetFiles(fullPath, "*.png", SearchOption.AllDirectories);

        foreach (string file in files)
        {
            string assetPath = "Assets" + file.Replace(Application.dataPath, "").Replace("\\", "/");
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite != null)
            {
                spriteList.sprites.Add(sprite);
            }
        }

        // ���� ���� ������ �����
        AssetDatabase.DeleteAsset(savePath);
        AssetDatabase.CreateAsset(spriteList, savePath);
        AssetDatabase.SaveAssets();

        Debug.Log($"SpriteList.asset ���� �Ϸ�! ��ϵ� ��������Ʈ ��: {spriteList.sprites.Count}");
    }
}


