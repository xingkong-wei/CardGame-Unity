/// <summary>
/// 禅定药水 - 抽3张牌
/// </summary>
public class MeditationPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI == null) return;

        int drawCount = data.effectValue;
        fightUI.CreateCardItem(drawCount);
        fightUI.UpdateCardItemPos();
        fightUI.UpdateCardCount();
    }
}
