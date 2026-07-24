/// <summary>
/// 启明凝露 - 抽1张牌，获得4层启明凝露，在下3个回合开始时额外抽1张牌
/// </summary>
public class DawnCondensedDewPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        // 立即抽1张牌
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.CreateCardItem(1);
            fightUI.UpdateCardItemPos();
            fightUI.UpdateCardCount();
        }

        // 获得4层启明凝露（4层 = 下3回合各触发1次额外抽牌）
        BuffManager.Instance.AddStatus(StatusType.DawnCondensedDew, 4);
    }
}
