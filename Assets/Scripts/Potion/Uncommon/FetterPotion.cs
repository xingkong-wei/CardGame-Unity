using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 桎梏魔药 - 所有敌人获得7层枷锁
/// </summary>
public class FetterPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        int stacks = data.effectValue;
        var allEnemies = EnemyManager.Instance.GetAliveEnemies();
        int count = 0;
        foreach (Enemy e in allEnemies)
        {
            e.AddStatus(StatusType.Fetter, stacks);
            count++;
        }
        if (count > 0)
            UIManager.Instance.ShowTip($"所有敌人枷锁 +{stacks}", Color.magenta);
    }
}
