using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 药水信息面板控制器
/// 挂载在 FightUI/Invemtory/Potion 物体上
/// 
/// 悬停行为：鼠标放在 PotionBtn1/2/3 上 → 显示 Panel(只露 Desc)，离开消失，PosX = -200/-100/0
/// 点击行为：点击 PotionBtn1/2/3 → 显示完整 Panel(UseBtn+DiscardBtn+Desc)，PosX = 0/100/200
/// </summary>
public class PotionPanelController : MonoBehaviour
{
    private GameObject panel;
    private RectTransform panelRect;
    private GameObject descObj;
    private Text descText;
    private TextMeshProUGUI descTmp;
    private RectTransform parentRect;

    // Panel 子物体
    private Button useBtn;
    private Button discardBtn;

    // 当前选中按钮对应的药水数据
    private PotionData selectedPotion;
    private Button selectedBtn;

    // Panel 中除 Desc 以外的子物体（悬停时需要隐藏）
    private List<GameObject> panelOtherChildren = new List<GameObject>();

    private Button potionBtn1;
    private Button potionBtn2;
    private Button potionBtn3;

    // 默认背景图（空位时显示）
    private Sprite defaultBtnBg;

    // 是否通过点击打开了 Panel
    private bool isPanelShownByClick = false;

    private void Awake()
    {
        FightManager.CachedPotionPanel = this;
    }

    private void OnDestroy()
    {
        if (FightManager.CachedPotionPanel == this)
            FightManager.CachedPotionPanel = null;
    }

    private void Start()
    {
        parentRect = transform.GetComponent<RectTransform>();

        // ---- 查找 Panel ----
        Transform panelTf = transform.Find("Panel");
        if (panelTf != null)
        {
            panel = panelTf.gameObject;
            panelRect = panelTf.GetComponent<RectTransform>();
            panel.SetActive(false);

            // 收集除 Desc 以外的子物体
            foreach (Transform child in panelTf)
            {
                if (child.name == "UseBtn")
                    useBtn = child.GetComponent<Button>();
                else if (child.name == "DiscardBtn")
                    discardBtn = child.GetComponent<Button>();

                if (child.name != "Desc")
                    panelOtherChildren.Add(child.gameObject);
            }

            // 查找 Desc（药水描述文本，兼容 Text 和 TMP）
            Transform descTf = panelTf.Find("Desc");
            if (descTf != null)
            {
                descObj = descTf.gameObject;
                descText = descTf.GetComponent<Text>();
                descTmp = descTf.GetComponent<TextMeshProUGUI>();
                if (descText == null && descTmp == null)
                    Debug.LogWarning("[PotionPanel] Desc 没有 Text 或 TMP 组件！");
            }
        }

        // ---- 绑定 Panel 内按钮事件 ----
        if (useBtn != null) useBtn.onClick.AddListener(OnUsePotion);
        if (discardBtn != null) discardBtn.onClick.AddListener(OnDiscardPotion);

        // ---- 查找三个药水按钮 ----
        potionBtn1 = transform.Find("PotionBtn1")?.GetComponent<Button>();
        potionBtn2 = transform.Find("PotionBtn2")?.GetComponent<Button>();
        potionBtn3 = transform.Find("PotionBtn3")?.GetComponent<Button>();

        // 保存按钮上的原始背景图（预制体已设置好 PotionBg）作为默认空位图
        if (potionBtn1 != null)
            defaultBtnBg = potionBtn1.GetComponent<Image>()?.sprite;

        // ---- 绑定事件 ----
        SetupHover(potionBtn1);
        SetupHover(potionBtn2);
        SetupHover(potionBtn3);

        potionBtn1?.onClick.AddListener(() => OnButtonClick(potionBtn1, 0));
        potionBtn2?.onClick.AddListener(() => OnButtonClick(potionBtn2, 1));
        potionBtn3?.onClick.AddListener(() => OnButtonClick(potionBtn3, 2));

        // 初始化按钮图标
        RefreshPotionButtons();
    }

