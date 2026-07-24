using UnityEngine;

/// <summary>
/// 液态记忆 - 将弃牌堆中的1张牌放入手牌，本回合免费打出
/// </summary>
public class LiquidMemoryPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI == null) return;

        var usedPile = FightCardManager.Instance.usedCardList;
        if (usedPile == null || usedPile.Count == 0)
        {
            UIManager.Instance.ShowTip("弃牌堆没有牌", Color.yellow);
            return;
        }

        // 随机选1张
        int idx = Random.Range(0, usedPile.Count);
        DeckCard dc = usedPile[idx];
        usedPile.RemoveAt(idx);

        // 加入手牌，本回合免费
        fightUI.AddCardToHand(dc.cardData, dc);
        var handList = fightUI.GetCardItemList();
        CardItem card = handList[handList.Count - 1];
        card.costOverride = 0;
        card.RefreshCostDisplay();

        fightUI.UpdateCardItemPos();
        fightUI.UpdateCardCount();
    }
}
