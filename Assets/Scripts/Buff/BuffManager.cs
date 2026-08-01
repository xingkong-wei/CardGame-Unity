using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Buff/状态管理器 - 管理战斗中所有状态效果
/// </summary>
public class BuffManager
{
    private static BuffManager _instance;
    public static BuffManager Instance => _instance ??= new BuffManager();

    /// <summary>所有状态效果列表</summary>
    private List<StatusEffect> statusEffects = new List<StatusEffect>();

    /// <summary>通知UI更新的回调</summary>
    public event System.Action OnBuffChanged;

    /// <summary>亲和度获得值修改事件：参数(元素类型, 层数) 返回修改后层数</summary>
    public event System.Func<StatusType, int, int> OnAffinityModify;

    /// <summary>亲和度消耗事件：参数(元素类型, 消耗层数)</summary>
    public event System.Action<StatusType, int> OnAffinityConsumed;

    // ===== 冥想效果追踪（与卡牌费用减1共用计数器）=====
    /// <summary>还需要费用减免的法术牌数量（冥想+卡牌费用减1能力）</summary>
    public int spellCostDiscountCount = 0;

    private BuffManager()
    {
        Reset();
    }

    /// <summary>
    /// 重置所有状态
    /// </summary>
    public void Reset()
    {
        statusEffects.Clear();
        OnBuffChanged?.Invoke();
    }

    /// <summary>
    /// 添加状态效果
    /// </summary>
    public void AddStatus(StatusType type, int stack = 1, int duration = -1)
    {
        bool isAffinity = IsAffinityType(type);
        int iceAdd = 0;

        // 亲和度类型：调用 modifyAffinityGain 回调处理翻倍等逻辑
        if (isAffinity)
        {
            // 查找所有有 modifyAffinityGain 回调的状态（如 AffinityDouble），
            // 用它们各自的层数来计算翻倍
            foreach (var se in statusEffects)
            {
                if (se.modifyAffinityGain != null && se.stack > 0)
                    stack = se.modifyAffinityGain(se, stack);
            }

            stack = OnAffinityModify?.Invoke(type, stack) ?? stack;
        }

        int maxStack = GetMaxStack(type);
        
        StatusEffect existing = statusEffects.Find(s => s.type == type);
        
        if (existing != null)
        {
            int canAdd = maxStack - existing.stack;
            int actualAdd = Mathf.Min(stack, canAdd);
            existing.stack += actualAdd;
            if (existing.remainingTurns != -1 && duration != -1)
            {
                existing.remainingTurns += duration;
            }
            existing.UpdateDescription();
            iceAdd = actualAdd;

            // 触发 onAdded 回调
            existing.onAdded?.Invoke(existing, actualAdd);
        }
        else
        {
            int actualStack = Mathf.Min(stack, maxStack);
            StatusEffect newEffect = new StatusEffect(type, actualStack, duration);
            StatusCallbacks.Inject(newEffect);
            statusEffects.Add(newEffect);
            iceAdd = actualStack;

            newEffect.onAdded?.Invoke(newEffect, actualStack);
        }

        OnBuffChanged?.Invoke();

        // 极地风暴
        if (type == StatusType.IceAffinity && iceAdd > 0)
        {
            TriggerPolarStorm(iceAdd);
        }
    }

    /// <summary>
    /// 获取状态效果的最大层数
    /// </summary>
    private int GetMaxStack(StatusType type)
    {
        if (IsAffinityType(type))
        {
            int baseMax = GetStack(StatusType.SpiritFocus) > 0 ? 15 : 10;
            int relicMax = RelicManager.Instance.GetAffinityMaxStack();
            return Mathf.Max(baseMax, relicMax);
        }
        return int.MaxValue;
    }

    /// <summary>是否为亲和度类型</summary>
    private bool IsAffinityType(StatusType type)
    {
        return type == StatusType.FireAffinity ||
               type == StatusType.IceAffinity ||
               type == StatusType.LightningAffinity;
    }

