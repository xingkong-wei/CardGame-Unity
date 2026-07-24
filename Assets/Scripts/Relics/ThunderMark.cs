/// <summary>
/// 雷光微记 - 战斗开始时获得1层雷亲和度
/// </summary>
public class ThunderMark : RelicBase
{
    public override void OnBattleStart()
    {
        BuffManager.Instance.AddStatus(StatusType.LightningAffinity, 1);
    }
}
