using System.Collections.Generic;
using UnityEngine;

//战斗枚举
public enum FightType
{
    None,
    Init,
    Player,//玩家回合
    Enemy,//敌人回合
    Win,
    Loss
}

/// <summary>
/// 战斗管理器（门面）
/// 内部委托给 PlayerStateManager / RelicInventory / PotionInventory
/// </summary>
public class FightManager : MonoBehaviour
{
    public static FightManager Instance;

    #region 模块实例

    private PlayerStateManager _playerState = new PlayerStateManager();
    private RelicInventory _relicInventory = new RelicInventory();
    private PotionInventory _potionInventory = new PotionInventory();

    #endregion

    #region 静态缓存

    /// <summary>RelicsUI 静态引用缓存（由 RelicsUI.Awake 注册），避免 FindObjectOfType</summary>
    public static RelicsUI CachedRelicsUI;
    /// <summary>PotionPanelController 静态引用缓存（由 PotionPanelController.Awake 注册），避免 FindObjectOfType</summary>
    public static PotionPanelController CachedPotionPanel;

    #endregion

    #region 属性转发（保持外部 API 不变）

    public int MaxHp { get => _playerState.MaxHp; set => _playerState.MaxHp = value; }
    public int CurHp { get => _playerState.CurHp; set => _playerState.CurHp = value; }
    public int MaxPowerCount { get => _playerState.MaxPowerCount; set => _playerState.MaxPowerCount = value; }
    public int CurPowerCount { get => _playerState.CurPowerCount; set => _playerState.CurPowerCount = value; }
    public int DefenseCount { get => _playerState.DefenseCount; set => _playerState.DefenseCount = value; }
    public int CoinAmount => _playerState.CoinAmount;
    public int cardsPlayedThisTurn { get => _playerState.CardsPlayedThisTurn; set => _playerState.CardsPlayedThisTurn = value; }

    public List<RelicData> relicList => _relicInventory.RelicList;
    public List<PotionData> potionList => _potionInventory.PotionList;

    public FightUnit fightUnit;

    #endregion

    #region 岛屿/节点

    [HideInInspector] public int currentIslandIndex;
    [HideInInspector] public Vector2Int currentNodePoint;
    [HideInInspector] public Map.NodeType currentNodeType;

    public void SetCurrentIslandIndex(int index) => currentIslandIndex = index;
    public int GetCurrentIslandIndex() => currentIslandIndex;
    public void SetCurrentNodePoint(Vector2Int point) => currentNodePoint = point;
    public Vector2Int GetCurrentNodePoint() => currentNodePoint;
    public Map.NodeType GetCurrentNodeType() => currentNodeType;
    public void SetCurrentNodeType(Map.NodeType type) => currentNodeType = type;

    #endregion

    #region Unity 生命周期

    private void Awake() => Instance = this;

    private void Update()
    {
        fightUnit?.OnUpdate();
    }

    #endregion

    #region 初始化

    public void Init()
    {
        _playerState.Init(currentIslandIndex);
    }

    public void InitPotions() => _potionInventory.Init();
    public void InitRelics() => _relicInventory.Init();

    #endregion

    #region 遗物管理（转发）

    public void AddRelic(RelicData relicData) => _relicInventory.Add(relicData);
    public void RemoveRelic(RelicData relicData) => _relicInventory.Remove(relicData);

    #endregion

    #region 血量/金币管理（转发）

    public void SetCoinAmount(int amount) => _playerState.SetCoinAmount(amount);
    public void AddCoin(int amount)
    {
        _playerState.AddCoin(amount);
        UIManager.Instance.GetUI<FightUI>("FightUI")?.UpdateCoinDisplay(CoinAmount);
    }

    public void AddMaxHp(int amount)
    {
        _playerState.AddMaxHp(amount);
        UIManager.Instance.GetUI<FightUI>("FightUI")?.UpdateHp();
    }

    public static void ResetHp() => PlayerStateManager.ResetHp();
    public static void HealPlayer(int amount) => PlayerStateManager.StaticHealPlayer(amount);

    #endregion

    #region 战斗流程

    public void ChangeType(FightType type)
    {
        switch (type)
        {
            case FightType.Init: fightUnit = new FightInit(); break;
            case FightType.Player: fightUnit = new Fight_PlayerTurn(); break;
            case FightType.Enemy: fightUnit = new Fight_EnemyTurn(); break;
            case FightType.Win: fightUnit = new Fight_Win(); break;
            case FightType.Loss: fightUnit = new Fight_Loss(); break;
        }
        fightUnit?.Init();
    }

    public void GetPlayerHit(int hit, Enemy attacker = null)
    {
        // 幸运格挡
        if (BuffManager.Instance.HasStatus(StatusType.LuckyBlock))
        {
            BuffManager.Instance.RemoveStatus(StatusType.LuckyBlock, 1);
            UIManager.Instance.ShowTip("幸运格挡!", Color.cyan);
            return;
        }

        // 荆棘反伤
        int thorns = BuffManager.Instance.GetStack(StatusType.Thorns);
        if (thorns > 0 && attacker != null && attacker.CurHp > 0)
        {
            attacker.Hit(thorns);
            UIManager.Instance.ShowTip($"荆棘反伤 -{thorns}", Color.green);
        }

        // 应用易伤Buff修改
        int finalHit = BuffManager.Instance.ModifyTakenDamage(hit);

        //扣护盾
        if (DefenseCount >= finalHit)
        {
            DefenseCount -= finalHit;
        }
        else
        {
            finalHit = finalHit - DefenseCount;
            DefenseCount = 0;
            CurHp -= finalHit;
            UIManager.Instance.ShowDamageEffect();

            if (CurHp <= 0)
            {
                if (_potionInventory.TryUseBottledSprite())
                {
                    CurHp = Mathf.CeilToInt(MaxHp * 0.3f);
                }
                else
                {
                    CurHp = 0;
                    ChangeType(FightType.Loss);
                    _playerState.SaveHp();

                    var fightUI2 = UIManager.Instance.GetUI<FightUI>("FightUI");
                    if (fightUI2 != null)
                    {
                        fightUI2.UpdateHp();
                        fightUI2.UpdateDefense();
                    }
                    return;
                }
            }
        }

        _playerState.SaveHp();

        var fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
        {
            fightUI.UpdateHp();
            fightUI.UpdateDefense();
        }
    }

    #endregion
}
