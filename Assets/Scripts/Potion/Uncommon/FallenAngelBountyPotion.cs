using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 伪天使馈赏 - 手牌加1张随机攻击/技能/能力牌，本回合免费
/// </summary>
public class FallenAngelBountyPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        List<CardData> allCards = GameConfigManager.Instance.GetCardDataList();
        CardData atk = RandomPick(allCards, "攻击");
        CardData skl = RandomPick(allCards, "技能");
        CardData abi = RandomPick(allCards, "能力");

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI == null) return;

        AddAndFree(fightUI, atk);
        AddAndFree(fightUI, skl);
        AddAndFree(fightUI, abi);

        fightUI.UpdateCardItemPos();
        UIManager.Instance.ShowTip("伪天使馈赏：+1攻击 +1技能 +1能力", Color.yellow);
    }

    private CardData RandomPick(List<CardData> all, string typeName)
    {
        List<CardData> pool = new List<CardData>();
        foreach (var cd in all)
        {
            if (cd != null && cd.cardTypes != null)
                foreach (var ct in cd.cardTypes)
                    if (ct != null && ct.typeName == typeName)
                    { pool.Add(cd); break; }
        }
        if (pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }

    private void AddAndFree(FightUI fightUI, CardData cardData)
    {
        if (cardData == null) return;
        fightUI.AddCardToHand(cardData);
        var list = fightUI.GetCardItemList();
        if (list.Count > 0)
        {
            CardItem card = list[list.Count - 1];
            card.costOverride = 0;
            card.RefreshCostDisplay();
        }
    }
}
