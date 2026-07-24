using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 逆乱原液 - 在所有空药水栏中获得随机药水
/// </summary>
public class PervertedEssencePotion : PotionBase
{
    private const int MAX_POTION_SLOTS = 3;

    public override void Use()
    {
        base.Use();

        var potionList = FightManager.Instance.potionList;
        // +1：逆乱原液自身即将被移除，那个位置也算空位
        int emptySlots = MAX_POTION_SLOTS - potionList.Count + 1;

        // 加载所有药水，排除瓶中精灵（无法主动使用/无意义的被动药水）
        PotionData[] allPotions = Resources.LoadAll<PotionData>("Date_Potion");
        List<PotionData> pool = new List<PotionData>();
        foreach (var p in allPotions)
        {
            if (p != null && p.scriptName != "BottledSpritePotion" && p.scriptName != "PervertedEssencePotion")
                pool.Add(p);
        }

        if (pool.Count == 0) return;

        for (int i = 0; i < emptySlots; i++)
        {
            int idx = Random.Range(0, pool.Count);
            potionList.Add(pool[idx]);
        }

        // 刷新药水UI
        PotionPanelController ppc = Object.FindObjectOfType<PotionPanelController>();
        if (ppc != null) ppc.RefreshPotionButtons();

        UIManager.Instance.ShowTip($"逆乱原液：获得 {emptySlots} 瓶随机药水", Color.magenta);
    }
}
