using UnityEngine;

/// <summary>
/// 迷缘秘液 - 选择1种亲和度，全部转化为另一种（与元素融合一致）
/// </summary>
public class FateBindElixirPotion : PotionBase
{
    public override void Use()
    {
        int fire = BuffManager.Instance.GetStack(StatusType.FireAffinity);
        int ice = BuffManager.Instance.GetStack(StatusType.IceAffinity);
        int lightning = BuffManager.Instance.GetStack(StatusType.LightningAffinity);

        if (fire + ice + lightning == 0)
        {
            UIManager.Instance.ShowTip("没有亲和度", Color.yellow);
            return;
        }

        base.Use();

        // 第一步：选择来源元素
        UIManager.Instance.ShowUI<ElementSelectUI>("ElementSelectUI");
        var ui1 = UIManager.Instance.GetUI<ElementSelectUI>("ElementSelectUI");
        ui1.ShowSelectForSource((StatusType fromType) =>
        {
            ui1.Close();

            // 第二步：选择目标元素
            UIManager.Instance.ShowUI<ElementSelectUI>("ElementSelectUI");
            var ui2 = UIManager.Instance.GetUI<ElementSelectUI>("ElementSelectUI");
            ui2.ShowSelectForTarget(fromType, (StatusType toType) =>
            {
                ui2.Close();
                BuffManager.Instance.ConvertAffinity(fromType, toType);
                UIManager.Instance.ShowTip("转化完成", Color.magenta);
            });
        });
    }
}
