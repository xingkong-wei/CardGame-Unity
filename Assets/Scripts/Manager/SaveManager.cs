using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 存档管理器 - 节点入口快照，SL 读档回到节点开始
/// </summary>
public class SaveManager : MonoBehaviour
{
    private const string SAVE_KEY = "GameSave";

    /// <summary>正在读档恢复中，此时不覆盖存档</summary>
    public static bool IsLoading { get; set; }

    private static SaveManager _instance;
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SaveManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SaveManager");
                    _instance = go.AddComponent<SaveManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null) { _instance = this; DontDestroyOnLoad(gameObject); }
        else if (_instance != this) Destroy(gameObject);
    }

    public static bool HasSave() => PlayerPrefs.HasKey(SAVE_KEY);

    /// <summary>保存（支持不同游戏阶段）</summary>
    public static void Save(SavePhase phase = SavePhase.Fight)
    {
        GameSaveData data = new GameSaveData();
        data.savePhase = phase;

        FightManager fm = FightManager.Instance;
        if (fm != null)
        {
            data.curHp = fm.CurHp;
            data.maxHp = fm.MaxHp;
            data.coinAmount = fm.CoinAmount;
            data.currentIslandIndex = fm.currentIslandIndex;
            data.currentNodeX = fm.currentNodePoint.x;
            data.currentNodeY = fm.currentNodePoint.y;
            data.currentNodeTypeStr = fm.currentNodeType.ToString();

            if (fm.potionList != null)
                foreach (var p in fm.potionList)
                    if (p != null) data.potionIds.Add(p.scriptName);

            if (fm.relicList != null)
                foreach (var r in fm.relicList)
                    if (r != null) data.relicIds.Add(r.scriptName);
        }

        if (RoleManager.Instance?.cardList != null)
        {
            foreach (var dc in RoleManager.Instance.cardList)
            {
                if (dc?.cardData == null) continue;
                data.deckCards.Add(new CardSaveEntry
                {
                    cardDataId = dc.cardData.id,
                    instanceId = dc.instanceId,
                    upgraded = dc.upgraded
                });
            }
        }

        string mapKey = $"Map_Island_{data.currentIslandIndex}";
        if (PlayerPrefs.HasKey(mapKey))
            data.mapJson = PlayerPrefs.GetString(mapKey);

        // 保存当前敌人关卡ID（确保 SL 后同一个节点遇到同一组敌人）
        data.levelId = EnemyManager.Instance.CurrentLevelId;

        // 奖励界面数据：从 SelectCardUI 获取
        if (phase == SavePhase.Reward)
        {
            SelectCardUI selectCard = UIManager.Instance?.GetUI<SelectCardUI>("SelectCardUI") as SelectCardUI;
            if (selectCard != null)
                selectCard.WriteSaveData(data);
        }
        else if (phase == SavePhase.Shop)
        {
            ShopUI shopUI = UIManager.Instance?.GetUI<ShopUI>("ShopUI") as ShopUI;
            if (shopUI != null)
                shopUI.WriteSaveData(data);
        }

        data.saveTimeTicks = DateTime.Now.Ticks;

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    /// <summary>加载存档</summary>
    public static GameSaveData Load()
    {
        if (!HasSave()) return null;
        return JsonUtility.FromJson<GameSaveData>(PlayerPrefs.GetString(SAVE_KEY));
    }

    /// <summary>删除存档</summary>
    public static void DeleteSave()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY)) { PlayerPrefs.DeleteKey(SAVE_KEY); PlayerPrefs.Save(); }
    }

    /// <summary>清除所有游戏数据</summary>
    public static void ClearAllGameData()
    {
        DeleteSave();
        PlayerPrefs.DeleteKey("SavedCurHp"); PlayerPrefs.DeleteKey("SavedMaxHp");
        PlayerPrefs.DeleteKey("SavedCoinAmount"); PlayerPrefs.DeleteKey("SavedIslandIndex");
        PlayerPrefs.DeleteKey("Map"); PlayerPrefs.DeleteKey("UpgradedCardIds");
        PlayerPrefs.DeleteKey("CompletedLevels");
        for (int i = 0; i < 20; i++) { string k = $"Map_Island_{i}"; if (PlayerPrefs.HasKey(k)) PlayerPrefs.DeleteKey(k); }
        PlayerPrefs.Save();
    }
}
