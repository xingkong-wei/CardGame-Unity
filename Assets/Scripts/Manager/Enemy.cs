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
/// 敌人基类 - 提供通用逻辑（UI、动画、Buff、特效）
/// 子类可重写 SetRandomAction / DoAction / OnHit 等方法实现专属行为
/// </summary>
public class Enemy : MonoBehaviour
{
    protected Dictionary<string, string> data;
    public ActionType type;

    public GameObject hpItemObj;
    public GameObject actionObj;
    private EnemyStatusUI statusUI;

    public Transform attackTf;
    public Transform defendTf;
    public Transform healTf;
    public TextMeshProUGUI defendTxt;
    public TextMeshProUGUI hpTxt;
    public Image hpImg;

    public int Defend;
    public int Attack;
    public int MaxHp;
    public int CurHp;

    protected SkinnedMeshRenderer _meshRenderer;
    public Animator ani;

    protected List<string> attackAnimList = new List<string>();
    protected List<string> defenseAnimList = new List<string>();
    protected string idleAnimName = "idle";
    protected string hitAnimName = "hit";

    protected List<string> flightAttackAnimList = new List<string>();
    protected List<string> flightDefenseAnimList = new List<string>();
    protected string flightIdleAnimName = "idle";
    protected string flightHitAnimName = "hit";

    [HideInInspector] public bool isFlightMode = false;
    [HideInInspector] public int turnsWithoutAttack = 0;
    protected const int FORCE_ATTACK_THRESHOLD = 3;

    protected string defenseEffectPath = "Effects/MagicEffect/Prefabs/Magic circle 18";
    protected string healEffectPath;

    protected AttackEffectConfig attackEffectConfig;
    protected string currentAttackAnimName;
    private const string DEFAULT_ATTACK_EFFECT_CONFIG_PATH = "Data_Enemy/MonsterAttackEffects";

    [HideInInspector] public EnemyData enemyDataSO;

    // ===== 初始化 =====

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

        // 动画
        if (!string.IsNullOrEmpty(enemyData.attackAnim))
        {
            foreach (string anim in enemyData.attackAnim.Split('='))
                if (!string.IsNullOrEmpty(anim.Trim())) attackAnimList.Add(anim.Trim());
        }
        if (attackAnimList.Count == 0) attackAnimList.Add("attack");

        if (!string.IsNullOrEmpty(enemyData.defenseAnim))
        {
            foreach (string anim in enemyData.defenseAnim.Split('='))
                if (!string.IsNullOrEmpty(anim.Trim())) defenseAnimList.Add(anim.Trim());
        }
        if (defenseAnimList.Count == 0) defenseAnimList.Add("defense");

        idleAnimName = string.IsNullOrEmpty(enemyData.idleAnim) ? "idle" : enemyData.idleAnim;
        hitAnimName = string.IsNullOrEmpty(enemyData.hitAnim) ? "hit" : enemyData.hitAnim;

        // 飞行
        if (!string.IsNullOrEmpty(enemyData.flightAttackAnim))
        {
            foreach (string anim in enemyData.flightAttackAnim.Split('='))
                if (!string.IsNullOrEmpty(anim.Trim())) flightAttackAnimList.Add(anim.Trim());
        }
        if (flightAttackAnimList.Count == 0) flightAttackAnimList.AddRange(attackAnimList);

        if (!string.IsNullOrEmpty(enemyData.flightDefenseAnim))
        {
            foreach (string anim in enemyData.flightDefenseAnim.Split('='))
                if (!string.IsNullOrEmpty(anim.Trim())) flightDefenseAnimList.Add(anim.Trim());
        }
        if (flightDefenseAnimList.Count == 0) flightDefenseAnimList.AddRange(defenseAnimList);

        flightIdleAnimName = string.IsNullOrEmpty(enemyData.flightIdleAnim) ? idleAnimName : enemyData.flightIdleAnim;
        flightHitAnimName = string.IsNullOrEmpty(enemyData.flightHitAnim) ? hitAnimName : enemyData.flightHitAnim;

        // 特效
        defenseEffectPath = string.IsNullOrEmpty(enemyData.defenseEffectPath)
            ? "Effects/MagicEffect/Prefabs/Magic circle 18" : enemyData.defenseEffectPath;
        healEffectPath = enemyData.healEffectPath;

        LoadAttackEffectConfigFromSO();
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

    // ===== Start =====

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
            // 初始护盾：登场时自带，不使用随机波动
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

