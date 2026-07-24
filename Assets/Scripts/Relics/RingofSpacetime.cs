using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 时空指环 - 回合内未出牌就结束回合时，重置抽牌堆并获得3层火冰雷亲和度（每场战斗限一次）
/// </summary>
public class RingofSpacetime : RelicBase
{
    private bool usedThisBattle;
    private bool playedCardThisTurn;

    public override void OnBattleStart()
    {
        usedThisBattle = false;
        playedCardThisTurn = false;
    }

    public override void OnTurnStart()
    {
        playedCardThisTurn = false;
    }

    public override void OnCardPlayed(CardItem card)
    {
        playedCardThisTurn = true;
    }

    public override void OnTurnEnd()
    {
        if (usedThisBattle || playedCardThisTurn) return;

        usedThisBattle = true;

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI == null) return;

        // 把手牌放回抽牌堆
        List<CardItem> handCards = fightUI.GetCardItemList();
        for (int i = handCards.Count - 1; i >= 0; i--)
        {
            CardItem card = handCards[i];
            if (card.sourceDeckCard != null)
                FightCardManager.Instance.cardList.Add(card.sourceDeckCard);
            Object.Destroy(card.gameObject);
        }
        handCards.Clear();

        // 弃牌堆和废牌堆洗入抽牌堆
        FightCardManager.Instance.cardList.AddRange(FightCardManager.Instance.usedCardList);
        FightCardManager.Instance.usedCardList.Clear();
        FightCardManager.Instance.cardList.AddRange(FightCardManager.Instance.consumeCardList);
        FightCardManager.Instance.consumeCardList.Clear();

        // 清除消耗标记，让消耗牌可以再次被抽到并使用
        FightCardManager.Instance.ResetConsumedCards();

        // 洗牌
        ShuffleList(FightCardManager.Instance.cardList);

        // 刷新手牌位置和抽牌数量UI
        fightUI.UpdateCardItemPos();
        fightUI.UpdateCardCount();

        // 获得3层火、冰、雷亲和度
        BuffManager.Instance.AddStatus(StatusType.FireAffinity, 3);
        BuffManager.Instance.AddStatus(StatusType.IceAffinity, 3);
        BuffManager.Instance.AddStatus(StatusType.LightningAffinity, 3);
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }
}
