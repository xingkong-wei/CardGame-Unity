/// <summary>
/// 契约书 - 元素亲和度叠加速度翻倍，但每回合开始时失去5点生命
/// </summary>
public class PactCodex : RelicBase
{
    public override int ModifyAffinityGain(StatusType type, int stack)
    {
        return stack * 2;
    }

    public override void OnTurnStart()
    {
        FightManager.Instance.GetPlayerHit(5);
    }
}
