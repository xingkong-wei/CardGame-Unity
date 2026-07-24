using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private AudioSource bgmSource;
    private AudioConfig audioConfig;

    public float MainVolume { get; private set; } = 0.5f;
    public float BattleVolume { get; private set; } = 0.5f;
    public float EffectVolume { get; private set; } = 0.5f;

    private string currentBGMName;
    private VolumeCategory currentVolumeCategory;
    public System.Action<float> OnEffectVolumeChanged;

    private void Awake()
    {
        Instance = this;
    }

    public void Init()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        audioConfig = AudioConfig.Instance;
    }

    // 播放BGM
    public void PlayBGM(string name, bool isLoop = true)
    {
        currentBGMName = name;

        var entry = audioConfig?.GetBgm(name);
        if (entry == null || entry.clip == null)
        {
            Debug.LogWarning($"[AudioManager] BGM '{name}' 未在 AudioConfig 中配置！");
            return;
        }

        currentVolumeCategory = entry.volumeCategory;
        bgmSource.clip = entry.clip;
        bgmSource.loop = isLoop;
        bgmSource.volume = GetVolume(entry.volumeCategory);
        bgmSource.Play();
    }

    // 播放音效
    public void PlayEffect(string name)
    {
        AudioClip clip = audioConfig?.GetSfx(name);
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, EffectVolume);
        }
        else
        {
            Debug.LogWarning($"[AudioManager] 音效 '{name}' 未在 AudioConfig 中配置！");
        }
    }

    private float GetVolume(VolumeCategory category)
    {
        return category == VolumeCategory.Battle ? BattleVolume : MainVolume;
    }

    // 设置主音量
    public void SetMainVolume(float value)
    {
        MainVolume = value;
        if (currentVolumeCategory == VolumeCategory.Main && bgmSource != null)
            bgmSource.volume = MainVolume;
    }

    // 设置战斗音量
    public void SetBattleVolume(float value)
    {
        BattleVolume = value;
        if (currentVolumeCategory == VolumeCategory.Battle && bgmSource != null)
            bgmSource.volume = BattleVolume;
    }

    // 设置音效音量
    public void SetEffectVolume(float value)
    {
        EffectVolume = value;
        OnEffectVolumeChanged?.Invoke(EffectVolume);
    }
}
