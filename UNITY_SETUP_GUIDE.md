# Unity快速配置指南

> 《美食守护者》项目Unity编辑器配置步骤

---

## 📋 前置检查

确认以下文件已存在：
- ✅ 所有C#脚本（36个核心脚本）
- ✅ 现有的CharacterCtrlBase、LevelManager等基础系统
- ✅ Spine动画系统已集成
- ✅ NodeCanvas行为树系统已集成

---

## 🎯 步骤1：创建ScriptableObject数据

### 1.1 创建文件夹结构
```
Assets/Resources/
├── ScriptableObjects/
│   ├── Ingredients/           # 食材数据
│   ├── Recipes/               # 配方数据
│   ├── Orders/                # 订单数据
│   ├── IngredientDropTables/  # 掉落表
│   └── Waves/                 # 波次数据
└── Prefabs/
    ├── Appliances/            # 厨具预制体
    ├── Ingredients/           # 食材预制体
    ├── Counters/              # 柜台预制体
    ├── UI/                    # UI预制体
    └── Towers/                # 防御塔预制体
```

### 1.2 创建食材数据
1. 在Project窗口右键: `Create > Cooking > Ingredient Data`
2. 创建14种食材（参考设计文档）：
   ```
   - 生肉          (Meat, Raw)
   - 蔬菜          (Vegetable, Raw)
   - 调料          (Seasoning, Raw)
   - 面粉          (Flour, Raw)
   - 炒肉          (Meat, Cooked)
   - 炒菜          (Vegetable, Cooked)
   - 等等...
   ```

3. 配置每个食材：
   ```
   - Ingredient Name: "生肉"
   - Ingredient Type: Meat
   - State: Raw / Cooked / Processed
   - Prefab: (食材的3D/2D模型)
   - Icon: (食材图标Sprite)
   ```

### 1.3 创建配方数据
1. 在Project窗口右键: `Create > Cooking > Recipe Data`
2. 创建菜品配方（示例）：
   
   **配方：炒肉**
   ```
   - Recipe Name: "炒肉"
   - Required Ingredients: [生肉]
   - Output Ingredient: 炒肉
   - Cooking Method: StirFry
   - Cooking Time: 3.0
   - Star Level: 2
   - Energy Value: 20
   - Icon: (菜品图标)
   ```
   
   **配方：宫保鸡丁**
   ```
   - Recipe Name: "宫保鸡丁"
   - Required Ingredients: [炒肉, 炒菜, 调料]
   - Cooking Method: StirFry
   - Cooking Time: 5.0
   - Star Level: 4
   - Energy Value: 50
   ```

### 1.4 创建订单数据
1. 在Project窗口右键: `Create > Cooking > Order Data`
2. 创建大订单（示例）：
   ```
   - Order Type: Large
   - Recipe: 宫保鸡丁
   - Target Count: 10
   - Time Limit: 0 (无限时)
   - Reward Coins: 1000
   ```

3. 小订单由OrderManager运行时自动生成，无需手动创建

### 1.5 创建掉落表
1. 在Project窗口右键: `Create > Cooking > Ingredient Drop Table`
2. 为每种怪物创建掉落表：
   
   **肉块怪掉落表**
   ```
   - Ingredient: 生肉
   - Drop Chance: 80%
   - Min Count: 1
   - Max Count: 2
   ```
   
   **蔬菜怪掉落表**
   ```
   - Ingredient: 蔬菜
   - Drop Chance: 80%
   - Min Count: 1
   - Max Count: 2
   ```

### 1.6 创建波次数据
1. 在Project窗口右键: `Create > Wave > Wave Data`
2. 创建各个波次配置：
   
   **Wave 1**
   ```
   - Wave Index: 0
   - Preparation Time: 15
   - Interval Time: 5
   - Enemy Types: [MeatMonster]
   - Spawn Counts: [10]
   - Spawn Intervals: [2.0]
   ```
   
   **Wave 2**
   ```
   - Wave Index: 1
   - Enemy Types: [MeatMonster, VegetableMonster]
   - Spawn Counts: [8, 8]
   - Spawn Intervals: [1.5, 1.5]
   ```

