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

    // ==================== 生命周期回调委托 ====================

    /// <summary>回合开始时触发</summary>
    public System.Action<StatusEffect> onTurnStart;

    /// <summary>玩家回合结束时触发</summary>
    public System.Action<StatusEffect> onPlayerTurnEnd;

    /// <summary>敌人回合结束时触发（完整回合结束）</summary>
    public System.Action<StatusEffect> onTurnEnd;

    /// <summary>敌人回合结束时触发（敌方状态递减，需要 enemy 实例）</summary>
    public System.Action<StatusEffect, Enemy> onEnemyTurnEnd;

    /// <summary>添加状态时触发（新增层数时调用）</summary>
    public System.Action<StatusEffect, int> onAdded;

    /// <summary>移除状态时触发</summary>
    public System.Action<StatusEffect, int> onRemoved;

    /// <summary>受到伤害时修改伤害值（输入原始伤害，返回修正后）</summary>
    public System.Func<StatusEffect, int, int> modifyTakenDamage;

    /// <summary>造成攻击伤害时修改伤害值</summary>
    public System.Func<StatusEffect, int, int> modifyAttackDamage;

    /// <summary>法术攻击时额外修改伤害值（在 modifyAttackDamage 之后调用）</summary>
    public System.Func<StatusEffect, int, int> modifySpellDamage;

    /// <summary>获得护甲时修改护甲值</summary>
    public System.Func<StatusEffect, int, int> modifyDefenseGain;

    /// <summary>攻击造成伤害后触发</summary>
    public System.Action<StatusEffect, int> onDealDamage;

    /// <summary>计算额外抽牌数（返回额外抽牌数）</summary>
    public System.Func<StatusEffect, int> getExtraDrawCards;

    /// <summary>亲和度获得时修改层数（输入原始层数，返回修改后）</summary>
    public System.Func<StatusEffect, int, int> modifyAffinityGain;

    /// <summary>亲和度消耗时触发</summary>
    public System.Action<StatusEffect, int> onAffinityConsumed;

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
    /// 根据类型初始化显示属性（从配置表读取）
    /// </summary>
    private void InitFromType()
    {
        var data = StatusDisplayDB.Get(type);
        effectName = data.name;
        description = data.GetDescription(stack);
        iconPath = data.iconPath;
        displayType = data.displayType;
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
