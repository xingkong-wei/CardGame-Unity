using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 药水库存管理器
/// 负责：药水列表、初始化、瓶中精灵检测
/// </summary>
public class PotionInventory
{
    public List<PotionData> PotionList = new List<PotionData>();

    public void Init()
    {
        PotionList.Clear();
        var cfg = GameConfig.Instance;
        if (cfg.initialPotions != null)
        {
            foreach (var potion in cfg.initialPotions)
            {
                if (potion != null) PotionList.Add(potion);
            }
        }
    }

    /// <summary>
    /// 检查并触发瓶中精灵（死亡时自动使用）
    /// </summary>
    public bool TryUseBottledSprite()
    {
        for (int i = PotionList.Count - 1; i >= 0; i--)
        {
            if (PotionList[i] != null && PotionList[i].scriptName == "BottledSpritePotion")
            {
                PotionData spriteData = PotionList[i];
                PotionList.RemoveAt(i);

                BottledSpritePotion sprite = new BottledSpritePotion();
                sprite.Init(spriteData);
                sprite.UseBaseEffects();

                if (FightManager.CachedPotionPanel != null)
                    FightManager.CachedPotionPanel.RefreshPotionButtons();

                UIManager.Instance.ShowTip("瓶中精灵触发！", Color.green);
                return true;
            }
        }
        return false;
    }
}
