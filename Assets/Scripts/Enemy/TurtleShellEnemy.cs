using UnityEngine;

/// <summary>
/// 潮息龟 - 高防御敌人
/// 特性：
/// 1. 第一回合强制防御 + 给玩家5层缩小
/// 2. 不会回血
/// 3. 每2回合不攻击则下回合强制攻击
/// </summary>
public class TurtleShellEnemy : Enemy
{
    private int turnCount = 0;

    public override void SetRandomAction()
    {
        turnCount++;

        // 第一回合：强制防御
        if (turnCount == 1)
        {
            type = ActionType.Defend;
            UpdateActionIcon();
            return;
        }

        // 不会回血，只在攻击/防御间随机
        if (turnsWithoutAttack >= 2)
        {
            type = ActionType.Attack;
        }
        else
        {
            type = Random.Range(0, 2) == 0 ? ActionType.Attack : ActionType.Defend;
        }

        UpdateActionIcon();
    }

    protected override void PerformDefend()
    {
        base.PerformDefend();

        // 第一回合防御时给玩家5层缩小
        if (turnCount == 1)
        {
            BuffManager.Instance.AddStatus(StatusType.Shrink, 5);
        }
    }
}
