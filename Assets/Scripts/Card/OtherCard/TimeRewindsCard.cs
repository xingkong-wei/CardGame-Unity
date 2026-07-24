using UnityEngine;
using System.Collections.Generic;

// 时间回溯（技能牌）- 将弃牌堆随机一张牌置入手牌，该牌本回合费用为0
public class TimeRewindsCard : CardItem
{
    /// <summary>
    /// 使用条件：弃牌堆必须有牌
    /// </summary>
    protected override bool CanUseCondition()
    {
        if (FightCardManager.Instance.usedCardList.Count == 0)
        {
            UIManager.Instance.ShowTip("弃牌堆没有牌", Color.red);
            return false;
        }
        return true;
    }

    protected override void OnCardUsed()
    {
        base.OnCardUsed();

        // 收集可抽取的卡牌（排除自身）
        List<DeckCard> pickableCards = new List<DeckCard>();
        foreach (DeckCard dc in FightCardManager.Instance.usedCardList)
        {
            if (dc.cardData != this.data)
                pickableCards.Add(dc);
        }

        if (pickableCards.Count == 0)
        {
            UIManager.Instance.ShowTip("弃牌堆只有时间回溯本身", Color.red);
            return;
        }

        int randomIndex = Random.Range(0, pickableCards.Count);
        DeckCard picked = pickableCards[randomIndex];
        FightCardManager.Instance.usedCardList.Remove(picked);

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.AddCardToHand(picked.cardData, picked);

            var cardItemList = fightUI.GetCardItemList();
            if (cardItemList.Count > 0)
            {
                CardItem newCard = cardItemList[cardItemList.Count - 1];
                newCard.costOverride = 0;
            }

            fightUI.UpdateCardItemPos();
            fightUI.UpdateUsedCardCount();
        }

        // 显示提示
        UIManager.Instance.ShowTip("时间回溯！", Color.cyan);

        // 播放特效（与防御牌一致）
        Vector3 pos = Camera.main.transform.position;
        pos.y = 0;
        PlayEffect(pos);
    }
}
