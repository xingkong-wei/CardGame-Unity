using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// ============================================================
// Enemy 视觉/UI/特效模块 (partial class)
// 负责：UI 更新（血量、护盾）、选中高亮、行动图标、
//       攻击/防御/治疗特效、特效配置加载
// ============================================================
public partial class Enemy
{
    #region UI 引用

    public GameObject hpItemObj;
    public GameObject actionObj;
    public Transform attackTf;
    public Transform defendTf;
    public Transform healTf;
    public TextMeshProUGUI defendTxt;
    public TextMeshProUGUI hpTxt;
    public Image hpImg;

    #endregion

    #region UI 更新

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

    #endregion

    #region 选中/未选中高亮

    protected SkinnedMeshRenderer _meshRenderer;

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

    #endregion

    #region 特效配置

    protected string defenseEffectPath = "Effects/MagicEffect/Prefabs/Magic circle 18";
    protected string healEffectPath;
    protected AttackEffectConfig attackEffectConfig;
    protected string currentAttackAnimName;
    private const string DEFAULT_ATTACK_EFFECT_CONFIG_PATH = "Data_Enemy/MonsterAttackEffects";

    private void LoadAttackEffectConfigFromSO()
    {
        if (enemyDataSO == null) { LoadAttackEffectConfig(); return; }
        attackEffectConfig = ResourceCache.Get<AttackEffectConfig>($"Data_Enemy/EnemyAttackEffects/{enemyDataSO.id}")
            ?? ResourceCache.Get<AttackEffectConfig>($"Data_Enemy/EnemyAttackEffects/{enemyDataSO.enemyName}")
            ?? ResourceCache.Get<AttackEffectConfig>(DEFAULT_ATTACK_EFFECT_CONFIG_PATH);
    }

    private void LoadAttackEffectConfig()
    {
        string id = data.TryGetValue("Id", out string i) ? i : "";
        string name = data.TryGetValue("Name", out string n) ? n : "";
        attackEffectConfig = ResourceCache.Get<AttackEffectConfig>($"Data_Enemy/EnemyAttackEffects/{id}")
            ?? ResourceCache.Get<AttackEffectConfig>($"Data_Enemy/EnemyAttackEffects/{name}")
            ?? ResourceCache.Get<AttackEffectConfig>(DEFAULT_ATTACK_EFFECT_CONFIG_PATH);
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

    #endregion

    #region 特效播放

    protected void PlayDefendEffect()
    {
        GameObject prefab = ResourceCache.Get<GameObject>(defenseEffectPath);
        if (prefab == null) return;
        EffectParams p = enemyDataSO != null ? enemyDataSO.defenseEffectParams : new EffectParams();
        SpawnGroundEffect(prefab, p);
    }

    protected void PlayHealEffect()
    {
        if (string.IsNullOrEmpty(healEffectPath)) return;
        GameObject prefab = ResourceCache.Get<GameObject>(healEffectPath);
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

        GameObject prefab = ResourceCache.Get<GameObject>(effectData.effectPrefabPath);
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

    protected System.Collections.IEnumerator DelayedSpawnEffect(GameObject prefab, Vector3 pos, AttackEffectData data)
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

    #endregion
}
