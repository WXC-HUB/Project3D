# 核心系统快速开始指南

## 🚀 10分钟快速部署全系统

### 步骤1：创建波次配置（2分钟）

1. 右键 `Create -> Game -> Wave Data`
2. 命名为 `Wave1_Data`
3. 配置：
```
Wave ID: 1
Wave Name: "第一波"
Preparation Time: 15
Interval Time: 5

Monsters:
- Monster ID: 1 (茄茄章)
  Count: 10
  Spawn Interval: 2
  Spawn Root ID: 101
```

### 步骤2：设置防御塔Prefab（3分钟）

```
1. 打开你的Tower Prefab
2. 添加组件：
   - TowerEnergySystem
     * Max Energy: 100
     * Energy Decay Rate: 2
     * Excited Threshold: 66
     * Normal Threshold: 33
   
   - TowerController
     * Drag TowerEnergySystem to reference
     * Attack Range: 5
   
   - InteractableTower
     * (自动获取引用)

3. 设置Layer为 "Interactable"
4. 保存Prefab
```

### 步骤3：设置场景管理器（2分钟）

```
1. 创建空GameObject "WaveManager"
   - 添加组件: WaveManager
   - Waves: 拖入Wave1_Data等
   - Auto Start Next Wave: ✓

2. 确保场景中有 "OrderManager"
   - 如果没有，创建并添加OrderManager组件

3. 确保场景中有 "LevelManager"
   - 已存在，无需修改
```

### 步骤4：设置玩家（2分钟）

```
1. 选择Player Prefab
2. 添加组件：
   - PlayerInventory (如果没有)
     * Max Capacity: 6
     * 创建子对象 "IngredientHoldPoint"
   
   - InteractionSystem
     * Interaction Range: 2
     * Interactable Layer: 选择 "Interactable"
     * Interact Key: E

3. 保存Prefab
```

### 步骤5：设置可交互对象（1分钟）

```
仓库：
- 添加 InteractableStorage
- 配置 Available Ingredients（拖入食材SO）

厨具（炒锅、炖锅、炸锅、烤炉）：
- 添加 InteractableAppliance

出餐口：
- 添加 InteractableDelivery

防御塔：
- 已在步骤2完成
```

---

## 🎮 测试流程

### 测试1：防御塔能量系统

```csharp
1. 运行游戏
2. 选中一个Tower对象
3. Inspector中右键菜单：
   - "Test: Deploy Tower" - 部署
   - "Test: Take Damage" - 受伤
   - "Test: Refill Energy" - 补充
4. 观察Console输出和能量变化
```

### 测试2：波次系统

```csharp
1. 运行游戏
2. WaveManager会自动开始
3. 观察：
   - 15秒准备阶段
   - 怪物开始生成
   - 击杀怪物后计数
   - 5秒间隔后下一波
```

### 测试3：交互系统

```csharp
1. 运行游戏
2. 击杀怪物，食材掉落
3. 走近食材，看到提示 "拾取 番茄 [E]"
4. 按E键拾取
5. 走近厨具，按E放入
6. 等待烹饪完成
7. 按E拾取成品
8. 走到出餐口，按E提交
```

---

## 🔧 常见问题解决

### Q: 按E没反应？
```
检查项：
1. 玩家是否有 InteractionSystem 组件
2. 对象是否有对应的 Interactable 组件
3. Layer设置是否正确
4. 是否在交互范围内（2米）
```

### Q: 防御塔能量不衰减？
```
检查项：
1. 是否调用了 tower.Deploy()
2. 是否调用了 tower.StartDecay()
3. isFirstRound 是否还是true
```

### Q: 波次不开始？
```
检查项：
1. WaveManager是否在场景中
2. Waves列表是否有配置
3. 是否调用了 WaveManager.Instance.StartWaves()
```

### Q: 怪物不掉落食材？
```
检查项：
1. Enemy Prefab是否有 IngredientDropper
2. 是否配置了 DropTable
3. CharacterCtrlBase.Die() 是否集成了掉落逻辑（已完成）
```

---

## 🎨 下一步：UI集成

### 能量槽UI示例

```csharp
using UnityEngine.UI;
using Assets.Scripts.Tower;

public class TowerEnergyUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Text percentText;
    [SerializeField] private TowerEnergySystem energySystem;
    
    private void Start()
    {
        energySystem.OnEnergyChanged += UpdateUI;
    }
    
    private void UpdateUI(object sender, TowerEnergySystem.EnergyChangedEventArgs e)
    {
        fillImage.fillAmount = e.normalizedEnergy;
        percentText.text = $"{e.normalizedEnergy * 100:F0}%";
        
        // 状态颜色
        switch (energySystem.CurrentState)
        {
            case TowerEnergyState.Excited:
                fillImage.color = Color.yellow;
                break;
            case TowerEnergyState.Normal:
                fillImage.color = Color.green;
                break;
            case TowerEnergyState.Weak:
                fillImage.color = Color.red;
                break;
            case TowerEnergyState.Shutdown:
                fillImage.color = Color.gray;
                break;
        }
    }
}
```

### 波次UI示例

