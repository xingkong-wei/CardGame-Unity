using UnityEngine;

/// <summary>
/// 缠绕 - 状态卡牌，回合结束时若还在手上则对玩家造成6点伤害
/// </summary>
public class IntertwineCard : CardItem
{
    public override void OnPlayerTurnEndInHand()
    {
        FightManager.Instance.GetPlayerHit(6);
        UIManager.Instance.ShowTip("缠绕 -6", Color.red);
    }
}
