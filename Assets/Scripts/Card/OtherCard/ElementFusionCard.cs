using UnityEngine;
using UnityEngine.EventSystems;

// 元素融合 - 将一种元素亲和度全部转换为另一种（1:1转换）
public class ElementFusionCard : CardItem
{
    private bool _pendingDuplicate;

    /// <summary>
    /// 检查是否有亲和度可以转换
    /// </summary>
    protected override bool CanUseCondition()
    {
        int fireStack = BuffManager.Instance.GetStack(StatusType.FireAffinity);
        int iceStack = BuffManager.Instance.GetStack(StatusType.IceAffinity);
        int lightningStack = BuffManager.Instance.GetStack(StatusType.LightningAffinity);

        bool hasAffinity = fireStack > 0 || iceStack > 0 || lightningStack > 0;

        if (!hasAffinity)
        {
            UIManager.Instance.ShowTip("没有亲和度可以转换", Color.yellow);
            return false;
        }
        return true;
    }

    protected override void OnCardUsed()
    {
        // 提前接管复制
        if (!_pendingDuplicate && BuffManager.Instance.HasStatus(StatusType.Duplicate))
        {
            _pendingDuplicate = true;
            BuffManager.Instance.RemoveStatus(StatusType.Duplicate, 1);
        }

        base.OnCardUsed();

        // 第一步：选择来源元素
        UIManager.Instance.ShowUI<ElementSelectUI>("ElementSelectUI");
        var selectUI = UIManager.Instance.GetUI<ElementSelectUI>("ElementSelectUI");
        selectUI.ShowSelectForSource((fromType) => {
            selectUI.Close();
            // 第二步：选择目标元素
            UIManager.Instance.ShowUI<ElementSelectUI>("ElementSelectUI");
            var selectUI2 = UIManager.Instance.GetUI<ElementSelectUI>("ElementSelectUI");
            selectUI2.ShowSelectForTarget(fromType, (toType) => {
                selectUI2.Close();
                // 执行转换
                BuffManager.Instance.ConvertAffinity(fromType, toType);

                // 复制药水：第一次转换完后出第二次
                if (_pendingDuplicate)
                {
                    _pendingDuplicate = false;
                    UIManager.Instance.ShowTip("复制!", Color.magenta);
                    OnCardUsed();
                }
            });
        });
    }
}
