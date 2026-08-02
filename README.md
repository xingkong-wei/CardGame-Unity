# 《牌咒》—— Roguelike 卡牌构筑游戏

<p align="center">
  <b>独立开发的类《杀戮尖塔》Roguelike 卡牌构筑游戏</b><br>
  <sub>历时 3 个月完成可玩版本 · 227 个 C# 脚本 · 160+ 可配置数据资产</sub>
</p>

---

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

《牌咒》是一款回合制 Roguelike 卡牌构筑游戏。玩家在随机生成的 12 层地图中探索，穿越战斗、商店、宝箱、休息点等事件节点，不断收集卡牌、药水和遗物来强化卡组，最终击败 Boss 完成冒险。

### 核心玩法循环

```
┌─────────────────────────────────────────────────┐
│  进入地图 → 逐层探索节点                        │
│    ├── 战斗：回合制卡牌对战，胜利获得奖励        │
│    ├── 商店：花费金币购买卡牌/药水/遗物         │
│    ├── 宝箱：获得遗物或药水                     │
│    ├── 休息：回复生命或升级卡牌                  │
│    └── 随机事件：触发特殊剧情/奖励              │
│   ↓                                              │
│  构筑卡组 → 击败 Boss → 通关                    │
└─────────────────────────────────────────────────┘
```

### 战斗系统

- **回合制**：玩家回合与敌人回合交替进行
- **能量系统**：每回合 3 点能量（可通过 Buff / 遗物提升）
- **卡牌堆机制**：抽牌堆 → 手牌 → 弃牌堆 → 消耗牌堆；手牌上限 10 张，每回合抽 4 张
- **卡牌类型**（共 9 种标签）：
  | 类型 | 描述 | 使用方式 |
  | --- | --- | --- |
  | 攻击 | 点击敌人释放，支持多段攻击 | 点击选择 → 划线瞄准 → 左键攻击 |
  | 技能 | 提供护盾或辅助效果 | 向上拖拽释放 |
  | 法术 | 受元素亲和度加成 | 向上拖拽释放 |
  | 能力 | 一次性使用后消耗，提供永久 Buff | 向上拖拽释放 |
  | 状态 | 敌人塞入牌组的负面卡牌 | — |
  | 诅咒 / 虚无 / 消耗 / 任务 | 特殊机制卡牌 | — |
- **升级系统**：卡牌实例级升级，可减少费用、增强效果或移除消耗标签
- **护盾机制**：格挡值先于血量承受伤害

### 元素亲和度系统（核心特色）

三种元素亲和度构筑差异化战斗风格，通过卡牌和遗物获得亲和度层数：

| 元素 | 效果 | 层数上限 |
| --- | --- | --- |
| 🔥 火亲和度 | 每层法术伤害 +1 | 10（聚灵后 15） |
| ❄️ 冰亲和度 | 每层回合结束 +1 格挡 | 10（聚灵后 15） |
| ⚡ 电亲和度 | 每 2 层抽牌数 +1 | 10（聚灵后 15） |

亲和度系统与遗物深度联动：`元素法典` 开局翻倍亲和度、`元素转换器` 切换元素类型、`贤者之石` 以亲和度换取能量等。

---

## 游戏内容规模

| 类别 | 数量 | 说明 |
| --- | --- | --- |
| 卡牌 | 35 张 | 30 张玩家卡 + 5 张敌方状态卡 |
| 卡牌稀有度 | 7 级 | Basic / Common / Uncommon / Rare / Event / Quest / Generated |
| 敌人 | 11 种 | 10 种普通/精英 + Boss 赤焰龙 |
| Buff / Debuff | 50+ 种 | 含力量、易伤、中毒、燃烧、亲和度等 |
| 药水 | 45 种 | Common 16 / Uncommon 17 / Rare 12 |
| 遗物 | 25 种 | 含战斗遗物、亲和度遗物、资源遗物等 |
| 关卡配置 | 11 个 | 普通 7 / 精英 2 / Boss 2 |
| 地图层数 | 12 层 | 含固定宝箱层、精英层、休息层、Boss 层 |
| 地图节点蓝图 | 9 种 | 小怪 / 精英 / 休息 / 宝箱 / 商店 / 随机事件 + 3 种 Boss 变体 |
| 代码规模 | ~227 个 .cs 文件 | 覆盖 Manager / Card / Enemy / Buff / UI / Map 等模块 |

---

## 技术架构

### 架构总览

