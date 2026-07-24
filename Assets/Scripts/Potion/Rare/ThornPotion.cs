/// <summary>
/// 荆棘药水 - 获得3层荆棘
/// </summary>
public class ThornPotion : PotionBase
{
    public override void Use()
    {
        base.Use();
        BuffManager.Instance.AddStatus(StatusType.Thorns, data.effectValue);
    }
}
