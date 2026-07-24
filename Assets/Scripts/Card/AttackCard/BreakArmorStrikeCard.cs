using UnityEngine;
using UnityEngine.EventSystems;

// 裂甲冲撞 - 将格挡转化为伤害，并消耗格挡（基础50%，升级25%）
public class BreakArmorStrikeCard : AttackCardItem
{
    protected override int GetAttackDamage()
    {
        return FightManager.Instance.DefenseCount;
    }

    protected override void OnCardUsed()
    {
        // 获取当前格挡值作为伤害
        int damage = FightManager.Instance.DefenseCount;

        // 扣除格挡（基础50%，升级后25%）
        float reduceRatio = IsUpgraded() ? 0.25f : 0.5f;
        int reduceAmount = Mathf.FloorToInt(damage * reduceRatio);
        FightManager.Instance.DefenseCount -= reduceAmount;

        // 更新UI显示
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.UpdateDefense();
        }

        // base.OnCardUsed() 会处理卡牌移除
        base.OnCardUsed();
    }

    /// <summary>
    /// 复制时第二段攻击：格挡已被扣除，伤害减少
    /// </summary>
    public override int GetDuplicateSecondHitBonus()
    {
        float reduceRatio = IsUpgraded() ? 0.25f : 0.5f;
        int reduceAmount = Mathf.FloorToInt(GetAttackDamage() * reduceRatio);
        return -reduceAmount;
    }
}