```
GameApp (启动入口)
    │
    ├── SaveFileManager ─── 二进制存档读写
    ├── ResourceCache ───── 资源预加载缓存
    ├── PoolManager ──────── 对象池 (CardItem / Tips / DamageEffect / BuffIcon)
    ├── GameConfigManager ── 全局配置加载
    ├── EnemyDataManager ─── 敌人数据预加载
    ├── AudioManager ─────── 音频管理 (BGM / SFX)
    ├── RoleManager ──────── 玩家卡组 & 升级
    └── FightManager ─────── 战斗总控 (门面模式)
            │
            ├── PlayerStateManager ─── HP / 能量 / 护盾 / 金币
            ├── FightCardManager ───── 抽牌堆 / 弃牌堆 / 消耗堆
            ├── EnemyManager ───────── 敌人管理 & AI 调度
            ├── BuffManager ────────── 玩家 Buff 状态管理
            ├── RelicInventory ─────── 遗物背包 (反射实例化)
            ├── PotionInventory ────── 药水背包 (反射实例化)
            ├── UIManager ──────────── UI 窗口管理
            └── FightUnit (FSM) ─────── 战斗状态机
                    ├── FightInit        → 初始化
                    ├── Fight_PlayerTurn → 玩家回合
                    ├── Fight_EnemyTurn  → 敌人回合
                    ├── Fight_Win        → 胜利结算
                    └── Fight_Loss       → 失败结算
```

### 设计模式

| 模式 | 应用场景 |
| --- | --- |
| **有限状态机 (FSM)** | 战斗流程：`Init → PlayerTurn → EnemyTurn → Win / Loss`，通过 `FightUnit` 多态实现 |
| **门面模式** | `FightManager` 作为战斗统一入口，内部委托 `PlayerStateManager` / `RelicInventory` / `PotionInventory` / `FightCardManager` 等子模块 |
| **策略模式** | Buff 系统：50+ 种状态效果通过 `StatusCallbacks.Inject()` 注入回调委托，新增 Buff 无需修改 `BuffManager` 核心代码 |
| **数据驱动** | 卡牌、敌人、遗物、药水、关卡全部基于 `ScriptableObject` 配置，策划可独立编辑 |
| **单例模式** | 全局管理器（`FightManager`、`EnemyManager`、`UIManager` 等） |
| **事件驱动** | `GameEvents` 静态事件（`OnBattleVictory` / `OnNodeVisited` / `OnAffinityChanged` 等）实现模块间解耦通信 |
| **反射工厂** | 敌人 / 遗物 / 药水通过 `scriptName` 字段反射实例化，新增内容只需添加配置和脚本，零耦合扩展 |
| **对象池** | `CardItem`、`Tips`、`DamageEffect`、`BuffIcon` 等高频对象复用，减少 GC 压力 |
| **Partial Class** | `Enemy` 拆分为 Core / Status / Animation / Visual 四个部分，职责分离清晰 |

### 项目结构

