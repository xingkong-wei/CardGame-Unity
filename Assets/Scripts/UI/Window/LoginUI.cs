using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoginUI : UIBase
{
    [Header("按钮")]
    public Button startBtn;
    public Button newBtn;
    public Button quitBtn;
    public Button setBtn;
    public Button encyclpediaBtn;

    private void Awake()
    {
        startBtn.onClick.AddListener(() => onStartGameBtn(startBtn.gameObject, null));
        newBtn.onClick.AddListener(() => onNewGameBtn(newBtn.gameObject, null));
        quitBtn.onClick.AddListener(() => onExitGameBtn(quitBtn.gameObject, null));
        setBtn.onClick.AddListener(() => onSetGameBtn(setBtn.gameObject, null));
        encyclpediaBtn.onClick.AddListener(() => onEncyclpediaBtn(encyclpediaBtn.gameObject, null));
    }

    //开始游戏按钮事件
    private void onStartGameBtn(GameObject obj, PointerEventData pData)
    {
        //关闭login界面
        Close();

        // 清除保存的地图数据,重新开始游戏
        if (SaveFileManager.HasKey("Map"))
        {
            SaveFileManager.DeleteKey("Map");
            SaveFileManager.Flush();
        }

        // 清除旧升级数据，重新初始化卡组
        SaveFileManager.DeleteKey("SavedCurHp");
        SaveFileManager.DeleteKey("UpgradedCardIds");
        SaveFileManager.Flush();
        RoleManager.Instance.Init();
        RoleManager.Instance.ApplyUpgradesToDeck();

        // 重置战斗计时器
        FightUI.ResetBattleTimer();

        // 重置血量
        FightManager.ResetHp();

        // 重置战斗卡牌管理器（清空所有战斗相关数据，包括废牌堆）
        FightCardManager.Instance.ResetForNewGame();

        // 显示地图界面
        MapUI mapUI = UIManager.Instance.ShowUI<MapUI>("MapUI") as MapUI;

        // 重置游戏状态
        mapUI.OnNewGameStarted();
    }

    //退出游戏按钮事件
    private void onExitGameBtn(GameObject obj, PointerEventData pData)
    {
        //关闭login界面
        Close();

        ExitManager.Instance.OnExitGameClicked();
    }

    //设置按钮事件
    private void onSetGameBtn(GameObject obj, PointerEventData pData)
    {
        Close(); // 关闭登录界面
        UIManager.Instance.ShowUI<GameSettingUI_Login>("GameSettingUI_Login");
    }

    //新游戏按钮事件（清除所有存档数据）
    private void onNewGameBtn(GameObject obj, PointerEventData pData)
    {
        SaveManager.ClearAllGameData();
        FightCardManager.Instance.ResetForNewGame();
        RoleManager.Instance.Init();
        RoleManager.Instance.ApplyUpgradesToDeck();
        FightUI.ResetBattleTimer();
        FightManager.ResetHp();
        FightManager.Instance.relicList.Clear();
        FightManager.Instance.potionList.Clear();

        // 清除所有岛屿地图
        SaveFileManager.DeleteKeysByPrefix("Map_Island_");
        SaveFileManager.DeleteKey("Map");
        SaveFileManager.DeleteKey("UpgradedCardIds");
        SaveFileManager.DeleteKey("SavedCurHp");
        SaveFileManager.DeleteKey("SavedMaxHp");
        SaveFileManager.DeleteKey("SavedCoinAmount");
        SaveFileManager.DeleteKey("SavedIslandIndex");
        SaveFileManager.Flush();

        Close();
        MapUI mapUI = UIManager.Instance.ShowUI<MapUI>("MapUI") as MapUI;
        mapUI.OnNewGameStarted();
    }

    //图鉴按钮事件
    private void onEncyclpediaBtn(GameObject obj, PointerEventData pData)
    {
        UIManager.Instance.ShowUI<EncyclpediaUI>("EncyclpediaUI");
    }
}
