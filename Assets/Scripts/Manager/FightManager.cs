using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//战斗枚举
public enum FightType
{
    None,
    Init,
    Player,//玩家回合
    Enemy,//敌人回合
    Win,
    Loss
}

//战斗管理器
public class FightManager : MonoBehaviour
{
    public static FightManager Instance;

    public FightUnit fightUnit;//战斗单元

    [HideInInspector] public int MaxHp;//最大血量
    [HideInInspector] public int CurHp;//当前血量
    [HideInInspector] public int MaxPowerCount;//最大能量（行动力）数量
    [HideInInspector] public int CurPowerCount;//当前能量
    [HideInInspector] public int DefenseCount;//防御值
    [HideInInspector] public int CoinAmount { get; private set; } // 当前金币

    /// <summary>当前持有的药水列表（最多3瓶）</summary>
    [HideInInspector] public List<PotionData> potionList = new List<PotionData>();

    /// <summary>当前持有的遗物列表（数据）</summary>
    [HideInInspector] public List<RelicData> relicList = new List<RelicData>();

    private int currentIslandIndex; // 当前岛屿索引
    private Vector2Int currentNodePoint; // 当前节点坐标
    private Map.NodeType currentNodeType; // 当前节点类型

    private static int savedMaxHp = 100; // 保存的最大血量（跨岛屿继承），首次使用 GameConfig 值覆盖
    private static int savedCurHp = 100; // 保存的当前血量（同一岛屿内保持）
    private static int savedIslandIndex = -1; // 上次保存血量时的岛屿索引

    //初始化
    public void Init()
    {
        var cfg = GameConfig.Instance;

        // 从 PlayerPrefs 恢复血量（脚本重编译后保持）
        savedCurHp = PlayerPrefs.GetInt("SavedCurHp", savedCurHp);

        if (savedIslandIndex != currentIslandIndex)
        {
            savedIslandIndex = currentIslandIndex;
            // 新 Act 开始，重置药水掉落计数器
            PotionDropManager.ResetCounter();
            // 优先使用 PlayerPrefs 中保存的永久最大血量（如鲜萃果液增加后）
            int savedMaxHpPref = PlayerPrefs.GetInt("SavedMaxHp", -1);
            savedMaxHp = savedMaxHpPref > 0 ? savedMaxHpPref : cfg.maxHp;
            savedCurHp = cfg.curHp;
        }

        MaxHp = savedMaxHp;
        CurHp = savedCurHp;

        // 其他战斗属性每次都重置
        MaxPowerCount = cfg.maxPowerCount;
        CurPowerCount = cfg.maxPowerCount;
        DefenseCount = 0; // 初始护盾为0
        CoinAmount = cfg.initialCoin;
    }

    /// <summary>
    /// 一次性初始化药水库存（仅在游戏启动时调用，不会在每场战斗重置）
    /// </summary>
    public void InitPotions()
    {
        potionList.Clear();
        var cfg = GameConfig.Instance;
        if (cfg.initialPotions != null)
        {
            foreach (var potion in cfg.initialPotions)
            {
                if (potion != null)
                    potionList.Add(potion);
            }
        }
    }

    /// <summary>
    /// 一次性初始化遗物（仅在游戏启动时调用）
    /// </summary>
    public void InitRelics()
    {
        relicList.Clear();
        RelicManager.Instance.Clear();
        var cfg = GameConfig.Instance;
        if (cfg.initialRelics != null)
        {
            foreach (var relic in cfg.initialRelics)
            {
                if (relic != null)
                    AddRelic(relic);
            }
        }
    }

    /// <summary>
    /// 添加遗物（数据 + 创建运行实例）
    /// </summary>
    public void AddRelic(RelicData relicData)
    {
        if (relicData == null) return;

        relicList.Add(relicData);

        // 通过反射创建遗物实例，注册到 RelicManager
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

        // 刷新遗物UI
        RelicsUI relicsUI = Object.FindObjectOfType<RelicsUI>();
        if (relicsUI != null) relicsUI.RefreshUI();
    }

    /// <summary>
    /// 移除遗物
    /// </summary>
    public void RemoveRelic(RelicData relicData)
    {
        if (relicData == null) return;
        relicList.Remove(relicData);

        RelicsUI relicsUI = Object.FindObjectOfType<RelicsUI>();
        if (relicsUI != null) relicsUI.RefreshUI();
    }

    private void Awake()
    {
        Instance = this;
    }

    //设置当前岛屿索引
    public void SetCurrentIslandIndex(int index)
    {
        currentIslandIndex = index;
    }

    //获取当前岛屿索引
    public int GetCurrentIslandIndex()
    {
        return currentIslandIndex;
    }

    //设置当前节点坐标
    public void SetCurrentNodePoint(Vector2Int point)
    {
        currentNodePoint = point;
    }

    //获取当前节点坐标
    public Vector2Int GetCurrentNodePoint()
    {
        return currentNodePoint;
    }

    //切换战斗状态
    public void ChangeType(FightType type)
    {
        switch (type)
        {
            case FightType.None:
                break;

            case FightType.Init:
                fightUnit = new FightInit();
                break;

            case FightType.Player:
                fightUnit = new Fight_PlayerTurn();
                break;

            case FightType.Enemy:
                fightUnit = new Fight_EnemyTurn();
                break;

            case FightType.Win:
                fightUnit = new Fight_Win();
                break;

            case FightType.Loss:
                fightUnit = new Fight_Loss();
                break;
        }
        fightUnit.Init();//初始化
    }

