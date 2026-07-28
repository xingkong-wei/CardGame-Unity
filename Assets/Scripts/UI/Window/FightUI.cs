using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using TMPro;

// 独立的计时器管理器（不受UI隐藏影响）
public class BattleTimer : MonoBehaviour
{
    public System.Action<int> onSecondTick;
    private int totalSeconds = 0;
    private Coroutine timerCoroutine;

    public void StartTimer()
    {
        StopTimer();
        totalSeconds = 0;
        timerCoroutine = StartCoroutine(TimerCoroutine());
    }

    public void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    private IEnumerator TimerCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            totalSeconds++;
            onSecondTick?.Invoke(totalSeconds);
        }
    }

    public int GetBattleTime()
    {
        return totalSeconds;
    }
}

//战斗界面
public class FightUI : UIBase
{
    [Header("卡牌堆")]
    public TextMeshProUGUI cardCountTxt;
    public TextMeshProUGUI noCardCountTxt;
    public TextMeshProUGUI consumeCardCountTxt;
    public TextMeshProUGUI collectionCountTxt;

    [Header("战斗状态")]
    public TextMeshProUGUI powerTxt;
    public TextMeshProUGUI hpTxt;
    public Image hpImg;
    public TextMeshProUGUI fyTxt;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI timeText;

    [Header("按钮")]
    public Button turnBtn;
    public Button setBtn;
    public Button plotBtn;
    public Button cardBtn;
    public Button mapBtn;
    public Button hasCardBtn;
    public Button noCardBtn;
    public Button consumeCardBtn;

    [Header("药水面板")]
    public GameObject potionObj;

    [Header("遗物面板")]
    public RelicsUI relicsUI;

    [Header("伤害预览")]
    public GameObject damagePreviewPrefab;
    private GameObject damagePreviewInstance;

    private List<CardItem> cardItemList;

    public static readonly Vector2 CardCancelPos = new Vector2(-800, -500);

    private int currentCoinDisplay;
    private BattleTimer battleTimer;
    private static BattleTimer sharedTimer;

    private void Awake()
    {
        cardItemList = new List<CardItem>();

        // 创建或获取共享的计时器实例
        if (sharedTimer == null)
        {
            GameObject timerObj = new GameObject("BattleTimer");
            sharedTimer = timerObj.AddComponent<BattleTimer>();
            DontDestroyOnLoad(timerObj);
        }
        battleTimer = sharedTimer;

        // 按钮事件绑定
        turnBtn.onClick.AddListener(onChangeTurnBtn);
        setBtn.onClick.AddListener(OnSetBtnClick);
        plotBtn.onClick.AddListener(OnPlotBtnClick);
        cardBtn.onClick.AddListener(OnCardBtnClick);
        mapBtn.onClick.AddListener(OnMapBtnClick);
        hasCardBtn.onClick.AddListener(OnHasCardBtnClick);
        noCardBtn.onClick.AddListener(OnNoCardBtnClick);
        consumeCardBtn.onClick.AddListener(OnConsumeCardBtnClick);

        // 添加药水面板控制器
        if (potionObj != null && potionObj.GetComponent<PotionPanelController>() == null)
            potionObj.AddComponent<PotionPanelController>();
    }

    //玩家回合结束，切换敌人回合
    private void onChangeTurnBtn()
    {
        //只有玩家回合才能切换
        if (FightManager.Instance.fightUnit is Fight_PlayerTurn)
        {
            // 冥想效果不结束，持续到第一张法术牌打出

            // 玩家回合结束：触发Buff效果
            BuffManager.Instance.OnPlayerTurnEnd();

            // 玩家回合结束：触发遗物效果
            RelicManager.Instance.TriggerTurnEnd();

            FightManager.Instance.ChangeType(FightType.Enemy);
        }
    }

    //禁用结束回合按钮
    public void DisableTurnButton()
    {
        if (turnBtn != null) turnBtn.interactable = false;
    }

