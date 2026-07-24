using UnityEngine;

/// <summary>
/// 药水效果基类 - 所有药水脚本继承此类
/// 通过 scriptName 反射创建实例
/// </summary>
public abstract class PotionBase
{
    /// <summary>药水的数据配置</summary>
    protected PotionData data;

    /// <summary>药水按钮在屏幕上的位置（用于 LineUI 起点）</summary>
    public Vector2 potionBtnScreenPos;

    /// <summary>
    /// 初始化药水数据
    /// </summary>
    public virtual void Init(PotionData potionData)
    {
        data = potionData;
    }

    /// <summary>
    /// 使用药水，执行具体效果
    /// </summary>
    public virtual void Use()
    {
        if (data == null)
        {
            Debug.LogError("PotionBase.Use: data is null");
            return;
        }

        // 播放音效
        if (!string.IsNullOrEmpty(data.useSound))
        {
            AudioManager.Instance?.PlayEffect(data.useSound);
        }

        // 播放特效
        if (!string.IsNullOrEmpty(data.useEffect))
        {
            PlayEffect(data.useEffect);
        }

        // 注册使用后图标
        if (data.usedIconConfig != null)
        {
            SpireBuffUI.AddUsedPotion(data.usedIconConfig);
        }
    }

    /// <summary>
    /// 播放药水特效
    /// </summary>
    protected void PlayEffect(string effectPath)
    {
        GameObject effectPrefab = Resources.Load<GameObject>(effectPath);
        if (effectPrefab != null)
        {
            Object.Instantiate(effectPrefab);
        }
    }
}
