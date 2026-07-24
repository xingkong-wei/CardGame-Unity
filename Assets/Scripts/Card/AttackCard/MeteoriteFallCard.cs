using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

// 陨石召唤 - 对所有敌人造成14点伤害，并给予3层易伤和3层虚弱
public class MeteoriteFallCard : CardItem
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

        int damage = data != null ? GetArg0() : 14;
        if (BuffManager.Instance.HasStatus(StatusType.Duplicate))
        {
            // 第二段因第一段上的易伤+25%
            int d1 = damage;
            int d2 = Mathf.CeilToInt(damage * 1.25f);
            damage = d1 + d2;
        }

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
        base.OnCardUsed();

        List<Enemy> enemies = GetAliveEnemies();
        if (enemies.Count == 0) return;

        int damage = data != null ? GetArg0() : 14;
        int debuffStacks = 3;

        AudioManager.Instance.PlayEffect("Effect/sword");

        foreach (Enemy enemy in enemies)
        {
            if (enemy != null && enemy.gameObject != null)
            {
                enemy.Hit(damage);
                BuffManager.Instance.OnDealDamage(damage);

                enemy.AddStatus(StatusType.Vulnerable, debuffStacks);
                enemy.AddStatus(StatusType.Weak, debuffStacks);
            }
        }

        UIManager.Instance.ShowTip($"对所有敌人造成 {damage} 点伤害，并施加 {debuffStacks} 层易伤和 {debuffStacks} 层虚弱", Color.red);

        // 播放特效
        if (!string.IsNullOrEmpty(data.effects))
        {
            GameObject effectObj = Instantiate(Resources.Load(data.effects)) as GameObject;
            if (effectObj != null)
            {
                Vector3 pos = Camera.main.transform.position;
                pos.y = 0;
                effectObj.transform.position = pos;
                Destroy(effectObj, 2);
            }
        }
    }

    private List<Enemy> GetAliveEnemies()
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
