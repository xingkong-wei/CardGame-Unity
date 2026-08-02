# 《牌咒》—— Roguelike 卡牌构筑游戏

> 独立开发的类《杀戮尖塔》Roguelike 卡牌构筑游戏，历时 3 个月完成可玩版本。

## 项目信息

| 项目 | 详情 |
| --- | --- |
| 引擎 | Unity 2022.3.62f3 |
| 语言 | C# |
| 渲染管线 | URP (Universal Render Pipeline) |
| 目标平台 | Windows (Standalone, IL2CPP) |
| 分辨率 | 1920×1080 |
| 版本控制 | Git + Git LFS + GitHub |

---

## 游戏简介

《牌咒》是一款回合制 Roguelike 卡牌构筑游戏。玩家探索随机生成的 12 层地图，在战斗、商店、宝箱、休息点等事件中不断强化卡组，最终击败 Boss 完成一轮冒险。

### 核心玩法循环

```
进入地图 → 逐层探索节点 → 战斗 / 商店 / 宝箱 / 休息 / 随机事件
    → 获得卡牌 / 药水 / 遗物 / 金币 → 构筑卡组 → 击败 Boss → 通关
```

### 战斗系统

- **回合制**：玩家回合 → 敌人回合交替进行
- **能量系统**：每回合 3 点能量（可通过 Buff / 遗物提升）
- **卡牌堆机制**：抽牌堆 → 弃牌堆 → 消耗牌堆，手牌上限 10 张，基础抽牌 4 张
- **卡牌类型**（共 9 种标签）：
  - **攻击**：点击敌人释放，支持多段攻击
  - **技能**：点击释放，提供护盾或辅助效果
  - **法术**：向上拖拽释放，受元素亲和度加成
  - **能力**：一次性使用后消耗，提供永久 Buff
  - **状态**：敌人塞入牌组的负面卡牌
  - **诅咒 / 虚无 / 消耗 / 任务**
- **升级系统**：卡牌实例级升级，可减少费用、增强效果或移除消耗标签
- **护盾机制**：格挡值先于血量承受伤害

### 元素亲和度系统（核心特色）

三种元素亲和度构筑差异化战斗风格：

| 元素 | 效果 | 层数上限 |
| --- | --- | --- |
| 🔥 火亲和度 | 每层法术伤害 +1 | 10（聚灵后 15） |
| ❄️ 冰亲和度 | 每层回合结束 +1 格挡 | 10（聚灵后 15） |
| ⚡ 电亲和度 | 每 2 层抽牌数 +1 | 10（聚灵后 15） |

---

## 游戏内容规模

| 类别 | 数量 |
| --- | --- |
| 卡牌 | 35 张（30 张玩家卡 + 5 张敌方状态卡） |
| 卡牌稀有度 | 4 级（基础 / 普通 / 稀有 / 传说） |
| 敌人 | 11 种（10 种普通/精英 + Boss 赤焰龙） |
| Buff / Debuff | 34 种 |
| 药水 | 45 种（普通 16 / 罕见 17 / 稀有 12） |
| 遗物 | 23 种 |
| 关卡配置 | 11 个（普通 7 / 精英 2 / Boss 2） |
| 地图层数 | 12 层（含固定宝箱层、精英层、休息层、Boss 层） |
| 地图节点蓝图 | 9 种（小怪 / 精英 / 休息 / 宝箱 / 商店 / 随机事件 + 3 种 Boss 变体） |

---

## 技术架构

### 设计模式

| 模式 | 应用场景 |
| --- | --- |
| **有限状态机 (FSM)** | 战斗流程：`Init → PlayerTurn → EnemyTurn → Win / Loss` |
| **门面模式** | FightManager 对外统一入口，内部委托 PlayerStateManager / RelicInventory / PotionInventory |
| **策略模式** | Buff 系统：34 种状态效果通过回调委托注入行为，新增 Buff 无需修改核心代码 |
| **数据驱动** | 卡牌、敌人、遗物、药水、关卡全部基于 ScriptableObject 配置，策划可独立编辑 |
| **单例模式** | 全局管理器（FightManager、EnemyManager、UIManager 等） |
| **事件驱动** | `GameEvents` 静态事件（OnBattleVictory / OnNodeVisited）实现模块间解耦通信 |
| **反射工厂** | 敌人 / 遗物 / 药水通过 `scriptName` 反射实例化，扩展新内容零耦合 |
| **对象池** | CardItem、Tips、DamageEffect、BuffIcon 等高频对象复用，减少 GC 压力 |

