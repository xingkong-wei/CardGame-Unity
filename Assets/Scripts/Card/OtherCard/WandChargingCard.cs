using UnityEngine;

/// <summary>
/// 魔杖充能 - 消耗型
/// 获得两点能量，本回合下次攻击造成双倍伤害
/// </summary>
public class WandChargingCard : CardItem
{
    protected override void OnCardUsed()
    {
        // 提前接管复制，一次性加双倍层数
        int extraStacks = 0;
        if (BuffManager.Instance.HasStatus(StatusType.Duplicate))
        {
            extraStacks = 1;
            BuffManager.Instance.RemoveStatus(StatusType.Duplicate, 1);
        }

        base.OnCardUsed();

        // 获得能量（复制时双倍）
        int energyGain = (data != null ? GetArg0() : 2) * (1 + extraStacks);
        FightManager.Instance.CurPowerCount += energyGain;

        // 添加魔杖充能Buff（复制时2层）
        BuffManager.Instance.AddStatus(StatusType.WandCharging, 1 + extraStacks, 1);

        // 更新UI
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.UpdatePower();
        }

        // 显示提示
        UIManager.Instance.ShowTip($"获得{energyGain}点能量", new Color(0.3f, 0.6f, 1f));

        // 播放特效
        PlayEffect(transform.position);
    }
}
