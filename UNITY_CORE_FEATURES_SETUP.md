# Unity核心功能配置指南

> 手把手教你如何在Unity中配置4个核心战斗功能

---

## 📋 前置准备

### 1. 检查现有场景
- 打开你的游戏场景（如 `Assets/Scenes/Level_1.unity`）
- 确认场景中已有：
  - [ ] 玩家角色（带 `PlayerCharacterCtrl`）
  - [ ] 怪物生成点（SpawnRoot）
  - [ ] 至少一个防御塔位置

### 2. 创建必要的Layer
```
Window > Layers > Add Layer

Layer 8: Enemy        (怪物)
Layer 9: Tower        (防御塔)
Layer 10: Kitchen     (厨房区域)
```

---

## 🎯 步骤1：设置关卡生命值系统

### 1.1 创建管理器对象

```
Hierarchy右键 > Create Empty
名称: _LevelManagers

在_LevelManagers下:
  右键 > Create Empty
  名称: LevelHealthSystem
```

### 1.2 添加脚本组件

```
选中 LevelHealthSystem
Inspector > Add Component
搜索: "LevelHealthSystem"
添加脚本
```

### 1.3 配置参数

```
LevelHealthSystem 组件:
├── Max Health: 10          (初始生命值)
```

**测试**:
- 运行游戏
- 在Console输入: `LevelHealthSystem.Instance.TakeDamage(1)`
- 观察生命值是否减少

---

## 🚪 步骤2：设置厨房区域触发器

### 2.1 创建厨房区域

```
Hierarchy右键 > Create Empty
名称: KitchenZone
Position: 放在玩家基地的后方位置
```

### 2.2 添加触发器

```
选中 KitchenZone
Inspector > Add Component > Box Collider 2D

Box Collider 2D 设置:
├── Is Trigger: ✓ (勾选！)
├── Size: 根据你的厨房区域调整
│   例如: X=10, Y=10
└── Offset: 调整到合适位置
```

### 2.3 添加脚本

```
选中 KitchenZone
Inspector > Add Component
搜索: "KitchenZoneTrigger"
添加脚本

KitchenZoneTrigger 组件:
├── Show Debug Gizmos: ✓ (勾选，方便调试)
└── Gizmo Color: 红色半透明
```

### 2.4 可视化调试

```
在Scene视图中，你应该能看到红色半透明区域
如果看不到，点击Scene视图右上角的 Gizmos 按钮确保开启
```

**测试**:
- 运行游戏
- 手动拖动一个怪物到厨房区域
- 观察：
  - [ ] 怪物是否死亡
  - [ ] 关卡生命值是否减少
  - [ ] 怪物是否没有掉落食材

---

## 🏰 步骤3：配置防御塔自动攻击

### 3.1 设置防御塔Layer

```
选中你的防御塔Prefab（或场景中的防御塔）
Inspector > Layer > Tower (选择Tower层)
```

### 3.2 检查现有组件

你的防御塔应该已经有：
```
✓ CharacterCtrlBase
✓ PlayerCharacterCtrl
✓ TowerEnergySystem
✓ TowerController
✓ InteractableTower
```

### 3.3 配置TowerController的攻击参数

```
选中防御塔
找到 TowerController 组件

【攻击配置】
├── Attack Range: 5              (攻击范围，可调整)
├── Attack Interval: 1           (攻击间隔，1秒1次)
├── Base Attack: 10              (基础攻击力)
├── Skill Rate: 1.0              (技能倍率)
├── Attack Type: Physical        (物理攻击)
└── Enemy Layer: Enemy          (选择Enemy层！重要！)
```

### 3.4 设置怪物Layer

```
选中你的怪物Prefab（如 MeatMonster）
Inspector > Layer > Enemy (选择Enemy层)

对所有怪物Prefab都这样设置！
```

### 3.5 查看攻击范围

```
在Scene视图中:
- 选中防御塔
- 你应该看到红色圆圈（攻击范围）
- 当怪物进入范围时，会看到黄色连线
```

**测试**:
- 运行游戏
- 让怪物走到防御塔攻击范围内
- 观察：
  - [ ] 防御塔是否自动攻击怪物
  - [ ] 怪物血量是否减少
  - [ ] Console是否输出攻击日志
  - [ ] 宕机状态是否停止攻击

---

## 👹 步骤4：配置怪物攻击防御塔

### 4.1 修改怪物Prefab

