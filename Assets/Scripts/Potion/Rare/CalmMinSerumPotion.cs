/// <summary>
/// 安绪浆液 - 保留手牌2回合
/// </summary>
public class CalmMinSerumPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        int duration = data.effectValue;
        BuffManager.Instance.AddStatus(StatusType.RetainHand, duration);
    }
}
