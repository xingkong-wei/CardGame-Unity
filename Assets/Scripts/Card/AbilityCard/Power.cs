using UnityEngine;
using DG.Tweening;

/// <summary>
/// 魔能卡（能力牌）
/// 使用后提升魔能，每点魔能提升1点伤害
/// 能力牌使用后不放任何堆
/// </summary>
public class Power : AbilityCard
{
    protected override void OnCardUsed()
    {
        base.OnCardUsed();

        int powerGain = data != null ? GetArg0() : 2;
        
        // 添加魔能Buff（永久）
        BuffManager.Instance.AddStatus(StatusType.Power, powerGain, -1);

        // 显示提示
        UIManager.Instance.ShowTip($"魔能 +{powerGain}", new Color(0.6f, 0.3f, 1f)); // 紫色

        // 播放特效
        PlayAbilityEffect();
    }
}
