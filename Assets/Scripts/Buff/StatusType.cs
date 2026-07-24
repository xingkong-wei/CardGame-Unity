using UnityEngine;

/// <summary>
/// 状态效果类型枚举
/// </summary>
public enum StatusType
{
    // ===== 属性Buff =====
    /// <summary>力量：攻击时额外增加伤害</summary>
    Strength,
    /// <summary>虚弱：攻击时减少伤害</summary>
    Weak,
    /// <summary>易伤：受到伤害增加</summary>
    Vulnerable,
    /// <summary>虚弱（易伤解除）</summary>
    Frail,
    
    // ===== 特殊能力 =====
    /// <summary>荆棘：受到攻击时反弹伤害</summary>
    Thorns,
    /// <summary>吸血：攻击时偷取生命</summary>
    Lifesteal,
    /// <summary>护盾：每回合开始时增加的护甲</summary>
    PlatedArmor,
    /// <summary>金属化：回合结束获得护甲</summary>
    Metallicize,
    
    // ===== 持续伤害 =====
    /// <summary>中毒：每回合受到持续伤害</summary>
    Poison,
    /// <summary>燃烧：每回合受到持续伤害</summary>
    Burning,
    /// <summary>流血：攻击时受到伤害</summary>
    Bleeding,
    
    // ===== 增益效果 =====
    /// <summary>护甲强化：回合开始时额外获得护甲</summary>
    ArmorUp,
    /// <summary>力量爆发：每回合开始获得力量</summary>
    Rage,
    /// <summary>集中：抽牌时额外抽牌</summary>
    Focus,
    /// <summary>再生：每回合恢复生命</summary>
    Regeneration,
    
    // ===== 负面效果 =====
    /// <summary>易伤（已受伤）：受到伤害增加</summary>
    Wounded,
    /// <summary>锁甲：无法获得护甲</summary>
    LockedArmor,
    /// <summary>诅咒：每回合受到最大生命值的伤害</summary>
    Curse,
    
    /// <summary>魔能：每点提升1点伤害（杀戮尖塔风格）</summary>
    Power,

    // ===== 元素亲和度 =====
    /// <summary>火亲和度：每层使法术攻击伤害+1（上限10层）</summary>
    FireAffinity,
    /// <summary>冰亲和度：每层回合结束+1格挡（上限10层）</summary>
    IceAffinity,
    /// <summary>电亲和度：每层抽牌数+0.5（上限10层）</summary>
    LightningAffinity,
    
    // ===== 特殊卡牌效果 =====
    /// <summary>冥想：下一张法术牌费用-1，否则所有法术牌费用-1</summary>
    Meditation,
    /// <summary>卡牌费用减1：下几张法术牌费用-1</summary>
    CardCostMinus1,
    /// <summary>法术共鸣：每回合开始获得1层亲和度最高的元素</summary>
    SpellResonance,
    /// <summary>奥术智慧：本回合亲和度叠加翻倍</summary>
    AffinityDouble,
    /// <summary>魔杖充能：本回合下次攻击双倍伤害</summary>
    WandCharging,
    /// <summary>元素主宰：每回合开始获得火、冰、电亲和度各1层</summary>
    ElementalDominance,
    /// <summary>聚灵：亲和度上限从10层提升至15层</summary>
    SpiritFocus,
    /// <summary>大法师袍：每打出一张法术牌，获得2点格挡</summary>
    GrandMageRobe,
    /// <summary>极地风暴：每次获得或消耗冰亲和度，对所有敌人造成4点伤害</summary>
    PolarStorm,

    // ===== 属性Buff（敏捷）=====
    /// <summary>敏捷：每层使获得的格挡+1</summary>
    Agility,

    // ===== 临时效果 =====
    /// <summary>启明凝露：每层每回合开始额外抽1张牌（持续回合）</summary>
    DawnCondensedDew,
    /// <summary>圣辉酊液：每层每回合开始额外获得1点能量（持续回合）</summary>
    HolyTinct,
    /// <summary>复制：本回合打出的下一张牌额外打出一次</summary>
    Duplicate,
    /// <summary>安绪浆液：回合结束时保留手牌</summary>
    RetainHand,
    /// <summary>寂灭：每回合结束时失去生命</summary>
    VoidDust,
    /// <summary>君威之睨：每回合开始获得力量</summary>
    SovereignGlare,
    /// <summary>守护神汁：下回合开始获得护盾</summary>
    GuardianElixir,
    /// <summary>幸运：抵挡下1次伤害</summary>
    LuckyBlock,
    /// <summary>枷锁：每层降低敌人1点攻击伤害</summary>
    Fetter,
    /// <summary>缩小：每层使敌人攻击伤害减少30%（乘法叠加）</summary>
    Shrink,
    /// <summary>超巨化：下1张攻击牌造成3倍伤害（可跨回合保留）</summary>
    GiantGrowth,
}

/// <summary>
/// 状态效果显示类型
/// </summary>
public enum StatusDisplayType
{
    Buff,   // 增益效果（绿色）
    Debuff, // 减益效果（红色）
    Special // 特殊效果（蓝色）
}
