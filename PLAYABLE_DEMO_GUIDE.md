# 🎮 可玩Demo使用指南

## 📦 已创建的内容

### ✅ 新增脚本（3个）

#### 1. `SimplePlayerController.cs`
玩家移动控制脚本
- ✅ WASD / 方向键移动
- ✅ Shift加速奔跑
- ✅ 自动旋转朝向移动方向
- ✅ 摄像机跟随

#### 2. `SimpleEnemySpawner.cs`
敌人自动生成系统
- ✅ 波次系统（每波5个敌人）
- ✅ 自动生成间隔
- ✅ 敌人自动移动到目标
- ✅ 可视化生成范围（Scene视图中）

#### 3. `SimpleDemoUI.cs`
游戏UI显示
- ✅ 控制说明（左上角）
- ✅ 游戏状态（右上角）
  - 关卡生命
  - 当前波次
  - 敌人数量
  - 防御塔数量
  - FPS显示

### ✅ 编辑器工具

#### `PlayableDemoSetup.cs`
一键升级Demo场景为可玩版本
- 菜单位置: `游戏Demo → 升级为可玩Demo`

---

## 🚀 使用方法（3步）

### 第1步：打开Demo场景

```
Unity Editor → Project窗口 → Assets/Scenes/DemoScene.unity
双击打开
```

### 第2步：升级为可玩版本

**方法A：使用菜单**
```
顶部菜单栏 → 游戏Demo → 升级为可玩Demo
→ 点击"🚀 升级为可玩Demo"按钮
→ 等待完成提示
```

**方法B：手动添加（如果菜单不可用）**

在Hierarchy中：
1. 选中Player对象
   - Add Component → Simple Player Controller

2. 创建空对象，命名为"EnemySpawner"
   - Add Component → Simple Enemy Spawner
   - Position设置为(0, 0, 20)

3. 创建空对象，命名为"DemoUIManager"
   - Add Component → Simple Demo UI

### 第3步：点击播放 ▶️

```
Unity Editor顶部工具栏 → 播放按钮 ▶️
或者按快捷键: Ctrl+P (Windows) / Cmd+P (Mac)
```

---

## 🎮 游戏控制

### 移动控制
```
W / ↑     - 向前移动
S / ↓     - 向后移动
A / ←     - 向左移动
D / →     - 向右移动
Shift     - 加速奔跑
ESC       - 退出播放模式
```

### 摄像机
- 自动跟随玩家
- 固定视角（俯视30度）

---

## 👀 你会看到什么？

### 游戏开始后（5秒内）

```
✅ 白色胶囊（玩家）- 可以用WASD控制移动
✅ 红色方块（敌人）- 每2秒生成1个，共5个
✅ 蓝色圆柱（防御塔）- 自动攻击范围内的敌人
✅ UI信息:
   - 左上角：控制说明
   - 右上角：游戏状态（生命、波次、敌人数等）
```

### 第一波完成后（约15秒）

```
📣 Console提示："第1波生成完成"
⏱ 等待15秒波次间隔
📣 开始第2波
```

### 游戏循环

```
1. 生成第N波敌人（5个）
2. 敌人向(0,0,0)移动
3. 防御塔自动攻击敌人
4. 敌人被击杀或到达目标
5. 等待波次间隔
6. 回到第1步（波次+1）
```

---

## 🔍 调试和观察

### Console窗口看到的日志

```
✓ 玩家控制器已激活 - 使用WASD移动，Shift加速
✓ 敌人生成器已激活 - 准备生成第一波
✓ Demo UI已初始化
📣 第1波开始！准备生成5个敌人
👾 生成敌人: Enemy_1_1 在位置 (x, y, z)
👾 生成敌人: Enemy_1_2 在位置 (x, y, z)
...
✓ 第1波生成完成！共5个敌人
```

### Scene视图（不在播放时）

```
黄色球体 - 敌人生成范围
红色球体 - 敌人目标点
青色线条 - 生成点到目标的路径
```

### Game视图（播放时）

```
左上角 - 控制说明
右上角 - 实时游戏状态
场景中 - 玩家、敌人、防御塔
```

---

## ⚙️ 自定义设置

### 调整敌人生成

选中`EnemySpawner`对象，在Inspector中：

```
Enemies Per Wave   - 每波敌人数量（默认5）
Spawn Interval     - 生成间隔秒数（默认2）
Wave Interval      - 波次间隔秒数（默认15）
Spawn Radius       - 生成范围半径（默认15）
Enemy Move Speed   - 敌人移动速度（默认3）
```

### 调整玩家移动

选中`Player`对象，在Inspector中：

```
Move Speed         - 普通移动速度（默认5）
Sprint Speed       - 冲刺速度（默认8）
Rotation Speed     - 旋转速度（默认10）
Camera Distance    - 摄像机距离（默认10）
Camera Height      - 摄像机高度（默认5）
```

