/// <summary>
/// 共鸣石 - 三种亲和度均≥5层时，每回合多抽1张牌
/// </summary>
public class ResonanceStone : RelicBase
{
    public override void OnTurnStart()
    {
        int fire = BuffManager.Instance.GetStack(StatusType.FireAffinity);
        int ice = BuffManager.Instance.GetStack(StatusType.IceAffinity);
        int lightning = BuffManager.Instance.GetStack(StatusType.LightningAffinity);

        if (fire >= 5 && ice >= 5 && lightning >= 5)
        {
            FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
            if (fightUI != null)
            {
                fightUI.CreateCardItem(1);
                fightUI.UpdateCardItemPos();
            }
        }
    }
}
