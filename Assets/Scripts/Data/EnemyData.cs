using UnityEngine;

[CreateAssetMenu(fileName = "新敌人", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header(" 基础信息 ")]
    public int id;
    public string enemyName;
    public int maxHp;
    public string modelPath;

    [Header(" 动画 - 通用 ")]
    public string idleAnim;
    public string hitAnim;

    [Header(" 攻击配置 ")]
    public int attack;
    public string attackAnim;

    [Header(" 防御配置 ")]
    public int defense;
    public string defenseAnim;
    public string defenseEffectPath;
    public EffectParams defenseEffectParams = new EffectParams();

    [Header(" 飞行配置 ")]
    public string flightAttackAnim;
    public string flightDefenseAnim;
    [Tooltip("是否使用飞行动画")]
    public bool useFlightAnim = false;
    [Range(0f, 1f)]
    [Tooltip("血量低于此比例时切换到飞行状态")]
    public float flightThreshold = 0.5f;
    public string flightIdleAnim;
    public string flightHitAnim;

    [Header(" 回血配置 ")]
    public string healAnim;
    public string healEffectPath;
    public EffectParams healEffectParams = new EffectParams();
}

/// <summary>
/// 特效参数配置
/// </summary>
[System.Serializable]
public class EffectParams
{
    [Tooltip("位置偏移")]
    public Vector3 positionOffset = Vector3.zero;
    
    [Tooltip("旋转偏移（欧拉角）")]
    public Vector3 rotationOffset = Vector3.zero;
    
    [Range(0.1f, 10f)]
    [Tooltip("特效缩放")]
    public float effectScale = 1f;
    
    [Tooltip("持续时间（秒），0表示使用默认值2秒）")]
    public float duration = 2f;
}