```
打开怪物Prefab（如 Assets/Resources/Prefabs/Monsters/MeatMonster）
Inspector > Add Component
搜索: "MonsterCombatBehavior"
添加脚本
```

### 4.2 配置攻击参数

```
MonsterCombatBehavior 组件:
【攻击配置】
├── Attack Range: 1.5            (攻击范围)
├── Attack Interval: 2           (攻击间隔，2秒1次)
├── Energy Damage: 10            (能量伤害)
└── Tower Layer: Tower          (选择Tower层！重要！)

【调试】
└── Show Debug Gizmos: ✓ (勾选)
```

### 4.3 保存Prefab

```
点击Prefab上方的 "Apply" 按钮
确保所有该类型的怪物都应用了改动
```

### 4.4 对所有怪物重复操作

```
对每个怪物Prefab都添加 MonsterCombatBehavior：
- MeatMonster (茄茄章)
- VegetableMonster (菇菇鱼)
- PotatoMonster (豆豆牛)
- OnionMonster (葱葱鸡)
等等...
```

**测试**:
- 运行游戏
- 让怪物走到防御塔旁边
- 观察：
  - [ ] 怪物是否攻击防御塔
  - [ ] 防御塔能量是否减少
  - [ ] 能量为0时是否宕机
  - [ ] 宕机后是否失去碰撞体积

---

## 🖥️ 步骤5：创建关卡生命值UI

### 5.1 打开UI Canvas

```
Hierarchy > 找到你的 GameHUD Canvas
如果没有，创建一个:
  右键 > UI > Canvas
  名称: GameHUD
```

### 5.2 创建生命值UI面板

```
在 GameHUD 下:
右键 > UI > Panel
名称: LevelHealthPanel

设置位置（Anchor）:
- 点击Rect Transform左上角的方框
- 选择 "top-left" (左上角)
- Position: X=150, Y=-50
- Width: 250, Height: 60
```

### 5.3 添加UI元素

#### A. 背景图片
```
LevelHealthPanel 已经有 Image 组件
可以设置颜色或Sprite
```

#### B. 血条
```
在LevelHealthPanel下:
右键 > UI > Slider
名称: HealthSlider

Slider 设置:
├── Min Value: 0
├── Max Value: 10
├── Whole Numbers: ✓ (勾选)
├── Value: 10
```

调整Slider内部元素:
```
HealthSlider
├── Background (灰色)
├── Fill Area
│   └── Fill (Image)
│       └── Color: 绿色 (#00FF00)
└── Handle Slide Area (可以删除，不需要拖动)
```

#### C. 文本显示
```
在LevelHealthPanel下:
右键 > UI > Text
名称: HealthText

Text 组件:
├── Text: "10/10"
├── Font Size: 24
├── Alignment: Center
├── Color: 白色
```

#### D. 失败面板
```
在GameHUD下:
右键 > UI > Panel
名称: GameOverPanel

设置:
├── Anchor: 居中拉伸 (stretch-stretch)
├── Color: 黑色半透明 (A=180)

在GameOverPanel下添加文本:
右键 > UI > Text
名称: GameOverText
├── Text: "游戏失败！"
├── Font Size: 48
├── Color: 红色
├── Alignment: Center
```

### 5.4 添加UI脚本

```
选中 LevelHealthPanel
Inspector > Add Component
搜索: "LevelHealthUI"
添加脚本

连接引用:
├── Health Slider: 拖入 HealthSlider
├── Fill Image: 拖入 HealthSlider/Fill Area/Fill
├── Health Text: 拖入 HealthText
├── Game Over Panel: 拖入 GameOverPanel
```

### 5.5 设置颜色阈值

```
LevelHealthUI 组件:
【颜色设置】
├── Healthy Color: 绿色 (#00FF00)
├── Warning Color: 黄色 (#FFFF00)
├── Critical Color: 红色 (#FF0000)

【阈值】
├── Warning Threshold: 0.5    (50%以下变黄)
└── Critical Threshold: 0.3   (30%以下变红)
```

### 5.6 隐藏失败面板

```
选中 GameOverPanel
Inspector > 取消勾选左上角的复选框（禁用）
```

**测试**:
- 运行游戏
- 在Console执行: `LevelHealthSystem.Instance.TakeDamage(3)`
- 观察：
  - [ ] UI显示 "7/10"
  - [ ] 血条减少
  - [ ] 血量低于50%时变黄
  - [ ] 血量为0时显示失败面板

---

