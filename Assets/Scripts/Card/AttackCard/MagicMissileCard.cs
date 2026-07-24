using UnityEngine;

// 魔法飞弹 - 法术攻击卡，造成伤害两次（基础5，升级7）
public class MagicMissileCard : SpellAttackCard
{
    // 攻击次数
    private const int ATTACK_TIMES = 2;

    // 特效路径
    private const string EFFECT_PATH = "Effects/GreenBloodExplosion";

    private int GetPerHitDamage()
    {
        return IsUpgraded() ? 7 : 5;
    }

    protected override int GetAttackDamage()
    {
        // 应用Buff修改（包含火亲和度）
        int damage = BuffManager.Instance.ModifySpellDamage(GetPerHitDamage());
        return damage;
    }

    protected override int GetAttackTimes()
    {
        return ATTACK_TIMES;
    }

    protected override string GetAttackEffectPath()
    {
        return EFFECT_PATH;
    }
}
