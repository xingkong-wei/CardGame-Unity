using UnityEngine;

#region 药水相关枚举

/// <summary>
/// 药水稀有度
/// </summary>
public enum PotionRarity
{
    /// <summary>普通</summary>
    Common,
    /// <summary>罕见</summary>
    Uncommon,
    /// <summary>稀有</summary>
    Rare,
}

/// <summary>
/// 药水目标类型
/// </summary>
public enum PotionTarget
{
    /// <summary>自身</summary>
    Self,
    /// <summary>单个敌人</summary>
    SingleEnemy,
    /// <summary>全体敌人</summary>
    AllEnemies,
    /// <summary>全体角色（含玩家和敌人）</summary>
    AllCharacters,
}

#endregion

[CreateAssetMenu(fileName = "新药水", menuName = "Potion/PotionData")]
public class PotionData : ScriptableObject
{
    [Header(" 基础信息 ")]
    public string potionName;
    [TextArea]
    public string description;
    public string icon;
    /// <summary>执行效果的脚本类名（如 CurePotion / FirePotion 等）</summary>
    public string scriptName;

    [Header(" 经济数值 ")]
    public int buyPrice;
    public int sellPrice;
    public PotionRarity rarity;

    [Header(" 效果数值 ")]
    /// <summary>持续回合 = -1 表示永久</summary>
    public const int DURATION_PERMANENT = -1;
    /// <summary>持续回合 = 0 表示一次性（立即生效，无持续）</summary>
    public const int DURATION_INSTANT = 0;

    public int effectValue;
    [Tooltip("持续回合数，0 = 一次性，-1 = 永久")]
    public int duration;
    public PotionTarget target;

    [Header(" 使用条件 ")]
    public int requiredEnergy;
    [Tooltip("是否可以在战斗中使用")]
    public bool canUseInCombat = true;
    [Tooltip("是否可以在大地图界面使用")]
    public bool canUseInMap = true;
    [Tooltip("当生命/能量已满时，是否允许使用")]
    public bool canUseWhenFull;

    [Header(" 特殊效果 ")]
    public string useSound;
    public string useEffect;

    [Header(" 使用后图标 ")]
    /// <summary>
    /// 使用后显示在 SpireBuffUI 的 Buff 配置（引用 Data_Ability 下的 .asset）
    /// 留空表示使用后不需要显示图标
    /// </summary>
    [Tooltip("使用后Buff图标配置，引用 Data_Ability 下的 Buff 资产，留空则不显示")]
    public BuffConfig.StatusEffectConfig usedIconConfig;
}
