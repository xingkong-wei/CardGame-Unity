using UnityEngine;

/// <summary>
/// 圣辉酊液 - 获得1点能量，在下3个回合开始时额外获得1点能量
/// </summary>
public class HolyTinctPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        // 立即获得1点能量
        int amount = data.effectValue;
        FightManager.Instance.CurPowerCount += amount;
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        fightUI?.UpdatePower();

        // 获得3层圣辉酊液
        BuffManager.Instance.AddStatus(StatusType.HolyTinct, 3);
    }
}
