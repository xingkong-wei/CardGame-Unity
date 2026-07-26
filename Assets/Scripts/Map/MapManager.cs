using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace Map
{
    public class MapManager : MonoBehaviour
    {
        public MapConfig config;
        public MapView view;

        public Map CurrentMap { get; set; }

        private void Start()
        {
            if (PlayerPrefs.HasKey("Map"))
            {
                string mapJson = PlayerPrefs.GetString("Map");
                Map map = JsonConvert.DeserializeObject<Map>(mapJson);
                // using this instead of .Contains()
                if (map.path.Any(p => p.Equals(map.GetBossNode().point)))
                {
                    // payer has already reached the boss, generate a new map
                    GenerateNewMap();
                }
                else
                {
                    CurrentMap = map;
                    // player has not reached the boss yet, load the current map
                    view.ShowMap(map);
                }
            }
            else
            {
                GenerateNewMap();
            }
        }

        public void GenerateNewMap()
        {
            Map map = MapGenerator.GetMap(config);
            CurrentMap = map;
            Debug.Log(map.ToJson());
            view.ShowMap(map);
        }

        public void SaveMap()
        {
            if (CurrentMap == null) return;

            string json = JsonConvert.SerializeObject(CurrentMap, Formatting.Indented,
                new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
            PlayerPrefs.SetString("Map", json);
            PlayerPrefs.Save();
        }

        private void OnApplicationQuit()
        {
            SaveMap();
        }

        void OnEnable()
        {
            GameEvents.OnBattleVictory += OnBattleVictory;
        }

        void OnDisable()
        {
            GameEvents.OnBattleVictory -= OnBattleVictory;
        }

        void OnBattleVictory(Vector2Int nodePoint)
        {
            // 节点状态已经在点击时更新了,这里只保留事件监听接口
            // 不需要额外的处理
            Debug.Log($"战斗胜利事件触发: {nodePoint}");
        }

        // 辅助方法：根据坐标查找 MapNode
        private MapNode FindMapNode(Vector2Int point)
        {
            MapNode[] nodes = FindObjectsOfType<MapNode>();
            foreach (var mn in nodes)
            {
                if (mn.Node.point == point)
                    return mn;
            }
            return null;
        }
    }
}
