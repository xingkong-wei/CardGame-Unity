using UnityEngine;
using UnityEngine.EventSystems;

// 绝对零度 - 获得12点格挡，消耗所有冰亲和度，每消耗1层额外+2格挡
public class AbsoluteZeroCard : CardItem
{
    protected override void OnCardUsed()
    {
        base.OnCardUsed();

        AudioManager.Instance.PlayEffect("Effect/healspell");

        // 基础格挡值
        int baseDefense = data != null ? GetArg0() : 12;

        // 消耗所有冰亲和度，每层格挡（基础+2，升级+3）
        int iceAffinity = BuffManager.Instance.GetStack(StatusType.IceAffinity);
        if (iceAffinity > 0)
        {
            BuffManager.Instance.RemoveStatus(StatusType.IceAffinity, iceAffinity);
            UIManager.Instance.ShowTip($"消耗 {iceAffinity} 层冰亲和度", Color.cyan);
        }

        // 计算总格挡值
        int bonusPerAffinity = IsUpgraded() ? 3 : 2;
        int bonusDefense = iceAffinity * bonusPerAffinity;
        int totalDefense = baseDefense + bonusDefense;

        // 应用护甲修正（脆弱/锁甲）
        int actualGain = BuffManager.Instance.ModifyDefenseGain(totalDefense);
        FightManager.Instance.DefenseCount += actualGain;

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.UpdateDefense();
        }

        // 播放特效
        Vector3 pos = Camera.main.transform.position;
        pos.y = 0;
        PlayEffect(pos);
    }
}
