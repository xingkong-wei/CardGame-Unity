using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

// 攻击卡 - 点击选中、划线瞄准、左键攻击
public class AttackCardItem : CardItem
{
    private bool isAttackSelected = false;
    private Vector2 originalPos;
    private int index;

    // 选中时向上偏移的距离
    private const float SELECT_OFFSET_Y = 360f;

    /// <summary>
    /// 获取攻击伤害值，子类可重写实现动态伤害
    /// </summary>
    protected virtual int GetAttackDamage()
    {
        if (data == null) return 0;

        int baseValue = 0;
        switch (data.damageSource)
        {
            case DamageSourceType.Fixed:
                baseValue = GetArg0();
                break;
            case DamageSourceType.Defense:
                baseValue = FightManager.Instance.DefenseCount;
                break;
            case DamageSourceType.CurrentHp:
                baseValue = FightManager.Instance.CurHp;
                break;
            case DamageSourceType.Coin:
                baseValue = FightManager.Instance.CoinAmount;
                break;
            default:
                baseValue = GetArg0();
                break;
        }

        // 应用百分比
        int damage = Mathf.FloorToInt(baseValue * data.damagePercent);

        // 应用Buff修改（力量/虚弱），但魔杖充能不能在这里处理
        damage = BuffManager.Instance.ModifyAttackDamage(damage, false);

        return damage;
    }

    /// <summary>
    /// 获取实际攻击伤害（包含魔杖充能，会消耗Buff）
    /// </summary>
    public int GetFinalAttackDamage()
    {
        int damage = GetAttackDamage();
        // 魔杖充能：双倍伤害（消耗1层）- 单独处理
        int wandCharging = BuffManager.Instance.GetStack(StatusType.WandCharging);
        if (wandCharging > 0)
        {
            damage *= 2;
            BuffManager.Instance.RemoveStatus(StatusType.WandCharging, 1);
        }
        // 超巨化：3倍伤害（消耗1层，可跨回合保留）
        if (BuffManager.Instance.HasStatus(StatusType.GiantGrowth))
        {
            damage *= 3;
            BuffManager.Instance.RemoveStatus(StatusType.GiantGrowth, 1);
        }
        // 恐惧：攻击后减1层（伤害-6已在 ModifyAttackDamage 中通过回调处理）
        if (BuffManager.Instance.HasStatus(StatusType.Fear))
        {
            BuffManager.Instance.RemoveStatus(StatusType.Fear, 1);
        }
        return damage;
    }

    /// <summary>
    /// 获取预览伤害（包含魔杖充能预览，但不消耗Buff）
    /// </summary>
    public int GetPreviewDamage()
    {
        int damage = GetAttackDamage();
        // 魔杖充能预览：双倍伤害（不消耗Buff）
        int wandCharging = BuffManager.Instance.GetStack(StatusType.WandCharging);
        if (wandCharging > 0)
        {
            damage *= 2;
        }
        // 超巨化预览：3倍伤害（不消耗Buff）
        if (BuffManager.Instance.HasStatus(StatusType.GiantGrowth))
        {
            damage *= 3;
        }
        // 遗物伤害倍率预览
        float relicMultiplier = RelicManager.Instance.GetDamagePreviewMultiplier();
        if (relicMultiplier > 1f)
            damage = Mathf.CeilToInt(damage * relicMultiplier);
        return damage;
    }

    /// <summary>
    /// 获取攻击次数，子类可重写实现多段攻击（默认1次）
    /// </summary>
    protected virtual int GetAttackTimes()
    {
        return 1;
    }

    /// <summary>
    /// 获取攻击特效路径，子类可重写实现自定义特效
    /// </summary>
    protected virtual string GetAttackEffectPath()
    {
        if (data == null) return null;
        return data.effects;
    }

    /// <summary>
    /// 复制药水预览：第二段攻击相比第一段的额外伤害（子类按OnCardUsed效果重写）
    /// </summary>
    public virtual int GetDuplicateSecondHitBonus()
    {
        return 0;
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        // 攻击卡不参与基类的拖拽逻辑
    }

    public override void OnDrag(PointerEventData eventData)
    {
        // 攻击卡不参与基类的拖拽逻辑
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        // 攻击卡不参与基类的拖拽逻辑
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (isAttackSelected)
            {
                CancelAttackSelect();
                return;
            }
        }

        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (isAttackSelected) return;

        // 选中攻击卡
        isAttackSelected = true;
        useState = CardUseState.Dragging;
        AudioManager.Instance.PlayEffect("Cards/draw");

        originalPos = GetComponent<RectTransform>().anchoredPosition;
        index = transform.GetSiblingIndex();

        // 突出显示
        Vector2 selectedPos = originalPos + new Vector2(0, SELECT_OFFSET_Y);
        GetComponent<RectTransform>().DOAnchorPos(selectedPos, 0.2f);
        transform.DOScale(1.2f, 0.2f);
        transform.SetAsLastSibling();
        SetHighlight(true);

        // 显示曲线界面
        UIManager.Instance.ShowUI<LineUI>("LineUI");
        UIManager.Instance.GetUI<LineUI>("LineUI").SetStartPos(GetComponent<RectTransform>().anchoredPosition);

