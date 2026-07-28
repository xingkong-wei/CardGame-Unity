using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 炭嘴兽 - 多段攻击型敌人
/// 特性：
/// 1. 第1回合：给予玩家3层虚弱，向弃牌堆塞入5张眩晕
/// 2. 第2回合起：BiteAttack 与 ProjectileAttack 交替使用
/// 3. BiteAttack：造成13点伤害
/// 4. ProjectileAttack：造成4*3点伤害（多段攻击）
/// </summary>
public class CarbonBeastEnemy : Enemy
{
    private int _turnCount;
    private bool _nextIsBite = true; // true=BiteAttack, false=ProjectileAttack

    public override void SetRandomAction()
    {
        _turnCount++;

        // 第1回合：防御（给虚弱+塞眩晕）
        if (_turnCount == 1)
        {
            type = ActionType.Defend;
        }
        else
        {
            // 第2回合起交替
            type = ActionType.Attack;
            currentAttackAnimName = _nextIsBite ? "BiteAttack" : "ProjectileAttack";
            _nextIsBite = !_nextIsBite;
        }

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
                // ProjectileAttack：3段攻击，每段播放特效
                for (int i = 0; i < 3; i++)
                {
                    PlayAttackEffect(currentAttackAnimName);
                    yield return new WaitForSeconds(0.3f);
                    PerformSingleProjectileHit(i);
                }
            }
            else
            {
                // BiteAttack：单次攻击
                PlayAttackEffect(currentAttackAnimName);
                yield return new WaitForSeconds(0.5f);
                PerformSingleBiteHit();
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

    private void PerformSingleBiteHit()
    {
        int baseDamage = GetAttackDamageForAnim("BiteAttack");
        int modifiedAttack = ModifyAttackDamage(baseDamage);
        FightManager.Instance.GetPlayerHit(modifiedAttack, this);
        Camera.main.DOShakePosition(0.1f, 0.2f, 5, 45);
    }

    private void PerformSingleProjectileHit(int index)
    {
        int baseDamage = GetAttackDamageForAnim("ProjectileAttack");
        int modifiedAttack = ModifyAttackDamage(baseDamage);
        FightManager.Instance.GetPlayerHit(modifiedAttack, this);
        Camera.main.DOShakePosition(0.1f, 0.2f, 5, 45);
    }

    protected override void PerformDefend()
    {
        base.PerformDefend();

        // 第1回合防御时：给玩家3层虚弱 + 弃牌堆塞5张眩晕
        if (_turnCount == 1)
        {
            BuffManager.Instance.AddStatus(StatusType.Weak, 3);
            ShuffleDizzinessToDiscardPile(5);
        }
    }

    /// <summary>
    /// 向玩家弃牌堆塞入指定数量的眩晕卡
    /// </summary>
    private void ShuffleDizzinessToDiscardPile(int count)
    {
        CardData dizzinessData = Resources.Load<CardData>("Date_Card/Card/2002_眩晕");
        if (dizzinessData == null)
        {
            Debug.LogWarning("眩晕卡牌数据加载失败");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            DeckCard dc = new DeckCard(dizzinessData);
            FightCardManager.Instance.usedCardList.Add(dc);
        }

        // 刷新弃牌堆UI
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.UpdateUsedCardCount();
        }
    }
}
