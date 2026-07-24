using UnityEngine;
using System.Collections.Generic;

namespace BuffConfig
{
    /// <summary>
    /// Buff状态配置 - ScriptableObject格式
    /// 用于在Unity Inspector中可视化配置每个Buff的显示信息
    /// </summary>
    [CreateAssetMenu(fileName = "新Buff配置", menuName = "Buff/StatusEffectConfig")]
    public class StatusEffectConfig : ScriptableObject
    {
        [Header("基础配置")]
        public StatusType statusType;
        
        [Header("显示配置")]
        public string displayName;        // 显示名称
        [TextArea(2, 4)]
        public string description;       // 描述文本
        public string iconPath;          // 图标资源路径
        public StatusDisplayType displayType;  // 显示类型（Buff/Debuff/Special）
        
        [Header("层数显示格式")]
        [Tooltip("描述中层数的格式，例如：+{0} 伤害，{0}层 等")]
        public string stackFormat = "+{0}";
        

        
        /// <summary>
        /// 根据当前层数生成描述
        /// </summary>
        public string GetFormattedDescription(int stack)
        {
            if (string.IsNullOrEmpty(stackFormat))
                return description;
            return string.Format(description, string.Format(stackFormat, stack));
        }
        
        /// <summary>
        /// 创建StatusEffect实例
        /// </summary>
        public StatusEffect CreateStatusEffect(int stack = 1, int duration = -1)
        {
            StatusEffect effect = new StatusEffect(statusType, stack, duration);
            effect.effectName = displayName;
            effect.description = GetFormattedDescription(stack);
            effect.iconPath = iconPath;
            effect.displayType = displayType;
            return effect;
        }
    }
    
    /// <summary>
    /// Buff配置数据管理器
    /// 集中管理所有Buff配置
    /// </summary>
    public class BuffConfigManager
    {
        private static BuffConfigManager _instance;
        public static BuffConfigManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BuffConfigManager();
                    _instance.LoadAllConfigs();
                }
                return _instance;
            }
        }
        
        private Dictionary<StatusType, StatusEffectConfig> configDict = new Dictionary<StatusType, StatusEffectConfig>();
        
        /// <summary>
        /// 加载所有Buff配置
        /// </summary>
        private void LoadAllConfigs()
        {
            // 支持多个路径
            string[] paths = { "Config/Buff", "Data_Ability" };
            
            foreach (var path in paths)
            {
                StatusEffectConfig[] configs = Resources.LoadAll<StatusEffectConfig>(path);
                foreach (var config in configs)
                {
                    if (!configDict.ContainsKey(config.statusType))
                    {
                        configDict.Add(config.statusType, config);
                    }
                    else
                    {
                        Debug.LogWarning($"[BuffConfigManager] 发现重复配置: {config.statusType}");
                    }
                }
            }
        }
        
        /// <summary>
        /// 获取指定类型的配置
        /// </summary>
        public StatusEffectConfig GetConfig(StatusType type)
        {
            if (configDict.TryGetValue(type, out var config))
            {
                return config;
            }
            return null;
        }
        
        /// <summary>
        /// 获取指定类型的显示名称
        /// </summary>
        public string GetDisplayName(StatusType type)
        {
            var config = GetConfig(type);
            return config != null ? config.displayName : type.ToString();
        }
        
        /// <summary>
        /// 获取指定类型的图标路径
        /// </summary>
        public string GetIconPath(StatusType type)
        {
            var config = GetConfig(type);
            return config != null ? config.iconPath : string.Empty;
        }
        
        /// <summary>
        /// 获取指定类型的显示类型
        /// </summary>
        public StatusDisplayType GetDisplayType(StatusType type)
        {
            var config = GetConfig(type);
            return config != null ? config.displayType : StatusDisplayType.Special;
        }
    }
}
