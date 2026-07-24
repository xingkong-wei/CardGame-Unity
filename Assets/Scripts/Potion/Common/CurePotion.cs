using UnityEngine;

/// <summary>
/// 治愈药水 - 恢复最大生命值的20%
/// </summary>
public class CurePotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        int maxHp = FightManager.Instance.MaxHp;
        int healAmount = Mathf.CeilToInt(maxHp * data.effectValue / 100f);

        int oldHp = FightManager.Instance.CurHp;
        FightManager.Instance.CurHp = Mathf.Min(oldHp + healAmount, maxHp);
        int actualHeal = FightManager.Instance.CurHp - oldHp;

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        fightUI?.UpdateHp();

        UIManager.Instance.ShowTip($"恢复 {actualHeal} 点生命", new Color(0.4f, 1f, 0.4f));
    }
}
