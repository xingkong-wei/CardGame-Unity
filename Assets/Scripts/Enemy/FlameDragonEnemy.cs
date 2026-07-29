using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 赤焰龙 - Boss，半血后切换飞行模式
/// 
/// 地面行动（3步循环）：攻击(3选1) → 攻击(3选1) → 防御
/// 飞行行动（10步循环）：FlyBiteAttackLow → FlyProjectileAttackLow → 防御 → FlyFireBreathAttackHigh → 防御
///                      → FlyBiteAttackHigh → 防御 → FlyProjectileAttackHigh → FlyFireBreathAttackLow → 防御
/// </summary>
public class FlameDragonEnemy : Enemy
{
    private int _stepIndex;
    private string _groundAttack1;
    private string _groundAttack2;

    private static readonly string[] GroundAttacks = { "BiteAttack", "FireBreathAttack", "ProjectileAttack" };

    // 飞行循环（10步）
    private static readonly string[] FlightPattern =
    {
        "A", "FlyBiteAttackLow",          // 0
        "A", "FlyProjectileAttackLow",    // 1
        "D", null,                         // 2 防御
        "A", "FlyFireBreathAttackHigh",    // 3
        "D", null,                         // 4 防御
        "A", "FlyBiteAttackHigh",          // 5
        "D", null,                         // 6 防御
        "A", "FlyProjectileAttackHigh",    // 7
        "A", "FlyFireBreathAttackLow",     // 8
        "D", null,                         // 9 防御
    };

    protected override void OnFlightModeChanged()
    {
        _stepIndex = 0;
        SetFlightAction();
        UpdateActionIcon();
    }

    public override void SetRandomAction()
    {

        if (isFlightMode)
        {
            SetFlightAction();
        }
        else
        {
            SetGroundAction();
        }

        UpdateActionIcon();
    }

    private void SetGroundAction()
    {
        // 地面循环：攻击→攻击→防御→攻击→防御（5步）
        switch (_stepIndex)
        {
            case 0: // 攻击(3选1)
                type = ActionType.Attack;
                _groundAttack1 = GroundAttacks[Random.Range(0, 3)];
                currentAttackAnimName = _groundAttack1;
                break;
            case 1: // 攻击(3选1)
                type = ActionType.Attack;
                _groundAttack2 = GroundAttacks[Random.Range(0, 3)];
                currentAttackAnimName = _groundAttack2;
                break;
            case 2: // 防御
                type = ActionType.Defend;
                break;
            case 3: // 攻击(3选1)
                type = ActionType.Attack;
                currentAttackAnimName = GroundAttacks[Random.Range(0, 3)];
                break;
            case 4: // 防御
                type = ActionType.Defend;
                break;
        }

        _stepIndex = (_stepIndex + 1) % 5;
    }

    private void SetFlightAction()
    {
        string actionType = FlightPattern[_stepIndex * 2];
        string animName = FlightPattern[_stepIndex * 2 + 1];

        if (actionType == "A")
        {
            type = ActionType.Attack;
            currentAttackAnimName = animName;
        }
        else
        {
            type = ActionType.Defend;
        }

        _stepIndex = (_stepIndex + 1) % 10;
    }

    public override IEnumerator DoAction()
    {
        // 如果血量降到飞行阈值但行动意图还是地面模式，立即切换为飞行行动
        bool isGroundAction = currentAttackAnimName == null || !currentAttackAnimName.StartsWith("Fly");
        if (isFlightMode && type != ActionType.None && isGroundAction)
        {
            _stepIndex = 0;
            SetFlightAction();
        }

        HideAction();

        if (type == ActionType.Attack)
        {
            SafeCrossFade(currentAttackAnimName, 0.2f);

            switch (currentAttackAnimName)
            {
                case "FlyBiteAttackHigh":
                    // 6*3 多段
                    for (int i = 0; i < 3; i++)
                    {
                        PlayAttackEffect(currentAttackAnimName);
                        yield return new WaitForSeconds(0.3f);
                        PerformSingleHit();
                    }
                    BuffManager.Instance.AddStatus(StatusType.Fear, 3);
                    break;

                case "FlyProjectileAttackHigh":
                    // 5*2 多段
                    for (int i = 0; i < 2; i++)
                    {
                        PlayAttackEffect(currentAttackAnimName);
                        yield return new WaitForSeconds(0.3f);
                        PerformSingleHit();
                    }
                    AddStatus(StatusType.Strength, 2);
                    BuffManager.Instance.AddStatus(StatusType.Weak, 1);
                    break;

                default:
                    PlayAttackEffect(currentAttackAnimName);
                    yield return new WaitForSeconds(0.5f);
                    PerformSingleHit();
                    ApplyGroundAttackEffect();
                    ApplyFlightAttackEffect();
                    break;
            }
        }
        else if (type == ActionType.Defend)
        {
            SafeCrossFade(GetRandomDefenseAnim(), 0.2f);
            yield return new WaitForSeconds(0.5f);
            PerformDefend();
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

    private void PerformSingleHit()
    {
        int baseDamage = GetAttackDamageForAnim(currentAttackAnimName);
        int modifiedAttack = ModifyAttackDamage(baseDamage);
        FightManager.Instance.GetPlayerHit(modifiedAttack, this);
        Camera.main.DOShakePosition(0.1f, 0.2f, 5, 45);
    }

    private void ApplyGroundAttackEffect()
    {
        switch (currentAttackAnimName)
        {
            case "BiteAttack":
                BuffManager.Instance.AddStatus(StatusType.Fear, 1);
                break;
            case "FireBreathAttack":
                BuffManager.Instance.AddStatus(StatusType.Scorch, 1);
                break;
            case "ProjectileAttack":
                AddStatus(StatusType.Strength, 1);
                break;
        }
    }

    private void ApplyFlightAttackEffect()
    {
        switch (currentAttackAnimName)
        {
            case "FlyBiteAttackLow":
                BuffManager.Instance.AddStatus(StatusType.Fear, 2);
                break;
            case "FlyFireBreathAttackLow":
                BuffManager.Instance.AddStatus(StatusType.Scorch, 2);
                break;
            case "FlyFireBreathAttackHigh":
                BuffManager.Instance.AddStatus(StatusType.Scorch, 3);
                break;
            case "FlyProjectileAttackLow":
                AddStatus(StatusType.Strength, 2);
                BuffManager.Instance.AddStatus(StatusType.Weak, 1);
                break;
        }
    }

    protected override void PerformDefend()
    {
        base.PerformDefend();

        // 防御时给玩家1层虚弱
        BuffManager.Instance.AddStatus(StatusType.Weak, 1);
    }
}
