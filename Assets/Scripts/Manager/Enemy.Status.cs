using System.Collections.Generic;
using UnityEngine;

// ============================================================
// Enemy 状态/Buff 模块 (partial class)
// 负责：AddStatus、RemoveStatus、GetStatusStack、HasStatus、
//       ClearAllStatus、GetAllStatus、OnEnemyTurnStart/End、
//       攻击/受击伤害修正、StatusUI 初始化
// ============================================================
public partial class Enemy
{
    #region 状态数据

    protected Dictionary<StatusType, int> statusDict = new Dictionary<StatusType, int>();
    public event System.Action<StatusType, int, bool> OnStatusChanged;

    #endregion

    #region 状态增删查

    public void AddStatus(StatusType type, int stacks)
    {
        statusDict[type] = statusDict.ContainsKey(type) ? statusDict[type] + stacks : stacks;
        OnStatusChanged?.Invoke(type, statusDict[type], true);
    }

    public void RemoveStatus(StatusType type, int stacks = -1)
    {
        if (!statusDict.ContainsKey(type)) return;
        if (stacks < 0 || stacks >= statusDict[type])
        {
            statusDict.Remove(type);
            OnStatusChanged?.Invoke(type, 0, false);
        }
        else
        {
            statusDict[type] -= stacks;
            OnStatusChanged?.Invoke(type, statusDict[type], true);
        }
    }

    public int GetStatusStack(StatusType type) => statusDict.TryGetValue(type, out int s) ? s : 0;
    public bool HasStatus(StatusType type) => statusDict.ContainsKey(type) && statusDict[type] > 0;

    public void ClearAllStatus()
    {
        statusDict.Clear();
        OnStatusChanged?.Invoke(StatusType.Weak, 0, false);
    }

    public Dictionary<StatusType, int> GetAllStatus()
    {
        var valid = new Dictionary<StatusType, int>();
        foreach (var kvp in statusDict)
            if (kvp.Value > 0) valid[kvp.Key] = kvp.Value;
        return valid;
    }

    #endregion

    #region 回合触发

    /// <summary>敌人回合开始时触发状态效果</summary>
    public void OnEnemyTurnStart()
    {
        var snapshot = new Dictionary<StatusType, int>(statusDict);
        foreach (var kvp in snapshot)
        {
            if (kvp.Value <= 0) continue;
            var temp = new StatusEffect(kvp.Key, kvp.Value);
            StatusCallbacks.Inject(temp);
            temp.onEnemyTurnEnd?.Invoke(temp, this);
        }
    }

    /// <summary>敌人回合结束时触发状态效果（递减、伤害等）</summary>
    public void OnEnemyTurnEnd()
    {
        var snapshot = new Dictionary<StatusType, int>(statusDict);
        foreach (var kvp in snapshot)
        {
            if (kvp.Value <= 0) continue;
            var temp = new StatusEffect(kvp.Key, kvp.Value);
            StatusCallbacks.Inject(temp);
            temp.onEnemyTurnEnd?.Invoke(temp, this);
        }
    }

    #endregion

    #region 伤害修正

    /// <summary>敌人攻击伤害修正（通过状态回调计算）</summary>
    public int ModifyAttackDamage(int baseDamage)
    {
        int result = baseDamage;
        foreach (var kvp in statusDict)
        {
            if (kvp.Value <= 0) continue;
            var temp = new StatusEffect(kvp.Key, kvp.Value);
            StatusCallbacks.Inject(temp);
            if (temp.modifyAttackDamage != null)
                result = temp.modifyAttackDamage(temp, result);
        }
        return result;
    }

    /// <summary>敌人受到伤害修正（通过状态回调计算）</summary>
    public int ModifyTakenDamage(int baseDamage)
    {
        int result = baseDamage;
        foreach (var kvp in statusDict)
        {
            if (kvp.Value <= 0) continue;
            var temp = new StatusEffect(kvp.Key, kvp.Value);
            StatusCallbacks.Inject(temp);
            if (temp.modifyTakenDamage != null)
                result = temp.modifyTakenDamage(temp, result);
        }
        return result;
    }

    #endregion

    #region StatusUI

    private EnemyStatusUI statusUI;

    private void InitializeStatusUI()
    {
        if (hpItemObj == null) return;
        statusUI = hpItemObj.GetComponent<EnemyStatusUI>();
        if (statusUI == null) statusUI = hpItemObj.AddComponent<EnemyStatusUI>();
        Transform container = hpItemObj.transform.Find("hpTxt/StatusContainer");
        if (container != null)
        {
            statusUI.iconContainer = container.GetComponent<RectTransform>();
            statusUI.buffIconPrefab = ResourceCache.Get<GameObject>("UI/BuffIcon");
            statusUI.Show(true);
        }
        statusUI.Initialize(this);
        statusUI.RefreshUI();
    }

    #endregion
}
