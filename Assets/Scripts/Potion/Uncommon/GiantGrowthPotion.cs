using UnityEngine;

/// <summary>
/// 超巨化药水 - 下一张攻击牌造成3倍伤害（可跨回合保留）
/// </summary>
public class GiantGrowthPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        BuffManager.Instance.AddStatus(StatusType.GiantGrowth, 1);
        UIManager.Instance.ShowTip("超巨化：下1张攻击牌3倍伤害", Color.yellow);
    }
}
