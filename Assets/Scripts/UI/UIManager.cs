using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

//UI管理器
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public Transform canvasTf;//画布的变化组件

    /// <summary>Dictionary O(1) 查找，替代 List O(n) 线性搜索</summary>
    private Dictionary<string, UIBase> _uiDict;

    private void Awake()
    {
        Instance = this;

        //找世界中的画布
        canvasTf = GameObject.Find("Canvas").transform;

        //初始化集合
        _uiDict = new Dictionary<string, UIBase>();
    }

    //显示
    public UIBase ShowUI<T>(string uiName) where T : UIBase
    {
        if (!_uiDict.TryGetValue(uiName, out UIBase ui) || ui == null)
        {
            GameObject obj = Instantiate(ResourceCache.Get<GameObject>("UI/" + uiName), canvasTf) as GameObject;
            obj.name = uiName;
            ui = obj.GetComponent<T>();          // 尝试获取已有组件
            if (ui == null)
            {
                ui = obj.AddComponent<T>();      // 没有则添加
            }
            _uiDict[uiName] = ui;
        }
        ui.Show(); // 首次加载和已存在都需要调用 Show
        return ui;
    }
    //隐藏
    public void HideUI(string uiName)
    {
        if (_uiDict.TryGetValue(uiName, out UIBase ui) && ui != null)
        {
            ui.Hide();
        }
    }

    //关闭所有界面
    public void CloseAllUI()
    {
        foreach (var ui in _uiDict.Values)
        {
            if (ui != null)
                Destroy(ui.gameObject);
        }
        _uiDict.Clear();
    }

    //关闭某个界面
    public void CloseUI(string uiName)
    {
        if (_uiDict.TryGetValue(uiName, out UIBase ui) && ui != null)
        {
            _uiDict.Remove(uiName);
            Destroy(ui.gameObject);
        }
    }

    //从集合中找到名字对应的界面脚本（O(1)）
    public UIBase Find(string uiName)
    {
        _uiDict.TryGetValue(uiName, out UIBase ui);
        return ui;
    }

    //获得某个界面的脚本
    public T GetUI<T>(string uiName) where T : UIBase
    {
        if (_uiDict.TryGetValue(uiName, out UIBase ui) && ui != null)
        {
            return ui.GetComponent<T>();
        }
        return null;   
    }

    //创建敌人头部的行动图标物体
    public GameObject CreateActionIcon()
    {
        GameObject prefab = ResourceCache.Get<GameObject>("UI/actionIcon");
        if (prefab == null)
        {
            Debug.LogError("行动图标预制体加载失败: UI/actionIcon");
            return null;
        }
        GameObject obj = Instantiate(prefab, canvasTf) as GameObject;
        obj.transform.SetAsFirstSibling();//设置在父级的第一位
        return obj;
    }

    //创建敌人底部的血量物体
    public GameObject CreateHpItem()
    {
        GameObject prefab = ResourceCache.Get<GameObject>("UI/HpItem");
        if (prefab == null)
        {
            Debug.LogError("血量UI预制体加载失败: UI/HpItem");
            return null;
        }
        GameObject obj = Instantiate(prefab, canvasTf) as GameObject;
        obj.transform.SetAsFirstSibling();//设置在父级的第一位
        return obj;
    }

    //提示界面
    public void ShowTip(string msg, Color color, System.Action callback = null)
    {
        GameObject obj = PoolManager.Get("Tips");
        obj.transform.SetParent(canvasTf, false);
        obj.transform.localScale = Vector3.one;

        TextMeshProUGUI text = obj.transform.Find("bg/Text").GetComponent<TextMeshProUGUI>();
        text.text = msg; 
        text.color = color;
        Tween scalel = obj.transform.Find("bg").DOScaleY(1, 0.4f);
        Tween scale2 = obj.transform.Find("bg").DOScaleY(0, 0.4f);

        Sequence seq = DOTween.Sequence();
        seq.Append(scalel);
        seq.AppendInterval(0.5f);
        seq.Append(scale2);
        seq.AppendCallback(delegate ()
        {
            if (callback != null) callback();
            PoolManager.Release("Tips", obj);
        });
    }

    //受伤特效
    public void ShowDamageEffect()
    {
        GameObject effect = PoolManager.Get("DamageEffect");
        if (effect == null) return;
        effect.transform.SetParent(canvasTf, false);

        // 获取 Image 组件
        Image img = effect.GetComponent<Image>();
        if (img == null)
        {
            Debug.LogError("DamageEffect prefab missing Image component");
            PoolManager.Release("DamageEffect", effect);
            return;
        }

        // 初始 alpha = 0
        Color color = img.color;
        color.a = 0f;
        img.color = color;

        // 使用 DOTween 播放淡入淡出动画：0 -> 0.5 -> 0
        Sequence seq = DOTween.Sequence();
        seq.Append(img.DOFade(0.5f, 0.1f));  // 快速淡入到半透明
        seq.Append(img.DOFade(0f, 0.3f));    // 缓慢淡出
        seq.OnComplete(() => PoolManager.Release("DamageEffect", effect)); // 动画完成后归还
    }

    // 获取所有已加载的 UI
    public List<UIBase> GetAllUI()
    {
        return new List<UIBase>(_uiDict.Values);
    }
}
