using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 遗物库存管理器
/// 负责：遗物列表、添加/移除、初始化
/// </summary>
public class RelicInventory
{
    public List<RelicData> RelicList = new List<RelicData>();

    public void Init()
    {
        RelicList.Clear();
        RelicManager.Instance.Clear();
        var cfg = GameConfig.Instance;
        if (cfg.initialRelics != null)
        {
            foreach (var relic in cfg.initialRelics)
            {
                if (relic != null) Add(relic);
            }
        }
    }

    public void Add(RelicData relicData)
    {
        if (relicData == null) return;

        RelicList.Add(relicData);

        if (!string.IsNullOrEmpty(relicData.scriptName))
        {
            System.Type type = System.Type.GetType(relicData.scriptName);
            if (type != null && typeof(RelicBase).IsAssignableFrom(type))
            {
                RelicBase relic = System.Activator.CreateInstance(type) as RelicBase;
                relic.Init(relicData);
                RelicManager.Instance.AddRelic(relic);
            }
        }

        if (FightManager.CachedRelicsUI != null)
            FightManager.CachedRelicsUI.RefreshUI();
    }

    public void Remove(RelicData relicData)
    {
        if (relicData == null) return;
        RelicList.Remove(relicData);

        if (FightManager.CachedRelicsUI != null)
            FightManager.CachedRelicsUI.RefreshUI();
    }
}