## 🎨 步骤6：调整Layer和Collision Matrix

### 6.1 设置Collision Matrix

```
Edit > Project Settings > Physics 2D

在 Layer Collision Matrix 中:
- Enemy 与 Tower: ✓ (勾选，可以碰撞)
- Enemy 与 Kitchen: ✓ (勾选，触发检测)
- Tower 与 Player: ✓ (勾选)
- Enemy 与 Enemy: ✗ (不勾选，怪物之间不碰撞)
```

---

## 🧪 完整测试流程

### 测试1：关卡生命值系统
```
1. 运行游戏
2. 在Console输入: 
   LevelHealthSystem.Instance.TakeDamage(1)
3. 观察UI是否显示 "9/10"
4. 连续扣血到0
5. 观察是否显示失败面板
```

### 测试2：怪物进入厨房
```
1. 运行游戏
2. 等待怪物生成
3. 如果怪物没有自动寻路到厨房，手动拖动一个
4. 观察：
   - 怪物进入厨房区域后死亡
   - 关卡生命值减少
   - 没有掉落食材
```

### 测试3：防御塔攻击
```
1. 确保防御塔能量充足
2. 让怪物进入防御塔攻击范围（5单位）
3. 观察：
   - Scene视图中出现黄色连线
   - 怪物血量减少
   - Console输出攻击日志
4. 让防御塔能量耗尽宕机
5. 观察宕机后是否停止攻击
```

### 测试4：怪物攻击防御塔
```
1. 让怪物走到防御塔旁边（1.5单位内）
2. 观察：
   - Scene视图中怪物有橙色攻击范围
   - 防御塔能量槽下降
   - Console输出怪物攻击日志
3. 等待防御塔能量耗尽
4. 观察：
   - 防御塔进入宕机状态
   - 怪物可以穿过防御塔
```

### 测试5：完整游戏循环
```
1. 开始游戏
2. 怪物生成并前进
3. 防御塔自动攻击怪物
4. 怪物攻击防御塔消耗能量
5. 玩家拾取掉落食材
6. 烹饪菜品
7. 喂食防御塔恢复能量
8. 如果怪物突破到厨房，关卡生命值减少
9. 生命值为0 = 游戏失败
10. 完成大订单 = 游戏胜利
```

---

## 🐛 常见问题排查

### 问题1: 防御塔不攻击怪物

**检查清单**:
- [ ] 怪物的Layer是否设置为 "Enemy"
- [ ] TowerController的Enemy Layer是否选择了 "Enemy"
- [ ] 怪物是否在攻击范围内（红色圆圈）
- [ ] 防御塔是否宕机（能量为0）
- [ ] 怪物的Team是否为 TeamB

**解决方案**:
```
1. 选中怪物Prefab
2. 确认 Layer = Enemy
3. 确认 CharacterCtrlBase.CharacterTeam = TeamB
4. 选中防御塔
5. 确认 TowerController.enemyLayer 包含 Enemy 层
```

### 问题2: 怪物不攻击防御塔

**检查清单**:
- [ ] 防御塔的Layer是否设置为 "Tower"
- [ ] MonsterCombatBehavior的Tower Layer是否选择了 "Tower"
- [ ] 怪物是否有 MonsterCombatBehavior 组件
- [ ] 怪物是否在攻击范围内（1.5单位）

**解决方案**:
```
1. 选中防御塔
2. 确认 Layer = Tower
3. 选中怪物Prefab
4. 确认有 MonsterCombatBehavior 组件
5. 确认 MonsterCombatBehavior.towerLayer 包含 Tower 层
```

### 问题3: 怪物进入厨房不扣血

**检查清单**:
- [ ] KitchenZone的Collider是否设为Trigger
- [ ] 怪物是否有Collider2D组件
- [ ] KitchenZone是否有 KitchenZoneTrigger 脚本
- [ ] LevelHealthSystem是否在场景中

**解决方案**:
```
1. 选中 KitchenZone
2. 确认 Box Collider 2D > Is Trigger = ✓
3. 确认有 KitchenZoneTrigger 组件
4. 选中怪物
5. 确认有 Collider2D（不是Trigger）
6. 确认 CharacterTeam = TeamB
```

### 问题4: UI不显示

**检查清单**:
- [ ] Canvas的Render Mode是否正确
- [ ] LevelHealthUI的引用是否连接
- [ ] GameOverPanel初始是否禁用
- [ ] EventSystem是否在场景中

