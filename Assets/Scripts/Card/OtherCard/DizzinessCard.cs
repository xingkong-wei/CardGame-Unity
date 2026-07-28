using UnityEngine;

/// <summary>
/// 眩晕 - 虚无状态卡牌，无法被打出
/// 回合结束时若仍在手上，自动消耗（虚无）
/// </summary>
public class DizzinessCard : CardItem
{
    protected override bool CanUseCondition()
    {
        UIManager.Instance.ShowTip("眩晕无法打出", Color.red);
        return false;
    }

    protected override void OnCardUsed()
    {
        // 不会执行到这里（CanUseCondition 始终返回 false）
        base.OnCardUsed();
    }
}
