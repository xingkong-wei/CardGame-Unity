using UnityEngine;

/// <summary>
/// 瓶中精灵 - 死亡时自动触发，回复最大生命值的30%
/// </summary>
public class BottledSpritePotion : PotionBase
{
    /// <summary>
    /// 仅播放音效和特效（回血逻辑由 FightManager 处理）
    /// </summary>
    public void UseBaseEffects()
    {
        if (data == null) return;

        if (!string.IsNullOrEmpty(data.useSound))
            AudioManager.Instance?.PlayEffect(data.useSound);

        if (!string.IsNullOrEmpty(data.useEffect))
            PlayEffect(data.useEffect);
    }

    public override void Use()
    {
        base.Use();

        int healPercent = data.effectValue; // 30
        int healAmount = Mathf.CeilToInt(FightManager.Instance.MaxHp * healPercent / 100f);
        FightManager.HealPlayer(healAmount);
        UIManager.Instance.ShowTip($"瓶中精灵触发！回复 {healAmount} 生命", Color.green);
    }
}
