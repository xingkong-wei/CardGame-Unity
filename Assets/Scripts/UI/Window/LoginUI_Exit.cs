using System.Collections.Generic;
using Map;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 登录界面（带继续游戏按钮）
/// </summary>
public class LoginUI_Exit : UIBase
{
    [Header("按钮")]
    public Button startBtn;
    public Button continueBtn;
    public Button setBtn;
    public Button encyclpediaBtn;
    public Button quitBtn;

    private void Awake()
    {
        startBtn.onClick.AddListener(OnStartGame);
        continueBtn.onClick.AddListener(OnContinue);
        setBtn.onClick.AddListener(OnSetBtnClick);
        encyclpediaBtn.onClick.AddListener(OnEncyclpediaBtnClick);
        quitBtn.onClick.AddListener(OnExitGame);
    }

    private void Start()
    {
        continueBtn.gameObject.SetActive(SaveManager.HasSave());
    }

    // ===== 新游戏 =====

    private void OnStartGame()
    {
        SaveManager.ClearAllGameData();
        FightCardManager.Instance.ResetForNewGame();
        RoleManager.Instance.Init();
        RoleManager.Instance.ApplyUpgradesToDeck();
        FightUI.ResetBattleTimer();
        FightManager.ResetHp();
        FightManager.Instance.relicList.Clear();
        FightManager.Instance.potionList.Clear();

        SaveFileManager.DeleteKeysByPrefix("Map_Island_");
        SaveFileManager.DeleteKey("Map"); SaveFileManager.DeleteKey("UpgradedCardIds");
        SaveFileManager.DeleteKey("SavedCurHp"); SaveFileManager.DeleteKey("SavedMaxHp");
        SaveFileManager.DeleteKey("SavedCoinAmount"); SaveFileManager.DeleteKey("SavedIslandIndex");
        SaveFileManager.Flush();

        Close();
        MapUI mapUI = UIManager.Instance.ShowUI<MapUI>("MapUI") as MapUI;
        mapUI.OnNewGameStarted();
    }

    // ===== 继续游戏 =====

    private void OnContinue()
    {
        GameSaveData data = SaveManager.Load();
        if (data == null) return;

        Close();

        FightManager fm = FightManager.Instance;

        // 恢复基础数值（血量由 FightInit 重新初始化，这里只恢复持久化数据）
        fm.SetCoinAmount(data.coinAmount);
        fm.currentIslandIndex = data.currentIslandIndex;
        fm.currentNodePoint = new Vector2Int(data.currentNodeX, data.currentNodeY);
        System.Enum.TryParse(data.currentNodeTypeStr, out Map.NodeType nodeType);
        fm.currentNodeType = nodeType;

        // 恢复牌组
        RoleManager.Instance.cardList.Clear();
        foreach (var entry in data.deckCards)
        {
            CardData cd = FindCardDataById(entry.cardDataId);
            if (cd != null)
            {
                var dc = new DeckCard(cd) { instanceId = entry.instanceId, upgraded = entry.upgraded };
                RoleManager.Instance.cardList.Add(dc);
            }
        }

        // 恢复地图到 MapManager（防止 Start() 重新生成）
        if (!string.IsNullOrEmpty(data.mapJson))
        {
            string mapKey = $"Map_Island_{data.currentIslandIndex}";
            SaveFileManager.SetString(mapKey, data.mapJson);

            Map.Map map = JsonConvert.DeserializeObject<Map.Map>(data.mapJson);
            if (map != null)
            {
                MapManager mapManager = FindFirstObjectByType<MapManager>();
                if (mapManager != null) mapManager.CurrentMap = map;
            }
        }

        // 持久化数值（供 FightInit 等读取）
        SaveFileManager.SetInt("SavedCurHp", data.curHp);
        SaveFileManager.SetInt("SavedMaxHp", data.maxHp);
        SaveFileManager.SetInt("SavedCoinAmount", data.coinAmount);
        SaveFileManager.SetInt("SavedIslandIndex", data.currentIslandIndex);
        SaveFileManager.Flush();

        // 恢复敌人关卡ID
        EnemyManager.Instance.CurrentLevelId = data.levelId > 0 ? data.levelId : -1;

        // 根据存档阶段恢复
        switch (data.savePhase)
        {
            case SavePhase.Reward: RestoreReward(data); break;
            case SavePhase.Shop:   RestoreShop(data);   break;
            default:               RestoreFight(data);  break;
        }
    }

    private void RestoreFight(GameSaveData data)
    {
        SaveManager.IsLoading = true;
        FightManager.Instance.ChangeType(FightType.Init);
        SaveManager.IsLoading = false;
        RestoreRelicsAndPotions(data);
    }

