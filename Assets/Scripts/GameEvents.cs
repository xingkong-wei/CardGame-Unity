using System;
using UnityEngine;

public static class GameEvents
{
    // 当战斗胜利时触发，参数为当前节点在Map中的坐标（Vector2Int）
    public static Action<Vector2Int> OnBattleVictory;

    // 可选：当节点被访问时触发（可用于通用更新）
    public static Action<Vector2Int> OnNodeVisited;
}