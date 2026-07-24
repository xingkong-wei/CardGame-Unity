/// <summary>
/// 冰霜护符 - 战斗开始时获得1层冰亲和度
/// </summary>
public class FrostTalisman : RelicBase
{
    public override void OnBattleStart()
    {
        BuffManager.Instance.AddStatus(StatusType.IceAffinity, 1);
    }
}
