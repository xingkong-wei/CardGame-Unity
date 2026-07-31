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
            // 如果 CurrentMap 已经被外部加载（如继续游戏时恢复），直接显示即可
            if (CurrentMap != null)
            {
                view?.ShowMap(CurrentMap);
                return;
            }

            // 尝试从持久化数据加载地图
            string mapJson = null;

            // 优先从存档中读取当前岛屿索引
            int islandIndex = -1;
            if (SaveManager.HasSave())
            {
                GameSaveData saveData = SaveManager.Load();
                if (saveData != null)
                    islandIndex = saveData.currentIslandIndex;
            }
            if (islandIndex < 0)
                islandIndex = FightManager.Instance?.currentIslandIndex ?? 0;

            string islandKey = $"Map_Island_{islandIndex}";

            if (SaveFileManager.HasKey(islandKey))
            {
                mapJson = SaveFileManager.GetString(islandKey);
            }
            else if (SaveFileManager.HasKey("Map"))
            {
                mapJson = SaveFileManager.GetString("Map");
            }

            if (!string.IsNullOrEmpty(mapJson))
            {
                Map map = JsonConvert.DeserializeObject<Map>(mapJson);
                if (map != null)
                {
                    // 检查是否已到达 Boss
                    var bossNode = map.GetBossNode();
                    if (bossNode != null && map.path.Any(p => p.Equals(bossNode.point)))
                    {
                        // 已到达 Boss，生成新地图
                        GenerateNewMap();
                    }
                    else
                    {
                        CurrentMap = map;
                        view?.ShowMap(map);
                    }
                    return;
                }
            }

            // 没有可用的地图数据，生成新地图
            GenerateNewMap();
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
            SaveFileManager.SetString("Map", json);
            SaveFileManager.Flush();
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

        // 辅助方法：根据坐标查找 MapNode（使用 MapView 缓存的节点列表）
        private MapNode FindMapNode(Vector2Int point)
        {
            if (MapView.Instance == null) return null;
            foreach (var mn in MapView.Instance.MapNodes)
            {
                if (mn.Node.point == point)
                    return mn;
            }
            return null;
        }
    }
}
