using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 甲虫萃取液 - 选择一名敌人，给予4层缩小（每层攻击伤害-30%）
/// </summary>
public class BeetleExtractPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        List<Enemy> aliveEnemies = GetAliveEnemies();
        if (aliveEnemies.Count == 0)
        {
            UIManager.Instance.ShowTip("没有存活的敌人", Color.red);
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
                fightUI.StartCoroutine(EnemyTargetingRoutine(aliveEnemies));
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
                targetSelected = true;

            yield return null;
        }

        if (lineUI != null)
            UIManager.Instance.CloseUI("LineUI");
    }

    private void ApplyToEnemy(Enemy enemy)
    {
        int stacks = 4;
        enemy.AddStatus(StatusType.Shrink, stacks);
        UIManager.Instance.ShowTip($"缩小 +{stacks}", Color.magenta);
    }

    private List<Enemy> GetAliveEnemies()
    {
        return EnemyManager.Instance.GetAliveEnemies();
    }
}
