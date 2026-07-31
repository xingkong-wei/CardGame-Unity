using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//卡牌战斗初始化
public class FightInit : FightUnit
{
    public override void Init()
    {
        //初始化战斗数值
        FightManager.Instance.Init();

        // 重置Buff管理器（新战斗开始）
        BuffManager.Instance.Reset();

        // 清除上局已用药水图标
        SpireBuffUI.ClearUsedPotions();
        
        // 重置消耗卡追踪（每场战斗开始清空，消耗卡仅限本场不可用）
        FightCardManager.Instance.ResetConsumedCards();
        if (FightCardManager.Instance.consumeCardList != null)
            FightCardManager.Instance.consumeCardList.Clear();
        
        // 重置能力牌追踪（本场战斗内能力牌不会重复出现）
        FightCardManager.Instance.ResetUsedAbilityCards();
        // 重置临时升级标记和免费卡标记
        FightCardManager.Instance.ResetTempUpgraded();
        FightCardManager.Instance.ResetFreeCards();

        //切换bgm
        AudioManager.Instance.PlayBGM("battle");

        // 根据岛屿索引和节点类型加载敌人
        int islandIndex = FightManager.Instance.GetCurrentIslandIndex();
        Map.NodeType nodeType = FightManager.Instance.GetCurrentNodeType();

        // 读档时使用保存的关卡ID，确保遇到同一组敌人
        if (SaveManager.IsLoading && EnemyManager.Instance.CurrentLevelId > 0)
        {
            EnemyManager.Instance.LoadResByLevelId(EnemyManager.Instance.CurrentLevelId);
        }
        else
        {
            EnemyManager.Instance.LoadRes(islandIndex, nodeType);
        }

        // 隐藏节点地图UI
        UIManager.Instance.HideUI("SlayTheSpireMapUI");

        //初始化战斗卡牌（重新发牌）
        FightCardManager.Instance.Init();

        //显示战斗界面
        FightUI fightUI = UIManager.Instance.ShowUI<FightUI>("FightUI") as FightUI;

        // 清理旧的手牌并刷新显示
        if (fightUI != null)
        {
            fightUI.ClearAllCards(); // 清理旧手牌
            fightUI.UpdateDefense();  // 刷新防御值显示
            fightUI.UpdateUsedCardCount(); // 刷新弃牌堆数量显示（重置为0）
            fightUI.UpdateConsumeCardCount(); // 刷新废牌堆数量显示（重置为0）
            fightUI.CreateCardItem(0); // 初始手牌为0张
            fightUI.UpdateCardItemPos(); // 更新手牌位置
        }

        //切换到玩家回合
        FightManager.Instance.ChangeType(FightType.Player);

        // 触发遗物战斗开始钩子
        RelicManager.Instance.TriggerBattleStart();

        // 战斗初始化完成后存档（SL 读档点，仅正常进入时存档，读档恢复时不覆盖）
        if (!SaveManager.IsLoading)
            SaveManager.Save();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
    }
}
