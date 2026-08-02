using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 枯雾诡水 - 消耗手牌中所有牌（放入废牌堆，无法被抽到）
/// </summary>
public class WitheredMistVenomPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI == null) return;

        List<CardItem> handCards = new List<CardItem>(fightUI.GetCardItemList());

        if (handCards.Count == 0)
        {
            UIManager.Instance.ShowTip("手牌为空", Color.yellow);
            return;
        }

        foreach (CardItem card in handCards)
        {
            if (card == null || card.data == null) continue;

            // 加入废牌堆（仅本场战斗，不永久移除）
            DeckCard dc = card.sourceDeckCard ?? new DeckCard(card.data);
            FightCardManager.Instance.consumeCardList.Add(dc);
            // 归还卡牌到对象池
            card.enabled = false;
            PoolManager.Release("CardItem", card.gameObject);
        }

        fightUI.GetCardItemList().Clear();
        fightUI.UpdateCardCount();
        fightUI.UpdateConsumeCardCount();
        fightUI.UpdateCardItemPos();

        UIManager.Instance.ShowTip($"枯雾诡水：消耗 {handCards.Count} 张牌", Color.magenta);
    }
}
