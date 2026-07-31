using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// 卡牌列表显示类型
/// </summary>
public enum CardListType
{
    Collection,   // 拥有的卡牌（RoleManager）
    DrawPile,     // 抽牌堆（FightCardManager.cardList）
    DiscardPile,  // 弃牌堆（FightCardManager.usedCardList）
    ConsumePile   // 废牌堆（FightCardManager.consumeCardList）
}

public class CardCollectionUI : UIBase
{
    public Transform content;           // ScrollView 的 Content 节点
    public GameObject CardItem;   // 已有的 CardItem 预制体
    public Button closeBtn;             // 关闭按钮
    public TextMeshProUGUI titleText;              // 标题文本
    public Button confirmBtn;           // 多选确认按钮

    private static CardListType currentType = CardListType.Collection;
    private static List<CardData> customCardList = null; // 自定义卡牌列表
    
    // ===== 选择模式 =====
    private static bool isSelectMode = false;
    private static System.Action<CardData> onSelectCallback;

    // ===== 多选模式 =====
    private static bool isMultiSelect = false;
    private static System.Action<List<CardData>> onMultiSelectCallback;
    private HashSet<GameObject> selectedCardObjs = new HashSet<GameObject>();
    private Dictionary<GameObject, CardData> objToCardData = new Dictionary<GameObject, CardData>();
    
    // ===== 预览模式（点击放大）=====
    private GameObject previewCard = null;      // 当前预览的卡牌
    private CardData previewCardData = null;    // 预览的卡牌数据
    private bool isPreviewing = false;          // 是否正在预览
    private GameObject lastClickedCard = null;  // 最后点击的卡牌

    private void Awake()
    {
        if (closeBtn != null)
            closeBtn.onClick.AddListener(OnCloseBtnClick);
        if (confirmBtn != null)
            confirmBtn.onClick.AddListener(OnConfirmBtnClick);
        if (gameObject.GetComponent<UIMouseScroll>() == null)
            gameObject.AddComponent<UIMouseScroll>();
    }

    private void Start() { }


    private void OnEnable()
    {
        RefreshDisplay();
    }
    
    private void OnDestroy()
    {
        // 清理预览相关
    }

    /// <summary>
    /// 显示卡牌列表（静态方法，方便外部调用）
    /// </summary>
    public static void ShowCardList(CardListType type, string title)
    {
        ShowCardList(type, title, false, null);
    }

