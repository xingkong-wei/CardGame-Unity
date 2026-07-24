using UnityEngine;

// 雷击 - 法术攻击卡，造成伤害并获得雷亲和度
public class LightningCard : SpellAttackCard
{
    protected override void OnCardUsed()
    {
        base.OnCardUsed();

        // 获得1点雷亲和度
        BuffManager.Instance.AddStatus(StatusType.LightningAffinity, 1);
        UIManager.Instance.ShowTip("获得1点雷亲和度", Color.yellow);
    }
}
