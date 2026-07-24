using UnityEngine;

// 法术攻击卡 - 攻击卡的一种，支持火亲和度加成
public class SpellAttackCard : AttackCardItem
{
    /// <summary>
    /// 获取法术攻击伤害值，包含火亲和度加成
    /// </summary>
    protected override int GetAttackDamage()
    {
        if (data == null) return 0;

        int baseValue = 0;
        switch (data.damageSource)
        {
            case DamageSourceType.Fixed:
                baseValue = GetArg0();
                break;
            case DamageSourceType.Defense:
                baseValue = FightManager.Instance.DefenseCount;
                break;
            case DamageSourceType.CurrentHp:
                baseValue = FightManager.Instance.CurHp;
                break;
            case DamageSourceType.Coin:
                baseValue = FightManager.Instance.CoinAmount;
                break;
            default:
                baseValue = GetArg0();
                break;
        }

        // 应用百分比
        int damage = Mathf.FloorToInt(baseValue * data.damagePercent);

        // 应用Buff修改（包含火亲和度）
        damage = BuffManager.Instance.ModifySpellDamage(damage);

        return damage;
    }
}