    /// <summary>
    /// 显示自定义卡牌列表
    /// </summary>
    public static void ShowCardList(List<CardData> cards, string title, bool selectMode, System.Action<CardData> callback)
    {
        customCardList = cards;
        currentType = CardListType.Collection;
        isSelectMode = selectMode;
        isMultiSelect = false;
        onSelectCallback = callback;

        CardCollectionUI ui = UIManager.Instance.ShowUI<CardCollectionUI>("CardCollectionUI") as CardCollectionUI;
        if (ui != null)
        {
            ui.HookTitle(title);
            if (ui.confirmBtn != null)
                ui.confirmBtn.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 显示卡牌列表（可选择模式）
    /// </summary>
    /// <param name="type">卡牌列表类型</param>
    /// <param name="title">标题</param>
    /// <param name="selectMode">是否为选择模式（选择模式下点击卡牌直接回调）</param>
    /// <param name="callback">选择模式下的回调</param>
    public static void ShowCardList(CardListType type, string title, bool selectMode, System.Action<CardData> callback)
    {
        customCardList = null;
        currentType = type;
        isSelectMode = selectMode;
        isMultiSelect = false;
        onSelectCallback = callback;
        
        CardCollectionUI ui = UIManager.Instance.ShowUI<CardCollectionUI>("CardCollectionUI") as CardCollectionUI;
        if (ui != null)
        {
            ui.HookTitle(title);
            if (ui.confirmBtn != null)
                ui.confirmBtn.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 多选模式 — 显示卡牌列表
    /// </summary>
    public static void ShowMultiSelectCardList(List<CardData> cards, string title, System.Action<List<CardData>> callback)
    {
        customCardList = cards;
        currentType = CardListType.Collection;
        isSelectMode = false;
        isMultiSelect = true;
        onMultiSelectCallback = callback;

        CardCollectionUI ui = UIManager.Instance.ShowUI<CardCollectionUI>("CardCollectionUI") as CardCollectionUI;
        if (ui != null)
        {
            ui.HookTitle(title);
            if (ui.confirmBtn != null)
                ui.confirmBtn.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 关闭按钮点击
    /// </summary>
    private void OnCloseBtnClick()
    {
        customCardList = null;
        isMultiSelect = false;
        onMultiSelectCallback = null;
        selectedCardObjs.Clear();
        objToCardData.Clear();
        if (confirmBtn != null)
            confirmBtn.gameObject.SetActive(false);
        // 如果是选择模式，点击关闭相当于取消选择
        if (isSelectMode)
        {
            onSelectCallback = null;
        }
        Close();
    }

    /// <summary>
    /// 多选确认按钮
    /// </summary>
    private void OnConfirmBtnClick()
    {
        var result = new List<CardData>();
        foreach (var obj in selectedCardObjs)
            if (objToCardData.TryGetValue(obj, out var cd))
                result.Add(cd);
        var cb = onMultiSelectCallback;
        selectedCardObjs.Clear();
        objToCardData.Clear();
        isMultiSelect = false;
        onMultiSelectCallback = null;
        customCardList = null;
        if (confirmBtn != null)
            confirmBtn.gameObject.SetActive(false);
        Close();
        cb?.Invoke(result);
    }

    /// <summary>
    /// 设置标题（避免重复代码）
    /// </summary>
    private void HookTitle(string title)
    {
        if (titleText != null)
            titleText.text = title;
    }

    /// <summary>
    /// 获取卡牌词条文本（固有/保留/奇巧）
    /// </summary>
    private static string GetTraitsText(CardData card, bool upgraded)
    {
        if (card == null) return "";
        List<string> traits = new List<string>();
        if (upgraded ? card.upgradedIsInnate : card.isInnate) traits.Add("固有");
        if (upgraded ? card.upgradedIsRetain : card.isRetain) traits.Add("保留");
        if (upgraded ? card.upgradedAutoPlayOnDiscard : card.autoPlayOnDiscard) traits.Add("奇巧");
        return traits.Count > 0 ? "[" + string.Join("][", traits) + "]" : "";
    }

    /// <summary>
    /// 手动设置卡牌视觉显示（不添加 CardItem 组件，避免鼠标事件干扰）
    /// </summary>
    private void SetupCardVisuals(GameObject cardObj, CardData cardData, bool upgraded = false)
    {
        Transform bg = cardObj.transform.Find("bg");
        if (bg == null) return;

        // 升级状态由调用方传入（实例级别）

        // 背景图
        Image bgImg = bg.GetComponent<Image>();
        if (bgImg != null && !string.IsNullOrEmpty(cardData.bgIcon))
            bgImg.sprite = ResourceCache.GetSprite(cardData.bgIcon);

        // 图标
        Transform iconTf = bg.Find("icon");
        if (iconTf != null)
        {
            Image iconImg = iconTf.GetComponent<Image>();
            if (iconImg != null && !string.IsNullOrEmpty(cardData.icon))
                iconImg.sprite = ResourceCache.GetSprite(cardData.icon);
        }

        // 描述文本（升级后使用升级描述，词条金色显示在开头）
        TextMeshProUGUI msgTxt = bg.Find("msgTxt")?.GetComponent<TextMeshProUGUI>();
        if (msgTxt != null)
        {
            string desc;
            if (upgraded && !string.IsNullOrEmpty(cardData.upgradedDescription))
                desc = string.Format(cardData.upgradedDescription, cardData.upgradedArg0);
            else
                desc = cardData.GetFormattedDescription();

            // 词条（固有/保留/奇巧）放在描述开头，金色
            string traits = GetTraitsText(cardData, upgraded);
            if (!string.IsNullOrEmpty(traits))
                desc = "<color=yellow>" + traits + "</color> " + desc;
            msgTxt.text = desc;
        }

        // 名称文本（升级后金色 + "+"）
        TextMeshProUGUI nameTxt = bg.Find("nameTxt")?.GetComponent<TextMeshProUGUI>();
        if (nameTxt != null)
        {
            if (upgraded)
            {
                nameTxt.text = cardData.cardName + "+";
                nameTxt.color = Color.yellow;
            }
            else
            {
                nameTxt.text = cardData.cardName;
                nameTxt.color = Color.white;
            }
        }

        // 费用文本（升级后使用 upgradedExpend）
        TextMeshProUGUI useTxt = bg.Find("useTxt")?.GetComponent<TextMeshProUGUI>();
        if (useTxt != null)
            useTxt.text = (upgraded ? cardData.upgradedExpend : cardData.expend).ToString();

        // 类型文本（升级后去除消耗类型）
        TextMeshProUGUI typeTxt = bg.Find("Text")?.GetComponent<TextMeshProUGUI>();
        if (typeTxt != null)
        {
            string typeNames = cardData.GetTypeNames();
            if (upgraded && cardData.removeConsumeOnUpgrade && cardData.IsConsumeCard())
                typeNames = typeNames.Replace("消耗", "").Replace("//", "/").Trim('/');
            typeTxt.text = typeNames;
        }

        // 边框材质
        if (bgImg != null)
        {
            Material outlineMat = ResourceCache.Get<Material>("Mats/outline");
            if (outlineMat != null)
                bgImg.material = Instantiate(outlineMat);
        }

        // 设置 CanvasGroup 以接收射线检测
        CanvasGroup cg = cardObj.GetComponent<CanvasGroup>();
        if (cg == null) cg = cardObj.AddComponent<CanvasGroup>();
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private void RefreshDisplay()
    {
        // 清空原有内容
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        
        // 清理预览
        ClosePreview();

        List<DeckCard> dcList = GetDeckCardListByType(currentType);

        if (dcList == null || dcList.Count == 0)
            return;

        foreach (DeckCard dc in dcList)
        {
            if (dc == null || dc.cardData == null) continue;
            CardData cardData = dc.cardData;

            // 实例化 CardItem 预制体
            GameObject cardObj = Instantiate(CardItem, content);

            // 移除预制体自带的 CardItem（防止拖拽/使用行为）
            CardItem[] items = cardObj.GetComponents<CardItem>();
            foreach (var ci in items) DestroyImmediate(ci);

            // 手动设置卡牌显示（传入实例级升级标记）
            SetupCardVisuals(cardObj, cardData, dc.upgraded);

            // 关闭所有子 Image 的射线检测，拖拽事件无法穿透
            Image[] allImgs = cardObj.GetComponentsInChildren<Image>();
            foreach (var img in allImgs) img.raycastTarget = false;

            // 创建透明点击层
            GameObject catcher = new GameObject("ClickCatcher", typeof(RectTransform), typeof(Image));
            catcher.transform.SetParent(cardObj.transform, false);
            RectTransform cr = catcher.GetComponent<RectTransform>();
            cr.anchorMin = Vector2.zero;
            cr.anchorMax = Vector2.one;
            cr.sizeDelta = Vector2.zero;
            catcher.GetComponent<Image>().color = new Color(0, 0, 0, 0);

            // 只在点击层上注册 PointerClick
            EventTrigger trigger = catcher.AddComponent<EventTrigger>();
            EventTrigger.Entry clickEntry = new EventTrigger.Entry();
            clickEntry.eventID = EventTriggerType.PointerClick;
            clickEntry.callback.AddListener((data) => OnCardClickHandler(cardObj, cardData));
            trigger.triggers.Add(clickEntry);
        }
    }
    
    /// <summary>
    /// 设置卡牌选中视觉效果（金色边框）
    /// </summary>
    private void SetCardSelectedVisual(GameObject cardObj, bool selected)
    {
        Transform bg = cardObj.transform.Find("bg");
        if (bg == null) return;
        Image bgImg = bg.GetComponent<Image>();
        if (bgImg == null || bgImg.material == null) return;
        if (selected)
        {
            bgImg.material.SetColor("_lineColor", new Color(1f, 0.85f, 0.2f));
            bgImg.material.SetFloat("_lineWidth", 12);
        }
        else
        {
            bgImg.material.SetColor("_lineColor", Color.black);
            bgImg.material.SetFloat("_lineWidth", 1);
        }
    }

    /// <summary>
    /// 添加点击事件
    /// </summary>
    private void AddClickEvent(GameObject cardObj, CardData cardData)
    {
        EventTrigger trigger = cardObj.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = cardObj.AddComponent<EventTrigger>();
        trigger.triggers.Clear();
        
        EventTrigger.Entry clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerClick;
        clickEntry.callback.AddListener((data) => OnCardClickHandler(cardObj, cardData));
        trigger.triggers.Add(clickEntry);
    }
    
    /// <summary>
    /// 卡牌点击处理
    /// </summary>
    private void OnCardClickHandler(GameObject cardObj, CardData cardData)
    {
        // 多选模式：切换选中（按物体追踪，同模板卡独立）
        if (isMultiSelect)
        {
            if (selectedCardObjs.Contains(cardObj))
            {
                selectedCardObjs.Remove(cardObj);
                SetCardSelectedVisual(cardObj, false);
            }
            else
            {
                selectedCardObjs.Add(cardObj);
                objToCardData[cardObj] = cardData;
                SetCardSelectedVisual(cardObj, true);
            }
            return;
        }

        // 选择模式：直接回调
        if (isSelectMode)
        {
            customCardList = null;
            onSelectCallback?.Invoke(cardData);
            onSelectCallback = null;
            Close();
            return;
        }
        
        // 浏览模式：预览卡牌
        if (isPreviewing && previewCard == cardObj)
        {
            // 点击同一个卡牌：关闭预览
            ClosePreview();
        }
        else
        {
            // 点击不同卡牌：切换预览
            ClosePreview();
            ShowPreview(cardObj, cardData);
        }
    }
    
    /// <summary>
    /// 显示卡牌预览（放大到屏幕中间）
    /// </summary>
    private void ShowPreview(GameObject cardObj, CardData cardData)
    {
        isPreviewing = true;
        previewCardData = cardData;
        lastClickedCard = cardObj;
        
        // 保存原始信息
        RectTransform rt = cardObj.GetComponent<RectTransform>();
        Vector2 originalPos = rt.anchoredPosition;
        int originalIndex = rt.GetSiblingIndex();
        
        // 创建占位物体，防止 Layout Group 重排
        GameObject placeholder = new GameObject("PreviewPlaceholder");
        placeholder.transform.SetParent(content);
        placeholder.transform.SetSiblingIndex(originalIndex);
        RectTransform prt = placeholder.AddComponent<RectTransform>();
        prt.anchorMin = rt.anchorMin;
        prt.anchorMax = rt.anchorMax;
        prt.pivot = rt.pivot;
        prt.sizeDelta = rt.sizeDelta;
        prt.anchoredPosition = originalPos;
        prt.localScale = Vector3.one;
        // 设置为不可见
        CanvasGroup pcg = placeholder.AddComponent<CanvasGroup>();
        pcg.alpha = 0;
        pcg.blocksRaycasts = false;
        pcg.interactable = false;
        
        // 临时移到 Canvas 层级以便居中显示
        rt.SetParent(transform);
        rt.position = rt.position; // 保持世界位置
        
        // 先终止可能残留的动画，再放大并移动到中心
        rt.DOKill();
        rt.DOMove(new Vector3(Screen.width / 2f, Screen.height / 2f, 0), 0.3f).SetEase(Ease.OutBack);
        rt.DOScale(2.5f, 0.3f).SetEase(Ease.OutBack);
        
        // 保存原始信息用于恢复
        CardCollectionUI_OriginalPos posHelper = cardObj.GetComponent<CardCollectionUI_OriginalPos>();
        if (posHelper == null)
            posHelper = cardObj.AddComponent<CardCollectionUI_OriginalPos>();
        posHelper.originalPos = originalPos;
        posHelper.originalSiblingIndex = originalIndex;
        posHelper.placeholder = placeholder;
        
        previewCard = cardObj;
    }
    
    /// <summary>
    /// 关闭预览
    /// </summary>
    private void ClosePreview()
    {
        if (!isPreviewing) return;
        
        isPreviewing = false;
        
        if (previewCard != null && lastClickedCard != null)
        {
            RectTransform rt = previewCard.GetComponent<RectTransform>();
            CardCollectionUI_OriginalPos posHelper = lastClickedCard.GetComponent<CardCollectionUI_OriginalPos>();
            
            // 终止所有动画
            rt.DOKill();
            
            // 如果存在占位物体，将卡牌插回占位位置后销毁占位
            if (posHelper != null && posHelper.placeholder != null)
            {
                RectTransform prt = posHelper.placeholder.GetComponent<RectTransform>();
                rt.SetParent(content);
                rt.SetSiblingIndex(prt.GetSiblingIndex()); // 以占位当前位置为准
                rt.anchoredPosition = posHelper.originalPos;
                Destroy(posHelper.placeholder);
                posHelper.placeholder = null;
            }
            else
            {
                // 回退：直接恢复
                rt.SetParent(content);
                if (posHelper != null)
                {
                    rt.SetSiblingIndex(posHelper.originalSiblingIndex);
                    rt.anchoredPosition = posHelper.originalPos;
                }
            }
            rt.localScale = Vector3.one;
            // 清理临时组件
            if (posHelper != null)
                Destroy(posHelper);
        }
        
        previewCard = null;
        previewCardData = null;
        lastClickedCard = null;
    }
    
    /// <summary>
    /// 根据类型获取对应的DeckCard列表（保留实例级升级标记）
    /// </summary>
    private List<DeckCard> GetDeckCardListByType(CardListType type)
    {
        if (customCardList != null)
        {
            var list = new List<DeckCard>();
            foreach (var cd in customCardList)
                list.Add(new DeckCard(cd));
            return list;
        }

        switch (type)
        {
            case CardListType.Collection:
                return RoleManager.Instance?.cardList;
            case CardListType.DrawPile:
                return FightCardManager.Instance?.cardList;
            case CardListType.DiscardPile:
                return FightCardManager.Instance?.usedCardList;
            case CardListType.ConsumePile:
                return FightCardManager.Instance?.consumeCardList;
            default:
                return null;
        }
    }
}

/// <summary>
/// 用于保存卡牌原始位置（临时组件，预览结束后删除）
/// </summary>
public class CardCollectionUI_OriginalPos : MonoBehaviour
{
    public Vector2 originalPos;
    public int originalSiblingIndex;
    public GameObject placeholder;
}
