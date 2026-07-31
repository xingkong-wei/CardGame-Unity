using System.Collections.Generic;
using UnityEngine;

// ============================================================
// Enemy 动画模块 (partial class)
// 负责：动画列表配置、随机选取、CrossFade 播放、飞行模式切换
// ============================================================
public partial class Enemy
{
    #region 动画配置

    protected List<string> attackAnimList = new List<string>();
    protected List<string> defenseAnimList = new List<string>();
    protected string idleAnimName = "idle";
    protected string hitAnimName = "hit";

    protected List<string> flightAttackAnimList = new List<string>();
    protected List<string> flightDefenseAnimList = new List<string>();
    protected string flightIdleAnimName = "idle";
    protected string flightHitAnimName = "hit";

    [HideInInspector] public bool isFlightMode = false;
    protected const int FORCE_ATTACK_THRESHOLD = 3;

    #endregion

    #region 动画列表解析（从字典/ScriptableObject 加载）

    private List<string> ParseAnimList(Dictionary<string, string> d, string key, string fallback)
    {
        List<string> list = new List<string>();
        if (d.TryGetValue(key, out string val) && !string.IsNullOrEmpty(val))
        {
            foreach (string anim in val.Split('='))
                if (!string.IsNullOrEmpty(anim.Trim())) list.Add(anim.Trim());
        }
        if (list.Count == 0 && fallback != null) list.Add(fallback);
        return list;
    }

    private string GetStr(Dictionary<string, string> d, string key, string fallback)
    {
        return d.TryGetValue(key, out string val) && !string.IsNullOrEmpty(val) ? val.Trim() : fallback;
    }

    private void ParseAnimationConfig(Dictionary<string, string> d)
    {
        attackAnimList = ParseAnimList(d, "AttackAnim", "attack");
        defenseAnimList = ParseAnimList(d, "DefenseAnim", "defense");
        idleAnimName = GetStr(d, "IdleAnim", "idle");
        hitAnimName = GetStr(d, "HitAnim", "hit");
        flightAttackAnimList = ParseAnimList(d, "FlightAttackAnim", null);
        if (flightAttackAnimList.Count == 0) flightAttackAnimList.AddRange(attackAnimList);
        flightDefenseAnimList = ParseAnimList(d, "FlightDefenseAnim", null);
        if (flightDefenseAnimList.Count == 0) flightDefenseAnimList.AddRange(defenseAnimList);
        flightIdleAnimName = GetStr(d, "FlightIdleAnim", idleAnimName);
        flightHitAnimName = GetStr(d, "FlightHitAnim", hitAnimName);
        defenseEffectPath = GetStr(d, "DefenseEffect", defenseEffectPath);
    }

    private void ParseAnimationConfigFromSO(EnemyData ed)
    {
        // 攻击动画
        if (!string.IsNullOrEmpty(ed.attackAnim))
        {
            foreach (string anim in ed.attackAnim.Split('='))
                if (!string.IsNullOrEmpty(anim.Trim())) attackAnimList.Add(anim.Trim());
        }
        if (attackAnimList.Count == 0) attackAnimList.Add("attack");

        // 防御动画
        if (!string.IsNullOrEmpty(ed.defenseAnim))
        {
            foreach (string anim in ed.defenseAnim.Split('='))
                if (!string.IsNullOrEmpty(anim.Trim())) defenseAnimList.Add(anim.Trim());
        }
        if (defenseAnimList.Count == 0) defenseAnimList.Add("defense");

        idleAnimName = string.IsNullOrEmpty(ed.idleAnim) ? "idle" : ed.idleAnim;
        hitAnimName = string.IsNullOrEmpty(ed.hitAnim) ? "hit" : ed.hitAnim;

        // 飞行动画
        if (!string.IsNullOrEmpty(ed.flightAttackAnim))
        {
            foreach (string anim in ed.flightAttackAnim.Split('='))
                if (!string.IsNullOrEmpty(anim.Trim())) flightAttackAnimList.Add(anim.Trim());
        }
        if (flightAttackAnimList.Count == 0) flightAttackAnimList.AddRange(attackAnimList);

        if (!string.IsNullOrEmpty(ed.flightDefenseAnim))
        {
            foreach (string anim in ed.flightDefenseAnim.Split('='))
                if (!string.IsNullOrEmpty(anim.Trim())) flightDefenseAnimList.Add(anim.Trim());
        }
        if (flightDefenseAnimList.Count == 0) flightDefenseAnimList.AddRange(defenseAnimList);

        flightIdleAnimName = string.IsNullOrEmpty(ed.flightIdleAnim) ? idleAnimName : ed.flightIdleAnim;
        flightHitAnimName = string.IsNullOrEmpty(ed.flightHitAnim) ? hitAnimName : ed.flightHitAnim;
    }

    #endregion

    #region 动画获取与播放

    protected List<string> GetCurrentAttackAnimList() => isFlightMode ? flightAttackAnimList : attackAnimList;
    protected List<string> GetCurrentDefenseAnimList() => isFlightMode ? flightDefenseAnimList : defenseAnimList;
    protected string GetCurrentIdleAnim() => isFlightMode ? flightIdleAnimName : idleAnimName;
    protected string GetCurrentHitAnim() => isFlightMode ? flightHitAnimName : hitAnimName;

    protected string GetRandomAttackAnim()
    {
        var list = GetCurrentAttackAnimList();
        return list.Count > 0 ? list[Random.Range(0, list.Count)] : "attack";
    }

    protected string GetRandomDefenseAnim()
    {
        var list = GetCurrentDefenseAnimList();
        return list.Count > 0 ? list[Random.Range(0, list.Count)] : "defense";
    }

    protected string GetHealAnim()
    {
        if (enemyDataSO != null && !string.IsNullOrEmpty(enemyDataSO.healAnim))
        {
            string[] anims = enemyDataSO.healAnim.Split('=');
            if (anims.Length > 0 && !string.IsNullOrEmpty(anims[0].Trim())) return anims[0].Trim();
        }
        if (data != null && data.TryGetValue("HealAnim", out string ha) && !string.IsNullOrEmpty(ha))
            return ha.Trim();
        return "heal";
    }

    public void SafeCrossFade(string animName, float fadeTime)
    {
        if (ani == null) return;
        if (ani.HasState(0, Animator.StringToHash(animName)))
            ani.CrossFade(animName, fadeTime);
    }

    protected System.Collections.IEnumerator ReturnToIdleAfterHit()
    {
        yield return new WaitForSeconds(0.5f);
        SafeCrossFade(GetCurrentIdleAnim(), 0f);
    }

    #endregion

    #region 飞行模式

    protected void CheckFlightMode()
    {
        if (isFlightMode) return;
        float threshold = 0.5f;
        if (enemyDataSO != null) threshold = enemyDataSO.flightThreshold;
        else if (data != null && data.TryGetValue("FlightThreshold", out string t))
            float.TryParse(t, out threshold);

        if (MaxHp > 0 && CurHp <= MaxHp * threshold)
        {
            isFlightMode = true;
            if (HasAnimatorParameter("isFlying")) ani.SetBool("isFlying", true);
            OnFlightModeChanged();
        }
    }

    protected virtual void OnFlightModeChanged()
    {
        UpdateActionIcon();
    }

    protected bool HasAnimatorParameter(string paramName)
    {
        if (ani == null || ani.runtimeAnimatorController == null) return false;
        foreach (var p in ani.parameters)
            if (p.name == paramName) return true;
        return false;
    }

    #endregion
}
