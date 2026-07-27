using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//敌人管理器
public class EnemyManager
{
    private static EnemyManager instance;
    public static EnemyManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new EnemyManager();
            }
            return instance;
        }
    }

    private List<Enemy> enemyList;//存储战斗中的敌人

    // 存储每个关卡的权重（通过的关卡权重降低）
    private Dictionary<int, int> levelWeights = new Dictionary<int, int>();

    // 存储所有关卡及其所属的岛屿
    private Dictionary<int, int> levelToIslandMap = new Dictionary<int, int>();

    // 存储每个岛屿的可用关卡列表
    private Dictionary<int, List<int>> islandLevelsMap = new Dictionary<int, List<int>>();

    // 加载已通过的关卡列表
    private void LoadCompletedLevels()
    {
        // 从PlayerPrefs加载已通过的关卡
        string completedStr = PlayerPrefs.GetString("CompletedLevels", "");
        if (string.IsNullOrEmpty(completedStr))
        {
            return;
        }

        string[] completedIds = completedStr.Split(',');
        foreach (string id in completedIds)
        {
            if (int.TryParse(id, out int levelId))
            {
                // 已通过的关卡，权重设置为10（未被通过的权重默认为100）
                if (!levelWeights.ContainsKey(levelId))
                {
                    levelWeights[levelId] = 10;
                }
            }
        }
    }

    // 初始化关卡到岛屿的映射
    private void InitializeLevelToIslandMap()
    {
        levelToIslandMap.Clear();
        islandLevelsMap.Clear();

        // 优先从 LevelConfigManager（ScriptableObject）读取
        foreach (var cfg in LevelConfigManager.Instance.GetAllLevels())
        {
            if (cfg == null) continue;

            levelToIslandMap[cfg.levelId] = cfg.islandIndex;

            if (!islandLevelsMap.ContainsKey(cfg.islandIndex))
                islandLevelsMap[cfg.islandIndex] = new List<int>();
            islandLevelsMap[cfg.islandIndex].Add(cfg.levelId);
        }

        // 兼容旧 txt 格式
        if (islandLevelsMap.Count == 0)
        {
            int levelsPerIsland = 3;
            for (int levelId = 10001; levelId < 10001 + 100; levelId++)
            {
                Dictionary<string, string> levelData = GameConfigManager.Instance.GetLevelById(levelId.ToString());
                if (levelData != null)
                {
                    int islandIndex = -1;
                    if (levelData.ContainsKey("Island"))
                        int.TryParse(levelData["Island"], out islandIndex);
                    else
                        islandIndex = (levelId - 10001) / levelsPerIsland;

                    levelToIslandMap[levelId] = islandIndex;

                    if (!islandLevelsMap.ContainsKey(islandIndex))
                        islandLevelsMap[islandIndex] = new List<int>();
                    islandLevelsMap[islandIndex].Add(levelId);
                }
            }
        }
    }

    // 保存通过的关卡
    private void SaveCompletedLevel(int levelId)
    {
        string completedStr = PlayerPrefs.GetString("CompletedLevels", "");
        List<int> completedList = new List<int>();

        if (!string.IsNullOrEmpty(completedStr))
        {
            string[] completedIds = completedStr.Split(',');
            foreach (string id in completedIds)
            {
                if (int.TryParse(id, out int idInt))
                {
                    completedList.Add(idInt);
                }
            }
        }

        if (!completedList.Contains(levelId))
        {
            completedList.Add(levelId);
            PlayerPrefs.SetString("CompletedLevels", string.Join(",", completedList));
            PlayerPrefs.Save();

            // 降低该关卡的权重
            levelWeights[levelId] = 10;
        }
    }

    //加载敌人资源
    public void LoadRes(int islandIndex, Map.NodeType nodeType = Map.NodeType.MinorEnemy)
    {
        enemyList = new List<Enemy>();

        // 根据节点类型确定需要加载的敌人等级
        EnemyTier targetTier = NodeTypeToEnemyTier(nodeType);

        if (levelToIslandMap.Count == 0)
            InitializeLevelToIslandMap();

        LoadCompletedLevels();

        List<int> availableLevels = new List<int>();
        List<int> weights = new List<int>();

        if (islandLevelsMap.ContainsKey(islandIndex))
        {
            availableLevels = islandLevelsMap[islandIndex];
            foreach (int levelId in availableLevels)
            {
                int weight = levelWeights.ContainsKey(levelId) ? levelWeights[levelId] : 100;
                weights.Add(weight);
            }
        }

        if (availableLevels.Count == 0)
        {
            Debug.LogError($"岛屿 {islandIndex} 没有可用的关卡");
            return;
        }

        int randomLevelId = GetWeightedRandom(availableLevels, weights);
        SaveCompletedLevel(randomLevelId);

        // 优先使用 ScriptableObject 配置
        LevelConfig levelCfg = LevelConfigManager.Instance.GetLevelById(randomLevelId);
        if (levelCfg != null)
        {
            SpawnEnemiesFromConfig(levelCfg, targetTier);
        }
        else
        {
            SpawnEnemiesFromTxt(randomLevelId.ToString(), targetTier);
        }
    }

    /// <summary>
    /// 节点类型 → 敌人等级映射
    /// </summary>
    private EnemyTier NodeTypeToEnemyTier(Map.NodeType nodeType)
    {
        switch (nodeType)
        {
            case Map.NodeType.EliteEnemy:
                return EnemyTier.Elite;
            case Map.NodeType.Boss:
                return EnemyTier.Boss;
            case Map.NodeType.MinorEnemy:
            case Map.NodeType.Mystery:
                return EnemyTier.Normal;
            default:
                return EnemyTier.Normal;
        }
    }

    /// <summary>
    /// 从 ScriptableObject 关卡配置生成敌人（按等级筛选）
    /// </summary>
    private void SpawnEnemiesFromConfig(LevelConfig cfg, EnemyTier targetTier)
    {
        foreach (var entry in cfg.enemies)
        {
            EnemyData enemyDataSO = EnemyDataManager.Instance.GetEnemyDataById(entry.enemyId);
            if (enemyDataSO == null)
            {
                Debug.LogError($"找不到敌人数据 ID: {entry.enemyId}");
                continue;
            }

            // 按敌人等级筛选：只加载匹配等级的敌人
            if (enemyDataSO.tier != targetTier)
                continue;

            SpawnEnemy(enemyDataSO, entry.position);
        }
    }

    /// <summary>
    /// 从旧 txt 格式生成敌人（兼容，按等级筛选）
    /// </summary>
    private void SpawnEnemiesFromTxt(string levelId, EnemyTier targetTier)
    {
        Dictionary<string, string> data = GameConfigManager.Instance.GetLevelById(levelId);
        if (data == null)
        {
            Debug.LogError($"关卡配置未找到：{levelId}");
            return;
        }

        string[] enemyIds = data["EnemyIds"].Split('=');
        string[] enemyPos = data["Pos"].Split('=');

        for (int i = 0; i < enemyIds.Length; i++)
        {
            string enemyId = enemyIds[i];
            string[] posArr = enemyPos[i].Split(',');

            EnemyData enemyDataSO = EnemyDataManager.Instance.GetEnemyDataById(enemyId);
            if (enemyDataSO == null)
            {
                Debug.LogError($"找不到敌人数据 ID: {enemyId}");
                continue;
            }

            // 按敌人等级筛选
            if (enemyDataSO.tier != targetTier)
                continue;

            float x = float.Parse(posArr[0]);
            float y = float.Parse(posArr[1]);
            float z = float.Parse(posArr[2]);

            SpawnEnemy(enemyDataSO, new Vector3(x, y, z));
        }
    }

    /// <summary>
    /// 生成单个敌人
    /// </summary>
    private void SpawnEnemy(EnemyData data, Vector3 position)
    {
        GameObject enemyPrefab = Resources.Load<GameObject>(data.modelPath);
        if (enemyPrefab == null)
        {
            Debug.LogError($"敌人模型加载失败: {data.modelPath}");
            return;
        }

        GameObject obj = Object.Instantiate(enemyPrefab);
        Enemy enemy = obj.AddComponent<Enemy>();
        enemy.Init(data);
        enemyList.Add(enemy);
        obj.transform.position = position;
    }


    //移除敌人,判断胜利
    public void DeleteEnemy(Enemy enemy)
    {
        enemyList.Remove(enemy);

        // 添加金币
        FightManager.Instance.AddCoin(GameConfig.Instance.killCoinReward);

        //是否全部死亡进行判断
        if (enemyList.Count == 0)
        {
            FightManager.Instance.ChangeType(FightType.Win);
        }
    }

    //清空所有敌人
    public void ClearAllEnemies()
    {
        foreach (var enemy in enemyList)
        {
            if (enemy != null && enemy.gameObject != null)
                Object.Destroy(enemy.gameObject);
        }
        enemyList.Clear();
    }

    //执行所有敌人的行为
    public IEnumerator DoAllEnemyAction()
    {
        for (int i = 0; i < enemyList.Count; i++)
        {
            yield return FightManager.Instance.StartCoroutine(enemyList[i].DoAction());
        }

        //判断执行完所有敌人的行为
        for (int i = 0; i < enemyList.Count; i++)
        {
            enemyList[i].SetRandomAction();
        }

        // 敌人回合结束：处理敌人状态效果
        for (int i = 0; i < enemyList.Count; i++)
        {
            // 易伤、虚弱、缩小递减，枷锁本回合结束后全部移除
            if (enemyList[i].GetStatusStack(StatusType.Vulnerable) > 0)
                enemyList[i].RemoveStatus(StatusType.Vulnerable, 1);
            if (enemyList[i].GetStatusStack(StatusType.Weak) > 0)
                enemyList[i].RemoveStatus(StatusType.Weak, 1);
            if (enemyList[i].GetStatusStack(StatusType.Shrink) > 0)
                enemyList[i].RemoveStatus(StatusType.Shrink, 1);
            if (enemyList[i].GetStatusStack(StatusType.Fetter) > 0)
                enemyList[i].RemoveStatus(StatusType.Fetter, 99);

            // 寂灭：每层失去9点生命
            int voidDust = enemyList[i].GetStatusStack(StatusType.VoidDust);
            if (voidDust > 0 && enemyList[i].CurHp > 0)
            {
                enemyList[i].Hit(9 * voidDust);
            }
        }

        // 回合结束触发Buff效果（金属化、流血、中毒等）- 冰亲和度已在玩家回合结束时处理
        BuffManager.Instance.OnTurnEnd();

        // 每个敌人回合结束时清空护盾（杀戮尖塔机制）
        FightManager.Instance.DefenseCount = 0;

        // 更新界面护盾显示
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.UpdateDefense();
        }

        //切换到下一回合
        FightManager.Instance.ChangeType(FightType.Player);
    }

    // 基于权重的随机选择
    private int GetWeightedRandom(List<int> items, List<int> weights)
    {
        if (items.Count == 0 || weights.Count == 0 || items.Count != weights.Count)
        {
            Debug.LogError("GetWeightedRandom: 参数错误");
            return items.Count > 0 ? items[0] : 10001;
        }

        // 计算总权重
        int totalWeight = 0;
        foreach (int weight in weights)
        {
            totalWeight += weight;
        }

        // 随机选择一个权重值
        int randomWeight = Random.Range(0, totalWeight);

        // 找到对应的关卡
        int currentWeight = 0;
        for (int i = 0; i < items.Count; i++)
        {
            currentWeight += weights[i];
            if (randomWeight < currentWeight)
            {
                return items[i];
            }
        }

        // 如果没有找到（理论上不应该发生），返回第一个
        return items[0];
    }

    /// <summary>
    /// 对随机敌人造成伤害（用于荆棘效果等）
    /// </summary>
    public void DealDamageToRandomEnemy(int damage)
    {
        if (enemyList == null || enemyList.Count == 0) return;
        
        // 找到存活的敌人
        List<Enemy> aliveEnemies = enemyList.FindAll(e => e != null && e.gameObject != null);
        if (aliveEnemies.Count == 0) return;
        
        // 随机选择一个敌人
        Enemy target = aliveEnemies[Random.Range(0, aliveEnemies.Count)];
        target.Hit(damage);
    }
}