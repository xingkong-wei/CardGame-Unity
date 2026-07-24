# 怪物攻击特效配置编辑器工具

## 使用方法

1. 在 Unity 编辑器中打开 `Assets/Scripts/Editor/AttackEffectConfigGenerator.cs`

2. 在 Unity 菜单中选择 `Tools > Generate Enemy Attack Effects`

3. 所有怪物的攻击特效配置文件将自动生成到 `Assets/Resources/Data/EnemyAttackEffects/` 目录下

4. 如果需要修改某个怪物的特效，直接编辑对应的 .asset 文件即可

## 配置文件说明

每个怪物的配置文件包含：
- effectName: 必须与 enemy.txt 中 AttackAnim 字段的动画名完全匹配
- effectPrefabPath: 特效预制体路径
- spawnPositionType: 0=脚下, 1=中心, 2=头顶, 3=目标位置
- effectScale: 特效缩放倍数
- duration: 特效持续时间
- rotationOffset: 特效旋转角度
