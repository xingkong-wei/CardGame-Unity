/// <summary>
/// 元素护腕 - 每打出一张能力牌，获得火冰雷亲和度各1层
/// </summary>
public class ElementBracers : RelicBase
{
    public override void OnCardPlayed(CardItem card)
    {
        if (card == null || card.data == null) return;
        if (!card.data.HasCardType("能力")) return;

        BuffManager.Instance.AddStatus(StatusType.FireAffinity, 1);
        BuffManager.Instance.AddStatus(StatusType.IceAffinity, 1);
        BuffManager.Instance.AddStatus(StatusType.LightningAffinity, 1);
    }
}
