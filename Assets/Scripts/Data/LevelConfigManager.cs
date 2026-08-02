using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 关卡配置管理器 — 替代 level.txt 的解析方式
/// 自动加载 Resources/Data_Level/ 下所有 LevelConfig ScriptableObject
/// </summary>
public class LevelConfigManager
{
    public static LevelConfigManager Instance = new LevelConfigManager();

    private Dictionary<int, LevelConfig> levelDict = new Dictionary<int, LevelConfig>();
    private Dictionary<int, List<int>> islandLevelsMap = new Dictionary<int, List<int>>();

    public void Init()
    {
        levelDict.Clear();
        islandLevelsMap.Clear();

        LevelConfig[] configs = Resources.LoadAll<LevelConfig>("Data_Level");
        foreach (LevelConfig cfg in configs)
        {
            if (cfg == null) continue;

            levelDict[cfg.levelId] = cfg;

            if (!islandLevelsMap.ContainsKey(cfg.islandIndex))
                islandLevelsMap[cfg.islandIndex] = new List<int>();
            islandLevelsMap[cfg.islandIndex].Add(cfg.levelId);
        }
    }

    /// <summary>
    /// 根据关卡 ID 获取配置
    /// </summary>
    public LevelConfig GetLevelById(int levelId)
    {
        levelDict.TryGetValue(levelId, out LevelConfig cfg);
        return cfg;
    }

    /// <summary>
    /// 获取指定岛屿的所有关卡 ID 列表
    /// </summary>
    public List<int> GetLevelIdsByIsland(int islandIndex)
    {
        islandLevelsMap.TryGetValue(islandIndex, out List<int> list);
        return list ?? new List<int>();
    }

    /// <summary>
    /// 获取所有关卡配置
    /// </summary>
    public IEnumerable<LevelConfig> GetAllLevels()
    {
        return levelDict.Values;
    }
}
