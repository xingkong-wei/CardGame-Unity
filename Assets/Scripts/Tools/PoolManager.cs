using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 全局对象池管理器
/// 在 GameApp 中初始化，统一管理所有常用对象池
/// </summary>
public static class PoolManager
{
    private static readonly Dictionary<string, ObjectPool> _pools = new Dictionary<string, ObjectPool>();
    private static Transform _poolRoot;

    /// <summary>池根节点（所有池对象放在此节点下，保持场景整洁）</summary>
    public static Transform PoolRoot
    {
        get
        {
            if (_poolRoot == null)
            {
                var go = new GameObject("[ObjectPool]");
                go.SetActive(false); // 根节点不激活，避免影响子对象
                Object.DontDestroyOnLoad(go);
                _poolRoot = go.transform;
            }
            return _poolRoot;
        }
    }

    /// <summary>
    /// 创建或获取一个对象池
    /// </summary>
    public static ObjectPool GetOrCreatePool(string key, GameObject prefab, int preloadCount = 5)
    {
        if (_pools.TryGetValue(key, out var pool))
            return pool;

        pool = new ObjectPool(prefab, PoolRoot, preloadCount, key);
        _pools[key] = pool;
        return pool;
    }

    /// <summary>
    /// 获取已存在的池
    /// </summary>
    public static ObjectPool GetPool(string key)
    {
        _pools.TryGetValue(key, out var pool);
        return pool;
    }

    /// <summary>
    /// 从指定池中获取对象
    /// </summary>
    public static GameObject Get(string key)
    {
        var pool = GetPool(key);
        return pool?.Get();
    }

    /// <summary>
    /// 归还对象到指定池
    /// </summary>
    public static void Release(string key, GameObject obj)
    {
        var pool = GetPool(key);
        pool?.Release(obj);
    }

    /// <summary>
    /// 初始化所有常用对象池（在 GameApp.Start 中调用）
    /// </summary>
    public static void Init()
    {
        // 卡牌对象池（战斗中最频繁）
        var cardPrefab = ResourceCache.Get<GameObject>("UI/CardItem");
        if (cardPrefab != null)
            GetOrCreatePool("CardItem", cardPrefab, 10);

        // 提示对象池
        var tipPrefab = ResourceCache.Get<GameObject>("UI/Tips");
        if (tipPrefab != null)
            GetOrCreatePool("Tips", tipPrefab, 5);

        // 伤害特效对象池
        var damagePrefab = ResourceCache.Get<GameObject>("UI/DamageEffect");
        if (damagePrefab != null)
            GetOrCreatePool("DamageEffect", damagePrefab, 3);

        // Buff 图标对象池
        var buffIconPrefab = ResourceCache.Get<GameObject>("UI/BuffIcon");
        if (buffIconPrefab != null)
            GetOrCreatePool("BuffIcon", buffIconPrefab, 8);
    }

    /// <summary>
    /// 清除所有池（切换场景时调用）
    /// </summary>
    public static void ClearAll()
    {
        foreach (var pool in _pools.Values)
            pool.Clear();
        _pools.Clear();
    }
}
