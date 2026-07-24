using UnityEngine;

/// <summary>
/// 魔力虹吸 - 击杀敌人时，获得该敌人最大生命值5%层数的随机一种元素亲和度（至少1层）
/// </summary>
public class ManaSiphon : RelicBase
{
    private const float HP_PERCENT = 0.05f;

    public override void OnEnemyKilled(Enemy enemy)
    {
        if (enemy == null) return;

        int stacks = Mathf.Max(1, Mathf.CeilToInt(enemy.MaxHp * HP_PERCENT));

        StatusType randomType = Random.Range(0, 3) switch
        {
            0 => StatusType.FireAffinity,
            1 => StatusType.IceAffinity,
            _ => StatusType.LightningAffinity,
        };

        BuffManager.Instance.AddStatus(randomType, stacks);
    }
}
