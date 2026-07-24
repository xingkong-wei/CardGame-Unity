using UnityEngine;

// 极地风暴（能力牌）- 每次获得或消耗冰亲和度，对所有敌人造成2点伤害
public class PolarStormCard : AbilityCard
{
    protected override void OnCardUsed()
    {
        base.OnCardUsed();

        // 添加极地风暴状态（永久）
        BuffManager.Instance.AddStatus(StatusType.PolarStorm, 1, -1);

        // 显示提示
        UIManager.Instance.ShowTip("获得极地风暴能力", Color.cyan);

        // 播放特效
        PlayAbilityEffect();
    }
}
