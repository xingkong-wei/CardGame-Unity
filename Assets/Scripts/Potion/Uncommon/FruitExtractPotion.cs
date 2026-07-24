using UnityEngine;

/// <summary>
/// 鲜萃果液 - 永久获得5点最大生命值（整局游戏有效）
/// </summary>
public class FruitExtractPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        int amount = data.effectValue; // 5
        FightManager.Instance.AddMaxHp(amount);
        UIManager.Instance.ShowTip($"鲜萃果液：最大生命值 +{amount}", Color.green);
    }
}
