using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

//敌人的行动枚举
public enum ActionType
{
    None,
    Defend,//加防御
    Attack,//攻击
    Heal,  //回血
}

/// <summary>
/// 敌人基类 - 核心逻辑（属性、初始化、行动、受伤）
/// 动画模块 → Enemy.Animation.cs
/// 状态模块 → Enemy.Status.cs
/// 视觉/特效模块 → Enemy.Visual.cs
/// </summary>
public partial class Enemy : MonoBehaviour
{
    #region 核心属性

    protected Dictionary<string, string> data;
    public ActionType type;

    public Animator ani;
    public int Defend;
    public int Attack;
    public int MaxHp;
    public int CurHp;
    [HideInInspector] public int turnsWithoutAttack = 0;
    [HideInInspector] public EnemyData enemyDataSO;

    #endregion

    #region 初始化

    public virtual void Init(Dictionary<string, string> data)
    {
        this.data = data;
        ParseAnimationConfig(data);
        LoadAttackEffectConfig();
    }

    public virtual void Init(EnemyData enemyData)
    {
        this.enemyDataSO = enemyData;
        this.data = new Dictionary<string, string>
        {
            { "Id", enemyData.id.ToString() },
            { "Name", enemyData.enemyName },
            { "Hp", enemyData.maxHp.ToString() },
            { "Attack", enemyData.attack.ToString() },
            { "Defend", enemyData.defense.ToString() },
            { "Model", enemyData.modelPath },
        };

        ParseAnimationConfigFromSO(enemyData);

        // 特效路径
        defenseEffectPath = string.IsNullOrEmpty(enemyData.defenseEffectPath)
            ? "Effects/MagicEffect/Prefabs/Magic circle 18" : enemyData.defenseEffectPath;
        healEffectPath = enemyData.healEffectPath;

        LoadAttackEffectConfigFromSO();
    }

    #endregion

    #region Start

    protected virtual void Start()
    {
        _meshRenderer = transform.GetComponentInChildren<SkinnedMeshRenderer>();
        ani = transform.GetComponent<Animator>();

        type = ActionType.None;
        turnsWithoutAttack = 0;
        hpItemObj = UIManager.Instance.CreateHpItem();
        actionObj = UIManager.Instance.CreateActionIcon();
        InitializeStatusUI();

        if (hpItemObj == null || actionObj == null) return;

        attackTf = actionObj.transform.Find("attack");
        defendTf = actionObj.transform.Find("defend");
        healTf = actionObj.transform.Find("Heart");

        defendTxt = hpItemObj.transform.Find("fangyu/Text")?.GetComponent<TextMeshProUGUI>();
        hpTxt = hpItemObj.transform.Find("hpTxt")?.GetComponent<TextMeshProUGUI>();
        hpImg = hpItemObj.transform.Find("fill")?.GetComponent<Image>();

        hpItemObj.transform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.down * 0.2f);
        Vector3 hpLocalPos = hpItemObj.transform.localPosition;
        actionObj.transform.localPosition = new Vector3(hpLocalPos.x + 110f, hpLocalPos.y, hpLocalPos.z);

