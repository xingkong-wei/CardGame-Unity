using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//战斗卡牌管理器
public class FightCardManager
{
    public static FightCardManager Instance = new FightCardManager();

    public List<DeckCard> cardList;//卡堆集合
    public List<DeckCard> usedCardList;//弃牌堆集合（可洗回抽牌堆）
    public List<DeckCard> consumeCardList;//废牌堆集合（消耗卡，使用后无法再次使用）

    /// <summary>
    /// 本场战斗已被消耗的卡牌实例集合
    /// </summary>
    private HashSet<int> consumedInstanceIds = new HashSet<int>();
    private HashSet<int> usedAbilityInstanceIds = new HashSet<int>();
    private HashSet<int> tempUpgradedInstanceIds = new HashSet<int>();

    public void MarkCardAsConsumed(int instanceId) => consumedInstanceIds.Add(instanceId);
    public bool IsCardConsumed(int instanceId) => consumedInstanceIds.Contains(instanceId);
    public void MarkAbilityAsUsed(int instanceId) => usedAbilityInstanceIds.Add(instanceId);
    public bool IsAbilityUsed(int instanceId) => usedAbilityInstanceIds.Contains(instanceId);
    public void ResetConsumedCards() => consumedInstanceIds.Clear();
    public void ResetUsedAbilityCards() => usedAbilityInstanceIds.Clear();

    /// <summary>临时升级（本场战斗有效）</summary>
    public void MarkTempUpgraded(int instanceId) => tempUpgradedInstanceIds.Add(instanceId);
    public bool IsTempUpgraded(int instanceId) => tempUpgradedInstanceIds.Contains(instanceId);
    public void ResetTempUpgraded() => tempUpgradedInstanceIds.Clear();

    private HashSet<int> freeCardInstanceIds = new HashSet<int>();
    public void MarkFreeCard(int instanceId) => freeCardInstanceIds.Add(instanceId);
    public bool IsFreeCard(int instanceId) => freeCardInstanceIds.Contains(instanceId);
    public void ResetFreeCards() => freeCardInstanceIds.Clear();

    public void ResetForNewGame()
    {
        cardList = new List<DeckCard>();
        usedCardList = new List<DeckCard>();
        consumeCardList = new List<DeckCard>();
        consumedInstanceIds.Clear();
        usedAbilityInstanceIds.Clear();
    }

    //初始化
    public void Init()
    {
        if (cardList == null) cardList = new List<DeckCard>();
        else cardList.Clear();
        if (usedCardList == null) usedCardList = new List<DeckCard>();
        else usedCardList.Clear();
        if (consumeCardList == null) consumeCardList = new List<DeckCard>();

        List<DeckCard> tempList = new List<DeckCard>();
        foreach (var dc in RoleManager.Instance.cardList)
        {
            if (dc == null || dc.cardData == null) continue;
            if (consumedInstanceIds.Contains(dc.instanceId)) continue;
            if (usedAbilityInstanceIds.Contains(dc.instanceId)) continue;
            tempList.Add(dc);
        }

        while (tempList.Count > 0)
        {
            int idx = Random.Range(0, tempList.Count);
            cardList.Add(tempList[idx]);
            tempList.RemoveAt(idx);
        }
    }

    public bool HasCard() => cardList.Count > 0;

    public DeckCard DrawCard()
    {
        if (cardList.Count == 0) ShuffleDiscardToDraw();
        if (cardList.Count == 0) return null;
        DeckCard dc = cardList[cardList.Count - 1];
        cardList.RemoveAt(cardList.Count - 1);
        return dc;
    }

    public void ShuffleDiscardToDraw()
    {
        if (usedCardList == null || usedCardList.Count == 0) return;
        if (cardList == null) cardList = new List<DeckCard>();
        cardList.AddRange(usedCardList);
        usedCardList.Clear();
        for (int i = cardList.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = cardList[i];
            cardList[i] = cardList[j];
            cardList[j] = tmp;
        }
    }
}