    /// <summary>
    /// 移除状态效果
    /// </summary>
    public void RemoveStatus(StatusType type, int stack = -1)
    {
        StatusEffect effect = statusEffects.Find(s => s.type == type);
        if (effect == null) return;

        int iceRemoved = 0;
        int actuallyRemoved = 0;

        if (stack < 0 || effect.stack <= stack)
        {
            iceRemoved = effect.stack;
            actuallyRemoved = effect.stack;
            statusEffects.Remove(effect);
        }
        else
        {
            iceRemoved = stack;
            actuallyRemoved = stack;
            effect.stack -= stack;
            effect.UpdateDescription();
        }

        OnBuffChanged?.Invoke();

        // 亲和度消耗事件
        if (actuallyRemoved > 0 && IsAffinityType(type))
        {
            OnAffinityConsumed?.Invoke(type, actuallyRemoved);
            effect.onAffinityConsumed?.Invoke(effect, actuallyRemoved);
        }

        // 触发 onRemoved 回调
        effect.onRemoved?.Invoke(effect, actuallyRemoved);

        // 极地风暴
        if (type == StatusType.IceAffinity && iceRemoved > 0 && GetStack(StatusType.PolarStorm) > 0)
        {
            TriggerPolarStorm(iceRemoved);
        }
    }

    /// <summary>
    /// 获取状态层数
    /// </summary>
    public int GetStack(StatusType type)
    {
        StatusEffect effect = statusEffects.Find(s => s.type == type);
        return effect?.stack ?? 0;
    }

    /// <summary>
    /// 检查是否有该状态
    /// </summary>
    public bool HasStatus(StatusType type)
    {
        return statusEffects.Exists(s => s.type == type && s.stack > 0);
    }

    /// <summary>
    /// 获取所有状态
    /// </summary>
    public List<StatusEffect> GetAllStatus()
    {
        return new List<StatusEffect>(statusEffects);
    }

    // ==================== 回合触发效果 ====================

    /// <summary>
    /// 回合开始时触发效果
    /// </summary>
    public void OnTurnStart()
    {
        // 快照遍历，防止回调中修改 statusEffects 集合
        var snapshot = GetAllStatus();
        foreach (var effect in snapshot)
            effect.onTurnStart?.Invoke(effect);

        OnBuffChanged?.Invoke();
    }

    /// <summary>
    /// 玩家回合结束时触发效果
    /// </summary>
    public void OnPlayerTurnEnd()
    {
        var snapshot = GetAllStatus();
        foreach (var effect in snapshot)
            effect.onPlayerTurnEnd?.Invoke(effect);

        OnBuffChanged?.Invoke();
    }

    /// <summary>
    /// 回合结束时触发效果
    /// </summary>
    public void OnTurnEnd()
    {
        var snapshot = GetAllStatus();
        foreach (var effect in snapshot)
            effect.onTurnEnd?.Invoke(effect);

        ReduceDuration();
        OnBuffChanged?.Invoke();
    }

    /// <summary>
    /// 攻击时修改伤害（力量/虚弱/魔能/缩小）
    /// </summary>
    public int ModifyAttackDamage(int baseDamage, bool includeWandCharging = true)
    {
        int modifiedDamage = baseDamage;

        foreach (var effect in statusEffects)
        {
            if (effect.modifyAttackDamage != null)
                modifiedDamage = effect.modifyAttackDamage(effect, modifiedDamage);
        }

        return modifiedDamage;
    }

    /// <summary>
    /// 法术攻击时修改伤害（包含火亲和度）
    /// 注意：此方法会先调 ModifyAttackDamage，适合首次计算伤害的场景
    /// 如果已经调用过 ModifyAttackDamage，请用 ApplySpellDamageModifier
    /// </summary>
    public int ModifySpellDamage(int baseDamage)
    {
        int modifiedDamage = ModifyAttackDamage(baseDamage);

        foreach (var effect in statusEffects)
        {
            if (effect.modifySpellDamage != null)
                modifiedDamage = effect.modifySpellDamage(effect, modifiedDamage);
        }

        return modifiedDamage;
    }

    /// <summary>
    /// 仅应用 modifySpellDamage 回调（不重复应用 modifyAttackDamage）
    /// 用于已经调用过 ModifyAttackDamage 的场景
    /// </summary>
    public int ApplySpellDamageModifier(int damage)
    {
        foreach (var effect in statusEffects)
        {
            if (effect.modifySpellDamage != null)
                damage = effect.modifySpellDamage(effect, damage);
        }
        return damage;
    }

    /// <summary>
    /// 受到伤害时修改（易伤）
    /// </summary>
    public int ModifyTakenDamage(int originalDamage)
    {
        int result = originalDamage;

        foreach (var effect in statusEffects)
        {
            if (effect.modifyTakenDamage != null)
                result = effect.modifyTakenDamage(effect, result);
        }

        return result;
    }

