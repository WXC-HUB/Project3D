# 烹饪系统快速开始指南

## 🚀 5分钟快速部署

### 步骤1：创建食材配置

1. 在Project窗口右键
2. `Create -> Cooking -> Ingredient Data`
3. 命名为 `Tomato_Data`
4. 配置：
   - Ingredient Type: `Tomato`
   - Ingredient Name: "番茄"
   - Allowed Methods: 勾选 `Stir` (炒)

5. 重复以上步骤创建其他食材（鸡蛋、鱼等）

### 步骤2：创建配方

1. 右键 `Create -> Cooking -> Recipe Data`
2. 命名为 `TomatoEgg_Recipe`
3. 配置：
   - Recipe ID: `1`
   - Recipe Name: "番茄炒蛋"
   - Star Level: `2`
   - Steps (添加2个)：
     - Step 1: Ingredient=`Tomato`, CookingMethod=`Stir`, CookingTime=`5`
     - Step 2: Ingredient=`Egg`, CookingMethod=`Stir`, CookingTime=`3`
   - Energy Reward: `100`
   - Attack Bonus: `1.2`

### 步骤3：创建掉落表

1. 右键 `Create -> Cooking -> Ingredient Drop Table`
2. 命名为 `BasicEnemy_Drops`
3. 在Normal Drops中添加：
   - Tomato (Drop Chance: 0.8, Min Count: 1, Max Count: 2)
   - Fish (Drop Chance: 0.5, Min Count: 1, Max Count: 1)

### 步骤4：场景设置

#### A. 仓库
```
1. 创建空GameObject "Storage"
2. 添加组件: StorageCounter
3. 设置: Max Capacity = 99
```

#### B. 炒锅
```
1. 创建空GameObject "StirFryPot"
2. 添加组件: StirFryAppliance
3. 创建子对象 "HoldPoint"
4. 将HoldPoint拖到Ingredient Hold Point字段
5. Base Cooking Time = 5
```

#### C. 出餐口
```
1. 创建空GameObject "DeliveryCounter"
2. 添加组件: DeliveryCounter
3. 设置Interaction Range = 2
```

#### D. 订单管理器
```
1. 创建空GameObject "OrderManager" (放在场景根目录)
2. 添加组件: OrderManager
3. Available Recipes: 拖入你创建的配方
4. Max Small Orders = 3
5. Small Order Interval = 30
6. Small Order Time Limit = 60
```

#### E. 怪物设置
```
1. 选择你的敌人Prefab
2. 添加组件: IngredientDropper
3. Drop Table: 拖入你创建的掉落表
4. Drop Force = 3
5. Auto Add To Storage = true (勾选)
6. Auto Collect Delay = 2
```

### 步骤5：玩家设置

在玩家Prefab上：
```
1. 添加组件: PlayerInventory
2. Max Capacity = 6
3. 创建子对象 "IngredientHoldPoint"
4. 将其拖到Ingredient Hold Point字段
```

### 步骤6：测试

1. 运行游戏
2. 击杀怪物，观察食材掉落
3. 走到Storage附近，食材自动收集
4. 走到炒锅，将食材放入
5. 等待烹饪完成
6. 走到出餐口提交

## 📦 预制配置包

为了快速测试，建议创建以下配置：

### 基础食材（6种）
- 番茄 (Tomato)
- 鸡蛋 (Egg)
- 鱼 (Fish)
- 土豆 (Potato)
- 牛肉 (Beef)
- 洋葱 (Onion)

### 基础配方（3个）
1. **番茄炒蛋** (2星)
   - 番茄 + 鸡蛋 (炒)

2. **薯条** (1星)
   - 土豆 (炸)

3. **烤鱼** (1星)
   - 鱼 (烤)

### 怪物掉落表（2个）
1. **BasicEnemy_Drops**
   - 番茄 80%
   - 鱼 50%

2. **EliteEnemy_Drops**
   - 牛肉 100%
   - 土豆 70%
   - 稀有: 特殊调料 10%

## 🎮 玩家交互代码示例

### 在PlayerInput或PlayerCharacterCtrl中：

```csharp
using Assets.Scripts.Cooking;

// 检测拾取食材
void TryPickupIngredient()
{
    Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f);
    
    foreach (var hit in hits)
    {
        Ingredient ingredient = hit.GetComponent<Ingredient>();
        if (ingredient != null)
        {
            PlayerInventory inventory = GetComponent<PlayerInventory>();
            if (inventory != null && inventory.AddIngredient(ingredient))
            {
                Debug.Log($"Picked up {ingredient.GetIngredientData().ingredientName}");
                break;
            }
        }
    }
}

// 与厨具交互
void TryUseAppliance()
{
    Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 2f);
    
    foreach (var hit in hits)
    {
        KitchenApplianceBase appliance = hit.GetComponent<KitchenApplianceBase>();
        if (appliance != null)
        {
            PlayerInventory inventory = GetComponent<PlayerInventory>();
            Ingredient current = inventory.GetCurrentIngredient();
            
            if (current != null && appliance.AddIngredient(current))
            {
                inventory.RemoveIngredient(current);
                Debug.Log("Started cooking!");
                return;
            }
            
            // 拾取烹饪结果
            if (appliance.HasResult())
            {
                Ingredient result = appliance.TakeResult();
                inventory.AddIngredient(result);
                Debug.Log("Picked up cooked ingredient!");
                return;
            }
        }
    }
}
```

## 🐛 常见问题

### Q: 食材掉落后立即消失？
A: 检查 `IngredientDropper` 的 `Auto Add To Storage` 是否勾选，以及 `Auto Collect Delay` 是否过短。

### Q: 厨具无法放入食材？
A: 确保食材的 `Allowed Methods` 包含该厨具的烹饪方法。

### Q: 订单一直不生成？
A: 确保 `OrderManager` 在场景中且 `Available Recipes` 列表不为空。

### Q: 提交菜品没反应？
A: 检查是否有匹配的订单，使用 `Test: Print All Orders` 查看当前订单。

### Q: 玩家背包满了怎么办？
A: 可以走到Storage存入，或者实现"丢弃"功能。

## 📊 性能建议

1. **食材对象池**：建议使用对象池管理食材生成
2. **限制最大掉落数**：避免大量怪物同时死亡导致卡顿
3. **自动清理**：超时未拾取的食材会自动销毁（30秒）

## 🎨 下一步

1. 创建UI界面显示订单和背包
2. 添加音效和特效
3. 创建更多配方
4. 实现防御塔"喂食"功能
5. 添加配方解锁系统

## 📞 需要帮助？

查看 `README_CookingSystem.md` 获取完整文档
查看 `CookingSystemExample.cs` 获取代码示例

