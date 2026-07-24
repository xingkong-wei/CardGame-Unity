using UnityEngine;
using System.Collections;

public class ExitManager : MonoBehaviour
{
    private static ExitManager _instance;
    public static ExitManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ExitManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("ExitManager");
                    _instance = go.AddComponent<ExitManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void OnExitGameClicked()
    {
        // 重置战斗计时器
        FightUI.ResetBattleTimer();

        // 重置血量
        FightManager.ResetHp();

        // 播放退出音乐（直接调用 AudioManager）
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBGM("Exit");
        else
            Debug.LogError("AudioManager 实例不存在！");

        // 显示退出 UI（复用 UIManager 的 Canvas 引用）
        ShowExitUI();

        // 延迟退出
        StartCoroutine(DelayedQuit(1f));
    }

    private void ShowExitUI()
    {
        GameObject prefab = Resources.Load<GameObject>("UI/ExitUI");
        if (prefab == null)
        {
            Debug.LogError("ExitUI 预制体未找到！");
            return;
        }

        // 优先使用 UIManager 中的 Canvas，避免重复查找
        Transform parent = UIManager.Instance?.canvasTf;
        if (parent == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
                parent = canvas.transform;
            else
            {
                Debug.LogError("场景中没有 Canvas，无法显示 ExitUI！");
                return;
            }
        }

        Instantiate(prefab, parent, false);
    }

    private IEnumerator DelayedQuit(float delay)
    {
        yield return new WaitForSeconds(delay);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}