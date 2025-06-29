using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject option; // 인스펙터에서 할당할 옵션 GameObject
    public GameData gameData; // 인스펙터에서 할당할 GameData 스크립트

    public bool isOptionOpen = false; // 옵션 창 열림 여부

    [Header("배경음악 설정")]
    public AudioSource bgmSource; // 인스펙터에서 할당
    public AudioClip bgmClip;     // 인스펙터에서 할당

    [Header("사운드 옵션")]
    public Slider soundSlider;    // 옵션창의 슬라이더(인스펙터에서 할당)

    void Start()
    {
        if (gameData == null)
            gameData = FindObjectOfType<GameData>();

        option.SetActive(false);

        if (bgmSource == null)
            bgmSource = GetComponent<AudioSource>();

        if (bgmSource != null && bgmClip != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
        Debug.Log(gameData.bgmVolume);

        if (soundSlider != null && gameData != null)
        {
            soundSlider.value = gameData.bgmVolume; // GameData 값으로 슬라이더 초기화
            Debug.Log(gameData.bgmVolume);
            bgmSource.volume = gameData.bgmVolume;  // GameData 값으로 볼륨 초기화
            soundSlider.onValueChanged.AddListener(SetVolume); // ★ 슬라이더 이벤트 연결
        }
    }

    void Update()
    {
    }

    public void OnClickNewGame()
    {
        SceneManager.LoadScene("1.GameScene");
    }

    public void OnClickTutorial()
    {
        gameData.isTutorial = true;
        SceneManager.LoadScene("1.GameScene");
    }

    public void SetVolume(float value)
    {
        if (bgmSource != null)
            bgmSource.volume = value;

        // GameData에 볼륨 값 저장
        if (gameData != null)
            gameData.bgmVolume = value;
    }

    public void LoadVolume()
    {
        if (gameData != null)
        {
            // GameData의 볼륨 값을 슬라이더와 오디오 소스에 반영
            if (soundSlider != null)
                soundSlider.value = gameData.bgmVolume;
            if (bgmSource != null)
                bgmSource.volume = gameData.bgmVolume;
        }
    }

    public void OnClickOption()
    {
        if (!isOptionOpen)
        {
            option.SetActive(true);
            isOptionOpen = true;
        }
        else
        {
            gameData.bgmVolume = bgmSource.volume;
            Debug.Log(bgmSource.volume);
            Debug.Log(gameData.bgmVolume);

            option.SetActive(false);
            isOptionOpen = false;
        }
    }
}