using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单个攻击特效配置
/// </summary>
[Serializable]
public class AttackEffectData
{
    [Tooltip("特效名称（与 Animator 参数名对应，如 Bite Attack）")]
    public string effectName;
    
    [Tooltip("特效预制体路径（Resources 路径）")]
    public string effectPrefabPath;
    
    [Tooltip("特效生成位置类型：0=敌人脚下, 1=敌人中心, 2=敌人头顶, 3=屏幕中心, 4=指定子物体")]
    public int spawnPositionType = 0;
    
    [Tooltip("子物体路径（当 spawnPositionType=4 时使用，如 Hips/Spine/Chest/Head）")]
    public string spawnTransformPath;
    
    [Tooltip("特效相对位置偏移")]
    public Vector3 positionOffset = Vector3.zero;
    
    [Tooltip("特效缩放")]
    public float effectScale = 1f;
    
    [Tooltip("特效延迟播放时间（秒）")]
    public float delayTime = 0f;
    
    [Tooltip("特效持续时间（秒），0表示使用预制体自身销毁时间")]
    public float duration = 2f;
    
    [Tooltip("特效旋转角度")]
    public Vector3 rotationOffset = Vector3.zero;
}

/// <summary>
/// 怪物攻击特效配置数据（ScriptableObject）
/// </summary>
[CreateAssetMenu(fileName = "AttackEffectConfig", menuName = "Config/AttackEffectConfig", order = 1)]
public class AttackEffectConfig : ScriptableObject
{
    [Header("配置信息")]
    [Tooltip("配置版本")]
    public string version = "1.0";
    
    [Tooltip("配置描述")]
    public string description;
    
    [Header("攻击特效列表")]
    [Tooltip("每个 Animator 参数对应的特效配置")]
    public List<AttackEffectData> attackEffects = new List<AttackEffectData>();
    
    /// <summary>
    /// 根据特效名称获取特效配置
    /// </summary>
    public AttackEffectData GetEffectByName(string effectName)
    {
        if (string.IsNullOrEmpty(effectName))
            return null;
        
        return attackEffects.Find(e => e.effectName == effectName);
    }
    
    /// <summary>
    /// 获取所有特效名称列表
    /// </summary>
    public List<string> GetAllEffectNames()
    {
        List<string> names = new List<string>();
        foreach (var effect in attackEffects)
        {
            if (!string.IsNullOrEmpty(effect.effectName))
                names.Add(effect.effectName);
        }
        return names;
    }
}
