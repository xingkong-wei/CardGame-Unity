/// <summary>
/// 君威之睨 - 每回合开始获得1层力量
/// </summary>
public class SovereignGlarePotion : PotionBase
{
    public override void Use()
    {
        base.Use();
        BuffManager.Instance.AddStatus(StatusType.SovereignGlare, data.effectValue);
    }
}
