using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }

    public bool isTutorial = false; // 튜토리얼 여부
    public float bgmVolume = 0.5f;

    void Awake()
    {
        Screen.SetResolution(1920, 1080, false);

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
}