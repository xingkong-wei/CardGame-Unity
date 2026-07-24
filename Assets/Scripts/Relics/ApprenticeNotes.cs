/// <summary>
/// 学徒笔记 - 获得元素亲和度时额外+1，每场战斗最多5次
/// </summary>
public class ApprenticeNotes : RelicBase
{
    private int triggerCount;

    public override void OnBattleStart()
    {
        triggerCount = 0;
    }

    public override int ModifyAffinityGain(StatusType type, int stack)
    {
        if (triggerCount >= 5) return stack;
        triggerCount++;
        return stack + 1;
    }
}
