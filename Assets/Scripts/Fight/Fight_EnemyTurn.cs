using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//敌人回合
public class Fight_EnemyTurn : FightUnit
{
    public override void Init()
    {
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            // 安绪浆液：保留手牌，不丢弃
            if (BuffManager.Instance.HasStatus(StatusType.RetainHand))
            {
                BuffManager.Instance.RemoveStatus(StatusType.RetainHand, 1);
                UIManager.Instance.ShowTip("安绪浆液：手牌已保留", Color.green);
            }
            else
            {
                fightUI.RemoveAllCards();
            }
        }
        else
        {
            Debug.LogError("FightUI 未找到");
        }

        //显示敌人回合提示
        UIManager.Instance.ShowTip("敌人回合", new Color(0.7f, 0.1f, 0.1f), delegate ()
        {
            FightManager.Instance.StartCoroutine(EnemyManager.Instance.DoAllEnemyAction());
        });
    }

    public override void OnUpdate()
    {
        
    }
}
