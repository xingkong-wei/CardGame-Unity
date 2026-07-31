using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 资源缓存管理器
/// 预加载常用 Resources 资源，避免重复 IO 和同步加载
/// 在游戏启动时调用 Init() 一次
/// </summary>
public static class ResourceCache
{
    private static readonly Dictionary<string, Object> _cache = new Dictionary<string, Object>();
    private static bool _initialized = false;

    /// <summary>是否已初始化</summary>
    public static bool Initialized => _initialized;

    /// <summary>
    /// 初始化缓存，预加载所有常用资源
    /// 在 GameApp.Awake() 或 Start() 中调用一次
    /// </summary>
    public static void Init()
    {
        if (_initialized) return;
        _initialized = true;

        // ===== UI 预制体 =====
        Preload<GameObject>("UI/CardItem");
        Preload<GameObject>("UI/Tips");
        Preload<GameObject>("UI/DamageEffect");
        Preload<GameObject>("UI/BuffIcon");
        Preload<GameObject>("UI/ExitUI");
        Preload<GameObject>("UI/HpItem");
        Preload<GameObject>("UI/actionIcon");
        Preload<GameObject>("UI/RelicIcon");
        Preload<GameObject>("UI/PotionIcon");

        // ===== 常用材质 =====
        Preload<Material>("Mats/outline");

        // ===== 音频（可选，按需） =====
        // Preload<AudioClip>("Sounds/xxx");
    }

    /// <summary>
    /// 从缓存获取资源，如果未缓存则同步加载并缓存
    /// </summary>
    public static T Get<T>(string path) where T : Object
    {
        if (_cache.TryGetValue(path, out Object obj) && obj != null)
            return obj as T;

        T loaded = Resources.Load<T>(path);
        if (loaded != null)
            _cache[path] = loaded;

        return loaded;
    }

    /// <summary>
    /// 预加载指定路径的资源到缓存
    /// </summary>
    public static void Preload<T>(string path) where T : Object
    {
        if (_cache.ContainsKey(path)) return;
        T loaded = Resources.Load<T>(path);
        if (loaded != null)
            _cache[path] = loaded;
    }

    /// <summary>
    /// 预加载 Sprite（用于卡牌/药水图标等）
    /// 如果已在缓存中则跳过
    /// </summary>
    public static Sprite GetSprite(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        path = path.Trim();
        if (_cache.TryGetValue(path, out Object obj) && obj != null)
            return obj as Sprite;

        Sprite loaded = Resources.Load<Sprite>(path);
        if (loaded != null)
            _cache[path] = loaded;

        return loaded;
    }

    /// <summary>
    /// 清除所有缓存（切换场景或退出时调用）
    /// </summary>
    public static void Clear()
    {
        _cache.Clear();
        _initialized = false;
    }
}
