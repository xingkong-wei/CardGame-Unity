using UnityEngine;

/// <summary>
/// 运行后直接显示商店 UI，方便调节布局参数
/// 挂到场景中任意 GameObject，勾选 Preview 启动
/// </summary>
public class ShopPreview : MonoBehaviour
{
    public bool preview;

    private bool lastPreview;
    private GameObject shopInstance;

    private void Start()
    {
        // 初始化系统
        if (GameConfigManager.Instance != null)
            GameConfigManager.Instance.Init();

        RoleManager.Instance?.Init();

        if (FightManager.Instance != null)
        {
            FightManager.Instance.InitRelics();
            FightManager.Instance.AddCoin(500);
        }

        ShowShop();
    }

    private void Update()
    {
        if (preview != lastPreview)
        {
            lastPreview = preview;
            if (preview) ShowShop();
        }
    }

    private void ShowShop()
    {
        if (!Application.isPlaying) return;
        UIManager.Instance.ShowUI<ShopUI>("ShopUI");
    }
}
