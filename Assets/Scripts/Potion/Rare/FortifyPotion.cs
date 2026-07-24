using UnityEngine;

/// <summary>
/// 坚固药水 - 将当前护盾值变为3倍
/// </summary>
public class FortifyPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        int multiplier = data.effectValue; // 3
        FightManager.Instance.DefenseCount *= multiplier;

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
            fightUI.UpdateDefense();

        UIManager.Instance.ShowTip($"坚固药水：护盾 ×{multiplier}", Color.cyan);
    }
}
