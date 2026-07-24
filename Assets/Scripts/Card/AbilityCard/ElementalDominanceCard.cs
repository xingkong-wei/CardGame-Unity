using UnityEngine;

/// <summary>
/// 元素主宰（能力牌）
/// 使用后每回合开始获得1点火、1点冰、1点雷亲和度（不包括使用回合）
/// </summary>
public class ElementalDominanceCard : AbilityCard
{
    protected override void OnCardUsed()
    {
        base.OnCardUsed();

        // 添加元素主宰状态（永久）
        BuffManager.Instance.AddStatus(StatusType.ElementalDominance, 1, -1);

        // 显示提示
        UIManager.Instance.ShowTip("元素主宰已激活", Color.yellow);

        // 播放特效
        PlayAbilityEffect();
    }
}
