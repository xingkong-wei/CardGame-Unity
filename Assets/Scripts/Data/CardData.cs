using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 攻击伤害来源类型
/// </summary>
public enum DamageSourceType
{
    /// <summary>使用 arg0 作为固定伤害值（默认）</summary>
    Fixed = 0,
    /// <summary>当前格挡值</summary>
    Defense = 1,
    /// <summary>当前生命值</summary>
    CurrentHp = 2,
    /// <summary>当前金币数量</summary>
    Coin = 3,
}

/// <summary>
/// 卡牌稀有度
/// </summary>
public enum CardRarity
{
    /// <summary>初始</summary>
    Basic,
    /// <summary>普通</summary>
    Common,
    /// <summary>罕见</summary>
    Uncommon,
    /// <summary>稀有</summary>
    Rare,
    /// <summary>事件</summary>
    Event,
    /// <summary>先古之民</summary>
    Ancient,
    /// <summary>状态</summary>
    Status,
    /// <summary>诅咒</summary>
    Curse,
    /// <summary>任务</summary>
    Quest,
    /// <summary>衍生</summary>
    Generated,
}

[CreateAssetMenu(fileName = "新卡牌", menuName = "Card/CardData")]
public class CardData : ScriptableObject
{
    [Header(" 基础信息 ")]
    public int id;
    public string cardName;
    public CardRarity rarity;

    [Header(" 经济数值 ")]
    [Tooltip("商店购买价格")]
    public int price;

    [Header(" 类型与脚本 ")]
    public List<CardTypeData> cardTypes;  // 支持多类型
    public string scriptName;

    [Header(" 资源路径 ")]
    public string bgIcon;
    public string icon;

    [Header(" 消耗与效果 ")]
    public int expend;
    public int arg0;
    public string effects;

    [Header(" 动态伤害配置 ")]
    [Tooltip("伤害来源类型，默认为 Fixed（使用 arg0）")]
    public DamageSourceType damageSource = DamageSourceType.Fixed;

    [Tooltip("伤害来源的百分比（1 = 100%），例如 0.5 = 50%")]
    public float damagePercent = 1f;

    [Header(" 描述 ")]
    [TextArea]
    public string description;

    [Header(" 升级属性 ")]
    [Tooltip("是否可升级")]
    public bool upgradable;

    [Tooltip("升级后费用")]
    public int upgradedExpend;

    [Tooltip("升级后效果参数")]
    public int upgradedArg0;

    [TextArea]
    [Tooltip("升级后描述")]
    public string upgradedDescription;

    [Header(" 卡牌词条 ")]
    [Tooltip("升级后移除消耗类型")]
    public bool removeConsumeOnUpgrade;

    [Tooltip("固有 - 战斗开始时必定在手牌")]
    public bool isInnate;
    [Tooltip("升级后固有")]
    public bool upgradedIsInnate;

    [Tooltip("保留 - 回合结束不丢弃")]
    public bool isRetain;
    [Tooltip("升级后保留")]
    public bool upgradedIsRetain;

    [Tooltip("奇巧 - 被丢弃时自动使用")]
    public bool autoPlayOnDiscard;
    [Tooltip("升级后奇巧")]
    public bool upgradedAutoPlayOnDiscard;

    public string GetFormattedDescription()
    {
        return string.Format(description, arg0);
    }

    // 检查是否包含指定类型
    public bool HasCardType(string typeName)
    {
        if (cardTypes == null) return false;
        foreach (var cardType in cardTypes)
        {
            if (cardType != null && cardType.typeName == typeName)
                return true;
        }
        return false;
    }

    // 获取类型名称字符串（用于显示，多类型用"/"隔开）
    public string GetTypeNames()
    {
        if (cardTypes == null || cardTypes.Count == 0)
            return "";
        List<string> names = new List<string>();
        foreach (var cardType in cardTypes)
        {
            if (cardType != null && !string.IsNullOrEmpty(cardType.typeName))
                names.Add(cardType.typeName);
        }
        return string.Join("/", names);
    }

    // 是否是消耗类型卡牌
    public bool IsConsumeCard()
    {
        return HasCardType("消耗");
    }

    /// <summary>是否是虚无卡牌（回合结束时还在手上则消耗）</summary>
    public bool IsEtherealCard()
    {
        return HasCardType("虚无");
    }
}
