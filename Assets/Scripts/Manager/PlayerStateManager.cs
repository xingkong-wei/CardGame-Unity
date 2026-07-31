using UnityEngine;

/// <summary>
/// 玩家状态管理器
/// 负责：血量、护盾、能量、金币 的存取和二进制文件持久化
/// </summary>
public class PlayerStateManager
{
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

        _savedCurHp = SaveFileManager.GetInt("SavedCurHp", _savedCurHp);
        _savedCoinAmount = SaveFileManager.GetInt("SavedCoinAmount", -1);

        if (_savedIslandIndex != currentIslandIndex)
        {
            _savedIslandIndex = currentIslandIndex;
            PotionDropManager.ResetCounter();
            int savedMaxHpPref = SaveFileManager.GetInt("SavedMaxHp", -1);
            _savedMaxHp = savedMaxHpPref > 0 ? savedMaxHpPref : cfg.maxHp;
            _savedCurHp = cfg.curHp;
        }

        MaxHp = _savedMaxHp;
        CurHp = _savedCurHp;
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
        _savedMaxHp = MaxHp;
        _savedCurHp = CurHp;
        SaveFileManager.SetInt("SavedMaxHp", _savedMaxHp);
        SaveFileManager.SetInt("SavedCurHp", _savedCurHp);
        SaveFileManager.Flush();
    }

    public void HealPlayer(int amount)
    {
        _savedCurHp = Mathf.Min(_savedCurHp + amount, _savedMaxHp);
        SaveFileManager.SetInt("SavedCurHp", _savedCurHp);
        SaveFileManager.Flush();
        CurHp = _savedCurHp;
    }

    public void SaveHp()
    {
        _savedCurHp = CurHp;
        _savedMaxHp = MaxHp;
        SaveFileManager.SetInt("SavedCurHp", _savedCurHp);
        SaveFileManager.Flush();
    }

    public static void ResetHp()
    {
        var cfg = GameConfig.Instance;
        SaveFileManager.DeleteKey("SavedCurHp");
        SaveFileManager.DeleteKey("SavedMaxHp");
        SaveFileManager.SetInt("SavedCoinAmount", cfg.initialCoin);
        SaveFileManager.Flush();
    }

    public static void StaticHealPlayer(int amount)
    {
        int curHp = SaveFileManager.GetInt("SavedCurHp", 100);
        int maxHp = SaveFileManager.GetInt("SavedMaxHp", 100);
        curHp = Mathf.Min(curHp + amount, maxHp);
        SaveFileManager.SetInt("SavedCurHp", curHp);
        SaveFileManager.Flush();
    }

    #endregion

    #region 金币

    public void AddCoin(int amount)
    {
        CoinAmount += amount;
        _savedCoinAmount = CoinAmount;
        SaveFileManager.SetInt("SavedCoinAmount", _savedCoinAmount);
        SaveFileManager.Flush();
    }

    public void SetCoinAmount(int amount)
    {
        CoinAmount = amount;
        if (_savedCoinAmount < 0) _savedCoinAmount = amount;
    }

    #endregion
}
