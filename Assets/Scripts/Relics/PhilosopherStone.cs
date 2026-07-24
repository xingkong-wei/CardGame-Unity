using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 贤者之石 - 回合开始+1能量，回合结束随机减少1种有层数的元素亲和度2层
/// </summary>
public class PhilosopherStone : RelicBase
{
    public override void OnTurnStart()
    {
        FightManager.Instance.CurPowerCount += 1;
    }

    public override void OnTurnEnd()
    {
        var candidates = new List<StatusType>();
        if (BuffManager.Instance.GetStack(StatusType.FireAffinity) > 0)
            candidates.Add(StatusType.FireAffinity);
        if (BuffManager.Instance.GetStack(StatusType.IceAffinity) > 0)
            candidates.Add(StatusType.IceAffinity);
        if (BuffManager.Instance.GetStack(StatusType.LightningAffinity) > 0)
            candidates.Add(StatusType.LightningAffinity);

        if (candidates.Count == 0) return;

        StatusType type = candidates[Random.Range(0, candidates.Count)];
        BuffManager.Instance.RemoveStatus(type, 2);
    }
}
