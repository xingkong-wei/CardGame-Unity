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

//敌人脚本
public class Enemy : MonoBehaviour
{
    protected Dictionary<string, string> data;//敌人数据表信息

    public ActionType type;

    public GameObject hpItemObj;
    public GameObject actionObj;
    private EnemyStatusUI statusUI; // 敌人状态UI组件

    //UI组件
    public Transform attackTf;
    public Transform defendTf;
    public Transform healTf;
    public TextMeshProUGUI defendTxt;
    public TextMeshProUGUI hpTxt;
    public Image hpImg;

    //数值字段
    public int Defend;
    public int Attack;
    public int MaxHp;
    public int CurHp;

    //模型组件
    SkinnedMeshRenderer _meshRenderer;
    public Animator ani;

    // 动画名称列表（从配置读取）
    private List<string> attackAnimList = new List<string>();
    private List<string> defenseAnimList = new List<string>();
    private string idleAnimName = "idle";
    private string hitAnimName = "hit";

    // 飞行动画列表
    private List<string> flightAttackAnimList = new List<string>();
    private List<string> flightDefenseAnimList = new List<string>();
    private string flightIdleAnimName = "idle";
    private string flightHitAnimName = "hit";

    // 飞行状态
    private bool isFlightMode = false;
    
    // 连续未攻击回合计数
    private int turnsWithoutAttack = 0;
    // 强制攻击阈值（连续N回合未攻击则强制攻击）
    private const int FORCE_ATTACK_THRESHOLD = 3;

    // 防御特效路径
    private string defenseEffectPath = "Effects/Magic circle 18";

    // 回血特效路径
    private string healEffectPath;

    // 攻击特效配置
    private AttackEffectConfig attackEffectConfig;
    private string currentAttackAnimName; // 当前执行的攻击动画名称

    // 默认攻击特效配置路径
    private const string DEFAULT_ATTACK_EFFECT_CONFIG_PATH = "Data_Enemy/MonsterAttackEffects";

    // EnemyData ScriptableObject 引用
    private EnemyData enemyDataSO;

    public void Init(Dictionary<string, string> data)
    {
        this.data = data;

        // 从配置读取动画名称（多个动画用 = 分隔）
        if (data.ContainsKey("AttackAnim") && !string.IsNullOrEmpty(data["AttackAnim"]))
        {
            string[] anims = data["AttackAnim"].Split('=');
            foreach (string anim in anims)
            {
                if (!string.IsNullOrEmpty(anim.Trim()))
                    attackAnimList.Add(anim.Trim());
            }
        }
        if (attackAnimList.Count == 0)
            attackAnimList.Add("attack"); // 默认值

        if (data.ContainsKey("DefenseAnim") && !string.IsNullOrEmpty(data["DefenseAnim"]))
        {
            string[] anims = data["DefenseAnim"].Split('=');
            foreach (string anim in anims)
            {
                if (!string.IsNullOrEmpty(anim.Trim()))
                    defenseAnimList.Add(anim.Trim());
            }
        }
        if (defenseAnimList.Count == 0)
            defenseAnimList.Add("defense"); // 默认值

        if (data.ContainsKey("IdleAnim") && !string.IsNullOrEmpty(data["IdleAnim"]))
        {
            idleAnimName = data["IdleAnim"].Trim();
        }

        if (data.ContainsKey("HitAnim") && !string.IsNullOrEmpty(data["HitAnim"]))
        {
            hitAnimName = data["HitAnim"].Trim();
        }

        // 读取飞行动画配置
        if (data.ContainsKey("FlightAttackAnim") && !string.IsNullOrEmpty(data["FlightAttackAnim"]))
        {
            string[] anims = data["FlightAttackAnim"].Split('=');
            foreach (string anim in anims)
            {
                if (!string.IsNullOrEmpty(anim.Trim()))
                    flightAttackAnimList.Add(anim.Trim());
            }
        }
        if (flightAttackAnimList.Count == 0)
            flightAttackAnimList.AddRange(attackAnimList); // 默认使用普通攻击动画

        if (data.ContainsKey("FlightDefenseAnim") && !string.IsNullOrEmpty(data["FlightDefenseAnim"]))
        {
            string[] anims = data["FlightDefenseAnim"].Split('=');
            foreach (string anim in anims)
            {
                if (!string.IsNullOrEmpty(anim.Trim()))
                    flightDefenseAnimList.Add(anim.Trim());
            }
        }
        if (flightDefenseAnimList.Count == 0)
            flightDefenseAnimList.AddRange(defenseAnimList);

        if (data.ContainsKey("FlightIdleAnim") && !string.IsNullOrEmpty(data["FlightIdleAnim"]))
        {
            flightIdleAnimName = data["FlightIdleAnim"].Trim();
        }
        else
            flightIdleAnimName = idleAnimName;

        if (data.ContainsKey("FlightHitAnim") && !string.IsNullOrEmpty(data["FlightHitAnim"]))
        {
            flightHitAnimName = data["FlightHitAnim"].Trim();
        }
        else
            flightHitAnimName = hitAnimName;

        // 读取防御特效配置
        if (data.ContainsKey("DefenseEffect") && !string.IsNullOrEmpty(data["DefenseEffect"]))
        {
            defenseEffectPath = data["DefenseEffect"].Trim();
        }

        // 加载攻击特效配置
        LoadAttackEffectConfig();
    }

