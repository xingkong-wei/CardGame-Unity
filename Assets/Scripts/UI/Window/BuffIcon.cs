using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BuffConfig;

/// <summary>
/// 单个Buff图标组件
/// </summary>
public class BuffIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("图标")]
    public Image iconImage;
    
    [Header("层数文本")]
    public TextMeshProUGUI stackText;
    
    [Header("提示面板")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipNameText;
    public TextMeshProUGUI tooltipDescText;
    public RectTransform tooltipPanelRT;

    [Header("图标大小设置")]
    public Vector2 iconSize = new Vector2(50f, 50f);
    
    [Header("Tooltip设置")]
    public float tooltipMaxWidth = 200f;      // 最大宽度
    public float tooltipPaddingX = 20f;        // 水平内边距
    public float tooltipPaddingY = 15f;        // 垂直内边距
    public float nameDescSpacing = 8f;         // 名称和描述间距
    
    private StatusEffect currentStatus;
    private int originalSiblingIndex;

    /// <summary>
    /// 设置Buff信息
    /// </summary>
    public void Setup(StatusEffect status)
    {
        currentStatus = status;

        // 隐藏提示面板（默认不显示）
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);

        // 设置层数 - 始终显示层数
        if (stackText != null)
        {
            stackText.text = status.stack.ToString();
        }

        // 尝试从 BuffConfigManager 加载图标（优先使用配置文件的路径）
        if (iconImage != null)
        {
            Sprite sprite = null;
            string iconPath = BuffConfig.BuffConfigManager.Instance.GetIconPath(status.type);
            
            sprite = TryLoadSprite(iconPath);
            
            // 备用：尝试从 status.iconPath 加载
            if (sprite == null && !string.IsNullOrEmpty(status.iconPath))
            {
                sprite = TryLoadSprite(status.iconPath);
            }
            
            // 最后备用：使用默认图标
            if (sprite == null)
            {
                sprite = GetDefaultIcon(status.displayType);
            }
            
            iconImage.sprite = sprite;
            // 修复：确保图标完全不透明
            Color c = iconImage.color;
            c.a = 1f;
            iconImage.color = c;
        }

        // 设置提示信息 - 从配置文件获取
        SetTooltipInfo(status);
    }

    /// <summary>
    /// 尝试加载精灵，处理路径空格问题
    /// </summary>
    private Sprite TryLoadSprite(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        path = path.Trim();
        if (string.IsNullOrEmpty(path)) return null;
        return Resources.Load<Sprite>(path);
    }

    /// <summary>
    /// 设置提示面板信息 - 从Date_Ability/.asset配置文件读取
    /// </summary>
    private void SetTooltipInfo(StatusEffect status)
    {
        // 从BuffConfigManager获取配置
        var configMgr = BuffConfig.BuffConfigManager.Instance;
        if (configMgr != null)
        {
            var config = configMgr.GetConfig(status.type);
            if (config != null && !string.IsNullOrEmpty(config.displayName))
            {
                if (tooltipNameText != null)
                    tooltipNameText.text = config.displayName;
                if (tooltipDescText != null)
                    tooltipDescText.text = config.description;
            }
            else
            {
                // 备用：从StatusEffect获取
                if (tooltipNameText != null)
                    tooltipNameText.text = status.effectName;
                if (tooltipDescText != null)
                    tooltipDescText.text = status.description;
            }
        }
        else
        {
            // 备用：从StatusEffect获取
            if (tooltipNameText != null)
                tooltipNameText.text = status.effectName;
            if (tooltipDescText != null)
                tooltipDescText.text = status.description;
        }
        
        // 强制布局计算
        Canvas.ForceUpdateCanvases();
        
        // 调整TooltipPanel大小
        AdjustTooltipSize();
    }

    /// <summary>
    /// 调整TooltipPanel大小以适应内容
    /// </summary>
    private void AdjustTooltipSize()
    {
        if (tooltipPanel == null || tooltipPanelRT == null) return;
        
        float textMaxWidth = tooltipMaxWidth - tooltipPaddingX * 2;
        
        RectTransform nameRT = tooltipNameText?.GetComponent<RectTransform>();
        RectTransform descRT = tooltipDescText?.GetComponent<RectTransform>();
        
        if (descRT != null)
            descRT.sizeDelta = new Vector2(textMaxWidth, 0);
        if (nameRT != null)
            nameRT.sizeDelta = new Vector2(textMaxWidth, 0);
        
        // 强制 TMP 完成文本测量
        tooltipNameText?.ForceMeshUpdate();
        tooltipDescText?.ForceMeshUpdate();
        
        // 使用 GetPreferredValues 获取准确尺寸（考虑换行）
        Vector2 nameValues = tooltipNameText != null ? tooltipNameText.GetPreferredValues(textMaxWidth, 0) : Vector2.zero;
        Vector2 descValues = tooltipDescText != null ? tooltipDescText.GetPreferredValues(textMaxWidth, 0) : Vector2.zero;
        
        float nameWidth = nameValues.x;
        float descWidth = descValues.x;
        float nameHeight = nameValues.y;
        float descHeight = descValues.y;
        
        float contentWidth = Mathf.Min(tooltipPaddingX * 2 + Mathf.Max(nameWidth, descWidth), tooltipMaxWidth);
        float contentHeight = tooltipPaddingY * 2 + nameHeight + nameDescSpacing + descHeight;
        
        tooltipPanelRT.sizeDelta = new Vector2(contentWidth, contentHeight);
        
        float halfHeight = contentHeight / 2f;
        
        if (nameRT != null)
        {
            float nameY = halfHeight - tooltipPaddingY - nameHeight / 2f;
            nameRT.anchoredPosition = new Vector2(0, nameY);
        }
        
        if (descRT != null)
        {
            float descY = halfHeight - tooltipPaddingY - nameHeight - nameDescSpacing - descHeight / 2f;
            descRT.anchoredPosition = new Vector2(0, descY);
        }
    }

    /// <summary>
    /// 调整TooltipPanel位置 - 紧贴图标右侧，垂直居中
    /// </summary>
    private void AdjustTooltipPosition()
    {
        if (tooltipPanelRT == null) return;
        
        // 紧贴图标右侧，垂直居中
        tooltipPanelRT.anchoredPosition = new Vector2(iconSize.x, 0);
    }

    private Sprite GetDefaultIcon(StatusDisplayType displayType)
    {
        string path = displayType switch
        {
            StatusDisplayType.Buff => "Icon/Buff/DefaultBuff",
            StatusDisplayType.Debuff => "Icon/Buff/DefaultDebuff",
            StatusDisplayType.Special => "Icon/Buff/DefaultSpecial",
            _ => "Icon/Buff/DefaultBuff"
        };
        return Resources.Load<Sprite>(path);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
            AdjustTooltipPosition();
            // 把当前图标提到最前面，避免被后面的图标挡住 TooltipPanel
            originalSiblingIndex = transform.GetSiblingIndex();
            transform.SetAsLastSibling();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
        // 恢复原始顺序
        transform.SetSiblingIndex(originalSiblingIndex);
    }

    public void UpdateStack(int newStack)
    {
        if (currentStatus != null)
        {
            currentStatus.stack = newStack;
            if (stackText != null) stackText.text = newStack.ToString();
        }
    }
}