        InitStats();
        SetRandomAction();
        UpdateHp();
        UpdateDefend();
    }

    protected virtual void InitStats()
    {
        if (enemyDataSO != null)
        {
            Attack = RandomizeValue(enemyDataSO.attack, enemyDataSO.attackVariance);
            Defend = enemyDataSO.initialDefense;
            CurHp = enemyDataSO.maxHp;
            MaxHp = CurHp;
        }
        else
        {
            Attack = int.TryParse(data["Attack"], out int a) ? a : 0;
            CurHp = int.TryParse(data["Hp"], out int h) ? h : 100;
            MaxHp = CurHp;
            Defend = int.TryParse(data["Defend"], out int d) ? d : 0;
        }
        isFlightMode = false;
        if (ani != null && ani.parameterCount > 0 && HasAnimatorParameter("isFlying"))
            ani.SetBool("isFlying", false);
    }

    protected int RandomizeValue(int baseValue, float variance)
    {
        if (variance <= 0f || baseValue <= 0) return baseValue;
        return Mathf.Max(1, Mathf.RoundToInt(baseValue * Random.Range(1f - variance, 1f + variance)));
    }

    #endregion

    #region 行动

    public virtual void SetRandomAction()
    {
        if (turnsWithoutAttack >= FORCE_ATTACK_THRESHOLD)
            type = ActionType.Attack;
        else
            type = (ActionType)Random.Range(1, 4);

        UpdateActionIcon();
    }

    public virtual IEnumerator DoAction()
    {
        HideAction();

        switch (type)
        {
            case ActionType.Attack:
                currentAttackAnimName = GetRandomAttackAnim();
                SafeCrossFade(currentAttackAnimName, 0.2f);
                PlayAttackEffect(currentAttackAnimName);
                break;
            case ActionType.Defend:
                SafeCrossFade(GetRandomDefenseAnim(), 0.2f);
                break;
            case ActionType.Heal:
                SafeCrossFade(GetHealAnim(), 0.2f);
                break;
            default:
                SafeCrossFade(GetCurrentIdleAnim(), 0.2f);
                break;
        }

        yield return new WaitForSeconds(0.5f);

        switch (type)
        {
            case ActionType.Defend:
                PerformDefend();
                break;
            case ActionType.Attack:
                PerformAttack();
                break;
            case ActionType.Heal:
                PerformHeal();
                break;
        }

        yield return new WaitForSeconds(1);

        turnsWithoutAttack = (type == ActionType.Attack) ? 0 : turnsWithoutAttack + 1;
        SafeCrossFade(GetCurrentIdleAnim(), 0f);
    }

    protected virtual void PerformDefend()
    {
        int shieldGain = 1;
        if (enemyDataSO != null)
            shieldGain = RandomizeValue(enemyDataSO.defense, enemyDataSO.defenseVariance);

        Defend += shieldGain;
        UpdateDefend();
        PlayDefendEffect();
    }

    protected virtual void PerformAttack()
    {
        int modifiedAttack = ModifyAttackDamage(Attack);
        FightManager.Instance.GetPlayerHit(modifiedAttack, this);
        Camera.main.DOShakePosition(0.1f, 0.2f, 5, 45);

        if (HasStatus(StatusType.Fear))
            RemoveStatus(StatusType.Fear, 1);
    }

    protected virtual void PerformHeal()
    {
        int healAmount;
        if (enemyDataSO != null && enemyDataSO.healAmount > 0)
            healAmount = RandomizeValue(enemyDataSO.healAmount, enemyDataSO.healVariance);
        else
            healAmount = Mathf.CeilToInt(MaxHp * 0.1f);
        CurHp = Mathf.Min(CurHp + healAmount, MaxHp);
        UpdateHp();
        PlayHealEffect();
    }

    /// <summary>根据动画名获取攻击伤害值</summary>
    public int GetAttackDamageForAnim(string animName)
    {
        if (enemyDataSO != null && enemyDataSO.attackDamages != null)
        {
            foreach (var entry in enemyDataSO.attackDamages)
            {
                if (entry.animName == animName)
                    return entry.damage;
            }
        }
        return Attack;
    }

    #endregion

    #region 受伤

    public virtual void Hit(int val)
    {
        int thorns = GetStatusStack(StatusType.Thorns);
        if (thorns > 0)
        {
            FightManager.Instance.GetPlayerHit(thorns);
            UIManager.Instance.ShowTip($"荆棘反伤 -{thorns}", Color.green);
        }

        val = ModifyTakenDamage(val);

        if (Defend >= val)
        {
            Defend -= val;
            if (ani != null && ani.HasState(0, Animator.StringToHash(GetCurrentHitAnim())))
                ani.Play(GetCurrentHitAnim(), 0, 0);
        }
        else
        {
            val -= Defend;
            Defend = 0;
            CurHp -= val;
            if (CurHp <= 0)
            {
                CurHp = 0;
                StopAllCoroutines();
                RelicManager.Instance.TriggerEnemyKilled(this);
                if (ani != null && ani.HasState(0, Animator.StringToHash("die")))
                    ani.Play("die");
                EnemyManager.Instance.DeleteEnemy(this);
                Destroy(gameObject, 1);
                Destroy(actionObj);
                Destroy(hpItemObj);
            }
            else
            {
                if (ani != null && ani.HasState(0, Animator.StringToHash(GetCurrentHitAnim())))
                    ani.Play(GetCurrentHitAnim(), 0, 0);
                StartCoroutine(ReturnToIdleAfterHit());
            }
        }

        UpdateDefend();
        UpdateHp();
    }

    #endregion

    #region 销毁

    protected virtual void OnDestroy()
    {
        if (actionObj != null) Destroy(actionObj);
        if (hpItemObj != null) Destroy(hpItemObj);
    }

    #endregion
}
