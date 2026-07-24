using DG.Tweening;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//FightUI界面的设置按钮
public class GameSettingUI : UIBase
{
    private string previousUIName; // 记录从哪个界面打开

    [Header("按钮")]
    public Button returnBtn;
    public Button quitBtn;
    public Button saveBtn;  // 保存并退出（待实现）

    [Header("主音量")]
    public Slider mainSlider;
    public TextMeshProUGUI mainText;

    [Header("战斗音量")]
    public Slider musicSlider;
    public TextMeshProUGUI musicText;

    [Header("音效音量")]
    public Slider effectSlider;
    public TextMeshProUGUI effectText;

    public void SetPreviousUIName(string name)
    {
        previousUIName = name;
    }

    private void Awake()
    {
        returnBtn.onClick.AddListener(OnReturnBtnClick);
        quitBtn.onClick.AddListener(OnQuitBtnClick);

        mainSlider.value = AudioManager.Instance.MainVolume;
        mainText.text = (mainSlider.value * 100).ToString("F0") + "%";
        musicSlider.value = AudioManager.Instance.BattleVolume;
        musicText.text = (musicSlider.value * 100).ToString("F0") + "%";
        effectSlider.value = AudioManager.Instance.EffectVolume;
        effectText.text = (effectSlider.value * 100).ToString("F0") + "%";

        mainSlider.onValueChanged.AddListener(OnMainVolumeChanged);
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        effectSlider.onValueChanged.AddListener(OnEffectVolumeChanged);
    }

    //返回按钮事件
    private void OnReturnBtnClick()
    {
        // 关闭当前设置界面
        Close();

        // 根据来源界面返回
        if (!string.IsNullOrEmpty(previousUIName))
        {
            // 检查该界面是否已经存在于 UIManager 中
            UIBase previousUI = UIManager.Instance.Find(previousUIName);
            if (previousUI != null)
            {
                // 如果存在，直接显示（适用于 FightUI 等未被销毁的情况）
                previousUI.Show();
            }
            else
            {
                // 不存在，重新创建（适用于 LoginUI 等已被销毁的情况）
                // 根据名字创建对应的界面
                switch (previousUIName)
                {
                    case "LoginUI":
                        UIManager.Instance.ShowUI<LoginUI>("LoginUI");
                        break;
                    case "FightUI":
                        UIManager.Instance.ShowUI<FightUI>("FightUI");
                        break;
                    // 可继续添加其他可能的来源界面
                    default:
                        Debug.LogWarning($"未知的上一界面：{previousUIName}，无法返回");
                        break;
                }
            }
        }
    }

    //放弃按钮事件
    private void OnQuitBtnClick()
    {
        // 清除当前战斗中的敌人（避免残留）
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.ClearAllEnemies();

        // 关闭所有UI
        UIManager.Instance.CloseAllUI();

        // 终止所有 DOTween 动画，防止相机震动残留
        DOTween.KillAll();

        // 显示登录界面
        UIManager.Instance.ShowUI<LoginUI>("LoginUI");

        // 切换回登录背景音乐
        AudioManager.Instance.PlayBGM("bgm1");
    }

    //主音量调节
    private void OnMainVolumeChanged(float val)
    {
        mainText.text = (val * 100).ToString("F0") + "%";
        AudioManager.Instance.SetMainVolume(val);
    }

    //战斗音量调节
    private void OnMusicVolumeChanged(float val)
    {
        musicText.text = (val * 100).ToString("F0") + "%";
        AudioManager.Instance.SetBattleVolume(val);
    }

    //音效音量调节
    private void OnEffectVolumeChanged(float val)
    {
        effectText.text = (val * 100).ToString("F0") + "%";
        AudioManager.Instance.SetEffectVolume(val);
    }
}