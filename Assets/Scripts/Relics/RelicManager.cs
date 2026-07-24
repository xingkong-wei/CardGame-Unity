using System.Collections.Generic;

/// <summary>
/// 遗物管理器 - 管理遗物实例和生命周期触发
/// </summary>
public class RelicManager
{
    private static RelicManager _instance;
    public static RelicManager Instance => _instance ??= new RelicManager();

    private List<RelicBase> relicInstances = new List<RelicBase>();

    // 追踪上次亲和度层数，用于检测变化
    private int lastFireStack;
    private int lastIceStack;
    private int lastLightningStack;

    private RelicManager()
    {
        BuffManager.Instance.OnBuffChanged += OnBuffChanged;
        BuffManager.Instance.OnAffinityModify += OnAffinityModify;
        BuffManager.Instance.OnAffinityConsumed += OnAffinityConsumed;
    }

    private void OnAffinityConsumed(StatusType type, int amount)
    {
        foreach (var r in relicInstances)
            r.OnAffinityConsumed(type, amount);
    }

    private int OnAffinityModify(StatusType type, int stack)
    {
        int result = stack;
        foreach (var r in relicInstances)
            result = r.ModifyAffinityGain(type, result);
        return result;
    }

    /// <summary>
    /// 添加遗物实例
    /// </summary>
    public void AddRelic(RelicBase relic)
    {
        if (relic == null) return;
        relicInstances.Add(relic);
    }

    /// <summary>
    /// 清空所有遗物实例
    /// </summary>
    public void Clear()
    {
        relicInstances.Clear();
    }

    // ===== Buff 变化监听 =====

    private void OnBuffChanged()
    {
        int fire = BuffManager.Instance.GetStack(StatusType.FireAffinity);
        int ice = BuffManager.Instance.GetStack(StatusType.IceAffinity);
        int lightning = BuffManager.Instance.GetStack(StatusType.LightningAffinity);

        if (fire != lastFireStack)
        {
            lastFireStack = fire;
            foreach (var r in relicInstances)
                r.OnAffinityChanged(StatusType.FireAffinity, fire);
        }
        if (ice != lastIceStack)
        {
            lastIceStack = ice;
            foreach (var r in relicInstances)
                r.OnAffinityChanged(StatusType.IceAffinity, ice);
        }
        if (lightning != lastLightningStack)
        {
            lastLightningStack = lightning;
            foreach (var r in relicInstances)
                r.OnAffinityChanged(StatusType.LightningAffinity, lightning);
        }
    }

    // ===== 生命周期触发 =====

    public void TriggerBattleStart()
    {
        // 重置亲和度追踪
        lastFireStack = 0;
        lastIceStack = 0;
        lastLightningStack = 0;

        foreach (var r in relicInstances)
            r.OnBattleStart();
    }

    public void TriggerTurnStart()
    {
        foreach (var r in relicInstances)
            r.OnTurnStart();
    }

    public void TriggerTurnEnd()
    {
        foreach (var r in relicInstances)
            r.OnTurnEnd();
    }

    public int TriggerPlayerHit(int damage)
    {
        int result = damage;
        foreach (var r in relicInstances)
            result = r.OnPlayerHit(result);
        return result;
    }

    public int TriggerDealDamage(int damage)
    {
        int result = damage;
        foreach (var r in relicInstances)
            result = r.OnDealDamage(result);
        return result;
    }

    public int TriggerGainBlock(int block)
    {
        int result = block;
        foreach (var r in relicInstances)
            result = r.OnGainBlock(result);
        return result;
    }

    public void TriggerCardPlayed(CardItem card)
    {
        foreach (var r in relicInstances)
            r.OnCardPlayed(card);
    }

    public void TriggerCardDrawn()
    {
        foreach (var r in relicInstances)
            r.OnCardDrawn();
    }

    /// <summary>
    /// 查询遗物修改后的亲和度上限（取最大值，默认10）
    /// </summary>
    /// <summary>
    /// 查询是否有遗物提供全局法术牌费用-1
    /// </summary>
    /// <summary>
    /// 获取攻击伤害预览总倍率（所有遗物倍率相乘）
    /// </summary>
    public float GetDamagePreviewMultiplier()
    {
        float result = 1f;
        foreach (var r in relicInstances)
            result *= r.GetDamagePreviewMultiplier();
        return result;
    }

    public bool HasSpellCostReduction()
    {
        foreach (var r in relicInstances)
            if (r.HasSpellCostReduction()) return true;
        return false;
    }

    public int GetAffinityMaxStack()
    {
        int max = 10;
        foreach (var r in relicInstances)
        {
            int modified = r.ModifyAffinityMaxStack();
            if (modified > max) max = modified;
        }
        return max;
    }

    public int TriggerGainEnergy(int energy)
    {
        int result = energy;
        foreach (var r in relicInstances)
            result = r.OnGainEnergy(result);
        return result;
    }

    public void TriggerEnemyKilled(Enemy enemy)
    {
        foreach (var r in relicInstances)
            r.OnEnemyKilled(enemy);
    }
}
