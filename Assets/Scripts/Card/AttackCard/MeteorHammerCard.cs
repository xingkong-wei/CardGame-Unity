using UnityEngine;

// 陨锤 - 高伤害攻击卡
public class MeteorHammerCard : AttackCardItem
{
    protected override void OnCardUsed()
    {
        base.OnCardUsed();
        
        // 陨锤特有效果：可以在这里添加额外逻辑
        // 例如：播放特殊音效、添加额外伤害等
        Debug.Log("陨锤使用！造成 " + (data != null ? GetArg0() : 0) + " 点伤害");
    }
}