---

## 🏗️ 步骤2：创建预制体

### 2.1 厨具预制体

**炒锅 (StirFryAppliance)**
1. 创建空GameObject: `StirFryAppliance`
2. 添加组件：
   - `StirFryAppliance` 脚本
   - `SpriteRenderer` 或 3D模型组件
   - `InteractableAppliance` 脚本
3. 配置参数：
   ```
   StirFryAppliance:
   - Appliance Name: "炒锅"
   - Cooking Method: StirFry
   - Available Recipes: [配置所有炒菜配方]
   ```
4. 保存为Prefab

**重复以上步骤创建**：
- StewAppliance (炖锅)
- FryAppliance (炸锅)  
- RoastAppliance (烤箱)

### 2.2 柜台预制体

**仓库 (StorageCounter)**
1. 创建GameObject: `StorageCounter`
2. 添加组件：
   - `StorageCounter` 脚本
   - `InteractableStorage` 脚本
   - 模型或Sprite
3. 配置InteractionPrompt: "E - 访问仓库"

**出餐口 (DeliveryCounter)**
1. 创建GameObject: `DeliveryCounter`
2. 添加组件：
   - `DeliveryCounter` 脚本
   - `InteractableDelivery` 脚本
3. 配置：
   ```
   InteractableDelivery:
   - Delivery Counter: (自引用)
   ```

### 2.3 食材预制体模板

**通用食材Prefab**
1. 创建GameObject: `IngredientPrefab`
2. 添加组件：
   - `Ingredient` 脚本
   - `InteractableIngredient` 脚本
   - `SpriteRenderer`
   - `Rigidbody2D` (或 Rigidbody for 3D)
   - `Collider2D` (或 Collider for 3D)
3. 配置：
   ```
   Ingredient:
   - Ingredient Data: (在运行时由IngredientDropper设置)
   
   InteractableIngredient:
   - Ingredient: (自引用)
   ```

### 2.4 怪物Prefab修改

为所有怪物Prefab添加：
1. 找到现有的怪物Prefab（如 `MeatMonster`）
2. 添加 `IngredientDropper` 组件
3. 配置：
   ```
   IngredientDropper:
   - Drop Tables: [肉块怪掉落表]
   - Ingredient Prefab: IngredientPrefab
   - Drop Position Offset: (0, 0.5, 0)
   ```

### 2.5 防御塔Prefab修改

为所有防御塔Prefab添加能量系统：
1. 找到现有的防御塔Prefab
2. 添加 `TowerEnergySystem` 组件
3. 添加 `TowerController` 组件
4. 添加 `InteractableTower` 组件
5. 配置：
   ```
   TowerEnergySystem:
   - Max Normal Energy: 100
   - Max Bonus Energy: 100
   - Normal Decay Rate: 5 (每秒)
   - Bonus Decay Rate: 10
   
   TowerController:
   - Tower Energy System: (自引用)
   - Character Ctrl: (引用CharacterCtrlBase组件)
   
   InteractableTower:
   - Tower Controller: (自引用)
   ```

### 2.6 玩家Prefab修改

为玩家角色添加新系统：
1. 找到玩家Prefab（`PlayerCharacterCtrl`）
2. 添加以下组件：
   - `PlayerInventory` - 背包系统
   - `InteractionSystem` - 交互系统
3. 配置：
   ```
   PlayerInventory:
   - Max Capacity: 5
   
   InteractionSystem:
   - Interaction Range: 2.0
   - Interaction Layer: (设置为包含所有可交互对象的层)
   - Interact Key: E
   ```

---

## 🎨 步骤3：UI配置

### 3.1 创建主Canvas

1. 创建Canvas GameObject:
   ```
   Hierarchy右键 > UI > Canvas
   名称: GameHUD
   ```

