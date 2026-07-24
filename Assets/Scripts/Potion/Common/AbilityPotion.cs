using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 能力药水 - 从 3 张随机能力牌中选择 1 张加入手牌，本回合免费打出
/// </summary>
public class AbilityPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        // 1. 筛选所有能力牌
        List<CardData> allCards = GameConfigManager.Instance.GetCardDataList();
        List<CardData> abilityCards = new List<CardData>();

        foreach (var card in allCards)
        {
            if (IsAbilityCard(card))
                abilityCards.Add(card);
        }

        if (abilityCards.Count == 0)
        {
            Debug.LogWarning("[AbilityPotion] 没有找到任何能力牌！");
            return;
        }

        // 2. 随机选 3 张（不足 3 张则全选）
        int count = Mathf.Min(3, abilityCards.Count);
        List<CardData> choices = new List<CardData>();
        List<CardData> pool = new List<CardData>(abilityCards);

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            choices.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        // 3. 显示选择 UI
        RewardInterfaceUI.ShowCustomReward(choices, OnAbilityCardPicked);
    }

    private void OnAbilityCardPicked(CardData cardData)
    {
        if (cardData == null) return;

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI == null) return;

        fightUI.AddCardToHand(cardData);
        fightUI.UpdateCardItemPos();

        var cardItemList = fightUI.GetCardItemList();
        if (cardItemList.Count > 0)
        {
            CardItem newCard = cardItemList[cardItemList.Count - 1];
            newCard.costOverride = 0;
            newCard.RefreshCostDisplay();
        }
    }

    private bool IsAbilityCard(CardData cardData)
    {
        if (cardData.cardTypes == null) return false;
        foreach (var ct in cardData.cardTypes)
        {
            if (ct != null && ct.typeName.Contains("能力"))
                return true;
        }
        return false;
    }
}
