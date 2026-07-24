using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

// 末日审判 - 消耗所有元素亲和度，每层额外+2伤害
public class JudgmentCard : AttackCardItem
{
    private bool isAttackSelected = false;
    private Vector2 originalPos;
    private int index;
    private const float SELECT_OFFSET_Y = 360f;

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
            if (hitEnemy == null) return;

            hitEnemy.OnSelect();

            // 显示伤害预览
            FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
            if (fightUI != null)
            {
                int previewDamage = CalculateTotalDamage();
                fightUI.ShowDamagePreview(previewDamage, hitEnemy.transform.position);
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

                    int totalDamage = CalculateTotalDamage();
                    string effectPath = GetAttackEffectPath();

                    // 先造成伤害，再触发OnCardUsed
                    if (hitEnemy != null)
                        StartCoroutine(PerformHit(hitEnemy, totalDamage, effectPath));
                    OnCardUsed();

                    // 复制药水：用最新BUFF重算伤害
                    if (BuffManager.Instance.HasStatus(StatusType.Duplicate))
                    {
                        BuffManager.Instance.RemoveStatus(StatusType.Duplicate, 1);
                        UIManager.Instance.ShowTip("复制!", Color.magenta);
                        int totalDamage2 = CalculateTotalDamage();
                        if (hitEnemy != null)
                            StartCoroutine(PerformHit(hitEnemy, totalDamage2, effectPath));
                        OnCardUsed();
                    }
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
    /// 获取所有元素亲和度总层数
    /// </summary>
    private int GetTotalElementAffinity()
    {
        int fireAffinity = BuffManager.Instance.GetStack(StatusType.FireAffinity);
        int iceAffinity = BuffManager.Instance.GetStack(StatusType.IceAffinity);
        int lightningAffinity = BuffManager.Instance.GetStack(StatusType.LightningAffinity);
        return fireAffinity + iceAffinity + lightningAffinity;
    }

    /// <summary>
    /// 计算总伤害（基础伤害 + 火亲和度加成 + 元素消耗加成）
    /// </summary>
    private int CalculateTotalDamage()
    {
        // 基础伤害
        int baseDamage = data != null ? GetArg0() : 20;

        // 包含火亲和度加成的法术伤害
        int spellDamage = BuffManager.Instance.ModifySpellDamage(baseDamage);

        // 元素消耗加成：每层加成（基础+2，升级+3）
        int totalAffinity = GetTotalElementAffinity();
        int bonusPerAffinity = IsUpgraded() ? 3 : 2;
        int elementBonus = totalAffinity * bonusPerAffinity;

        return spellDamage + elementBonus;
    }

    /// <summary>
    /// 执行单体攻击
    /// </summary>
    private IEnumerator PerformHit(Enemy target, int damage, string effectPath)
    {
        // 播放攻击音效
        AudioManager.Instance.PlayEffect("Effect/sword");

        // 消耗所有元素亲和度
        int fireAffinity = BuffManager.Instance.GetStack(StatusType.FireAffinity);
        int iceAffinity = BuffManager.Instance.GetStack(StatusType.IceAffinity);
        int lightningAffinity = BuffManager.Instance.GetStack(StatusType.LightningAffinity);
        int totalAffinity = fireAffinity + iceAffinity + lightningAffinity;

        if (totalAffinity > 0)
        {
            if (fireAffinity > 0)
                BuffManager.Instance.RemoveStatus(StatusType.FireAffinity, fireAffinity);
            if (iceAffinity > 0)
                BuffManager.Instance.RemoveStatus(StatusType.IceAffinity, iceAffinity);
            if (lightningAffinity > 0)
                BuffManager.Instance.RemoveStatus(StatusType.LightningAffinity, lightningAffinity);

            // 显示消耗提示
            string tip = $"消耗全部亲和度";
            UIManager.Instance.ShowTip(tip, Color.yellow);
        }

        // 播放特效
        if (!string.IsNullOrEmpty(effectPath))
        {
            PlayEffect(target.transform.position, effectPath);
        }

        // 造成伤害
        target.Hit(damage);
        BuffManager.Instance.OnDealDamage(damage);

        yield return null;
    }

    /// <summary>
    /// 播放指定路径的攻击特效
    /// </summary>
    protected new void PlayEffect(Vector3 pos, string effectPath)
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
