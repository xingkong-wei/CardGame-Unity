/// <summary>
/// 能量药水 - 本回合获得2点能量
/// </summary>
public class EnergyPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        int amount = data.effectValue;
        FightManager.Instance.CurPowerCount += amount;

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        fightUI?.UpdatePower();
    }
}
