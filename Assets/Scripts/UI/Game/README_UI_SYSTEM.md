# UI系统文档

## 📋 概述

本UI系统为《美食守护者》提供完整的游戏界面支持，包括6个主要UI模块。所有UI组件都基于事件驱动架构，与游戏系统解耦。

---

## 🎨 UI组件列表

### 1. TowerEnergyUI - 防御塔能量槽UI
**位置**: `Assets/Scripts/UI/Game/TowerEnergyUI.cs`

**功能**:
- 显示防御塔当前能量百分比
- 四色能量槽（亢奋/正常/衰弱/宕机）
- 额外能量槽显示（喂食奖励能量）
- 状态文本提示
- 低能量警告图标

**使用方法**:
```csharp
TowerEnergyUI ui = GetComponent<TowerEnergyUI>();
TowerEnergySystem tower = someTower.GetComponent<TowerEnergySystem>();
ui.SetTarget(tower);
```

**所需UI元素**:
- `fillImage`: Image - 能量填充图片
- `bonusFillImage`: Image - 额外能量填充图片
- `percentText`: Text - 百分比文本
- `stateText`: Text - 状态文本
- `warningIcon`: GameObject - 警告图标

---

### 2. WaveInfoUI - 波次信息UI
**位置**: `Assets/Scripts/UI/Game/WaveInfoUI.cs`

**功能**:
- 显示当前波次编号（如 "波次 3/10"）
- 显示怪物击杀进度（如 "怪物: 15/20"）
- 显示当前阶段（准备/战斗中/波次间隔）
- 阶段倒计时进度条
- 准备阶段和间隔阶段的特殊提示面板

**使用方法**:
```csharp
// WaveManager是单例，UI会自动连接
WaveInfoUI ui = GetComponent<WaveInfoUI>();
```

**所需UI元素**:
- `waveNumberText`: Text - 波次编号
- `monsterCountText`: Text - 怪物计数
- `phaseText`: Text - 阶段文本
- `phaseTimerSlider`: Slider - 阶段计时器
- `preparationPanel`: GameObject - 准备阶段面板
- `intervalPanel`: GameObject - 间隔阶段面板

---

### 3. OrderUI - 订单显示UI
**位置**: `Assets/Scripts/UI/Game/OrderUI.cs`

**功能**:
- 显示所有活跃的小订单和大订单
- 订单配方图标和名称
- 大订单完成进度（如 "2/5"）
- 订单倒计时进度条
- 快超时时的警告效果
- 订单星级显示
- 全部大订单完成时的胜利面板

**使用方法**:
```csharp
// OrderManager是单例，UI会自动连接
// 订单会自动创建和移除UI项
```

**所需UI元素**:
- `smallOrderContainer`: Transform - 小订单容器
- `largeOrderContainer`: Transform - 大订单容器
- `orderItemPrefab`: GameObject - 订单项Prefab
- `victoryPanel`: GameObject - 胜利面板

**OrderItemPrefab结构**（需要包含OrderUIItem组件）:
- `iconImage`: Image - 配方图标
- `recipeNameText`: Text - 配方名称
- `progressText`: Text - 进度文本
- `timerSlider`: Slider - 倒计时
- `warningEffect`: GameObject - 警告特效
- `starIcons`: Image[] - 星级图标数组

---

### 4. InventoryUI - 背包UI
**位置**: `Assets/Scripts/UI/Game/InventoryUI.cs`

**功能**:
- 显示玩家背包中的所有食材
- 高亮当前选中的食材
- 显示背包容量（如 "3/5"）
- 空槽位提示

**使用方法**:
```csharp
InventoryUI ui = GetComponent<InventoryUI>();
PlayerInventory inventory = player.GetComponent<PlayerInventory>();
ui.SetTarget(inventory);
```

**所需UI元素**:
- `slots`: InventorySlotUI[] - 槽位数组
- `capacityText`: Text - 容量文本

**InventorySlotUI结构**（每个槽位需要包含InventorySlotUI组件）:
- `iconImage`: Image - 食材图标
- `countText`: Text - 数量文本（暂不使用）
- `selectedFrame`: GameObject - 选中框
- `emptyIndicator`: GameObject - 空槽提示

---

### 5. InteractionPromptUI - 交互提示UI
**位置**: `Assets/Scripts/UI/Game/InteractionPromptUI.cs`

**功能**:
- 显示可交互对象的提示文本（如 "E - 拾取食材"）
- 显示交互按键图标
- 淡入淡出动画
- 自动跟随交互目标

**使用方法**:
```csharp
InteractionPromptUI ui = GetComponent<InteractionPromptUI>();
InteractionSystem system = player.GetComponent<InteractionSystem>();
ui.SetTarget(system);
```

**所需UI元素**:
- `promptPanel`: GameObject - 提示面板（需包含CanvasGroup）
- `promptText`: Text - 提示文本
- `keyImage`: Image - 按键图标
- `eKeySprite`: Sprite - E键图标资源

---

### 6. CookingProgressUI - 烹饪进度UI
**位置**: `Assets/Scripts/UI/Game/CookingProgressUI.cs`

**功能**:
- 显示厨具的烹饪进度条
- 显示当前烹饪的食材名称
- 快完成时进度条变色
- 支持世界空间UI（跟随厨具位置）
- 完成时的特效提示

**使用方法**:
```csharp
// 方式1：屏幕空间UI
CookingProgressUI ui = GetComponent<CookingProgressUI>();
KitchenApplianceBase appliance = someAppliance.GetComponent<KitchenApplianceBase>();
ui.SetTarget(appliance);

// 方式2：世界空间UI（推荐）
CookingProgressUI ui = GetComponent<CookingProgressUI>();
ui.SetTarget(appliance, appliance.transform); // 会跟随厨具位置
```

