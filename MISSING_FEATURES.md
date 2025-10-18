# 《美食守护者》待实现功能清单

> 基于策划案分析，代码层面已实现的功能 vs 还需实现的功能

---

## ✅ 已完成的核心系统（100%）

### 1. 烹饪系统 ✅
- [x] 4种厨具（炒/炖/炸/烤）
- [x] 烹饪读条进度
- [x] 多步骤配方系统
- [x] 仓库分类存储
- [x] 玩家背包（最多5个）
- [x] 出餐口系统

### 2. 订单系统 ✅
- [x] 大订单（多阶段完成）
- [x] 小订单（自动生成+倒计时）
- [x] 配方验证和匹配
- [x] 星级系统

### 3. 食材系统 ✅
- [x] 怪物掉落食材
- [x] E键拾取
- [x] 自动归类到仓库

### 4. 防御塔能量系统 ✅
- [x] 四段式能量槽（亢奋/正常/衰弱/宕机）
- [x] 喂食机制
- [x] 自动能量衰减
- [x] 状态属性修改

### 5. 波次系统 ✅
- [x] 准备阶段（15秒）
- [x] 波次间隔（5秒）
- [x] 怪物生成控制

### 6. 交互系统 ✅
- [x] E键交互
- [x] 范围检测
- [x] 多种交互对象

### 7. UI系统 ✅
- [x] 能量槽UI
- [x] 波次信息UI
- [x] 订单UI
- [x] 背包UI
- [x] 交互提示UI
- [x] 烹饪进度UI

---

## ✅ 已完成的游戏逻辑功能（最高优先级）

### ✅ 关卡生命值系统
**策划案位置**: 2.3.1.2 生命值  
**实现文件**: `/Assets/Scripts/GamePlay/Level/LevelHealthSystem.cs`

**已实现**:
- [x] 关卡拥有初始生命值（如：10点）
- [x] 怪物进入厨房区域时扣除对应生命值
  - 小怪：-1点
  - 精英怪：-3点
  - BOSS：-5点
- [x] 生命值为0时判负
- [x] UI显示当前生命值（`LevelHealthUI.cs`）

### ✅ 怪物进入厨房检测系统
**策划案位置**: 2.3.1.6.3 怪物死亡  
**实现文件**: `/Assets/Scripts/GamePlay/Level/KitchenZoneTrigger.cs`

**已实现**:
- [x] 检测怪物进入厨房区域
- [x] 扣除关卡生命值
- [x] 怪物直接死亡且不掉落食材
- [x] 计入波次击杀数

### ✅ 防御塔自动攻击系统
**策划案位置**: 2.4 单位设定  
**实现文件**: `/Assets/Scripts/GamePlay/Tower/TowerController.cs` (扩展)

**已实现**:
- [x] 防御塔自动攻击范围内的怪物
- [x] 攻击间隔（攻击速度）
- [x] 伤害计算（攻击力 x 技能倍率）
- [x] 不同能量状态有不同属性加成

### ✅ 怪物攻击防御塔系统
**策划案位置**: 2.3.1.6.2 怪物攻击  
**实现文件**: `/Assets/Scripts/GamePlay/AI/MonsterCombatBehavior.cs`

**已实现**:
- [x] 怪物在攻击范围内攻击防御塔
- [x] 攻击扣除防御塔能量（不是血量！）
- [x] 能量小于等于0，防御塔进入宕机状态
- [x] 宕机时失去阻挡能力

---

## ❌ 待实现的游戏逻辑功能

### 🏗️ 防御塔部署系统（高优先级）
**策划案位置**: 2.3.1.5.1 防御塔

**需求**:
- [ ] 玩家可在可部署区域部署防御塔
- [ ] 部署时显示防御塔选择界面
- [ ] 部署后进入建造读条状态
- [ ] 读条期间不会被攻击
- [ ] 建造完成后进入完整状态
- [ ] 可以移动已部署的防御塔（策划案提到）

**实现建议**:
```csharp
// 新建文件：/Assets/Scripts/GamePlay/Tower/TowerDeploymentSystem.cs
public class TowerDeploymentSystem : MonoBehaviour
{
    public void ShowDeploymentUI(Vector3 position) { }
    public void DeployTower(TowerData towerData, Vector3 position) { }
    public void StartBuildingTower(TowerController tower) { }
}

// 新建文件：/Assets/Scripts/GamePlay/Tower/TowerBuildingState.cs
public enum TowerBuildingState {
    Building,    // 建造中（读条）
    Complete,    // 建造完成
    Idle         // 待命
}
```

---

### ⏱️ 食材自动消失机制
**策划案位置**: 2.3.1.7 素材

**需求**:
- [ ] 食材掉落后一段时间自动消失（如30秒）
- [ ] 消失前有闪烁提示

