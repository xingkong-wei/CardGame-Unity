using UnityEngine;

/// <summary>
/// 状态效果回调注册表
/// 集中管理每种 StatusType 的行为回调，注入到 StatusEffect 实例中
/// 新增 Buff 只需在此注册，无需修改 BuffManager
/// </summary>
public static class StatusCallbacks
{
    /// <summary>
    /// 为 StatusEffect 实例注入对应类型的行为回调
    /// </summary>
    public static void Inject(StatusEffect effect)
    {
        switch (effect.type)
        {
            // ========== onTurnStart（回合开始时） ==========
            case StatusType.ElementalDominance:
                effect.onTurnStart = e =>
                {
                    int stack = e.stack;
                    BuffManager.Instance.AddStatus(StatusType.FireAffinity, stack);
                    BuffManager.Instance.AddStatus(StatusType.IceAffinity, stack);
                    BuffManager.Instance.AddStatus(StatusType.LightningAffinity, stack);
                    UIManager.Instance.ShowTip($"火、冰、电亲和 +{stack}", Color.yellow);
                };
                break;

            case StatusType.SpellResonance:
                effect.onTurnStart = e =>
                {
                    StatusType maxAffinity = BuffManager.Instance.GetRandomTopAffinity();
                    BuffManager.Instance.AddStatus(maxAffinity, e.stack);
                    UIManager.Instance.ShowTip($"法术共鸣 +{e.stack} {BuffManager.Instance.GetAffinityName(maxAffinity)}", Color.yellow);
                };
                break;

            case StatusType.SovereignGlare:
                effect.onTurnStart = e =>
                {
                    BuffManager.Instance.AddStatus(StatusType.Strength, e.stack);
                };
                break;

            case StatusType.GuardianElixir:
                effect.onTurnStart = e =>
                {
                    FightManager.Instance.DefenseCount += 10;
                    FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
                    fightUI?.UpdateDefense();
                    BuffManager.Instance.RemoveStatus(StatusType.GuardianElixir, 99);
                };
                break;

            case StatusType.Rage:
                effect.onTurnStart = e =>
                {
                    BuffManager.Instance.AddStatus(StatusType.Strength, e.stack, 1);
                };
                break;

            case StatusType.PlatedArmor:
                effect.onTurnStart = e =>
                {
                    BuffManager.Instance.RemoveStatus(StatusType.PlatedArmor, 1);
                };
                effect.onPlayerTurnEnd = e =>
                {
                    FightManager.Instance.DefenseCount += e.stack;
                    FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
                    fightUI?.UpdateDefense();
                };
                break;

            case StatusType.HolyTinct:
                effect.onTurnStart = e =>
                {
                    FightManager.Instance.CurPowerCount += 1;
                    FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
                    fightUI?.UpdatePower();
                    BuffManager.Instance.RemoveStatus(StatusType.HolyTinct, 1);
                };
                break;

            case StatusType.DawnCondensedDew:
                effect.onTurnStart = e =>
                {
                    FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
                    if (fightUI != null)
                    {
                        fightUI.CreateCardItem(1);
                        fightUI.UpdateCardItemPos();
                        fightUI.UpdateCardCount();
                    }
                    BuffManager.Instance.RemoveStatus(StatusType.DawnCondensedDew, 1);
                };
                break;

            case StatusType.Regeneration:
                effect.onTurnStart = e =>
                {
                    if (FightManager.Instance.CurHp <= 0) return;
                    int healAmount = Mathf.Min(e.stack, FightManager.Instance.MaxHp - FightManager.Instance.CurHp);
                    if (healAmount > 0)
                    {
                        FightManager.Instance.CurHp += healAmount;
                        UIManager.Instance.ShowTip($"再生 +{healAmount}", Color.green);
                    }
                    BuffManager.Instance.RemoveStatus(StatusType.Regeneration, 1);
                    FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
                    fightUI?.UpdateHp();
                };
                break;

            // ========== onPlayerTurnEnd（玩家回合结束时） ==========
            case StatusType.Shrink:
                effect.onPlayerTurnEnd = e =>
                {
                    BuffManager.Instance.RemoveStatus(StatusType.Shrink, 1);
                };
                effect.modifyAttackDamage = (e, dmg) => Mathf.CeilToInt(dmg * 0.7f);
                // 敌人端：缩小在敌人回合结束时递减
                effect.onEnemyTurnEnd = (e, enemy) =>
                {
                    enemy.RemoveStatus(StatusType.Shrink, 1);
                };
                break;

            // ========== onTurnEnd（完整回合结束时） ==========
            case StatusType.Duplicate:
                effect.onTurnEnd = e =>
                {
                    BuffManager.Instance.RemoveStatus(StatusType.Duplicate, 99);
                };
                break;

            case StatusType.Vulnerable:
                effect.onTurnStart = e =>
                {
                    BuffManager.Instance.RemoveStatus(StatusType.Vulnerable, 1);
                };
                effect.modifyTakenDamage = (e, dmg) => Mathf.CeilToInt(dmg * 1.25f);
                // 敌人端：易伤在敌人回合开始时递减（敌人回合在玩家回合之后）
                effect.onEnemyTurnEnd = (e, enemy) =>
                {
                    enemy.RemoveStatus(StatusType.Vulnerable, 1);
                };
                break;

            case StatusType.Weak:
                effect.onPlayerTurnEnd = e =>
                {
                    BuffManager.Instance.RemoveStatus(StatusType.Weak, 1);
                };
                effect.modifyAttackDamage = (e, dmg) => Mathf.CeilToInt(dmg * 0.75f); // 固定减少25%伤害
                // 敌人端：虚弱在敌人回合结束时递减
                effect.onEnemyTurnEnd = (e, enemy) =>
                {
                    enemy.RemoveStatus(StatusType.Weak, 1);
                };
                break;

            case StatusType.Metallicize:
                effect.onTurnEnd = e =>
                {
                    int stack = e.stack;
                    FightManager.Instance.DefenseCount += stack;
                    UIManager.Instance.ShowTip($"金属化 +{stack}", Color.cyan);
                };
                break;

            case StatusType.Bleeding:
                effect.onTurnEnd = e =>
                {
                    int stack = e.stack;
                    FightManager.Instance.GetPlayerHit(stack);
                    UIManager.Instance.ShowTip($"流血 -{stack}", Color.red);
                };
                break;

            case StatusType.Poison:
                effect.onTurnEnd = e =>
                {
                    int stack = e.stack;
                    FightManager.Instance.GetPlayerHit(stack);
                    UIManager.Instance.ShowTip($"中毒 -{stack}", Color.green);
                };
                break;

            case StatusType.Burning:
                effect.onTurnEnd = e =>
                {
                    int stack = e.stack;
                    FightManager.Instance.GetPlayerHit(stack);
                    UIManager.Instance.ShowTip($"燃烧 -{stack}", new Color(1f, 0.5f, 0f));
                };
                break;

            case StatusType.Curse:
                effect.onTurnEnd = e =>
                {
                    int curseDamage = Mathf.CeilToInt(FightManager.Instance.MaxHp * e.stack / 100f);
                    FightManager.Instance.GetPlayerHit(curseDamage);
                    UIManager.Instance.ShowTip($"诅咒 -{curseDamage}", Color.magenta);
                };
                break;

            // ========== modifyAttackDamage（攻击伤害修正） ==========
            case StatusType.Strength:
                effect.modifyAttackDamage = (e, dmg) => dmg + e.stack;
                break;

            case StatusType.Power:
                effect.modifyAttackDamage = (e, dmg) => dmg + e.stack;
                break;

            // ========== modifySpellDamage（法术伤害额外修正） ==========
            case StatusType.FireAffinity:
                effect.modifySpellDamage = (e, dmg) => dmg + e.stack;
                break;

            // ========== modifyDefenseGain（护甲获得修正） ==========
            case StatusType.LockedArmor:
                effect.modifyDefenseGain = (e, dmg) => 0;
                break;

            case StatusType.Frail:
                effect.onPlayerTurnEnd = e =>
                {
                    BuffManager.Instance.RemoveStatus(StatusType.Frail, 1);
                };
                effect.modifyDefenseGain = (e, dmg) => Mathf.CeilToInt(dmg * 0.75f); // 固定减少25%护甲获得
                break;

            case StatusType.Agility:
                effect.modifyDefenseGain = (e, dmg) => dmg + e.stack;
                break;

            // ========== onDealDamage（造成伤害后） ==========
            case StatusType.Lifesteal:
                effect.onDealDamage = (e, damage) =>
                {
                    int healAmount = Mathf.Min(e.stack, FightManager.Instance.MaxHp - FightManager.Instance.CurHp);
                    if (healAmount > 0)
                    {
                        FightManager.Instance.CurHp += healAmount;
                        UIManager.Instance.ShowTip($"吸血 +{healAmount}", Color.red);
                    }
                };
                break;

            // ========== getExtraDrawCards（额外抽牌） ==========
            case StatusType.Focus:
                effect.getExtraDrawCards = e => e.stack;
                break;

            case StatusType.LightningAffinity:
                effect.getExtraDrawCards = e => Mathf.CeilToInt(e.stack * 0.5f);
                break;

            // ========== modifyAffinityGain（亲和度获得修正） ==========
            case StatusType.AffinityDouble:
                effect.modifyAffinityGain = (e, stack) => stack * Mathf.RoundToInt(Mathf.Pow(2, e.stack));
                break;

            case StatusType.GrandMageRobe:
                // 由 BuffManager.OnSpellCardUsed 遍历触发
                break;

            // ========== 冰亲和度：玩家回合结束时提供护盾 ==========
            case StatusType.IceAffinity:
                effect.onPlayerTurnEnd = e =>
                {
                    FightManager.Instance.DefenseCount += e.stack;
                    FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
                    fightUI?.UpdateDefense();
                };
                break;

            // ========== 安绪浆液：玩家回合结束时消耗一层（保留手牌） ==========
            case StatusType.RetainHand:
                effect.onPlayerTurnEnd = e =>
                {
                    BuffManager.Instance.RemoveStatus(StatusType.RetainHand, 1);
                };
                break;

            // ========== 敌人端状态（敌人回合结束时触发） ==========
            case StatusType.Fetter:
                effect.modifyAttackDamage = (e, dmg) => Mathf.Max(0, dmg - e.stack);
                effect.onEnemyTurnEnd = (e, enemy) =>
                {
                    enemy.RemoveStatus(StatusType.Fetter, 99);
                };
                break;

            case StatusType.VoidDust:
                effect.onEnemyTurnEnd = (e, enemy) =>
                {
                    if (enemy.CurHp > 0)
                        enemy.Hit(9 * e.stack);
                };
                break;

            // ========== Dizzy：每回合只能打2张牌，回合结束减1层 ==========
            case StatusType.Dizzy:
                effect.onPlayerTurnEnd = e =>
                {
                    BuffManager.Instance.RemoveStatus(StatusType.Dizzy, 1);
                };
                break;

            // ========== Fear：攻击伤害-6，攻击后减1层 ==========
            case StatusType.Fear:
                effect.modifyAttackDamage = (e, dmg) => Mathf.Max(0, dmg - 6);
                break;

            // ========== Scorch：每层回合结束时受到1点伤害，层数不减少 ==========
            case StatusType.Scorch:
                effect.onPlayerTurnEnd = e =>
                {
                    FightManager.Instance.GetPlayerHit(e.stack);
                    UIManager.Instance.ShowTip($"灼烧 -{e.stack}", new Color(1f, 0.4f, 0f));
                };
                break;
        }
    }
}
