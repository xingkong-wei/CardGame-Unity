using UnityEngine;

// 寒冰护甲 - 获得护盾和冰亲和度
public class IceArmorCard : DefendCard
{
    protected override void OnCardUsed()
    {
        base.OnCardUsed();

        // 获得2点冰亲和度
        BuffManager.Instance.AddStatus(StatusType.IceAffinity, 2);
        UIManager.Instance.ShowTip("获得2点冰亲和度", Color.cyan);
    }
}
