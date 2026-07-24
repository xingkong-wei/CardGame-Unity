using UnityEngine;

// 火球术 - 法术攻击卡，造成伤害并获得火亲和度
public class FireMagicCard : SpellAttackCard
{
    protected override void OnCardUsed()
    {
        base.OnCardUsed();

        // 获得2点火亲和度
        BuffManager.Instance.AddStatus(StatusType.FireAffinity, 2);
        UIManager.Instance.ShowTip("获得2点火亲和度", Color.red);
    }

    /// <summary>
    /// 复制时第二段攻击：用第一段获得的2点火亲和度+2伤害
    /// </summary>
    public override int GetDuplicateSecondHitBonus()
    {
        return 2;
    }
}
