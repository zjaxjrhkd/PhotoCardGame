using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SpriteListGenerator
{
    [MenuItem("Tools/Build SpriteList from Resources/Image/Stage")]
    public static void GenerateSpriteList()
    {
        // 에셋 생성 경로
        string savePath = "Assets/Resources/Data/SpriteList.asset";

        // ScriptableObject 생성
        SpriteListSO spriteList = ScriptableObject.CreateInstance<SpriteListSO>();
        spriteList.sprites = new List<Sprite>();

        // Resources/Image/Stage 경로 안의 모든 이미지 파일 찾기
        string resourcePath = "Resources/Image/Stage";
        string fullPath = Path.Combine(Application.dataPath, resourcePath);

        if (!Directory.Exists(fullPath))
        {
            Debug.LogError("폴더가 존재하지 않습니다: " + fullPath);
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

        // 기존 파일 있으면 덮어쓰기
        AssetDatabase.DeleteAsset(savePath);
        AssetDatabase.CreateAsset(spriteList, savePath);
        AssetDatabase.SaveAssets();

        Debug.Log($"SpriteList.asset 생성 완료! 등록된 스프라이트 수: {spriteList.sprites.Count}");
    }
}


