using UnityEngine;
using UnityEngine.UI;

public class LoginUI_Exit : UIBase
{
    [Header("按钮")]
    public Button startBtn;
    public Button quitBtn;
    public Button setBtn;
    public Button encyclpediaBtn;
    public Button continueBtn;

    private void Awake()
    {
        if (startBtn != null)
            startBtn.onClick.AddListener(OnStartGame);
        if (quitBtn != null)
            quitBtn.onClick.AddListener(OnExitGame);
        if (setBtn != null)
            setBtn.onClick.AddListener(OnSetGame);
        if (encyclpediaBtn != null)
            encyclpediaBtn.onClick.AddListener(OnEncyclpedia);
        if (continueBtn != null)
            continueBtn.onClick.AddListener(OnContinue);
    }

    private void OnStartGame()
    {
        Close();

        if (PlayerPrefs.HasKey("Map"))
        {
            PlayerPrefs.DeleteKey("Map");
            PlayerPrefs.Save();
        }

        FightUI.ResetBattleTimer();
        FightManager.ResetHp();
        FightCardManager.Instance.ResetForNewGame();

        MapUI mapUI = UIManager.Instance.ShowUI<MapUI>("MapUI") as MapUI;
        mapUI.OnNewGameStarted();
    }

    private void OnExitGame()
    {
        Close();
        ExitManager.Instance.OnExitGameClicked();
    }

    private void OnSetGame()
    {
        Close();
        UIManager.Instance.ShowUI<GameSettingUI_Login>("GameSettingUI_Login");
    }

    private void OnEncyclpedia()
    {
        UIManager.Instance.ShowUI<EncyclpediaUI>("EncyclpediaUI");
    }

    private void OnContinue()
    {
        // TODO: 继续游戏
    }
}