```
Assets/
├── Scripts/                           # 核心 C# 脚本（~227 个文件）
│   ├── GameApp.cs                     #   游戏入口，启动流程编排
│   ├── GameEvents.cs                  #   静态事件总线（模块解耦）
│   ├── Manager/                       #   管理器层（16 个文件）
│   │   ├── FightManager.cs            #     战斗总控（门面模式）
│   │   ├── EnemyManager.cs            #     敌人管理 & AI 调度
│   │   ├── FightCardManager.cs        #     卡牌堆（抽牌 / 弃牌 / 消耗）
│   │   ├── RoleManager.cs             #     玩家卡组 & 升级
│   │   ├── PlayerStateManager.cs      #     玩家战斗状态（HP / 能量 / 护盾 / 金币）
│   │   ├── SaveManager.cs             #     JSON 存档序列化
│   │   ├── SaveFileManager.cs         #     二进制存档文件读写
│   │   ├── AudioManager.cs            #     音频管理（BGM / SFX）
│   │   ├── PotionInventory.cs         #     药水背包
│   │   ├── RelicInventory.cs          #     遗物背包
│   │   ├── Enemy.cs                   #     敌人基类（partial: Core/Status/Animation/Visual）
│   │   └── ExitManager.cs             #     退出管理
│   ├── Fight/                         #   战斗状态机（5 状态 + 1 基类）
│   │   ├── FightInit.cs               #     Init：重置 Buff / 卡牌堆，加载敌人
│   │   ├── Fight_PlayerTurn.cs        #     PlayerTurn：重置能量，抽牌
│   │   ├── Fight_EnemyTurn.cs         #     EnemyTurn：敌人行动，回合结算
│   │   ├── Fight_Win.cs               #     Win：结算掉落，选卡奖励
│   │   ├── Fight_Loss.cs              #     Loss：重置状态，失败界面
│   │   └── FightUnit.cs               #     状态基类
│   ├── Card/                          #   卡牌系统（~40 个文件）
│   │   ├── AttackCard/                #     攻击卡子类
│   │   ├── DefenseCard/               #     防御卡子类
│   │   ├── AbilityCard/               #     能力卡子类
│   │   └── OtherCard/                 #     其他卡牌子类
│   ├── Buff/                          #   Buff 状态系统
│   │   ├── StatusEffect.cs            #     状态效果基类
│   │   ├── StatusCallbacks.cs         #     策略模式回调注册中心
│   │   └── BuffManager.cs             #     Buff 管理器
│   ├── Enemy/                         #   敌人 AI 行为脚本（11 种）
│   ├── Map/                           #   随机地图生成 & 节点导航（~23 个文件）
│   ├── UI/                            #   UGUI 界面（~30 个文件，20+ 窗口）
│   ├── Data/                          #   ScriptableObject 数据定义（16 个文件）
│   ├── Relics/                        #   遗物效果脚本（25 种）
│   ├── Potion/                        #   药水效果脚本（45 种）
│   └── Tools/                         #   工具类
│       ├── PoolManager.cs             #     对象池管理器
│       ├── ObjectPool.cs              #     对象池实现
│       └── ResourceCache.cs           #     资源缓存
├── Resources/                         # 运行时资源
│   ├── Data_Card/Card/                #   35 张卡牌配置 (.asset)
│   ├── Data_Card/CardType/            #   9 种卡牌类型定义
│   ├── Data_Enemy/Enemies/            #   11 种敌人配置 (.asset)
│   ├── Data_Relic/                    #   25 种遗物配置 (.asset)
│   ├── Data_Potion/                   #   45 种药水配置 (Common/Uncommon/Rare)
│   ├── Data_Ability/                  #   50+ 种 Buff/Debuff 配置 (.asset)
│   ├── Data_Level/                    #   11 个关卡配置 (.asset)
│   ├── UI/                            #   UI 预制体（35+）
│   ├── Effects/                       #   粒子特效预制体（500+）
│   ├── Model/                         #   怪物模型（10 个） + 场景物件
│   ├── Sounds/                        #   音频资源
│   ├── GameConfig.asset               #   游戏全局配置
│   └── AudioConfig.asset              #   音频配置
├── Scenes/
│   └── GameRun.unity                  #   主运行场景
├── Arts/                              # 美术资源
│   ├── gui/                           #   GUI 素材
│   ├── Simple Toon/                   #   卡通风格素材
│   └── [怪物模型资源]                  #   Mini Legion Rock Golem / RPG Monster Duo 等
├── SlayTheSpireMap/                   # 地图系统素材 & 配置
└── [第三方插件]                        #   DOTween / TextMesh Pro / Epic Toon FX 等
```

### 启动流程

```
SaveFileManager.Load()              → 初始化二进制存档系统
ResourceCache.Init()                → 预加载常用 UI 预制体
PoolManager.Init()                  → 初始化对象池
GameConfigManager.Init()            → 加载全局配置表
EnemyDataManager.LoadAll()          → 预加载全部敌人数据
AudioManager.Init()                 → 初始化音频管理器
RoleManager.Init() + ApplyUpgrades  → 初始化玩家卡组 & 应用升级
FightManager.InitPotions()          → 初始化药水背包
FightManager.InitRelics()           → 初始化遗物背包
UIManager.ShowUI<LoginUI>()         → 显示登录界面
AudioManager.PlayBGM("bgm1")        → 播放背景音乐
```

### 地图生成

| 层 | 节点类型 | 说明 |
| --- | --- | --- |
| 0 | 小怪 | 起始层 |
| 1 ~ 2 | 小怪 | 73.9% 概率随机替换为商店 / 宝箱 / 休息 / 随机事件 |
| 3 | 宝箱 | 固定宝箱层 |
| 4 ~ 8 | 小怪 | 随机替换层 |
| 9 | 精英 | 固定精英层 |
| 10 | 休息 | 固定休息层 |
| 11 | Boss | Boss 层 |

### 第三方依赖

