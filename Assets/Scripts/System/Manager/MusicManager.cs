using UnityEngine;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip jooinMusic;
    public AudioClip sitryMusic;
    public AudioClip limiMusic;
    public AudioClip ianaMusic;
    public AudioClip razMusic;
    public AudioClip noiMusic;
    public AudioClip beldirMusic;

    public Slider volumeSlider; // 인스펙터에서 할당

    public void Init()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // 슬라이더가 할당되어 있으면 이벤트 연결
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetVolume);
            // 슬라이더 값과 오디오 볼륨 동기화
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
            Debug.LogWarning("[MusicManager] AudioSource 또는 Stage Music이 할당되지 않았습니다.");
        }
    }
}