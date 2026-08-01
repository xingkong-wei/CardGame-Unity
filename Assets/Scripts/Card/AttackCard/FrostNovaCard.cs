using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// 冰霜新星 - AOE攻击+给敌人挂debuff+获得元素亲和度
/// 对所有敌人造成10点伤害，给予1层虚弱，获得3点冰亲和度
/// </summary>
public class FrostNovaCard : CardItem
{
    private const float AOE_DRAG_THRESHOLD_PERCENT = 0.25f;

    private Vector2 aoeDragStartPos;
    private Vector2 aoeInitPos;
    private bool aoeHasDraggedFar = false;

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

        StartUsing();
        transform.DOScale(1.2f, 0.2f);
        transform.SetAsLastSibling();
        SetHighlight(true);
        AudioManager.Instance.PlayEffect("Cards/draw");
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        StartUsing();
        aoeInitPos = GetComponent<RectTransform>().anchoredPosition;
        aoeDragStartPos = eventData.position;
        aoeHasDraggedFar = false;
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (Input.GetMouseButton(1))
        {
            CancelUsing();
            HideAOEDamagePreview();
            return;
        }

        if (useState == CardUseState.Cancelled) return;

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

        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out pos))
        {
            GetComponent<RectTransform>().anchoredPosition = pos;
        }

        // 显示伤害预览
        UpdateAOEDamagePreview();
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        // 隐藏伤害预览
        HideAOEDamagePreview();

        if (useState == CardUseState.Cancelled)
        {
            useState = CardUseState.None;
            return;
        }

        if (!aoeHasDraggedFar)
        {
            CancelUsing();
            return;
        }

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

    protected override void CancelUsing()
    {
        // 终止所有动画，防止动画覆盖恢复后的位置/缩放
        transform.DOKill();

        base.CancelUsing();
        SetHighlight(false);
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

    /// <summary>
    /// 更新AOE伤害预览
    /// </summary>
    private void UpdateAOEDamagePreview()
    {
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        if (fightUI == null) return;

        int damage = data != null ? GetArg0() : 10;
        if (BuffManager.Instance.HasStatus(StatusType.Duplicate))
            damage *= 2;

        List<Enemy> aliveEnemies = GetAliveEnemies();
        if (aliveEnemies.Count == 0)
        {
            fightUI.HideDamagePreview();
            return;
        }

        Vector3 previewPos;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10000, LayerMask.GetMask("Enemy")))
        {
            previewPos = hit.transform.position;
        }
        else
        {
            previewPos = aliveEnemies[0].transform.position;
        }

        fightUI.ShowDamagePreview(damage, previewPos);
    }

    protected override void OnCardUsed()
    {
        // 提前接管复制
        bool grabbedDup = false;
        if (BuffManager.Instance.HasStatus(StatusType.Duplicate))
        {
            grabbedDup = true;
            BuffManager.Instance.RemoveStatus(StatusType.Duplicate, 1);
        }

        base.OnCardUsed();

        List<Enemy> enemies = GetAliveEnemies();
        if (enemies.Count == 0) return;

        int baseDamage = data != null ? GetArg0() : 10;
        int damage = BuffManager.Instance.ModifyAttackDamage(baseDamage);
        if (IsSpellCard()) damage = BuffManager.Instance.ApplySpellDamageModifier(damage);
        int weakStacks = (IsUpgraded() ? 2 : 1) * (grabbedDup ? 2 : 1);
        int iceAffinityGain = 3 * (grabbedDup ? 2 : 1);

        for (int i = 0; i < (grabbedDup ? 2 : 1); i++)
        {
            foreach (Enemy enemy in enemies)
            {
                if (enemy != null && enemy.gameObject != null)
                {
                    enemy.Hit(damage);
                    BuffManager.Instance.OnDealDamage(damage);
                }
            }
        }

        foreach (Enemy enemy in enemies)
        {
            if (enemy != null && enemy.gameObject != null)
                enemy.AddStatus(StatusType.Weak, weakStacks);
        }

        BuffManager.Instance.AddStatus(StatusType.IceAffinity, iceAffinityGain, -1);
        UIManager.Instance.ShowTip($"冰亲和度 +{iceAffinityGain}", Color.cyan);

        PlayEffect(transform.position);
    }

    /// <summary>
    /// 获取所有存活的敌人（使用 EnemyManager 缓存，O(1) 性能）
    /// </summary>
    private List<Enemy> GetAliveEnemies()
    {
        return EnemyManager.Instance.GetAliveEnemies();
    }
}