    //玩家受伤逻辑
    public void GetPlayerHit(int hit, Enemy attacker = null)
    {
        // 幸运格挡
        if (BuffManager.Instance.HasStatus(StatusType.LuckyBlock))
        {
            BuffManager.Instance.RemoveStatus(StatusType.LuckyBlock, 1);
            UIManager.Instance.ShowTip("幸运格挡!", Color.cyan);
            return;
        }

        // 荆棘反伤
        int thorns = BuffManager.Instance.GetStack(StatusType.Thorns);
        if (thorns > 0 && attacker != null && attacker.CurHp > 0)
        {
            attacker.Hit(thorns);
            UIManager.Instance.ShowTip($"荆棘反伤 -{thorns}", Color.green);
        }

        // 应用易伤Buff修改
        int finalHit = BuffManager.Instance.ModifyTakenDamage(hit);

        //扣护盾
        if (DefenseCount >= finalHit)
        {
            DefenseCount -= finalHit;
        }
        else
        {
            finalHit = finalHit - DefenseCount;
            DefenseCount = 0;
            CurHp -= finalHit;
            // 显示受伤特效
            UIManager.Instance.ShowDamageEffect();

            if (CurHp < 0)
            {
                // 检查是否有瓶中精灵：阻止死亡，回复至最大生命值30%
                if (TryUseBottledSprite())
                {
                    // 瓶中精灵触发后，直接设置血量为30%，跳过Loss
                    CurHp = Mathf.CeilToInt(MaxHp * 0.3f);
                    // 继续执行保存和UI更新
                }
                else
                {
                    CurHp = 0;
                    //切换到游戏失败
                    ChangeType(FightType.Loss);
                    // 保存并更新UI后直接return，避免后续重复更新
                    savedCurHp = CurHp;
                    savedMaxHp = MaxHp;
                    PlayerPrefs.SetInt("SavedCurHp", savedCurHp);
                    PlayerPrefs.Save();

                    FightUI fightUI2 = UIManager.Instance.GetUI<FightUI>("FightUI");
                    if (fightUI2 != null)
                    {
                        fightUI2.UpdateHp();
                        fightUI2.UpdateDefense();
                    }
                    return;
                }
            }
        }

        // 保存当前血量（同一岛屿内保持 + 脚本重编译后恢复）
        savedCurHp = CurHp;
        savedMaxHp = MaxHp;
        PlayerPrefs.SetInt("SavedCurHp", savedCurHp);
        PlayerPrefs.Save();

        // 更新界面（安全调用）
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.UpdateHp();
            fightUI.UpdateDefense();
        }
    }

    /// <summary>
    /// 检查并触发瓶中精灵（死亡时自动使用）
    /// </summary>
    private bool TryUseBottledSprite()
    {
        for (int i = potionList.Count - 1; i >= 0; i--)
        {
            if (potionList[i] != null && potionList[i].scriptName == "BottledSpritePotion")
            {
                PotionData spriteData = potionList[i];
                potionList.RemoveAt(i);

                // 触发药水的音效和特效（不回血，血量由外层设置）
                BottledSpritePotion sprite = new BottledSpritePotion();
                sprite.Init(spriteData);
                sprite.UseBaseEffects();

                // 刷新药水UI图标
                PotionPanelController ppc = Object.FindObjectOfType<PotionPanelController>();
                if (ppc != null) ppc.RefreshPotionButtons();

                UIManager.Instance.ShowTip("瓶中精灵触发！", Color.green);
                return true;
            }
        }
        return false;
    }

    // 增加血量上限和当前血量（永久，跨岛屿保持）
    public void AddMaxHp(int amount)
    {
        MaxHp += amount;
        CurHp += amount;
        savedMaxHp = MaxHp;
        savedCurHp = CurHp;
        PlayerPrefs.SetInt("SavedMaxHp", savedMaxHp);
        PlayerPrefs.SetInt("SavedCurHp", savedCurHp);
        PlayerPrefs.Save();

        // 更新界面
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        fightUI?.UpdateHp();
    }

    // 重置血量（重新开始游戏时调用）
    public static void ResetHp()
    {
        var cfg = GameConfig.Instance;
        savedMaxHp = cfg.maxHp;
        savedCurHp = cfg.curHp;
        savedIslandIndex = -1;
        PlayerPrefs.DeleteKey("SavedCurHp");
        PlayerPrefs.DeleteKey("SavedMaxHp");
        PlayerPrefs.Save();
    }

    // 治疗玩家（RestSite 等使用，不依赖 Instance）
    public static void HealPlayer(int amount)
    {
        savedCurHp = Mathf.Min(savedCurHp + amount, savedMaxHp);
        PlayerPrefs.SetInt("SavedCurHp", savedCurHp);
        PlayerPrefs.Save();
        if (Instance != null)
        {
            Instance.CurHp = savedCurHp;
            Instance.MaxHp = savedMaxHp;
        }
    }

    //增加金币
    public void AddCoin(int amount)
    {
        CoinAmount += amount;
        UIManager.Instance.GetUI<FightUI>("FightUI")?.UpdateCoinDisplay(CoinAmount);
    }

    public Map.NodeType GetCurrentNodeType() => currentNodeType;

    public void SetCurrentNodeType(Map.NodeType type)
    {
        currentNodeType = type;
    }

    private void Update()
    {
        if (fightUnit != null)
        {
            fightUnit.OnUpdate();
        }
    }
}