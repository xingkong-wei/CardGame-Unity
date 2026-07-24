using UnityEngine;

// 元素屏障 - 获得格挡，同时获得火、冰、雷亲和度
public class ElementalBarrierCard : DefendCard
{
    protected override void OnCardUsed()
    {
        base.OnCardUsed();

        // 获得1点火、冰、雷亲和度
        BuffManager.Instance.AddStatus(StatusType.FireAffinity, 1);
        BuffManager.Instance.AddStatus(StatusType.IceAffinity, 1);
        BuffManager.Instance.AddStatus(StatusType.LightningAffinity, 1);
        UIManager.Instance.ShowTip("获得1点火、冰、雷亲和度", Color.white);
    }
}
