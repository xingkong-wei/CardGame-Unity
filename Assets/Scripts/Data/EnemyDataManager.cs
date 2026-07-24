using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人数据管理器 - 从 ScriptableObject 加载敌人数据
/// </summary>
public class EnemyDataManager
{
    private static EnemyDataManager instance;
    public static EnemyDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new EnemyDataManager();
            }
            return instance;
        }
    }

    // 所有敌人数据的缓存
    private Dictionary<int, EnemyData> enemyDataCache = new Dictionary<int, EnemyData>();

    /// <summary>
    /// 预加载所有敌人数据
    /// </summary>
    public void LoadAllEnemyData()
    {
        enemyDataCache.Clear();
        
        EnemyData[] allEnemyData = Resources.LoadAll<EnemyData>("Data_Enemy/Enemies");
        
        foreach (EnemyData data in allEnemyData)
        {
            if (data != null && !enemyDataCache.ContainsKey(data.id))
            {
                enemyDataCache[data.id] = data;
            }
        }
    }

    /// <summary>
    /// 根据ID获取敌人数据
    /// </summary>
    public EnemyData GetEnemyDataById(int id)
    {
        if (enemyDataCache.TryGetValue(id, out EnemyData data))
        {
            return data;
        }
        
        // 如果缓存中没有，尝试直接加载
        EnemyData directData = Resources.Load<EnemyData>($"Data_Enemy/Enemies/{id}");
        if (directData != null)
        {
            enemyDataCache[id] = directData;
            return directData;
        }
        
        Debug.LogWarning($"找不到敌人数据 ID: {id}");
        return null;
    }

    /// <summary>
    /// 根据ID获取敌人数据（字符串版本）
    /// </summary>
    public EnemyData GetEnemyDataById(string id)
    {
        if (int.TryParse(id, out int intId))
        {
            return GetEnemyDataById(intId);
        }
        Debug.LogWarning($"无效的敌人ID: {id}");
        return null;
    }
}
