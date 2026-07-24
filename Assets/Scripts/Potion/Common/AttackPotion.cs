using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 攻击药水 - 从 3 张随机攻击牌中选择 1 张加入手牌，本回合免费打出
/// </summary>
public class AttackPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        // 1. 筛选所有攻击牌
        List<CardData> allCards = GameConfigManager.Instance.GetCardDataList();
        List<CardData> attackCards = new List<CardData>();

        foreach (var card in allCards)
        {
            if (IsAttackCard(card))
                attackCards.Add(card);
        }

        if (attackCards.Count == 0)
        {
            Debug.LogWarning("[AttackPotion] 没有找到任何攻击牌！");
            return;
        }

        // 2. 随机选 3 张（不足 3 张则全选）
        int count = Mathf.Min(3, attackCards.Count);
        List<CardData> choices = new List<CardData>();
        List<CardData> pool = new List<CardData>(attackCards);

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            choices.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        // 3. 显示选择 UI
        RewardInterfaceUI.ShowCustomReward(choices, OnAttackCardPicked);
    }

    /// <summary>
    /// 玩家选择了攻击牌 → 加入手牌，本回合费用设为 0
    /// </summary>
    private void OnAttackCardPicked(CardData cardData)
    {
        if (cardData == null) return;

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI == null) return;

        // 加入手牌
        fightUI.AddCardToHand(cardData);
        fightUI.UpdateCardItemPos();

        // 标记本回合费用为 0
        var cardItemList = fightUI.GetCardItemList();
        if (cardItemList.Count > 0)
        {
            CardItem newCard = cardItemList[cardItemList.Count - 1];
            newCard.costOverride = 0;
            newCard.RefreshCostDisplay();
        }
    }

    /// <summary>
    /// 判断是否为攻击牌（卡牌类型中包含"攻击"）
    /// </summary>
    private bool IsAttackCard(CardData cardData)
    {
        if (cardData.cardTypes == null) return false;
        foreach (var ct in cardData.cardTypes)
        {
            if (ct != null && ct.typeName.Contains("攻击"))
                return true;
        }
        return false;
    }
}
