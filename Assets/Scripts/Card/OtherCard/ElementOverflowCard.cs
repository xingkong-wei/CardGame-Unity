using UnityEngine;

/// <summary>
/// 元素四溢 - 选择消耗一种元素亲和度，每消耗5层获得1点能量
/// </summary>
public class ElementOverflowCard : CardItem
{
    private bool _hasPendingDuplicate;
    private int _duplicateCount;

    protected override void OnCardUsed()
    {
        // 提前接管复制状态，避免OnEndDrag重复触发
        if (!_hasPendingDuplicate)
            GrabDuplicate();

        base.OnCardUsed();

        // 检查是否有≥5层的亲和度
        int fireStack = BuffManager.Instance.GetStack(StatusType.FireAffinity);
        int iceStack = BuffManager.Instance.GetStack(StatusType.IceAffinity);
        int lightningStack = BuffManager.Instance.GetStack(StatusType.LightningAffinity);

        bool hasOption = fireStack >= 5 || iceStack >= 5 || lightningStack >= 5;

        if (!hasOption)
        {
            UIManager.Instance.ShowTip("亲和度都不足5层", Color.yellow);
            return;
        }

        // 显示选择弹窗
        UIManager.Instance.ShowUI<ElementSelectUI>("ElementSelectUI");
        UIManager.Instance.GetUI<ElementSelectUI>("ElementSelectUI").ShowSelect(OnElementSelected);
    }

    /// <summary>
    /// 接管复制药水逻辑（在OnCardUsed里提前消费复制状态，避免OnEndDrag重复触发）
    /// </summary>
    public void GrabDuplicate()
    {
        if (BuffManager.Instance.HasStatus(StatusType.Duplicate))
        {
            _hasPendingDuplicate = true;
            _duplicateCount = BuffManager.Instance.GetStack(StatusType.Duplicate);
            BuffManager.Instance.RemoveStatus(StatusType.Duplicate, 99);
        }
    }

    /// <summary>
    /// 选择亲和度后的回调
    /// </summary>
    private void OnElementSelected(StatusType type)
    {
        // 消耗5层亲和度
        BuffManager.Instance.RemoveStatus(type, 5);

        // 获得1点能量
        FightManager.Instance.CurPowerCount += 1;
        UIManager.Instance.ShowTip("元素四溢：消耗5层亲和度，获得1能量", Color.yellow);

        // 更新UI
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.UpdatePower();
        }

        // 复制药水：首次选完后自动弹出第二次
        if (_hasPendingDuplicate)
        {
            _hasPendingDuplicate = false;
            UIManager.Instance.ShowTip("复制!", Color.magenta);
            OnCardUsed();
        }
    }
}