    private void RestoreReward(GameSaveData data)
    {
        RestoreRelicsAndPotions(data);

        // 先创建 FightUI 作为 Inventory 宿主（不触发战斗流程）
        FightUI fightUI = UIManager.Instance.ShowUI<FightUI>("FightUI") as FightUI;
        if (fightUI != null)
        {
            fightUI.UpdateDefense();
            fightUI.UpdateUsedCardCount();
            fightUI.UpdateConsumeCardCount();
        }

        // 打开奖励界面
        SelectCardUI selectCard = UIManager.Instance.ShowUI<SelectCardUI>("SelectCardUI") as SelectCardUI;
        if (selectCard == null) return;

        selectCard.RestoreFromSave(data);

        SelectCardUI.OnClosed += () =>
        {
            SaveManager.Save(SavePhase.Fight);
            UIManager.Instance.HideUI("FightUI");
            ReopenNodeMap();
        };
    }

    private void RestoreShop(GameSaveData data)
    {
        RestoreRelicsAndPotions(data);

        // 手动创建 ShopUI，确保 MarkRestoring 在 Show() 之前生效
        ShopUI shopUI = CreateUIManually<ShopUI>("UI/ShopUI", "ShopUI");
        if (shopUI == null) return;

        shopUI.MarkRestoring();
        shopUI.Show();
        shopUI.RestoreFromSave(data);

        shopUI.OnClosed += () =>
        {
            SaveManager.Save(SavePhase.Fight);
            ReopenNodeMap();
        };
    }

    // ===== 工具方法 =====

    /// <summary>手动创建 UI 并注册到 UIManager（绕过 ShowUI 的自动 Show）</summary>
    private T CreateUIManually<T>(string prefabPath, string objName) where T : UIBase
    {
        GameObject prefab = ResourceCache.Get<GameObject>(prefabPath);
        if (prefab == null) { Debug.LogError($"{prefabPath} prefab 未找到"); return null; }

        Transform parent = UIManager.Instance?.canvasTf;
        GameObject go = Object.Instantiate(prefab, parent);
        go.name = objName;

        T ui = go.GetComponent<T>() ?? go.AddComponent<T>();

        // 注册到 _uiDict 和 uiList，确保 CloseUI 能找到
        var dictField = typeof(UIManager).GetField("_uiDict",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (dictField != null)
        {
            var dict = dictField.GetValue(UIManager.Instance) as System.Collections.Generic.Dictionary<string, UIBase>;
            if (dict != null) dict[objName] = ui;
        }

        var uiListField = typeof(UIManager).GetField("uiList",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (uiListField != null)
        {
            var list = uiListField.GetValue(UIManager.Instance) as List<UIBase>;
            list?.Add(ui);
        }
        return ui;
    }

    /// <summary>恢复遗物和药水列表</summary>
    private void RestoreRelicsAndPotions(GameSaveData data)
    {
        FightManager fm = FightManager.Instance;
        RelicManager.Instance.Clear();
        fm.relicList.Clear();
        foreach (var rid in data.relicIds)
        {
            var allRelics = Resources.LoadAll<RelicData>("Data_Relic");
            var found = System.Array.Find(allRelics, r => r.scriptName == rid);
            if (found != null) fm.AddRelic(found);
        }

        fm.potionList.Clear();
        foreach (var pid in data.potionIds)
        {
            var allPotions = Resources.LoadAll<PotionData>("Data_Potion");
            var found = System.Array.Find(allPotions, p => p.scriptName == pid);
            if (found != null) fm.potionList.Add(found);
        }
    }

    /// <summary>重新打开节点地图</summary>
    private void ReopenNodeMap()
    {
        SlayTheSpireMapUI nodeMapUI = UIManager.Instance.GetUI<SlayTheSpireMapUI>("SlayTheSpireMapUI");
        if (nodeMapUI == null)
            nodeMapUI = UIManager.Instance.ShowUI<SlayTheSpireMapUI>("SlayTheSpireMapUI") as SlayTheSpireMapUI;
        if (nodeMapUI != null) nodeMapUI.ReopenAfterVictory();
    }

    private static CardData FindCardDataById(int id)
    {
        var all = Resources.LoadAll<CardData>("Data_Card/Card");
        return System.Array.Find(all, c => c.id == id);
    }

    // ===== 按钮回调 =====

    private void OnExitGame()
    {
        Close();
        ExitManager.Instance.OnExitGameClicked();
    }

    private void OnSetBtnClick()
    {
        GameSettingUI settingUI = UIManager.Instance.ShowUI<GameSettingUI>("GameSettingUI") as GameSettingUI;
        if (settingUI != null) settingUI.SetPreviousUIName("LoginUI_Exit");
        Hide();
    }

    private void OnEncyclpediaBtnClick()
    {
        UIManager.Instance.ShowUI<EncyclpediaUI>("EncyclpediaUI");
    }
}
