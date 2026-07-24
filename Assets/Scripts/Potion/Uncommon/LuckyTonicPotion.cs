/// <summary>
/// 幸运补剂 - 抵挡下1次伤害
/// </summary>
public class LuckyTonicPotion : PotionBase
{
    public override void Use()
    {
        base.Use();
        BuffManager.Instance.AddStatus(StatusType.LuckyBlock, data.effectValue);
    }
}
