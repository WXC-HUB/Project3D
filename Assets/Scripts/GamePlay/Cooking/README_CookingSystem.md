# 烹饪系统实现文档

## 📋 概述

本烹饪系统参考了 `UnityKitchenChaos` 的架构设计，为《美食守护者》实现了完整的烹饪玩法循环。

## 🏗️ 系统架构

### 核心类

1. **食材系统**
   - `IngredientType.cs` - 食材类型枚举
   - `IngredientData.cs` - 食材配置（ScriptableObject）
   - `Ingredient.cs` - 食材运行时对象
   - `IIngredientHolder.cs` - 可持有食材的接口

2. **烹饪系统**
   - `CookingMethodType.cs` - 烹饪方法枚举（炒/炖/炸/烤）
   - `KitchenApplianceBase.cs` - 厨具基类
   - `StirFryAppliance.cs` - 炒锅（铁锅）
   - `StewAppliance.cs` - 炖锅（砂锅）
   - `FryAppliance.cs` - 炸锅
   - `RoastAppliance.cs` - 烤炉

3. **配方和订单系统**
   - `RecipeData.cs` - 配方配置（ScriptableObject）
   - `OrderData.cs` - 订单运行时数据
   - `OrderManager.cs` - 订单管理器（单例）
   - `DeliveryCounter.cs` - 出餐口

4. **存储系统**
   - `PlayerInventory.cs` - 玩家背包（最多6个食材）
   - `StorageCounter.cs` - 仓库（自动存储掉落的食材）

5. **掉落系统**
   - `IngredientDropTable.cs` - 掉落表配置（ScriptableObject）
   - `IngredientDropper.cs` - 掉落组件（附加到怪物上）

## 🎮 使用流程

### 1. 创建食材配置

```
Assets -> Create -> Cooking -> Ingredient Data
```

配置字段：
- `ingredientType` - 食材类型（Tomato, Fish等）
- `ingredientName` - 显示名称
- `icon` - UI图标
- `prefab` - 3D模型
- `allowedMethods` - 允许的烹饪方法

### 2. 创建配方

```
Assets -> Create -> Cooking -> Recipe Data
```

配置字段：
- `recipeID` - 唯一ID
- `recipeName` - 菜名
- `starLevel` - 1-3星级
- `steps` - 烹饪步骤列表
  - 每个步骤包含：食材类型 + 烹饪方法 + 时间
- `energyReward` - 给防御塔的能量值
- `attackBonus` - 攻击力加成

### 3. 设置怪物掉落

```
Assets -> Create -> Cooking -> Ingredient Drop Table
```

配置字段：
- `normalDrops` - 普通掉落列表
- `rareDrops` - 稀有掉落列表
- `rareDropChance` - 稀有掉落触发概率

然后在怪物Prefab上添加 `IngredientDropper` 组件，并指定掉落表。

### 4. 场景设置

#### 仓库
1. 创建空GameObject "Storage"
2. 添加 `StorageCounter` 组件
3. 设置最大容量

#### 厨具
1. 创建空GameObject，如 "StirFryPot"
2. 添加对应组件（`StirFryAppliance`, `StewAppliance`等）
3. 设置 `ingredientHoldPoint` - 食材放置位置
4. 配置烹饪时间和特效

#### 出餐口
1. 创建空GameObject "DeliveryCounter"
2. 添加 `DeliveryCounter` 组件
3. 设置成功/失败特效

#### 订单管理器
1. 在场景中创建空GameObject "OrderManager"
2. 添加 `OrderManager` 组件
3. 设置可用配方池
4. 配置订单生成参数

## 📝 代码示例

### 玩家拾取食材

```csharp
// 检测到食材
Ingredient ingredient = hit.GetComponent<Ingredient>();
PlayerInventory inventory = player.GetComponent<PlayerInventory>();

if (ingredient != null && inventory.AddIngredient(ingredient))
{
    Debug.Log($"Picked up {ingredient.GetIngredientData().ingredientName}");
}
```

