using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 裂颚蜓 - 纯攻击型敌人，BreathAttack 与 BiteAttack 交替
/// 特性：
/// 1. BreathAttack → BiteAttack 交替，不防御不回血
/// 2. BreathAttack：8点伤害 + 给玩家1层虚弱
/// 3. BiteAttack：6点伤害 + 自身获得3点力量
/// </summary>
public class RiftDragonflyEnemy : Enemy
{
    private bool _nextIsBreath = true;

    public override void SetRandomAction()
    {
        type = ActionType.Attack;
        currentAttackAnimName = _nextIsBreath ? "BreathAttack" : "BiteAttack";
        _nextIsBreath = !_nextIsBreath;
        UpdateActionIcon();
    }

    public override IEnumerator DoAction()
    {
        HideAction();

        SafeCrossFade(currentAttackAnimName, 0.2f);
        PlayAttackEffect(currentAttackAnimName);
        yield return new WaitForSeconds(0.5f);
        PerformAttack();

        yield return new WaitForSeconds(1);

        turnsWithoutAttack = 0;
        SafeCrossFade(GetCurrentIdleAnim(), 0f);
    }

    protected override void PerformAttack()
    {
        int baseDamage = GetAttackDamageForAnim(currentAttackAnimName);
        int modifiedAttack = ModifyAttackDamage(baseDamage);
        FightManager.Instance.GetPlayerHit(modifiedAttack, this);
        Camera.main.DOShakePosition(0.1f, 0.2f, 5, 45);

        if (currentAttackAnimName == "BreathAttack")
        {
            BuffManager.Instance.AddStatus(StatusType.Weak, 1);
        }
        else
        {
            AddStatus(StatusType.Strength, 3);
        }
    }
}
