using UnityEngine;

/// <summary>
/// 元素转换器 - 回合结束时，最低层数元素转移1层给最高层数元素
/// </summary>
public class ElementConvector : RelicBase
{
    public override void OnTurnEnd()
    {
        int fire = BuffManager.Instance.GetStack(StatusType.FireAffinity);
        int ice = BuffManager.Instance.GetStack(StatusType.IceAffinity);
        int lightning = BuffManager.Instance.GetStack(StatusType.LightningAffinity);

        // 只有一种元素有层数时不转化
        int hasCount = (fire > 0 ? 1 : 0) + (ice > 0 ? 1 : 0) + (lightning > 0 ? 1 : 0);
        if (hasCount <= 1) return;

        // 找最高和最低
        int max = Mathf.Max(fire, Mathf.Max(ice, lightning));
        int min = Mathf.Min(fire > 0 ? fire : int.MaxValue,
                  Mathf.Min(ice > 0 ? ice : int.MaxValue, lightning > 0 ? lightning : int.MaxValue));

        if (max <= min) return;

        // 收集最高和最低的元素列表（处理并列）
        var high = new System.Collections.Generic.List<StatusType>();
        var low = new System.Collections.Generic.List<StatusType>();
        if (fire == max) high.Add(StatusType.FireAffinity);
        if (ice == max) high.Add(StatusType.IceAffinity);
        if (lightning == max) high.Add(StatusType.LightningAffinity);
        if (fire == min && fire > 0) low.Add(StatusType.FireAffinity);
        if (ice == min && ice > 0) low.Add(StatusType.IceAffinity);
        if (lightning == min && lightning > 0) low.Add(StatusType.LightningAffinity);

        StatusType from = low[Random.Range(0, low.Count)];
        StatusType to = high[Random.Range(0, high.Count)];

        BuffManager.Instance.RemoveStatus(from, 1);
        BuffManager.Instance.AddStatus(to, 1);
    }
}
