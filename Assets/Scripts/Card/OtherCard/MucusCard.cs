using UnityEngine;

/// <summary>
/// 黏液 - 状态卡牌（消耗），抽一张牌
/// </summary>
public class MucusCard : CardItem
{
    protected override void OnCardUsed()
    {
        base.OnCardUsed();

        int drawCount = data != null ? GetArg0() : 1;

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.CreateCardItem(drawCount);
            fightUI.UpdateCardItemPos();
        }

        Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 2.5f));
        PlayEffect(pos);
    }
}