| 插件 | 用途 |
| --- | --- |
| DOTween | 动画引擎（卡牌移动、UI 动效、摄像机震动） |
| TextMesh Pro | 高质量文字渲染 |
| Epic Toon FX | 粒子特效（500+ 预制体） |
| Fire Creatures Pack | 火焰生物模型（8 种） |
| OneLine | 编辑器属性绘制扩展 |

---

## 快速开始

1. 使用 **Unity 2022.3.62f3** 打开项目
2. 打开 `Assets/Scenes/GameRun.unity` 场景
3. 点击 Play 运行游戏

---

## 核心系统设计

### Buff 系统（策略模式 + 委托回调）

Buff 系统是战斗的核心扩展点。通过 `StatusCallbacks` 注册中心为每种 `StatusType` 注入行为回调，`BuffManager` 在回合关键节点触发：

```
回合开始 → OnTurnStart()     → 结算力量衰减、再生等
出牌时   → OnCardPlayed()    → 法术共鸣、冥想计数
造成伤害 → OnDealDamage()    → 吸血、荆棘反伤
受到伤害 → ModifyTakenDamage() → 易伤加成
回合结束 → OnTurnEnd()       → 中毒/燃烧/流血伤害、冰亲和度格挡
```

新增 Buff 只需在 `StatusCallbacks.Inject()` 中注册回调，无需修改核心战斗逻辑。

### 遗物系统（反射工厂 + 生命周期钩子）

25 种遗物通过 `scriptName` 反射创建 `RelicBase` 子类实例。每种遗物可按需重写生命周期钩子：
`OnBattleStart` / `OnTurnStart` / `OnTurnEnd` / `OnCardPlayed` / `OnCardDrawn` / `OnDealDamage` / `OnGainBlock` / `OnEnemyKilled` / `OnAffinityChanged` / `ModifyAffinityGain` 等。

### 药水掉落系统（怜悯计数器）

- 基础掉落率 40%，精英 +12.5%
- 未掉落则概率累计 +10%（怜悯机制）
- 稀有度权重：Common 65% / Uncommon 25% / Rare 10%
- 精英遗物掉落：Common 50% / Uncommon 36% / Rare 14%
- Boss 遗物：必定 Uncommon 或 Rare

### 存档系统（双层架构）

| 层级 | 组件 | 职责 |
| --- | --- | --- |
| 底层 | `SaveFileManager` | 二进制 key-value 文件存储（`gamesave.bin`），替代 PlayerPrefs |
| 上层 | `SaveManager` | 游戏存档序列化（JSON），支持节点入口快照与 SL 读档 |

存档内容：血量 / 金币 / 岛屿进度 / 节点坐标 / 卡组（含 instanceId 和升级状态）/ 药水 / 遗物 / 地图 JSON。

---

## 技术亮点

- **反射驱动扩展**：卡牌、敌人、遗物、药水均通过 `scriptName` 反射实例化，新增内容零耦合
- **Partial Class 模块化**：Enemy 拆分为 Core / Status / Animation / Visual 四个 partial 文件，职责清晰
- **对象池优化**：CardItem、Tips、DamageEffect、BuffIcon 等高频对象预创建复用
- **资源预加载**：`ResourceCache` 启动时缓存常用 UI 预制体，避免运行时 IO 卡顿
- **伤害预览系统**：攻击前实时显示预计伤害，含魔杖充能 / 超巨化 / 复制药水等倍率计算
- **完整存档方案**：双层存储（二进制 + JSON），支持战斗入口 SL 读档

---

## 开发路线图

- [x] 核心战斗系统（FSM / 卡牌堆 / 能量 / 护盾）
- [x] 元素亲和度系统（火 / 冰 / 电三元素构筑）
- [x] 50+ 种 Buff / Debuff 状态效果
- [x] 45 种药水 & 25 种遗物
- [x] 11 种敌人（含 Boss 赤焰龙，含飞行模式切换）
- [x] 随机地图生成（12 层 / 9 种节点蓝图）
- [x] 卡牌升级系统（实例级，可减少费用 / 增强效果 / 移除消耗）
- [x] 存档系统（JSON 序列化 + 二进制文件读写）
- [x] 怜悯计数器药水掉落机制
- [ ] 多岛屿 / 关卡扩展（当前仅 1 个岛屿）
- [ ] 更多卡牌与敌人
- [ ] 平衡性调整

---

## 许可证

个人学习项目，保留所有权利。