2. 配置Canvas:
   ```
   Render Mode: Screen Space - Overlay
   Canvas Scaler:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920x1080
   - Match: 0.5
   ```

3. 确保场景中有EventSystem

### 3.2 创建UI面板

#### 顶部面板 (TopPanel)
```
Canvas/TopPanel (Anchor: Top-Stretch)
├── WaveInfoPanel (Left)
│   ├── WaveNumberText (Text: "波次 1/10")
│   ├── MonsterCountText (Text: "怪物: 0/20")
│   ├── PhaseText (Text: "准备阶段")
│   ├── PhaseTimerSlider (Slider)
│   ├── PreparationPanel (默认隐藏)
│   └── IntervalPanel (默认隐藏)
│   └── [WaveInfoUI脚本]
│
└── TowerEnergyContainer (Right)
    └── (为每个防御塔动态创建TowerEnergyUI)
```

**WaveInfoPanel配置**:
1. 添加 `WaveInfoUI` 组件
2. 连接UI引用：
   ```
   - Wave Number Text: WaveNumberText
   - Monster Count Text: MonsterCountText
   - Phase Text: PhaseText
   - Phase Timer Slider: PhaseTimerSlider
   - Preparation Panel: PreparationPanel
   - Interval Panel: IntervalPanel
   ```

#### 底部面板 (BottomPanel)
```
Canvas/BottomPanel (Anchor: Bottom-Stretch)
├── InventoryPanel (Left)
│   ├── Slot1 [InventorySlotUI]
│   ├── Slot2 [InventorySlotUI]
│   ├── Slot3 [InventorySlotUI]
│   ├── Slot4 [InventorySlotUI]
│   ├── Slot5 [InventorySlotUI]
│   ├── CapacityText (Text: "0/5")
│   └── [InventoryUI脚本]
│
├── InteractionPrompt (Center)
│   ├── PromptPanel [CanvasGroup]
│   │   ├── KeyImage (Image: E键图标)
│   │   └── PromptText (Text: "E - 交互")
│   └── [InteractionPromptUI脚本]
│
└── OrderPanel (Right)
    ├── LargeOrderContainer
    ├── SmallOrderContainer
    ├── VictoryPanel (默认隐藏)
    └── [OrderUI脚本]
```

**InventoryPanel配置**:
1. 创建5个InventorySlotUI：
   ```
   每个Slot结构：
   - IconImage (Image, 默认隐藏)
   - EmptyIndicator (GameObject: "空")
   - SelectedFrame (Image: 黄色边框, 默认隐藏)
   - [InventorySlotUI脚本]
   ```

2. 添加 `InventoryUI` 组件到InventoryPanel
3. 配置：
   ```
   - Slots: [拖入5个InventorySlotUI]
   - Capacity Text: CapacityText
   - Target Inventory: (在运行时设置，或拖入玩家的PlayerInventory)
   ```

**OrderPanel配置**:
1. 创建 `OrderItemPrefab`:
   ```
   OrderItem (带OrderUIItem脚本):
   ├── IconImage (Image)
   ├── RecipeNameText (Text)
   ├── ProgressText (Text)
   ├── TimerSlider (Slider)
   ├── WarningEffect (默认隐藏)
   └── StarContainer
       ├── Star1 (Image)
       ├── Star2 (Image)
       ├── Star3 (Image)
       ├── Star4 (Image)
       └── Star5 (Image)
   ```

2. 配置OrderUI：
   ```
   - Small Order Container: SmallOrderContainer
   - Large Order Container: LargeOrderContainer
   - Order Item Prefab: OrderItemPrefab
   - Victory Panel: VictoryPanel
   ```

### 3.3 创建世界空间UI（厨具烹饪进度）

