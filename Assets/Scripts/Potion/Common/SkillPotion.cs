using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 技能药水 - 从 3 张随机技能牌中选择 1 张加入手牌，本回合免费打出
/// </summary>
public class SkillPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        // 1. 筛选所有技能牌
        List<CardData> allCards = GameConfigManager.Instance.GetCardDataList();
        List<CardData> skillCards = new List<CardData>();

        foreach (var card in allCards)
        {
            if (IsSkillCard(card))
                skillCards.Add(card);
        }

        if (skillCards.Count == 0)
        {
            Debug.LogWarning("[SkillPotion] 没有找到任何技能牌！");
            return;
        }

        // 2. 随机选 3 张（不足 3 张则全选）
        int count = Mathf.Min(3, skillCards.Count);
        List<CardData> choices = new List<CardData>();
        List<CardData> pool = new List<CardData>(skillCards);

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            choices.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        // 3. 显示选择 UI
        RewardInterfaceUI.ShowCustomReward(choices, OnSkillCardPicked);
    }

    /// <summary>
    /// 玩家选择了技能牌 → 加入手牌，本回合费用设为 0
    /// </summary>
    private void OnSkillCardPicked(CardData cardData)
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
    /// 判断是否为技能牌（卡牌类型中包含"技能"）
    /// </summary>
    private bool IsSkillCard(CardData cardData)
    {
        if (cardData.cardTypes == null) return false;
        foreach (var ct in cardData.cardTypes)
        {
            if (ct != null && ct.typeName.Contains("技能"))
                return true;
        }
        return false;
    }
}
