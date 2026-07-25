using Map;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // 引入 DOTween
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SlayTheSpireMapUI : UIBase
{
    public System.Action OnClosed; // 关闭回调

    private MapManager mapManager;
    private MapView mapView;
    private Button closeBtn;
    private CanvasGroup canvasGroup; // 用于控制淡出
    private int currentIslandIndex = -1;  // 记录当前岛屿
    private bool isFirstOpen = true;  // 标记是否是第一次打开该地图
    private List<UIBase> hiddenUIs = new List<UIBase>(); // 记录被隐藏的 UI
    private bool hideCloseBtn = false; // 是否隐藏关闭按钮（战斗胜利后）

    private void Awake()
    {
        // 获取或添加 CanvasGroup 组件（用于淡出）
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // 查找关闭按钮
        closeBtn = transform.Find("CloseBtn")?.GetComponent<Button>();
        if (closeBtn != null)
        {
            closeBtn.onClick.AddListener(OnCloseButtonClick);
        }
        else
        {
            Debug.LogWarning("SlayTheSpireMapUI: 未找到 CloseBtn 按钮");
        }

        TryGetMapComponents();
    }

    private void TryGetMapComponents()
    {
        if (mapManager == null)
            mapManager = GetComponentInChildren<MapManager>();
        if (mapView == null)
            mapView = GetComponentInChildren<MapView>();
    }

    private void OnCloseButtonClick()
    {
        CloseWithAnimation(); // 调用带动画的关闭
    }

    // 带动画的关闭
    private void CloseWithAnimation()
    {
        // 禁用按钮交互，避免动画期间重复点击
        if (closeBtn != null) closeBtn.interactable = false;

        // 恢复隐藏的UI
        RestoreHiddenUIs();

        // 播放淡出动画（同时可缩小）
        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(0f, 0.2f));      // 淡出
        seq.Join(transform.DOScale(0.9f, 0.2f));       // 轻微缩小
        seq.OnComplete(() =>
        {
            // 动画完成后，调用基类 Hide 隐藏对象，并触发回调
            base.Hide(); // 隐藏 GameObject，不销毁
            OnClosed?.Invoke();
        });
    }


    // 重置地图状态（用于重新开始游戏）
    public void ResetMapState()
    {
        isFirstOpen = true;
    }

    // 重写Show方法，确保UI正确显示
    public override void Show()
    {
        base.Show();
        transform.SetAsLastSibling();

        // 重置CanvasGroup的alpha值（因为关闭时将其设置为0）
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        // 重置缩放
        transform.localScale = Vector3.one;

        // 恢复关闭按钮交互
        if (closeBtn != null)
        {
            closeBtn.interactable = true;
        }

        // 如果需要隐藏关闭按钮
        if (hideCloseBtn && closeBtn != null)
        {
            closeBtn.gameObject.SetActive(false);
        }
    }

    // 设置是否隐藏关闭按钮
    public void SetHideCloseBtn(bool hide)
    {
        hideCloseBtn = hide;
        if (closeBtn != null)
        {
            closeBtn.gameObject.SetActive(!hide);
        }
    }

    public void SetIslandIndex(int index)
    {
        currentIslandIndex = index;  // 记录

        // 重置CanvasGroup的alpha值（因为关闭时将其设置为0）
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        // 重置缩放
        transform.localScale = Vector3.one;

        // 恢复关闭按钮交互和显示（正常打开时显示关闭按钮）
        if (closeBtn != null)
        {
            closeBtn.interactable = true;
            closeBtn.gameObject.SetActive(true);
            hideCloseBtn = false; // 重置为不隐藏
        }

        // 隐藏战斗UI（如果战斗进行中）
        HideFightUIs();

        // 获取 MapManager
        if (mapManager == null)
            mapManager = GetComponentInChildren<MapManager>();

        if (mapManager != null)
        {
            // 检查地图是否已经被访问过（有节点在path中）
            bool hasVisitedNodes = mapManager.CurrentMap != null && mapManager.CurrentMap.path.Count > 0;

            // 检查是否有保存的地图数据
            bool hasSavedMap = PlayerPrefs.HasKey("Map");

            if (hasVisitedNodes || hasSavedMap)
            {
                // 已经有节点被访问过或保存了地图数据，保持当前地图状态
                MapPlayerTracker.Instance.Locked = true; // 锁定地图，不允许点击
                isFirstOpen = false; // 确保标记为非首次打开
            }
            else if (isFirstOpen)
            {
                // 没有访问记录且是第一次打开，生成新地图
                // 清除之前保存的地图数据
                if (PlayerPrefs.HasKey("Map"))
                {
                    PlayerPrefs.DeleteKey("Map");
                    PlayerPrefs.Save();
                }

                // 生成新地图
                mapManager.GenerateNewMap();
                isFirstOpen = false;
            }
            else
            {
                // 不是第一次打开但没有访问记录（战斗中打开），保持地图状态
                MapPlayerTracker.Instance.Locked = true; // 锁定地图，不允许点击
            }
        }
        else
        {
            Debug.LogWarning("SlayTheSpireMapUI: 未找到 MapManager 组件");
        }
    }

    // 节点被点击时触发战斗
    public void OnNodeClicked(MapNode node)
    {
        if (node == null) return;

        if (mapManager != null && mapManager.CurrentMap != null &&
            mapManager.CurrentMap.path.Contains(node.Node.point))
        {
            return;
        }

        if (mapManager != null && mapManager.CurrentMap != null)
            mapManager.CurrentMap.path.Add(node.Node.point);

        node.SetState(NodeStates.Visited);

        if (mapView != null)
            mapView.SetAttainableNodes();

        FightManager.Instance.SetCurrentNodePoint(node.Node.point);
        FightManager.Instance.SetCurrentNodeType(node.Node.nodeType);

        if (currentIslandIndex >= 0 && IsBossNode(node))
        {
            int nextIsland = currentIslandIndex + 1;
            if (!RoleManager.Instance.IsIslandUnlocked(nextIsland))
            {
                RoleManager.Instance.UnlockNextIsland();
                UIManager.Instance.ShowTip($"解锁岛屿 {nextIsland + 1}!", new Color(0.2f, 0.8f, 0.2f));
            }
        }

        // RestSite 节点特殊处理
        if (node.Node.nodeType == NodeType.RestSite)
        {
            UIManager.Instance.ShowUI<RestSiteUI>("RestSiteUI");
            return;
        }

        // Store 节点特殊处理
        if (node.Node.nodeType == NodeType.Store)
        {
            Hide(); // 先隐藏地图
            ShopUI shopUI = UIManager.Instance.ShowUI<ShopUI>("ShopUI") as ShopUI;
            if (shopUI != null)
                shopUI.OnClosed += OnShopClosed;
            return;
        }

        // Treasure 节点特殊处理
        if (node.Node.nodeType == NodeType.Treasure)
        {
            Hide();
            TreasureUI treasureUI = UIManager.Instance.ShowUI<TreasureUI>("TreasureUI") as TreasureUI;
            if (treasureUI != null)
                treasureUI.OnClosed += () => { Show(); mapView?.SetAttainableNodes(); };
            return;
        }

        FightManager.Instance.SetCurrentIslandIndex(currentIslandIndex);
        FightManager.Instance.ChangeType(FightType.Init);

        // 保存地图状态
        if (mapManager != null)
        {
            mapManager.SaveMap();
        }
    }

    private void OnShopClosed()
    {
        // 商店关闭后重新显示地图
        Show();
        if (mapView != null)
            mapView.SetAttainableNodes();
    }

    private bool IsBossNode(MapNode node)
    {
        if (mapManager == null || mapManager.CurrentMap == null)
            return false;

        Map.Node bossNode = mapManager.CurrentMap.GetBossNode();
        if (bossNode == null)
            return false;

        return node.Node.point == bossNode.point;
    }

    // 如果外部直接调用 Close，也使用动画关闭（可选）
    public override void Close()
    {
        // 开始播放关闭动画（原有逻辑）
        StartCoroutine(CloseWithAnimationCoroutine());
    }

    private IEnumerator CloseWithAnimationCoroutine()
    {
        // 恢复隐藏的UI
        RestoreHiddenUIs();

        // 淡出动画（或你的其他动画）
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            float duration = 0.3f;
            float startAlpha = cg.alpha;
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(startAlpha, 0, elapsed / duration);
                yield return null;
            }
            cg.alpha = 0;
        }

        // 动画完成，执行隐藏而不是销毁
        base.Hide();           // 隐藏 GameObject，不销毁
        OnClosed?.Invoke();    // 触发回调（让 MapUI 重新显示）
    }

    private bool IsIslandCompleted()
    {
        if (mapManager == null || mapManager.CurrentMap == null)
            return false;

        Map.Map currentMap = mapManager.CurrentMap;  // 明确使用 Map.Map 类型

        // 获取Boss节点
        Map.Node bossNode = currentMap.GetBossNode();
        if (bossNode == null)
            return false;

        // 检查路径中是否包含Boss节点的位置
        return currentMap.path.Any(p => p.Equals(bossNode.point));
    }

    // 隐藏战斗相关的UI
    private void HideFightUIs()
    {
        hiddenUIs.Clear();

        // 隐藏FightUI
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null && fightUI.gameObject.activeSelf)
        {
            hiddenUIs.Add(fightUI);
            fightUI.Hide();
        }

        // 如果需要，可以添加其他战斗相关的UI
        // 例如：PlotUI, CardCollectionUI等
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