using Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//胜利
public class Fight_Win : FightUnit
{
    public override void Init()
    {
        AudioManager.Instance.PlayBGM("Win");

        Map.NodeType nodeType = FightManager.Instance.GetCurrentNodeType();
        bool isElite = nodeType == Map.NodeType.EliteEnemy;
        bool isBoss = nodeType == Map.NodeType.Boss;

        // 战斗结算：判定药水掉落
        PotionData droppedPotion = PotionDropManager.RollDrop(isElite);

        // 战斗结算：判定遗物掉落
        RelicData droppedRelic = null;
        if (isElite)
            droppedRelic = PotionDropManager.RollEliteRelic();
        else if (isBoss)
            droppedRelic = PotionDropManager.RollBossRelic();
        // 小怪节点：droppedRelic 保持 null

        // 打开奖励界面
        SelectCardUI selectCard = UIManager.Instance.ShowUI<SelectCardUI>("SelectCardUI") as SelectCardUI;
        if (selectCard != null)
        {
            selectCard.SetDroppedPotion(droppedPotion);
            selectCard.SetDroppedRelic(droppedRelic);
        }

        // 订阅奖励界面关闭事件
        SelectCardUI.OnClosed += OnRewardSelected;
    }

    private void OnRewardSelected()
    {
        // 触发战斗胜利事件，更新节点状态
        Vector2Int nodePoint = FightManager.Instance.GetCurrentNodePoint();
        GameEvents.OnBattleVictory?.Invoke(nodePoint);
        SelectCardUI.OnClosed -= OnRewardSelected;

        // 清理战斗界面手牌
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.ClearAllCards();
        }

        // 隐藏战斗界面（不销毁，保留计时器）
        UIManager.Instance.HideUI("FightUI");

        // 解锁地图，允许继续点击节点
        if (MapPlayerTracker.Instance != null)
        {
            MapPlayerTracker.Instance.Locked = false;
        }

        // 重新显示节点地图（SlayTheSpireMapUI）
        SlayTheSpireMapUI nodeMapUI = UIManager.Instance.GetUI<SlayTheSpireMapUI>("SlayTheSpireMapUI");
        if (nodeMapUI != null)
        {
            // 隐藏关闭按钮（战斗胜利后）
            nodeMapUI.SetHideCloseBtn(true);
            nodeMapUI.Show();
        }
        else
        {
            // 节点地图不存在，返回MapUI
            MapUI mapUI = UIManager.Instance.GetUI<MapUI>("MapUI");
            if (mapUI != null)
            {
                mapUI.Show();
            }
            else
            {
                // MapUI也不存在，创建新的MapUI
                UIManager.Instance.ShowUI<MapUI>("MapUI");
            }
        }
    }
}
