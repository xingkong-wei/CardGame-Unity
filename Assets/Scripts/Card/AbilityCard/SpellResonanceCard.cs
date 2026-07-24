using UnityEngine;

// 法术共鸣（能力牌）- 每回合开始获得1层亲和度最高的元素
public class SpellResonanceCard : AbilityCard
{
    private bool _pendingDuplicate;

    protected override void OnCardUsed()
    {
        // 提前接管复制，避免OnEndDrag重复触发
        bool grabbedDup = false;
        if (!_pendingDuplicate && BuffManager.Instance.HasStatus(StatusType.Duplicate))
        {
            grabbedDup = true;
            BuffManager.Instance.RemoveStatus(StatusType.Duplicate, 1);
        }

        base.OnCardUsed();

        // 添加法术共鸣Buff（永久）
        BuffManager.Instance.AddStatus(StatusType.SpellResonance, 1, -1);
        UIManager.Instance.ShowTip("法术共鸣已激活", new Color(1f, 0.8f, 0f));
        PlayAbilityEffect();

        // 复制药水：触发第二次
        if (grabbedDup)
        {
            _pendingDuplicate = true;
            UIManager.Instance.ShowTip("复制!", Color.magenta);
            OnCardUsed();
            _pendingDuplicate = false;
        }
    }
}
