using UnityEngine;

/// <summary>
/// 霜冻之心 - 冰亲和度达到10层时获得10点格挡，然后减半
/// </summary>
public class FrostHeart : RelicBase
{
    private const int THRESHOLD = 10;

    public override void OnAffinityChanged(StatusType type, int currentStack)
    {
        if (type != StatusType.IceAffinity || currentStack < THRESHOLD) return;

        FightManager.Instance.DefenseCount += 10;
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null) fightUI.UpdateDefense();

        int removeAmount = Mathf.FloorToInt(currentStack / 2f);
        BuffManager.Instance.RemoveStatus(StatusType.IceAffinity, removeAmount);
    }
}
