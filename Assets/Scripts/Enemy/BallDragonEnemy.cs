using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 球腹龙 - 固定循环型敌人
/// 特性：
/// 1. 防御时获得护盾 + 1点力量
/// 2. 固定循环：ProjectileAttack(2*4) → 防御 → HeadAttack(11) → 回血(12)
/// </summary>
public class BallDragonEnemy : Enemy
{
    private int _stepIndex; // 0=Projectile, 1=防御, 2=HeadAttack, 3=回血

    public override void SetRandomAction()
    {
        switch (_stepIndex)
        {
            case 0:
                type = ActionType.Attack;
                currentAttackAnimName = "ProjectileAttack";
                break;
            case 1:
                type = ActionType.Defend;
                break;
            case 2:
                type = ActionType.Attack;
                currentAttackAnimName = "HeadAttack";
                break;
            case 3:
                type = ActionType.Heal;
                break;
        }

        _stepIndex = (_stepIndex + 1) % 4;
        UpdateActionIcon();
    }

    public override IEnumerator DoAction()
    {
        HideAction();

        if (type == ActionType.Attack)
        {
            SafeCrossFade(currentAttackAnimName, 0.2f);

            if (currentAttackAnimName == "ProjectileAttack")
            {
                // 2*4 多段攻击，每段播放特效
                for (int i = 0; i < 4; i++)
                {
                    PlayAttackEffect(currentAttackAnimName);
                    yield return new WaitForSeconds(0.3f);
                    PerformSingleHit("ProjectileAttack");
                }
            }
            else
            {
                // HeadAttack：单次攻击
                PlayAttackEffect(currentAttackAnimName);
                yield return new WaitForSeconds(0.5f);
                PerformSingleHit("HeadAttack");
            }
        }
        else if (type == ActionType.Defend)
        {
            SafeCrossFade(GetRandomDefenseAnim(), 0.2f);
            yield return new WaitForSeconds(0.5f);
            PerformDefend();
        }
        else if (type == ActionType.Heal)
        {
            SafeCrossFade(GetHealAnim(), 0.2f);
            yield return new WaitForSeconds(0.5f);
            PerformHeal();
        }
        else
        {
            SafeCrossFade(GetCurrentIdleAnim(), 0.2f);
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(1);

        turnsWithoutAttack = (type == ActionType.Attack) ? 0 : turnsWithoutAttack + 1;
        SafeCrossFade(GetCurrentIdleAnim(), 0f);
    }

    private void PerformSingleHit(string animName)
    {
        int baseDamage = GetAttackDamageForAnim(animName);
        int modifiedAttack = ModifyAttackDamage(baseDamage);
        FightManager.Instance.GetPlayerHit(modifiedAttack, this);
        Camera.main.DOShakePosition(0.1f, 0.2f, 5, 45);
    }

    protected override void PerformDefend()
    {
        base.PerformDefend();

        // 防御时额外获得1点力量
        AddStatus(StatusType.Strength, 1);
    }
}
