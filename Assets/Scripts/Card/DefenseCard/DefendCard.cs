using UnityEngine;
using UnityEngine.EventSystems;

// 防御卡 - 拖拽使用，增加护盾
public class DefendCard : CardItem
{
    protected override void OnCardUsed()
    {
        base.OnCardUsed();

        int val = data != null ? GetArg0() : 0;
        AudioManager.Instance.PlayEffect("Effect/healspell");

        // 应用护甲修正（脆弱/锁甲）
        int actualGain = BuffManager.Instance.ModifyDefenseGain(val);
        FightManager.Instance.DefenseCount += actualGain;

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.UpdateDefense();
        }

        Vector3 pos = Camera.main.transform.position;
        pos.y = 0;
        PlayEffect(pos);
    }
}
