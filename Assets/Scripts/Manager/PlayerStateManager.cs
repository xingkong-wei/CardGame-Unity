using UnityEngine;

/// <summary>
/// 玩家状态管理器
/// 负责：血量、护盾、能量、金币 的存取和二进制文件持久化
/// </summary>
public class PlayerStateManager
{
    // 持久化 Key 常量
    private const string KEY_CUR_HP = "SavedCurHp";
    private const string KEY_MAX_HP = "SavedMaxHp";
    private const string KEY_COIN = "SavedCoinAmount";
    private const string KEY_ISLAND = "SavedIslandIndex";
    private const string KEY_ENTRY_HP = "NodeEntryCurHp";

    public int MaxHp;
    public int CurHp;
    public int MaxPowerCount;
    public int CurPowerCount;
    public int DefenseCount;

    public int CoinAmount { get; private set; }
    public int CardsPlayedThisTurn;

    private int _savedMaxHp = 100;
    private int _savedCurHp = 100;
    private int _savedCoinAmount = -1;
    private int _savedIslandIndex = -1;

    #region 初始化

    public void Init(int currentIslandIndex)
    {
        var cfg = GameConfig.Instance;

        if (SaveManager.IsLoading)
            InitFromSave(cfg);
        else
            InitNormal(currentIslandIndex, cfg);

        ApplyState(cfg);
    }

    /// <summary>SL 读档初始化：恢复到进入节点时的血量</summary>
    private void InitFromSave(GameConfig cfg)
    {
        int savedMax = SaveFileManager.GetInt(KEY_MAX_HP, -1);
        _savedMaxHp = savedMax > 0 ? savedMax : cfg.maxHp;

        int entryHp = SaveFileManager.GetInt(KEY_ENTRY_HP, -1);
        _savedCurHp = entryHp >= 0 ? entryHp : _savedMaxHp;

        _savedCoinAmount = SaveFileManager.GetInt(KEY_COIN, -1);
        if (_savedCoinAmount < 0) _savedCoinAmount = cfg.initialCoin;
    }

    /// <summary>正常初始化</summary>
    private void InitNormal(int currentIslandIndex, GameConfig cfg)
    {
        _savedCurHp = SaveFileManager.GetInt(KEY_CUR_HP, _savedCurHp);
        _savedCoinAmount = SaveFileManager.GetInt(KEY_COIN, -1);

        if (_savedIslandIndex != currentIslandIndex)
        {
            _savedIslandIndex = currentIslandIndex;
            PotionDropManager.ResetCounter();
            int savedMax = SaveFileManager.GetInt(KEY_MAX_HP, -1);
            _savedMaxHp = savedMax > 0 ? savedMax : cfg.maxHp;
            _savedCurHp = cfg.curHp;
        }
    }

    /// <summary>应用状态到公开字段</summary>
    private void ApplyState(GameConfig cfg)
    {
        MaxHp = _savedMaxHp > 0 ? _savedMaxHp : cfg.maxHp;
        CurHp = _savedCurHp > 0 ? _savedCurHp : MaxHp;
        MaxPowerCount = cfg.maxPowerCount;
        CurPowerCount = cfg.maxPowerCount;
        DefenseCount = 0;
        CoinAmount = _savedCoinAmount >= 0 ? _savedCoinAmount : cfg.initialCoin;
        _savedCoinAmount = CoinAmount;
    }

    #endregion

    #region 血量

    public void AddMaxHp(int amount)
    {
        MaxHp += amount;
        CurHp += amount;
        SavePersistentInts((KEY_MAX_HP, MaxHp), (KEY_CUR_HP, CurHp));
        _savedMaxHp = MaxHp;
        _savedCurHp = CurHp;
    }

    public void HealPlayer(int amount)
    {
        _savedCurHp = Mathf.Min(_savedCurHp + amount, _savedMaxHp);
        SaveFileManager.SetInt(KEY_CUR_HP, _savedCurHp);
        SaveFileManager.Flush();
        CurHp = _savedCurHp;
    }

    public void SaveHp()
    {
        _savedCurHp = CurHp;
        _savedMaxHp = MaxHp;
        SaveFileManager.SetInt(KEY_CUR_HP, _savedCurHp);
        SaveFileManager.Flush();
    }

    public static void ResetHp()
    {
        SaveFileManager.DeleteKey(KEY_CUR_HP);
        SaveFileManager.DeleteKey(KEY_MAX_HP);
        SaveFileManager.SetInt(KEY_COIN, GameConfig.Instance.initialCoin);
        SaveFileManager.Flush();
    }

    public static void StaticHealPlayer(int amount)
    {
        int curHp = SaveFileManager.GetInt(KEY_CUR_HP, 100);
        int maxHp = SaveFileManager.GetInt(KEY_MAX_HP, 100);
        curHp = Mathf.Min(curHp + amount, maxHp);
        SaveFileManager.SetInt(KEY_CUR_HP, curHp);
        SaveFileManager.Flush();
    }

    #endregion

    #region 金币

    public void AddCoin(int amount)
    {
        CoinAmount += amount;
        _savedCoinAmount = CoinAmount;
        SaveFileManager.SetInt(KEY_COIN, _savedCoinAmount);
        SaveFileManager.Flush();
    }

    public void SetCoinAmount(int amount)
    {
        CoinAmount = amount;
        if (_savedCoinAmount < 0) _savedCoinAmount = amount;
    }

    #endregion

    #region 工具

    /// <summary>批量写入多个 int 键值对</summary>
    private static void SavePersistentInts(params (string key, int value)[] entries)
    {
        foreach (var (k, v) in entries)
            SaveFileManager.SetInt(k, v);
        SaveFileManager.Flush();
    }

    #endregion
}