    /// <summary>
    /// 攻击造成伤害后（吸血）
    /// </summary>
    public void OnDealDamage(int damage)
    {
        foreach (var effect in statusEffects)
            effect.onDealDamage?.Invoke(effect, damage);
    }

    /// <summary>
    /// 获得护甲时修改（锁甲/脆弱/敏捷）
    /// </summary>
    public int ModifyDefenseGain(int baseDefense)
    {
        int result = baseDefense;

        foreach (var effect in statusEffects)
        {
            if (effect.modifyDefenseGain != null)
                result = effect.modifyDefenseGain(effect, result);
        }

        return Mathf.Max(result, 0);
    }

    /// <summary>
    /// 获取额外抽牌数量（集中+电亲和度）
    /// </summary>
    public int GetExtraDrawCards()
    {
        int extra = 0;

        foreach (var effect in statusEffects)
        {
            if (effect.getExtraDrawCards != null)
                extra += effect.getExtraDrawCards(effect);
        }

        return extra;
    }

    /// <summary>
    /// 减少持续回合
    /// </summary>
    private void ReduceDuration()
    {
        List<StatusType> toRemove = new List<StatusType>();
        foreach (var status in statusEffects)
        {
            if (status.remainingTurns > 0)
            {
                status.remainingTurns--;
                if (status.remainingTurns <= 0)
                    toRemove.Add(status.type);
            }
        }
        foreach (var type in toRemove)
            RemoveStatus(type);
    }

    // ===== 冥想法术相关 =====

    /// <summary>
    /// 激活冥想效果（每调用一次，下一张法术牌费用-1）
    /// </summary>
    public void ActivateMeditation()
    {
        spellCostDiscountCount++;
    }

    /// <summary>
    /// 法术牌被使用时，消耗一次费用减免，并刷新所有法术牌的费用显示
    /// </summary>
    public void OnSpellCardUsed()
    {
        if (spellCostDiscountCount > 0)
        {
            spellCostDiscountCount--;
            
            // 减少 Meditation 状态的层数（ Meditation 和 CardCostMinus1 都添加到 Meditation 了）
            int meditationStack = GetStack(StatusType.Meditation);
            if (meditationStack > 0)
            {
                RemoveStatus(StatusType.Meditation, 1);
            }
            else
            {
                // 备用：从 CardCostMinus1 减少
                RemoveStatus(StatusType.CardCostMinus1, 1);
            }
            
            // 刷新所有法术牌的费用显示（可能从绿色变回白色）
            FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
            if (fightUI != null)
            {
                foreach (var cardItem in fightUI.GetCardItemList())
                {
                    if (cardItem != null && cardItem.IsSpellCard())
                    {
                        cardItem.RefreshCostDisplay();
                    }
                }
            }
        }

        // 大法师袍：每打出一张法术牌，每层获得2点格挡
        int grandMageRobe = GetStack(StatusType.GrandMageRobe);
        if (grandMageRobe > 0)
        {
            int block = grandMageRobe * 2;
            FightManager.Instance.DefenseCount += block;
            FightUI fightUI2 = UIManager.Instance.GetUI<FightUI>("FightUI");
            if (fightUI2 != null)
            {
                fightUI2.UpdateDefense();
            }
        }

        // 极地风暴：打出法术牌时也会触发冰亲和度变化
        // （具体由 AddStatus/RemoveStatus 中的冰亲和度变化触发）
    }

    /// <summary>
    /// 极地风暴：按冰亲和度变化层数触发多次伤害
    /// </summary>
    /// <param name="times">触发次数（等于获得或消耗的冰亲和度层数）</param>
    private void TriggerPolarStorm(int times)
    {
        int polarStorm = GetStack(StatusType.PolarStorm);
        if (polarStorm == 0) return;
        if (FightManager.Instance == null) return;

        // 每层每触发造成2点伤害（使用 EnemyManager 缓存，避免 FindObjectsOfType）
        for (int t = 0; t < times; t++)
        {
            foreach (Enemy enemy in EnemyManager.Instance.GetAliveEnemies())
            {
                enemy.Hit(2 * polarStorm);
            }
        }
    }

