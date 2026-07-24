using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.EventSystems;

// 连锁闪电 - 群体法术攻击，对所有敌人造成伤害
// 拖拽方式与烈焰风暴相同（向上拖拽使用）
public class ChainLightningCard : SpellAttackCard
{
    // ========== AOE卡牌通用逻辑 ==========

    private Vector2 aoeDragStartPos;
    private Vector2 aoeInitPos;
    private bool aoeHasDraggedFar = false;
    private const float AOE_DRAG_THRESHOLD_PERCENT = 0.25f;

    // ========== 覆盖攻击牌的拖拽方式，使用普通卡牌拖拽 ==========

    public override void OnPointerDown(PointerEventData eventData)
    {
        // 右键取消
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (useState == CardUseState.Dragging || useState == CardUseState.Using)
            {
                CancelUsing();
                HideAOEDamagePreview();
            }
            return;
        }

        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (useState != CardUseState.None) return;

        // AOE卡牌不显示LineUI，直接进入使用状态
        StartUsing();
        transform.DOScale(1.2f, 0.2f);
        transform.SetAsLastSibling();
        SetHighlight(true);
        AudioManager.Instance.PlayEffect("Cards/draw");
    }

    /// <summary>
    /// 隐藏AOE伤害预览
    /// </summary>
    private void HideAOEDamagePreview()
    {
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
            fightUI.HideDamagePreview();
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        // AOE卡牌使用普通卡牌的拖拽方式
        StartUsing();
        aoeInitPos = GetComponent<RectTransform>().anchoredPosition;
        aoeDragStartPos = eventData.position;
        aoeHasDraggedFar = false;
    }

    public override void OnDrag(PointerEventData eventData)
    {
        // 右键取消
        if (Input.GetMouseButton(1))
        {
            CancelUsing();
            return;
        }

        if (useState == CardUseState.Cancelled) return;

        // 检测向上拖拽距离
        float verticalDrag = eventData.position.y - aoeDragStartPos.y;
        float threshold = Screen.height * AOE_DRAG_THRESHOLD_PERCENT;

        if (verticalDrag > threshold)
        {
            aoeHasDraggedFar = true;
        }
        else if (verticalDrag < threshold * 0.5f)
        {
            aoeHasDraggedFar = false;
        }

        // 移动卡牌
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out pos))
        {
            GetComponent<RectTransform>().anchoredPosition = pos;
        }

        // 显示AOE伤害预览
        UpdateAOEDamagePreview();
    }

    /// <summary>
    /// 更新AOE伤害预览
    /// </summary>
    private void UpdateAOEDamagePreview()
    {
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI == null) return;

        // 计算总伤害：基础伤害，复制药水翻倍
        int baseDamage = GetAttackDamage();
        if (BuffManager.Instance.HasStatus(StatusType.Duplicate))
            baseDamage *= 2;

        // 获取所有存活的敌人
        List<Enemy> aliveEnemies = GetAliveEnemies();
        if (aliveEnemies.Count == 0)
        {
            fightUI.HideDamagePreview();
            return;
        }

        // 获取鼠标位置的敌人
        Vector3 previewPos;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10000, LayerMask.GetMask("Enemy")))
        {
            previewPos = hit.transform.position;
        }
        else
        {
            // 没有指向敌人，使用第一个敌人位置
            previewPos = aliveEnemies[0].transform.position;
        }

        fightUI.ShowDamagePreview(baseDamage, previewPos);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        // 隐藏伤害预览
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI != null)
            fightUI.HideDamagePreview();

        // 已取消
        if (useState == CardUseState.Cancelled)
        {
            useState = CardUseState.None;
            return;
        }

        // 未拖拽超过阈值，取消使用
        if (!aoeHasDraggedFar)
        {
            CancelUsing();
            return;
        }

        // 拖拽超过阈值，使用卡牌
        if (TryUse())
        {
            OnCardUsed();
            if (BuffManager.Instance.HasStatus(StatusType.Duplicate))
            {
                BuffManager.Instance.RemoveStatus(StatusType.Duplicate, 1);
                UIManager.Instance.ShowTip("复制!", Color.magenta);
                OnCardUsed();
            }
        }
        else
        {
            OnCardUseFailed();
        }
    }

    // ========== AOE卡牌伤害逻辑 ==========

    protected override void OnCardUsed()
    {
        base.OnCardUsed();

        string effectPath = GetAttackEffectPath();

        // 消耗所有电亲和度
        int lightningAffinity = BuffManager.Instance.GetStack(StatusType.LightningAffinity);
        if (lightningAffinity > 0)
        {
            BuffManager.Instance.RemoveStatus(StatusType.LightningAffinity, lightningAffinity);
            UIManager.Instance.ShowTip($"消耗 {lightningAffinity} 层电亲和度", Color.yellow);
        }

        // 基础伤害8点
        int baseDamage = GetAttackDamage();

        // 对所有存活敌人造成伤害
        StartCoroutine(DealDamageToAllEnemies(baseDamage, effectPath));

        // 每消耗N层抽一张牌（基础2层，升级1层）
        if (lightningAffinity > 0)
        {
            float divisor = IsUpgraded() ? 1f : 2f;
            int drawCount = Mathf.CeilToInt(lightningAffinity / divisor);
            UIManager.Instance.ShowTip($"抽 {drawCount} 张牌", Color.cyan);
            FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
            fightUI?.CreateCardItem(drawCount);
            fightUI?.UpdateCardItemPos();
        }
    }

    /// <summary>
    /// 对所有敌人造成伤害（协程）
    /// </summary>
    protected IEnumerator DealDamageToAllEnemies(int damage, string effectPath)
    {
        List<Enemy> aliveEnemies = GetAliveEnemies();
        if (aliveEnemies.Count == 0) yield break;

        AudioManager.Instance.PlayEffect("Effect/sword");

        foreach (Enemy enemy in aliveEnemies)
        {
            if (enemy != null && enemy.gameObject != null)
            {
                if (!string.IsNullOrEmpty(effectPath))
                {
                    PlayEffect(enemy.transform.position, effectPath);
                }
                enemy.Hit(damage);
                BuffManager.Instance.OnDealDamage(damage);
                yield return new WaitForSeconds(0.15f);
            }
        }
    }

    /// <summary>
    /// 获取所有存活的敌人列表（AOE卡牌通用）
    /// </summary>
    protected List<Enemy> GetAliveEnemies()
    {
        List<Enemy> aliveEnemies = new List<Enemy>();
        Enemy[] allEnemies = Object.FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in allEnemies)
        {
            if (enemy != null && enemy.gameObject != null && enemy.gameObject.activeInHierarchy)
            {
                aliveEnemies.Add(enemy);
            }
        }
        return aliveEnemies;
    }
}