    #region 刷新药水按钮状态

    /// <summary>
    /// 根据药水库存刷新按钮显示
    /// </summary>
    public void RefreshPotionButtons()
    {
        var potionList = FightManager.Instance?.potionList;
        if (potionList == null) return;

        UpdatePotionBtn(potionBtn1, 0, potionList);
        UpdatePotionBtn(potionBtn2, 1, potionList);
        UpdatePotionBtn(potionBtn3, 2, potionList);
    }

    private void UpdatePotionBtn(Button btn, int index, List<PotionData> potionList)
    {
        if (btn == null) return;

        if (index < potionList.Count)
        {
            // 有药水：显示图标
            btn.gameObject.SetActive(true);
            SetButtonIcon(btn, potionList[index]);
        }
        else
        {
            // 无药水：显示空位或隐藏
            btn.gameObject.SetActive(true);
            SetButtonIcon(btn, null);
        }
    }

    /// <summary>
    /// 设置按钮图标
    /// </summary>
    private void SetButtonIcon(Button btn, PotionData data)
    {
        Image btnImg = btn.GetComponent<Image>();
        if (btnImg == null) return;

        if (data != null && !string.IsNullOrEmpty(data.icon))
        {
            // 有药水：显示药水图标（使用缓存）
            Sprite sprite = ResourceCache.GetSprite(data.icon);
            if (sprite != null)
            {
                btnImg.sprite = sprite;
                btnImg.color = Color.white;
                return;
            }
        }

        // 空位：显示默认药水背景图
        if (defaultBtnBg != null)
        {
            btnImg.sprite = defaultBtnBg;
            btnImg.color = Color.white;
        }
        else
        {
            // 备用：灰色空位
            btnImg.sprite = null;
            btnImg.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        }
    }

    /// <summary>
    /// 获取指定按钮对应的药水数据
    /// </summary>
    private PotionData GetPotionData(int index)
    {
        var list = FightManager.Instance?.potionList;
        if (list == null || index >= list.Count) return null;
        return list[index];
    }

    #endregion

    #region 悬停行为

