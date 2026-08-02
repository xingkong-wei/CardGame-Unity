using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EncyclpediaUI : UIBase
{
    [Header("按钮")]
    public Button returnBtn;
    public Button cardBtn;
    public Button potionBtn;
    public Button reliqueBtn;
    public Button enemyBtn;
    public Button roleDataBtn;
    public Button historicalDataBtn;

    // 记录图鉴打开前哪个登录界面是可见的
    private static string previousUIName = null;

    private void Awake()
    {
        if (returnBtn != null)
            returnBtn.onClick.AddListener(OnReturnBtnClick);
        if (cardBtn != null)
            cardBtn.onClick.AddListener(OnCardBtnClick);
        if (potionBtn != null)
            potionBtn.onClick.AddListener(OnPotionBtnClick);
        if (reliqueBtn != null)
            reliqueBtn.onClick.AddListener(OnReliqueBtnClick);
        if (enemyBtn != null)
            enemyBtn.onClick.AddListener(OnEnemyBtnClick);
        if (roleDataBtn != null)
            roleDataBtn.onClick.AddListener(OnRoleDataBtnClick);
        if (historicalDataBtn != null)
            historicalDataBtn.onClick.AddListener(OnHistoricalDataBtnClick);
    }

    private void Start()
    {
        // 记录当前可见的登录界面
        if (UIManager.Instance.Find("LoginUI") != null &&
            UIManager.Instance.Find("LoginUI").gameObject.activeSelf)
        {
            previousUIName = "LoginUI";
        }
        else if (UIManager.Instance.Find("LoginUI_Exit") != null &&
                 UIManager.Instance.Find("LoginUI_Exit").gameObject.activeSelf)
        {
            previousUIName = "LoginUI_Exit";
        }

        // 隐藏所有登录界面
        UIManager.Instance.HideUI("LoginUI");
        UIManager.Instance.HideUI("LoginUI_Exit");
    }

    private void OnReturnBtnClick()
    {
        // 恢复之前可见的登录界面
        if (!string.IsNullOrEmpty(previousUIName))
        {
            if (previousUIName == "LoginUI")
                UIManager.Instance.ShowUI<LoginUI>("LoginUI");
            else if (previousUIName == "LoginUI_Exit")
                UIManager.Instance.ShowUI<LoginUI_Exit>("LoginUI_Exit");
        }
        previousUIName = null;
        Close();
    }

    private void OnCardBtnClick()
    {
        UIManager.Instance.ShowUI<CardBagUI>("CardBagUI");
    }

    private void OnPotionBtnClick()
    {
        UIManager.Instance.ShowUI<PotionBagUI>("PotionBagUI");
    }

    private void OnReliqueBtnClick()
    {
        UIManager.Instance.ShowUI<RelicBagUI>("RelicBagUI");
    }

    private void OnEnemyBtnClick()
    {
        // TODO: 打开怪物图鉴
    }

    private void OnRoleDataBtnClick()
    {
        // TODO: 打开角色数据
    }

    private void OnHistoricalDataBtnClick()
    {
        // TODO: 打开历史数据
    }
}
