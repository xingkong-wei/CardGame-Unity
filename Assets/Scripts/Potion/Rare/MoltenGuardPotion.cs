using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 熔火护佑 - 本场战斗升级当前手牌所有牌（已升级的跳过）
/// </summary>
public class MoltenGuardPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI == null) return;

        List<CardItem> handCards = fightUI.GetCardItemList();
        int count = 0;

        foreach (CardItem card in handCards)
        {
            if (card == null || card.sourceDeckCard == null) continue;
            // 已升级的跳过
            if (card.sourceDeckCard.upgraded || FightCardManager.Instance.IsTempUpgraded(card.sourceDeckCard.instanceId))
                continue;

            // 标记临时升级
            FightCardManager.Instance.MarkTempUpgraded(card.sourceDeckCard.instanceId);
            // 刷新升级显示（名称+费用+描述）
            card.RefreshUpgradeDisplay();
            count++;
        }

        if (count > 0)
            UIManager.Instance.ShowTip($"熔火护佑：升级 {count} 张牌", Color.red);
        else
            UIManager.Instance.ShowTip("没有可升级的手牌", Color.yellow);
    }
}