    private void SetupHover(Button btn)
    {
        if (btn == null) return;

        EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>()
            ?? btn.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => OnHoverEnter(btn));
        trigger.triggers.Add(enterEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => OnHoverExit());
        trigger.triggers.Add(exitEntry);
    }

    private void OnHoverEnter(Button btn)
    {
        if (isPanelShownByClick) return;
        if (panel == null || descObj == null) return;

        // 找到按钮对应的药水数据
        int index = btn == potionBtn1 ? 0 : (btn == potionBtn2 ? 1 : 2);
        PotionData data = GetPotionData(index);
        if (data == null) return;

        float btnCenterX = GetButtonCenterX(btn);
        SetPanelX(btnCenterX - 300f);

        // 设置 Desc 图标 + 描述文本
        SetDescContent(data);

        panel.SetActive(true);
        foreach (var child in panelOtherChildren)
            child.SetActive(false);
        descObj.SetActive(true);
    }

    private void OnHoverExit()
    {
        if (isPanelShownByClick) return;
        if (panel != null) panel.SetActive(false);
    }

    #endregion

    #region 点击行为

    private void OnButtonClick(Button btn, int index)
    {
        if (panel == null || panelRect == null) return;

        // 再次点击同一个按钮 → 关闭
        if (isPanelShownByClick && btn == selectedBtn)
        {
            HidePanel();
            return;
        }

        PotionData data = GetPotionData(index);
        if (data == null) return;

        selectedPotion = data;
        selectedBtn = btn;

        float btnCenterX = GetButtonCenterX(btn);
        SetPanelX(btnCenterX - 100f);

        // 设置 Desc 图标 + 描述文本
        SetDescContent(data);

        // 显示完整 Panel
        panel.SetActive(true);
        foreach (var child in panelOtherChildren)
            child.SetActive(true);
        if (descObj != null) descObj.SetActive(true);

        isPanelShownByClick = true;
    }

    /// <summary>
    /// 使用药水
    /// </summary>
    private void OnUsePotion()
    {
        if (selectedPotion == null) return;

        // 通过反射创建药水实例并执行
        Type potionType = Type.GetType(selectedPotion.scriptName);
        if (potionType == null || !typeof(PotionBase).IsAssignableFrom(potionType))
        {
            Debug.LogError($"无法找到药水脚本: {selectedPotion.scriptName}");
            return;
        }

        PotionBase potion = Activator.CreateInstance(potionType) as PotionBase;
        potion.Init(selectedPotion);
        
        // 转换到 Canvas 坐标系（与 AttackCardItem 的 anchoredPosition 一致）
        if (selectedBtn != null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            RectTransform canvasRT = canvas.GetComponent<RectTransform>();
            RectTransform btnRT = selectedBtn.GetComponent<RectTransform>();
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRT, btnRT.position, cam, out Vector2 localPos);
            potion.potionBtnScreenPos = localPos;
        }

        // 判断当前是否在战斗中（Invemtory 在 FightUI 下 = 战斗中；被移到 SelectCardUI 下 = 奖励界面）
        bool isInCombat = transform.parent != null && 
                          transform.parent.parent != null &&
                          transform.parent.parent.GetComponent<FightUI>() != null;

        if (isInCombat)
        {
            if (!selectedPotion.canUseInCombat)
            {
                UIManager.Instance.ShowTip("该药水无法主动使用", Color.yellow);
                selectedPotion = null;
                HidePanel();
                return;
            }
            if (!(FightManager.Instance.fightUnit is Fight_PlayerTurn))
            {
                UIManager.Instance.ShowTip("无法使用", Color.red);
                selectedPotion = null;
                HidePanel();
                return;
            }
        }
        else
        {
            // 非战斗中（地图/商店）：检查 canUseInMap
            if (!selectedPotion.canUseInMap)
            {
                UIManager.Instance.ShowTip("无法使用", Color.red);
                selectedPotion = null;
                HidePanel();
                return;
            }
        }

        potion.Use();

        // 从库存移除
        FightManager.Instance.potionList.Remove(selectedPotion);
        selectedPotion = null;

        HidePanel();
        RefreshPotionButtons();
    }

    /// <summary>
    /// 丢弃药水
    /// </summary>
    private void OnDiscardPotion()
    {
        if (selectedPotion == null) return;

        FightManager.Instance.potionList.Remove(selectedPotion);
        selectedPotion = null;

        HidePanel();
        RefreshPotionButtons();
    }

    private void HidePanel()
    {
        if (panel != null) panel.SetActive(false);
        isPanelShownByClick = false;
        selectedPotion = null;
        selectedBtn = null;
    }

    private void Update()
    {
        if (!isPanelShownByClick) return;

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if (!IsPointerOverPotionUI())
            {
                HidePanel();
            }
        }
    }

    private bool IsPointerOverPotionUI()
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            Transform t = result.gameObject.transform;
            while (t != null)
            {
                if (t == transform) return true;
                t = t.parent;
            }
        }
        return false;
    }

    #endregion

    #region 工具方法

    /// <summary>
    /// 设置 Desc 描述文本
    /// </summary>
    private void SetDescContent(PotionData data)
    {
        if (descText != null)
            descText.text = data.description;
        else if (descTmp != null)
            descTmp.text = data.description;
    }

    private float GetButtonCenterX(Button btn)
    {
        RectTransform btnRect = btn.GetComponent<RectTransform>();
        return btnRect.anchorMin.x * parentRect.rect.width + btnRect.anchoredPosition.x;
    }

    private void SetPanelX(float x)
    {
        Vector3 pos = panelRect.anchoredPosition;
        panelRect.anchoredPosition = new Vector3(x, pos.y, pos.z);
    }

    #endregion

    private void OnEnable()
    {
        HidePanel();
    }
}
