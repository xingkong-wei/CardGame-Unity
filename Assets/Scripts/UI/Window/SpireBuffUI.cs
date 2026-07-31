using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 战斗Buff图标面板 - 杀戮尖塔风格
/// 特点：图标横向排列，紧凑简洁，无文字标题
/// 同时支持显示已用药水的图标
/// </summary>
public class SpireBuffUI : MonoBehaviour
{
    [Header("图标预制体")]
    public GameObject buffIconPrefab;

    [Header("容器设置")]
    public RectTransform iconContainer;

    [Header("图标设置")]
    public float iconSpacing = 8f;      // 图标间距
    public float iconSize = 50f;        // 图标尺寸
    public int maxIconsPerRow = 10;     // 每行最大图标数

    private List<GameObject> buffIcons = new List<GameObject>();
    private List<StatusEffect> currentBuffs = new List<StatusEffect>();

    // ---- 已用药水图标追踪 ----
    private struct PotionIconData
    {
        public string iconPath;
        public string potionName;
        public string description;
    }
    private static List<PotionIconData> usedPotionIcons = new List<PotionIconData>();
    private static List<SpireBuffUI> allInstances = new List<SpireBuffUI>();

    /// <summary>
    /// 注册已用药水的图标（由药水使用逻辑调用）
    /// 从 StatusEffectConfig 读取图标路径、名称、描述
    /// </summary>
    public static void AddUsedPotion(BuffConfig.StatusEffectConfig config)
    {
        if (config == null || string.IsNullOrEmpty(config.iconPath)) return;

        usedPotionIcons.Add(new PotionIconData
        {
            iconPath = config.iconPath,
            potionName = config.displayName,
            description = config.description
        });

        // 刷新所有 SpireBuffUI 实例
        foreach (var ui in allInstances)
        {
            if (ui != null)
                ui.RefreshUI();
        }
    }

    /// <summary>
    /// 清除所有已用药水图标（新战斗开始时调用）
    /// </summary>
    public static void ClearUsedPotions()
    {
        usedPotionIcons.Clear();
    }

    private void Awake()
    {
        allInstances.Add(this);
    }

    private void Start()
    {
        if (BuffManager.Instance != null)
            BuffManager.Instance.OnBuffChanged += RefreshUI;
        RefreshUI();
    }

    private void OnDestroy()
    {
        allInstances.Remove(this);
        if (BuffManager.Instance != null)
            BuffManager.Instance.OnBuffChanged -= RefreshUI;
    }

    /// <summary>
    /// 刷新Buff显示（Buff + 已用药水）
    /// </summary>
    public void RefreshUI()
    {
        if (BuffManager.Instance == null) return;

        // 获取所有有效状态
        List<StatusEffect> allStatus = BuffManager.Instance.GetAllStatus();

        // 只保留层数大于0的
        currentBuffs.Clear();
        foreach (var status in allStatus)
        {
            if (status.stack > 0)
                currentBuffs.Add(status);
        }

        // 重新构建图标
        RebuildIcons();
    }

    /// <summary>
    /// 重新构建所有图标（Buff + 已用药水，自动去重）
    /// </summary>
    private void RebuildIcons()
    {
        ClearIcons();

        int index = 0;

        // 先收集 Buff 图标的路径（用于后续去重）
        HashSet<string> buffIconPaths = new HashSet<string>();
        for (int i = 0; i < currentBuffs.Count; i++)
        {
            string path = GetBuffIconPath(currentBuffs[i]);
            if (!string.IsNullOrEmpty(path))
                buffIconPaths.Add(path);
        }

        // 1. 先显示 Buff 图标
        for (int i = 0; i < currentBuffs.Count; i++)
        {
            CreateBuffIcon(currentBuffs[i], index);
            index++;
        }

        // 2. 再显示已用药水图标（跳过与 Buff 图标路径重复的）
        for (int i = 0; i < usedPotionIcons.Count; i++)
        {
            if (buffIconPaths.Contains(usedPotionIcons[i].iconPath))
                continue; // 路径重复：BuffManager 已有相同图标，跳过

            CreatePotionIcon(usedPotionIcons[i], index);
            index++;
        }
    }

    /// <summary>
    /// 获取 Buff 的实际图标路径
    /// </summary>
    private string GetBuffIconPath(StatusEffect status)
    {
        // 优先从 BuffConfigManager 获取
        string path = BuffConfig.BuffConfigManager.Instance.GetIconPath(status.type);
        if (!string.IsNullOrEmpty(path)) return path;

        // 备用：从 StatusEffect 自身获取
        return status.iconPath;
    }

    /// <summary>
    /// 创建单个Buff图标
    /// </summary>
    private void CreateBuffIcon(StatusEffect status, int index)
    {
        if (buffIconPrefab == null || iconContainer == null) return;

        GameObject iconObj = Instantiate(buffIconPrefab, iconContainer);
        buffIcons.Add(iconObj);

        // 设置位置
        SetIconPosition(iconObj, index);

        BuffIcon icon = iconObj.GetComponent<BuffIcon>();
        if (icon != null)
        {
            icon.Setup(status);
        }
    }

    /// <summary>
    /// 创建已用药水图标
    /// </summary>
    private void CreatePotionIcon(PotionIconData data, int index)
    {
        if (buffIconPrefab == null || iconContainer == null) return;

        GameObject iconObj = Instantiate(buffIconPrefab, iconContainer);
        buffIcons.Add(iconObj);

        // 设置位置
        SetIconPosition(iconObj, index);

        // 手动设置图标（不使用 StatusEffect.Setup）
        BuffIcon iconComp = iconObj.GetComponent<BuffIcon>();
        if (iconComp != null)
        {
            // 隐藏层数（药水没有层数概念）
            if (iconComp.stackText != null)
                iconComp.stackText.gameObject.SetActive(false);

            // 设置图标精灵
            if (iconComp.iconImage != null && !string.IsNullOrEmpty(data.iconPath))
            {
                Sprite sprite = ResourceCache.GetSprite(data.iconPath);
                if (sprite != null)
                {
                    iconComp.iconImage.sprite = sprite;
                    Color c = iconComp.iconImage.color;
                    c.a = 1f;
                    iconComp.iconImage.color = c;
                }
            }

            // 设置悬停提示
            if (iconComp.tooltipNameText != null)
                iconComp.tooltipNameText.text = data.potionName;
            if (iconComp.tooltipDescText != null)
                iconComp.tooltipDescText.text = data.description;
        }
    }

    /// <summary>
    /// 设置图标的位置
    /// </summary>
    private void SetIconPosition(GameObject iconObj, int index)
    {
        RectTransform iconRT = iconObj.GetComponent<RectTransform>();
        if (iconRT == null) return;

        int col = index % maxIconsPerRow;
        int row = index / maxIconsPerRow;
        float xPos = col * (iconSize + iconSpacing);
        float yPos = -row * (iconSize + iconSpacing);

        iconRT.anchorMin = Vector2.zero;
        iconRT.anchorMax = Vector2.zero;
        iconRT.pivot = Vector2.zero;
        iconRT.anchoredPosition = new Vector2(xPos, yPos);
        iconRT.sizeDelta = new Vector2(iconSize, iconSize);
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

    /// <summary>
    /// 显示/隐藏面板
    /// </summary>
    public void Show(bool show)
    {
        gameObject.SetActive(show);
    }
}
