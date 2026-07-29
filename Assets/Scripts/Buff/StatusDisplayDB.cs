using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 状态显示数据
/// </summary>
public struct StatusDisplayData
{
    /// <summary>显示名称</summary>
    public string name;

    /// <summary>描述模板（{0} 会被替换为 stack）</summary>
    public string descriptionTemplate;

    /// <summary>图标资源路径</summary>
    public string iconPath;

    /// <summary>显示类型</summary>
    public StatusDisplayType displayType;

    /// <summary>自定义描述格式化（优先级高于 descriptionTemplate）</summary>
    public System.Func<int, string> descriptionFunc;

    /// <summary>根据层数生成描述文本</summary>
    public string GetDescription(int stack)
    {
        if (descriptionFunc != null)
            return descriptionFunc(stack);

        if (string.IsNullOrEmpty(descriptionTemplate))
            return $"层数: {stack}";

        return descriptionTemplate.Replace("{0}", stack.ToString());
    }
}

/// <summary>
/// 状态显示数据配置表
/// 替代 StatusEffect.InitFromType() 中的巨型 switch
/// 新增状态只需在此添加一行，无需修改 StatusEffect.cs
/// </summary>
public static class StatusDisplayDB
{
    private static readonly Dictionary<StatusType, StatusDisplayData> _data = new Dictionary<StatusType, StatusDisplayData>
    {
        // ===== 属性Buff =====
        { StatusType.Strength,       new StatusDisplayData { name = "力量",     descriptionTemplate = "攻击伤害 +{0}",                       iconPath = "Icon/Buff/Strength"       , displayType = StatusDisplayType.Buff   } },
        { StatusType.Weak,           new StatusDisplayData { name = "虚弱",     descriptionFunc = s => $"攻击伤害 -{s * 25}%",                iconPath = "Icon/Buff/Weak"           , displayType = StatusDisplayType.Debuff } },
        { StatusType.Vulnerable,     new StatusDisplayData { name = "易伤",     descriptionTemplate = "受到伤害 +25%（每回合减少1层）",         iconPath = "Icon/Buff/Vulnerable"     , displayType = StatusDisplayType.Debuff } },
        { StatusType.Frail,          new StatusDisplayData { name = "脆弱",     descriptionFunc = s => $"护甲获得 -{s * 25}%",                iconPath = "Icon/Buff/Frail"          , displayType = StatusDisplayType.Debuff } },
        { StatusType.Agility,        new StatusDisplayData { name = "敏捷",     descriptionTemplate = "每层获得格挡 +{0}",                     iconPath = "Icon/Buff/Agility"        , displayType = StatusDisplayType.Buff   } },

        // ===== 特殊能力 =====
        { StatusType.Thorns,         new StatusDisplayData { name = "荆棘",     descriptionTemplate = "受到攻击时反弹 {0} 伤害",               iconPath = "Icon/Buff/Thorns"         , displayType = StatusDisplayType.Buff   } },
        { StatusType.Lifesteal,      new StatusDisplayData { name = "吸血",     descriptionTemplate = "攻击时偷取 {0} 生命",                   iconPath = "Icon/Buff/Lifesteal"      , displayType = StatusDisplayType.Buff   } },
        { StatusType.PlatedArmor,    new StatusDisplayData { name = "覆甲",     descriptionTemplate = "回合结束时获得 {0} 格挡",                iconPath = "Icon/Buff/PlatedArmor"    , displayType = StatusDisplayType.Buff   } },
        { StatusType.Metallicize,    new StatusDisplayData { name = "金属化",   descriptionTemplate = "回合结束获得 {0} 护甲",                  iconPath = "Icon/Buff/Metallicize"    , displayType = StatusDisplayType.Buff   } },
        { StatusType.Power,          new StatusDisplayData { name = "魔能",     descriptionTemplate = "每层 +{0} 伤害",                       iconPath = "Icon/Ability/Power"       , displayType = StatusDisplayType.Buff   } },

        // ===== 持续伤害 =====
        { StatusType.Poison,         new StatusDisplayData { name = "中毒",     descriptionTemplate = "每回合失去 {0} 生命",                   iconPath = "Icon/Buff/Poison"         , displayType = StatusDisplayType.Debuff } },
        { StatusType.Burning,        new StatusDisplayData { name = "燃烧",     descriptionTemplate = "每回合失去 {0} 生命",                   iconPath = "Icon/Buff/Burning"        , displayType = StatusDisplayType.Debuff } },
        { StatusType.Bleeding,       new StatusDisplayData { name = "流血",     descriptionTemplate = "回合结束失去 {0} 生命",                  iconPath = "Icon/Buff/Bleeding"       , displayType = StatusDisplayType.Debuff } },

        // ===== 增益效果 =====
        { StatusType.ArmorUp,        new StatusDisplayData { name = "护甲增强", descriptionTemplate = "回合开始获得 {0} 护甲",                 iconPath = "Icon/Buff/ArmorUp"        , displayType = StatusDisplayType.Buff   } },
        { StatusType.Rage,           new StatusDisplayData { name = "怒火",     descriptionTemplate = "每回合开始获得 {0} 力量",               iconPath = "Icon/Buff/Rage"           , displayType = StatusDisplayType.Buff   } },
        { StatusType.Focus,          new StatusDisplayData { name = "集中",     descriptionTemplate = "每回合额外抽 {0} 张牌",                 iconPath = "Icon/Buff/Focus"          , displayType = StatusDisplayType.Buff   } },
        { StatusType.Regeneration,   new StatusDisplayData { name = "再生",     descriptionTemplate = "每回合恢复 {0} 生命",                   iconPath = "Icon/Buff/Regeneration"   , displayType = StatusDisplayType.Buff   } },

        // ===== 负面效果 =====
        { StatusType.Wounded,        new StatusDisplayData { name = "受伤",     descriptionFunc = s => $"受到伤害 +{s * 25}%",                iconPath = "Icon/Buff/Wounded"        , displayType = StatusDisplayType.Debuff } },
        { StatusType.LockedArmor,    new StatusDisplayData { name = "锁甲",     descriptionTemplate = "无法获得护甲",                          iconPath = "Icon/Buff/LockedArmor"    , displayType = StatusDisplayType.Debuff } },
        { StatusType.Curse,          new StatusDisplayData { name = "诅咒",     descriptionTemplate = "每回合失去最大生命的 {0}%",             iconPath = "Icon/Buff/Curse"          , displayType = StatusDisplayType.Debuff } },

        // ===== 元素亲和度 =====
        { StatusType.FireAffinity,      new StatusDisplayData { name = "火亲和度", descriptionTemplate = "每层法术攻击伤害 +{0}（上限10层）",    iconPath = "Icon/Ability/FireAffinity"     , displayType = StatusDisplayType.Buff   } },
        { StatusType.IceAffinity,       new StatusDisplayData { name = "冰亲和度", descriptionTemplate = "每层回合结束 +{0} 格挡（上限10层）",    iconPath = "Icon/Ability/IceAffinity"      , displayType = StatusDisplayType.Buff   } },
        { StatusType.LightningAffinity, new StatusDisplayData { name = "电亲和度", descriptionFunc = s => $"每层抽牌数 +{Mathf.CeilToInt(s * 0.5f)}（上限10层）", iconPath = "Icon/Ability/LightningAffinity", displayType = StatusDisplayType.Buff   } },

        // ===== 特殊卡牌效果 =====
        { StatusType.Meditation,         new StatusDisplayData { name = "冥想",       descriptionTemplate = "下一张法术牌费用 -1",                    iconPath = "Icon/Ability/Meditation"        , displayType = StatusDisplayType.Buff    } },
        { StatusType.CardCostMinus1,     new StatusDisplayData { name = "卡牌费用减1", descriptionTemplate = "下 {0} 张法术牌费用 -1",                 iconPath = "Icon/Ability/Fee-1"             , displayType = StatusDisplayType.Buff    } },
        { StatusType.SpellResonance,     new StatusDisplayData { name = "法术共鸣",   descriptionTemplate = "每回合开始获得1层亲和度最高的元素",       iconPath = "Icon/Ability/SpellResonance"    , displayType = StatusDisplayType.Buff    } },
        { StatusType.AffinityDouble,     new StatusDisplayData { name = "奥术智慧",   descriptionFunc = s => $"本回合亲和度叠加×{Mathf.RoundToInt(Mathf.Pow(2, s))}", iconPath = "Icon/Ability/ArcaneWisdom"      , displayType = StatusDisplayType.Special } },
        { StatusType.ElementalDominance, new StatusDisplayData { name = "元素主宰",   descriptionTemplate = "每回合开始获得火、冰、电亲和度各 +1",    iconPath = "Icon/Ability/ElementalDominance", displayType = StatusDisplayType.Buff    } },
        { StatusType.SpiritFocus,        new StatusDisplayData { name = "聚灵",       descriptionTemplate = "亲和度上限提升至15层",                   iconPath = "Icon/Ability/SpiritFocus"       , displayType = StatusDisplayType.Buff    } },
        { StatusType.GrandMageRobe,      new StatusDisplayData { name = "大法师袍",   descriptionTemplate = "每打出一张法术牌，获得2点格挡",          iconPath = "Icon/Ability/GrandMageRobe"     , displayType = StatusDisplayType.Buff    } },
        { StatusType.PolarStorm,         new StatusDisplayData { name = "极地风暴",   descriptionTemplate = "每次获得或消耗冰亲和度，对所有敌人造成4点伤害", iconPath = "Icon/Ability/PolarStorm"   , displayType = StatusDisplayType.Buff    } },
        { StatusType.WandCharging,       new StatusDisplayData { name = "魔杖充能",   descriptionTemplate = "本回合下次攻击双倍伤害",                 iconPath = "Icon/Ability/WandCharging"      , displayType = StatusDisplayType.Special } },
        { StatusType.GiantGrowth,        new StatusDisplayData { name = "超巨化",     descriptionTemplate = "下1张攻击牌造成3倍伤害",                 iconPath = "Icon/Ability/GiantGrowth"       , displayType = StatusDisplayType.Special } },

        // ===== 临时效果 =====
        { StatusType.DawnCondensedDew, new StatusDisplayData { name = "启明凝露", descriptionTemplate = "每回合开始额外抽 {0} 张牌",              iconPath = "Icon/Ability/DawnCondensedDew", displayType = StatusDisplayType.Special } },
        { StatusType.HolyTinct,        new StatusDisplayData { name = "圣辉酊液", descriptionTemplate = "回合开始额外获得 {0} 点能量",             iconPath = "Icon/Ability/HolyTinct"       , displayType = StatusDisplayType.Special } },
        { StatusType.Duplicate,        new StatusDisplayData { name = "复制",     descriptionTemplate = "打出的下一张牌会额外打出一次",            iconPath = "Icon/Ability/Duplicate"       , displayType = StatusDisplayType.Special } },
        { StatusType.RetainHand,       new StatusDisplayData { name = "安绪浆液", descriptionTemplate = "回合结束时保留手牌",                     iconPath = "Icon/Ability/RetainHand"      , displayType = StatusDisplayType.Special } },
        { StatusType.VoidDust,         new StatusDisplayData { name = "寂灭",     descriptionTemplate = "每回合结束时失去 9 点生命",               iconPath = "Icon/Ability/VoidDust"        , displayType = StatusDisplayType.Debuff  } },
        { StatusType.SovereignGlare,   new StatusDisplayData { name = "君威之睨", descriptionTemplate = "每回合开始获得 {0} 层力量",              iconPath = "Icon/Ability/SovereignGlare"  , displayType = StatusDisplayType.Buff    } },
        { StatusType.GuardianElixir,   new StatusDisplayData { name = "守护神汁", descriptionTemplate = "下回合开始时获得 10 点护盾",              iconPath = "Icon/Ability/GuardianElixir"  , displayType = StatusDisplayType.Buff    } },
        { StatusType.LuckyBlock,       new StatusDisplayData { name = "幸运",     descriptionTemplate = "抵挡下 {0} 次伤害",                     iconPath = "Icon/Ability/LuckyBlock"      , displayType = StatusDisplayType.Buff    } },
        { StatusType.Fetter,           new StatusDisplayData { name = "枷锁",     descriptionTemplate = "每层降低攻击力 {0} 点",                  iconPath = "Icon/Ability/Fetter"          , displayType = StatusDisplayType.Debuff  } },
        { StatusType.Dizzy,            new StatusDisplayData { name = "眩晕",     descriptionTemplate = "每回合只能打出2张牌，回合结束减少1层",    iconPath = "Icon/Ability/Dizzy"           , displayType = StatusDisplayType.Debuff  } },
        { StatusType.Fear,             new StatusDisplayData { name = "恐惧",     descriptionTemplate = "下一次攻击伤害-6，攻击后减少1层",         iconPath = "Icon/Ability/Fear"            , displayType = StatusDisplayType.Debuff  } },
        { StatusType.Scorch,           new StatusDisplayData { name = "灼烧",     descriptionTemplate = "每层回合结束时受到1点伤害，层数不减少",    iconPath = "Icon/Ability/Scorch"          , displayType = StatusDisplayType.Debuff  } },
    };

    /// <summary>获取指定类型的数据（未配置则返回默认值）</summary>
    public static StatusDisplayData Get(StatusType type)
    {
        if (_data.TryGetValue(type, out var data))
            return data;

        return new StatusDisplayData
        {
            name = type.ToString(),
            descriptionTemplate = "层数: {0}",
            iconPath = "Icon/Buff/Default",
            displayType = StatusDisplayType.Special
        };
    }
}
