/// <summary>
/// 溯流之泉 - 获得1点能量，抽2张牌
/// </summary>
public class UpstreamPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        // 获得能量
        int energyGain = data.effectValue;
        FightManager.Instance.CurPowerCount += energyGain;

        // 抽2张牌
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.UpdatePower();
            fightUI.CreateCardItem(2);
            fightUI.UpdateCardItemPos();
            fightUI.UpdateCardCount();
        }
    }
}
