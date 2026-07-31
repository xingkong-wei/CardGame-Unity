using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 遗物图标面板 - 在UI右侧从右至左排列
/// </summary>
public class RelicsUI : MonoBehaviour
{
    [Header("图标预制体")]
    public GameObject relicIconPrefab;

    [Header("容器设置")]
    public RectTransform iconContainer;

    [Header("图标设置")]
    public float iconSpacing = 6f;
    public float iconSize = 60f;
    public int maxIconsPerRow = 10;

    private List<GameObject> relicIcons = new List<GameObject>();

    private void Awake()
    {
        FightManager.CachedRelicsUI = this;
    }

    private void OnDestroy()
    {
        if (FightManager.CachedRelicsUI == this)
            FightManager.CachedRelicsUI = null;
    }

    private void Start()
    {
        RefreshUI();
    }

    /// <summary>
    /// 刷新遗物显示
    /// </summary>
    public void RefreshUI()
    {
        ClearIcons();

        List<RelicData> relicList = FightManager.Instance?.relicList;
        if (relicList == null || relicList.Count == 0) return;

        for (int i = 0; i < relicList.Count; i++)
        {
            CreateRelicIcon(relicList[i], i);
        }
    }

    /// <summary>
    /// 添加单个遗物（带动画）
    /// </summary>
    public void AddRelic(RelicData relic)
    {
        if (relic == null) return;
        RefreshUI();
    }

    private void CreateRelicIcon(RelicData relic, int index)
    {
        if (relicIconPrefab == null || iconContainer == null) return;

        GameObject iconObj = Instantiate(relicIconPrefab, iconContainer);
        relicIcons.Add(iconObj);

        RectTransform iconRT = iconObj.GetComponent<RectTransform>();
        if (iconRT != null)
        {
            // 从右至左排列：锚点设在右上角
            int col = index % maxIconsPerRow;
            int row = index / maxIconsPerRow;
            float xPos = -(col * (iconSize + iconSpacing));
            float yPos = -row * (iconSize + iconSpacing);

            iconRT.anchorMin = new Vector2(1f, 1f);
            iconRT.anchorMax = new Vector2(1f, 1f);
            iconRT.pivot = new Vector2(1f, 1f);
            iconRT.anchoredPosition = new Vector2(xPos, yPos);
            iconRT.sizeDelta = new Vector2(iconSize, iconSize);
        }

        RelicIcon icon = iconObj.GetComponent<RelicIcon>();
        if (icon != null)
        {
            icon.Setup(relic);
        }
    }

    private void ClearIcons()
    {
        foreach (var icon in relicIcons)
        {
            if (icon != null)
                Destroy(icon);
        }
        relicIcons.Clear();
    }
}
