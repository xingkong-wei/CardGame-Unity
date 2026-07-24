using UnityEngine;

// 玄甲固守 - 增加护盾，具有消耗类型（使用后进入废牌堆）
public class UnmovingCard : CardItem
{
    protected override void OnCardUsed()
    {
        base.OnCardUsed();

        int val = data != null ? GetArg0() : 0;
        AudioManager.Instance.PlayEffect("Effect/healspell");

        // 应用护甲修正（锁甲/脆弱/敏捷）
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