    // ===== 行动（子类可重写） =====

    public virtual void SetRandomAction()
    {
        if (turnsWithoutAttack >= FORCE_ATTACK_THRESHOLD)
            type = ActionType.Attack;
        else
            type = (ActionType)Random.Range(1, 4);

        UpdateActionIcon();
    }

    protected void UpdateActionIcon()
    {
        if (attackTf != null) attackTf.gameObject.SetActive(type == ActionType.Attack);
        if (defendTf != null) defendTf.gameObject.SetActive(type == ActionType.Defend);
        if (healTf != null) healTf.gameObject.SetActive(type == ActionType.Heal);
    }

    public void HideAction()
    {
        if (attackTf != null) attackTf.gameObject.SetActive(false);
        if (defendTf != null) defendTf.gameObject.SetActive(false);
        if (healTf != null) healTf.gameObject.SetActive(false);
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
        // 防御动作：获得护盾（使用 EnemyData.defense 的值，带随机波动）
        int shieldGain = 1; // 兼容旧字典方式
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

        // 恐惧：攻击后减1层
        if (HasStatus(StatusType.Fear))
            RemoveStatus(StatusType.Fear, 1);
    }

    /// <summary>
    /// 根据动画名获取攻击伤害值
    /// 优先从 EnemyData.attackDamages 查找，找不到则使用默认 Attack
    /// </summary>
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

    protected virtual void PerformHeal()
    {
        int healAmount;
        if (enemyDataSO != null && enemyDataSO.healAmount > 0)
        {
            healAmount = RandomizeValue(enemyDataSO.healAmount, enemyDataSO.healVariance);
        }
        else
        {
            healAmount = Mathf.CeilToInt(MaxHp * 0.1f);
        }
        CurHp = Mathf.Min(CurHp + healAmount, MaxHp);
        UpdateHp();
        PlayHealEffect();
    }

    // ===== 受伤 =====

    public virtual void Hit(int val)
    {
        // 荆棘反伤：敌人有荆棘时反弹给攻击者（玩家）
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

    protected IEnumerator ReturnToIdleAfterHit()
    {
        yield return new WaitForSeconds(0.5f);
        SafeCrossFade(GetCurrentIdleAnim(), 0f);
    }

    // ===== UI 更新 =====

    public void UpdateHp()
    {
        if (hpTxt != null && hpImg != null)
        {
            hpTxt.text = CurHp + "/" + MaxHp;
            hpImg.fillAmount = (float)CurHp / (float)MaxHp;
        }
        CheckFlightMode();
    }

    public void UpdateDefend()
    {
        if (defendTxt != null) defendTxt.text = Defend.ToString();
    }

    // ===== 选中/未选中 =====

    public virtual void OnSelect()
    {
        if (_meshRenderer == null) return;
        int otlID = Shader.PropertyToID("_OtlColor");
        int emissionID = Shader.PropertyToID("_EmissionColor");
        foreach (Material mat in _meshRenderer.materials)
        {
            if (mat == null) continue;
            if (mat.HasProperty(otlID)) mat.SetColor(otlID, Color.red);
            if (mat.HasProperty(emissionID))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor(emissionID, new Color(1f, 0f, 0f, 1f));
            }
        }
    }

    public virtual void OnUnSelect()
    {
        if (_meshRenderer == null) return;
        foreach (Material mat in _meshRenderer.materials)
        {
            if (mat == null) continue;
            int otlID = Shader.PropertyToID("_OtlColor");
            if (mat.HasProperty(otlID)) mat.SetColor(otlID, Color.black);
            int emissionID = Shader.PropertyToID("_EmissionColor");
            if (mat.HasProperty(emissionID)) mat.SetColor(emissionID, Color.black);
        }
    }

    // ===== 动画 =====

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

    protected void SafeCrossFade(string animName, float fadeTime)
    {
        if (ani == null) return;
        if (ani.HasState(0, Animator.StringToHash(animName)))
            ani.CrossFade(animName, fadeTime);
    }

    // ===== 飞行 =====

    /// <summary>切换到飞行模式时触发，子类可重写更新行动意图和图标</summary>
    protected virtual void OnFlightModeChanged()
    {
        UpdateActionIcon();
    }

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

    protected bool HasAnimatorParameter(string paramName)
    {
        if (ani == null || ani.runtimeAnimatorController == null) return false;
        foreach (var p in ani.parameters)
            if (p.name == paramName) return true;
        return false;
    }

    // ===== Buff =====

