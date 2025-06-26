using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;      // BGM��
    public AudioSource sfxSource;        // SFX�� (�ν����Ϳ��� �Ҵ�)

    // BGM
    public AudioClip jooinMusic;
    public AudioClip sitryMusic;
    public AudioClip limiMusic;
    public AudioClip ianaMusic;
    public AudioClip razMusic;
    public AudioClip noiMusic;
    public AudioClip beldirMusic;

    // SFX
    public AudioClip sfxCardSelect;
    public AudioClip sfxCardRemove;
    public AudioClip sfxBuyBuff;
    public AudioClip sfxComboComplete;
    public AudioClip sfxDropCard;
    public AudioClip sfxFlipCard;
    public AudioClip sfxGetCoin;
    public AudioClip sfxUIClick;
    public AudioClip sfxCardEffect;

    private GameData gameData; // GameData ������


    public Slider volumeSlider; // �ν����Ϳ��� �Ҵ�
    public Slider sfxvolumeSlider; // SFX ���� ������ �����̴� (�ν����Ϳ��� �Ҵ�)


    private void Awake()
    {
        // GameData ������Ʈ���� GameData ������Ʈ ��������
        gameData = FindObjectOfType<GameData>();
        if (gameData == null)
        {
            Debug.LogError("[MusicManager] GameData ������Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    public void Init()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetVolume);
            // GameData�� �������� �����̴��� �ݿ�
            if (gameData != null)
                volumeSlider.value = gameData.bgmVolume;
            else
                volumeSlider.value = audioSource != null ? audioSource.volume : 1f;
        }

        if (sfxvolumeSlider != null)
        {
            sfxvolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            sfxvolumeSlider.value = sfxSource != null ? sfxSource.volume : 1f;
        }
    }

    public void SetVolume(float value)
    {
        if (audioSource != null)
            audioSource.volume = value;
    }

    // SFX ���� ���� �޼��� �߰�
    public void SetSFXVolume(float value)
    {
        if (sfxSource != null)
            sfxSource.volume = value;
    }

    public void PlayStageMusic(StageManager.StageType stageType)
    {
        AudioClip clip = null;
        switch (stageType)
        {
            case StageManager.StageType.Churos:
                clip = jooinMusic;
                break;
            case StageManager.StageType.Chami:
                clip = sitryMusic;
                break;
            case StageManager.StageType.Donddatge:
                clip = limiMusic;
                break;
            case StageManager.StageType.Muddung:
                clip = ianaMusic;
                break;
            case StageManager.StageType.Rasky:
                clip = razMusic;
                break;
            case StageManager.StageType.Roze:
                clip = noiMusic;
                break;
            case StageManager.StageType.Yasu:
                clip = beldirMusic;
                break;
        }

        if (audioSource != null && clip != null)
        {
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("[MusicManager] AudioSource �Ǵ� Stage Music�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    // SFX ��� �޼���
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }

    // �� SFX���� ���� �޼��� (�ʿ��)
    public void PlayCardSelectSFX() => PlaySFX(sfxCardSelect);
    public void PlayCardRemoveSFX() => PlaySFX(sfxCardRemove);
    public void PlayBuyBuffSFX() => PlaySFX(sfxBuyBuff);
    public void PlayComboCompleteSFX() => PlaySFX(sfxComboComplete);
    public void PlayDropCardSFX() => PlaySFX(sfxDropCard);
    public void PlayFlipCardSFX() => PlaySFX(sfxFlipCard);
    public void PlayGetCoinSFX() => PlaySFX(sfxGetCoin);
    public void PlayUIClickSFX() => PlaySFX(sfxUIClick);
    public void PlayCardEffectSFX() => PlaySFX(sfxCardEffect);
}