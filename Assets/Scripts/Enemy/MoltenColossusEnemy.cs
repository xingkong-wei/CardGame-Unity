using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 熔核巨像 - Boss，7步固定循环
/// 特性：
/// MeleeAttack02(10+2力量) → ShockwaveAttack(8+2脆弱) → CrushAttack(6+1眩晕)
/// → FireBreathAttack(3*2) → 防御 → 回血 → MeleeAttack01(14+1力量)
/// </summary>
public class MoltenColossusEnemy : Enemy
{
    private int _stepIndex; // 0=Melee02, 1=Shockwave, 2=Crush, 3=FireBreath, 4=防御, 5=回血, 6=Melee01

    public override void SetRandomAction()
    {
        switch (_stepIndex)
        {
            case 0:
                type = ActionType.Attack;
                currentAttackAnimName = "MeleeAttack02";
                break;
            case 1:
                type = ActionType.Attack;
                currentAttackAnimName = "ShockwaveAttack";
                break;
            case 2:
                type = ActionType.Attack;
                currentAttackAnimName = "CrushAttack";
                break;
            case 3:
                type = ActionType.Attack;
                currentAttackAnimName = "FireBreathAttack";
                break;
            case 4:
                type = ActionType.Defend;
                break;
            case 5:
                type = ActionType.Heal;
                break;
            case 6:
                type = ActionType.Attack;
                currentAttackAnimName = "MeleeAttack01";
                break;
        }

        _stepIndex = (_stepIndex + 1) % 7;
        UpdateActionIcon();
    }

    public override IEnumerator DoAction()
    {
        HideAction();

        if (type == ActionType.Attack)
        {
            SafeCrossFade(currentAttackAnimName, 0.2f);

            if (currentAttackAnimName == "FireBreathAttack")
            {
                // 3*2 多段攻击
                for (int i = 0; i < 2; i++)
                {
                    PlayAttackEffect(currentAttackAnimName);
                    yield return new WaitForSeconds(0.3f);
                    PerformSingleHit();
                }
            }
            else
            {
                PlayAttackEffect(currentAttackAnimName);
                yield return new WaitForSeconds(0.5f);
                PerformSingleHit();
            }

            // 攻击附加效果
            switch (currentAttackAnimName)
            {
                case "MeleeAttack01":
                    AddStatus(StatusType.Strength, 1);
                    break;
                case "MeleeAttack02":
                    AddStatus(StatusType.Strength, 2);
                    break;
                case "ShockwaveAttack":
                    BuffManager.Instance.AddStatus(StatusType.Frail, 2);
                    break;
                case "CrushAttack":
                    BuffManager.Instance.AddStatus(StatusType.Dizzy, 1);
                    break;
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

    private void PerformSingleHit()
    {
        int baseDamage = GetAttackDamageForAnim(currentAttackAnimName);
        int modifiedAttack = ModifyAttackDamage(baseDamage);
        FightManager.Instance.GetPlayerHit(modifiedAttack, this);
        Camera.main.DOShakePosition(0.1f, 0.2f, 5, 45);
    }
}
