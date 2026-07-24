/// <summary>
/// 敏捷药水 - 获得敏捷
/// 使用 BuffManager 为玩家添加敏捷状态
/// </summary>
public class AgilityPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        // 添加敏捷：effectValue = 敏捷层数, duration = -1 表示永久（整场战斗有效）
        int amount = data.effectValue;
        int duration = data.duration == PotionData.DURATION_INSTANT
            ? PotionData.DURATION_PERMANENT  // 一次性药水 → 永久Buff
            : data.duration;

        BuffManager.Instance.AddStatus(StatusType.Agility, amount, duration);
    }
}
