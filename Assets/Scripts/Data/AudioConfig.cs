using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音量类别
/// </summary>
public enum VolumeCategory
{
    Main,   // 主音量（菜单、地图等）
    Battle  // 战斗音量
}

/// <summary>
/// BGM 配置条目
/// </summary>
[System.Serializable]
public class BgmEntry
{
    [Tooltip("BGM 键名（代码中调用 PlayBGM 时使用的名称）")]
    public string key;
    [Tooltip("场景说明（给策划/美术看的备注）")]
    public string description;
    [Tooltip("音频文件（直接拖拽）")]
    public AudioClip clip;
    [Tooltip("音量类别")]
    public VolumeCategory volumeCategory = VolumeCategory.Main;
}

/// <summary>
/// 音效配置条目
/// </summary>
[System.Serializable]
public class SfxEntry
{
    [Tooltip("音效键名（代码中调用 PlayEffect 时使用的名称）")]
    public string key;
    [Tooltip("场景说明（给策划/美术看的备注）")]
    public string description;
    [Tooltip("音频文件（直接拖拽）")]
    public AudioClip clip;
}

/// <summary>
/// 音频配置 - 集中管理所有 BGM 和音效
/// 创建后放入 Resources 目录，在 Inspector 中拖拽替换音频即可，无需改代码
/// </summary>
[CreateAssetMenu(fileName = "AudioConfig", menuName = "Game/AudioConfig")]
public class AudioConfig : ScriptableObject
{
    [Header(" BGM 背景音乐 ")]
    public List<BgmEntry> bgmList = new List<BgmEntry>();

    [Header(" SFX 音效 ")]
    public List<SfxEntry> sfxList = new List<SfxEntry>();

    // 运行时查找字典
    private Dictionary<string, BgmEntry> bgmDict;
    private Dictionary<string, AudioClip> sfxDict;

    /// <summary>
    /// 构建查找字典（在 Init 时调用一次）
    /// </summary>
    public void BuildLookup()
    {
        bgmDict = new Dictionary<string, BgmEntry>();
        foreach (var entry in bgmList)
        {
            if (!string.IsNullOrEmpty(entry.key) && entry.clip != null)
                bgmDict[entry.key] = entry;
        }

        sfxDict = new Dictionary<string, AudioClip>();
        foreach (var entry in sfxList)
        {
            if (!string.IsNullOrEmpty(entry.key) && entry.clip != null)
                sfxDict[entry.key] = entry.clip;
        }
    }

    /// <summary>
    /// 根据键名查找 BGM 配置
    /// </summary>
    public BgmEntry GetBgm(string key)
    {
        if (bgmDict == null) BuildLookup();
        bgmDict.TryGetValue(key, out BgmEntry entry);
        return entry;
    }

    /// <summary>
    /// 根据键名查找音效
    /// </summary>
    public AudioClip GetSfx(string key)
    {
        if (sfxDict == null) BuildLookup();
        sfxDict.TryGetValue(key, out AudioClip clip);
        return clip;
    }

    // ===== 单例 =====

    private static AudioConfig _instance;
    public static AudioConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<AudioConfig>("AudioConfig");
                if (_instance == null)
                    Debug.LogError("[AudioConfig] 未找到 Resources/AudioConfig.asset，请创建！");
                else
                    _instance.BuildLookup();
            }
            return _instance;
        }
    }

    public static void ClearCache()
    {
        _instance = null;
    }
}
