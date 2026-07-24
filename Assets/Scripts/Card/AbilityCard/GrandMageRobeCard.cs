using UnityEngine;

// 大法师袍（能力牌）- 每打出一张法术牌，获得2点格挡
public class GrandMageRobeCard : AbilityCard
{
    protected override void OnCardUsed()
    {
        // 提前接管复制
        int extraStacks = 0;
        if (BuffManager.Instance.HasStatus(StatusType.Duplicate))
        {
            extraStacks = 1;
            BuffManager.Instance.RemoveStatus(StatusType.Duplicate, 1);
        }

        base.OnCardUsed();

        // 添加大法师袍状态（永久，复制时2层）
        BuffManager.Instance.AddStatus(StatusType.GrandMageRobe, 1 + extraStacks, -1);

        UIManager.Instance.ShowTip("大法师袍已激活", Color.green);
        PlayAbilityEffect();
    }
}