### 项目结构

```
Assets/
├── Scripts/                      # 核心 C# 脚本（225 个文件）
│   ├── Manager/                  # 管理器层（16 个文件）
│   │   ├── FightManager.cs       #   战斗总控（门面模式）
│   │   ├── EnemyManager.cs       #   敌人管理 & AI 调度
│   │   ├── FightCardManager.cs   #   卡牌堆（抽牌 / 弃牌 / 消耗）
│   │   ├── RoleManager.cs        #   玩家卡组 & 升级
│   │   ├── PlayerStateManager.cs #   玩家战斗状态（HP / 能量 / 护盾 / 金币）
│   │   ├── SaveManager.cs        #   JSON 存档序列化
│   │   ├── SaveFileManager.cs    #   二进制存档文件读写
│   │   ├── AudioManager.cs       #   音频管理（BGM / SFX）
│   │   ├── PotionInventory.cs    #   药水背包
│   │   ├── RelicInventory.cs     #   遗物背包
│   │   ├── Enemy.cs              #   敌人基类（partial：核心 / 动画 / 状态 / 视觉）
│   │   └── ExitManager.cs        #   退出管理
│   ├── Fight/                    # 战斗状态机（5 个状态 + 1 个基类）
│   │   ├── FightInit.cs          #   Init：重置 Buff / 卡牌堆，加载敌人
│   │   ├── Fight_PlayerTurn.cs   #   PlayerTurn：重置能量（3 点），抽牌（4 张）
│   │   ├── Fight_EnemyTurn.cs    #   EnemyTurn：敌人行动，回合结束结算，清除护盾
│   │   ├── Fight_Win.cs          #   Win：结算药水 / 遗物掉落，打开选卡奖励界面
│   │   ├── Fight_Loss.cs         #   Loss：重置状态，显示失败界面
│   │   └── FightUnit.cs          #   状态基类
│   ├── Card/                     # 卡牌系统（40 个文件）
│   │   ├── AttackCard/           #   攻击卡子类
│   │   ├── DefenseCard/          #   防御卡子类
│   │   ├── AbilityCard/          #   能力卡子类
│   │   └── OtherCard/            #   其他卡牌子类
│   ├── Buff/                     # Buff 状态系统（6 个文件）
│   │   ├── StatusEffect.cs       #   状态效果基类
│   │   ├── StatusCallbacks.cs    #   策略模式回调（OnTurnStart / OnHit / OnDeath 等）
│   │   └── BuffManager.cs        #   Buff 管理器
│   ├── Enemy/                    # 敌人 AI（11 种独立行为脚本）
│   ├── Map/                      # 随机地图生成 & 节点导航（23 个文件）
│   ├── UI/                       # UGUI 界面（30 个文件）
│   ├── Data/                     # ScriptableObject 数据定义（16 个文件）
│   ├── Relics/                   # 遗物系统（25 个文件）
│   ├── Potion/                   # 药水系统（46 个文件）
│   ├── Tools/                    # 工具类
│   │   ├── PoolManager.cs        #   对象池管理器
│   │   ├── ObjectPool.cs         #   对象池实现
│   │   └── ResourceCache.cs      #   资源缓存（预加载 9 个 UI 预制体 + 1 个材质）
│   ├── GameApp.cs                # 游戏入口（启动流程编排）
│   └── GameEvents.cs             # 静态事件总线
├── Resources/                    # 运行时资源
│   ├── Data_Card/Card/           # 35 张卡牌配置 (.asset)
│   ├── Data_Card/CardType/       # 9 种卡牌类型定义
│   ├── Data_Enemy/Enemies/       # 11 种敌人配置 (.asset)
│   ├── Data_Enemy/EnemyAttackEffects/  # 11 种敌人攻击特效配置
│   ├── Data_Relic/               # 23 种遗物配置 (.asset)
│   ├── Data_Potion/              # 45 种药水配置（Common / Uncommon / Rare）
│   ├── Data_Ability/             # 34 种 Buff / Debuff 配置 (.asset)
│   ├── Data_Level/               # 11 个关卡配置 (.asset)
│   ├── UI/                       # UI 预制体（35+）
│   ├── Effects/                  # 粒子特效预制体（500+）
│   ├── Model/                    # 怪物模型（10 个） + 场景物件（3 个）
│   ├── Sounds/                   # 音频资源
│   ├── GameConfig.asset          # 游戏全局配置
│   └── AudioConfig.asset         # 音频配置
├── Scenes/
│   ├── GameRun.unity             # 主运行场景
│   ├── SampleScene.unity         # 示例场景
│   └── SampleSceneUI.unity       # UI 示例场景
├── Arts/                         # 美术资源
│   ├── gui/                      # GUI 素材
│   ├── gui2/                     # GUI 素材（第二套）
│   ├── Simple Toon/              # 卡通风格素材
│   ├── Mini Legion Rock Golem/   # 石魔像模型
│   └── RPG Monster Duo/          # RPG 怪物模型
├── Controllers/                  # 动画控制器
│   ├── GolemController.controller
│   ├── slmController.controller  # 史莱姆动画
│   └── TurtleShellController.controller
├── SlayTheSpireMap/              # 地图系统素材 & 配置
│   └── Scriptable Objects/MapConfigs/DefaultMapConfig.asset
└── [第三方插件]                   # DOTween / TextMesh Pro / Epic Toon FX 等
```