**解决方案**:
```
1. 选中 Canvas
2. 确认 Render Mode = Screen Space - Overlay
3. 选中 LevelHealthPanel
4. 确认 LevelHealthUI 的所有引用都已连接
5. 选中 GameOverPanel
6. 确认初始状态为禁用（左上角不勾选）
```

### 问题5: 防御塔宕机后怪物还是卡住

**检查清单**:
- [ ] 防御塔是否有Collider2D
- [ ] TowerController是否正确处理宕机状态

**解决方案**:
```
1. 选中防御塔
2. 确认有 Collider2D 组件
3. 运行游戏，让防御塔宕机
4. 在Inspector中观察 Collider2D.enabled 是否变为 false
5. 如果没有变化，检查 TowerController.UpdateBlockingState 方法
```

---

## 📊 调试技巧

### 1. 使用Gizmos可视化

```
Scene视图右上角 > Gizmos 按钮 > 确保开启

你应该看到：
- 红色圆圈：防御塔攻击范围
- 黄色线条：防御塔攻击目标连线
- 橙色圆圈：怪物攻击范围
- 红色区域：厨房区域
```

### 2. 使用Console日志

所有系统都会输出详细日志：
```
"Tower attacked MeatMonster for 10 damage"
"Monster MeatMonster attacked tower for 10 energy damage"
"Monster MeatMonster entered kitchen zone!"
"Level took 1 damage. Health: 10 -> 9"
"Game Over! All health depleted."
```

### 3. 使用Inspector实时查看

运行游戏时：
```
选中防御塔 > 观察：
- TowerEnergySystem.CurrentEnergy
- TowerEnergySystem.CurrentState
- TowerController.currentTarget

选中 LevelHealthSystem > 观察：
- CurrentHealth
```

### 4. 手动触发测试

在运行时的Console输入：
```csharp
// 扣除关卡生命值
LevelHealthSystem.Instance.TakeDamage(1);

// 重置生命值
LevelHealthSystem.Instance.ResetHealth();

// 给防御塔喂食（需要先创建RecipeData）
// towerController.Feed(recipeData);
```

---

## 🎯 优化建议

### 性能优化

1. **调整攻击检测频率**
```
如果怪物很多，可以降低攻击频率：
TowerController.attackInterval = 1.5f  // 从1秒改为1.5秒
MonsterCombatBehavior.attackInterval = 3f  // 从2秒改为3秒
```

2. **使用对象池**
```
如果怪物很多，考虑使用对象池复用怪物GameObject
```

### 平衡性调整

1. **调整伤害值**
```
TowerController.baseAttack = 15  // 增加防御塔攻击力
MonsterCombatBehavior.energyDamage = 5  // 减少怪物能量伤害
```

2. **调整能量衰减**
```
TowerEnergySystem.normalDecayRate = 3  // 从5改为3，衰减变慢
```

3. **调整关卡生命值**
```
LevelHealthSystem.maxHealth = 15  // 从10改为15，更容易
```

---

## 📝 配置清单

完成所有配置后，检查以下内容：

### Managers
- [ ] _LevelManagers/LevelHealthSystem 已创建并配置

### 场景对象
- [ ] KitchenZone 已创建，有触发器和脚本
- [ ] 所有防御塔 Layer = Tower
- [ ] 所有怪物 Layer = Enemy

### 防御塔
- [ ] TowerController 攻击参数已配置
- [ ] Enemy Layer 已选择
- [ ] 攻击范围可见

### 怪物
- [ ] 所有怪物Prefab添加了 MonsterCombatBehavior
- [ ] Tower Layer 已选择
- [ ] 攻击范围可见

### UI
- [ ] LevelHealthPanel 已创建
- [ ] HealthSlider 已配置
- [ ] LevelHealthUI 脚本已添加并连接引用
- [ ] GameOverPanel 初始禁用

### Physics
- [ ] Layer 8: Enemy
- [ ] Layer 9: Tower
- [ ] Layer 10: Kitchen
- [ ] Collision Matrix 已设置

---

## 🎉 完成！

如果所有配置都正确，你现在应该有一个完整可玩的游戏核心循环了！

**按下Play按钮，开始测试吧！** 🚀

---

## 📞 需要帮助？

如果遇到问题：
1. 查看 "常见问题排查" 部分
2. 检查Console的错误日志
3. 确认所有引用都已正确连接
4. 使用Gizmos可视化调试

---

*最后更新: 2025年10月18日*

