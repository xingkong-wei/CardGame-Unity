using UnityEngine;

/// <summary>
/// 蛇毒 - 状态消耗卡牌，可打出
/// 回合结束时若还在手上，获得2层易伤（玩家回合开始时减少1层）
/// </summary>
public class SnakeVenomCard : CardItem
{
    public override void OnPlayerTurnEndInHand()
    {
        BuffManager.Instance.AddStatus(StatusType.Vulnerable, 2);
        UIManager.Instance.ShowTip("蛇毒：易伤 +2", Color.red);
    }
}
