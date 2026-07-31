using UnityEngine;

/// <summary>
/// 过载火花 - 火亲和度达到10层时对随机敌人造成12点伤害，然后减半
/// </summary>
public class OverloadSpark : RelicBase
{
    private const int THRESHOLD = 10;

    public override void OnAffinityChanged(StatusType type, int currentStack)
    {
        if (type != StatusType.FireAffinity || currentStack < THRESHOLD) return;

        Enemy target = GetRandomAliveEnemy();
        if (target != null) target.Hit(12);

        int removeAmount = Mathf.FloorToInt(currentStack / 2f);
        BuffManager.Instance.RemoveStatus(StatusType.FireAffinity, removeAmount);
    }

    private Enemy GetRandomAliveEnemy()
    {
        var alive = EnemyManager.Instance.GetAliveEnemies();
        return alive.Count > 0 ? alive[Random.Range(0, alive.Count)] : null;
    }
}
