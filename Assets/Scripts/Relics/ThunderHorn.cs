using UnityEngine;

/// <summary>
/// 雷霆号角 - 雷亲和度达到5层时抽3张牌，然后减半
/// </summary>
public class ThunderHorn : RelicBase
{
    private const int THRESHOLD = 5;

    public override void OnAffinityChanged(StatusType type, int currentStack)
    {
        if (type != StatusType.LightningAffinity || currentStack < THRESHOLD) return;

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.CreateCardItem(3);
            fightUI.UpdateCardItemPos();
        }

        int removeAmount = Mathf.FloorToInt(currentStack / 2f);
        BuffManager.Instance.RemoveStatus(StatusType.LightningAffinity, removeAmount);
    }
}