        Cursor.visible = false;
        StopAllCoroutines();
        StartCoroutine(OnMouseDragging(eventData));
    }

    private void CancelAttackSelect()
    {
        isAttackSelected = false;
        useState = CardUseState.None;

        GetComponent<RectTransform>().anchoredPosition = originalPos;
        transform.localScale = Vector3.one;
        transform.SetSiblingIndex(index);
        SetHighlight(false);

        UIManager.Instance.CloseUI("LineUI");
        Cursor.visible = true;
        StopAllCoroutines();

        // 隐藏伤害预览
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
            fightUI.HideDamagePreview();
    }

    private IEnumerator OnMouseDragging(PointerEventData pData)
    {
        while (true)
        {
            if (Input.GetMouseButton(1))
            {
                CancelAttackSelect();
                yield break;
            }

            Vector2 pos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform.parent.GetComponent<RectTransform>(),
                pData.position,
                pData.pressEventCamera,
                out pos))
            {
                UIManager.Instance.GetUI<LineUI>("LineUI").SetEndPos(pos);
                CheckRayToEnemy();
            }

            yield return null;
        }
    }

    private Enemy hitEnemy;

    private void CheckRayToEnemy()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 10000, LayerMask.GetMask("Enemy")))
        {
            hitEnemy = hit.transform.GetComponent<Enemy>();
            if (hitEnemy == null)
            {
                Debug.LogWarning("Raycast hit something but no Enemy component found!");
                return;
            }


            hitEnemy.OnSelect();

            // 显示伤害预览（支持多段攻击格式，包含魔杖充能 + 敌人易伤）
            FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
            if (fightUI != null)
            {
                int d1 = GetPreviewDamage();
                if (hitEnemy.GetStatusStack(StatusType.Vulnerable) > 0)
                    d1 = Mathf.CeilToInt(d1 * 1.25f);
                int attackTimes = GetAttackTimes();
                if (BuffManager.Instance.HasStatus(StatusType.Duplicate))
                {
                    int d2 = d1 + GetDuplicateSecondHitBonus();
                    if (attackTimes > 1)
                        fightUI.ShowDamagePreview(d1 + d2, hitEnemy.transform.position, attackTimes);
                    else
                        fightUI.ShowDamagePreview(d1 + d2, hitEnemy.transform.position, 1);
                }
                else
                {
                    fightUI.ShowDamagePreview(d1, hitEnemy.transform.position, attackTimes);
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                StopAllCoroutines();
                Cursor.visible = true;
                UIManager.Instance.CloseUI("LineUI");
                if (fightUI != null)
                    fightUI.HideDamagePreview();

                if (TryUse())
                {
                    isAttackSelected = false;
                    RelicManager.Instance.TriggerCardPlayed(this);

                    int val = GetFinalAttackDamage();
                    int times = GetAttackTimes();
                    string effectPath = GetAttackEffectPath();

                    // 先造成伤害，再触发OnCardUsed
                    if (hitEnemy != null)
                        StartCoroutine(PerformMultiHit(hitEnemy, val, times, effectPath));
                    OnCardUsed();

                    // 复制药水：用最新BUFF重算伤害再打一次
                    if (BuffManager.Instance.HasStatus(StatusType.Duplicate))
                    {
                        BuffManager.Instance.RemoveStatus(StatusType.Duplicate, 1);
                        UIManager.Instance.ShowTip("复制!", Color.magenta);
                        int val2 = GetFinalAttackDamage();
                        if (hitEnemy != null)
                            StartCoroutine(PerformMultiHit(hitEnemy, val2, times, effectPath));
                        OnCardUsed();
                    }
                }
                else
                {
                    // 使用失败（眩晕限制等），取消攻击选中状态
                    CancelAttackSelect();
                }

                hitEnemy.OnUnSelect();
                hitEnemy = null;
            }
        }
        else
        {
            if (hitEnemy != null)
            {
                hitEnemy.OnUnSelect();
                hitEnemy = null;
                
                // 隐藏伤害预览
                FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
                if (fightUI != null)
                    fightUI.HideDamagePreview();
            }
        }
    }

    protected override void OnCardUsed()
    {
        base.OnCardUsed();
    }

    /// <summary>
    /// 执行多段攻击协程
    /// </summary>
    private System.Collections.IEnumerator PerformMultiHit(Enemy target, int damage, int times, string effectPath)
    {
        // 播放攻击音效（只播放一次）
        AudioManager.Instance.PlayEffect("Effect/sword");

        for (int i = 0; i < times; i++)
        {
            if (!string.IsNullOrEmpty(effectPath))
            {
                PlayEffect(target.transform.position, effectPath);
            }
            int modifiedDamage = RelicManager.Instance.TriggerDealDamage(damage);
            target.Hit(modifiedDamage);
            // 触发攻击后Buff效果（吸血）
            BuffManager.Instance.OnDealDamage(modifiedDamage);

            // 如果不是最后一次攻击，等待一下再执行下一次
            if (i < times - 1)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    /// <summary>
    /// 播放指定路径的攻击特效
    /// </summary>
    protected void PlayEffect(Vector3 pos, string effectPath)
    {
        if (string.IsNullOrEmpty(effectPath)) return;
        GameObject effectObj = Resources.Load(effectPath) as GameObject;
        if (effectObj != null)
        {
            GameObject instance = Instantiate(effectObj);
            instance.transform.position = pos;
            Destroy(instance, 2);
        }
    }
}