### 调整防御塔

选中`Tower`对象（如果有TowerController组件）：

```
Attack Interval    - 攻击间隔
Base Attack        - 基础攻击力
Attack Range       - 攻击范围
Attack Type        - 攻击类型
```

---

## 🐛 常见问题

### Q: 点击播放后看不到敌人？

**A1**: 等待2-5秒，第一个敌人会生成
**A2**: 查看Console，确认看到"敌人生成器已激活"
**A3**: 在Scene视图中调整视角，敌人可能在(0,0,20)附近

### Q: 玩家无法移动？

**A1**: 确认Player对象有`SimplePlayerController`组件
**A2**: 确认Game窗口是激活状态（点击一下）
**A3**: 检查Console是否有错误

### Q: 防御塔不攻击？

**A1**: 确认Tower有`TowerController`组件
**A2**: 确认敌人在攻击范围内（默认可能需要调整）
**A3**: 查看Console日志

### Q: UI不显示？

**A1**: 确认有`DemoUIManager`对象
**A2**: 确认场景中有Canvas
**A3**: 查看Console是否有字体相关警告

### Q: 编译错误？

**A1**: 等待Unity自动编译完成（1-2分钟）
**A2**: 查看Console的具体错误信息
**A3**: 确认所有新文件都在`Assets/Scripts/Demo/`目录下

---

## 📊 技术细节

### 脚本依赖关系

```
SimplePlayerController
  └─ Rigidbody (自动添加)
  └─ Camera.main (自动查找)

SimpleEnemySpawner
  └─ Resources.Load<GameObject>("CharacterPrefabs/CC_Enemy_1") (可选)
  └─ SimpleEnemyMover (自动添加到生成的敌人)
  └─ IngredientDropper (尝试添加，可能失败)

SimpleDemoUI
  └─ LevelHealthSystem (FindObjectOfType)
  └─ WaveManager (FindObjectOfType)
  └─ Canvas (自动创建)
```

### 系统工作流程

```
[开始游戏]
    ↓
[SimplePlayerController.Start]
    ├─ 配置Rigidbody
    ├─ 查找摄像机
    └─ 开始监听输入
    ↓
[SimpleEnemySpawner.Start]
    ├─ 加载或创建敌人Prefab
    └─ 启动波次协程
    ↓
[SimpleDemoUI.Start]
    ├─ 创建UI元素
    ├─ 查找游戏系统
    └─ 开始更新循环
    ↓
[游戏循环]
    ├─ 玩家移动（Update/FixedUpdate）
    ├─ 敌人生成（协程）
    ├─ 敌人移动（Update）
    ├─ UI更新（定时）
    └─ 防御塔攻击（如果存在）
```

---

## 🎯 下一步

### 当前状态
- ✅ 玩家可移动
- ✅ 敌人自动生成
- ✅ UI信息显示
- ⚠️ 防御塔可能需要手动配置
- ⚠️ 食材系统未连接
- ⚠️ 烹饪系统未连接

### 可以尝试的改进

1. **添加更多防御塔**
   - 在Scene中放置多个Tower
   - 调整位置形成防线

2. **调整游戏难度**
   - 增加每波敌人数量
   - 减少波次间隔
   - 提高敌人速度

3. **测试完整系统**
   - 参考`UNITY_CORE_FEATURES_SETUP.md`
   - 连接烹饪系统
   - 连接订单系统

4. **自定义美术资源**
   - 替换玩家模型
   - 添加敌人Prefab
   - 美化UI界面

---

## 📚 相关文档

- `WHY_DEMO_IS_STATIC.md` - 为什么Demo最初是静态的
- `DEMO_QUICKSTART.md` - Demo场景快速启动
- `UNITY_CORE_FEATURES_SETUP.md` - 核心功能Unity设置
- `CORE_FEATURES_COMPLETED.md` - 已完成功能清单
- `MISSING_FEATURES.md` - 待完成功能清单

---

## ✨ 总结

### 你现在拥有：

```
✅ 完整的代码系统（100%）
✅ 基础数据资源（ScriptableObjects）
✅ 可玩的Demo场景
✅ 玩家移动控制
✅ 敌人生成系统
✅ 游戏UI显示
✅ 编辑器工具

⏳ 待完善：
- 完整的烹饪系统集成
- 订单系统集成
- 更多游戏玩法
- 美术资源
- 关卡设计
```

### 使用流程：

```
1. 打开DemoScene ✅
2. 点击"游戏Demo → 升级为可玩Demo" ✅
3. 点击播放 ▶️ ✅
4. 开始游戏！🎮
```

---

**祝游戏开发愉快！** 🎉

有任何问题随时查看Console日志和相关文档！