    //启用结束回合按钮
    public void EnableTurnButton()
    {
        if (turnBtn != null) turnBtn.interactable = true;
    }

    private void Start()
    {
        UpdateHp();
        UpdatePower();
        UpdateDefense();
        UpdateCardCount();
        UpdateUsedCardCount();
        UpdateConsumeCardCount();
        UpdateCollectionCount();



        // 初始化金币显示
        currentCoinDisplay = FightManager.Instance.CoinAmount;
        coinText.text = currentCoinDisplay.ToString();

        // 预热 DamagePreview 预制体（避免首次使用时卡顿）
        PreWarmDamagePreview();

        // 初始化计时器
        if (battleTimer != null && battleTimer.gameObject != null)
        {
            // 设置回调
            battleTimer.onSecondTick = UpdateTimeDisplay;

            // 更新显示
            int currentTime = battleTimer.GetBattleTime();
            if (timeText != null)
            {
                timeText.text = FormatTime(currentTime);
            }

            // 如果计时器协程未运行，启动它
            if (currentTime == 0)
            {
                StartTimer();
            }
        }
    }

    //更新血量显示
    public void UpdateHp()
    {
        hpTxt.text = FightManager.Instance.CurHp + "/" + FightManager.Instance.MaxHp;
        hpImg.fillAmount = (float)FightManager.Instance.CurHp / (float)FightManager.Instance.MaxHp;
    }



    //更新能量
    public void UpdatePower()
    {
        powerTxt.text = FightManager.Instance.CurPowerCount + "/" + FightManager.Instance.MaxPowerCount;
    }

    //防御更新
    public void UpdateDefense()
    {
        fyTxt.text = FightManager.Instance.DefenseCount.ToString();
    }

    //更新卡牌数量
    public void UpdateCardCount()
    {
        if (cardCountTxt != null)
            cardCountTxt.text = FightCardManager.Instance.cardList.Count.ToString();
        if (noCardCountTxt != null)
            noCardCountTxt.text = FightCardManager.Instance.usedCardList.Count.ToString();
    }

    //更新集卡簿卡牌数量
    public void UpdateCollectionCount()
    {
        if (collectionCountTxt != null)
        {
            collectionCountTxt.text = RoleManager.Instance.cardList.Count.ToString();
        }
    }

    //更新弃牌数量
    public void UpdateUsedCardCount()
    {
        if (noCardCountTxt != null)
            noCardCountTxt.text = FightCardManager.Instance.usedCardList.Count.ToString();
    }

    //更新废牌数量
    public void UpdateConsumeCardCount()
    {
        if (consumeCardCountTxt != null)
            consumeCardCountTxt.text = FightCardManager.Instance.consumeCardList.Count.ToString();
    }


    //创建卡牌物体
    public void CreateCardItem(int Count)
    {
        for (int i = 0; i < Count; i++)
        {
            // 如果抽牌堆为空，先将弃牌堆洗入抽牌堆（杀戮尖塔机制）
            if (FightCardManager.Instance.cardList.Count == 0)
            {
                FightCardManager.Instance.ShuffleDiscardToDraw();
            }

            // 如果洗牌后仍然没有牌，停止抽牌
            if (FightCardManager.Instance.cardList.Count == 0)
            {
                break;
            }

            GameObject obj = Instantiate(Resources.Load("UI/CardItem"), transform) as GameObject;
            obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-1000, -700);

            //抽卡（返回DeckCard含升级标记）
            DeckCard drawn = FightCardManager.Instance.DrawCard();
            if (drawn == null || drawn.cardData == null)
            {
                Debug.LogError("抽卡失败，卡牌数据为null");
                Destroy(obj);
                continue;
            }

            CardData cardData = drawn.cardData;
            System.Type cardType = System.Type.GetType(cardData.scriptName);
            if (cardType != null && typeof(CardItem).IsAssignableFrom(cardType))
            {
                CardItem item = obj.AddComponent(cardType) as CardItem;
                item.Init(cardData, drawn);
                cardItemList.Add(item);
            }
            else
            {
                Debug.LogError($"无法创建卡牌脚本类型: {cardData.scriptName}");
                Destroy(obj);
            }
        }