    /// <summary>
    /// 获取法术费用减免值
    /// 返回true表示费用-1
    /// </summary>
    public bool HasSpellCostDiscount()
    {
        return spellCostDiscountCount > 0;
    }

    /// <summary>
    /// 获取当前费用减免次数
    /// </summary>
    public int GetSpellCostDiscountCount()
    {
        return spellCostDiscountCount;
    }

    /// <summary>
    /// 恢复所有手牌中法术牌的费用（绿色->白色）
    /// </summary>
    public void RestoreSpellCostForAllSpellCards()
    {
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI == null) return;

        foreach (var cardItem in fightUI.GetCardItemList())
        {
            if (cardItem != null && cardItem.IsSpellCard())
            {
                cardItem.RestoreMeditationCost();
            }
        }
    }

    /// <summary>
    /// 应用冥想效果到所有手牌中的法术牌
    /// </summary>
    public void ApplySpellCostDiscountToAllSpellCards()
    {
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI == null) return;

        foreach (var cardItem in fightUI.GetCardItemList())
        {
            if (cardItem != null && cardItem.IsSpellCard())
            {
                cardItem.ApplyMeditationCostReduction();
            }
        }
    }

    /// <summary>
    /// 冥想效果是否激活（兼容旧代码）
    /// </summary>
    public bool IsMeditationActive()
    {
        return spellCostDiscountCount > 0;
    }

    /// <summary>
    /// 结束冥想效果（回合结束时调用，如果没有使用法术牌则应用费用减免）
    /// </summary>
    public void EndMeditation()
    {
        if (spellCostDiscountCount > 0)
        {
            // 没有使用法术牌，应用全部法术牌费用-1
            ApplySpellCostDiscountToAllSpellCards();
        }
        
        // 清空计数器
        spellCostDiscountCount = 0;
        
        // 清除冥想状态图标（层数归零时图标应该消失）
        RemoveStatus(StatusType.Meditation);
    }

    // ===== 法术共鸣相关 =====

    /// <summary>
    /// 获取亲和度最高的元素类型列表（可能有多个，用于随机选择）
    /// </summary>
    public List<StatusType> GetTopAffinities()
    {
        int fire = GetStack(StatusType.FireAffinity);
        int ice = GetStack(StatusType.IceAffinity);
        int lightning = GetStack(StatusType.LightningAffinity);

        int maxStack = Mathf.Max(fire, ice, lightning);
        List<StatusType> topAffinities = new List<StatusType>();

        if (fire == maxStack) topAffinities.Add(StatusType.FireAffinity);
        if (ice == maxStack) topAffinities.Add(StatusType.IceAffinity);
        if (lightning == maxStack) topAffinities.Add(StatusType.LightningAffinity);

        return topAffinities;
    }

    /// <summary>
    /// 获取亲和度最高的元素类型（随机选择，如果有多个则随机）
    /// </summary>
    public StatusType GetRandomTopAffinity()
    {
        List<StatusType> topAffinities = GetTopAffinities();
        if (topAffinities.Count == 0)
        {
            // 没有亲和度，随机返回一个
            StatusType[] all = { StatusType.FireAffinity, StatusType.IceAffinity, StatusType.LightningAffinity };
            return all[Random.Range(0, all.Length)];
        }
        return topAffinities[Random.Range(0, topAffinities.Count)];
    }

    /// <summary>
    /// 获取亲和度名称
    /// </summary>
    public string GetAffinityName(StatusType type)
    {
        return StatusDisplayDB.Get(type).name;
    }

    /// <summary>
    /// 将一种元素亲和度全部转换为另一种（1:1转换）
    /// </summary>
    public void ConvertAffinity(StatusType from, StatusType to)
    {
        if (from == to) return;

        int amount = GetStack(from);
        if (amount <= 0) return;

        // 清空来源
        RemoveStatus(from, amount);

        // 获取目标当前层数，计算还能加多少
        int targetStack = GetStack(to);
        int maxStack = GetMaxStack(to); // 亲和度上限（支持聚灵+5）
        int canAdd = maxStack - targetStack;
        int actualAdd = Mathf.Min(amount, canAdd);

        if (actualAdd > 0)
        {
            AddStatus(to, actualAdd);
        }

        string fromName = GetAffinityName(from);
        string toName = GetAffinityName(to);
        UIManager.Instance.ShowTip($"{fromName}×{amount} → {toName}×{actualAdd}", Color.cyan);
    }
}
