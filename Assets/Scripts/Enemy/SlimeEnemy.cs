using UnityEngine;

/// <summary>
/// 史莱姆 - 最基础的小怪
/// 特性：
/// 1. 不会回血
/// 2. 每2回合不攻击则下回合强制攻击
/// 3. 每攻击2次，第3次攻击时往玩家抽牌堆塞一张黏液（本场战斗有效）
/// </summary>
public class SlimeEnemy : Enemy
{
    private int attackCount = 0; // 攻击次数计数器（用于黏液特性）

    public override void SetRandomAction()
    {
        if (turnsWithoutAttack >= 2)
        {
            type = ActionType.Attack;
        }
        else
        {
            type = UnityEngine.Random.Range(0, 2) == 0 ? ActionType.Attack : ActionType.Defend;
        }

        UpdateActionIcon();
    }

    protected override void PerformAttack()
    {
        // 攻击前先累加计数器
        attackCount++;

        // 每攻击2次（即第3次、第6次...），往玩家抽牌堆塞一张黏液
        if (attackCount % 3 == 0)
        {
            ShuffleMucusToDrawPile();
        }

        // 调用基类攻击逻辑
        base.PerformAttack();
    }

    /// <summary>
    /// 往玩家的抽牌堆塞一张黏液，并刷新抽牌堆显示
    /// </summary>
    private void ShuffleMucusToDrawPile()
    {
        // 加载黏液卡牌数据
        CardData mucusData = Resources.Load<CardData>("Data_Card/Card/2001_黏液");
        if (mucusData == null)
        {
            Debug.LogWarning("黏液卡牌数据加载失败");
            return;
        }

        // 创建一个新的 DeckCard 实例（本场战斗有效）
        DeckCard dc = new DeckCard(mucusData);

        // 插入抽牌堆的随机位置（模拟"洗入"效果，类似杀戮尖塔）
        int insertIndex = UnityEngine.Random.Range(0, FightCardManager.Instance.cardList.Count + 1);
        FightCardManager.Instance.cardList.Insert(insertIndex, dc);

        // 刷新抽牌堆数量显示
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.UpdateCardCount();
        }
    }
}

