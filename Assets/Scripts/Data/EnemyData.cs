using UnityEngine;

/// <summary>
/// 敌人等级分类
/// </summary>
public enum EnemyTier
{
    Normal,  // 普通敌人（小怪节点、？节点）
    Elite,   // 精英敌人（精英节点）
    Boss     // Boss（Boss节点）
}

[CreateAssetMenu(fileName = "新敌人", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header(" 基础信息 ")]
    public int id;
    public string enemyName;
    [Tooltip("最大生命值")]
    public int maxHp;
    public string modelPath;
    [Tooltip("登场时自带的护盾值")]
    public int initialDefense;

    [Header(" 等级分类 ")]
    [Tooltip("敌人等级：普通/精英/Boss")]
    public EnemyTier tier = EnemyTier.Normal;

    [Header(" 脚本绑定 ")]
    [Tooltip("敌人专属脚本类名（如 SlimeEnemy / TurtleShellEnemy），留空使用基类 Enemy")]
    public string scriptName;

    [Header(" 动画 - 通用 ")]
    public string idleAnim;
    public string hitAnim;

    [Header(" 攻击配置 ")]
    [Tooltip("基础攻击力（实际值为此值的 80%~120%）。如果配置了「多段攻击伤害」，则各动画使用对应值")]
    public int attack;
    public string attackAnim;
    [Range(0f, 1f)]
    [Tooltip("攻击力随机波动比例（0=固定，0.2=±20%）")]
    public float attackVariance = 0.2f;
    [Tooltip("按动画名配置不同伤害值，key=动画名, value=伤害。留空则所有攻击使用上方 attack")]
    public AttackDamageEntry[] attackDamages;

    [Header(" 防御配置 ")]
    [Tooltip("防御动作时获得的护盾值（实际值为此值的 80%~120%）")]
    public int defense;
    public string defenseAnim;
    public string defenseEffectPath;
    public EffectParams defenseEffectParams = new EffectParams();
    [Range(0f, 1f)]
    [Tooltip("护盾随机波动比例（0=固定，0.2=±20%）")]
    public float defenseVariance = 0.2f;
    

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
/// 攻击伤害配置（按动画名映射）
/// </summary>
[System.Serializable]
public class AttackDamageEntry
{
    [Tooltip("对应攻击动画名")]
    public string animName;
    [Tooltip("伤害值")]
    public int damage;
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
    
    [Tooltip("持续时间（秒），0表示使用默认值2秒")]
    public float duration = 2f;
}
