using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 单个遗物图标组件 - 基于 BuffIcon 结构
/// </summary>
public class RelicIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("图标")]
    public Image iconImage;

    [Header("提示面板")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipNameText;
    public TextMeshProUGUI tooltipDescText;
    public RectTransform tooltipPanelRT;

    [Header("图标大小设置")]
    public Vector2 iconSize = new Vector2(60f, 60f);

    [Header("Tooltip设置")]
    public float tooltipMaxWidth = 200f;
    public float tooltipPaddingX = 20f;
    public float tooltipPaddingY = 15f;
    public float nameDescSpacing = 8f;

    private RelicData relicData;
    private int originalSiblingIndex;

    /// <summary>
    /// 设置遗物信息
    /// </summary>
    public void Setup(RelicData relic)
    {
        relicData = relic;

        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);

        // 设置图标
        if (iconImage != null && relic.sprite != null)
        {
            iconImage.sprite = relic.sprite;
            Color c = iconImage.color;
            c.a = 1f;
            iconImage.color = c;
        }

        // 设置提示信息
        if (tooltipNameText != null)
            tooltipNameText.text = relic.relicName;
        if (tooltipDescText != null)
            tooltipDescText.text = relic.description;

        Canvas.ForceUpdateCanvases();
        AdjustTooltipSize();
    }

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

        tooltipNameText?.ForceMeshUpdate();
        tooltipDescText?.ForceMeshUpdate();

        Vector2 nameValues = tooltipNameText != null ? tooltipNameText.GetPreferredValues(textMaxWidth, 0) : Vector2.zero;
        Vector2 descValues = tooltipDescText != null ? tooltipDescText.GetPreferredValues(textMaxWidth, 0) : Vector2.zero;

        float nameHeight = nameValues.y;
        float descHeight = descValues.y;

        float contentHeight = tooltipPaddingY * 2 + nameHeight + nameDescSpacing + descHeight;

        tooltipPanelRT.sizeDelta = new Vector2(tooltipMaxWidth, contentHeight);

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
    /// 手动触发 Tooltip 大小调整（药水等非 Setup 场景使用）
    /// </summary>
    public void InvokeAdjustTooltip()
    {
        Canvas.ForceUpdateCanvases();
        AdjustTooltipSize();
    }

    /// <summary>
    /// 静态方法：根据文本内容自适应 TooltipPanel 大小（不需要 RelicIcon 实例）
    /// </summary>
    public static void AdjustTooltipSizeStatic(GameObject tooltipPanel, TextMeshProUGUI nameText, TextMeshProUGUI descText)
    {
        if (tooltipPanel == null) return;
        var panelRT = tooltipPanel.GetComponent<RectTransform>();
        if (panelRT == null) return;

        float tooltipMaxWidth = 280f;
        float tooltipPaddingX = 12f;
        float tooltipPaddingY = 10f;
        float nameDescSpacing = 6f;

        float textMaxWidth = tooltipMaxWidth - tooltipPaddingX * 2;

        RectTransform nameRT = nameText?.GetComponent<RectTransform>();
        RectTransform descRT = descText?.GetComponent<RectTransform>();

        if (descRT != null) descRT.sizeDelta = new Vector2(textMaxWidth, 0);
        if (nameRT != null) nameRT.sizeDelta = new Vector2(textMaxWidth, 0);

        nameText?.ForceMeshUpdate();
        descText?.ForceMeshUpdate();

        Vector2 nameValues = nameText != null ? nameText.GetPreferredValues(textMaxWidth, 0) : Vector2.zero;
        Vector2 descValues = descText != null ? descText.GetPreferredValues(textMaxWidth, 0) : Vector2.zero;

        float nameHeight = nameValues.y;
        float descHeight = descValues.y;
        float contentHeight = tooltipPaddingY * 2 + nameHeight + nameDescSpacing + descHeight;

        panelRT.sizeDelta = new Vector2(tooltipMaxWidth, contentHeight);

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
            Canvas.ForceUpdateCanvases();
            if (tooltipPanelRT != null)
                tooltipPanelRT.anchoredPosition = new Vector2(-iconSize.x, 0);
            // 关闭 Tooltip 内所有子节点的射线检测，防止工具提示挡住鼠标导致闪烁
            SetTooltipRaycast(false);
            // 将 TooltipPanel 临时移到根 Canvas，避免被同级图标遮挡
            originalSiblingIndex = tooltipPanelRT.GetSiblingIndex();
            tooltipPanelRT.SetParent(GetRootCanvas(), true);
            tooltipPanelRT.SetAsLastSibling();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
            // 恢复 TooltipPanel 到原来的父级
            if (tooltipPanelRT.parent != transform)
            {
                tooltipPanelRT.SetParent(transform, true);
                tooltipPanelRT.SetSiblingIndex(originalSiblingIndex);
            }
        }
    }

    private Transform GetRootCanvas()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        return canvas != null ? canvas.transform : transform.root;
    }

    private void SetTooltipRaycast(bool enabled)
    {
        if (tooltipPanel == null) return;
        Graphic[] graphics = tooltipPanel.GetComponentsInChildren<Graphic>();
        foreach (var g in graphics)
            g.raycastTarget = enabled;
    }
}
