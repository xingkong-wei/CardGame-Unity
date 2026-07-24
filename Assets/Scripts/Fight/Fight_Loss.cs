using DG.Tweening;
using UnityEngine;

public class Fight_Loss : FightUnit
{
    public override void Init()
    {
        Debug.Log("DEFEAT");
        FightManager.Instance.StopAllCoroutines();

        // 重置战斗计时器
        FightUI.ResetBattleTimer();

        // 重置血量（失败后重新开始会重置）
        FightManager.ResetHp();

        AudioManager.Instance.PlayBGM("Loss", true);
        UIManager.Instance.ShowUI<LossUI>("LossUI");
    }
}