**实现建议**:
```csharp
// 扩展 Ingredient.cs
public class Ingredient : MonoBehaviour
{
    [Header("自动消失")]
    public float disappearTime = 30f;
    public float blinkStartTime = 25f;
    
    private float spawnTime;
    
    void Start() {
        spawnTime = Time.time;
    }
    
    void Update() {
        float elapsed = Time.time - spawnTime;
        
        if (elapsed >= disappearTime) {
            Destroy(gameObject);
        }
        else if (elapsed >= blinkStartTime) {
            // 闪烁效果
            float blinkSpeed = 5f;
            GetComponent<SpriteRenderer>().enabled = Mathf.Sin(Time.time * blinkSpeed) > 0;
        }
    }
}
```

---

### 🍳 烹饪完成后的等待时间机制
**策划案位置**: 2.3.1.4.2 厨具

**需求**:
- [ ] 烹饪完成后有等待时间（倒计时）
- [ ] 倒计时结束前可拾取
- [ ] 倒计时结束后菜品失效消失

**实现建议**:
```csharp
// 扩展 KitchenApplianceBase.cs
[Header("完成后等待")]
public float pickupWaitTime = 10f;

private void OnCookingFinished() {
    StartCoroutine(WaitForPickup());
}

IEnumerator WaitForPickup() {
    float timer = 0;
    while (timer < pickupWaitTime) {
        timer += Time.deltaTime;
        // 触发事件更新UI倒计时
        OnPickupTimeChanged?.Invoke(this, new PickupTimeEventArgs {
            remainingTime = pickupWaitTime - timer,
            normalizedTime = timer / pickupWaitTime
        });
        yield return null;
    }
    
    // 时间到，菜品失效
    if (cookedIngredient != null) {
        Destroy(cookedIngredient.gameObject);
        cookedIngredient = null;
        OnIngredientExpired?.Invoke(this, EventArgs.Empty);
    }
}
```

---

### 💰 货币/金币系统
**策划案位置**: 订单奖励、对局后养成

**需求**:
- [ ] 金币管理器
- [ ] 完成订单获得金币
- [ ] UI显示金币数量
- [ ] 金币可用于对局后升级

**实现建议**:
```csharp
// 新建文件：/Assets/Scripts/Core/Manager/CurrencyManager.cs
public class CurrencyManager : MonoSingleton<CurrencyManager>
{
    private int currentCoins = 0;
    
    public event EventHandler<CurrencyChangedEventArgs> OnCurrencyChanged;
    
    public void AddCoins(int amount) { }
    public bool SpendCoins(int amount) { }
    public int GetCurrentCoins() => currentCoins;
}
```

---

### 🎁 道具/Buff系统
**策划案位置**: 2.3.1.3 配方 - "完成大配方时，若未完成关卡，则获得强力道具"

**需求**:
- [ ] 完成小订单获得随机道具
- [ ] 完成大订单（未通关时）获得强力道具
- [ ] 道具效果（如：临时提升攻击力、临时增加能量等）
- [ ] 道具UI显示和使用

**实现建议**:
```csharp
// 新建文件：/Assets/Scripts/GamePlay/Items/ItemData.cs
[CreateAssetMenu(menuName = "Items/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public ItemType itemType;
    public float duration;
    public float effectValue;
}

// 新建文件：/Assets/Scripts/GamePlay/Items/ItemManager.cs
public class ItemManager : MonoSingleton<ItemManager>
{
    public void GiveRandomItem() { }
    public void UsePowerfulItem() { }
}
```

---

### ⚔️ 完整的战斗伤害系统
**策划案位置**: 2.4 单位设定 - 伤害计算

**需求**:
- [ ] 物理/魔法伤害类型
- [ ] 防御力系统
- [ ] 魔法抗性系统
- [ ] 完整的伤害计算公式

**实现建议**:
```csharp
// 新建文件：/Assets/Scripts/GamePlay/Combat/DamageCalculator.cs
public static class DamageCalculator
{
    public static int CalculateDamage(int attack, float skillRate, AttackType attackType, int defense, int magicResist)
    {
        int damageDeal = (int)(attack * skillRate);
        int damageTaken;
        
        if (attackType == AttackType.Physical) {
            if (damageDeal <= defense) {
                damageTaken = 100;  // 最小伤害
            } else {
                damageTaken = damageDeal - defense;
            }
        } else {  // Magic
            if (damageDeal <= magicResist) {
                damageTaken = 100;
            } else {
                damageTaken = (int)(damageDeal * (1 - magicResist / 100f));
            }
        }
        
        return damageTaken;
    }
}

// 扩展 CharacterCtrlBase.cs 添加防御属性
public int defense = 0;
public int magicResist = 0;
```

---

### 🛡️ 防御塔阻挡机制
**策划案位置**: 2.3.1.5.2 能量槽 - "有阻挡"/"没有阻挡"

**需求**:
- [ ] 非宕机状态：防御塔有物理碰撞，可以阻挡怪物
- [ ] 宕机状态：防御塔失去碰撞体积，怪物可穿过

