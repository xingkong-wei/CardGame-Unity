using UnityEngine;

/// <summary>
/// 单个状态效果实例
/// </summary>
[System.Serializable]
public class StatusEffect
{
    /// <summary>状态类型</summary>
    public StatusType type;
    
    /// <summary>当前层数</summary>
    public int stack;
    
    /// <summary>剩余持续回合（-1表示永久）</summary>
    public int remainingTurns;
    
    /// <summary>状态效果名称</summary>
    public string effectName;
    
    /// <summary>描述文本</summary>
    public string description;
    
    /// <summary>图标路径</summary>
    public string iconPath;
    
    /// <summary>显示类型</summary>
    public StatusDisplayType displayType;

    public StatusEffect()
    {
        stack = 0;
        remainingTurns = -1;
    }

    public StatusEffect(StatusType type, int stack = 1, int duration = -1)
    {
        this.type = type;
        this.stack = stack;
        this.remainingTurns = duration;
        InitFromType();
    }

    /// <summary>
    /// 根据类型初始化属性
    /// </summary>
    private void InitFromType()
    {
        switch (type)
        {
            case StatusType.Strength:
                effectName = "力量";
                description = $"攻击伤害 +{stack}";
                iconPath = "Icon/Buff/Strength";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.Weak:
                effectName = "虚弱";
                description = $"攻击伤害 -{stack * 25}%";
                iconPath = "Icon/Buff/Weak";
                displayType = StatusDisplayType.Debuff;
                break;
            case StatusType.Vulnerable:
                effectName = "易伤";
                description = $"受到伤害 +25%（每回合减少1层）";
                iconPath = "Icon/Buff/Vulnerable";
                displayType = StatusDisplayType.Debuff;
                break;
            case StatusType.Frail:
                effectName = "脆弱";
                description = $"护甲获得 -{stack * 25}%";
                iconPath = "Icon/Buff/Frail";
                displayType = StatusDisplayType.Debuff;
                break;
            case StatusType.Thorns:
                effectName = "荆棘";
                description = $"受到攻击时反弹 {stack} 伤害";
                iconPath = "Icon/Buff/Thorns";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.Lifesteal:
                effectName = "吸血";
                description = $"攻击时偷取 {stack} 生命";
                iconPath = "Icon/Buff/Lifesteal";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.PlatedArmor:
                effectName = "覆甲";
                description = $"回合结束时获得 {stack} 格挡";
                iconPath = "Icon/Buff/PlatedArmor";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.Metallicize:
                effectName = "金属化";
                description = $"回合结束获得 {stack} 护甲";
                iconPath = "Icon/Buff/Metallicize";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.Poison:
                effectName = "中毒";
                description = $"每回合失去 {stack} 生命";
                iconPath = "Icon/Buff/Poison";
                displayType = StatusDisplayType.Debuff;
                break;
            case StatusType.Burning:
                effectName = "燃烧";
                description = $"每回合失去 {stack} 生命";
                iconPath = "Icon/Buff/Burning";
                displayType = StatusDisplayType.Debuff;
                break;
            case StatusType.Bleeding:
                effectName = "流血";
                description = $"回合结束失去 {stack} 生命";
                iconPath = "Icon/Buff/Bleeding";
                displayType = StatusDisplayType.Debuff;
                break;
            case StatusType.ArmorUp:
                effectName = "护甲增强";
                description = $"回合开始获得 {stack} 护甲";
                iconPath = "Icon/Buff/ArmorUp";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.Rage:
                effectName = "怒火";
                description = $"每回合开始获得 {stack} 力量";
                iconPath = "Icon/Buff/Rage";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.Focus:
                effectName = "集中";
                description = $"每回合额外抽 {stack} 张牌";
                iconPath = "Icon/Buff/Focus";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.Regeneration:
                effectName = "再生";
                description = $"每回合恢复 {stack} 生命";
                iconPath = "Icon/Buff/Regeneration";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.Wounded:
                effectName = "受伤";
                description = $"受到伤害 +{stack * 25}%";
                iconPath = "Icon/Buff/Wounded";
                displayType = StatusDisplayType.Debuff;
                break;
            case StatusType.LockedArmor:
                effectName = "锁甲";
                description = $"无法获得护甲";
                iconPath = "Icon/Buff/LockedArmor";
                displayType = StatusDisplayType.Debuff;
                break;
            case StatusType.Curse:
                effectName = "诅咒";
                description = $"每回合失去最大生命的 {stack}%";
                iconPath = "Icon/Buff/Curse";
                displayType = StatusDisplayType.Debuff;
                break;
            case StatusType.Power:
                effectName = "魔能";
                description = $"每层 +{stack} 伤害";
                iconPath = "Icon/Ability/Power";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.FireAffinity:
                effectName = "火亲和度";
                description = $"每层法术攻击伤害 +{stack}（上限10层）";
                iconPath = "Icon/Ability/FireAffinity";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.IceAffinity:
                effectName = "冰亲和度";
                description = $"每层回合结束 +{stack} 格挡（上限10层）";
                iconPath = "Icon/Ability/IceAffinity";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.LightningAffinity:
                effectName = "电亲和度";
                description = $"每层抽牌数 +{Mathf.CeilToInt(stack * 0.5f)}（上限10层）";
                iconPath = "Icon/Ability/LightningAffinity";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.CardCostMinus1:
                effectName = "卡牌费用减1";
                description = $"下 {stack} 张法术牌费用 -1";
                iconPath = "Icon/Ability/Fee-1";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.SpellResonance:
                effectName = "法术共鸣";
                description = $"每回合开始获得1层亲和度最高的元素";
                iconPath = "Icon/Ability/SpellResonance";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.AffinityDouble:
                effectName = "奥术智慧";
                description = $"本回合亲和度叠加×{Mathf.RoundToInt(Mathf.Pow(2, stack))}";
                iconPath = "Icon/Ability/ArcaneWisdom";
                displayType = StatusDisplayType.Special;
                break;
            case StatusType.ElementalDominance:
                effectName = "元素主宰";
                description = "每回合开始获得火、冰、电亲和度各 +1";
                iconPath = "Icon/Ability/ElementalDominance";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.SpiritFocus:
                effectName = "聚灵";
                description = "亲和度上限提升至15层";
                iconPath = "Icon/Ability/SpiritFocus";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.GrandMageRobe:
                effectName = "大法师袍";
                description = "每打出一张法术牌，获得2点格挡";
                iconPath = "Icon/Ability/GrandMageRobe";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.PolarStorm:
                effectName = "极地风暴";
                description = "每次获得或消耗冰亲和度，对所有敌人造成4点伤害";
                iconPath = "Icon/Ability/PolarStorm";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.Agility:
                effectName = "敏捷";
                description = $"每层获得格挡 +{stack}";
                iconPath = "Icon/Buff/Agility";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.DawnCondensedDew:
                effectName = "启明凝露";
                description = $"每回合开始额外抽 {stack} 张牌";
                iconPath = "Icon/Ability/DawnCondensedDew";
                displayType = StatusDisplayType.Special;
                break;
            case StatusType.HolyTinct:
                effectName = "圣辉酊液";
                description = $"回合开始额外获得 {stack} 点能量";
                iconPath = "Icon/Ability/HolyTinct";
                displayType = StatusDisplayType.Special;
                break;
            case StatusType.Duplicate:
                effectName = "复制";
                description = "打出的下一张牌会额外打出一次";
                iconPath = "Icon/Ability/Duplicate";
                displayType = StatusDisplayType.Special;
                break;
            case StatusType.RetainHand:
                effectName = "安绪浆液";
                description = "回合结束时保留手牌";
                iconPath = "Icon/Ability/RetainHand";
                displayType = StatusDisplayType.Special;
                break;
            case StatusType.VoidDust:
                effectName = "寂灭";
                description = "每回合结束时失去 9 点生命";
                iconPath = "Icon/Ability/VoidDust";
                displayType = StatusDisplayType.Debuff;
                break;
            case StatusType.SovereignGlare:
                effectName = "君威之睨";
                description = $"每回合开始获得 {stack} 层力量";
                iconPath = "Icon/Ability/SovereignGlare";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.GuardianElixir:
                effectName = "守护神汁";
                description = "下回合开始时获得 10 点护盾";
                iconPath = "Icon/Ability/GuardianElixir";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.LuckyBlock:
                effectName = "幸运";
                description = $"抵挡下 {stack} 次伤害";
                iconPath = "Icon/Ability/LuckyBlock";
                displayType = StatusDisplayType.Buff;
                break;
            case StatusType.Fetter:
                effectName = "枷锁";
                description = $"每层降低攻击力 {stack} 点";
                iconPath = "Icon/Ability/Fetter";
                displayType = StatusDisplayType.Debuff;
                break;
            default:
                effectName = type.ToString();
                description = $"层数: {stack}";
                iconPath = "Icon/Buff/Default";
                displayType = StatusDisplayType.Special;
                break;
        }
    }

    /// <summary>
    /// 更新描述（层数变化时调用）
    /// </summary>
    public void UpdateDescription()
    {
        InitFromType();
    }

    /// <summary>
    /// 复制状态效果
    /// </summary>
    public StatusEffect Clone()
    {
        return new StatusEffect(type, stack, remainingTurns);
    }
}
