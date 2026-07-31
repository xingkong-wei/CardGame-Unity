using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 易伤药水 - 给予单个敌人 3 层易伤
/// LineUI 起点 = 被点击的药水按钮，终点跟随鼠标
/// </summary>
public class EasilyInjuredPotion : PotionBase
{
    public override void Use()
    {
        base.Use();

        List<Enemy> aliveEnemies = GetAliveEnemies();

        if (aliveEnemies.Count == 0)
        {
            Debug.LogWarning("[EasilyInjuredPotion] 没有存活的敌人！");
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

        // 显示 LineUI，设置起点为被点击的药水按钮位置
        UIManager.Instance.ShowUI<LineUI>("LineUI");
        LineUI lineUI = UIManager.Instance.GetUI<LineUI>("LineUI");
        if (lineUI != null)
            lineUI.SetStartPos(potionBtnScreenPos);

        bool targetSelected = false;

        while (!targetSelected)
        {
            if (lineUI != null && canvasRT != null)
            {
                // 将鼠标屏幕坐标转为 Canvas 局部坐标（与 SetStartPos 一致）
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
        enemy.AddStatus(StatusType.Vulnerable, stacks);
        UIManager.Instance.ShowTip($"敌人获得 {stacks} 层易伤", new Color(1f, 0.4f, 0.4f));
    }

    private List<Enemy> GetAliveEnemies()
    {
        return EnemyManager.Instance.GetAliveEnemies();
    }
}
