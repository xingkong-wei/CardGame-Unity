using System.Collections;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 灰烬蛛 - 精英敌人，固定循环 + 塞缠绕
/// 特性：
/// 1. 第一回合强制防御，往手牌/抽牌堆/弃牌堆各塞一张缠绕
/// 2. 循环：ProjectileAttack(14) → BiteAttack(9+2力量) → ClawAttack(7*2) → 回血 → BreathAttack(6+塞缠绕)
/// </summary>
public class AshSpiderEnemy : Enemy
{
    private int _turnCount;
    private int _stepIndex; // 0=Projectile, 1=BiteAttack, 2=ClawAttack, 3=回血, 4=BreathAttack

    public override void SetRandomAction()
    {
        _turnCount++;

        if (_turnCount == 1)
        {
            type = ActionType.Defend;
        }
        else
        {
            type = (_stepIndex == 3) ? ActionType.Heal : ActionType.Attack;

            if (type == ActionType.Attack)
            {
                switch (_stepIndex)
                {
                    case 0: currentAttackAnimName = "ProjectileAttack"; break;
                    case 1: currentAttackAnimName = "BiteAttack"; break;
                    case 2: currentAttackAnimName = "ClawAttack"; break;
                    case 4: currentAttackAnimName = "BreathAttack"; break;
                }
            }

            _stepIndex = (_stepIndex + 1) % 5;
        }

        UpdateActionIcon();
    }

    public override IEnumerator DoAction()
    {
        HideAction();

        if (type == ActionType.Attack)
        {
            SafeCrossFade(currentAttackAnimName, 0.2f);

            if (currentAttackAnimName == "ClawAttack")
            {
                // 7*2 多段攻击
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

            // BiteAttack 额外获得2点力量
            if (currentAttackAnimName == "BiteAttack")
            {
                AddStatus(StatusType.Strength, 2);
            }

            // BreathAttack 额外往抽牌堆塞一张缠绕
            if (currentAttackAnimName == "BreathAttack")
            {
                ShuffleIntertwineToDrawPile();
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

        // 第一回合防御时：往手牌/抽牌堆/弃牌堆各塞一张缠绕
        if (_turnCount == 1)
        {
            CardData intertwineData = ResourceCache.Get<CardData>("Data_Card/Card/2003_缠绕");
            if (intertwineData == null)
            {
                Debug.LogWarning("缠绕卡牌数据加载失败");
                return;
            }

            FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");

            // 往手牌塞一张
            if (fightUI != null)
            {
                fightUI.CreateCardItem(intertwineData);
                fightUI.UpdateCardItemPos();
            }

            // 往抽牌堆塞一张
            DeckCard dc2 = new DeckCard(intertwineData);
            int idx = Random.Range(0, FightCardManager.Instance.cardList.Count + 1);
            FightCardManager.Instance.cardList.Insert(idx, dc2);

            // 往弃牌堆塞一张
            DeckCard dc3 = new DeckCard(intertwineData);
            FightCardManager.Instance.usedCardList.Add(dc3);

            // 刷新UI
            if (fightUI != null)
            {
                fightUI.UpdateCardCount();
                fightUI.UpdateUsedCardCount();
            }
        }
    }

    /// <summary>
    /// 往玩家抽牌堆塞一张缠绕
    /// </summary>
    private void ShuffleIntertwineToDrawPile()
    {
        CardData intertwineData = ResourceCache.Get<CardData>("Data_Card/Card/2003_缠绕");
        if (intertwineData == null)
        {
            Debug.LogWarning("缠绕卡牌数据加载失败");
            return;
        }

        DeckCard dc = new DeckCard(intertwineData);
        int idx = Random.Range(0, FightCardManager.Instance.cardList.Count + 1);
        FightCardManager.Instance.cardList.Insert(idx, dc);

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        fightUI?.UpdateCardCount();
    }
}
