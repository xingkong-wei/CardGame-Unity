using UnityEngine;

/// <summary>
/// 药水掉落管理器 - 怜悯计数器系统
/// 每次战斗结算时判定是否掉落药水，未掉落则下次概率递增
/// 进入新 Act 或成功掉落药水后重置
/// </summary>
public static class PotionDropManager
{
    // 基础概率
    private const float BASE_DROP_RATE = 0.40f;
    private const float ELITE_BONUS = 0.125f;
    private const float STEP_INCREMENT = 0.10f;
    private const float MIN_RATE = 0f;
    private const float MAX_RATE = 1.0f;

    // 稀有度权重: Common=65, Uncommon=25, Rare=10
    private const int WEIGHT_COMMON = 65;
    private const int WEIGHT_UNCOMMON = 25;
    private const int WEIGHT_RARE = 10;

    // 当前累计概率
    private static float currentDropRate = BASE_DROP_RATE;
    private static bool initialized = false;

    /// <summary>
    /// 重置计数器（进入新 Act 或掉落药水后调用）
    /// </summary>
    public static void ResetCounter()
    {
        currentDropRate = BASE_DROP_RATE;
        initialized = true;
    }

    /// <summary>
    /// 初始化（游戏开始时调用一次）
    /// </summary>
    public static void Init()
    {
        if (!initialized)
            ResetCounter();
    }

    /// <summary>
    /// 战斗结算时判定是否掉落药水
    /// </summary>
    /// <param name="isElite">是否为精英战斗</param>
    /// <returns>掉落的药水，未掉落返回 null</returns>
    public static PotionData RollDrop(bool isElite)
    {
        Init();

        float rate = currentDropRate + (isElite ? ELITE_BONUS : 0f);
        rate = Mathf.Clamp(rate, MIN_RATE, MAX_RATE);

        bool dropped = Random.value < rate;

        if (dropped)
        {
            // 掉落成功，重置计数器
            ResetCounter();
            return RollRarity();
        }
        else
        {
            // 未掉落，增加概率
            currentDropRate = Mathf.Min(currentDropRate + STEP_INCREMENT, MAX_RATE);
            return null;
        }
    }

    /// <summary>
    /// 按稀有度权重随机选一瓶药水
    /// </summary>
    private static PotionData RollRarity()
    {
        PotionData[] all = Resources.LoadAll<PotionData>("Date_Potion");
        if (all == null || all.Length == 0) return null;

        // 按稀有度分组加权
        System.Collections.Generic.Dictionary<PotionRarity, int> weights = new System.Collections.Generic.Dictionary<PotionRarity, int>
        {
            { PotionRarity.Common, WEIGHT_COMMON },
            { PotionRarity.Uncommon, WEIGHT_UNCOMMON },
            { PotionRarity.Rare, WEIGHT_RARE },
        };

        System.Collections.Generic.List<PotionData> pool = new System.Collections.Generic.List<PotionData>();
        foreach (var p in all)
        {
            if (weights.TryGetValue(p.rarity, out int w))
            {
                for (int i = 0; i < w; i++)
                    pool.Add(p);
            }
        }

        if (pool.Count == 0) return null;
        return pool[UnityEngine.Random.Range(0, pool.Count)];
    }

    /// <summary>
    /// 获取当前掉落概率（调试用）
    /// </summary>
    public static float GetCurrentDropRate() => currentDropRate;

    // ===== 遗物掉落 =====

    /// <summary>
    /// 精英战斗遗物掉落：Common 50%, Uncommon 36%, Rare 14%
    /// </summary>
    public static RelicData RollEliteRelic()
    {
        RelicData[] all = Resources.LoadAll<RelicData>("Data_Relics");
        System.Collections.Generic.List<RelicData> pool = new System.Collections.Generic.List<RelicData>();

        int roll = Random.Range(0, 100);
        RelicRarity target;
        if (roll < 50) target = RelicRarity.Common;
        else if (roll < 86) target = RelicRarity.Uncommon;
        else target = RelicRarity.Rare;

        foreach (var r in all)
        {
            if (r.rarity == target)
                pool.Add(r);
        }

        if (pool.Count == 0) return null;
        return PickWeightedRelic(pool);
    }

    /// <summary>
    /// Boss 遗物掉落：必定 Uncommon 或 Rare
    /// </summary>
    public static RelicData RollBossRelic()
    {
        RelicData[] all = Resources.LoadAll<RelicData>("Data_Relics");
        System.Collections.Generic.List<RelicData> pool = new System.Collections.Generic.List<RelicData>();

        foreach (var r in all)
        {
            if (r.rarity == RelicRarity.Uncommon || r.rarity == RelicRarity.Rare)
                pool.Add(r);
        }

        if (pool.Count == 0) return null;
        return PickWeightedRelic(pool);
    }

    /// <summary>
    /// 从遗物池中按权重随机选取一个遗物。
    /// 已持有的遗物权重降低（默认降至 20%），避免重复出现。
    /// </summary>
    /// <param name="pool">候选遗物列表</param>
    /// <param name="ownedWeightMultiplier">已持有遗物的权重倍率（0~1），默认 0.2 即降至 20%</param>
    public static RelicData PickWeightedRelic(System.Collections.Generic.List<RelicData> pool, float ownedWeightMultiplier = 0.2f)
    {
        if (pool == null || pool.Count == 0) return null;

        // 获取当前已持有的遗物 id 集合
        System.Collections.Generic.HashSet<string> ownedIds = new System.Collections.Generic.HashSet<string>();
        if (FightManager.Instance != null && FightManager.Instance.relicList != null)
        {
            foreach (var relic in FightManager.Instance.relicList)
            {
                if (relic != null && !string.IsNullOrEmpty(relic.id))
                    ownedIds.Add(relic.id);
            }
        }

        // 所有权重相同 → 直接等概率随机
        if (ownedIds.Count == 0)
            return pool[Random.Range(0, pool.Count)];

        // 计算加权总权重
        float totalWeight = 0f;
        System.Collections.Generic.List<float> cumulativeWeights = new System.Collections.Generic.List<float>();
        foreach (var r in pool)
        {
            bool owned = !string.IsNullOrEmpty(r.id) && ownedIds.Contains(r.id);
            float w = owned ? ownedWeightMultiplier : 1f;
            totalWeight += w;
            cumulativeWeights.Add(totalWeight);
        }

        // 所有遗物都已持有且权重极低 → 仍然随机一个
        float roll = Random.Range(0f, totalWeight);
        for (int i = 0; i < pool.Count; i++)
        {
            if (roll < cumulativeWeights[i])
                return pool[i];
        }

        return pool[pool.Count - 1];
    }
}