### 玩家与厨具交互

```csharp
// 检测到厨具
KitchenApplianceBase appliance = hit.GetComponent<KitchenApplianceBase>();
Ingredient currentIngredient = inventory.GetCurrentIngredient();

if (appliance != null && currentIngredient != null)
{
    if (appliance.AddIngredient(currentIngredient))
    {
        inventory.RemoveIngredient(currentIngredient);
        Debug.Log("Started cooking!");
    }
}
```

### 提交菜品

```csharp
// 完成烹饪后
DeliveryCounter delivery = FindObjectOfType<DeliveryCounter>();
RecipeData recipe = completedDish.GetRecipe();

if (delivery.DeliverDish(recipe))
{
    Debug.Log("Order completed!");
}
```

### 创建订单

```csharp
// 在关卡开始时创建大订单
OrderManager.Instance.CreateLargeOrder(recipeData, targetCount: 3);

// 小订单会自动生成，也可以手动创建
OrderManager.Instance.CreateRandomSmallOrder();
```

## 🎨 事件系统

### 厨具事件

```csharp
appliance.OnCookingProgressChanged += (sender, args) =>
{
    // 更新进度条UI
    progressBar.fillAmount = args.progress;
};

appliance.OnCookingComplete += (sender, ingredient) =>
{
    // 烹饪完成音效
    PlaySound("CookingComplete");
};
```

### 订单事件

```csharp
OrderManager.Instance.OnOrderCreated += (sender, args) =>
{
    // 显示新订单UI
    ShowOrderUI(args.order);
};

OrderManager.Instance.OnOrderCompleted += (sender, args) =>
{
    // 播放完成特效
    PlaySuccessEffect();
};

OrderManager.Instance.OnOrderExpired += (sender, args) =>
{
    // 订单超时惩罚
    ApplyPenalty();
};
```

### 背包事件

```csharp
inventory.OnInventoryChanged += (sender, args) =>
{
    // 更新背包UI
    UpdateInventoryUI(args.ingredients, args.selectedIndex);
};
```

## 🔧 配置表集成

### 与现有CSV系统集成

建议在 `GameConfigDefs.cs` 中添加：

```csharp
public class IngredientConfig : TableDatabase
{
    public int IngredientID;
    public string IngredientName;
    public string PrefabPath;
    public List<int> AllowedMethods; // 1=炒, 2=炖, 3=炸, 4=烤
}

public class RecipeConfig : TableDatabase
{
    public int RecipeID;
    public string RecipeName;
    public int StarLevel;
    public List<int> RequiredIngredients;
    public List<int> CookingMethods;
    public int EnergyReward;
}
```

## ⚠️ 注意事项

1. **性能优化**
   - 食材掉落使用对象池（建议）
   - 烹饪完成的菜品也使用对象池
   
2. **物理设置**
   - 食材需要有 `Rigidbody2D` 和 `Collider2D`
   - 设置合适的Layer用于拾取检测
   
3. **UI集成**
   - 需要创建背包UI显示
   - 订单UI需要显示配方图标和倒计时
   - 厨具进度条UI
   
4. **音效和特效**
   - 每种厨具需要对应的烹饪音效
   - 食材掉落、拾取音效
   - 订单完成的庆祝特效

## 🚀 后续扩展

1. **多人协作**
   - 多个玩家共享仓库
   - 厨具队列管理
   
2. **更多烹饪方法**
   - 切菜（参考 CuttingCounter）
   - 混合调料
   
3. **菜品腐败系统**
   - 烹饪过久会烧焦
   - 食材过期机制
   
4. **技能树**
   - 解锁新配方
   - 提升烹饪速度

## 📞 集成支持

如有问题，请检查：
1. 所有ScriptableObject是否正确配置
2. 厨具的 `ingredientHoldPoint` 是否设置
3. OrderManager是否在场景中且是单例
4. 怪物是否有 `IngredientDropper` 组件