        // 更新抽牌堆数量显示
        UpdateCardCount();
    }

    //更新卡牌位置 - 扇形布局（杀戮尖塔风格）
    public void UpdateCardItemPos()
    {
        int count = cardItemList.Count;
        if (count == 0) return;

        // 布局参数
        float totalWidth = 1200f;           // 总宽度（增大分散）
        float baseY = -700f;               // 基础Y坐标（屏幕底部）
        float yOffset = 40f;               // 两边卡牌的Y偏移（向上）
        float maxRotation = 25f;            // 最大旋转角度

        // 根据卡牌数量计算均匀间隔
        float spacing = totalWidth / (count + 1);
        float centerX = 0f;

        for (int i = 0; i < count; i++)
        {
            // 计算卡牌索引对应的位置（0,1,2,... -> -n/2, ..., 0, ..., n/2）
            float posIndex = i - (count - 1) / 2f;
            
            // 计算对称的位置（-1到1范围）
            float normalizedPos = posIndex / Mathf.Max(1, (count - 1) / 2f);

            // 目标位置
            float x = centerX + posIndex * spacing;
            float y = baseY + (1f - Mathf.Abs(normalizedPos)) * yOffset;
            Vector2 targetPos = new Vector2(x, y);
            
            // 旋转角度（两边向内倾斜）
            float targetRotation = -normalizedPos * maxRotation;

            RectTransform rect = cardItemList[i].GetComponent<RectTransform>();
            
            // 立即设置位置和旋转（避免DOTween时序问题）
            rect.anchoredPosition = targetPos;
            rect.localRotation = Quaternion.Euler(0, 0, targetRotation);
            
            // 设置层级：最左边的牌在最底下，最右边的牌在最顶上
            rect.SetSiblingIndex(i);
        }
    }

    //删除卡牌物体
    /// <param name="item">要删除的卡牌</param>
    /// <param name="isUsed">是否是被使用（true=消耗卡入废牌堆），false=回合结束丢弃</param>
    public void RemoveCard(CardItem item, bool isUsed = true)
    {
        AudioManager.Instance.PlayEffect("Cards/cardShove");

        item.enabled = false;

        DeckCard dc = item.sourceDeckCard ?? new DeckCard(item.data);

        bool toExhaustPile = false; // 是否入废牌堆

        if (item.data != null)
        {
            // 消耗卡：使用时入废牌堆，回合结束丢弃时入弃牌堆
            if (item.data.IsConsumeCard())
            {
                toExhaustPile = isUsed;
            }
            // 虚无卡：回合结束时（isUsed=false）入废牌堆
            else if (item.data.IsEtherealCard() && !isUsed)
            {
                toExhaustPile = true;
            }
        }

        if (toExhaustPile)
        {
            FightCardManager.Instance.MarkCardAsConsumed(dc.instanceId);
            FightCardManager.Instance.consumeCardList.Add(dc);
            UpdateConsumeCardCount();
        }
        else
        {
            FightCardManager.Instance.usedCardList.Add(dc);
            UpdateUsedCardCount();
        }

        //从集合中删除
        cardItemList.Remove(item);

        //刷新卡牌位置
        UpdateCardItemPos();

        //卡牌移到对应堆效果（废牌堆在左边，弃牌堆在右边）
        Vector2 targetPos = toExhaustPile
            ? new Vector2(-1000, -700) 
            : new Vector2(1000, -700);
        item.GetComponent<RectTransform>().DOAnchorPos(targetPos, 0.25f);

        item.transform.DOScale(0, 0.25f);

        Destroy(item.gameObject, 1);
    }

    //删除所有卡牌（带动画效果）- 回合结束时的丢弃
    public void RemoveAllCards()
    {
        for (int i = cardItemList.Count - 1; i >= 0; i--)
        {
            RemoveCard(cardItemList[i], false); // false = 回合结束丢弃，消耗卡入弃牌堆
        }
    }

    //清理所有手牌（立即清理，用于进入新战斗时）
    public void ClearAllCards()
    {
        foreach (CardItem item in cardItemList)
        {
            if (item != null && item.gameObject != null)
            {
                Destroy(item.gameObject);
            }
        }
        cardItemList.Clear();
    }
    
    /// <summary>
    /// 直接移除卡牌（不放任何堆，用于能力牌）
    /// </summary>
    /// <summary>
    /// 获取当前手牌列表
    /// </summary>
    public List<CardItem> GetCardItemList()
    {
        return cardItemList;
    }

    /// <summary>
    /// 将指定卡牌直接置入手牌（不从抽牌堆抽卡）
    /// </summary>
    public void AddCardToHand(CardData cardData, DeckCard deckCard = null)
    {
        if (cardData == null) return;

        GameObject obj = Instantiate(Resources.Load("UI/CardItem"), transform) as GameObject;
        obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-1000, -700);

        System.Type cardType = System.Type.GetType(cardData.scriptName);
        if (cardType != null && typeof(CardItem).IsAssignableFrom(cardType))
        {
            CardItem item = obj.AddComponent(cardType) as CardItem;
            item.Init(cardData, deckCard);
            cardItemList.Add(item);
        }
        else
        {
            Debug.LogError($"无法创建卡牌脚本类型: {cardData.scriptName}");
            Destroy(obj);
        }

        UpdateCardCount();
    }

    public void RemoveCardDirectly(CardItem item)
    {
        if (item == null) return;
        
        // 从集合中移除
        if (cardItemList.Contains(item))
        {
            cardItemList.Remove(item);
        }
        
        // 刷新卡牌位置
        UpdateCardItemPos();
    }

    //点击设置按钮,显示设置界面
    private void OnSetBtnClick()
    {
        GameSettingUI settingUI = UIManager.Instance.ShowUI<GameSettingUI>("GameSettingUI") as GameSettingUI;
        if (settingUI != null)
        {
            settingUI.SetPreviousUIName("FightUI");
        }
    }

    //更新金币
    public void UpdateCoinDisplay(int targetAmount)
    {
        // 如果已有动画，先终止（使用一个唯一ID）
        DOTween.Kill(coinText);
        // 从当前显示值开始动画到目标值
        DOTween.To(() => currentCoinDisplay, x =>
        {
            currentCoinDisplay = x;
            coinText.text = currentCoinDisplay.ToString();
        }, targetAmount, 0.5f).SetEase(Ease.OutQuad).SetId(coinText);
    }

    //点击情节按钮，显示情节界面
    private void OnPlotBtnClick()
    {
        UIManager.Instance.ShowUI<PlotUI>("PlotUI");
    }

    //已拥有卡牌按钮事件
    private void OnCardBtnClick()
    {
        CardCollectionUI.ShowCardList(CardListType.Collection, "集卡簿");
    }

    //抽牌堆按钮事件
    private void OnHasCardBtnClick()
    {
        CardCollectionUI.ShowCardList(CardListType.DrawPile, "抽牌堆");
    }

    //弃牌堆按钮事件
    private void OnNoCardBtnClick()
    {
        CardCollectionUI.ShowCardList(CardListType.DiscardPile, "弃牌堆");
    }

    //废牌堆按钮事件
    private void OnConsumeCardBtnClick()
    {
        CardCollectionUI.ShowCardList(CardListType.ConsumePile, "废牌堆");
    }

    //地图按钮事件
    private void OnMapBtnClick()
    {
        OpenNodeMapForObservation();
    }

    /// <summary>
    /// 打开节点地图观察（战斗中、商店中通用）
    /// </summary>
    public static void OpenNodeMapForObservation()
    {
        SlayTheSpireMapUI nodeMapUI = UIManager.Instance.GetUI<SlayTheSpireMapUI>("SlayTheSpireMapUI");
        if (nodeMapUI == null)
            nodeMapUI = UIManager.Instance.ShowUI<SlayTheSpireMapUI>("SlayTheSpireMapUI") as SlayTheSpireMapUI;
        else
            nodeMapUI.Show();

        int islandIndex = FightManager.Instance.GetCurrentIslandIndex();
        nodeMapUI.OpenForObservation(islandIndex);
    }

    //重置计时器（公开方法，供外部调用）
    public static void ResetBattleTimer()
    {
        if (sharedTimer != null && sharedTimer.gameObject != null)
        {
            sharedTimer.StopTimer();
            sharedTimer.StartTimer();
        }
    }

    //重置计时器（私有方法）
    private void ResetTimer()
    {
        if (battleTimer != null && battleTimer.gameObject != null)
        {
            battleTimer.StopTimer();
            battleTimer.StartTimer();
            timeText.text = FormatTime(0);
        }
    }

    //重写Show方法，确保计时器正常工作
    public override void Show()
    {
        base.Show();

        // 更新血量显示（战斗结束后回血/升级可能已改变血量）
        UpdateHp();
        UpdatePower();
        UpdateDefense();
        UpdateCollectionCount();
        // 刷新金币显示
        if (coinText != null)
        {
            currentCoinDisplay = FightManager.Instance.CoinAmount;
            coinText.text = currentCoinDisplay.ToString();
        }

        // 确保计时器在运行
        if (battleTimer != null && battleTimer.gameObject != null)
        {
            battleTimer.onSecondTick = UpdateTimeDisplay;
            timeText.text = FormatTime(battleTimer.GetBattleTime());
        }
    }

    //启动计时器
    private void StartTimer()
    {
        if (battleTimer != null && battleTimer.gameObject != null)
        {
            battleTimer.onSecondTick = UpdateTimeDisplay;
            battleTimer.StartTimer();
        }
    }

    //停止计时器
    private void StopTimer()
    {
        if (battleTimer != null && battleTimer.gameObject != null)
        {
            battleTimer.StopTimer();
        }
    }

    //更新时间显示（带动画）
    private void UpdateTimeDisplay(int seconds)
    {
        if (timeText == null) return;
        
        string newTime = FormatTime(seconds);
        timeText.text = newTime;

        // 使用DOTween做秒数跳动动画（检查对象是否有效）
        if (timeText.transform != null)
            timeText.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0), 0.3f, 1, 0.5f);
    }

    //格式化时间为 MM:SS
    private string FormatTime(int seconds)
    {
        int minutes = seconds / 60;
        int secs = seconds % 60;
        return string.Format("{0:00}:{1:00}", minutes, secs);
    }

    //获取当前战斗时间（秒）
    public int GetBattleTime()
    {
        if (battleTimer != null && battleTimer.gameObject != null)
        {
            return battleTimer.GetBattleTime();
        }
        return 0;
    }

    //战斗结束时的清理
    private void OnDestroy()
    {
        StopTimer();
        if (timeText != null && timeText.transform != null)
            DOTween.Kill(timeText.transform);
        // 清理所有本对象的 DOTween
        DOTween.Kill(transform);

        // 清理事件监听
        if (battleTimer != null)
        {
            battleTimer.onSecondTick = null;
        }
    }

    #region 伤害预览

    /// <summary>
    /// 显示伤害预览（单段攻击）
    /// </summary>
    /// <param name="damage">预计伤害值</param>
    /// <param name="worldPos">敌人世界坐标</param>
    public void ShowDamagePreview(int damage, Vector3 worldPos)
    {
        ShowDamagePreview(damage, worldPos, 1);
    }

    /// <summary>
    /// 显示伤害预览（支持多段攻击）
    /// </summary>
    /// <param name="damage">预计伤害值</param>
    /// <param name="worldPos">敌人世界坐标</param>
    /// <param name="times">攻击次数（默认1）</param>
    public void ShowDamagePreview(int damage, Vector3 worldPos, int times)
    {
        if (damagePreviewPrefab == null)
        {
            Debug.LogWarning("damagePreviewPrefab is null!");
            return;
        }

        // 如果已存在，先销毁
        if (damagePreviewInstance != null)
        {
            Destroy(damagePreviewInstance);
        }

        // 创建预制体实例，放到 Canvas 最底部
        Transform canvasTf = UIManager.Instance.canvasTf;
        damagePreviewInstance = Instantiate(damagePreviewPrefab, canvasTf);
        
        // 设置更高的 sorting order（独立的 Canvas）
        Canvas dpCanvas = damagePreviewInstance.GetComponent<Canvas>();
        if (dpCanvas == null)
        {
            dpCanvas = damagePreviewInstance.AddComponent<Canvas>();
        }
        dpCanvas.overrideSorting = true;
        dpCanvas.sortingOrder = 100;
        
        if (damagePreviewInstance.GetComponent<GraphicRaycaster>() == null)
        {
            damagePreviewInstance.AddComponent<GraphicRaycaster>();
        }

        // 设置文字 (TextMeshPro)，支持多段攻击格式
        TextMeshProUGUI tmp = damagePreviewInstance.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = times > 1 ? $"{damage}x{times}" : damage.ToString();
            tmp.raycastTarget = false; // 不接收射线
        }

        // 设置位置到屏幕中央
        RectTransform rt = damagePreviewInstance.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one * 1.2f;
        }
        

    }

    /// <summary>
    /// 预热伤害预览预制体，避免首次使用时卡顿
    /// </summary>
    private void PreWarmDamagePreview()
    {
        if (damagePreviewPrefab != null)
        {
            // 预先实例化一次然后立即销毁，触发资源加载
            GameObject prewarm = Instantiate(damagePreviewPrefab, transform);
            Destroy(prewarm);
        }
    }

    /// <summary>
    /// 隐藏伤害预览
    /// </summary>
    public void HideDamagePreview()
    {
        if (damagePreviewInstance != null)
        {
            Destroy(damagePreviewInstance);
            damagePreviewInstance = null;
        }
    }

    /// <summary>
    /// 显示AOE伤害预览（显示在敌人位置）
    /// </summary>
    public void ShowAOEDamagePreview(int damage, Vector3 worldPos)
    {
        if (damagePreviewPrefab == null)
        {
            Debug.LogWarning("damagePreviewPrefab is null!");
            return;
        }

        // 如果已存在，先销毁
        if (damagePreviewInstance != null)
        {
            Destroy(damagePreviewInstance);
        }

        // 创建预制体实例
        Transform canvasTf = UIManager.Instance.canvasTf;
        damagePreviewInstance = Instantiate(damagePreviewPrefab, canvasTf);
        
        // 设置 Canvas
        Canvas dpCanvas = damagePreviewInstance.GetComponent<Canvas>();
        if (dpCanvas == null)
        {
            dpCanvas = damagePreviewInstance.AddComponent<Canvas>();
        }
        dpCanvas.overrideSorting = true;
        dpCanvas.sortingOrder = 100;
        
        if (damagePreviewInstance.GetComponent<GraphicRaycaster>() == null)
        {
            damagePreviewInstance.AddComponent<GraphicRaycaster>();
        }

        // 设置文字
        TextMeshProUGUI tmp = damagePreviewInstance.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = damage.ToString();
            tmp.raycastTarget = false;
        }

        // 设置位置到敌人世界坐标位置
        RectTransform rt = damagePreviewInstance.GetComponent<RectTransform>();
        if (rt != null)
        {
            // 将世界坐标转换为屏幕坐标
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPos);
            RectTransform canvasRect = canvasTf as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out Vector2 localPos);
            rt.anchoredPosition = localPos;
            rt.localScale = Vector3.one * 1.2f;
        }
    }

    /// <summary>
    /// 隐藏AOE伤害预览
    /// </summary>
    public void HideAOEDamagePreview()
    {
        HideDamagePreview();
    }

    #endregion
}
