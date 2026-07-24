using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 赌徒佳酿 - 丢弃任意张手牌，抽取相同数量的牌
/// </summary>
public class GamblerBrewPotion : PotionBase
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

        List<CardData> cardDatas = new List<CardData>();
        foreach (var c in handCards)
            if (c != null && c.data != null)
                cardDatas.Add(c.data);

        CardCollectionUI.ShowMultiSelectCardList(cardDatas, "选择丢弃", (List<CardData> selected) =>
        {
            if (selected == null || selected.Count == 0) return;

            int count = selected.Count;

            // 逐张丢弃（避开RemoveCard的内部UpdateCardItemPos干扰）
            foreach (CardData cd in selected)
            {
                for (int i = handCards.Count - 1; i >= 0; i--)
                {
                    if (handCards[i] != null && handCards[i].data == cd)
                    {
                        CardItem card = handCards[i];
                        DeckCard dc = card.sourceDeckCard ?? new DeckCard(card.data);
                        FightCardManager.Instance.usedCardList.Add(dc);
                        handCards.RemoveAt(i);
                        card.enabled = false;
                        Object.Destroy(card.gameObject);
                        break;
                    }
                }
            }

            // 统一刷新布局 → 抽等量牌 → 再刷新
            fightUI.UpdateCardItemPos();
            fightUI.CreateCardItem(count);
            fightUI.UpdateCardItemPos();
            fightUI.UpdateCardCount();
            fightUI.UpdateUsedCardCount();

            UIManager.Instance.ShowTip($"弃{count}张，抽{count}张", Color.yellow);
        });
    }
}
