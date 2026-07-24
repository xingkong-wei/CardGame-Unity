using UnityEngine;
using DG.Tweening;

/// <summary>
/// 能力牌基类 - 能力牌使用后不放任何堆（杀戮尖塔风格）
/// 子类只需重写 OnCardUsed() 实现具体效果
/// </summary>
public abstract class AbilityCard : CardItem
{
    protected override bool TryUse()
    {
        if (data == null)
        {
            Debug.LogError("AbilityCard: data is null!");
            return false;
        }

        int cost = GetCost();

        if (cost > FightManager.Instance.CurPowerCount)
        {
            AudioManager.Instance.PlayEffect("Effect/lose");
            UIManager.Instance.ShowTip("费用不足", Color.red);
            return false;
        }

        // 扣费
        FightManager.Instance.CurPowerCount -= cost;
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.UpdatePower();
            DestroyCardDirectly();
        }

        return true;
    }

    /// <summary>
    /// 直接销毁卡牌（不放任何堆）
    /// </summary>
    protected void DestroyCardDirectly()
    {
        AudioManager.Instance.PlayEffect("Cards/cardShove");
        
        // 标记能力牌为已使用（防止本场战斗重复加载）
        if (sourceDeckCard != null)
        {
            FightCardManager.Instance.MarkAbilityAsUsed(sourceDeckCard.instanceId);
        }
        
        // 获取FightUI的引用来移除cardItemList中的记录
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            // 通过反射或公开方法移除
            fightUI.RemoveCardDirectly(this);
        }
        
        // 播放消失动画
        transform.DOScale(0, 0.3f);
        Destroy(gameObject, 0.3f);
    }

    /// <summary>
    /// 播放特效
    /// </summary>
    protected void PlayAbilityEffect()
    {
        if (string.IsNullOrEmpty(data.effects)) return;
        GameObject effectObj = Instantiate(Resources.Load(data.effects)) as GameObject;
        Vector3 pos = Camera.main.transform.position;
        pos.y = 0;
        effectObj.transform.position = pos;
        Destroy(effectObj, 2);
    }
}
