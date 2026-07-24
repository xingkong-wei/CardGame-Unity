using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapUI : UIBase
{
    public Button returnBtn;   // 返回按钮
    private List<UIBase> hiddenUIs = new List<UIBase>(); // 记录被隐藏的 UI
    private bool justStartedFromLogin = false; // 标记是否刚从LoginUI开始
    private bool hasStartedGame = false; // 标记游戏是否已经开始（用于区分首次进入和战斗中打开）

    private void Awake()
    {
        // 原有返回按钮逻辑
        if (returnBtn == null)
            returnBtn = transform.Find("ReturnBtn")?.GetComponent<Button>();
        if (returnBtn != null)
            returnBtn.onClick.AddListener(OnReturnClick);

        // 绑定所有岛屿按钮
        BindIslandButtons();
    }

    private void BindIslandButtons()
    {
        Transform content = transform.Find("Scroll View/Viewport/Content");
        if (content == null)
        {
            Debug.LogError("MapUI: 未找到 Scroll View/Viewport/Content");
            return;
        }

        for (int i = 0; i < content.childCount; i++)
        {
            Transform island = content.GetChild(i);
            if (island.name.StartsWith("Island_"))
            {
                Button btn = island.Find("Button")?.GetComponent<Button>();
                if (btn != null)
                {
                    int index = i;
                    bool unlocked = RoleManager.Instance.IsIslandUnlocked(index);

                    // 设置按钮状态
                    btn.interactable = unlocked;
                    // 设置灰色样式（可选）
                    ColorBlock colors = btn.colors;
                    colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    btn.colors = colors;

                    if (unlocked)
                    {
                        // 清除旧监听，避免重复添加
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => OnIslandButtonClick(index));
                    }
                    // 如果未解锁，不添加点击监听
                }
            }
        }
    }

    private void OnIslandButtonClick(int islandIndex)
    {
        // 1. 隐藏当前 MapUI（但不销毁）
        Hide();

        // 2. 记录当前选择的岛屿索引
        FightManager.Instance.SetCurrentIslandIndex(islandIndex);

        // 3. 显示节点地图 (SlayTheSpireMapUI)
        ShowNodeMap(islandIndex);
    }

    private void ShowNodeMap(int islandIndex)
    {
        // 检查是否已存在 SlayTheSpireMapUI
        SlayTheSpireMapUI nodeMapUI = UIManager.Instance.GetUI<SlayTheSpireMapUI>("SlayTheSpireMapUI");

        if (nodeMapUI == null)
        {
            // 不存在，则创建新的
            nodeMapUI = UIManager.Instance.ShowUI<SlayTheSpireMapUI>("SlayTheSpireMapUI") as SlayTheSpireMapUI;
        }
        else
        {
            // 已存在，直接显示（不重置状态）
            // 是否生成新地图由 SlayTheSpireMapUI 内部的 hasVisitedNodes 检查决定
            nodeMapUI.Show();
        }

        // 设置节点地图关闭时的回调：关闭后返回 MapUI
        nodeMapUI.OnClosed = () =>
        {
            Show(); // 重新显示 MapUI
        };

        // 传递岛屿索引给节点地图
        nodeMapUI.SetIslandIndex(islandIndex);
    }

    private void OnReturnClick()
    {
        // 恢复隐藏的UI
        RestoreHiddenUIs();
        Close(); // 关闭 MapUI
    }

    // 从LoginUI进入时调用,重置所有游戏状态
    public void OnNewGameStarted()
    {
        // 重置SlayTheSpireMapUI的状态
        SlayTheSpireMapUI nodeMapUI = UIManager.Instance.GetUI<SlayTheSpireMapUI>("SlayTheSpireMapUI");
        if (nodeMapUI != null)
        {
            nodeMapUI.ResetMapState();
        }

        // 隐藏返回按钮（从LoginUI进入时）
        if (returnBtn != null)
        {
            returnBtn.gameObject.SetActive(false);
        }

        // 标记为刚从LoginUI开始和游戏已开始
        justStartedFromLogin = true;
        hasStartedGame = true;
    }

    public override void Show()
    {
        base.Show();
        transform.SetAsLastSibling();

        // 检查FightUI是否存在，如果存在则隐藏（战斗过程中）
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        bool isInBattle = fightUI != null && fightUI.gameObject.activeInHierarchy;

        // 如果刚从LoginUI开始,重置SlayTheSpireMapUI
        if (justStartedFromLogin)
        {
            SlayTheSpireMapUI nodeMapUI = UIManager.Instance.GetUI<SlayTheSpireMapUI>("SlayTheSpireMapUI");
            if (nodeMapUI != null)
            {
                nodeMapUI.ResetMapState();
            }
            justStartedFromLogin = false;
            hasStartedGame = true; // 标记游戏已经开始

            // 隐藏返回按钮（从LoginUI进入时）
            if (returnBtn != null)
            {
                returnBtn.gameObject.SetActive(false);
            }
        }

        // 如果在战斗中，隐藏战斗相关的UI（这个判断要独立执行，不管是否刚从LoginUI开始）
        if (hasStartedGame && isInBattle)
        {
            // 游戏已开始且在战斗中，隐藏战斗相关的UI
            HideFightUIs();

            // 显示返回按钮（战斗过程中进入时）
            if (returnBtn != null)
            {
                returnBtn.gameObject.SetActive(true);
            }
        }
    }

    // 隐藏战斗相关的UI
    private void HideFightUIs()
    {
        hiddenUIs.Clear();

        // 隐藏FightUI
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");

        if (fightUI != null)
        {
            // 检查FightUI是否真的存在并处于活动状态
            if (fightUI.gameObject.activeInHierarchy)
            {
                hiddenUIs.Add(fightUI);
                fightUI.Hide();
            }
        }
    }

    // 恢复隐藏的UI
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
