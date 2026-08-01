using Map;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SlayTheSpireMapUI : UIBase
{
    public System.Action OnClosed;

    private MapManager mapManager;
    private MapView mapView;
    private Button closeBtn;
    private CanvasGroup canvasGroup;
    private int currentIslandIndex = -1;

    // 观察模式下被隐藏的 UI 列表
    private List<UIBase> hiddenUIs = new List<UIBase>();
    // 记录被隐藏 UI 的原始 GameObject 引用（用于 SetActive 恢复，避免触发 Show 逻辑）
    private List<GameObject> hiddenObjects = new List<GameObject>();

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        closeBtn = transform.Find("CloseBtn")?.GetComponent<Button>();
        if (closeBtn != null)
            closeBtn.onClick.AddListener(OnCloseButtonClick);

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
        CloseWithAnimation();
    }

    private void CloseWithAnimation()
    {
        if (closeBtn != null) closeBtn.interactable = false;

        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(0f, 0.2f));
        seq.Join(transform.DOScale(0.9f, 0.2f));
        seq.OnComplete(() =>
        {
            if (this == null || gameObject == null) return;
            base.Hide();
            RestorePreviousUI();
            OnClosed?.Invoke();
        });
    }

    public void ResetMapState()
    {
        SaveFileManager.DeleteKeysByPrefix("Map_Island_");
        if (SaveFileManager.HasKey("Map"))
            SaveFileManager.DeleteKey("Map");
        SaveFileManager.Flush();
    }

    private string GetMapSaveKey(int islandIndex) => $"Map_Island_{islandIndex}";

    // ===== 场景1：从 MapUI 选择岛屿进入 =====

    public void EnterNewIsland(int islandIndex)
    {
        currentIslandIndex = islandIndex;

        OnClosed = () =>
        {
            MapUI mapUI = UIManager.Instance.GetUI<MapUI>("MapUI");
            if (mapUI != null)
            {
                mapUI.Show();
                mapUI.RefreshUnlockState();
            }
        };

        // 清除旧数据，生成新地图
        string key = GetMapSaveKey(islandIndex);
        if (SaveFileManager.HasKey(key))
            SaveFileManager.DeleteKey(key);
        SaveFileManager.Flush();

        ResetDisplay();
        ShowCloseBtn();

        TryGetMapComponents();
        if (mapManager == null) return;

        mapManager.CurrentMap = null;
        mapManager.GenerateNewMap();
        SaveCurrentMap();

        if (MapPlayerTracker.Instance != null)
            MapPlayerTracker.Instance.Locked = false;
    }

    // ===== 场景2~4：事件中按 MapBtn 观察地图 =====

    public void OpenForObservation(int islandIndex)
    {
        currentIslandIndex = islandIndex;

        // 用 SetActive(false) 隐藏事件UI（避免触发它们的 Show 逻辑）
        hiddenObjects.Clear();
        HideObject<FightUI>("FightUI");
        HideObject<ShopUI>("ShopUI");
        HideObject<TreasureUI>("TreasureUI");
        HideObject<RestSiteUI>("RestSiteUI");
        HideObject<SelectCardUI>("SelectCardUI");

        OnClosed = null;

        ResetDisplay();
        ShowCloseBtn();

        TryGetMapComponents();
        if (mapManager == null) return;

        // 优先用内存中的 CurrentMap
        if (mapManager.CurrentMap != null)
        {
            mapView.ShowMap(mapManager.CurrentMap);
        }
        else
        {
            string key = GetMapSaveKey(islandIndex);
            if (SaveFileManager.HasKey(key))
            {
                string mapJson = SaveFileManager.GetString(key);
                Map.Map map = Newtonsoft.Json.JsonConvert.DeserializeObject<Map.Map>(mapJson);
                if (map != null)
                {
                    mapManager.CurrentMap = map;
                    mapView.ShowMap(map);
                }
            }
        }

        // 锁定节点
        if (MapPlayerTracker.Instance != null)
            MapPlayerTracker.Instance.Locked = true;
    }

    private void HideObject<T>(string name) where T : UIBase
    {
        T ui = UIManager.Instance.GetUI<T>(name);
        if (ui != null && ui.gameObject.activeSelf)
        {
            ui.gameObject.SetActive(false);
            hiddenObjects.Add(ui.gameObject);
        }
    }

    private void RestorePreviousUI()
    {
        // 用 SetActive(true) 恢复（不触发 Show 逻辑，避免商店刷新等）
        foreach (GameObject obj in hiddenObjects)
        {
            if (obj != null)
                obj.SetActive(true);
        }
        hiddenObjects.Clear();
    }

    // ===== 场景5：非Boss胜利后，节点地图重新显示 =====

    public void ReopenAfterVictory()
    {
        // 从 FightManager 恢复岛屿索引（继续游戏后地图 UI 是新创建的，currentIslandIndex 为 -1）
        if (currentIslandIndex < 0)
            currentIslandIndex = FightManager.Instance.currentIslandIndex;

        // 重置显示状态
        ResetDisplay();
        HideCloseBtn();

        // 刷新地图视图（确保节点正确显示）
        TryGetMapComponents();
        if (mapManager != null && mapManager.CurrentMap != null && mapView != null)
        {
            mapView.ShowMap(mapManager.CurrentMap);
        }

        if (MapPlayerTracker.Instance != null)
            MapPlayerTracker.Instance.Locked = false;
    }

    // ===== 辅助方法 =====

    private void ResetDisplay()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
        transform.localScale = Vector3.one;
        // 确保 GameObject 是激活状态
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    private void ShowCloseBtn()
    {
        if (closeBtn != null)
        {
            closeBtn.interactable = true;
            closeBtn.gameObject.SetActive(true);
        }
    }

    private void HideCloseBtn()
    {
        if (closeBtn != null)
        {
            closeBtn.gameObject.SetActive(false);
        }
    }

    private void SaveCurrentMap()
    {
        if (mapManager == null || mapManager.CurrentMap == null) return;
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(mapManager.CurrentMap, Newtonsoft.Json.Formatting.Indented,
            new Newtonsoft.Json.JsonSerializerSettings { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore });
        SaveFileManager.SetString(GetMapSaveKey(currentIslandIndex), json);
        SaveFileManager.Flush();
    }

    // ===== 节点点击 =====

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

        // 任何节点点击后立即保存地图（确保 path 不丢失）
        SaveCurrentMap();

        if (currentIslandIndex >= 0 && IsBossNode(node))
        {
            RoleManager.Instance.MarkIslandCompleted(currentIslandIndex);

            int nextIsland = currentIslandIndex + 1;
            if (!RoleManager.Instance.IsIslandUnlocked(nextIsland))
            {
                RoleManager.Instance.UnlockNextIsland();
                UIManager.Instance.ShowTip($"解锁岛屿 {nextIsland + 1}!", new Color(0.2f, 0.8f, 0.2f));
            }
        }

        if (node.Node.nodeType == NodeType.RestSite)
        {
            UIManager.Instance.ShowUI<RestSiteUI>("RestSiteUI");
            return;
        }

        if (node.Node.nodeType == NodeType.Store)
        {
            Hide();
            ShopUI shopUI = UIManager.Instance.ShowUI<ShopUI>("ShopUI") as ShopUI;
            if (shopUI != null)
                shopUI.OnClosed += OnShopClosed;
            return;
        }

        if (node.Node.nodeType == NodeType.Treasure)
        {
            Hide();
            TreasureUI treasureUI = UIManager.Instance.ShowUI<TreasureUI>("TreasureUI") as TreasureUI;
            if (treasureUI != null)
                treasureUI.OnClosed += () =>
                {
                    ResetDisplay();
                    TryGetMapComponents();
                    if (mapManager != null && mapManager.CurrentMap != null && mapView != null)
                        mapView.ShowMap(mapManager.CurrentMap);
                };
            return;
        }

        if (node.Node.nodeType == NodeType.Mystery)
        {
            HandleMysteryNode();
            return;
        }

        FightManager.Instance.SetCurrentIslandIndex(currentIslandIndex);
        // 保存入场血量（SL 读档时恢复到进入节点时的状态）
        SaveFileManager.SetInt("NodeEntryCurHp", FightManager.Instance.CurHp);
        SaveFileManager.SetInt("NodeEntryMaxHp", FightManager.Instance.MaxHp);
        SaveFileManager.Flush();
        FightManager.Instance.ChangeType(FightType.Init);
    }

    private void HandleMysteryNode()
    {
        MysteryResult result = MysteryNodeResolver.Instance.RollResult();
        MysteryNodeResolver.Instance.RecordResult(result);

        switch (result)
        {
            case MysteryResult.Monster:
                FightManager.Instance.SetCurrentIslandIndex(currentIslandIndex);
                FightManager.Instance.ChangeType(FightType.Init);
                break;

            case MysteryResult.Shop:
                Hide();
                ShopUI shopUI = UIManager.Instance.ShowUI<ShopUI>("ShopUI") as ShopUI;
                if (shopUI != null)
                    shopUI.OnClosed += OnShopClosed;
                break;

            case MysteryResult.Treasure:
                Hide();
                TreasureUI treasureUI = UIManager.Instance.ShowUI<TreasureUI>("TreasureUI") as TreasureUI;
                if (treasureUI != null)
                    treasureUI.OnClosed += () =>
                    {
                        ResetDisplay();
                        TryGetMapComponents();
                        if (mapManager != null && mapManager.CurrentMap != null && mapView != null)
                            mapView.ShowMap(mapManager.CurrentMap);
                    };
                break;
        }
    }

    private void OnShopClosed()
    {
        ResetDisplay();
        TryGetMapComponents();
        if (mapManager != null && mapManager.CurrentMap != null && mapView != null)
            mapView.ShowMap(mapManager.CurrentMap);
        if (MapPlayerTracker.Instance != null)
            MapPlayerTracker.Instance.Locked = false;
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

    public override void Close()
    {
        StartCoroutine(CloseWithAnimationCoroutine());
    }

    private IEnumerator CloseWithAnimationCoroutine()
    {
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

        if (this == null || gameObject == null) yield break;
        base.Hide();
        RestorePreviousUI();
        OnClosed?.Invoke();
    }
}
