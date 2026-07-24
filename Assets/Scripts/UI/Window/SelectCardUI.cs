using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SelectCardUI : UIBase
{
    [Header("标题文本")]
    public TextMeshProUGUI titleText;

    [Header("三个奖励按钮")]
    public Button potionButton;
    public Button relicsButton;
    public Button cardButton;

    [Header("跳过奖励按钮")]
    public Button nextButton;

    [Header("Invemtory 挂载点")]
    public Transform inventoryMount; // SelectCardUI 中放 Invemtory 的位置

    public static System.Action OnClosed;

    private bool cardDone = false;
    private bool potionDone = false;
    private bool relicDone = false;
    private PotionData rolledPotion;
    private RelicData rolledRelic;

    private Transform inventoryOriginal;
    private int inventoryOriginalIndex;
    private Vector2 inventoryOriginalPos;
    private Vector2 inventoryOriginalSize;
    private Vector3 inventoryOriginalScale;
    private FightUI inventoryFightUI; // 保存引用，避免销毁后访问
    private bool inventoryMoved = false;

    private void Start()
    {
        PlayShowAnimation();

        RewardInterfaceUI.OnCardRewardSelected += OnCardRewardComplete;

        if (potionButton != null)
            potionButton.onClick.AddListener(OnPotionReward);
        if (relicsButton != null)
            relicsButton.onClick.AddListener(OnRelicsReward);
        if (cardButton != null)
            cardButton.onClick.AddListener(OnCardReward);
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClick);
    }

    /// <summary>
    /// 由 Fight_Win 调用，设置本场战斗是否掉落药水
    /// </summary>
    public void SetDroppedPotion(PotionData potion)
    {
        rolledPotion = potion;
        TextMeshProUGUI btnText = potionButton?.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();

        if (potion != null)
        {
            if (btnText != null) btnText.text = potion.potionName;
        }
        else
        {
            if (btnText != null) btnText.text = "无药水奖励";
            if (potionButton != null) potionButton.interactable = false;
            potionDone = true;
        }
    }

    public void SetDroppedRelic(RelicData relic)
    {
        rolledRelic = relic;
        TextMeshProUGUI btnText = relicsButton?.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();

        if (relic != null)
        {
            if (btnText != null) btnText.text = relic.relicName;
        }
        else
        {
            if (btnText != null) btnText.text = "无遗物奖励";
            if (relicsButton != null) relicsButton.interactable = false;
            relicDone = true;
        }
    }

    public override void Show()
    {
        base.Show();
        MountInventory();
        // MapBtn 需要先关闭 SelectCardUI 再打开 MapUI，确保层级正确
        OverrideMapBtn();
    }

    private void OverrideMapBtn()
    {
        if (inventoryFightUI == null) return;
        Transform mapBtnTf = inventoryFightUI.transform.Find("Invemtory/MapBtn");
        if (mapBtnTf == null) return;
        Button mapBtn = mapBtnTf.GetComponent<Button>();
        if (mapBtn == null) return;
        mapBtn.onClick.RemoveAllListeners();
        mapBtn.onClick.AddListener(() =>
        {
            MapUI mapUI = UIManager.Instance.ShowUI<MapUI>("MapUI") as MapUI;
            if (mapUI != null) mapUI.transform.SetAsLastSibling();
        });
    }

    public override void Close()
    {
        UnmountInventory();
        base.Close();
        OnClosed?.Invoke();
    }

    private void OnDestroy()
    {
        RewardInterfaceUI.OnCardRewardSelected -= OnCardRewardComplete;
        RewardInterfaceUI.ClearCache();
    }

    // ===== Invemtory 挂载 =====

    private void MountInventory()
    {
        if (inventoryMoved) return;

        inventoryFightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (inventoryFightUI == null) return;

        Transform invemtory = inventoryFightUI.transform.Find("Invemtory");
        if (invemtory == null || inventoryMount == null) return;

        inventoryOriginal = invemtory.parent;
        inventoryOriginalIndex = invemtory.GetSiblingIndex();
        RectTransform invRT = invemtory.GetComponent<RectTransform>();
        inventoryOriginalPos = invRT.anchoredPosition;
        inventoryOriginalSize = invRT.sizeDelta;
        inventoryOriginalScale = invRT.localScale;

        invemtory.SetParent(inventoryMount, true);
        invRT.anchoredPosition = Vector2.zero;
        // 确保 Invemtory 在 SelectCardUI 最上层
        inventoryMount.SetAsLastSibling();
        inventoryMoved = true;
    }

    private void UnmountInventory()
    {
        if (!inventoryMoved) return;
        inventoryMoved = false;

        Transform invemtory = inventoryMount.Find("Invemtory");
        if (invemtory == null) return;
        if (inventoryOriginal == null || inventoryOriginal.gameObject == null) return;

        invemtory.SetParent(inventoryOriginal, false);
        invemtory.SetSiblingIndex(inventoryOriginalIndex);
        RectTransform invRT = invemtory.GetComponent<RectTransform>();
        invRT.anchoredPosition = inventoryOriginalPos;
        invRT.sizeDelta = inventoryOriginalSize;
        invRT.localScale = inventoryOriginalScale;

        inventoryFightUI = null;
    }

    // ===== 动画 =====

    private void PlayShowAnimation()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(1, 0.3f).SetEase(Ease.OutBack);
    }

    // ===== 回调 =====

    private void OnCardRewardComplete()
    {
        cardDone = true;
        CheckAllDone();
    }

    // ===== 药水奖励 =====

    private void OnPotionReward()
    {
        if (potionDone) return;
        if (rolledPotion == null) return;

        if (FightManager.Instance.potionList.Count >= 3)
        {
            UIManager.Instance.ShowTip("药水栏已满", Color.red);
            return;
        }

        FightManager.Instance.potionList.Add(rolledPotion);
        RefreshPotionUI();
        potionDone = true;
        if (potionButton != null) potionButton.interactable = false;
        UIManager.Instance.ShowTip($"获得药水：{rolledPotion.potionName}", Color.yellow);
        CheckAllDone();
    }

    private void RefreshPotionUI()
    {
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null && fightUI.potionObj != null)
        {
            PotionPanelController ppc = fightUI.potionObj.GetComponent<PotionPanelController>();
            if (ppc != null) ppc.RefreshPotionButtons();
        }
    }

    // ===== 遗物奖励 =====

    private void OnRelicsReward()
    {
        if (relicDone) return;
        if (rolledRelic == null) return;

        FightManager.Instance.AddRelic(rolledRelic);
        relicDone = true;
        if (relicsButton != null) relicsButton.interactable = false;
        UIManager.Instance.ShowTip($"获得遗物：{rolledRelic.relicName}", Color.yellow);
        CheckAllDone();
    }

    // ===== 卡牌奖励 =====

    private void OnCardReward()
    {
        if (cardDone) return;
        RewardInterfaceUI.ShowReward();
    }

    // ===== 完成检查 =====

    private void CheckAllDone()
    {
        if (cardDone && potionDone && relicDone)
        {
            StartCoroutine(DelayClose(0.2f));
        }
    }

    private void OnNextButtonClick()
    {
        StartCoroutine(DelayClose(0.2f));
    }

    private IEnumerator DelayClose(float delay)
    {
        yield return new WaitForSeconds(delay);
        Close();
    }
}
