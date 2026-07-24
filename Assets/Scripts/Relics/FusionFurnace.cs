/// <summary>
/// 融合炉 - 消耗元素亲和度时，保留消耗量50%（向下取整）层数
/// </summary>
public class FusionFurnace : RelicBase
{
    private const float RETAIN_RATIO = 0.5f;

    public override void OnAffinityConsumed(StatusType type, int amount)
    {
        int retain = amount / 2; // 向下取整
        if (retain > 0)
            BuffManager.Instance.AddStatus(type, retain);
    }
}
