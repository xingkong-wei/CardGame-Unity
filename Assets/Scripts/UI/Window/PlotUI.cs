using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlotUI : UIBase
{
    [Header("锁定状态图片")]
    public Sprite lockedSprite;          // 未解锁时显示的图片（Main.png）
    public Color lockedColor = new Color(0.451f, 0.451f, 0.451f); // 灰色 #737373

    /// <summary>
    /// 剧情节点信息
    /// </summary>
    private class StoryNodeInfo
    {
        public string nodeName;          // 节点名称（Prefab中的GameObject名）
        public int storyId;              // 剧情ID
        public int requiredIslandIndex;  // 需要击败的岛屿索引（击败该岛Boss后解锁）
                                         // -1 表示始终解锁（Preface）
        public Image nodeImage;          // 节点的 Image 组件
        public Sprite originalSprite;    // 原始图片（预制体中配置的）
        public Color originalColor;      // 原始颜色
        public Button button;            // 按钮组件
    }

    private Dictionary<string, int> storyIdMap;
    private List<StoryNodeInfo> storyNodes = new List<StoryNodeInfo>();

    private void Awake()
    {
        // 初始化名称到ID的映射
        storyIdMap = new Dictionary<string, int>
        {
            { "Preface", 0 },
            { "1", 1 }, { "2", 2 }, { "3", 3 }, { "4", 4 },
            { "5", 5 }, { "6", 6 }, { "7", 7 }, { "8", 8 },
            { "9", 9 }, { "10", 10 }, { "11", 11 }, { "12", 12 },
            { "13", 13 }, { "14", 14 }, { "15", 15 }, { "16", 16 },
            { "17", 17 }, { "18", 18 }, { "19", 19 }, { "20", 20 },
            { "21", 21 }, { "22", 22 },
            { "Ending", 23 }
        };

        // 添加鼠标中键滚动组件
        if (gameObject.GetComponent<UIMouseScroll>() == null)
            gameObject.AddComponent<UIMouseScroll>();

        // 返回按钮
        Transform returnBtn = transform.Find("ReturnBtn");
        if (returnBtn != null)
            returnBtn.GetComponent<Button>().onClick.AddListener(OnReturnBtnClick);

        // 收集所有剧情节点信息
        CollectStoryNodes();

        // 应用初始解锁状态
        RefreshUnlockState();
    }

    /// <summary>
    /// 收集所有剧情节点的引用
    /// </summary>
    private void CollectStoryNodes()
    {
        storyNodes.Clear();

        // Preface 始终解锁 (requiredIslandIndex = -1)
        AddStoryNode("Preface", 0, -1);

        // 按钮 1~22：分别对应击败岛屿 0~21 的 Boss 后解锁
        // 按钮 "N" -> storyId=N -> 解锁条件：击败 Island_(N-1)
        for (int i = 1; i <= 22; i++)
        {
            AddStoryNode(i.ToString(), i, i - 1);
        }

        // Ending：击败岛屿 22 的 Boss 后解锁
        AddStoryNode("Ending", 23, 22);
    }

    private void AddStoryNode(string nodeName, int storyId, int requiredIslandIndex)
    {
        Transform nodeTrans = transform.Find($"Scroll View/Viewport/Content/{nodeName}");
        if (nodeTrans == null) return;

        Image nodeImage = nodeTrans.GetComponent<Image>();
        Transform btnTrans = nodeTrans.Find("Button");
        Button btn = btnTrans?.GetComponent<Button>();

        StoryNodeInfo info = new StoryNodeInfo
        {
            nodeName = nodeName,
            storyId = storyId,
            requiredIslandIndex = requiredIslandIndex,
            nodeImage = nodeImage,
            originalSprite = nodeImage != null ? nodeImage.sprite : null,
            originalColor = nodeImage != null ? nodeImage.color : Color.white,
            button = btn
        };

        if (btn != null)
        {
            int id = storyId;
            btn.onClick.AddListener(() => OnStoryButtonClick(id));
        }

        storyNodes.Add(info);
    }

    /// <summary>
    /// 刷新所有按钮的解锁状态（根据当前岛屿进度）
    /// </summary>
    public void RefreshUnlockState()
    {
        int maxUnlocked = RoleManager.Instance != null
            ? PlayerPrefs.GetInt("MaxUnlockedIsland", 0)
            : 0;

        foreach (var node in storyNodes)
        {
            bool isUnlocked = node.requiredIslandIndex < 0  // Preface 始终解锁
                           || node.requiredIslandIndex < maxUnlocked; // 击败该岛Boss后解锁

            ApplyNodeState(node, isUnlocked);
        }
    }

    /// <summary>
    /// 应用节点的锁定/解锁状态
    /// </summary>
    private void ApplyNodeState(StoryNodeInfo node, bool isUnlocked)
    {
        if (node.nodeImage != null)
        {
            if (isUnlocked)
            {
                // 恢复原始图片和颜色
                node.nodeImage.sprite = node.originalSprite;
                node.nodeImage.color = node.originalColor;
            }
            else
            {
                // 替换为锁定图片，变灰
                if (lockedSprite != null)
                    node.nodeImage.sprite = lockedSprite;
                node.nodeImage.color = lockedColor;
            }
        }

        if (node.button != null)
        {
            node.button.interactable = isUnlocked;
        }
    }

    public override void Show()
    {
        base.Show();
        RefreshUnlockState();
    }

    private void OnReturnBtnClick() => Close();

    private void OnStoryButtonClick(int storyId)
    {
        Hide();

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null) fightUI.Hide();

        StoryUI storyUI = (StoryUI)UIManager.Instance.ShowUI<StoryUI>("StoryUI");
        storyUI.SetStoryId(storyId);
    }
}