**所需UI元素**:
- `progressSlider`: Slider - 进度条
- `fillImage`: Image - 进度填充图片
- `progressPanel`: GameObject - 进度面板
- `ingredientNameText`: Text - 食材名称

---

## 🎮 快速集成指南

### 步骤1：创建UI Canvas
```
1. 在场景中创建 Canvas (Screen Space - Overlay)
2. 添加 EventSystem
3. 设置合适的 Canvas Scaler 参数
```

### 步骤2：创建各个UI面板
```
在Canvas下创建以下面板：
- TowerEnergyPanel (挂载 TowerEnergyUI)
- WaveInfoPanel (挂载 WaveInfoUI)
- OrderListPanel (挂载 OrderUI)
- InventoryPanel (挂载 InventoryUI)
- InteractionPrompt (挂载 InteractionPromptUI)
```

### 步骤3：创建世界空间UI Canvas（可选）
```
为每个厨具创建单独的世界空间Canvas：
- Canvas (World Space)
- 挂载 CookingProgressUI
- 设置 worldTarget 为厨具的 Transform
```

### 步骤4：设置目标引用
```csharp
// 在Start或适当的时机设置目标
public PlayerCharacterCtrl playerController;

void Start() {
    // 连接背包UI
    inventoryUI.SetTarget(playerController.GetComponent<PlayerInventory>());
    
    // 连接交互UI
    interactionPromptUI.SetTarget(playerController.GetComponent<InteractionSystem>());
    
    // 防御塔UI在创建防御塔时动态设置
    foreach (var tower in towers) {
        CreateTowerUI(tower);
    }
}
```

---

## 🎨 UI布局建议

### 屏幕布局
```
+--------------------------------------------------+
|  波次信息  怪物: 15/20                 能量槽 x3  |
|  波次 3/10                                        |
+--------------------------------------------------+
|                                                   |
|                                                   |
|                 游戏区域                          |
|                                                   |
|                                                   |
+--------------------------------------------------+
| [背包]  [食材1] [食材2] ...              订单列表 |
|         3/5                              大订单1  |
| [E - 拾取食材]                           小订单1  |
+--------------------------------------------------+
```

### UI层级
```
Canvas (Screen Space)
├── TopPanel
│   ├── WaveInfoUI (左上)
│   └── TowerEnergyUI (右上，可有多个)
├── BottomPanel
│   ├── InventoryUI (左下)
│   ├── InteractionPromptUI (左下中间)
│   └── OrderUI (右下)
└── VictoryPanel (全屏，默认隐藏)
```

---

## 📊 事件系统

所有UI都通过事件与游戏系统通信，无需手动Update：

| 游戏系统 | 触发的事件 | 监听的UI |
|---------|-----------|---------|
| TowerEnergySystem | OnEnergyChanged, OnStateChanged | TowerEnergyUI |
| WaveManager | OnWaveStarted, OnPhaseChanged | WaveInfoUI |
| OrderManager | OnOrderCreated, OnOrderCompleted | OrderUI |
| PlayerInventory | OnInventoryChanged | InventoryUI |
| InteractionSystem | OnTargetChanged | InteractionPromptUI |
| KitchenApplianceBase | OnCookingProgressChanged | CookingProgressUI |

---

## 🔧 自定义与扩展

### 修改颜色主题
在Inspector中调整各UI组件的颜色参数：
- TowerEnergyUI: 四种状态颜色
- CookingProgressUI: normalColor, warningColor
- WaveInfoUI: 可在代码中添加颜色参数

### 添加动画效果
```csharp
// 在OnWaveStarted等事件处理中添加：
public Animator waveStartAnimator;

void OnWaveStarted(...) {
    waveStartAnimator.SetTrigger("WaveStart");
    // ... 原有逻辑
}
```

### 添加音效
```csharp
// 在关键事件处理中添加：
public AudioClip orderCompleteSound;
private AudioSource audioSource;

void OnOrderCompleted(...) {
    audioSource.PlayOneShot(orderCompleteSound);
    // ... 原有逻辑
}
```

---

## ⚠️ 注意事项

1. **性能优化**: 订单UI使用对象池可以提升性能（当订单数量很多时）
2. **分辨率适配**: 确保Canvas Scaler设置正确，建议使用 Scale With Screen Size
3. **世界空间UI**: CookingProgressUI使用世界空间时，确保Canvas的Render Camera设置正确
4. **事件清理**: 所有UI都在OnDestroy中正确取消订阅事件，避免内存泄漏

---

## 🐛 常见问题

**Q: UI不更新？**
A: 检查是否正确设置了Target引用，以及对应的Manager是否正常工作。

**Q: 世界空间UI显示不正确？**
A: 确保设置了mainCamera和worldTarget，并且Canvas的Render Mode为World Space。

**Q: 订单UI项目无法显示？**
A: 检查orderItemPrefab是否正确设置，并且包含OrderUIItem组件。

**Q: 背包UI槽位数量不够？**
A: 在Inspector中增加InventorySlotUI数组的大小，或在代码中动态创建。

---

## 📝 待改进功能

1. 添加更多动画效果（如数字跳动、进度条缓动）
2. 订单UI对象池优化
3. 更丰富的视觉特效（粒子效果、屏幕震动等）
4. 多语言支持
5. 主题切换功能

---

## 📚 相关文档

- [烹饪系统文档](../GamePlay/Cooking/README_CookingSystem.md)
- [防御塔能量系统文档](../GamePlay/Tower/README.md)
- [波次系统文档](../GamePlay/Wave/README.md)
- [交互系统文档](../GamePlay/Interaction/README.md)

