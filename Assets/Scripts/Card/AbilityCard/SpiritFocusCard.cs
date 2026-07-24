using UnityEngine;

// 聚灵（能力牌）- 亲和度上限从10层提升至15层
public class SpiritFocusCard : AbilityCard
{
    protected override void OnCardUsed()
    {
        base.OnCardUsed();

        // 添加聚灵状态（永久）
        BuffManager.Instance.AddStatus(StatusType.SpiritFocus, 1, -1);

        // 显示提示
        UIManager.Instance.ShowTip("聚灵：亲和度上限提升至15层", Color.green);

        // 播放特效
        PlayAbilityEffect();
    }
}
