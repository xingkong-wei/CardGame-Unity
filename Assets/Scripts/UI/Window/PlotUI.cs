using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlotUI : UIBase
{
    private Dictionary<string, int> storyIdMap;

    private void Awake()
    {
        // 初始化名称到ID的映射
        storyIdMap = new Dictionary<string, int>
        {
            { "Preface", 0 },
            { "0", 1 }, { "1", 2 }, { "2", 3 }, { "3", 4 },
            { "4", 5 }, { "5", 6 }, { "6", 7 }, { "7", 8 },
            { "8", 9 }, { "9", 10 }, { "10", 11 }, { "11", 12 },
            { "12", 13 }, { "13", 14 }, { "14", 15 }, { "15", 16 },
            { "16", 17 }, { "17", 18 }, { "18", 19 }, { "19", 20 },
            { "20", 21 }, { "21", 22 },
            { "Ending", 23 }
        };

        // 添加鼠标中键滚动组件
        if (gameObject.GetComponent<UIMouseScroll>() == null)
            gameObject.AddComponent<UIMouseScroll>();

        // 返回按钮
        Transform returnBtn = transform.Find("ReturnBtn");
        if (returnBtn != null)
            returnBtn.GetComponent<Button>().onClick.AddListener(OnReturnBtnClick);
        else
            Debug.LogWarning("ReturnBtn not found in PlotUI");

        // 遍历所有故事按钮
        foreach (var kvp in storyIdMap)
        {
            string parentName = kvp.Key;
            int storyId = kvp.Value;

            // 构造按钮路径：Scroll View/Viewport/Content/{parentName}/Button
            Transform btnTrans = transform.Find($"Scroll View/Viewport/Content/{parentName}/Button");
            if (btnTrans != null)
            {
                Button btn = btnTrans.GetComponent<Button>();
                if (btn != null)
                {
                    int id = storyId; // 闭包捕获
                    btn.onClick.AddListener(() => OnStoryButtonClick(id));
                }
                else
                {
                    Debug.LogWarning($"Button component missing in {parentName}/Button");
                }
            }
            else
            {
                Debug.LogWarning($"Story button path not found: Scroll View/Viewport/Content/{parentName}/Button");
            }
        }
    }

    private void OnReturnBtnClick() => Close();

    private void OnStoryButtonClick(int storyId)
    {
        Hide(); // 隐藏地图界面

        // 隐藏战斗界面（如果存在）
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null) fightUI.Hide();

        // 显示故事界面并设置故事ID
        StoryUI storyUI = (StoryUI)UIManager.Instance.ShowUI<StoryUI>("StoryUI");
        storyUI.SetStoryId(storyId);
    }
}