    /// <summary>
    /// 使用 EnemyData ScriptableObject 初始化敌人
    /// </summary>
    public void Init(EnemyData enemyData)
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
            { "AttackAnim", enemyData.attackAnim },
            { "DefenseAnim", enemyData.defenseAnim },
            { "IdleAnim", enemyData.idleAnim },
            { "HitAnim", enemyData.hitAnim },
            { "FlightAttackAnim", enemyData.flightAttackAnim },
            { "FlightDefenseAnim", enemyData.flightDefenseAnim },
            { "FlightIdleAnim", enemyData.flightIdleAnim },
            { "FlightHitAnim", enemyData.flightHitAnim },
            { "FlightThreshold", enemyData.flightThreshold.ToString() },
            { "DefenseEffect", enemyData.defenseEffectPath }
        };

        // 从配置读取动画名称（多个动画用 = 分隔）
        if (!string.IsNullOrEmpty(enemyData.attackAnim))
        {
            string[] anims = enemyData.attackAnim.Split('=');
            foreach (string anim in anims)
            {
                if (!string.IsNullOrEmpty(anim.Trim()))
                    attackAnimList.Add(anim.Trim());
            }
        }
        if (attackAnimList.Count == 0)
            attackAnimList.Add("attack");

        if (!string.IsNullOrEmpty(enemyData.defenseAnim))
        {
            string[] anims = enemyData.defenseAnim.Split('=');
            foreach (string anim in anims)
            {
                if (!string.IsNullOrEmpty(anim.Trim()))
                    defenseAnimList.Add(anim.Trim());
            }
        }
        if (defenseAnimList.Count == 0)
            defenseAnimList.Add("defense");

        idleAnimName = string.IsNullOrEmpty(enemyData.idleAnim) ? "idle" : enemyData.idleAnim;
        hitAnimName = string.IsNullOrEmpty(enemyData.hitAnim) ? "hit" : enemyData.hitAnim;

        // 飞行动画
        if (!string.IsNullOrEmpty(enemyData.flightAttackAnim))
        {
            string[] anims = enemyData.flightAttackAnim.Split('=');
            foreach (string anim in anims)
            {
                if (!string.IsNullOrEmpty(anim.Trim()))
                    flightAttackAnimList.Add(anim.Trim());
            }
        }
        if (flightAttackAnimList.Count == 0)
            flightAttackAnimList.AddRange(attackAnimList);

        if (!string.IsNullOrEmpty(enemyData.flightDefenseAnim))
        {
            string[] anims = enemyData.flightDefenseAnim.Split('=');
            foreach (string anim in anims)
            {
                if (!string.IsNullOrEmpty(anim.Trim()))
                    flightDefenseAnimList.Add(anim.Trim());
            }
        }
        if (flightDefenseAnimList.Count == 0)
            flightDefenseAnimList.AddRange(defenseAnimList);

        flightIdleAnimName = string.IsNullOrEmpty(enemyData.flightIdleAnim) ? idleAnimName : enemyData.flightIdleAnim;
        flightHitAnimName = string.IsNullOrEmpty(enemyData.flightHitAnim) ? hitAnimName : enemyData.flightHitAnim;

        // 防御特效
        defenseEffectPath = string.IsNullOrEmpty(enemyData.defenseEffectPath) ? "Effects/Magic circle 18" : enemyData.defenseEffectPath;

        // 回血特效
        healEffectPath = enemyData.healEffectPath;

        // 加载攻击特效配置
        LoadAttackEffectConfigFromSO();
    }

    // 从 ScriptableObject 加载攻击特效配置
    private void LoadAttackEffectConfigFromSO()
    {
        if (enemyDataSO == null)
        {
            LoadAttackEffectConfig(); // 回退到字典方式
            return;
        }

        string configPath = null;

        // 优先级1：按怪物ID加载
        configPath = $"Data_Enemy/EnemyAttackEffects/{enemyDataSO.id}";
        attackEffectConfig = Resources.Load<AttackEffectConfig>(configPath);
        if (attackEffectConfig != null)
        {
            LogAvailableEffects();
            return;
        }

        // 优先级2：按怪物名称加载
        configPath = $"Data_Enemy/EnemyAttackEffects/{enemyDataSO.enemyName}";
        attackEffectConfig = Resources.Load<AttackEffectConfig>(configPath);
        if (attackEffectConfig != null)
        {
            LogAvailableEffects();
            return;
        }

        // 优先级3：默认全局配置
        configPath = DEFAULT_ATTACK_EFFECT_CONFIG_PATH;
        attackEffectConfig = Resources.Load<AttackEffectConfig>(configPath);
        if (attackEffectConfig != null)
        {
            LogAvailableEffects();
            return;
        }
    }

    // 加载攻击特效配置（按优先级：怪物ID专属 > 怪物名称专属 > 默认全局 > 内置默认）
    private void LoadAttackEffectConfig()
    {
        string configPath = null;
        
        // 优先级1：按怪物ID加载专属配置（如 Data_Enemy/EnemyAttackEffects/10004_火龙）
        string enemyId = data.ContainsKey("Id") ? data["Id"] : "";
        if (!string.IsNullOrEmpty(enemyId))
        {
            configPath = $"Data_Enemy/EnemyAttackEffects/{enemyId}";
            attackEffectConfig = Resources.Load<AttackEffectConfig>(configPath);
            if (attackEffectConfig != null)
            {
                LogAvailableEffects();
                return;
            }
        }
        
        // 优先级2：按怪物名称加载专属配置（如 Data_Enemy/EnemyAttackEffects/火龙）
        string enemyName = data.ContainsKey("Name") ? data["Name"] : "";
        if (!string.IsNullOrEmpty(enemyName))
        {
            configPath = $"Data_Enemy/EnemyAttackEffects/{enemyName}";
            attackEffectConfig = Resources.Load<AttackEffectConfig>(configPath);
            if (attackEffectConfig != null)
            {
                LogAvailableEffects();
                return;
            }
        }
        
        // 优先级3：使用默认全局配置
        configPath = DEFAULT_ATTACK_EFFECT_CONFIG_PATH;
        attackEffectConfig = Resources.Load<AttackEffectConfig>(configPath);
        if (attackEffectConfig != null)
        {
            LogAvailableEffects();
            return;
        }
        
        // 优先级4：使用内置默认配置
        attackEffectConfig = CreateDefaultAttackEffectConfig();
    }
    
    // 打印可用特效列表
    private void LogAvailableEffects()
    {
        if (attackEffectConfig != null)
        {
            var names = attackEffectConfig.GetAllEffectNames();
        }
    }

    // 创建内置默认攻击特效配置
    private AttackEffectConfig CreateDefaultAttackEffectConfig()
    {
        AttackEffectConfig config = ScriptableObject.CreateInstance<AttackEffectConfig>();
        config.description = "内置默认攻击特效配置";
        
        // 添加默认特效
        config.attackEffects = new List<AttackEffectData>
        {
            new AttackEffectData {
                effectName = "Bite Attack",
                effectPrefabPath = "MagicEffect/Prefabs/Magic circle 12",
                spawnPositionType = 0,
                effectScale = 1.5f,
                delayTime = 0.3f,
                duration = 1f,
                rotationOffset = new Vector3(-90, 0, 0)
            },
            new AttackEffectData {
                effectName = "Breath Attack",
                effectPrefabPath = "MagicEffect/Prefabs/Magic circle 13",
                spawnPositionType = 2,
                effectScale = 2f,
                delayTime = 0.4f,
                duration = 1.5f
            },
            new AttackEffectData {
                effectName = "Head Attack",
                effectPrefabPath = "MagicEffect/Prefabs/Magic circle 14",
                spawnPositionType = 1,
                effectScale = 1.5f,
                delayTime = 0.3f,
                duration = 1f
            },
            new AttackEffectData {
                effectName = "ProjectileAttack",
                effectPrefabPath = "MagicEffect/Prefabs/Magic circle 15",
                spawnPositionType = 1,
                effectScale = 1f,
                delayTime = 0.2f,
                duration = 0.8f
            },
            new AttackEffectData {
                effectName = "FireBreathAttack",
                effectPrefabPath = "MagicEffect/Prefabs/Magic circle 18",
                spawnPositionType = 2,
                effectScale = 2.5f,
                delayTime = 0.5f,
                duration = 2f
            },
            new AttackEffectData {
                effectName = "CastSpell",
                effectPrefabPath = "MagicEffect/Prefabs/Magic circle 16",
                spawnPositionType = 0,
                effectScale = 2f,
                delayTime = 0.2f,
                duration = 1.5f,
                rotationOffset = new Vector3(-90, 0, 0)
            },
            new AttackEffectData {
                effectName = "attack",
                effectPrefabPath = "MagicEffect/Prefabs/Magic circle 12",
                spawnPositionType = 0,
                effectScale = 1f,
                delayTime = 0.3f,
                duration = 1f,
                rotationOffset = new Vector3(-90, 0, 0)
            }
        };
        
        return config;
    }

    // 获取当前模式下的攻击动画列表
    private List<string> GetCurrentAttackAnimList()
    {
        return isFlightMode ? flightAttackAnimList : attackAnimList;
    }

    // 获取当前模式下的防御动画列表
    private List<string> GetCurrentDefenseAnimList()
    {
        return isFlightMode ? flightDefenseAnimList : defenseAnimList;
    }

    // 获取当前模式下的待机动画
    private string GetCurrentIdleAnim()
    {
        return isFlightMode ? flightIdleAnimName : idleAnimName;
    }

    // 获取当前模式下的受击动画
    private string GetCurrentHitAnim()
    {
        return isFlightMode ? flightHitAnimName : hitAnimName;
    }

    // 检查并切换飞行模式
    private void CheckFlightMode()
    {
        if (isFlightMode) return; // 已经进入飞行模式

        // 获取飞行阈值（默认50%）
        float flightThreshold = 0.5f;
        if (data.ContainsKey("FlightThreshold") && !string.IsNullOrEmpty(data["FlightThreshold"]))
        {
            if (float.TryParse(data["FlightThreshold"], out float threshold))
            {
                flightThreshold = threshold;
            }
        }

        if (MaxHp > 0 && CurHp <= MaxHp * flightThreshold)
        {
            isFlightMode = true;
            
            // 设置 Animator 参数，控制状态切换（仅当参数存在时，不是所有敌人都有飞行状态）
            if (HasAnimatorParameter("isFlying"))
                ani.SetBool("isFlying", true);
        }
    }

    /// <summary>
    /// 检查 Animator 是否存在指定参数
    /// </summary>
    private bool HasAnimatorParameter(string paramName)
    {
        if (ani == null || ani.runtimeAnimatorController == null) return false;
        foreach (var param in ani.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }

    // 随机获取一个攻击动画
    private string GetRandomAttackAnim()
    {
        var animList = GetCurrentAttackAnimList();
        if (animList.Count == 0) return "attack";
        return animList[Random.Range(0, animList.Count)];
    }

    // 随机获取一个防御动画
    private string GetRandomDefenseAnim()
    {
        var animList = GetCurrentDefenseAnimList();
        if (animList.Count == 0) return "defense";
        return animList[Random.Range(0, animList.Count)];
    }

    void Start()
    {
        _meshRenderer = transform.GetComponentInChildren<SkinnedMeshRenderer>();
        ani = transform.GetComponent<Animator>();

        type = ActionType.None;
        turnsWithoutAttack = 0; // 初始化连续未攻击回合计数器
        hpItemObj = UIManager.Instance.CreateHpItem();
        actionObj = UIManager.Instance.CreateActionIcon();
        
        // 初始化敌人状态UI
        InitializeStatusUI();

        if (hpItemObj == null || actionObj == null)
        {
            Debug.LogError("敌人 UI 初始化失败");
            return;
        }

        attackTf = actionObj.transform.Find("attack");
        defendTf = actionObj.transform.Find("defend");
        healTf = actionObj.transform.Find("Heart");

        defendTxt = hpItemObj.transform.Find("fangyu/Text")?.GetComponent<TextMeshProUGUI>();
        hpTxt = hpItemObj.transform.Find("hpTxt")?.GetComponent<TextMeshProUGUI>();
        hpImg = hpItemObj.transform.Find("fill")?.GetComponent<Image>();

        if (defendTxt == null || hpTxt == null || hpImg == null)
        {
            Debug.LogError("敌人 UI 组件获取失败");
        }

        //设置血量和行动图标位置
        hpItemObj.transform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.down * 0.2f);
        
        // 行动图标放在血条右边（使用 localPosition 避免缩放问题）
        Vector3 hpLocalPos = hpItemObj.transform.localPosition;
        actionObj.transform.localPosition = new Vector3(hpLocalPos.x + 110f, hpLocalPos.y, hpLocalPos.z);

        SetRandomAction();

        //初始化数值（从 EnemyData 读取，加入随机波动）
        if (enemyDataSO != null)
        {
            Attack = RandomizeValue(enemyDataSO.attack, enemyDataSO.attackVariance);
            Defend = RandomizeValue(enemyDataSO.defense, enemyDataSO.defenseVariance);
            CurHp = enemyDataSO.maxHp;
            MaxHp = CurHp;
        }
        else
        {
            // 兼容旧字典方式
            if (!int.TryParse(data["Attack"], out Attack))
            {
                Debug.LogError($"敌人攻击力解析失败: {data["Attack"]}");
                Attack = 0;
            }
            if (!int.TryParse(data["Hp"], out CurHp))
            {
                Debug.LogError($"敌人生命值解析失败: {data["Hp"]}");
                CurHp = 100;
            }
            MaxHp = CurHp;
            if (!int.TryParse(data["Defend"], out Defend))
            {
                Debug.LogError($"敌人防御力解析失败: {data["Defend"]}");
                Defend = 0;
            }
        }
        isFlightMode = false; // 重置飞行模式
        // 检查动画参数是否存在，避免警告
        if (ani.parameterCount > 0)
        {
            foreach (AnimatorControllerParameter param in ani.parameters)
            {
                if (param.name == "isFlying")
                {
                    ani.SetBool("isFlying", false);
                    break;
                }
            }
        }

        UpdateHp();
        UpdateDefend();
    }
    
    /// <summary>
    /// 初始化敌人状态UI
    /// </summary>
    private void InitializeStatusUI()
    {
        if (hpItemObj == null) return;
        
        // 获取或添加EnemyStatusUI组件
        statusUI = hpItemObj.GetComponent<EnemyStatusUI>();
        if (statusUI == null)
        {
            statusUI = hpItemObj.AddComponent<EnemyStatusUI>();
        }
        
        // 设置容器（StatusContainer 是 hpTxt 的子物体）
        Transform container = hpItemObj.transform.Find("hpTxt/StatusContainer");
        if (container != null)
        {
            statusUI.iconContainer = container.GetComponent<RectTransform>();
            statusUI.buffIconPrefab = Resources.Load<GameObject>("UI/BuffIcon");
            statusUI.Show(true);
        }
        
        // 绑定到当前敌人
        statusUI.Initialize(this);
        
        // 同步当前状态
        statusUI.RefreshUI();
    }

    //随机一个行动
    public void SetRandomAction()
    {
        // 检查是否需要强制攻击（连续3回合未攻击）
        if (turnsWithoutAttack >= FORCE_ATTACK_THRESHOLD)
        {
            type = ActionType.Attack;
        }
        else
        {
            int ran = Random.Range(1, 4); // 1-3: Defend, Attack, Heal
            type = (ActionType)ran;
        }

        switch (type)
        {
            case ActionType.None:
                break;
            case ActionType.Defend:
                attackTf.gameObject.SetActive(false);
                defendTf.gameObject.SetActive(true);
                break;
            case ActionType.Attack:
                attackTf.gameObject.SetActive(true);
                defendTf.gameObject.SetActive(false);
                break;
            case ActionType.Heal:
                attackTf.gameObject.SetActive(false);
                defendTf.gameObject.SetActive(false);
                if (healTf != null) healTf.gameObject.SetActive(true);
                break;
        }
    }

    //更新血量信息
    public void UpdateHp()
    {
        if (hpTxt != null && hpImg != null)
        {
            hpTxt.text = CurHp + "/" + MaxHp;
            hpImg.fillAmount = (float)CurHp / (float)MaxHp;
        }

        // 每次血量变化都检查是否需要切换飞行模式
        CheckFlightMode();
    }

    //更新防御信息
    public void UpdateDefend()
    {
        if (defendTxt != null) // 确保组件未被销毁
            defendTxt.text = Defend.ToString();
    }

    //被攻击卡选中，显示描边
    public void OnSelect()
    {
        if (_meshRenderer == null)
        {
            Debug.LogWarning($"Enemy {name}: _meshRenderer is null");
            return;
        }
        
        int otlID = Shader.PropertyToID("_OtlColor");
        int emissionID = Shader.PropertyToID("_EmissionColor");
        
        // 遍历所有材质设置高亮
        foreach (Material mat in _meshRenderer.materials)
        {
            if (mat == null) continue;
            
    
            
            // 尝试设置 _OtlColor
            if (mat.HasProperty(otlID))
            {
                mat.SetColor(otlID, Color.red);

            }
            
            // 同时启用 emission 作为备选
            if (mat.HasProperty(emissionID))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor(emissionID, new Color(1f, 0f, 0f, 1f));

            }
        }
        

    }

    //未选中
    public void OnUnSelect()
    {
        if (_meshRenderer == null) return;
        
        foreach (Material mat in _meshRenderer.materials)
        {
            if (mat == null) continue;
            
            int propertyID = Shader.PropertyToID("_OtlColor");
            if (mat.HasProperty(propertyID))
            {
                mat.SetColor("_OtlColor", Color.black);
            }
            
            int emissionID = Shader.PropertyToID("_EmissionColor");
            if (mat.HasProperty(emissionID))
            {
                mat.SetColor(emissionID, Color.black);
            }
        }
    }

    //受伤
    public void Hit(int val)
    {
        // 应用易伤加成（有易伤则受伤 +25%，与层数无关）
        if (GetStatusStack(StatusType.Vulnerable) > 0)
        {
            val = Mathf.CeilToInt(val * 1.25f);
        }

        //先扣护盾
        if (Defend >= val)
        {
            //扣护盾
            Defend -= val;

            //播放受伤动画（根据当前模式）
            ani.Play(GetCurrentHitAnim(), 0, 0);
        }
        else
        {
            val = val - Defend;
            Defend = 0;
            CurHp -= val;
            if (CurHp <= 0)
            {
                CurHp = 0;
                StopAllCoroutines();
                // 遗物：击杀敌人钩子
                RelicManager.Instance.TriggerEnemyKilled(this);
                if (ani != null && ani.HasState(0, Animator.StringToHash("die")))
                    ani.Play("die");
                EnemyManager.Instance.DeleteEnemy(this);//从敌人列表移除
                Destroy(gameObject, 1);
                Destroy(actionObj);
                Destroy(hpItemObj);
            }
            else
            {
                //受伤
                ani.Play(GetCurrentHitAnim(), 0, 0);
                
                // 受击动画播放完后返回待机动画
                StartCoroutine(ReturnToIdleAfterHit());
            }
        }

        //刷新血量防御UI
        UpdateDefend();
        UpdateHp();
    }

    // 受击动画播放完后返回待机动画
    private IEnumerator ReturnToIdleAfterHit()
    {
        yield return new WaitForSeconds(0.5f);
        SafeCrossFade(GetCurrentIdleAnim(), 0f);
    }

    //隐藏攻击和防御图标
    public void HideAction()
    {
        if (attackTf != null) attackTf.gameObject.SetActive(false);
        if (defendTf != null) defendTf.gameObject.SetActive(false);
        if (healTf != null) healTf.gameObject.SetActive(false);
    }

    //执行当前行动
    public IEnumerator DoAction()
    {
        HideAction();

        //播放对应动画（使用 SafeCrossFade 避免状态不存在的错误）
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

        //等待动画播放
        yield return new WaitForSeconds(0.5f);

        switch (type)
        {
            case ActionType.None:
                break;

            case ActionType.Defend:
                //加防御
                Defend += 1;
                UpdateDefend();
                //播放防御特效
                PlayDefendEffect();
                break;

            case ActionType.Attack:
                //玩家扣血（虚弱减百分比 + 缩小减百分比 + 枷锁减固定值）
                int modifiedAttack = Attack;
                int weakStack = GetStatusStack(StatusType.Weak);
                if (weakStack > 0)
                    modifiedAttack = Mathf.CeilToInt(modifiedAttack * (1f - weakStack * 0.25f));
                int shrinkStack = GetStatusStack(StatusType.Shrink);
                if (shrinkStack > 0)
                    modifiedAttack = Mathf.CeilToInt(modifiedAttack * 0.7f);
                int fetterStack = GetStatusStack(StatusType.Fetter);
                modifiedAttack = Mathf.Max(0, modifiedAttack - fetterStack);
                FightManager.Instance.GetPlayerHit(modifiedAttack, this);

                //摄像机震动
                Camera.main.DOShakePosition(0.1f, 0.2f, 5, 45);

                break;

            case ActionType.Heal:
                //回血逻辑
                int healAmount = Mathf.CeilToInt(MaxHp * 0.1f); // 回复10%最大生命
                CurHp = Mathf.Min(CurHp + healAmount, MaxHp);
                UpdateHp();

                //播放回血特效
                PlayHealEffect();
                break;
        }

        //等待动画结束
        yield return new WaitForSeconds(1);

        //更新连续未攻击回合计数
        if (type == ActionType.Attack)
        {
            turnsWithoutAttack = 0;

        }
        else
        {
            turnsWithoutAttack++;

        }

        //返回待机状态
        SafeCrossFade(GetCurrentIdleAnim(), 0f);
    }

    //敌人被销毁时自动销毁其UI物体
    private void OnDestroy()
    {
        if (actionObj != null) Destroy(actionObj);
        if (hpItemObj != null) Destroy(hpItemObj);
    }

    #region 状态Buff管理（供EnemyStatusUI使用）

    // 敌人状态字典：状态类型 -> 层数
    private Dictionary<StatusType, int> statusDict = new Dictionary<StatusType, int>();

    /// <summary>
    /// 添加或增加状态Buff层数
    /// </summary>
    public void AddStatus(StatusType type, int stacks)
    {
        if (statusDict.ContainsKey(type))
        {
            statusDict[type] += stacks;
        }
        else
        {
            statusDict[type] = stacks;
        }
        // 通知UI更新
        OnStatusChanged?.Invoke(type, statusDict[type], true);
    }

    /// <summary>
    /// 移除状态Buff层数
    /// </summary>
    public void RemoveStatus(StatusType type, int stacks = -1)
    {
        if (!statusDict.ContainsKey(type)) return;

        if (stacks < 0 || stacks >= statusDict[type])
        {
            // 完全移除
            statusDict.Remove(type);
            OnStatusChanged?.Invoke(type, 0, false);
        }
        else
        {
            statusDict[type] -= stacks;
            OnStatusChanged?.Invoke(type, statusDict[type], true);
        }
    }

    /// <summary>
    /// 获取状态层数
    /// </summary>
    public int GetStatusStack(StatusType type)
    {
        return statusDict.ContainsKey(type) ? statusDict[type] : 0;
    }

    /// <summary>
    /// 是否有某个状态
    /// </summary>
    public bool HasStatus(StatusType type)
    {
        return statusDict.ContainsKey(type) && statusDict[type] > 0;
    }

    /// <summary>
    /// 清空所有状态
    /// </summary>
    public void ClearAllStatus()
    {
        statusDict.Clear();
        OnStatusChanged?.Invoke(StatusType.Weak, 0, false);
    }
    
    /// <summary>
    /// 获取所有有效状态（供EnemyStatusUI使用）
    /// </summary>
    public Dictionary<StatusType, int> GetAllStatus()
    {
        Dictionary<StatusType, int> validStatus = new Dictionary<StatusType, int>();
        foreach (var kvp in statusDict)
        {
            if (kvp.Value > 0)
                validStatus[kvp.Key] = kvp.Value;
        }
        return validStatus;
    }

    /// <summary>
    /// 状态变化事件（供EnemyStatusUI订阅）
    /// 参数：状态类型, 当前层数, 是否添加（false表示移除）
    /// </summary>
    public event System.Action<StatusType, int, bool> OnStatusChanged;

    #endregion

    //播放防御特效
    private void PlayDefendEffect()
    {

        GameObject effectPrefab = Resources.Load<GameObject>(defenseEffectPath);
        if (effectPrefab != null)
        {

            
            // 获取特效参数
            EffectParams effectParams = GetDefenseEffectParams();
            
            // 计算特效位置（脚下 + 偏移）
            Vector3 footPos = new Vector3(transform.position.x, 0f, transform.position.z);
            Vector3 spawnPos = footPos + effectParams.positionOffset;
            
            // 实例化特效，应用旋转和缩放
            Quaternion rotation = Quaternion.Euler(effectParams.rotationOffset);
            GameObject effect = Instantiate(effectPrefab, spawnPos, rotation);
            effect.transform.localScale *= effectParams.effectScale;
            
            // 持续时间
            float duration = effectParams.duration > 0 ? effectParams.duration : 2f;
            Destroy(effect, duration);

        }
        else
        {
            Debug.LogWarning($"防御特效未找到，请检查路径：{defenseEffectPath}");
        }
    }
    
    // 获取防御特效参数
    private EffectParams GetDefenseEffectParams()
    {
        if (enemyDataSO != null)
        {
            return enemyDataSO.defenseEffectParams;
        }
        return new EffectParams();
    }

    // 获取回血动画名称
    private string GetHealAnim()
    {
        // 优先使用 ScriptableObject 中的配置
        if (enemyDataSO != null && !string.IsNullOrEmpty(enemyDataSO.healAnim))
        {
            string[] anims = enemyDataSO.healAnim.Split('=');
            if (anims.Length > 0 && !string.IsNullOrEmpty(anims[0].Trim()))
            {
                return anims[0].Trim();
            }
        }
        
        // 回退：从 data 字典中获取
        if (data != null && data.ContainsKey("HealAnim") && !string.IsNullOrEmpty(data["HealAnim"]))
        {
            return data["HealAnim"].Trim();
        }
        
        return "heal"; // 默认回血动画名称
    }

    // 播放回血特效
    private void PlayHealEffect()
    {
        if (string.IsNullOrEmpty(healEffectPath))
        {
            Debug.LogWarning("回血特效路径未配置");
            return;
        }
        

        
        GameObject effectPrefab = Resources.Load<GameObject>(healEffectPath);
        if (effectPrefab != null)
        {

            
            // 获取特效参数
            EffectParams effectParams = GetHealEffectParams();
            
            // 计算特效位置（脚下 + 偏移）
            Vector3 footPos = new Vector3(transform.position.x, 0f, transform.position.z);
            Vector3 spawnPos = footPos + effectParams.positionOffset;
            
            // 实例化特效，应用旋转和缩放
            Quaternion rotation = Quaternion.Euler(effectParams.rotationOffset);
            GameObject effect = Instantiate(effectPrefab, spawnPos, rotation);
            effect.transform.localScale *= effectParams.effectScale;
            
            // 持续时间
            float duration = effectParams.duration > 0 ? effectParams.duration : 2f;
            Destroy(effect, duration);

        }
        else
        {
            Debug.LogWarning($"回血特效未找到，请检查路径：{healEffectPath}");
        }
    }
    
    // 获取回血特效参数
    private EffectParams GetHealEffectParams()
    {
        if (enemyDataSO != null)
        {
            return enemyDataSO.healEffectParams;
        }
        return new EffectParams();
    }

    //播放攻击特效
    private void PlayAttackEffect(string attackAnimName)
    {
        if (attackEffectConfig == null || string.IsNullOrEmpty(attackAnimName))
        {
            Debug.LogWarning("攻击特效配置未加载或动画名称为空");
            return;
        }

        AttackEffectData effectData = attackEffectConfig.GetEffectByName(attackAnimName);
        if (effectData == null)
        {
            // 如果找不到精确匹配，尝试遍历配置查找包含动画名的特效
            foreach (var effectItem in attackEffectConfig.attackEffects)
            {
                if (attackAnimName.Contains(effectItem.effectName) || effectItem.effectName.Contains(attackAnimName))
                {
                    effectData = effectItem;
                    break;
                }
            }
            
            if (effectData == null)
            {
                Debug.LogWarning($"未找到攻击特效配置: {attackAnimName}，可用配置: {string.Join(", ", attackEffectConfig.GetAllEffectNames())}");
                return;
            }
        }

        GameObject effectPrefab = Resources.Load<GameObject>(effectData.effectPrefabPath);
        if (effectPrefab == null)
        {
            Debug.LogWarning($"攻击特效预制体未找到: {effectData.effectPrefabPath}");
            return;
        }

        // 计算特效生成位置
        Vector3 spawnPos = CalculateEffectPosition(effectData.spawnPositionType, effectData.positionOffset, effectData.spawnTransformPath);

        // 延迟播放特效
        if (effectData.delayTime > 0)
        {
            StartCoroutine(DelayedSpawnEffect(effectPrefab, spawnPos, effectData));
        }
        else
        {
            SpawnEffect(effectPrefab, spawnPos, effectData);
        }
    }

    // 根据生成位置类型计算特效位置
    private Vector3 CalculateEffectPosition(int positionType, Vector3 offset, string transformPath = null)
    {
        Vector3 basePos = transform.position;
        
        switch (positionType)
        {
            case 0: // 敌人脚下
                return new Vector3(basePos.x + offset.x, 0f + offset.y, basePos.z + offset.z);
            case 1: // 敌人中心
                return basePos + offset;
            case 2: // 敌人头顶
                return new Vector3(basePos.x + offset.x, basePos.y + 1f + offset.y, basePos.z + offset.z);
            case 3: // 摄像机脚下（与防御特效位置一致）
                if (Camera.main != null)
                {
                    Vector3 pos = Camera.main.transform.position;
                    pos.y = 0; // 地面位置
                    return pos + offset;
                }
                else
                {
                    return basePos + offset;
                }
            case 4: // 指定子物体
                if (!string.IsNullOrEmpty(transformPath))
                {
                    Transform targetTf = transform.Find(transformPath);
                    if (targetTf != null)
                    {
                        return targetTf.position + offset;
                    }
                    else
                    {
                        Debug.LogWarning($"找不到指定子物体: {transformPath}，使用敌人中心位置");
                        return basePos + offset;
                    }
                }
                else
                {
                    Debug.LogWarning("spawnPositionType=4 但未指定 spawnTransformPath");
                    return basePos + offset;
                }
            default:
                return basePos + offset;
        }
    }

    // 延迟生成特效
    private IEnumerator DelayedSpawnEffect(GameObject effectPrefab, Vector3 spawnPos, AttackEffectData effectData)
    {
        yield return new WaitForSeconds(effectData.delayTime);
        SpawnEffect(effectPrefab, spawnPos, effectData);
    }

    // 生成并配置特效
    private void SpawnEffect(GameObject effectPrefab, Vector3 spawnPos, AttackEffectData effectData)
    {
        if (effectPrefab == null) return;

        // 创建特效
        GameObject effectObj = Instantiate(effectPrefab, spawnPos, Quaternion.Euler(effectData.rotationOffset));
        
        // 设置缩放
        effectObj.transform.localScale *= effectData.effectScale;
        
        // 设置持续时间
        float duration = effectData.duration > 0 ? effectData.duration : 2f;
        Destroy(effectObj, duration);
    }



    /// <summary>
    /// 安全播放动画（动画状态不存在时回退到 idle）
    /// </summary>
    private void SafeCrossFade(string animName, float fadeTime)
    {
        if (ani == null) return;
        if (ani.HasState(0, Animator.StringToHash(animName)))
        {
            ani.CrossFade(animName, fadeTime);
        }
    }

    /// <summary>
    /// 根据基础值和波动比例生成随机值
    /// </summary>
    /// <param name="baseValue">基础值</param>
    /// <param name="variance">波动比例（0~1），如 0.2 表示 ±20%</param>
    /// <returns>随机后的值</returns>
    private int RandomizeValue(int baseValue, float variance)
    {
        if (variance <= 0f || baseValue <= 0)
            return baseValue;

        float ratio = Random.Range(1f - variance, 1f + variance);
        return Mathf.Max(1, Mathf.RoundToInt(baseValue * ratio));
    }
}