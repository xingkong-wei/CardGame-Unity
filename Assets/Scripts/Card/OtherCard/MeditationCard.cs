using UnityEngine;

// 冥想 - 抽2张牌，本回合下一张法术牌费用-1，如果没有打出则所有法术牌费用-1
public class MeditationCard : CardItem
{
    protected override void OnCardUsed()
    {
        base.OnCardUsed();

        // 抽2张牌
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.CreateCardItem(2);
            fightUI.UpdateCardItemPos();
            fightUI.UpdateCardCount();
        }

        int meditationCount = data != null ? GetArg0() : 1;

        // 添加冥想状态（冥想层数固定为1，只影响下一张法术牌）
        BuffManager.Instance.AddStatus(StatusType.Meditation, 1, -1);
        
        // 激活冥想效果（增加 spellCostDiscountCount 计数器）
        BuffManager.Instance.ActivateMeditation();
        
        // 为当前手牌中的法术牌显示费用减免
        RefreshAllSpellCardsCost();

        // 显示提示
        int totalCount = BuffManager.Instance.GetSpellCostDiscountCount();
        UIManager.Instance.ShowTip($"冥想：{totalCount}张法术牌费用-1", Color.cyan);
    }

    /// <summary>
    /// 刷新所有法术牌费用显示
    /// </summary>
    private void RefreshAllSpellCardsCost()
    {
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI == null) return;

        foreach (var cardItem in fightUI.GetCardItemList())
        {
            if (cardItem != null && cardItem.IsSpellCard())
            {
                cardItem.RefreshCostDisplay();
            }
        }
    }
}
