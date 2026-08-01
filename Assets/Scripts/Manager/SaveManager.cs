using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 存档管理器 - 节点入口快照，SL 读档回到节点开始
/// 使用二进制文件流替代 PlayerPrefs
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
                GameObject go = new GameObject("SaveManager");
                _instance = go.AddComponent<SaveManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null) { _instance = this; DontDestroyOnLoad(gameObject); }
        else if (_instance != this) Destroy(gameObject);
    }

    public static bool HasSave() => SaveFileManager.HasKey(SAVE_KEY);

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

            // 同步持久化数据到二进制文件，确保 SL 读档时血量一致
            SaveFileManager.SetInt("SavedCurHp", fm.CurHp);
            SaveFileManager.SetInt("SavedMaxHp", fm.MaxHp);
            SaveFileManager.SetInt("SavedCoinAmount", fm.CoinAmount);
            SaveFileManager.SetInt("SavedIslandIndex", fm.currentIslandIndex);

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
        if (SaveFileManager.HasKey(mapKey))
            data.mapJson = SaveFileManager.GetString(mapKey);

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
        SaveFileManager.SetString(SAVE_KEY, json);
        SaveFileManager.Flush();
    }

    /// <summary>加载存档</summary>
    public static GameSaveData Load()
    {
        if (!HasSave()) return null;
        return JsonUtility.FromJson<GameSaveData>(SaveFileManager.GetString(SAVE_KEY));
    }

    /// <summary>删除存档</summary>
    public static void DeleteSave()
    {
        if (SaveFileManager.HasKey(SAVE_KEY))
            SaveFileManager.DeleteKey(SAVE_KEY);
        SaveFileManager.Flush();
    }

    /// <summary>清除所有游戏数据</summary>
    public static void ClearAllGameData()
    {
        DeleteSave();
        SaveFileManager.DeleteKey("SavedCurHp");
        SaveFileManager.DeleteKey("SavedMaxHp");
        SaveFileManager.DeleteKey("SavedCoinAmount");
        SaveFileManager.DeleteKey("SavedIslandIndex");
        SaveFileManager.DeleteKey("Map");
        SaveFileManager.DeleteKey("UpgradedCardIds");
        SaveFileManager.DeleteKey("CompletedLevels");
        SaveFileManager.DeleteKeysByPrefix("Map_Island_");
        SaveFileManager.Flush();
    }
}
