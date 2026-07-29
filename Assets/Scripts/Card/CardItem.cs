using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// 卡牌使用状态
/// </summary>
public enum CardUseState
{
    None,       // 普通状态
    Hovering,   // 悬停中（鼠标进入）
    Dragging,   // 拖拽中
    Using,      // 使用中（右键可取消）
    Cancelled   // 已取消
}

/// <summary>
/// 卡牌基类 - 处理统一的点击、拖拽、取消逻辑
/// 子类只需重写 OnCardUsed() 实现具体效果
/// </summary>
public class CardItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    public CardData data;
    public DeckCard sourceDeckCard; // 来源DeckCard（含升级标记，null则用全局判断）

    public void Init(CardData data, DeckCard deckCard = null)
    {
        this.data = data;
        this.sourceDeckCard = deckCard;
        // 检查是否免费卡
        if (deckCard != null && FightCardManager.Instance.IsFreeCard(deckCard.instanceId))
            costOverride = 0;
    }

    // ============ 升级状态辅助方法 ============

    /// <summary>
    /// 当前卡牌是否已升级
    /// </summary>
    public bool IsUpgraded()
    {
        if (data == null) return false;
        // 临时升级（本场战斗）
        if (sourceDeckCard != null && FightCardManager.Instance.IsTempUpgraded(sourceDeckCard.instanceId))
            return true;
        // 实例级升级
        if (sourceDeckCard != null)
            return sourceDeckCard.upgraded;
        if (RoleManager.Instance == null) return false;
        return RoleManager.Instance.HasAnyUpgraded(data.id);
    }

    /// <summary>
    /// 获取当前费用（升级后返回 upgradedExpend，否则返回 expend）
    /// </summary>
    public int GetExpend()
    {
        if (data == null) return 0;
        return IsUpgraded() ? data.upgradedExpend : data.expend;
    }

    /// <summary>
    /// 获取当前效果参数（升级后返回 upgradedArg0，否则返回 arg0）
    /// </summary>
    public int GetArg0()
    {
        if (data == null) return 0;
        return IsUpgraded() ? data.upgradedArg0 : data.arg0;
    }

    /// <summary>
    /// 获取卡牌词条文本（固有/保留/奇巧），用于描述开头金色显示
    /// </summary>
    private string GetTraitsText(bool upgraded)
    {
        if (data == null) return "";
        List<string> traits = new List<string>();
        if (upgraded ? data.upgradedIsInnate : data.isInnate) traits.Add("固有");
        if (upgraded ? data.upgradedIsRetain : data.isRetain) traits.Add("保留");
        if (upgraded ? data.upgradedAutoPlayOnDiscard : data.autoPlayOnDiscard) traits.Add("奇巧");
        return traits.Count > 0 ? "[" + string.Join("][", traits) + "]" : "";
    }

    protected CardUseState useState = CardUseState.None;
    protected Vector2 usingStartPos;   // 使用开始时的位置
    protected Vector2 dragStartPos;   // 拖拽开始时的鼠标位置
    protected bool hasDraggedFar = false; // 是否已拖拽超过阈值
    protected Vector2 initPos;        // 拖拽开始时的卡牌位置

    private TextMeshProUGUI costText;            // 费用文本组件

    /// <summary>
    /// 费用覆盖值，-1=使用原始费用。用于本回合临时改为0等效果，随CardItem销毁自动重置
    /// </summary>
    public int costOverride = -1;

    // 拖拽距离阈值（屏幕高度的百分比，向上的垂直距离）
    private const float DRAG_THRESHOLD_PERCENT = 0.25f;

    // 是否是攻击类型卡牌（检查类型列表中是否有"攻击"）
    protected bool IsAttackCard()
    {
        return data != null && data.HasCardType("攻击");
    }

    // 获取当前使用状态
    public CardUseState GetUseState()
    {
        return useState;
    }

    // ============ 统一的事件处理 ============

    // 鼠标按下
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        // 右键取消
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (useState == CardUseState.Dragging || useState == CardUseState.Using)
            {
                CancelUsing();
            }
        }
    }

    // 鼠标进入
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (useState != CardUseState.None) return;

        useState = CardUseState.Hovering;
        transform.DOScale(1.5f, 0.25f);
        // 不再改变层级，保持由 FightUI.UpdateCardItemPos() 控制的层级顺序
        SetHighlight(true);
    }

    // 鼠标离开
    public void OnPointerExit(PointerEventData eventData)
    {
        if (useState != CardUseState.Hovering) return;

        useState = CardUseState.None;
        transform.DOScale(1, 0.25f);
        // 不再改变层级，保持原有顺序
        SetHighlight(false);
    }

    // 开始拖拽
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        // 非攻击牌进入使用状态
        if (!IsAttackCard())
        {
            StartUsing();
        }

        initPos = GetComponent<RectTransform>().anchoredPosition;
        dragStartPos = eventData.position;
        hasDraggedFar = false;

        AudioManager.Instance?.PlayEffect("Cards/draw");
    }

    // 拖拽中
    public virtual void OnDrag(PointerEventData eventData)
    {
        // 右键取消
        if (Input.GetMouseButton(1))
        {
            CancelUsing();
            return;
        }

        // 如果已取消，不再处理拖拽
        if (useState == CardUseState.Cancelled) return;

        // 检测向上拖拽距离是否超过阈值（只检测垂直向上）
        if (!IsAttackCard())
        {
            float verticalDrag = eventData.position.y - dragStartPos.y; // 向上为正
            float threshold = Screen.height * DRAG_THRESHOLD_PERCENT;
            
            if (verticalDrag > threshold)
            {
                hasDraggedFar = true;
            }
            else if (verticalDrag < threshold * 0.5f)
            {
                // 如果拖回低于阈值的一半，重置状态
                hasDraggedFar = false;
            }
        }

        // 移动卡牌
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out pos))
        {
            GetComponent<RectTransform>().anchoredPosition = pos;
        }
    }

    // 结束拖拽
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        // 攻击牌由AttackCardItem自己处理
        if (IsAttackCard()) return;

        // 已取消，不处理
        if (useState == CardUseState.Cancelled)
        {
            useState = CardUseState.None;
            return;
        }

        // 未拖拽超过阈值，取消使用
        if (!hasDraggedFar)
        {
            CancelUsing();
            return;
        }

        // 拖拽超过阈值，使用卡牌
        if (TryUse())
        {
            RelicManager.Instance.TriggerCardPlayed(this);
            OnCardUsed();

            // 复制药水效果：额外打出一次
            if (BuffManager.Instance.HasStatus(StatusType.Duplicate))
            {
                BuffManager.Instance.RemoveStatus(StatusType.Duplicate, 1);
                UIManager.Instance.ShowTip("复制!", Color.magenta);
                OnCardUsed();
            }
        }
        else
        {
            OnCardUseFailed();
        }
    }

    // ============ 使用状态管理 ============

    // 开始使用状态
    protected void StartUsing()
    {
        useState = CardUseState.Dragging;
        usingStartPos = GetComponent<RectTransform>().anchoredPosition;
    }

    // 取消使用
    protected virtual void CancelUsing()
    {
        useState = CardUseState.Cancelled;
        hasDraggedFar = false; // 重置拖拽标志

        // 回到原位置
        GetComponent<RectTransform>().anchoredPosition = usingStartPos;
        transform.localScale = Vector3.one;
        
        // 不再改变层级，保持原有顺序
        SetHighlight(false);

        // 延迟重置状态
        StartCoroutine(ResetState());
    }

    private IEnumerator ResetState()
    {
        yield return new WaitForSeconds(0.1f);
        useState = CardUseState.None;
    }

    // ============ 统一的高亮设置 ============

    protected void SetHighlight(bool highlight)
    {
        if (transform.Find("bg") == null) return;
        Image bgImg = transform.Find("bg").GetComponent<Image>();
        if (bgImg.material == null) return;

        if (highlight)
        {
            bgImg.material.SetColor("_lineColor", Color.yellow);
            bgImg.material.SetFloat("_lineWidth", 10);
        }
        else
        {
            bgImg.material.SetColor("_lineColor", Color.black);
            bgImg.material.SetFloat("_lineWidth", 1);
        }
    }

    // ============ 尝试使用卡牌 ============

    protected virtual bool TryUse()
    {
        if (data == null)
        {
            Debug.LogError("CardItem: data is null!");
            return false;
        }

        // 子类可重写此方法检查使用条件（如条件不满足则返回手牌，不消耗费用）
        if (!CanUseCondition())
        {
            CancelUsing();
            return false;
        }

        // 眩晕：每回合只能打出2张牌
        if (BuffManager.Instance.HasStatus(StatusType.Dizzy) && FightManager.Instance.cardsPlayedThisTurn >= 2)
        {
            UIManager.Instance.ShowTip("眩晕：本回合只能打出2张牌", Color.red);
            CancelUsing();
            return false;
        }

        int cost = GetCost();

        if (cost > FightManager.Instance.CurPowerCount)
        {
            AudioManager.Instance.PlayEffect("Effect/lose");
            UIManager.Instance.ShowTip("费用不足", Color.red);
            return false;
        }

        // 扣费并删除卡牌
        FightManager.Instance.CurPowerCount -= cost;
        FightManager.Instance.cardsPlayedThisTurn++;
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.UpdatePower();
            fightUI.RemoveCard(this);
        }

        // 如果是法术牌，处理法术相关效果（费用减免、大法师袍等）
        if (IsSpellCard())
        {
            BuffManager.Instance.OnSpellCardUsed();
        }

        return true;
    }

    /// <summary>
    /// 获取卡牌费用（考虑费用减免和费用覆盖）
    /// </summary>
    public int GetCost()
    {
        if (data == null) return 0;
        
        // 如果有费用覆盖（如时间回溯设为0），优先使用
        if (costOverride >= 0) return costOverride;
        
        int baseCost = GetExpend(); // 升级后使用 upgradedExpend
        
        // 检查费用减免效果（法术牌费用-1）
        if ((BuffManager.Instance.HasSpellCostDiscount() || RelicManager.Instance.HasSpellCostReduction()) && IsSpellCard() && baseCost > 0)
        {
            return baseCost - 1;
        }
        
        return baseCost;
    }

    /// <summary>
    /// 是否是法术类型卡牌
    /// </summary>
    public bool IsSpellCard()
    {
        return data != null && data.HasCardType("法术");
    }

    /// <summary>
    /// 应用冥想费用减免（绿色显示）
    /// </summary>
    public void ApplyMeditationCostReduction()
    {
        if (data == null || GetExpend() <= 0) return;
        
        if (costText == null)
        {
            costText = transform.Find("bg/useTxt")?.GetComponent<TextMeshProUGUI>();
        }
        if (costText != null)
        {
            costText.text = (GetExpend() - 1).ToString();
            costText.color = Color.green;
        }
    }

    /// <summary>
    /// 恢复冥想费用（白色显示）
    /// </summary>
    public void RestoreMeditationCost()
    {
        if (data == null) return;
        
        if (costText == null)
        {
            costText = transform.Find("bg/useTxt")?.GetComponent<TextMeshProUGUI>();
        }
        if (costText != null)
        {
            costText.text = GetExpend().ToString();
            costText.color = Color.white;
        }
    }

    /// <summary>
    /// 刷新升级相关显示（名称、费用、描述）
    /// </summary>
    public void RefreshUpgradeDisplay()
    {
        if (data == null) return;
        bool upgraded = IsUpgraded();

        // 刷新名称
        TextMeshProUGUI nameTxt = transform.Find("bg/nameTxt")?.GetComponent<TextMeshProUGUI>();
        if (nameTxt != null)
        {
            nameTxt.text = upgraded ? data.cardName + "+" : data.cardName;
            nameTxt.color = upgraded ? Color.yellow : Color.white;
        }

        // 刷新描述
        TextMeshProUGUI msgTxt = transform.Find("bg/msgTxt")?.GetComponent<TextMeshProUGUI>();
        if (msgTxt != null)
        {
            string desc = (upgraded && !string.IsNullOrEmpty(data.upgradedDescription))
                ? string.Format(data.upgradedDescription, data.upgradedArg0)
                : data.GetFormattedDescription();
            string traits = GetTraitsText(upgraded);
            if (!string.IsNullOrEmpty(traits))
                desc = "<color=yellow>" + traits + "</color> " + desc;
            msgTxt.text = desc;
        }

        // 刷新费用
        RefreshCostDisplay();
    }

    /// <summary>
    /// 刷新费用显示（用于费用减免效果激活/结束时调用）
    /// </summary>
    public void RefreshCostDisplay()
    {
        if (data == null) return;
        
        if (costText == null)
        {
            costText = transform.Find("bg/useTxt")?.GetComponent<TextMeshProUGUI>();
        }
        if (costText != null)
        {
            int cost = GetCost();
            costText.text = cost.ToString();
            // 费用覆盖（免费）或法术减免时显示绿色
            if (costOverride == 0 || ((BuffManager.Instance.HasSpellCostDiscount() || RelicManager.Instance.HasSpellCostReduction()) && IsSpellCard()))
            {
                costText.color = Color.green;
            }
            else
            {
                costText.color = Color.white;
            }
        }
    }

    // ============ 子类可重写的方法 ============

    /// <summary>
    /// 检查卡牌是否可以使用的条件 - 子类可重写实现自定义条件检查
    /// 返回 false 则卡牌返回手牌，不消耗费用
    /// </summary>
    protected virtual bool CanUseCondition()
    {
        return true;
    }

    /// <summary>
    /// 卡牌使用成功 - 子类重写实现具体效果
    /// </summary>
    protected virtual void OnCardUsed()
    {
        useState = CardUseState.None;
    }

    /// <summary>
    /// 回合结束时若该牌仍在手上触发 - 子类重写实现（如缠绕：造成伤害）
    /// </summary>
    public virtual void OnPlayerTurnEndInHand()
    {
    }

    /// <summary>
    /// 卡牌使用失败（费用不足等）- 子类重写处理
    /// </summary>
    protected virtual void OnCardUseFailed()
    {
        useState = CardUseState.None;
        // 回到原位
        GetComponent<RectTransform>().anchoredPosition = usingStartPos;
        transform.localScale = Vector3.one;
        // 不再改变层级，保持原有顺序
        SetHighlight(false);
    }

    // ============ 特效播放 ============

    public void PlayEffect(Vector3 pos)
    {
        if (string.IsNullOrEmpty(data.effects)) return;
        GameObject prefab = Resources.Load(data.effects) as GameObject;
        if (prefab == null) return;
        GameObject effectObj = Instantiate(prefab);
        effectObj.transform.position = pos;
        Destroy(effectObj, 2);
    }

    // ============ 初始化 ============

    private void Start()
    {
        if (data == null)
        {
            Debug.LogError("CardItem: data is null!");
            return;
        }

        bool upgraded = IsUpgraded();

        transform.Find("bg").GetComponent<Image>().sprite = Resources.Load<Sprite>(data.bgIcon);
        transform.Find("bg/icon").GetComponent<Image>().sprite = Resources.Load<Sprite>(data.icon);
        
        // 升级后使用升级描述，并在开头添加金色词条
        string desc;
        if (upgraded && !string.IsNullOrEmpty(data.upgradedDescription))
            desc = string.Format(data.upgradedDescription, data.upgradedArg0);
        else
            desc = data.GetFormattedDescription();
        
        string traits = GetTraitsText(upgraded);
        if (!string.IsNullOrEmpty(traits))
            desc = "<color=yellow>" + traits + "</color> " + desc;
        transform.Find("bg/msgTxt").GetComponent<TextMeshProUGUI>().text = desc;
        
        // 升级后卡牌名显示金色 + "+"
        TextMeshProUGUI nameTxt = transform.Find("bg/nameTxt").GetComponent<TextMeshProUGUI>();
        if (upgraded)
        {
            nameTxt.text = data.cardName + "+";
            nameTxt.color = Color.yellow;
        }
        else
        {
            nameTxt.text = data.cardName;
            nameTxt.color = Color.white;
        }
        
        // 使用覆盖费用（如时间回溯设为0）或升级后的费用
        int displayCost = costOverride >= 0 ? costOverride : GetExpend();
        transform.Find("bg/useTxt").GetComponent<TextMeshProUGUI>().text = displayCost.ToString();
        
        // 类型文本（升级后去除消耗类型）
        string typeNames = data.GetTypeNames();
        if (upgraded && data.removeConsumeOnUpgrade && data.IsConsumeCard())
            typeNames = typeNames.Replace("消耗", "").Replace("//", "/").Trim('/');
        transform.Find("bg/Text").GetComponent<TextMeshProUGUI>().text = typeNames;
        
        // 设置边框材质
        transform.Find("bg").GetComponent<Image>().material = Instantiate(Resources.Load<Material>("Mats/outline"));

        // 如果有费用覆盖，显示为绿色
        if (costOverride >= 0)
        {
            transform.Find("bg/useTxt").GetComponent<TextMeshProUGUI>().color = Color.green;
        }

        // 检查费用减免效果，如果有则更新费用显示
        if ((BuffManager.Instance.HasSpellCostDiscount() || RelicManager.Instance.HasSpellCostReduction()) && IsSpellCard())
        {
            RefreshCostDisplay();
        }
    }

    private void OnDestroy()
    {
        // 销毁时清理所有tween，防止DOTween报错
        transform.DOKill();
    }
}
