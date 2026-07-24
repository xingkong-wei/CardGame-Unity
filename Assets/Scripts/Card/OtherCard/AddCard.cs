using UnityEngine;
using UnityEngine.EventSystems;

// 无中生有卡 - 拖拽使用，抽卡效果
public class AddCard : CardItem
{
    protected override void OnCardUsed()
    {
        base.OnCardUsed();

        int val = data != null ? GetArg0() : 0;

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.CreateCardItem(val);
            fightUI.UpdateCardItemPos();
        }

        Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 2.5f));
        PlayEffect(pos);
    }
}
