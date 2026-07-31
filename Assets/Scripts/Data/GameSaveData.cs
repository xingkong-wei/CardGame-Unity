using System;
using System.Collections.Generic;

/// <summary>
/// 游戏存档数据（节点入口快照，用于 SL 读档重来）
/// </summary>
/// <summary>
/// 存档所在游戏阶段
/// </summary>
public enum SavePhase
{
    Fight,      // 战斗中（节点入口快照）
    Reward,     // 奖励界面
    Shop,       // 商店
    Treasure,   // 宝箱
    RestSite,   // 休息站
}

[Serializable]
public class GameSaveData
{
    // 战斗状态（进入节点时）
    public int curHp;
    public int maxHp;
    public int coinAmount;
    public int currentIslandIndex;
    public int currentNodeX;
    public int currentNodeY;
    public string currentNodeTypeStr;

    // 药水
    public List<string> potionIds = new List<string>();

    // 遗物
    public List<string> relicIds = new List<string>();

    // 卡牌牌组
    public List<CardSaveEntry> deckCards = new List<CardSaveEntry>();

    // 地图
    public string mapJson;

    // 敌人关卡（确保 SL 后同一个节点遇到同一组敌人，类似杀戮尖塔）
    public int levelId = -1;

    // ===== 存档阶段（决定读档后回到哪个界面） =====
    public SavePhase savePhase = SavePhase.Fight;

    // ===== 奖励界面数据 =====
    public string rewardPotionId;       // 掉落的药水 scriptName（空串=无）
    public string rewardRelicId;        // 掉落的遗物 scriptName（空串=无）
    public bool rewardCardDone;
    public bool rewardPotionDone;
    public bool rewardRelicDone;

    // ===== 商店数据 =====
    public List<string> shopCardIds = new List<string>();    // 商店卡牌 scriptName
    public List<int> shopCardPrices = new List<int>();
    public List<string> shopRelicIds = new List<string>();
    public List<int> shopRelicPrices = new List<int>();
    public List<string> shopPotionIds = new List<string>();
    public List<int> shopPotionPrices = new List<int>();
    public bool shopRemoveUsed;

    // ===== 元数据 =====
    public long saveTimeTicks;
}

[Serializable]
public class CardSaveEntry
{
    public int cardDataId;
    public int instanceId;
    public bool upgraded;
}
