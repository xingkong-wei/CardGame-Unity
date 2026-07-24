/// <summary>
/// 卡牌实例包装类 — 同一卡牌模板的多张复制各自独立，支持单张升级/消耗
/// </summary>
[System.Serializable]
public class DeckCard
{
    public int instanceId;
    public CardData cardData;
    public bool upgraded;

    private static int _nextId = 1;

    public DeckCard(CardData data)
    {
        instanceId = _nextId++;
        cardData = data;
        upgraded = false;
    }
}
