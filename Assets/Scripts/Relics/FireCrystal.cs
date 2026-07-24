/// <summary>
/// 火元素结晶 - 战斗开始时获得1层火亲和度
/// </summary>
public class FireCrystal : RelicBase
{
    public override void OnBattleStart()
    {
        BuffManager.Instance.AddStatus(StatusType.FireAffinity, 1);
    }
}
