# 🎮 Demo场景创建指南

## 快速开始（推荐）

### 🚀 一键创建Demo场景

在Unity中执行以下步骤：

```
1. Unity菜单栏 → Tools → Setup Demo Scene
2. 在弹出的窗口中点击 "创建Demo场景" 按钮
3. 确认对话框 → 点击 "确定"
4. 等待自动创建完成（约5-10秒）
5. 完成！场景已保存为 Assets/Scenes/DemoScene.unity
```

### ▶️ 启动Demo

```
1. 确保 DemoScene.unity 已打开
2. 点击Unity顶部的播放按钮 ▶️
3. 观察Console窗口的输出
4. 查看Game视图左上角的提示信息
```

---

## 📋 Demo场景包含什么？

### ✅ 自动创建的资源

#### 1. **ScriptableObject数据**
```
Assets/Resources/Data/
├── Ingredients/        (食材数据)
│   ├── Tomato.asset   (番茄)
│   └── Potato.asset   (土豆)
├── Recipes/           (配方数据)
│   └── TomatoStew.asset (番茄炖菜)
├── Orders/            (订单数据)
│   └── Order_TomatoStew.asset
├── Waves/             (波次数据)
│   └── Wave1.asset
└── Drops/             (掉落表)
    └── BasicEnemyDrop.asset
```

#### 2. **场景对象**
```
DemoScene
├── Main Camera          (摄像机)
├── Directional Light    (光源)
├── LevelHealthSystem    (关卡生命系统)
├── WaveManager          (波次管理器)
├── OrderManager         (订单管理器)
├── Ground               (地面 - 灰色平面)
├── KitchenZone          (厨房区域 - 红色半透明)
├── Player               (玩家对象)
└── DemoUICanvas         (UI提示)
```

#### 3. **核心系统**
- ✅ 关卡生命系统（初始10点生命）
- ✅ 波次管理系统（准备15秒，间隔5秒）
- ✅ 订单管理系统
- ✅ 厨房区域检测（怪物进入扣血）

---

## 🎯 Demo场景功能演示

### 当前可以测试的功能：

1. **关卡生命系统**
   - 初始生命值: 10
   - 怪物进入厨房区扣血
   - 生命值为0时游戏失败

2. **波次系统**
   - 准备阶段: 15秒
   - 波次间隔: 5秒
   - 波次信息显示在Console

3. **场景结构**
   - 地面平台
   - 厨房危险区域（红色）
   - 玩家位置

---

## 🛠️ 手动配置步骤（可选）

如果一键创建失败，或者想要手动配置，可以按照以下步骤：

### 步骤1: 创建ScriptableObjects

```
1. Project窗口右键 → Create → 选择对应类型
   - IngredientData (食材)
   - RecipeData (配方)
   - OrderData (订单)
   - WaveData (波次)
   - IngredientDropTable (掉落表)

2. 在Inspector中配置数据

3. 保存到 Assets/Resources/Data/ 对应目录
```

### 步骤2: 设置场景

```
1. Hierarchy右键 → Create Empty → 命名
2. Add Component → 添加对应组件：
   - LevelHealthSystem
   - WaveManager
   - OrderManager

3. 在Inspector中配置参数
```

### 步骤3: 创建地面和区域

```
1. Hierarchy右键 → 3D Object → Plane (地面)
2. Hierarchy右键 → Create Empty (厨房区域)
   - Add Component → Box Collider (勾选Is Trigger)
   - Add Component → Kitchen Zone Trigger
```

---

## 🔧 进阶配置

### 添加防御塔

```
1. 在Scene视图中找到合适位置
2. Project窗口 → Resources/CharacterPrefabs/CC_Object_Tower
3. 拖拽到场景中
4. 在Inspector中配置:
   - Add Component → Tower Controller
   - Add Component → Tower Energy System
```

### 添加怪物生成点

```
1. Hierarchy右键 → Create Empty → 命名 "SpawnPoint"
2. 调整位置到地图边缘
3. WaveManager → 在Inspector中配置:
   - Spawn Roots: 添加SpawnPoint
```

### 添加厨具

```
1. Hierarchy右键 → Create Empty → 命名 "StirFryStation"
2. Add Component → Stir Fry Appliance
3. Add Component → Interactable Appliance
4. 添加3D模型作为子对象（可选）
```

### 添加仓库

```
1. Hierarchy右键 → Create Empty → 命名 "Storage"
2. Add Component → Storage Counter
3. Add Component → Interactable Storage
4. 在Inspector中配置存储的食材
```

---

## 🎨 UI配置

### 添加能量槽UI

```
1. Hierarchy → DemoUICanvas 右键 → UI → Panel
2. 命名为 "TowerEnergyPanel"
3. Add Component → Tower Energy UI
4. 在Inspector中配置:
   - Link to TowerEnergySystem
   - 设置UI元素引用
```

### 添加波次信息UI

```
1. Hierarchy → DemoUICanvas 右键 → UI → Panel
2. 命名为 "WaveInfoPanel"
3. Add Component → Wave Info UI
4. 配置文本显示
```

---

## ⚠️ 常见问题

### Q1: 点击"Setup Demo Scene"没有反应？
**解决方案**:
- 检查Console窗口是否有错误
- 确保所有脚本已编译完成（无红色错误）
- 尝试重启Unity

### Q2: 创建的场景是空的？
**解决方案**:
- 检查是否有权限创建文件
- 手动打开 Assets/Scenes/DemoScene.unity
- 查看Hierarchy窗口是否有对象

### Q3: 场景中的对象没有组件？
**解决方案**:
- 选中对象查看Inspector
- 如果缺少组件，手动添加
- 参考上面的"手动配置步骤"

### Q4: Console显示很多警告/错误？
**解决方案**:
- 黄色警告可以忽略
- 红色错误需要解决，复制错误信息查询
- 常见错误: 缺少引用 → 在Inspector中手动配置

### Q5: 播放后没有反应？
**解决方案**:
- 打开Console窗口查看日志输出
- 确保Game窗口被选中（不是Scene窗口）
- 检查系统是否初始化成功（看Console日志）

---

## 📚 相关文档

- `UNITY_CORE_FEATURES_SETUP.md` - 完整功能配置指南
- `HOW_TO_START_GAME.md` - 游戏启动指南
- `CORE_FEATURES_COMPLETED.md` - 已实现功能列表
- `README_SYSTEMS.md` - 系统架构说明

---

## 🎯 下一步建议

完成Demo场景后，可以：

### 1. 测试核心系统
```
- 观察关卡生命值变化
- 查看波次系统运行
- 测试系统事件触发
```

### 2. 添加更多内容
```
- 添加更多食材和配方
- 配置敌人Prefab和掉落
- 创建更多波次配置
- 完善UI界面
```

### 3. 完整游戏配置
```
参考 UNITY_CORE_FEATURES_SETUP.md
按步骤配置完整的游戏功能
```

---

## 🚀 快速测试流程

```bash
# 最快的测试方式：
1. Unity菜单 → Tools → Setup Demo Scene
2. 点击 "创建Demo场景" 
3. 等待完成
4. 点击播放按钮 ▶️
5. 打开Console窗口查看输出
6. Done! 🎉
```

---

**提示**: 这是一个最小可玩的Demo，主要用于测试核心系统是否正常工作。要体验完整游戏，需要继续配置更多资源和功能。

**遇到问题？** 随时查看Console窗口的输出，或者询问我！😊

