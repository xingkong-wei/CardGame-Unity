using UnityEngine;

namespace Map
{
    /// <summary>
    /// 神秘节点（？节点）结果类型
    /// </summary>
    public enum MysteryResult
    {
        Monster,   // 怪物战斗
        Shop,      // 商店
        Treasure   // 宝箱
    }

    /// <summary>
    /// 神秘节点解析器 - 负责？节点的概率计算与结果判定
    /// 
    /// 概率规则：
    /// 初始：怪物20%、商店45%、宝箱35%
    /// 递增：每连续避开怪物一次，怪物概率+10%，商店和宝箱各-5%
    /// 重置：遇到怪物后恢复初始概率
    /// </summary>
    public class MysteryNodeResolver
    {
        private const string MYSTERY_STREAK_KEY = "MysteryStreakCount";
        private const string DEFAULT_STREAK = "0";

        // 初始概率
        private const float INIT_MONSTER  = 20f;
        private const float INIT_SHOP     = 45f;
        private const float INIT_TREASURE = 35f;

        // 每次未遇怪物的增减量
        private const float MONSTER_INCREASE   = 10f;
        private const float OTHER_DECREASE     = 5f;

        private static MysteryNodeResolver _instance;
        public static MysteryNodeResolver Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MysteryNodeResolver();
                return _instance;
            }
        }

        /// <summary>
        /// 当前连续未遇到怪物的次数
        /// </summary>
        private int StreakCount
        {
            get => SaveFileManager.GetInt(MYSTERY_STREAK_KEY, 0);
            set
            {
                SaveFileManager.SetInt(MYSTERY_STREAK_KEY, value);
                SaveFileManager.Flush();
            }
        }

        /// <summary>
        /// 根据当前概率随机出一个结果
        /// </summary>
        public MysteryResult RollResult()
        {
            int streak = StreakCount;

            float monsterChance  = INIT_MONSTER  + streak * MONSTER_INCREASE;
            float shopChance     = INIT_SHOP     - streak * OTHER_DECREASE;
            float treasureChance = INIT_TREASURE - streak * OTHER_DECREASE;

            // 防止概率超出合法范围
            monsterChance  = Mathf.Clamp(monsterChance,  0f, 100f);
            shopChance     = Mathf.Clamp(shopChance,     0f, 100f);
            treasureChance = Mathf.Clamp(treasureChance, 0f, 100f);

            float total = monsterChance + shopChance + treasureChance;
            float roll = Random.Range(0f, total);

            if (roll < monsterChance)
                return MysteryResult.Monster;
            else if (roll < monsterChance + shopChance)
                return MysteryResult.Shop;
            else
                return MysteryResult.Treasure;
        }

        /// <summary>
        /// 记录本次神秘节点的结果，更新 streak
        /// </summary>
        public void RecordResult(MysteryResult result)
        {
            if (result == MysteryResult.Monster)
            {
                // 遇到怪物，重置 streak
                StreakCount = 0;
            }
            else
            {
                // 未遇到怪物，streak +1
                StreakCount += 1;
            }
        }

        /// <summary>
        /// 重置神秘节点状态（新游戏开始时调用）
        /// </summary>
        public static void Reset()
        {
            SaveFileManager.SetInt(MYSTERY_STREAK_KEY, 0);
            SaveFileManager.Flush();
        }

        /// <summary>
        /// 获取当前概率信息（用于调试）
        /// </summary>
        public string GetCurrentProbabilities()
        {
            int streak = StreakCount;
            float monsterChance  = Mathf.Clamp(INIT_MONSTER  + streak * MONSTER_INCREASE,   0f, 100f);
            float shopChance     = Mathf.Clamp(INIT_SHOP     - streak * OTHER_DECREASE,     0f, 100f);
            float treasureChance = Mathf.Clamp(INIT_TREASURE - streak * OTHER_DECREASE,     0f, 100f);

            return $"怪物:{monsterChance:F0}% 商店:{shopChance:F0}% 宝箱:{treasureChance:F0}% (连续避开:{streak}次)";
        }
    }
}
