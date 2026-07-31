using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class RewardInterfaceUI : UIBase
{
    /// <summary>
    /// 卡牌奖励被选择时触发（用于通知 SelectCardUI 卡牌奖励已完成）
    /// </summary>
    public static System.Action OnCardRewardSelected;

    // 静态缓存：同一批卡牌奖励在跳过/关闭后再次打开不变
    private static List<CardData> cachedRewardCards = null;
    private static bool rewardConsumed = false;

    // 自定义模式：外部传入卡牌列表 + 回调
    private static List<CardData> customRewardCards = null;
    private static System.Action<CardData> customOnSelected = null;
    private static bool isCustomMode = false;

    /// <summary>
    /// 清除缓存（SelectCardUI 关闭时调用）
    /// </summary>
    public static void ClearCache()
    {
        cachedRewardCards = null;
        rewardConsumed = false;
        customRewardCards = null;
        customOnSelected = null;
        isCustomMode = false;
    }

    /// <summary>
    /// 自定义奖励模式：显示指定卡牌列表，选择后回调
    /// </summary>
    public static void ShowCustomReward(List<CardData> cards, System.Action<CardData> onSelected)
    {
        customRewardCards = new List<CardData>(cards);
        customOnSelected = onSelected;
        isCustomMode = true;
        rewardConsumed = false;
        ShowReward();
    }
    [Header("容器")]
    public Transform cardContainer;  // CardContainer

    [Header("跳过按钮")]
    public Button skipButton;        // SkipButton

    [Header("卡牌数量")]
    public int rewardCount = 3;

    /// <summary>
    /// 打开奖励界面
    /// </summary>
    public static void ShowReward()
    {
        UIManager.Instance.ShowUI<RewardInterfaceUI>("RewardInterfaceUI");
    }

    private void Awake()
    {
        // 自动查找组件
        Transform contentArea = transform.Find("ContentArea");
        if (contentArea != null)
        {
            if (cardContainer == null)
            {
                Transform ct = contentArea.Find("CardContainer");
                if (ct != null) cardContainer = ct;
            }
            if (skipButton == null)
            {
                Transform st = contentArea.Find("SkipButton");
                if (st != null) skipButton = st.GetComponent<Button>();
            }
        }

        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkip);
    }

    private void Start()
    {
        PlayShowAnimation();

        if (rewardConsumed)
        {
            Close();
            return;
        }

        List<CardData> cardsToShow;

        if (isCustomMode && customRewardCards != null)
        {
            cardsToShow = customRewardCards;
        }
        else
        {
            if (cachedRewardCards == null || cachedRewardCards.Count == 0)
                cachedRewardCards = GenerateRewardCards();
            cardsToShow = cachedRewardCards;
        }

        foreach (CardData cardData in cardsToShow)
        {
            CreateCardPreview(cardData);
        }
    }

    private void PlayShowAnimation()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(1, 0.3f).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// 带权重的奖励卡牌生成
    /// 规则：只从 Common/Rare/Uncommon 中随机，比例 5:3:2，已有卡牌权重减半
    /// </summary>
    private List<CardData> GenerateRewardCards()
    {
        List<CardData> result = new List<CardData>();
        if (cardContainer == null) return result;

        // 获取所有卡牌并过滤稀有度
        List<CardData> allCards = GameConfigManager.Instance.GetCardDataList();
        if (allCards == null || allCards.Count == 0) return result;

        // 获取玩家已有卡牌集合（用于降低权重）
        HashSet<CardData> ownedCards = new HashSet<CardData>();
        if (RoleManager.Instance != null && RoleManager.Instance.cardList != null)
        {
            foreach (var dc in RoleManager.Instance.cardList)
                ownedCards.Add(dc.cardData);
        }

        // 1. 筛选稀有度并构建权重池
        List<(CardData card, int weight)> pool = new List<(CardData, int)>();

        foreach (CardData card in allCards)
        {
            // 只保留 Common/Rare/Uncommon
            if (card.rarity != CardRarity.Common &&
                card.rarity != CardRarity.Rare &&
                card.rarity != CardRarity.Uncommon)
                continue;

            // 基础权重
            int weight = 0;
            switch (card.rarity)
            {
                case CardRarity.Common:   weight = 5; break;
                case CardRarity.Uncommon: weight = 3; break;
                case CardRarity.Rare:     weight = 2; break;
            }

            // 已拥有的卡牌权重减半（至少保留 1）
            if (ownedCards.Contains(card))
                weight = Mathf.Max(1, weight / 2);

            pool.Add((card, weight));
        }

        if (pool.Count == 0) return result;

        // 2. 带权重随机抽取（不重复）
        for (int i = 0; i < rewardCount && pool.Count > 0; i++)
        {
            int totalWeight = 0;
            foreach (var p in pool) totalWeight += p.weight;
            if (totalWeight <= 0) break;

            int roll = Random.Range(0, totalWeight);
            int cumulative = 0;
            int selectedIdx = 0;
            for (int j = 0; j < pool.Count; j++)
            {
                cumulative += pool[j].weight;
                if (roll < cumulative)
                {
                    selectedIdx = j;
                    break;
                }
            }

            result.Add(pool[selectedIdx].card);
            pool.RemoveAt(selectedIdx);
        }

        return result;
    }

    /// <summary>
    /// 创建单张卡牌预览
    /// 手动设置视觉，移除 CardItem 组件，彻底禁用拖拽
    /// 手动添加悬停放大+高亮效果
    /// EventTrigger 负责点击选择
    /// </summary>
    private void CreateCardPreview(CardData cardData)
    {
        GameObject cardObj = Instantiate(ResourceCache.Get<GameObject>("UI/CardItem"), cardContainer);

        // 移除预制体自带的 CardItem 组件，防止拖拽/攻击行为
        CardItem[] items = cardObj.GetComponents<CardItem>();
        foreach (var ci in items) DestroyImmediate(ci);

        // 手动设置卡牌视觉（与 CardBagUI 一致）
        Transform bg = cardObj.transform.Find("bg");
        Material outlineMat = null;
        if (bg != null)
        {
            Image bgImg = bg.GetComponent<Image>();
            if (bgImg != null && !string.IsNullOrEmpty(cardData.bgIcon))
                bgImg.sprite = ResourceCache.GetSprite(cardData.bgIcon);

            // 设置边框材质（CardItem.Start() 中原本会设置）
            if (bgImg != null)
            {
                Material srcMat = ResourceCache.Get<Material>("Mats/outline");
                if (srcMat != null) outlineMat = Object.Instantiate(srcMat);
                if (outlineMat != null)
                {
                    outlineMat.SetColor("_lineColor", Color.black);
                    outlineMat.SetFloat("_lineWidth", 1);
                    bgImg.material = outlineMat;
                }
            }

            Transform iconTf = bg.Find("icon");
            if (iconTf != null)
            {
                Image iconImg = iconTf.GetComponent<Image>();
                if (iconImg != null && !string.IsNullOrEmpty(cardData.icon))
                    iconImg.sprite = ResourceCache.GetSprite(cardData.icon);
            }

            TextMeshProUGUI msgTxt = bg.Find("msgTxt")?.GetComponent<TextMeshProUGUI>();
            if (msgTxt != null)
                msgTxt.text = cardData.GetFormattedDescription();

            TextMeshProUGUI nameTxt = bg.Find("nameTxt")?.GetComponent<TextMeshProUGUI>();
            if (nameTxt != null)
                nameTxt.text = cardData.cardName;

            TextMeshProUGUI useTxt = bg.Find("useTxt")?.GetComponent<TextMeshProUGUI>();
            if (useTxt != null)
                useTxt.text = cardData.expend.ToString();

            TextMeshProUGUI typeTxt = bg.Find("Text")?.GetComponent<TextMeshProUGUI>();
            if (typeTxt != null)
                typeTxt.text = cardData.GetTypeNames();
        }

        // 关闭所有子 Image 的射线检测，防止拖拽穿透
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

        // 注册点击事件（选择卡牌）
        EventTrigger trigger = catcher.AddComponent<EventTrigger>();
        CardData capturedData = cardData;
        Material capturedMat = outlineMat;
        Transform capturedCard = cardObj.transform;
        
        EventTrigger.Entry clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerClick;
        clickEntry.callback.AddListener((data) => OnCardSelected(capturedData));
        trigger.triggers.Add(clickEntry);

        // 悬停放大 + 高亮
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) =>
        {
            if (capturedCard == null) return;
            capturedCard.DOScale(1.5f, 0.25f);
            if (capturedMat != null)
            {
                capturedMat.SetColor("_lineColor", Color.yellow);
                capturedMat.SetFloat("_lineWidth", 10);
            }
        });
        trigger.triggers.Add(enterEntry);

        // 离开恢复
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) =>
        {
            if (capturedCard == null) return;
            capturedCard.DOScale(1, 0.25f);
            if (capturedMat != null)
            {
                capturedMat.SetColor("_lineColor", Color.black);
                capturedMat.SetFloat("_lineWidth", 1);
            }
        });
        trigger.triggers.Add(exitEntry);

        // 确保可点击
        CanvasGroup cg = cardObj.GetComponent<CanvasGroup>();
        if (cg == null) cg = cardObj.AddComponent<CanvasGroup>();
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private void OnDestroy()
    {
        KillAllCardTweens();
    }

    public override void Close()
    {
        KillAllCardTweens();
        base.Close();
    }

    /// <summary>
    /// 杀掉所有子卡牌的 DOTween 动画，防止对象销毁后动画仍在运行
    /// </summary>
    private void KillAllCardTweens()
    {
        if (cardContainer == null) return;
        foreach (Transform child in cardContainer)
        {
            child.DOKill();
        }
    }

    private void OnCardSelected(CardData cardData)
    {
        if (isCustomMode && customOnSelected != null)
        {
            // 自定义模式：触发外部回调
            customOnSelected.Invoke(cardData);
        }
        else
        {
            // 默认模式：添加到玩家卡牌库
            RoleManager.Instance.AddCard(cardData);
            if (FightCardManager.Instance != null)
                FightCardManager.Instance.Init();

            // 通知 SelectCardUI 卡牌奖励已完成
            OnCardRewardSelected?.Invoke();
        }

        // 清理状态
        rewardConsumed = true;
        cachedRewardCards = null;
        customRewardCards = null;
        customOnSelected = null;
        isCustomMode = false;

        StartCoroutine(DelayClose(0.2f));
    }

    private void OnSkip()
    {
        StartCoroutine(DelayClose(0.2f));
    }

    private IEnumerator DelayClose(float delay)
    {
        yield return new WaitForSeconds(delay);
        Close();
    }
}
