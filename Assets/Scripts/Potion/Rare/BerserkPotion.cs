using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 狂躁之水 - 选择手牌中的一张牌，本场战斗免费打出
/// </summary>
public class BerserkPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI == null) return;

        List<CardItem> handCards = fightUI.GetCardItemList();
        if (handCards.Count == 0)
        {
            UIManager.Instance.ShowTip("手牌为空", Color.yellow);
            return;
        }

        // 收集手牌 CardData
        List<CardData> cardDatas = new List<CardData>();
        foreach (var card in handCards)
        {
            if (card != null && card.data != null)
                cardDatas.Add(card.data);
        }

        CardCollectionUI.ShowCardList(cardDatas, "选择一张牌", true, (CardData selected) =>
        {
            if (selected == null) return;

            // 找到对应的手牌并标记免费
            foreach (var card in handCards)
            {
                if (card != null && card.sourceDeckCard != null && card.data == selected)
                {
                    FightCardManager.Instance.MarkFreeCard(card.sourceDeckCard.instanceId);
                    card.costOverride = 0;
                    card.RefreshCostDisplay();
                    UIManager.Instance.ShowTip("狂躁之水：免费打出", Color.red);
                    break;
                }
            }
        });
    }
}
