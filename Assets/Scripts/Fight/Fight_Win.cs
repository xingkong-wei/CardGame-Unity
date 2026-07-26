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
        Vector2Int nodePoint = FightManager.Instance.GetCurrentNodePoint();
        GameEvents.OnBattleVictory?.Invoke(nodePoint);
        SelectCardUI.OnClosed -= OnRewardSelected;

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.ClearAllCards();
        }

        UIManager.Instance.HideUI("FightUI");

        Map.NodeType nodeType = FightManager.Instance.GetCurrentNodeType();
        bool isBoss = nodeType == Map.NodeType.Boss;

        if (isBoss)
        {
            // Boss 战胜利 → 直接返回 MapUI（下一岛屿已解锁）
            SlayTheSpireMapUI nodeMapUI = UIManager.Instance.GetUI<SlayTheSpireMapUI>("SlayTheSpireMapUI");
            if (nodeMapUI != null)
                nodeMapUI.Hide();

            MapUI mapUI = UIManager.Instance.GetUI<MapUI>("MapUI");
            if (mapUI != null)
                mapUI.Show();
            else
                UIManager.Instance.ShowUI<MapUI>("MapUI");
        }
        else
        {
            // 非 Boss 战 → 返回节点地图继续探索（无关闭按钮）
            SlayTheSpireMapUI nodeMapUI = UIManager.Instance.GetUI<SlayTheSpireMapUI>("SlayTheSpireMapUI");
            if (nodeMapUI != null)
            {
                nodeMapUI.ReopenAfterVictory();
            }
            else
            {
                MapUI mapUI = UIManager.Instance.GetUI<MapUI>("MapUI");
                if (mapUI != null)
                    mapUI.Show();
                else
                    UIManager.Instance.ShowUI<MapUI>("MapUI");
            }
        }
    }
}