**实现建议**:
```csharp
// 扩展 TowerController.cs
private Collider2D towerCollider;

void Start() {
    towerCollider = GetComponent<Collider2D>();
    energySystem.OnStateChanged += OnEnergyStateChanged;
}

void OnEnergyStateChanged(object sender, TowerEnergySystem.StateChangedEventArgs e) {
    // 宕机时移除碰撞
    if (e.newState == TowerEnergyState.Shutdown) {
        towerCollider.enabled = false;
    } else {
        towerCollider.enabled = true;
    }
}
```

---

### 🎬 对局前战备选择阶段
**策划案位置**: 2.2 游戏核心循环 - 战备选择阶段

**需求**:
- [ ] 30秒选择时间
- [ ] 选择带入对局的防御塔（3-5个）
- [ ] 选择带入对局的食谱（5-10个）
- [ ] 战备选择UI

**实现建议**:
```csharp
// 新建文件：/Assets/Scripts/UI/PreGame/PreparationUI.cs
public class PreparationUI : MonoBehaviour
{
    public float preparationTime = 30f;
    public List<TowerData> availableTowers;
    public List<RecipeData> availableRecipes;
    
    public void ShowPreparationUI() { }
    public void SelectTower(TowerData tower) { }
    public void SelectRecipe(RecipeData recipe) { }
    public void StartGame() { }
}
```

---

### 📊 对局后结算系统
**策划案位置**: 2.2 游戏核心循环 - 对局后

**需求**:
- [ ] 显示本局统计数据
  - 完成的订单数量
  - 获得的金币
  - 击杀的怪物数量
  - 评级（S/A/B/C）
- [ ] 结算UI
- [ ] 进入养成界面

**实现建议**:
```csharp
// 新建文件：/Assets/Scripts/Core/Manager/GameResultManager.cs
public class GameResultManager : MonoSingleton<GameResultManager>
{
    public int ordersCompleted;
    public int coinsEarned;
    public int monstersKilled;
    
    public void ShowResults() { }
    public string GetGrade() { }  // S/A/B/C
}
```

---

### 🌟 局外养成系统
**策划案位置**: 2.2 游戏核心循环 - 对局后、10.17日快速讨论

**需求**:
- [ ] 英雄升级系统
- [ ] 美食图鉴系统
- [ ] 世界地图解锁
- [ ] 使用金币升级

**实现建议**:
```csharp
// 新建文件：/Assets/Scripts/MetaGame/HeroUpgradeSystem.cs
public class HeroUpgradeSystem : MonoBehaviour
{
    public void UpgradeHero(int heroId, int level) { }
}

// 新建文件：/Assets/Scripts/MetaGame/FoodEncyclopedia.cs
public class FoodEncyclopedia : MonoSingleton<FoodEncyclopedia>
{
    private HashSet<string> unlockedFoods = new HashSet<string>();
    
    public void UnlockFood(string foodName) { }
    public bool IsFoodUnlocked(string foodName) { }
}
```

---

## 📊 功能优先级建议

### ✅ 最高优先级（影响核心玩法）- 已完成！
1. ~~**关卡生命值系统**~~ - ✅ 已实现
2. ~~**怪物进入厨房检测**~~ - ✅ 已实现
3. ~~**防御塔自动攻击**~~ - ✅ 已实现
4. ~~**怪物攻击防御塔**~~ - ✅ 已实现

### ⚡ 高优先级（完善核心循环）- 建议下一步
5. **防御塔部署系统** - 策略性
6. **食材自动消失** - 紧迫感
7. **烹饪完成等待时间** - 时间管理
8. **货币系统** - 奖励反馈

### 📌 中等优先级（增强体验）
9. **完整伤害计算** - 战斗深度
10. **防御塔阻挡机制** - 策略性
11. **道具/Buff系统** - 策略多样性

### 🎨 低优先级（扩展内容）
12. **对局前战备选择** - 策略准备
13. **对局后结算** - 反馈优化
14. **局外养成系统** - 长线内容

---

## 🎯 总结

### 当前状态
- ✅ **7个核心系统**代码100%完成
- ✅ **36个C#脚本**，零错误
- ✅ **完整的UI系统**

### 还需实现
- ❌ **14个游戏逻辑功能**
- ❌ **4个最高优先级功能**（关卡生命值、怪物进厨房、防御塔攻击、怪物攻击塔）
- ❌ **4个高优先级功能**（部署系统、自动消失、等待时间、货币）

### 预估工作量
- 最高优先级功能：**2-3小时**
- 高优先级功能：**2-3小时**
- 中等优先级功能：**3-4小时**
- 低优先级功能：**5-8小时**

**总计**：约 **12-18小时** 可完成所有游戏逻辑功能

---

## 💡 建议

1. **先完成最高优先级的4个功能**，让游戏能够真正"玩起来"
2. 然后再逐步添加其他功能
3. 美术资源和局外养成可以最后做

**要不要我现在开始实现这些缺失的功能？**

