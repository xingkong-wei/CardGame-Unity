using System;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Map
{
    public class MapPlayerTracker : MonoBehaviour
    {
        public bool lockAfterSelecting = false;
        public float enterNodeDelay = 1f;
        public MapManager mapManager;
        public MapView view;

        public static MapPlayerTracker Instance;

        public bool Locked { get; set; }

        private void Awake()
        {
            Instance = this;
        }

        public void SelectNode(MapNode mapNode)
        {
            if (Locked) return;

            // Debug.Log("Selected node: " + mapNode.Node.point);

            if (mapManager.CurrentMap.path.Count == 0)
            {
                // player has not selected the node yet, he can select any of the nodes with y = 0
                if (mapNode.Node.point.y == 0)
                    SendPlayerToNode(mapNode);
                else
                    PlayWarningThatNodeCannotBeAccessed();
            }
            else
            {
                Vector2Int currentPoint = mapManager.CurrentMap.path[mapManager.CurrentMap.path.Count - 1];
                Node currentNode = mapManager.CurrentMap.GetNode(currentPoint);

                if (currentNode != null && currentNode.outgoing.Any(point => point.Equals(mapNode.Node.point)))
                    SendPlayerToNode(mapNode);
                else
                    PlayWarningThatNodeCannotBeAccessed();
            }
        }

        private void SendPlayerToNode(MapNode mapNode)
        {
            Locked = lockAfterSelecting;
            // 不要在这里添加到 path,而是在进入战斗(触发 OnNodeClicked)后再添加
            // mapManager.CurrentMap.path.Add(mapNode.Node.point);
            // mapManager.SaveMap();

            view.SetAttainableNodes();
            view.SetLineColors();
            mapNode.ShowSwirlAnimation();

            DOTween.Sequence().AppendInterval(enterNodeDelay).OnComplete(() => EnterNode(mapNode));
        }

        private static void EnterNode(MapNode mapNode)
        {
            // 1. 先尝试从 UIManager 获取
            SlayTheSpireMapUI mapUI = UIManager.Instance?.GetUI<SlayTheSpireMapUI>("SlayTheSpireMapUI");
            if (mapUI != null)
            {
                mapUI.OnNodeClicked(mapNode);
                return;
            }

            // 2. 如果 UIManager 中找不到,使用 FindObjectOfType
            SlayTheSpireMapUI[] mapUIs = UnityEngine.Object.FindObjectsOfType<SlayTheSpireMapUI>();
            if (mapUIs != null && mapUIs.Length > 0)
            {
                mapUIs[0].OnNodeClicked(mapNode);
                return;
            }

        }

        private void PlayWarningThatNodeCannotBeAccessed()
        {
            // 选中的节点无法访问
        }
    }
}