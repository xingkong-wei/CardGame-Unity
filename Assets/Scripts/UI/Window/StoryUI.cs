using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryUI : UIBase
{
    [Header("UI组件")]
    public TextMeshProUGUI storyText;
    public ScrollRect scrollRect;
    public Button returnBtn;
    public Button skipBtn;

    [Header("设置")]
    public float typeSpeed = 0.05f;

    private AudioSource audioSource;
    private string fullContent;
    private Coroutine typeCoroutine;
    private int currentStoryId = -1;

    private void Awake()
    {
        // 添加鼠标中键滚动组件
        if (gameObject.GetComponent<UIMouseScroll>() == null)
            gameObject.AddComponent<UIMouseScroll>();

        // 创建用于播放键盘音效的 AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        // 监听音量改变事件
        AudioManager.Instance.OnEffectVolumeChanged += OnEffectVolumeChanged;

        returnBtn.onClick.AddListener(OnReturnBtnClick);
        skipBtn.onClick.AddListener(OnSkipClick);
    }

    private void OnDestroy()
    {
        // 取消事件订阅
        if (AudioManager.Instance != null)
            AudioManager.Instance.OnEffectVolumeChanged -= OnEffectVolumeChanged;
    }

    private void OnEffectVolumeChanged(float volume)
    {
        if (audioSource != null)
            audioSource.volume = volume;
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        if (audioSource.isPlaying) audioSource.Stop();
        storyText.text = "";
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (audioSource.isPlaying) audioSource.Stop();
    }

    public void SetStoryId(int id)
    {
        currentStoryId = id;
        LoadStory(id);
    }

    private void LoadStory(int id)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Data/Stories");
        if (jsonFile == null)
        {
            Debug.LogError("Stories.json not found at Resources/Data/");
            return;
        }

        StoryData[] stories = JsonHelper.FromJson<StoryData>(jsonFile.text);
        if (stories == null || stories.Length == 0)
        {
            Debug.LogError("Failed to parse Stories.json");
            return;
        }

        foreach (var story in stories)
        {
            if (story.id == id)
            {
                fullContent = story.content;
                break;
            }
        }

        if (string.IsNullOrEmpty(fullContent))
        {
            Debug.LogError($"Story with id {id} not found.");
            return;
        }

        if (typeCoroutine != null)
            StopCoroutine(typeCoroutine);
        typeCoroutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        storyText.text = "";
        AudioManager.Instance.PlayBGM("Win");

        // 显示跳过按钮
        if (skipBtn != null) skipBtn.gameObject.SetActive(true);

        // 播放键盘音效
        AudioClip clip = Resources.Load<AudioClip>("Sounds/Effect/KeyboardEffect");
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.volume = AudioManager.Instance.EffectVolume; // 初始音量
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("KeyboardEffect.mp3 not found");
        }

        foreach (char c in fullContent)
        {
            storyText.text += c;
            yield return new WaitForSeconds(typeSpeed);

            if (scrollRect != null && scrollRect.content != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f; // 若滚动方向相反，改为1f
            }
        }

        audioSource.Stop();

        // 文字显示完毕，隐藏跳过按钮
        if (skipBtn != null) skipBtn.gameObject.SetActive(false);
        typeCoroutine = null;
    }

    //跳过剧情按钮
    private void OnSkipClick()
    {
        if (typeCoroutine == null) return; // 没有正在播放的剧情

        // 停止打字协程
        StopCoroutine(typeCoroutine);
        typeCoroutine = null;

        // 立即显示完整文本
        storyText.text = fullContent;

        // 滚动到底部
        if (scrollRect != null && scrollRect.content != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }

        // 停止音效
        if (audioSource.isPlaying) audioSource.Stop();

        // 隐藏跳过按钮
        if (skipBtn != null) skipBtn.gameObject.SetActive(false);
    }

    //返回按钮
    private void OnReturnBtnClick()
    {
        Close();

        PlotUI mapUI = UIManager.Instance.GetUI<PlotUI>("PlotUI");
        if (mapUI != null) mapUI.Show();
        else UIManager.Instance.ShowUI<PlotUI>("PlotUI");

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null) fightUI.Show();

        AudioManager.Instance.PlayBGM("battle");
    }

    [System.Serializable]
    public class StoryData { public int id; public string title; public string content; }

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string newJson = "{\"array\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
        }
        [System.Serializable] private class Wrapper<T> { public T[] array; }
    }
}