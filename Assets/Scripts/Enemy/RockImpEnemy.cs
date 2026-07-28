using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 岩屑小鬼 - 偷金币 + 荆棘反伤型敌人
/// 特性：
/// 1. 不回血
/// 2. 每次攻击偷取玩家10金币（被打败不返还）
/// 3. 每次防御获得1层荆棘（每层反伤1点）
/// 4. MeleeAttack：7点伤害×2，对应动作MeleeAttack
/// 5. ThrowRockAttack：5点伤害×2 + 给玩家2层虚弱，对应动作ThrowRockAttack
/// </summary>
public class RockImpEnemy : Enemy
{
    private bool _nextIsMelee = true;

    public override void SetRandomAction()
    {
        if (turnsWithoutAttack >= 2)
        {
            type = ActionType.Attack;
        }
        else
        {
            type = Random.Range(0, 2) == 0 ? ActionType.Attack : ActionType.Defend;
        }

        if (type == ActionType.Attack)
        {
            currentAttackAnimName = _nextIsMelee ? "MeleeAttack" : "ThrowRockAttack";
            _nextIsMelee = !_nextIsMelee;
        }

        UpdateActionIcon();
    }

    public override IEnumerator DoAction()
    {
        HideAction();

        if (type == ActionType.Attack)
        {
            SafeCrossFade(currentAttackAnimName, 0.2f);

            // 2段攻击，每段播放特效
            for (int i = 0; i < 2; i++)
            {
                PlayAttackEffect(currentAttackAnimName);
                yield return new WaitForSeconds(0.3f);
                PerformSingleHit();
            }

            // 偷取金币
            int stolen = Mathf.Min(10, FightManager.Instance.CoinAmount);
            if (stolen > 0)
            {
                FightManager.Instance.AddCoin(-stolen);
                UIManager.Instance.ShowTip($"金币 -{stolen}", Color.yellow);
            }

            // ThrowRockAttack 额外给玩家2层虚弱
            if (currentAttackAnimName == "ThrowRockAttack")
            {
                BuffManager.Instance.AddStatus(StatusType.Weak, 2);
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

    protected override void PerformDefend()
    {
        base.PerformDefend();

        // 防御时获得1层荆棘
        AddStatus(StatusType.Thorns, 1);
    }
}