为每个厨具创建进度条：
1. 在厨具Prefab下创建子物体:
   ```
   StirFryAppliance
   └── CookingProgressCanvas (World Space Canvas)
       ├── Canvas设置:
       │   - Render Mode: World Space
       │   - Width: 200, Height: 50
       │   - Scale: 0.01, 0.01, 0.01
       │   - Position Y: 偏移到厨具上方
       │
       └── ProgressUI [CookingProgressUI脚本]
           ├── ProgressPanel
           │   ├── BackgroundImage
           │   ├── ProgressSlider
           │   └── IngredientNameText
           └── 配置:
               - Target Appliance: (父物体的KitchenApplianceBase)
               - World Target: (父物体的Transform)
   ```

### 3.4 创建防御塔能量槽UI Prefab

创建一个可复用的TowerEnergyUI Prefab：
```
TowerEnergyUIPrefab
├── BackgroundImage
├── FillImage (Image, Fill Type: Filled)
├── BonusFillImage (Image, Fill Type: Filled, 不同颜色)
├── PercentText (Text: "100%")
├── StateText (Text: "正常")
└── WarningIcon (默认隐藏)
└── [TowerEnergyUI脚本]
```

在游戏开始时，为每个防御塔动态创建此UI并设置目标。

---

## 🎮 步骤4：场景配置

### 4.1 在现有关卡中放置对象

在 `Assets/Scenes/` 的关卡场景中：

1. **放置厨具**（烹饪区域）
   - 拖入4个厨具Prefab（炒/炖/炸/烤）
   - 放置在靠近玩家基地的安全区域
   - 确保玩家可以轻松访问

2. **放置仓库**
   - 拖入StorageCounter Prefab
   - 放置在厨具附近
   - 方便玩家快速存取食材

3. **放置出餐口**
   - 拖入DeliveryCounter Prefab
   - 放置在玩家基地中心位置

4. **配置防御塔**
   - 修改现有的防御塔位置（如果需要）
   - 确保每个防御塔都添加了TowerEnergySystem等组件

### 4.2 创建管理器对象

在Hierarchy中创建 `_Managers` 空对象，添加子对象：

```
_Managers
├── OrderManager (添加OrderManager脚本)
│   └── 配置:
│       - Available Recipes: [拖入所有配方ScriptableObject]
│       - Small Order Interval: 30
│       - Max Small Orders: 3
│       - Large Orders: [手动添加大订单数据]
│
└── WaveManager (添加WaveManager脚本)
    └── 配置:
        - Wave Datas: [拖入所有波次ScriptableObject]
        - Spawn Roots: [拖入场景中的所有生成点]
        - Start On Play: true
```

### 4.3 连接UI到系统

在GameHUD Canvas上：

1. 找到InventoryUI，设置：
   ```
   Target Inventory: (拖入玩家的PlayerInventory组件)
   ```

2. 找到InteractionPromptUI，设置：
   ```
   Target: (拖入玩家的InteractionSystem组件)
   ```

3. WaveInfoUI和OrderUI会自动连接到单例Manager，无需手动设置

---

## 🔧 步骤5：测试与调试

### 5.1 测试烹饪系统
1. 运行游戏
2. 击杀怪物，观察是否掉落食材
3. 按E键拾取食材
4. 走到厨具旁，按E放入食材
5. 等待烹饪完成，按E取出成品
6. 观察烹饪进度UI是否正常显示

### 5.2 测试订单系统
1. 运行游戏后，观察订单UI是否出现
2. 完成一个菜品，拿着走到出餐口
3. 按E提交，观察订单是否完成
4. 观察大订单进度是否更新

### 5.3 测试防御塔能量系统
1. 观察防御塔能量槽是否在缓慢下降
2. 拿着菜品走到防御塔旁
3. 按E喂食，观察能量是否增加
4. 观察不同能量状态的颜色变化
5. 等待能量耗尽，观察防御塔是否停止工作

### 5.4 测试波次系统
1. 运行游戏，观察准备阶段倒计时
2. 15秒后观察怪物是否开始生成
3. 击杀所有怪物，观察是否进入间隔阶段
4. 5秒后观察是否开始下一波

