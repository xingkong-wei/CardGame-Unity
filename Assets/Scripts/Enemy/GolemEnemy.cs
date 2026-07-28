/// <summary>
/// 戈仑石人 - 攻防交替，越战越强
/// 特性：
/// 1. 一攻一防，交替进行
/// 2. 每次攻击后自身获得3点力量
/// 3. 每点力量增加1点伤害（由 StatusCallbacks 中 Strength 的 modifyAttackDamage 自动处理）
/// </summary>
public class GolemEnemy : Enemy
{
    private bool _nextIsAttack = true; // 首次为攻击

    public override void SetRandomAction()
    {
        type = _nextIsAttack ? ActionType.Attack : ActionType.Defend;
        _nextIsAttack = !_nextIsAttack;
        UpdateActionIcon();
    }

    protected override void PerformAttack()
    {
        base.PerformAttack();

        // 攻击后获得3点力量
        AddStatus(StatusType.Strength, 3);
    }
}