    protected Dictionary<StatusType, int> statusDict = new Dictionary<StatusType, int>();
    public event System.Action<StatusType, int, bool> OnStatusChanged;

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

    /// <summary>
    /// 敌人回合开始时触发状态效果
    /// </summary>
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

    /// <summary>
    /// 敌人回合结束时触发状态效果（递减、伤害等）
    /// 通过 StatusCallbacks 注册的行为驱动，新增状态无需修改此处
    /// </summary>
    public void OnEnemyTurnEnd()
    {
        // 快照遍历，防止回调中修改 statusDict
        var snapshot = new Dictionary<StatusType, int>(statusDict);
        foreach (var kvp in snapshot)
        {
            if (kvp.Value <= 0) continue;
            var temp = new StatusEffect(kvp.Key, kvp.Value);
            StatusCallbacks.Inject(temp);
            temp.onEnemyTurnEnd?.Invoke(temp, this);
        }
    }

    private void InitializeStatusUI()
    {
        if (hpItemObj == null) return;
        statusUI = hpItemObj.GetComponent<EnemyStatusUI>();
        if (statusUI == null) statusUI = hpItemObj.AddComponent<EnemyStatusUI>();
        Transform container = hpItemObj.transform.Find("hpTxt/StatusContainer");
        if (container != null)
        {
            statusUI.iconContainer = container.GetComponent<RectTransform>();
            statusUI.buffIconPrefab = Resources.Load<GameObject>("UI/BuffIcon");
            statusUI.Show(true);
        }
        statusUI.Initialize(this);
        statusUI.RefreshUI();
    }

    // ===== 特效 =====

    protected void PlayDefendEffect()
    {
        GameObject prefab = Resources.Load<GameObject>(defenseEffectPath);
        if (prefab == null) return;
        EffectParams p = enemyDataSO != null ? enemyDataSO.defenseEffectParams : new EffectParams();
        SpawnGroundEffect(prefab, p);
    }

    protected void PlayHealEffect()
    {
        if (string.IsNullOrEmpty(healEffectPath)) return;
        GameObject prefab = Resources.Load<GameObject>(healEffectPath);
        if (prefab == null) return;
        EffectParams p = enemyDataSO != null ? enemyDataSO.healEffectParams : new EffectParams();
        SpawnGroundEffect(prefab, p);
    }

