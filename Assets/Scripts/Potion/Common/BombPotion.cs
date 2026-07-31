using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 烈爆安瓿 - 对所有敌人造成伤害
/// </summary>
public class BombPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        List<Enemy> aliveEnemies = GetAliveEnemies();

        if (aliveEnemies.Count == 0)
        {
            Debug.LogWarning("[BombPotion] 没有存活的敌人！");
            return;
        }

        int damage = data.effectValue;
        foreach (var enemy in aliveEnemies)
        {
            enemy.Hit(damage);
        }

        UIManager.Instance.ShowTip($"对所有敌人造成 {damage} 点伤害",
            new Color(1f, 0.5f, 0.1f));
    }

    private List<Enemy> GetAliveEnemies()
    {
        return EnemyManager.Instance.GetAliveEnemies();
    }
}
