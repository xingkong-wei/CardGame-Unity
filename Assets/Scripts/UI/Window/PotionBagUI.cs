using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PotionBagUI : UIBase
{
    [Header("下拉筛选")]
    public TMP_Dropdown rarityDropDown;

    [Header("药水展示区域")]
    public Transform grid;
    public GameObject potionIconPrefab;

    private List<PotionData> allPotions;
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

        allPotions = GameConfigManager.Instance.GetPotionDataList();
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
        if (potionIconPrefab == null)
        {
            potionIconPrefab = ResourceCache.Get<GameObject>("UI/PotionIcon");
        }
    }

    private void SetupDropdowns()
    {
        var options = new List<string> { "全部", "普通", "罕见", "稀有" };
        rarityDropDown.ClearOptions();
        rarityDropDown.AddOptions(options);
        rarityDropDown.onValueChanged.AddListener(_ => RefreshDisplay());
    }

    private void RefreshDisplay()
    {
        foreach (var item in displayedItems)
            Destroy(item);
        displayedItems.Clear();

        if (allPotions == null || allPotions.Count == 0) return;

        // value 0=全部, 1=普通(Common), 2=罕见(Uncommon), 3=稀有(Rare)
        int rarityIndex = rarityDropDown.value;

        foreach (var potion in allPotions)
        {
            if (rarityIndex != 0 && (int)potion.rarity + 1 != rarityIndex)
                continue;
            CreatePotionItem(potion);
        }
    }

    private void CreatePotionItem(PotionData potionData)
    {
        if (potionIconPrefab == null) return;
        GameObject potionObj = Instantiate(potionIconPrefab, grid);

        // 禁用悬停交互和 Button
        RelicIcon relicIcon = potionObj.GetComponent<RelicIcon>();
        if (relicIcon != null)
            relicIcon.enabled = false;

        Button btn = potionObj.GetComponent<Button>();
        if (btn != null)
            btn.enabled = false;

        // 设置药水图标
        Transform iconTf = potionObj.transform.Find("Icon");
        if (iconTf != null)
        {
            Image iconImg = iconTf.GetComponent<Image>();
            if (iconImg != null)
            {
                Sprite sprite = ResourceCache.GetSprite(potionData.icon);
                if (sprite != null)
                    iconImg.sprite = sprite;
            }
        }

        // 直接显示 TooltipPanel，放在图标右边
        Transform tooltipTf = potionObj.transform.Find("Icon/TooltipPanel");
        if (tooltipTf != null)
        {
            tooltipTf.gameObject.SetActive(true);

            // 镜像到右边：anchor 改为 right-center，pivot 改为 left-center
            RectTransform tooltipRt = tooltipTf as RectTransform;
            tooltipRt.anchorMin = new Vector2(1, 0.5f);
            tooltipRt.anchorMax = new Vector2(1, 0.5f);
            tooltipRt.pivot = new Vector2(0, 0.5f);
            tooltipRt.anchoredPosition = new Vector2(10, 0);

            TextMeshProUGUI nameText = tooltipTf.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = potionData.potionName;

            TextMeshProUGUI descText = tooltipTf.Find("DescText")?.GetComponent<TextMeshProUGUI>();
            if (descText != null)
                descText.text = potionData.description;

            // 根据文本内容自适应 TooltipPanel 大小
            RelicIcon.AdjustTooltipSizeStatic(tooltipTf.gameObject, nameText, descText);
        }

        displayedItems.Add(potionObj);
    }

    private void OnReturnBtn(GameObject obj, PointerEventData pData)
    {
        UIManager.Instance.ShowUI<EncyclpediaUI>("EncyclpediaUI");
        Close();
    }
}