    protected void SpawnGroundEffect(GameObject prefab, EffectParams p)
    {
        Vector3 pos = new Vector3(transform.position.x, 0f, transform.position.z) + p.positionOffset;
        GameObject effect = Instantiate(prefab, pos, Quaternion.Euler(p.rotationOffset));
        effect.transform.localScale *= p.effectScale;
        Destroy(effect, p.duration > 0 ? p.duration : 2f);
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

    protected void PlayAttackEffect(string attackAnimName)
    {
        if (attackEffectConfig == null || string.IsNullOrEmpty(attackAnimName)) return;
        AttackEffectData effectData = attackEffectConfig.GetEffectByName(attackAnimName);
        if (effectData == null)
        {
            foreach (var item in attackEffectConfig.attackEffects)
            {
                if (attackAnimName.Contains(item.effectName) || item.effectName.Contains(attackAnimName))
                { effectData = item; break; }
            }
        }
        if (effectData == null) return;

        GameObject prefab = Resources.Load<GameObject>(effectData.effectPrefabPath);
        if (prefab == null) return;

        Vector3 spawnPos = CalculateEffectPosition(effectData.spawnPositionType, effectData.positionOffset, effectData.spawnTransformPath);
        if (effectData.delayTime > 0)
            StartCoroutine(DelayedSpawnEffect(prefab, spawnPos, effectData));
        else
            SpawnEffect(prefab, spawnPos, effectData);
    }

    protected Vector3 CalculateEffectPosition(int type, Vector3 offset, string path)
    {
        Vector3 basePos = transform.position;
        switch (type)
        {
            case 0: return new Vector3(basePos.x + offset.x, 0f + offset.y, basePos.z + offset.z);
            case 1: return basePos + offset;
            case 2: return new Vector3(basePos.x + offset.x, basePos.y + 1f + offset.y, basePos.z + offset.z);
            case 3: return Camera.main != null ? new Vector3(Camera.main.transform.position.x, 0f, Camera.main.transform.position.z) + offset : basePos + offset;
            case 4:
                if (!string.IsNullOrEmpty(path)) { Transform t = transform.Find(path); if (t != null) return t.position + offset; }
                return basePos + offset;
            default: return basePos + offset;
        }
    }

    protected IEnumerator DelayedSpawnEffect(GameObject prefab, Vector3 pos, AttackEffectData data)
    {
        yield return new WaitForSeconds(data.delayTime);
        SpawnEffect(prefab, pos, data);
    }

    protected void SpawnEffect(GameObject prefab, Vector3 pos, AttackEffectData data)
    {
        if (prefab == null) return;
        GameObject obj = Instantiate(prefab, pos, Quaternion.Euler(data.rotationOffset));
        obj.transform.localScale *= data.effectScale;
        Destroy(obj, data.duration > 0 ? data.duration : 2f);
    }

    // ===== 特效配置加载 =====

    private void LoadAttackEffectConfigFromSO()
    {
        if (enemyDataSO == null) { LoadAttackEffectConfig(); return; }
        attackEffectConfig = Resources.Load<AttackEffectConfig>($"Data_Enemy/EnemyAttackEffects/{enemyDataSO.id}")
            ?? Resources.Load<AttackEffectConfig>($"Data_Enemy/EnemyAttackEffects/{enemyDataSO.enemyName}")
            ?? Resources.Load<AttackEffectConfig>(DEFAULT_ATTACK_EFFECT_CONFIG_PATH);
    }

    private void LoadAttackEffectConfig()
    {
        string id = data.TryGetValue("Id", out string i) ? i : "";
        string name = data.TryGetValue("Name", out string n) ? n : "";
        attackEffectConfig = Resources.Load<AttackEffectConfig>($"Data_Enemy/EnemyAttackEffects/{id}")
            ?? Resources.Load<AttackEffectConfig>($"Data_Enemy/EnemyAttackEffects/{name}")
            ?? Resources.Load<AttackEffectConfig>(DEFAULT_ATTACK_EFFECT_CONFIG_PATH);
        if (attackEffectConfig == null) attackEffectConfig = CreateDefaultAttackEffectConfig();
    }

    private AttackEffectConfig CreateDefaultAttackEffectConfig()
    {
        var config = ScriptableObject.CreateInstance<AttackEffectConfig>();
        config.attackEffects = new List<AttackEffectData>
        {
            new AttackEffectData { effectName="Bite Attack", effectPrefabPath="Effects/MagicEffect/Prefabs/Magic circle 12", spawnPositionType=0, effectScale=1.5f, delayTime=0.3f, duration=1f, rotationOffset=new Vector3(-90,0,0) },
            new AttackEffectData { effectName="Breath Attack", effectPrefabPath="Effects/MagicEffect/Prefabs/Magic circle 13", spawnPositionType=2, effectScale=2f, delayTime=0.4f, duration=1.5f },
            new AttackEffectData { effectName="Head Attack", effectPrefabPath="Effects/MagicEffect/Prefabs/Magic circle 14", spawnPositionType=1, effectScale=1.5f, delayTime=0.3f, duration=1f },
            new AttackEffectData { effectName="ProjectileAttack", effectPrefabPath="Effects/MagicEffect/Prefabs/Magic circle 15", spawnPositionType=1, effectScale=1f, delayTime=0.2f, duration=0.8f },
            new AttackEffectData { effectName="FireBreathAttack", effectPrefabPath="Effects/MagicEffect/Prefabs/Magic circle 18", spawnPositionType=2, effectScale=2.5f, delayTime=0.5f, duration=2f },
            new AttackEffectData { effectName="CastSpell", effectPrefabPath="Effects/MagicEffect/Prefabs/Magic circle 16", spawnPositionType=0, effectScale=2f, delayTime=0.2f, duration=1.5f, rotationOffset=new Vector3(-90,0,0) },
            new AttackEffectData { effectName="attack", effectPrefabPath="Effects/MagicEffect/Prefabs/Magic circle 12", spawnPositionType=0, effectScale=1f, delayTime=0.3f, duration=1f, rotationOffset=new Vector3(-90,0,0) },
        };
        return config;
    }

    protected int RandomizeValue(int baseValue, float variance)
    {
        if (variance <= 0f || baseValue <= 0) return baseValue;
        return Mathf.Max(1, Mathf.RoundToInt(baseValue * Random.Range(1f - variance, 1f + variance)));
    }

    protected virtual void OnDestroy()
    {
        if (actionObj != null) Destroy(actionObj);
        if (hpItemObj != null) Destroy(hpItemObj);
    }
}
