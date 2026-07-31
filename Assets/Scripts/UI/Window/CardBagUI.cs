using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardBagUI : UIBase
{
    [Header("下拉筛选")]
    public TMP_Dropdown typeDropDown;
    public TMP_Dropdown rarityDropDown;
    public TMP_Dropdown developDropDown;

    [Header("卡牌展示区域")]
    public Transform grid;
    public GameObject cardItemPrefab;

    private List<CardData> allCards;
    private List<GameObject> displayedItems = new List<GameObject>();

    private List<string> typeOptions = new List<string>();
    private List<string> rarityOptions = new List<string>();

    private ScrollRect scrollRect;

    private void Awake()
    {
        Register("bg/returnBtn").onClick = OnReturnBtn;
        AutoFindComponents();
        SetupDropdowns();
    }

    private bool hasInited = false;

    private void Start()
    {
        InitOnce();
    }

    public override void Show()
    {
        base.Show();
        // 每次打开时刷新显示（升级后再次进入能看到更新）
        if (!hasInited)
            InitOnce();
        else
            RefreshDisplay();
    }

    private void InitOnce()
    {
        if (hasInited) return;
        hasInited = true;

        // 隐藏 EncyclpediaUI
        UIManager.Instance.HideUI("EncyclpediaUI");

        if (scrollRect != null)
            scrollRect.scrollSensitivity = 60f;

        allCards = GameConfigManager.Instance.GetCardDataList();
        RefreshDisplay();
    }

    private void AutoFindComponents()
    {
        if (typeDropDown == null)
        {
            Transform t = transform.Find("bg/top/typeDropDown");
            if (t != null) typeDropDown = t.GetComponent<TMP_Dropdown>();
        }
        if (rarityDropDown == null)
        {
            Transform t = transform.Find("bg/top/rarityDropDown");
            if (t != null) rarityDropDown = t.GetComponent<TMP_Dropdown>();
        }
        if (developDropDown == null)
        {
            Transform t = transform.Find("bg/top/developDropDown");
            if (t != null) developDropDown = t.GetComponent<TMP_Dropdown>();
        }
        if (grid == null)
        {
            // 完整层级: bg/content/scroll/bg(内)/grid
            Transform t = transform.Find("bg/content/scroll/bg/grid");
            if (t != null) grid = t;
        }
        if (scrollRect == null)
        {
            Transform t = transform.Find("bg/content/scroll");
            if (t != null) scrollRect = t.GetComponent<ScrollRect>();
        }
        if (cardItemPrefab == null)
        {
            cardItemPrefab = ResourceCache.Get<GameObject>("UI/CardItem");
        }
    }

    private void SetupDropdowns()
    {
        // --- 类型下拉 ---
        typeOptions.Clear();
        typeOptions.Add("All");
        List<CardTypeData> typeList = GameConfigManager.Instance.GetCardTypeList();
        foreach (var ct in typeList)
            typeOptions.Add(ct.typeName);
        typeDropDown.ClearOptions();
        typeDropDown.AddOptions(typeOptions);
        typeDropDown.onValueChanged.AddListener(_ => RefreshDisplay());

        // --- 稀有度下拉 ---
        rarityOptions.Clear();
        rarityOptions.Add("All");
        foreach (CardRarity r in System.Enum.GetValues(typeof(CardRarity)))
            rarityOptions.Add(r.ToString());
        rarityDropDown.ClearOptions();
        rarityDropDown.AddOptions(rarityOptions);
        rarityDropDown.onValueChanged.AddListener(_ => RefreshDisplay());

        // --- 升级下拉 ---
        developDropDown.ClearOptions();
        developDropDown.AddOptions(new List<string> { "All", "基础", "升级" });
        developDropDown.onValueChanged.AddListener(_ => RefreshDisplay());
    }

    private void RefreshDisplay()
    {
        // 清理旧物体
        foreach (var item in displayedItems)
            Destroy(item);
        displayedItems.Clear();

        if (allCards == null || allCards.Count == 0) return;

        // 获取筛选条件
        string selectedType = typeDropDown.options[typeDropDown.value].text;
        string selectedRarity = rarityDropDown.options[rarityDropDown.value].text;
        string selectedDevelop = developDropDown.options[developDropDown.value].text;


        foreach (var card in allCards)
        {
            if (selectedType != "All" && !card.HasCardType(selectedType))
                continue;
            if (selectedRarity != "All" && card.rarity.ToString() != selectedRarity)
                continue;

            // 纯数据查询（不依赖游戏进度）：
            // "All"：显示所有卡牌基础版
            // "基础"：显示所有卡牌基础版
            // "升级"：显示所有可升级(upgradable=true)的卡牌，以升级形态展示
            bool showUpgraded = false;
            if (selectedDevelop == "升级")
            {
                if (!card.upgradable) continue;
                showUpgraded = true; // 显示升级态预览
            }
            // "All" / "基础"：均为基础态

            CreateCardItem(card, showUpgraded);
        }
    }

    private string GetCardTraits(CardData card, bool upgraded)
    {
        List<string> traits = new List<string>();
        if (upgraded ? card.upgradedIsInnate : card.isInnate) traits.Add("固有");
        if (upgraded ? card.upgradedIsRetain : card.isRetain) traits.Add("保留");
        if (upgraded ? card.upgradedAutoPlayOnDiscard : card.autoPlayOnDiscard) traits.Add("奇巧");
        return traits.Count > 0 ? "[" + string.Join("]", traits) + "]" : "";
    }

    private void CreateCardItem(CardData cardData, bool showUpgraded = false)
    {
        if (cardItemPrefab == null) return;
        GameObject cardObj = Instantiate(cardItemPrefab, grid);

        int displayExpend = showUpgraded ? cardData.upgradedExpend : cardData.expend;
        string displayDesc = showUpgraded && !string.IsNullOrEmpty(cardData.upgradedDescription)
            ? string.Format(cardData.upgradedDescription, cardData.upgradedArg0)
            : cardData.GetFormattedDescription();

        // 词条放在描述开头，显示金色
        string traits = GetCardTraits(cardData, showUpgraded);
        if (!string.IsNullOrEmpty(traits))
            displayDesc = "<color=yellow>" + traits + "</color> " + displayDesc;

        // 手动设置卡牌视觉
        cardObj.transform.Find("bg").GetComponent<Image>().sprite = ResourceCache.GetSprite(cardData.bgIcon);
        cardObj.transform.Find("bg/icon").GetComponent<Image>().sprite = ResourceCache.GetSprite(cardData.icon);
        cardObj.transform.Find("bg/msgTxt").GetComponent<TextMeshProUGUI>().text = displayDesc;
        cardObj.transform.Find("bg/nameTxt").GetComponent<TextMeshProUGUI>().text = showUpgraded ? cardData.cardName + "+" : cardData.cardName;
        if (showUpgraded)
            cardObj.transform.Find("bg/nameTxt").GetComponent<TextMeshProUGUI>().color = Color.yellow;
        cardObj.transform.Find("bg/useTxt").GetComponent<TextMeshProUGUI>().text = displayExpend.ToString();
        // 升级版去除"消耗"类型（根据配置）
        string typeNames = cardData.GetTypeNames();
        if (showUpgraded && cardData.removeConsumeOnUpgrade && cardData.IsConsumeCard())
            typeNames = typeNames.Replace("消耗", "").Replace("//", "/").Trim('/');
        cardObj.transform.Find("bg/Text").GetComponent<TextMeshProUGUI>().text = typeNames;

        // 立即移除所有 CardItem 组件，防止 Start()/拖拽等行为
        CardItem[] items = cardObj.GetComponents<CardItem>();
        foreach (var ci in items) DestroyImmediate(ci);

        // 关闭所有射线检测，纯展示
        Image[] imgs = cardObj.GetComponentsInChildren<Image>();
        foreach (var img in imgs)
            img.raycastTarget = false;

        displayedItems.Add(cardObj);
    }

    private void OnReturnBtn(GameObject obj, PointerEventData pData)
    {
        UIManager.Instance.ShowUI<EncyclpediaUI>("EncyclpediaUI");
        Close();
    }
}