```csharp
using UnityEngine.UI;
using Assets.Scripts.Wave;

public class WaveUI : MonoBehaviour
{
    [SerializeField] private Text waveText;
    [SerializeField] private Text monsterText;
    [SerializeField] private Text phaseText;
    [SerializeField] private WaveManager waveManager;
    
    private void Start()
    {
        waveManager.OnWaveStarted += UpdateWave;
        waveManager.OnMonsterKilled += UpdateMonsters;
        waveManager.OnPhaseChanged += UpdatePhase;
    }
    
    private void UpdateWave(object sender, WaveManager.WaveEventArgs e)
    {
        waveText.text = $"波次 {e.waveIndex + 1}/{waveManager.TotalWaves}";
    }
    
    private void UpdateMonsters(object sender, WaveManager.MonsterEventArgs e)
    {
        monsterText.text = $"怪物: {waveManager.MonstersRemaining}";
    }
    
    private void UpdatePhase(object sender, WaveManager.PhaseEventArgs e)
    {
        string phaseName = e.phase switch
        {
            WavePhase.Preparation => "准备阶段",
            WavePhase.InProgress => "战斗中",
            WavePhase.Interval => "波次间隔",
            _ => ""
        };
        phaseText.text = phaseName;
    }
}
```

### 交互提示UI示例

```csharp
using UnityEngine.UI;
using Assets.Scripts.Interaction;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private Text promptText;
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private InteractionSystem interactionSystem;
    
    private void Start()
    {
        interactionSystem.OnTargetChanged += UpdatePrompt;
        promptPanel.SetActive(false);
    }
    
    private void UpdatePrompt(object sender, InteractionSystem.InteractionEventArgs e)
    {
        if (e.target != null && e.isAvailable)
        {
            promptText.text = e.target.GetInteractionPrompt();
            promptPanel.SetActive(true);
        }
        else
        {
            promptPanel.SetActive(false);
        }
    }
}
```

---

## 📊 性能优化建议

### 1. 对象池
```csharp
// 建议为以下对象创建对象池：
- 怪物Prefab
- 食材Prefab
- 子弹Prefab
- VFX特效
```

### 2. 事件优化
```csharp
// 记得在OnDestroy中取消订阅
private void OnDestroy()
{
    if (energySystem != null)
    {
        energySystem.OnEnergyChanged -= UpdateUI;
    }
}
```

### 3. 检测优化
```csharp
// InteractionSystem 使用了定时检测
// 可以通过调整 detectionInterval 来平衡性能和响应性
detectionInterval = 0.1f; // 默认值，可根据需要调整
```

---

## 🎯 完整示例场景搭建

```
Scene Hierarchy:

GameRoot
├── Managers
│   ├── GameManager (已有)
│   ├── LevelManager (已有)
│   ├── OrderManager (烹饪系统)
│   ├── WaveManager (波次系统)
│   └── UIManager (已有)
│
├── Level
│   ├── Ground (地面)
│   ├── Walls (墙壁)
│   └── SpawnPoints (生成点)
│
├── Kitchen (厨房区域)
│   ├── Storage (仓库)
│   │   ├── StorageCounter
│   │   └── InteractableStorage
│   ├── StirFryPot (炒锅)
│   │   ├── StirFryAppliance
│   │   └── InteractableAppliance
│   ├── StewPot (炖锅)
│   ├── FryPot (炸锅)
│   ├── RoastOven (烤炉)
│   └── DeliveryCounter (出餐口)
│
├── Defense (防御区域)
│   ├── Tower_01
│   │   ├── TowerEnergySystem
│   │   ├── TowerController
│   │   ├── InteractableTower
│   │   └── PlayerCharacterCtrl
│   ├── Tower_02
│   └── Tower_03
│
├── Player
│   ├── PlayerCharacterCtrl
│   ├── PlayerInventory
│   ├── InteractionSystem
│   └── CameraFollow
│
└── UI
    ├── Canvas_HUD
    │   ├── TowerEnergyUI
    │   ├── WaveUI
    │   ├── OrderUI
    │   └── InventoryUI
    └── Canvas_Interaction
        └── InteractionPromptUI
```

---

## ✅ 最终检查清单

### 烹饪系统
- [x] 食材配置创建
- [x] 配方配置创建
- [x] 掉落表配置
- [x] 厨具设置
- [x] 仓库设置
- [x] 出餐口设置

### 防御塔系统
- [ ] Tower Prefab配置
- [ ] 能量系统测试
- [ ] 喂食机制测试
- [ ] 视觉状态切换

### 波次系统
- [ ] WaveData创建
- [ ] WaveManager设置
- [ ] 怪物生成测试
- [ ] 波次流程测试

### 交互系统
- [ ] 玩家InteractionSystem
- [ ] 所有Interactable组件
- [ ] Layer设置正确
- [ ] 交互提示UI

### 整合测试
- [ ] 完整游戏循环
- [ ] 击杀→掉落→烹饪→喂食
- [ ] 波次→战斗→完成
- [ ] 订单→完成→胜利

---

**🎉 恭喜！所有核心系统已准备就绪！**

需要帮助？查看：
- `COOKING_SYSTEM_SUMMARY.md` - 烹饪系统文档
- `CORE_SYSTEMS_SUMMARY.md` - 核心系统文档
- `CookingSystemExample.cs` - 代码示例

