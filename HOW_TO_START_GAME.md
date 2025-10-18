# 🎮 如何启动游戏

## 方法1：使用现有场景（推荐）

### 1. 打开Unity项目
```
Unity Hub → Open → 选择 /Users/mt/Documents/Project3D
```

### 2. 打开场景
```
Project窗口 → Assets → Scenes → SampleScene.unity
双击打开
```

### 3. 点击播放
```
Unity顶部工具栏 → 点击播放按钮 ▶️
或按快捷键: Cmd + P (Mac) / Ctrl + P (Windows)
```

---

## 方法2：使用测试场景

如果`SampleScene.unity`有问题，可以使用测试场景：

### 1. 创建测试场景
在Unity中：
```
File → New Scene → 2D
File → Save As → Assets/Scenes/GameTestScene.unity
```

### 2. 添加核心系统

#### A. 创建关卡生命系统
```
1. Hierarchy右键 → Create Empty
2. 命名为: LevelHealthSystem
3. Inspector → Add Component → Level Health System
4. 配置:
   - Max Health: 10
   - Starting Health: 10
```

#### B. 创建波次管理器
```
1. Hierarchy右键 → Create Empty
2. 命名为: WaveManager
3. Inspector → Add Component → Wave Manager
4. 配置:
   - Waves: 空（暂时）
   - Prep Time: 15
   - Interval Time: 5
```

#### C. 添加测试脚本
```
1. Hierarchy右键 → Create Empty
2. 命名为: GameSystemTest
3. Inspector → Add Component → Quick Start Test
4. 勾选: Enable Debug Logs
```

### 3. 添加摄像机（如果没有）
```
Hierarchy右键 → Camera
Position: (0, 0, -10)
```

### 4. 点击播放
```
点击 ▶️ 播放按钮
查看Console窗口的系统状态输出
左上角会显示系统状态UI
```

---

## 🎯 期望看到什么？

### Console输出示例：
```
[QuickStartTest] === 游戏系统快速启动测试 ===
[QuickStartTest] ✓ 关卡生命系统已激活
[QuickStartTest]   当前生命值: 10/10
[QuickStartTest] ✓ 波次管理系统已激活
[QuickStartTest]   当前波次: 1
[QuickStartTest] === 所有核心系统已初始化 ===
```

### Game视图左上角UI：
```
┌─────────────────────┐
│ === 游戏系统状态 === │
│ 关卡生命: 10/10      │
│ 当前波次: 1          │
│ 波次状态: 准备中     │
│                      │
│ [测试：扣除关卡生命] │
└─────────────────────┘
```

---

## ⚠️ 常见问题

### Q1: 点击播放后没有反应？
**解决方案**：
- 检查Console窗口是否有编译错误（红色）
- 确保场景中有Main Camera
- 检查Game窗口是否被选中

### Q2: 看到"Missing Prefab"错误？
**解决方案**：
- 这是正常的，因为还没有创建所有的Prefab
- 可以忽略，或者删除场景中引用缺失Prefab的对象

### Q3: 没有看到测试UI？
**解决方案**：
- 确保Game窗口被选中（不是Scene窗口）
- 检查QuickStartTest组件是否已添加到场景
- 查看Console窗口的日志输出

### Q4: 系统提示"Instance is null"？
**解决方案**：
- 确保添加了对应的Manager GameObject
- 检查Manager组件的执行顺序

---

## 📋 下一步：完整配置

启动游戏后，要玩到完整的游戏内容，需要：

### 1. 创建ScriptableObjects
参考：`UNITY_CORE_FEATURES_SETUP.md` 第1部分

### 2. 创建Prefabs
参考：`UNITY_CORE_FEATURES_SETUP.md` 第2部分

### 3. 配置场景
参考：`UNITY_CORE_FEATURES_SETUP.md` 第3-5部分

### 4. 设置UI
参考：`UNITY_CORE_FEATURES_SETUP.md` 第6部分

---

## 🚀 快速测试流程

如果只想快速看到系统运行：

```bash
# 1. 确保项目无编译错误
# 2. 创建空场景
# 3. 添加 LevelHealthSystem GameObject + 组件
# 4. 添加 WaveManager GameObject + 组件  
# 5. 添加 GameSystemTest GameObject + QuickStartTest组件
# 6. 添加 Main Camera
# 7. 点击播放 ▶️
```

---

## 📚 相关文档

- `UNITY_CORE_FEATURES_SETUP.md` - 完整的Unity配置指南
- `CORE_FEATURES_COMPLETED.md` - 已实现的核心功能列表
- `README_SYSTEMS.md` - 系统架构概览
- `MISSING_FEATURES.md` - 待实现功能列表

---

**提示**：现在所有代码都已经写好了，只需要在Unity中配置即可！如果遇到任何问题，可以随时询问。

