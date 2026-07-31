using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LossUI : UIBase
{
    [Header("按钮")]
    public Button restartBtn;
    public Button quitBtn;
    public Button backBtn;

    private void Awake()
    {
        restartBtn.onClick.AddListener(RestartGame);
        quitBtn.onClick.AddListener(QuitGame);
        backBtn.onClick.AddListener(BackToLogin);
    }

    private void RestartGame()
    {
        DOTween.KillAll();
        CleanupEnemies();
        UIManager.Instance.CloseAllUI();          // 关闭所有 UI（包括自身）
        RoleManager.Instance.Init();               // 重置玩家卡组
        RoleManager.Instance.ApplyUpgradesToDeck();
        FightManager.Instance.ChangeType(FightType.Init);
    }

    private void QuitGame()
    {
        UIManager.Instance.CloseAllUI();
        ExitManager.Instance.OnExitGameClicked();
    }

    private void BackToLogin()
    {
        DOTween.KillAll();
        CleanupEnemies();
        UIManager.Instance.CloseAllUI();
        UIManager.Instance.ShowUI<LoginUI>("LoginUI");
        AudioManager.Instance.PlayBGM("bgm1");
    }

    private void CleanupEnemies()
    {
        foreach (Enemy enemy in EnemyManager.Instance.GetEnemyList())
        {
            if (enemy != null && enemy.gameObject != null)
                Object.Destroy(enemy.gameObject);
        }
        EnemyManager.Instance.GetEnemyList().Clear();
    }
}