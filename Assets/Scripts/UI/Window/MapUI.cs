using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapUI : UIBase
{
    [Header("锁定状态图片")]
    public Sprite lockedSprite;          // 未解锁时显示的图片（Main.png）
    public Color lockedColor = new Color(0.451f, 0.451f, 0.451f); // 灰色 #737373
    public string lockedNameText = "未知"; // 未解锁时显示的名称

    /// <summary>
    /// 岛屿节点信息
    /// </summary>
    private class IslandNodeInfo
    {
        public int islandIndex;           // 岛屿索引
        public Image nodeImage;           // 岛屿的 Image 组件
        public Sprite originalSprite;     // 原始图片
        public Color originalColor;       // 原始颜色
        public Button button;             // 按钮组件
        public TextMeshProUGUI nameText;  // 名称文本组件
        public string originalNameText;   // 原始名称文本
    }

    public Button returnBtn;   // 返回按钮
    private List<UIBase> hiddenUIs = new List<UIBase>(); // 记录被隐藏的 UI
    private bool justStartedFromLogin = false; // 标记是否刚从LoginUI开始
    private bool hasStartedGame = false; // 标记游戏是否已经开始
    private List<IslandNodeInfo> islandNodes = new List<IslandNodeInfo>();

    private void Awake()
    {
        if (returnBtn == null)
            returnBtn = transform.Find("ReturnBtn")?.GetComponent<Button>();
        if (returnBtn != null)
            returnBtn.onClick.AddListener(OnReturnClick);

        // 收集所有岛屿节点
        CollectIslandNodes();
        // 应用初始解锁状态
        RefreshUnlockState();
    }

    /// <summary>
    /// 收集所有岛屿节点的引用
    /// </summary>
    private void CollectIslandNodes()
    {
        islandNodes.Clear();
        Transform content = transform.Find("Scroll View/Viewport/Content");
        if (content == null) return;

        for (int i = 0; i < content.childCount; i++)
        {
            Transform island = content.GetChild(i);
            if (island.name.StartsWith("Island_"))
            {
                // Image 组件在子节点 "Image" 上，不在根节点
                Transform imageTrans = island.Find("Image");
                Image nodeImage = imageTrans?.GetComponent<Image>();
                Button btn = island.Find("Button")?.GetComponent<Button>();
                // Name 文本在子节点 "Name" 上
                Transform nameTrans = island.Find("Name");
                TextMeshProUGUI nameText = nameTrans?.GetComponent<TextMeshProUGUI>();

                IslandNodeInfo info = new IslandNodeInfo
                {
                    islandIndex = i,
                    nodeImage = nodeImage,
                    originalSprite = nodeImage != null ? nodeImage.sprite : null,
                    originalColor = nodeImage != null ? nodeImage.color : Color.white,
                    button = btn,
                    nameText = nameText,
                    originalNameText = nameText != null ? nameText.text : ""
                };

                if (btn != null)
                {
                    int index = i;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnIslandButtonClick(index));
                }

                islandNodes.Add(info);
            }
        }
    }

    /// <summary>
    /// 刷新所有岛屿按钮的解锁状态
    /// </summary>
    public void RefreshUnlockState()
    {
        foreach (var node in islandNodes)
        {
            bool isUnlocked = RoleManager.Instance.IsIslandUnlocked(node.islandIndex);
            bool isCompleted = RoleManager.Instance.IsIslandCompleted(node.islandIndex);
            ApplyNodeState(node, isUnlocked, isCompleted);
        }
    }

    /// <summary>
    /// 应用岛屿的锁定/解锁状态
    /// </summary>
    private void ApplyNodeState(IslandNodeInfo node, bool isUnlocked, bool isCompleted)
    {
        if (node.nodeImage != null)
        {
            if (isUnlocked)
            {
                node.nodeImage.sprite = node.originalSprite;
                node.nodeImage.color = node.originalColor;
            }
            else
            {
                if (lockedSprite != null)
                    node.nodeImage.sprite = lockedSprite;
                node.nodeImage.color = lockedColor;
            }
        }

        if (node.button != null)
        {
            // 已通关的岛屿不可点击（保留图片但禁用交互）
            node.button.interactable = isUnlocked && !isCompleted;
        }

        // 名称文本：未解锁显示"未知"，解锁后恢复原始名称
        if (node.nameText != null)
        {
            node.nameText.text = isUnlocked ? node.originalNameText : lockedNameText;
        }
    }

    private void OnIslandButtonClick(int islandIndex)
    {
        // 已通关的岛屿不可进入
        if (RoleManager.Instance.IsIslandCompleted(islandIndex))
        {
            UIManager.Instance.ShowTip("已通关该地图", new Color(1f, 0.6f, 0.2f));
            return;
        }

        Hide();

        FightManager.Instance.SetCurrentIslandIndex(islandIndex);
        ShowNodeMap(islandIndex);
    }

    private void ShowNodeMap(int islandIndex)
    {
        SlayTheSpireMapUI nodeMapUI = UIManager.Instance.GetUI<SlayTheSpireMapUI>("SlayTheSpireMapUI");

        if (nodeMapUI == null)
        {
            nodeMapUI = UIManager.Instance.ShowUI<SlayTheSpireMapUI>("SlayTheSpireMapUI") as SlayTheSpireMapUI;
        }
        else
        {
            nodeMapUI.Show();
        }

        nodeMapUI.EnterNewIsland(islandIndex);
    }

    private void OnReturnClick()
    {
        RestoreHiddenUIs();
        Close();
    }

    public void OnNewGameStarted()
    {
        SlayTheSpireMapUI nodeMapUI = UIManager.Instance.GetUI<SlayTheSpireMapUI>("SlayTheSpireMapUI");
        if (nodeMapUI != null)
        {
            nodeMapUI.ResetMapState();
        }

        if (returnBtn != null)
        {
            returnBtn.gameObject.SetActive(false);
        }

        justStartedFromLogin = true;
        hasStartedGame = true;
    }

    public override void Show()
    {
        base.Show();
        transform.SetAsLastSibling();

        // 每次显示时刷新岛屿解锁状态
        RefreshUnlockState();

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        bool isInBattle = fightUI != null && fightUI.gameObject.activeInHierarchy;

        if (justStartedFromLogin)
        {
            SlayTheSpireMapUI nodeMapUI = UIManager.Instance.GetUI<SlayTheSpireMapUI>("SlayTheSpireMapUI");
            if (nodeMapUI != null)
            {
                nodeMapUI.ResetMapState();
            }
            justStartedFromLogin = false;
            hasStartedGame = true;

            if (returnBtn != null)
            {
                returnBtn.gameObject.SetActive(false);
            }
        }

        if (hasStartedGame && isInBattle)
        {
            HideFightUIs();

            if (returnBtn != null)
            {
                returnBtn.gameObject.SetActive(true);
            }
        }
    }

    private void HideFightUIs()
    {
        hiddenUIs.Clear();

        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null && fightUI.gameObject.activeInHierarchy)
        {
            hiddenUIs.Add(fightUI);
            fightUI.Hide();
        }
    }

    private void RestoreHiddenUIs()
    {
        foreach (UIBase ui in hiddenUIs)
        {
            if (ui != null)
            {
                ui.Show();
            }
        }
        hiddenUIs.Clear();
    }
}
