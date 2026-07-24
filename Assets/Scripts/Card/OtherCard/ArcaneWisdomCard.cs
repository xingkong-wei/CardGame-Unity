using UnityEngine;
using UnityEngine.EventSystems;

// 奥术智慧（技能牌）- 抽1张牌，本回合亲和度叠加翻倍
public class ArcaneWisdomCard : CardItem
{
    protected override void OnCardUsed()
    {
        // 提前接管复制，一次性加双倍层数
        int extraStacks = 0;
        if (BuffManager.Instance.HasStatus(StatusType.Duplicate))
        {
            extraStacks = 1;
            BuffManager.Instance.RemoveStatus(StatusType.Duplicate, 1);
        }

        base.OnCardUsed();

        // 抽1张牌（复制时抽2张）
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            int drawCount = 1 + extraStacks;
            fightUI.CreateCardItem(drawCount);
            fightUI.UpdateCardItemPos();
        }

        // 添加奥术智慧Buff（复制时加2层）
        int totalStacks = 1 + extraStacks;
        BuffManager.Instance.AddStatus(StatusType.AffinityDouble, totalStacks, 1);

        int stack = BuffManager.Instance.GetStack(StatusType.AffinityDouble);
        int multiplier = Mathf.RoundToInt(Mathf.Pow(2, stack));
        UIManager.Instance.ShowTip($"奥术智慧：本回合亲和度叠加×{multiplier}", Color.magenta);

        // 播放特效
        Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 2.5f));
        PlayEffect(pos);
    }
}