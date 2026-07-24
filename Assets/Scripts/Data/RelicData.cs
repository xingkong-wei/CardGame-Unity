using UnityEngine;

#region 遗物相关枚举

/// <summary>
/// 遗物稀有度，决定了获取途径
/// </summary>
public enum RelicRarity
{
    /// <summary>初始</summary>
    Starter,
    /// <summary>普通</summary>
    Common,
    /// <summary>罕见</summary>
    Uncommon,
    /// <summary>稀有</summary>
    Rare,
    /// <summary>商店</summary>
    Shop,
    /// <summary>事件</summary>
    Event,
    /// <summary>先古</summary>
    Ancient,
}

#endregion

[CreateAssetMenu(fileName = "新遗物", menuName = "Relic/RelicData")]
public class RelicData : ScriptableObject
{
    [Header(" 基础信息 ")]
    [Tooltip("遗物唯一标识符")]
    public string id;
    [Tooltip("遗物显示名称")]
    public string relicName;
    [TextArea]
    [Tooltip("遗物效果描述")]
    public string description;

    [Header(" 稀有度与获取 ")]
    public RelicRarity rarity;

    [Header(" 经济数值 ")]
    [Tooltip("购买价格")]
    public int price;

    [Header(" 资源路径 ")]
    [Tooltip("图标 Sprite")]
    public Sprite sprite;

    [Header(" 脚本绑定 ")]
    [Tooltip("执行效果的脚本类名（如 AbacusRelic / BurningBloodRelic 等）")]
    public string scriptName;
}
