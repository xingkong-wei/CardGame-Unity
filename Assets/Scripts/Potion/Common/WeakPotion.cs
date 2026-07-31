using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 虚弱药水 - 给予单个敌人3层虚弱
/// </summary>
public class WeakPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        List<Enemy> aliveEnemies = GetAliveEnemies();

        if (aliveEnemies.Count == 0)
        {
            Debug.LogWarning("[WeakPotion] 没有存活的敌人！");
            return;
        }

        if (aliveEnemies.Count == 1)
        {
            ApplyToEnemy(aliveEnemies[0]);
        }
        else
        {
            FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
            if (fightUI != null)
            {
                fightUI.StartCoroutine(EnemyTargetingRoutine(aliveEnemies));
            }
        }
    }

    private IEnumerator EnemyTargetingRoutine(List<Enemy> enemies)
    {
        FightUI fightUI = UIManager.Instance.GetUI<FightUI>("FightUI");
        Canvas canvas = fightUI?.GetComponentInParent<Canvas>();
        Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera : null;
        RectTransform canvasRT = canvas?.GetComponent<RectTransform>();

        UIManager.Instance.ShowUI<LineUI>("LineUI");
        LineUI lineUI = UIManager.Instance.GetUI<LineUI>("LineUI");
        if (lineUI != null)
            lineUI.SetStartPos(potionBtnScreenPos);

        bool targetSelected = false;

        while (!targetSelected)
        {
            if (lineUI != null && canvasRT != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRT, Input.mousePosition, cam, out Vector2 localMouse);
                lineUI.SetEndPos(localMouse);
            }

            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Enemy")))
                {
                    Enemy enemy = hit.collider.GetComponent<Enemy>();
                    if (enemy != null && enemies.Contains(enemy))
                    {
                        ApplyToEnemy(enemy);
                        targetSelected = true;
                    }
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                targetSelected = true;
            }

            yield return null;
        }

        UIManager.Instance.CloseUI("LineUI");
    }

    private void ApplyToEnemy(Enemy enemy)
    {
        int stacks = data.effectValue;
        enemy.AddStatus(StatusType.Weak, stacks);
        UIManager.Instance.ShowTip($"敌人获得 {stacks} 层虚弱", new Color(0.5f, 0.5f, 0.5f));
    }

    private List<Enemy> GetAliveEnemies()
    {
        return EnemyManager.Instance.GetAliveEnemies();
    }
}
