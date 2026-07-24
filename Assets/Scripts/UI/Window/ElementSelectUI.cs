using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 元素选择弹窗 - 用于元素四溢卡牌选择消耗哪种亲和度
/// </summary>
public class ElementSelectUI : UIBase
{
    [Header("按钮")]
    public Button fireBtn;
    public Button iceBtn;
    public Button lightningBtn;

    [Header("文本")]
    public TextMeshProUGUI topText;
    public TextMeshProUGUI fireText;
    public TextMeshProUGUI iceText;
    public TextMeshProUGUI lightningText;

    private Action<StatusType> onSelectCallback;

    private void Start()
    {
        // 绑定按钮事件
        if (fireBtn != null)
            fireBtn.onClick.AddListener(() => OnSelect(StatusType.FireAffinity));
        if (iceBtn != null)
            iceBtn.onClick.AddListener(() => OnSelect(StatusType.IceAffinity));
        if (lightningBtn != null)
            lightningBtn.onClick.AddListener(() => OnSelect(StatusType.LightningAffinity));
    }

    /// <summary>
    /// 显示选择弹窗
    /// </summary>
    /// <param name="callback">选择回调，参数为选择的亲和度类型</param>
    public void ShowSelect(Action<StatusType> callback)
    {
        onSelectCallback = callback;

        // 获取当前各亲和度层数
        int fireStack = BuffManager.Instance.GetStack(StatusType.FireAffinity);
        int iceStack = BuffManager.Instance.GetStack(StatusType.IceAffinity);
        int lightningStack = BuffManager.Instance.GetStack(StatusType.LightningAffinity);

        // 更新按钮文本和状态（需要≥5层才能选择）
        UpdateButton(fireBtn, fireText, "火亲和度", fireStack);
        UpdateButton(iceBtn, iceText, "冰亲和度", iceStack);
        UpdateButton(lightningBtn, lightningText, "电亲和度", lightningStack);

        // 显示UI
        Show();
    }

    /// <summary>
    /// 更新按钮显示
    /// </summary>
    private void UpdateButton(Button btn, TextMeshProUGUI txt, string name, int stack)
    {
        if (txt != null)
            txt.text = stack.ToString();

        if (btn != null)
        {
            btn.interactable = stack >= 5;
            // 禁用状态变灰
            var colors = btn.colors;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            btn.colors = colors;
        }
    }

    /// <summary>
    /// 选择亲和度
    /// </summary>
    private void OnSelect(StatusType type)
    {
        // 关闭弹窗
        Close();

        // 回调
        onSelectCallback?.Invoke(type);
    }

    /// <summary>
    /// 显示选择来源元素（亲和度>0的元素可点击）
    /// </summary>
    public void ShowSelectForSource(Action<StatusType> callback)
    {
        onSelectCallback = callback;

        // 显示顶部提示
        if (topText != null)
            topText.text = "来源选择";

        // 获取当前各亲和度层数
        int fireStack = BuffManager.Instance.GetStack(StatusType.FireAffinity);
        int iceStack = BuffManager.Instance.GetStack(StatusType.IceAffinity);
        int lightningStack = BuffManager.Instance.GetStack(StatusType.LightningAffinity);

        // 更新按钮文本和状态（需要>0层才能选择）
        UpdateButtonForSource(fireBtn, fireText, fireStack);
        UpdateButtonForSource(iceBtn, iceText, iceStack);
        UpdateButtonForSource(lightningBtn, lightningText, lightningStack);

        // 显示UI
        Show();
    }

    /// <summary>
    /// 显示选择目标元素（排除指定来源）
    /// </summary>
    public void ShowSelectForTarget(StatusType excludeSource, Action<StatusType> callback)
    {
        onSelectCallback = callback;

        // 显示顶部提示
        if (topText != null)
            topText.text = "目标选择";

        // 获取当前各亲和度层数
        int fireStack = BuffManager.Instance.GetStack(StatusType.FireAffinity);
        int iceStack = BuffManager.Instance.GetStack(StatusType.IceAffinity);
        int lightningStack = BuffManager.Instance.GetStack(StatusType.LightningAffinity);

        // 更新按钮状态（排除来源，其他都可选）
        UpdateButtonForTarget(fireBtn, fireText, fireStack, excludeSource == StatusType.FireAffinity);
        UpdateButtonForTarget(iceBtn, iceText, iceStack, excludeSource == StatusType.IceAffinity);
        UpdateButtonForTarget(lightningBtn, lightningText, lightningStack, excludeSource == StatusType.LightningAffinity);

        // 显示UI
        Show();
    }

    /// <summary>
    /// 更新按钮显示（来源选择：需要>0层）
    /// </summary>
    private void UpdateButtonForSource(Button btn, TextMeshProUGUI txt, int stack)
    {
        if (txt != null)
            txt.text = stack.ToString();

        if (btn != null)
        {
            btn.interactable = stack > 0;
            var colors = btn.colors;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            btn.colors = colors;
        }
    }

    /// <summary>
    /// 显示元素法典选择（所有按钮都可选，隐藏层数文本）
    /// </summary>
    public void ShowSelectForCodex(Action<StatusType> callback)
    {
        onSelectCallback = callback;

        // 隐藏层数文本
        if (fireText != null) fireText.gameObject.SetActive(false);
        if (iceText != null) iceText.gameObject.SetActive(false);
        if (lightningText != null) lightningText.gameObject.SetActive(false);

        // 所有按钮都可选
        if (fireBtn != null) fireBtn.interactable = true;
        if (iceBtn != null) iceBtn.interactable = true;
        if (lightningBtn != null) lightningBtn.interactable = true;

        Show();
    }

    /// <summary>
    /// 更新按钮显示（目标选择：排除来源）
    /// </summary>
    private void UpdateButtonForTarget(Button btn, TextMeshProUGUI txt, int stack, bool isExcluded)
    {
        if (txt != null)
            txt.text = stack.ToString();

        if (btn != null)
        {
            btn.interactable = !isExcluded;
            var colors = btn.colors;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            btn.colors = colors;
        }
    }
}
