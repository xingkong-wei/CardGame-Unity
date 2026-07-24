/// <summary>
/// 力量药水 - 获得力量
/// 使用 BuffManager 为玩家添加力量状态
/// </summary>
public class PowerPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        // 添加力量：effectValue = 力量层数, duration = -1 表示永久（整场战斗有效）
        int amount = data.effectValue;
        int duration = data.duration == PotionData.DURATION_INSTANT
            ? PotionData.DURATION_PERMANENT  // 一次性药水 → 永久Buff
            : data.duration;

        BuffManager.Instance.AddStatus(StatusType.Strength, amount, duration);
    }
}
