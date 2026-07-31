using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 沌元浓浆 - 打出抽牌堆随机的3张牌（不足3张时有多少打多少）
/// </summary>
public class ChaosEssenceSlurryPotion : PotionBase
{
    private static MethodInfo _onCardUsedMethod;
    private static MethodInfo _getAttackTimesMethod;

    public override void Use()
    {
        base.Use();

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI == null) return;

        List<DeckCard> drawPile = FightCardManager.Instance.cardList;
        int count = Mathf.Min(data.effectValue, drawPile.Count);

        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, drawPile.Count);
            DeckCard dc = drawPile[idx];
            drawPile.RemoveAt(idx);
            if (dc == null || dc.cardData == null) continue;

            PlayCard(dc);
        }

        fightUI.UpdateCardCount();
    }

    private void PlayCard(DeckCard dc)
    {
        CardData cd = dc.cardData;
        System.Type type = System.Type.GetType(cd.scriptName);
        if (type == null || !typeof(CardItem).IsAssignableFrom(type)) return;

        // 创建临时对象执行卡牌效果
        GameObject obj = new GameObject("TempCard");
        obj.SetActive(false);
        CardItem card = obj.AddComponent(type) as CardItem;
        card.Init(cd, dc);
        card.costOverride = 0;

        // 攻击牌：对随机敌人造成伤害
        if (cd.HasCardType("攻击"))
        {
            AttackCardItem atk = card as AttackCardItem;
            if (atk != null)
            {
                Enemy target = GetRandomAliveEnemy();
                if (target != null)
                {
                    int dmg = atk.GetFinalAttackDamage();
                    int times = GetAttackTimes(atk);
                    for (int j = 0; j < times; j++)
                        target.Hit(dmg);
                }
            }
        }

        // 触发OnCardUsed效果
        InvokeOnCardUsed(card);

        // 放入弃牌堆/废牌堆
        if (cd.IsConsumeCard())
        {
            FightCardManager.Instance.MarkCardAsConsumed(dc.instanceId);
            FightCardManager.Instance.consumeCardList.Add(dc);
        }
        else
        {
            FightCardManager.Instance.usedCardList.Add(dc);
        }

        Object.Destroy(obj);
    }

    private void InvokeOnCardUsed(CardItem card)
    {
        if (_onCardUsedMethod == null)
            _onCardUsedMethod = typeof(CardItem).GetMethod("OnCardUsed",
                BindingFlags.NonPublic | BindingFlags.Instance);
        _onCardUsedMethod?.Invoke(card, null);
    }

    private int GetAttackTimes(AttackCardItem atk)
    {
        if (_getAttackTimesMethod == null)
            _getAttackTimesMethod = typeof(AttackCardItem).GetMethod("GetAttackTimes",
                BindingFlags.NonPublic | BindingFlags.Instance);
        return _getAttackTimesMethod != null ? (int)_getAttackTimesMethod.Invoke(atk, null) : 1;
    }

    private Enemy GetRandomAliveEnemy()
    {
        List<Enemy> alive = EnemyManager.Instance.GetAliveEnemies();
        return alive.Count > 0 ? alive[Random.Range(0, alive.Count)] : null;
    }
}
