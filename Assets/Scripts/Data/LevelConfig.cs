using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单个敌人配置
/// </summary>
[System.Serializable]
public class EnemySpawnEntry
{
    [Tooltip("敌人数据 ID")]
    public string enemyId;

    [Tooltip("生成位置")]
    public Vector3 position;
}

/// <summary>
/// 关卡配置 ScriptableObject（替代 level.txt）
/// 在 Unity 中右键 → Create → Level/LevelConfig 创建
/// </summary>
[CreateAssetMenu(fileName = "LevelConfig", menuName = "Level/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    [Header("基础信息")]
    [Tooltip("关卡唯一 ID（如 10001）")]
    public int levelId;

    [Tooltip("关卡名称")]
    public string levelName;

    [Tooltip("所属岛屿索引")]
    public int islandIndex;

    [Header("敌人配置")]
    [Tooltip("本关卡中的所有敌人")]
    public List<EnemySpawnEntry> enemies = new List<EnemySpawnEntry>();
}
