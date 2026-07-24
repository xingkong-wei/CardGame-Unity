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
        int iceAdd = 0; // 实际增加的冰亲和度层数

        // 亲和度类型：本回合获得的亲和度翻倍（叠加翻倍 = 原有 + 新获得×2）
        if (type == StatusType.FireAffinity ||
            type == StatusType.IceAffinity ||
            type == StatusType.LightningAffinity)
        {
            int affinityDouble = GetStack(StatusType.AffinityDouble);
            if (affinityDouble > 0)
            {
                stack *= Mathf.RoundToInt(Mathf.Pow(2, affinityDouble));
            }

            // 允许外部修改亲和度获得值
            stack = OnAffinityModify?.Invoke(type, stack) ?? stack;
        }

        // 亲和度有上限（只限制新增部分，不限制原有层数）
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
        }
        else
        {
            int actualStack = Mathf.Min(stack, maxStack);
            StatusEffect newEffect = new StatusEffect(type, actualStack, duration);
            statusEffects.Add(newEffect);
            iceAdd = actualStack;
        }

        OnBuffChanged?.Invoke();

        // 极地风暴：按实际获得的冰亲和度层数触发多次伤害
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
        // 亲和度上限：聚灵/遗物可提升
        if (type == StatusType.FireAffinity || 
            type == StatusType.IceAffinity || 
            type == StatusType.LightningAffinity)
        {
            int baseMax = GetStack(StatusType.SpiritFocus) > 0 ? 15 : 10;
            int relicMax = RelicManager.Instance.GetAffinityMaxStack();
            return Mathf.Max(baseMax, relicMax);
        }
        return int.MaxValue;
    }

    /// <summary>
    /// 移除状态效果
    /// </summary>
    public void RemoveStatus(StatusType type, int stack = -1)
    {
        StatusEffect effect = statusEffects.Find(s => s.type == type);
        if (effect == null) return;

        int iceRemoved = 0; // 实际移除的冰亲和度层数
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
        if (actuallyRemoved > 0 && (type == StatusType.FireAffinity ||
            type == StatusType.IceAffinity || type == StatusType.LightningAffinity))
        {
            OnAffinityConsumed?.Invoke(type, actuallyRemoved);
        }

        // 极地风暴：按实际消耗的冰亲和度层数触发多次伤害
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
        // 元素主宰：每层每回合获得火、冰、电亲和度各1层
        int elementalDominance = GetStack(StatusType.ElementalDominance);
        if (elementalDominance > 0)
        {
            AddStatus(StatusType.FireAffinity, elementalDominance);
            AddStatus(StatusType.IceAffinity, elementalDominance);
            AddStatus(StatusType.LightningAffinity, elementalDominance);
            UIManager.Instance.ShowTip($"火、冰、电亲和 +{elementalDominance}", Color.yellow);
        }

        // 法术共鸣：每回合获得亲和度最高的元素（层数=获得次数）
        int spellResonance = GetStack(StatusType.SpellResonance);
        if (spellResonance > 0)
        {
            StatusType maxAffinity = GetRandomTopAffinity();
            AddStatus(maxAffinity, spellResonance);
            UIManager.Instance.ShowTip($"法术共鸣 +{spellResonance} {GetAffinityName(maxAffinity)}", Color.yellow);
        }

        // 君威之睨：每层每回合开始获得力量
        int sovereignGlare = GetStack(StatusType.SovereignGlare);
        if (sovereignGlare > 0)
        {
            AddStatus(StatusType.Strength, sovereignGlare);
        }

        // 守护神汁：下回合开始时获得护盾
        if (HasStatus(StatusType.GuardianElixir))
        {
            FightManager.Instance.DefenseCount += 10;
            FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
            fightUI?.UpdateDefense();
            RemoveStatus(StatusType.GuardianElixir, 99);
        }

        // 力量爆发（临时）
        int rage = GetStack(StatusType.Rage);
        if (rage > 0)
        {
            AddStatus(StatusType.Strength, rage, 1);
        }

        // 覆甲：回合开始时减少1层
        int platedArmor = GetStack(StatusType.PlatedArmor);
        if (platedArmor > 0)
        {
            RemoveStatus(StatusType.PlatedArmor, 1);
        }

        // 圣辉酊液：回合开始时额外获得1点能量，然后减少1层
        int holyTinct = GetStack(StatusType.HolyTinct);
        if (holyTinct > 0)
        {
            FightManager.Instance.CurPowerCount += 1;
            FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
            fightUI?.UpdatePower();
            RemoveStatus(StatusType.HolyTinct, 1);
        }

        // 启明凝露：回合开始时额外抽1张牌，然后减少1层
        int dawn = GetStack(StatusType.DawnCondensedDew);
        if (dawn > 0)
        {
            FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
            if (fightUI != null)
            {
                fightUI.CreateCardItem(1);
                fightUI.UpdateCardItemPos();
                fightUI.UpdateCardCount();
            }
            RemoveStatus(StatusType.DawnCondensedDew, 1);
        }

        // 再生：每层恢复1点生命，回合开始后减少1层
        int regen = GetStack(StatusType.Regeneration);
        if (regen > 0 && FightManager.Instance.CurHp > 0)
        {
            int healAmount = Mathf.Min(regen, FightManager.Instance.MaxHp - FightManager.Instance.CurHp);
            if (healAmount > 0)
            {
                FightManager.Instance.CurHp += healAmount;
                UIManager.Instance.ShowTip($"再生 +{healAmount}", Color.green);
            }

            // 回合开始后减少1层再生
            RemoveStatus(StatusType.Regeneration, 1);

            // 刷新血量显示
            FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
            fightUI?.UpdateHp();
        }

        OnBuffChanged?.Invoke();
    }

    /// <summary>
    /// 回合结束时触发效果
    /// </summary>
    public void OnTurnEnd()
    {
        // 复制：回合结束时移除（仅持续本回合）
        if (HasStatus(StatusType.Duplicate))
        {
            RemoveStatus(StatusType.Duplicate, 99);
        }

        // 易伤：回合结束时减少一层
        int vulnerable = GetStack(StatusType.Vulnerable);
        if (vulnerable > 0)
        {
            RemoveStatus(StatusType.Vulnerable, 1);
        }

        // 虚弱：回合结束时减少一层
        int weak = GetStack(StatusType.Weak);
        if (weak > 0)
        {
            RemoveStatus(StatusType.Weak, 1);
        }

        // 金属化
        int metallicize = GetStack(StatusType.Metallicize);
        if (metallicize > 0)
        {
            FightManager.Instance.DefenseCount += metallicize;
            UIManager.Instance.ShowTip($"金属化 +{metallicize}", Color.cyan);
        }

        // 流血
        int bleeding = GetStack(StatusType.Bleeding);
        if (bleeding > 0)
        {
            FightManager.Instance.GetPlayerHit(bleeding);
            UIManager.Instance.ShowTip($"流血 -{bleeding}", Color.red);
        }

        // 中毒
        int poison = GetStack(StatusType.Poison);
        if (poison > 0)
        {
            FightManager.Instance.GetPlayerHit(poison);
            UIManager.Instance.ShowTip($"中毒 -{poison}", Color.green);
        }

        // 燃烧
        int burning = GetStack(StatusType.Burning);
        if (burning > 0)
        {
            FightManager.Instance.GetPlayerHit(burning);
            UIManager.Instance.ShowTip($"燃烧 -{burning}", new Color(1f, 0.5f, 0f));
        }

        // 诅咒
        int curse = GetStack(StatusType.Curse);
        if (curse > 0)
        {
            int curseDamage = Mathf.CeilToInt(FightManager.Instance.MaxHp * curse / 100f);
            FightManager.Instance.GetPlayerHit(curseDamage);
            UIManager.Instance.ShowTip($"诅咒 -{curseDamage}", Color.magenta);
        }

        ReduceDuration();
        OnBuffChanged?.Invoke();
    }

    /// <summary>
    /// 攻击时修改伤害（力量/虚弱/魔能）
    /// </summary>
    public int ModifyAttackDamage(int baseDamage, bool includeWandCharging = true)
    {
        int modifiedDamage = baseDamage;

        // 力量
        int strength = GetStack(StatusType.Strength);
        if (strength > 0)
            modifiedDamage += strength;

        // 魔能（每层+1伤害，类似杀戮尖塔）
        int power = GetStack(StatusType.Power);
        if (power > 0)
            modifiedDamage += power;

        // 虚弱
        int weak = GetStack(StatusType.Weak);
        if (weak > 0)
            modifiedDamage = Mathf.CeilToInt(modifiedDamage * (1f - weak * 0.25f));

        return modifiedDamage;
    }

    /// <summary>
    /// 法术攻击时修改伤害（包含火亲和度）
    /// </summary>
    public int ModifySpellDamage(int baseDamage)
    {
        int modifiedDamage = ModifyAttackDamage(baseDamage);

        // 火亲和度：每层+1伤害（仅限法术攻击）
        int fireAffinity = GetStack(StatusType.FireAffinity);
        if (fireAffinity > 0)
            modifiedDamage += fireAffinity;

        return modifiedDamage;
    }

    /// <summary>
    /// 受到伤害时修改（易伤）
    /// </summary>
    public int ModifyTakenDamage(int originalDamage)
    {
        if (GetStack(StatusType.Vulnerable) > 0)
            return Mathf.CeilToInt(originalDamage * 1.25f);
        return originalDamage;
    }

    /// <summary>
    /// 攻击造成伤害后（吸血）
    /// </summary>
    public void OnDealDamage(int damage)
    {
        int lifesteal = GetStack(StatusType.Lifesteal);
        if (lifesteal > 0)
        {
            int healAmount = Mathf.Min(lifesteal, FightManager.Instance.MaxHp - FightManager.Instance.CurHp);
            if (healAmount > 0)
            {
                FightManager.Instance.CurHp += healAmount;
                UIManager.Instance.ShowTip($"吸血 +{healAmount}", Color.red);
            }
        }
    }

    /// <summary>
    /// 获得护甲时修改（锁甲/脆弱/敏捷）
    /// </summary>
    public int ModifyDefenseGain(int baseDefense)
    {
        if (HasStatus(StatusType.LockedArmor))
            return 0;

        int result = baseDefense;

        // 脆弱：每层减少25%格挡
        int frail = GetStack(StatusType.Frail);
        if (frail > 0)
            result = Mathf.CeilToInt(result * (1f - frail * 0.25f));

        // 敏捷：每层 +1 格挡
        int agility = GetStack(StatusType.Agility);
        if (agility > 0)
            result += agility;

        return Mathf.Max(result, 0);
    }

    /// <summary>
    /// 获取额外抽牌数量（集中+电亲和度）
    /// </summary>
    public int GetExtraDrawCards()
    {
        int focus = GetStack(StatusType.Focus);
        int lightningAffinity = GetStack(StatusType.LightningAffinity);
        // 电亲和度：每层+0.5，向上取整
        int lightningBonus = Mathf.CeilToInt(lightningAffinity * 0.5f);
        return focus + lightningBonus;
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

        // 每层每触发造成2点伤害
        for (int t = 0; t < times; t++)
        {
            Enemy[] allEnemies = Object.FindObjectsOfType<Enemy>();
            foreach (Enemy enemy in allEnemies)
            {
                if (enemy != null && enemy.gameObject != null && enemy.gameObject.activeInHierarchy)
                {
                    enemy.Hit(2 * polarStorm);
                }
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
        switch (type)
        {
            case StatusType.FireAffinity:
                return "火亲和";
            case StatusType.IceAffinity:
                return "冰亲和";
            case StatusType.LightningAffinity:
                return "电亲和";
            default:
                return type.ToString();
        }
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