### 5.5 测试交互系统
1. 走到各种可交互对象旁边
2. 观察是否显示交互提示
3. 按E键测试各种交互

---

## ⚙️ 步骤6：优化配置

### 6.1 平衡性调整

根据测试结果调整参数：

**能量系统**:
```
TowerEnergySystem:
- Normal Decay Rate: 3-10 (根据游戏节奏调整)
- 状态修改器倍数：可在TowerController中调整
```

**订单系统**:
```
OrderManager:
- Small Order Interval: 20-60秒
- 小订单时限: 60-180秒
- 奖励金币: 根据难度调整
```

**波次系统**:
```
WaveData:
- Preparation Time: 10-20秒
- Interval Time: 5-10秒
- 怪物数量: 根据难度递增
```

**烹饪时间**:
```
RecipeData:
- Cooking Time: 2-10秒（简单菜品短，复杂菜品长）
```

### 6.2 性能优化

1. **对象池**（如果需要）:
   - 食材预制体
   - 订单UI项
   - 粒子特效

2. **UI更新优化**:
   - 所有UI已使用事件驱动，无需优化

3. **碰撞检测优化**:
   - 确保Layer和LayerMask设置正确
   - InteractionSystem只检测Interactable层

---

## 📊 检查清单

在正式开始游戏前，确认以下项：

### 数据资源
- [ ] 至少创建了基础食材（生肉、蔬菜、调料、面粉）
- [ ] 至少创建了3个配方
- [ ] 创建了1个大订单
- [ ] 为每种怪物创建了掉落表
- [ ] 创建了至少3个波次数据

### 预制体
- [ ] 4种厨具预制体完成
- [ ] 仓库和出餐口预制体完成
- [ ] 食材预制体模板完成
- [ ] 怪物添加了IngredientDropper组件
- [ ] 防御塔添加了TowerEnergySystem等组件
- [ ] 玩家添加了PlayerInventory和InteractionSystem组件

### UI
- [ ] GameHUD Canvas创建完成
- [ ] 所有UI面板配置完成
- [ ] 世界空间UI配置完成
- [ ] UI脚本正确连接到目标

### 场景
- [ ] 厨具已放置在场景中
- [ ] 仓库和出餐口已放置
- [ ] OrderManager和WaveManager已配置
- [ ] 玩家和防御塔在正确位置

### 测试
- [ ] 烹饪系统测试通过
- [ ] 订单系统测试通过
- [ ] 能量系统测试通过
- [ ] 波次系统测试通过
- [ ] 交互系统测试通过

---

## 🚨 常见问题排查

### 问题1: 食材不掉落
- 检查IngredientDropper是否添加到怪物Prefab
- 检查Drop Table是否正确配置
- 检查IngredientPrefab是否设置
- 查看Console是否有错误日志

### 问题2: 无法拾取食材
- 检查InteractionSystem是否添加到玩家
- 检查食材的Layer是否在InteractionLayer中
- 检查InteractionRange是否足够大
- 检查PlayerInventory是否已满

### 问题3: 订单不生成
- 检查OrderManager的AvailableRecipes是否配置
- 检查OrderManager是否在场景中且Active
- 查看Console错误信息

### 问题4: 防御塔能量不变化
- 检查TowerEnergySystem是否正确添加
- 检查TowerController是否引用了EnergySystem
- 检查是否调用了StartEnergyDecay()

### 问题5: UI不显示
- 检查Canvas的Render Mode
- 检查UI元素的Anchor和Position
- 检查目标引用是否正确设置
- 检查Canvas的Sort Order

---

## 📝 下一步

完成Unity配置后：
1. 进行完整的游戏流程测试
2. 根据测试结果调整平衡性参数
3. 添加音效和粒子特效
4. 优化UI视觉效果
5. 制作更多关卡和波次配置
6. 添加更多菜品配方

---

**祝配置顺利！有任何问题请查看各系统的详细文档。** 🎉

