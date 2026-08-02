using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RelicBagUI : UIBase
{
    [Header("下拉筛选")]
    public TMP_Dropdown rarityDropDown;

    [Header("遗物展示区域")]
    public Transform grid;
    public GameObject relicIconPrefab;

    private List<RelicData> allRelics;
    private readonly List<GameObject> displayedItems = new();
    private ScrollRect scrollRect;
    private bool hasInited = false;

    private void Awake()
    {
        AutoFindComponents();
        SetupDropdowns();

        Transform returnBtnTf = transform.Find("bg/returnBtn");
        if (returnBtnTf != null)
            Register("bg/returnBtn").onClick = OnReturnBtn;
    }

    private void Start()
    {
        InitOnce();
    }

    public override void Show()
    {
        base.Show();
        if (!hasInited)
            InitOnce();
        else
            RefreshDisplay();
    }

    private void InitOnce()
    {
        if (hasInited) return;
        hasInited = true;

        UIManager.Instance.HideUI("EncyclpediaUI");
        transform.SetAsLastSibling();

        if (scrollRect != null)
            scrollRect.scrollSensitivity = 60f;

        allRelics = GameConfigManager.Instance.GetRelicDataList();
        RefreshDisplay();
    }

    private void AutoFindComponents()
    {
        if (rarityDropDown == null)
        {
            Transform t = transform.Find("bg/top/rarityDropDown");
            if (t != null) rarityDropDown = t.GetComponent<TMP_Dropdown>();
        }
        if (grid == null)
        {
            Transform t = transform.Find("bg/content/scroll/bg/grid");
            if (t != null) grid = t;
        }
        if (scrollRect == null)
        {
            Transform t = transform.Find("bg/content/scroll");
            if (t != null) scrollRect = t.GetComponent<ScrollRect>();
        }
        if (relicIconPrefab == null)
        {
            relicIconPrefab = ResourceCache.Get<GameObject>("UI/RelicIcon");
        }
    }

    private void SetupDropdowns()
    {
        var options = new List<string> { "全部", "初始", "普通", "罕见", "稀有" };
        rarityDropDown.ClearOptions();
        rarityDropDown.AddOptions(options);
        rarityDropDown.onValueChanged.AddListener(_ => RefreshDisplay());
    }

    private void RefreshDisplay()
    {
        foreach (var item in displayedItems)
            Destroy(item);
        displayedItems.Clear();

        if (allRelics == null || allRelics.Count == 0) return;

        // value 0=全部, 1=初始(Starter), 2=普通(Common), 3=罕见(Uncommon), 4=稀有(Rare)
        int rarityIndex = rarityDropDown.value;

        foreach (var relic in allRelics)
        {
            if (rarityIndex != 0 && (int)relic.rarity + 1 != rarityIndex)
                continue;
            CreateRelicItem(relic);
        }
    }

    private void CreateRelicItem(RelicData relicData)
    {
        if (relicIconPrefab == null) return;
        GameObject relicObj = Instantiate(relicIconPrefab, grid);

        // 禁用悬停交互和 Button
        RelicIcon relicIcon = relicObj.GetComponent<RelicIcon>();
        if (relicIcon != null)
            relicIcon.enabled = false;

        Button btn = relicObj.GetComponent<Button>();
        if (btn != null)
            btn.enabled = false;

        // 设置遗物图标（RelicData.sprite 是直接引用，不是路径字符串）
        Transform iconTf = relicObj.transform.Find("Icon");
        if (iconTf != null)
        {
            Image iconImg = iconTf.GetComponent<Image>();
            if (iconImg != null && relicData.sprite != null)
                iconImg.sprite = relicData.sprite;
        }

        // 直接显示 TooltipPanel，放在图标右边
        Transform tooltipTf = relicObj.transform.Find("Icon/TooltipPanel");
        if (tooltipTf != null)
        {
            tooltipTf.gameObject.SetActive(true);

            // 镜像到右边
            RectTransform tooltipRt = tooltipTf as RectTransform;
            tooltipRt.anchorMin = new Vector2(1, 0.5f);
            tooltipRt.anchorMax = new Vector2(1, 0.5f);
            tooltipRt.pivot = new Vector2(0, 0.5f);
            tooltipRt.anchoredPosition = new Vector2(10, 0);

            TextMeshProUGUI nameText = tooltipTf.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = relicData.relicName;

            TextMeshProUGUI descText = tooltipTf.Find("DescText")?.GetComponent<TextMeshProUGUI>();
            if (descText != null)
                descText.text = relicData.description;

            RelicIcon.AdjustTooltipSizeStatic(tooltipTf.gameObject, nameText, descText);
        }

        displayedItems.Add(relicObj);
    }

    private void OnReturnBtn(GameObject obj, PointerEventData pData)
    {
        UIManager.Instance.ShowUI<EncyclpediaUI>("EncyclpediaUI");
        Close();
    }
}
