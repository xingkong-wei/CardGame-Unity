using UnityEngine;

/// <summary>
/// 遗物效果基类 - 所有遗物脚本继承此类
/// 通过 scriptName 反射创建实例
/// </summary>
public abstract class RelicBase
{
    /// <summary>遗物的数据配置</summary>
    protected RelicData data;

    /// <summary>
    /// 初始化遗物数据
    /// </summary>
    public virtual void Init(RelicData relicData)
    {
        data = relicData;
    }

    // ===== 生命周期钩子（子类按需重写）=====

    /// <summary>战斗开始时触发</summary>
    public virtual void OnBattleStart() { }

    /// <summary>玩家回合开始时触发</summary>
    public virtual void OnTurnStart() { }

    /// <summary>玩家回合结束时触发</summary>
    public virtual void OnTurnEnd() { }

    /// <summary>玩家受到伤害时触发（在格挡计算前）</summary>
    public virtual int OnPlayerHit(int incomingDamage) { return incomingDamage; }

    /// <summary>玩家造成攻击伤害时触发</summary>
    public virtual int OnDealDamage(int damage) { return damage; }

    /// <summary>获得格挡时触发</summary>
    public virtual int OnGainBlock(int block) { return block; }

    /// <summary>打出卡牌时触发</summary>
    public virtual void OnCardPlayed(CardItem card) { }

    /// <summary>抽牌时触发</summary>
    public virtual void OnCardDrawn() { }

    /// <summary>获得能量时触发</summary>
    public virtual int OnGainEnergy(int energy) { return energy; }

    /// <summary>敌人死亡时触发</summary>
    public virtual void OnEnemyKilled(Enemy enemy) { }

    /// <summary>亲和度层数变化时触发：参数(元素类型, 当前层数)</summary>
    public virtual void OnAffinityChanged(StatusType type, int currentStack) { }

    /// <summary>修改亲和度获得值：参数(元素类型, 层数) 返回修改后层数</summary>
    public virtual int ModifyAffinityGain(StatusType type, int stack) { return stack; }

    /// <summary>修改亲和度上限：返回新上限，默认0表示不修改</summary>
    public virtual int ModifyAffinityMaxStack() { return 0; }

    /// <summary>亲和度被消耗时触发：参数(元素类型, 消耗层数)</summary>
    public virtual void OnAffinityConsumed(StatusType type, int amount) { }

    /// <summary>是否全局法术牌费用-1</summary>
    public virtual bool HasSpellCostReduction() { return false; }

    /// <summary>攻击伤害预览倍率（预览用，不实际消耗标记），默认1.0</summary>
    public virtual float GetDamagePreviewMultiplier() { return 1f; }
}
