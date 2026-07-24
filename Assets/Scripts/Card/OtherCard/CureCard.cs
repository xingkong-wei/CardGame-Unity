using UnityEngine;
using UnityEngine.EventSystems;

// 治愈卡 - 拖拽使用，恢复生命
public class CureCard : CardItem
{
    protected override void OnCardUsed()
    {
        base.OnCardUsed();

        int val = data != null ? GetArg0() : 0;
        AudioManager.Instance.PlayEffect("Effect/healspell");

        int oldHp = FightManager.Instance.CurHp;
        FightManager.Instance.CurHp += val;
        if (FightManager.Instance.CurHp > FightManager.Instance.MaxHp)
        {
            FightManager.Instance.CurHp = FightManager.Instance.MaxHp;
        }
        int healed = FightManager.Instance.CurHp - oldHp;
        Debug.Log($"治愈 {healed} 点生命，当前血量: {FightManager.Instance.CurHp}/{FightManager.Instance.MaxHp}");

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.UpdateHp();
        }

        Vector3 pos = Camera.main.transform.position;
        pos.y = 0;
        PlayEffect(pos);
    }
}
