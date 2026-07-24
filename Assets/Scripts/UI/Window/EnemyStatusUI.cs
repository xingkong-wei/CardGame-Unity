using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 敌人状态图标UI - 显示在敌人头顶的血条下面
/// 订阅对应Enemy的状态变化事件，显示该敌人的独有buff
/// </summary>
public class EnemyStatusUI : MonoBehaviour
{
    [Header("图标预制体")]
    public GameObject buffIconPrefab;
    
    [Header("容器设置")]
    public RectTransform iconContainer;
    
    [Header("图标设置")]
    public float iconSpacing = 28f;      // 图标间距
    public float iconSize = 32f;        // 图标尺寸（小一点，适合显示在血条下）
    public int maxIconsPerRow = 5;     // 每行最大图标数
    
    private List<GameObject> buffIcons = new List<GameObject>();
    private Dictionary<StatusType, int> currentStatus = new Dictionary<StatusType, int>();
    private Enemy targetEnemy;
    
    /// <summary>
    /// 初始化：绑定到指定敌人
    /// </summary>
    public void Initialize(Enemy enemy)
    {
        // 取消之前的订阅
        if (targetEnemy != null)
        {
            targetEnemy.OnStatusChanged -= OnEnemyStatusChanged;
        }
        
        targetEnemy = enemy;
        
        // 订阅新的敌人状态变化
        if (targetEnemy != null)
        {
            targetEnemy.OnStatusChanged += OnEnemyStatusChanged;
            
            // 初始化当前状态
            RefreshFromEnemy();
        }
    }
    
    /// <summary>
    /// 从敌人同步当前状态
    /// </summary>
    private void RefreshFromEnemy()
    {
        if (targetEnemy == null) return;
        
        currentStatus.Clear();
        
        // 使用Enemy提供的公开方法获取所有状态
        var allStatus = targetEnemy.GetAllStatus();
        foreach (var kvp in allStatus)
        {
            currentStatus[kvp.Key] = kvp.Value;
        }
        
        RebuildIcons();
    }
    
    /// <summary>
    /// 敌人状态变化回调
    /// </summary>
    private void OnEnemyStatusChanged(StatusType type, int stack, bool isAdded)
    {
        if (isAdded)
        {
            currentStatus[type] = stack;
        }
        else
        {
            currentStatus.Remove(type);
        }
        
        RebuildIcons();
    }
    
    /// <summary>
    /// 刷新UI显示
    /// </summary>
    public void RefreshUI()
    {
        RebuildIcons();
    }
    
    /// <summary>
    /// 重新构建所有图标
    /// </summary>
    private void RebuildIcons()
    {
        ClearIcons();
        
        int index = 0;
        foreach (var kvp in currentStatus)
        {
            if (kvp.Value > 0)
            {
                CreateIcon(kvp.Key, kvp.Value, index);
                index++;
            }
        }
    }
    
    /// <summary>
    /// 创建单个Buff图标
    /// </summary>
    private void CreateIcon(StatusType type, int stack, int index)
    {
        if (buffIconPrefab == null || iconContainer == null) return;
        
        GameObject iconObj = Instantiate(buffIconPrefab, iconContainer);
        buffIcons.Add(iconObj);
        
        // 计算位置（横向排列，从左到右）
        int col = index % maxIconsPerRow;
        float xPos = col * (iconSize + iconSpacing);
        float yPos = 0f;
        
        RectTransform iconRT = iconObj.GetComponent<RectTransform>();
        if (iconRT != null)
        {
            // 使用绝对定位
            iconRT.anchorMin = Vector2.zero;
            iconRT.anchorMax = Vector2.zero;
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.anchoredPosition = new Vector2(xPos, yPos);
            iconRT.sizeDelta = new Vector2(iconSize, iconSize);
        }
        
        // 设置图标内容
        BuffIcon icon = iconObj.GetComponent<BuffIcon>();
        if (icon != null)
        {
            StatusEffect effect = new StatusEffect(type, stack, -1);
            effect.effectName = GetStatusDisplayName(type);
            effect.iconPath = GetStatusIconPath(type);
            icon.Setup(effect);
        }
    }
    
    /// <summary>
    /// 获取状态显示名称
    /// </summary>
    private string GetStatusDisplayName(StatusType type)
    {
        // 优先从配置获取
        var config = BuffConfig.BuffConfigManager.Instance.GetConfig(type);
        if (config != null)
            return config.displayName;
        
        // 默认名称
        switch (type)
        {
            case StatusType.Weak: return "虚弱";
            case StatusType.Vulnerable: return "易伤";
            case StatusType.Frail: return "脆弱";
            case StatusType.Poison: return "中毒";
            case StatusType.Burning: return "燃烧";
            case StatusType.Bleeding: return "流血";
            default: return type.ToString();
        }
    }
    
    /// <summary>
    /// 获取状态图标路径
    /// </summary>
    private string GetStatusIconPath(StatusType type)
    {
        var config = BuffConfig.BuffConfigManager.Instance.GetConfig(type);
        return config != null ? config.iconPath : string.Empty;
    }
    
    /// <summary>
    /// 清除所有图标
    /// </summary>
    private void ClearIcons()
    {
        foreach (var icon in buffIcons)
        {
            if (icon != null)
                Destroy(icon);
        }
        buffIcons.Clear();
    }
    
    private void OnDestroy()
    {
        // 取消订阅
        if (targetEnemy != null)
        {
            targetEnemy.OnStatusChanged -= OnEnemyStatusChanged;
        }
    }
    
    /// <summary>
    /// 显示/隐藏面板
    /// </summary>
    public void Show(bool show)
    {
        gameObject.SetActive(show);
    }
}
