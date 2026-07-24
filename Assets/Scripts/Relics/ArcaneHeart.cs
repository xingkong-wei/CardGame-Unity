/// <summary>
/// 奥术之心 - 所有法术牌费用-1，但每回合最多获得合计4层元素亲和度
/// </summary>
public class ArcaneHeart : RelicBase
{
    private const int MAX_PER_TURN = 4;

    private int gainedThisTurn;

    public override bool HasSpellCostReduction() { return true; }

    public override void OnTurnStart()
    {
        gainedThisTurn = 0;
    }

    public override int ModifyAffinityGain(StatusType type, int stack)
    {
        int remaining = MAX_PER_TURN - gainedThisTurn;
        if (remaining <= 0) return 0;

        int actual = stack;
        if (actual > remaining) actual = remaining;

        gainedThisTurn += actual;
        return actual;
    }
}
