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

    public Slider volumeSlider; // �ν����Ϳ��� �Ҵ�

    public void Init()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // SFX�� AudioSource�� �ν����Ϳ��� �Ҵ� ����
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetVolume);
            volumeSlider.value = audioSource != null ? audioSource.volume : 1f;
        }
    }

    public void SetVolume(float value)
    {
        if (audioSource != null)
            audioSource.volume = value;
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