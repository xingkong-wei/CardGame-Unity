using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingUI_Login : UIBase
{
    [Header("返回按钮")]
    public Button returnBtn;

    [Header("主音量")]
    public Slider mainSlider;
    public TextMeshProUGUI mainText;

    [Header("战斗音量")]
    public Slider musicSlider;
    public TextMeshProUGUI musicText;

    [Header("音效音量")]
    public Slider effectSlider;
    public TextMeshProUGUI effectText;

    private void Awake()
    {
        returnBtn.onClick.AddListener(OnReturnBtnClick);

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

    //返回按钮
    private void OnReturnBtnClick()
    {
        Close();
        DOTween.KillAll();
        UIManager.Instance.ShowUI<LoginUI>("LoginUI");
    }
}