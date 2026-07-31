using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 炽后 - 精英敌人，固定循环 + 塞状态牌
/// 特性：
/// 1. 防御时获得护盾 + 往弃牌堆塞2张毒泡
/// 2. 回血时恢复生命 + 往抽牌堆塞2张蛇毒
/// 3. 循环：防御 → ProjectileAttack(4*4) → 回血 → ClawAttack(8+1力量)
/// </summary>
public class SuccubusQueenEnemy : Enemy
{
    private int _stepIndex; // 0=防御, 1=ProjectileAttack, 2=回血, 3=ClawAttack

    public override void SetRandomAction()
    {
        switch (_stepIndex)
        {
            case 0:
                type = ActionType.Defend;
                break;
            case 1:
                type = ActionType.Attack;
                currentAttackAnimName = "ProjectileAttack";
                break;
            case 2:
                type = ActionType.Heal;
                break;
            case 3:
                type = ActionType.Attack;
                currentAttackAnimName = "ClawAttack";
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
                // 4*4 多段攻击
                for (int i = 0; i < 4; i++)
                {
                    PlayAttackEffect(currentAttackAnimName);
                    yield return new WaitForSeconds(0.3f);
                    PerformSingleHit();
                }
            }
            else
            {
                // ClawAttack：单次攻击
                PlayAttackEffect(currentAttackAnimName);
                yield return new WaitForSeconds(0.5f);
                PerformSingleHit();

                // ClawAttack 额外获得1点力量
                AddStatus(StatusType.Strength, 1);
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

    protected override void PerformDefend()
    {
        base.PerformDefend();

        // 防御时往弃牌堆塞2张毒泡
        ShuffleCardToDiscardPile("Data_Card/Card/2004_毒泡", 2);
    }

    protected override void PerformHeal()
    {
        base.PerformHeal();

        // 回血时往抽牌堆塞2张蛇毒
        ShuffleCardToDrawPile("Data_Card/Card/2005_蛇毒", 2);
    }

    private void ShuffleCardToDiscardPile(string path, int count)
    {
        CardData cardData = ResourceCache.Get<CardData>(path);
        if (cardData == null)
        {
            Debug.LogWarning($"卡牌数据加载失败: {path}");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            FightCardManager.Instance.usedCardList.Add(new DeckCard(cardData));
        }

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        fightUI?.UpdateUsedCardCount();
    }

    private void ShuffleCardToDrawPile(string path, int count)
    {
        CardData cardData = ResourceCache.Get<CardData>(path);
        if (cardData == null)
        {
            Debug.LogWarning($"卡牌数据加载失败: {path}");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            DeckCard dc = new DeckCard(cardData);
            int idx = Random.Range(0, FightCardManager.Instance.cardList.Count + 1);
            FightCardManager.Instance.cardList.Insert(idx, dc);
        }

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        fightUI?.UpdateCardCount();
    }
}
