using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 宝箱配置数据 - 在 GameConfig.asset 中直接配置
/// </summary>
[System.Serializable]
public class TreasureConfigData
{
    [Header("金币")]
    public int goldMin = 23;
    public int goldMax = 27;
    [Range(0, 1)] public float goldChance = 0.5f;

    [Header("遗物稀有度权重")]
    [Range(0, 100)] public int commonWeight = 75;
    [Range(0, 100)] public int uncommonWeight = 25;
    [Range(0, 100)] public int rareWeight = 0;

    public RelicData RollRelic()
    {
        RelicData[] all = Resources.LoadAll<RelicData>("Data_Relics");
        if (all == null || all.Length == 0) return null;

        int totalWeight = commonWeight + uncommonWeight + rareWeight;
        if (totalWeight <= 0) return null;

        int roll = Random.Range(0, totalWeight);
        RelicRarity target;
        if (roll < commonWeight)
            target = RelicRarity.Common;
        else if (roll < commonWeight + uncommonWeight)
            target = RelicRarity.Uncommon;
        else
            target = RelicRarity.Rare;

        List<RelicData> pool = new List<RelicData>();
        foreach (var r in all)
        {
            if (r.rarity == target)
                pool.Add(r);
        }

        if (pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }
}
