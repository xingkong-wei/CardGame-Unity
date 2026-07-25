using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 卡牌配置条目：指定一张卡牌及其数量（从 StartingCard.cs 移入，统一管理）
/// </summary>
[System.Serializable]
public class CardEntry
{
    [Tooltip("卡牌数据")]
    public CardData cardData;
    [Tooltip("该卡牌的数量")]
    public int count = 1;
}

/// <summary>
/// 游戏初始配置 - 集中管理所有硬编码的初始数值
/// 在 Unity 中通过 Create Asset Menu 创建，放入 Resources/Config 目录
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header(" 战斗基础数值 ")]
    [Tooltip("初始最大血量")]
    public int maxHp = 100;
    [Tooltip("初始当前血量")]
    public int curHp = 100;
    [Tooltip("每回合回复能量")]
    public int maxPowerCount = 3;
    [Tooltip("初始金币")]
    public int initialCoin = 99;
    [Tooltip("击杀敌人获得金币")]
    public int killCoinReward = 25;

    [Header(" 抽牌 ")]
    [Tooltip("每回合基础抽牌数")]
    public int drawCardsPerTurn = 4;
    [Tooltip("手牌上限")]
    public int maxHandSize = 10;

    [Header(" 初始卡牌 ")]
    [Tooltip("初始拥有的卡牌列表（卡牌 + 数量）")]
    public List<CardEntry> initialCards = new List<CardEntry>();

    [Header(" 初始药水 ")]
    [Tooltip("游戏开始时持有的药水列表")]
    public List<PotionData> initialPotions;

    [Header(" 初始遗物 ")]
    [Tooltip("游戏开始时持有的遗物列表")]
    public List<RelicData> initialRelics;

    [Header(" 宝箱配置 ")]
    public TreasureConfigData smallChest;
    public TreasureConfigData mediumChest;
    public TreasureConfigData largeChest;

    /// <summary>
    /// 随机一个宝箱配置（50%/33%/17%）
    /// </summary>
    public TreasureConfigData RollRandomChest()
    {
        int roll = Random.Range(0, 100);
        if (roll < 50) return smallChest;
        if (roll < 83) return mediumChest;
        return largeChest;
    }

    /// <summary>
    /// 根据配置生成初始卡牌列表（每张独立DeckCard实例）
    /// </summary>
    public List<DeckCard> GenerateCardList()
    {
        List<DeckCard> cardList = new List<DeckCard>();
        foreach (var entry in initialCards)
        {
            if (entry.cardData == null) continue;
            for (int i = 0; i < entry.count; i++)
                cardList.Add(new DeckCard(entry.cardData));
        }
        return cardList;
    }

    // ===== 单例访问 =====

    private static GameConfig _instance;
    public static GameConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GameConfig>("GameConfig");
                if (_instance == null)
                    Debug.LogError("[GameConfig] 未找到 Resources/GameConfig.asset，请创建！");
            }
            return _instance;
        }
    }

    /// <summary>
    /// 清除缓存（场景切换时调用，不叫 Reset 避免与 ScriptableObject.Reset() 冲突）
    /// </summary>
    public static void ClearCache()
    {
        _instance = null;
    }
}
