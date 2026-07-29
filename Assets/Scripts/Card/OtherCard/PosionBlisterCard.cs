using UnityEngine;

/// <summary>
/// 毒泡 - 状态卡牌，无法打出
/// 回合结束时若还在手上，对玩家造成3点伤害
/// </summary>
public class PosionBlisterCard : CardItem
{
    protected override bool CanUseCondition()
    {
        UIManager.Instance.ShowTip("毒泡无法打出", Color.red);
        return false;
    }

    public override void OnPlayerTurnEndInHand()
    {
        FightManager.Instance.GetPlayerHit(3);
        UIManager.Instance.ShowTip("毒泡 -3", Color.green);
    }
}
