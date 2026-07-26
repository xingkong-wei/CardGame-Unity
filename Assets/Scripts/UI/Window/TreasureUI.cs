using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TreasureUI : UIBase
{
    [Header("宝箱状态")]
    public GameObject closedObj;
    public GameObject openObj;

    [Header("遗物奖励")]
    public Transform relicMount;     // RelicIcon 预制体挂载点
    public TextMeshProUGUI goldText; // 金币数量文本

    [Header("操作按钮")]
    public Button takeInBtn;         // 摄取
    public Button discardBtn;        // 丢弃

    [Header("特效")]
    public GameObject openEffectPrefab;
    public Transform effectSpawnPoint; // 特效生成位置

    public System.Action OnClosed;

    private RelicData rolledRelic;
    private int rolledGold;
    private TreasureConfigData chestType;
    private GameObject relicInstance;
    private bool eventsBound;

    private void Start()
    {
        BindEvents();
    }

    private void BindEvents()
    {
        if (eventsBound) return;
        eventsBound = true;

        // closed 按钮（通过 closedObj 获取 Button）
        Button closedBtn = closedObj?.GetComponent<Button>();
        if (closedBtn != null)
        {
            closedBtn.onClick.RemoveAllListeners();
            closedBtn.onClick.AddListener(OnClickClosed);
        }

        if (takeInBtn != null)
        {
            takeInBtn.onClick.RemoveAllListeners();
            takeInBtn.onClick.AddListener(OnClickTakeIn);
        }

        if (discardBtn != null)
        {
            discardBtn.onClick.RemoveAllListeners();
            discardBtn.onClick.AddListener(OnClickDiscard);
        }
    }

    public override void Show()
    {
        base.Show();

        chestType = GameConfig.Instance.RollRandomChest();

        // 重置
        closedObj.SetActive(true);
        openObj.SetActive(false);
        if (takeInBtn != null) takeInBtn.gameObject.SetActive(false);
        if (discardBtn != null) discardBtn.gameObject.SetActive(false);
        if (goldText != null) goldText.gameObject.SetActive(false);
        if (relicInstance != null) { Destroy(relicInstance); relicInstance = null; }

        RollReward();
    }

    /// <summary>
    /// 点击关闭的宝箱 → 打开
    /// </summary>
    public void OnClickClosed()
    {
        closedObj.SetActive(false);
        openObj.SetActive(true);

        // 特效暂时不使用

        ShowReward();
    }

    /// <summary>
    /// 摄取遗物 + 金币
    /// </summary>
    public void OnClickTakeIn()
    {
        if (rolledRelic != null)
            FightManager.Instance.AddRelic(rolledRelic);
        FightManager.Instance.AddCoin(rolledGold);

        // 刷新遗物栏 UI
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null && fightUI.relicsUI != null)
            fightUI.relicsUI.RefreshUI();

        Close();
        OnClosed?.Invoke();
    }

    /// <summary>
    /// 丢弃遗物，只拿金币
    /// </summary>
    public void OnClickDiscard()
    {
        FightManager.Instance.AddCoin(rolledGold);

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
            fightUI.UpdateCoinDisplay(FightManager.Instance.CoinAmount);

        Close();
        OnClosed?.Invoke();
    }

    private void RollReward()
    {
        if (chestType == null) return;

        // 宝箱必定有遗物
        rolledRelic = chestType.RollRelic();

        // 金币按配置概率获得
        bool gotGold = Random.value < chestType.goldChance;
        rolledGold = gotGold ? Random.Range(chestType.goldMin, chestType.goldMax + 1) : 0;
    }

    private void ShowReward()
    {
        // 金币（0 时不显示）
        if (goldText != null)
        {
            if (rolledGold > 0)
            {
                goldText.text = $"+ {rolledGold} 金币";
                goldText.gameObject.SetActive(true);
            }
            else
            {
                goldText.gameObject.SetActive(false);
            }
        }

        // 遗物
        if (rolledRelic != null && relicMount != null)
        {
            relicInstance = Instantiate(Resources.Load<GameObject>("UI/RelicIcon"), relicMount);
            RelicIcon ri = relicInstance.GetComponent<RelicIcon>();
            if (ri != null) ri.Setup(rolledRelic);
        }

        // 按钮弹出动画（scale=0 时不可点击，动画结束后恢复）
        if (takeInBtn != null)
        {
            takeInBtn.gameObject.SetActive(true);
            takeInBtn.interactable = false;
            takeInBtn.transform.localScale = Vector3.zero;
            takeInBtn.transform.DOScale(1, 0.4f).SetEase(Ease.OutBack)
                .OnComplete(() => { if (takeInBtn != null) takeInBtn.interactable = true; });
        }
        if (discardBtn != null)
        {
            discardBtn.gameObject.SetActive(true);
            discardBtn.interactable = false;
            discardBtn.transform.localScale = Vector3.zero;
            discardBtn.transform.DOScale(1, 0.4f).SetEase(Ease.OutBack).SetDelay(0.1f)
                .OnComplete(() => { if (discardBtn != null) discardBtn.interactable = true; });
        }
    }
}
