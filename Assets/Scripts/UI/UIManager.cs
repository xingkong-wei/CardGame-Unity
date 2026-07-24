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

    private List<UIBase> uiList;//存储加载过的界面集合

    private void Awake()
    {
        Instance = this;

        //找世界中的画布
        canvasTf = GameObject.Find("Canvas").transform;

        //初始化集合
        uiList = new List<UIBase>();
    }

    //显示
    public UIBase ShowUI<T>(string uiName) where T : UIBase
    {
        UIBase ui = Find(uiName);
        if (ui == null)
        {
            GameObject obj = Instantiate(Resources.Load("UI/" + uiName), canvasTf) as GameObject;
            obj.name = uiName;
            ui = obj.GetComponent<T>();          // 尝试获取已有组件
            if (ui == null)
            {
                ui = obj.AddComponent<T>();      // 没有则添加
            }
            uiList.Add(ui);
        }
        ui.Show(); // 首次加载和已存在都需要调用 Show
        return ui;
    }
    //隐藏
    public void HideUI(string uiName)
    {
        UIBase ui = Find(uiName);
        if (ui !=null)
        {
            ui.Hide();
        }
    }

    //关闭所有界面
    public void CloseAllUI()
    {
        for (int i = uiList.Count - 1; i >= 0; i--)
        {
            Destroy(uiList[i].gameObject);
        }

        uiList.Clear();//清空集合
    }

    //关闭某个界面
    public void CloseUI(string uiName)
    {
        UIBase ui = Find(uiName);
        if (ui != null)
        {
            uiList.Remove(ui);
            Destroy(ui.gameObject);
        }
    }

    //从集合中找到名字对应的界面脚本
    public UIBase Find(string uiName)
    {
        for (int i = 0; i < uiList.Count; i++)
        {
            if (uiList[i].name == uiName)
            {
                return uiList[i];
            }
        }
        return null;
    }

    //获得某个界面的脚本
    public T GetUI<T>(string uiName) where T : UIBase
    {
        UIBase ui = Find(uiName);
        if (ui != null)
        {
            return ui.GetComponent<T>();
        }
        return null;   
    }

    //创建敌人头部的行动图标物体
    public GameObject CreateActionIcon()
    {
        GameObject prefab = Resources.Load<GameObject>("UI/actionIcon");
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
        GameObject prefab = Resources.Load<GameObject>("UI/HpItem");
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
        GameObject obj = Instantiate(Resources.Load("UI/Tips"), canvasTf) as GameObject;
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
            if (callback != null)
            {
                callback();
            }
        });
        MonoBehaviour.Destroy(obj, 2);
    }

    //受伤特效
    public void ShowDamageEffect()
    {
        // 加载预制体
        GameObject prefab = Resources.Load<GameObject>("UI/DamageEffect");
        if (prefab == null)
        {
            Debug.LogError("DamageEffect prefab not found at Resources/UI/DamageEffect");
            return;
        }

        // 实例化到 Canvas 下
        GameObject effect = Instantiate(prefab, canvasTf);
        effect.name = "DamageEffect";

        // 获取 Image 组件
        Image img = effect.GetComponent<Image>();
        if (img == null)
        {
            Debug.LogError("DamageEffect prefab missing Image component");
            Destroy(effect);
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
        seq.OnComplete(() => Destroy(effect)); // 动画完成后销毁
    }

    // 获取所有已加载的 UI
    public List<UIBase> GetAllUI()
    {
        return new List<UIBase>(uiList);
    }
}
