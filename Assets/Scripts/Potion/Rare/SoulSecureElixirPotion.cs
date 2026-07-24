/// <summary>
/// 固心汤剂 - 获得7层覆甲，每回合减少1层
/// </summary>
public class SoulSecureElixirPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        int amount = data.effectValue;
        BuffManager.Instance.AddStatus(StatusType.PlatedArmor, amount);
    }
}
