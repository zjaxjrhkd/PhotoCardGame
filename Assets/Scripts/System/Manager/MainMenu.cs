using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject option; // �ν����Ϳ��� �Ҵ��� �ɼ� GameObject
    public GameData gameData; // �ν����Ϳ��� �Ҵ��� GameData ��ũ��Ʈ

    public bool isOptionOpen = false; // �ɼ� â ���� ����

    [Header("������� ����")]
    public AudioSource bgmSource; // �ν����Ϳ��� �Ҵ�
    public AudioClip bgmClip;     // �ν����Ϳ��� �Ҵ�

    [Header("���� �ɼ�")]
    public Slider soundSlider;    // �ɼ�â�� �����̴�(�ν����Ϳ��� �Ҵ�)

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
            soundSlider.value = gameData.bgmVolume; // GameData ������ �����̴� �ʱ�ȭ
            Debug.Log(gameData.bgmVolume);
            bgmSource.volume = gameData.bgmVolume;  // GameData ������ ���� �ʱ�ȭ
            soundSlider.onValueChanged.AddListener(SetVolume); // �� �����̴� �̺�Ʈ ����
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

        // GameData�� ���� �� ����
        if (gameData != null)
            gameData.bgmVolume = value;
    }

    public void LoadVolume()
    {
        if (gameData != null)
        {
            // GameData�� ���� ���� �����̴��� ����� �ҽ��� �ݿ�
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