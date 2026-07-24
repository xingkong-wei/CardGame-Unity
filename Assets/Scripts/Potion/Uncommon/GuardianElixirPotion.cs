/// <summary>
/// 守护神汁 - 获得10点护盾，下回合开始再获得10点护盾
/// </summary>
public class GuardianElixirPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        int amount = data.effectValue;
        FightManager.Instance.DefenseCount += amount;

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.UpdateDefense();
        }

        BuffManager.Instance.AddStatus(StatusType.GuardianElixir, 1);
    }
}