### 启动流程（GameApp.cs）

```
SaveFileManager.Load()           → 初始化二进制存档系统
ResourceCache.Init()             → 预加载常用 UI 预制体（避免重复 IO）
PoolManager.Init()               → 初始化对象池（CardItem ×10 / Tips ×5 / DamageEffect ×3 / BuffIcon ×8）
GameConfigManager.Init()         → 加载全局配置表
EnemyDataManager.LoadAll()       → 加载全部敌人 ScriptableObject 数据
AudioManager.Init()              → 初始化音频管理器
RoleManager.Init() + ApplyUpgrades → 初始化玩家卡组 & 应用升级
FightManager.InitPotions()       → 初始化药水背包
FightManager.InitRelics()        → 初始化遗物背包
UIManager.ShowUI<LoginUI>()      → 显示登录界面
AudioManager.PlayBGM("bgm1")     → 播放背景音乐
```

### 地图生成（DefaultMapConfig）

| 层 | 节点类型 | 说明 |
| --- | --- | --- |
| 0 | 小怪 | 起始层 |
| 1 ~ 2 | 小怪（随机替换） | 73.9% 概率替换为商店 / 宝箱 / 休息 / 随机事件 |
| 3 | **宝箱** | 固定宝箱层 |
| 4 ~ 8 | 小怪（随机替换） | 随机节点层 |
| 9 | **精英** | 固定精英层 |
| 10 | **休息** | 固定休息层 |
| 11 | **Boss** | Boss 层 |

### 第三方依赖

| 插件 | 用途 |
| --- | --- |
| DOTween | 动画引擎（卡牌移动、UI 动效、摄像机震动） |
| TextMesh Pro | 高质量文字渲染 |
| Epic Toon FX | 粒子特效 |
| Fire Creatures Pack | 火焰生物模型（8 种） |
| OneLine | 编辑器属性绘制扩展 |

---

## 快速开始

1. 使用 **Unity 2022.3.62f3** 打开项目
2. 打开 `Assets/Scenes/GameRun.unity` 场景
3. 点击 Play 运行游戏

---

## 开发路线图

- [x] 核心战斗系统（FSM / 卡牌堆 / 能量 / 护盾）
- [x] 元素亲和度系统（火 / 冰 / 电三元素构筑）
- [x] 34 种 Buff / Debuff 状态效果
- [x] 45 种药水 & 23 种遗物
- [x] 11 种敌人（含 Boss 赤焰龙）
- [x] 随机地图生成（12 层 / 9 种节点蓝图）
- [x] 卡牌升级系统（实例级，可减少费用 / 增强效果 / 移除消耗）
- [x] 存档系统（JSON 序列化 + 二进制文件读写）
- [ ] 多岛屿 / 关卡扩展（当前仅 1 个岛屿）
- [ ] 更多卡牌与敌人
- [ ] 平衡性调整

---

## 许可证

个人学习项目，保留所有权利。
