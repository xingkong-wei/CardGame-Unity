using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//玩家回合
public class Fight_PlayerTurn : FightUnit
{
    public override void Init()
    {
        //禁用结束回合按钮，防止竞态条件
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.DisableTurnButton();
        }
        
        UIManager.Instance.ShowTip("玩家回合", new Color(0.1f, 0.5f, 0.1f), delegate ()
        {
            //回复行动力
            FightManager.Instance.CurPowerCount = GameConfig.Instance.maxPowerCount;

            // 回合开始触发Buff效果（护甲强化、再生等）——在能量重置之后，确保Buff加的能量不被覆盖
            BuffManager.Instance.OnTurnStart();
            // 回合开始触发遗物效果
            RelicManager.Instance.TriggerTurnStart();
            if (fightUI != null)
            {
                fightUI.UpdatePower();
                fightUI.UpdateDefense(); // 更新护甲显示
                //启用结束回合按钮
                fightUI.EnableTurnButton();
            }
            else
            {
                return;
            }

            //如果已经没有卡（且手牌为空），重新初始化
            if(FightCardManager.Instance.HasCard() == false && fightUI.GetCardItemList().Count == 0)
            {
                FightCardManager.Instance.Init();

                //更新弃牌堆数量
                fightUI.UpdateUsedCardCount();
            }

            //抽牌（安绪浆液保留手牌时，按上限补齐）
            int drawCount = GameConfig.Instance.drawCardsPerTurn + BuffManager.Instance.GetExtraDrawCards();
            int currentHandSize = fightUI.GetCardItemList().Count;
            int maxHand = GameConfig.Instance.maxHandSize;
            int actualDraw = Mathf.Max(0, Mathf.Min(drawCount, maxHand - currentHandSize));
            if (actualDraw > 0)
                fightUI.CreateCardItem(actualDraw);
            fightUI.UpdateCardItemPos();

            //更新卡牌数量
            fightUI.UpdateCardCount();
        });
    }

    public override void OnUpdate()
    {
        
    }
}
