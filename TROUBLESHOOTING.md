# 🔧 故障排除指南

## 已解决的问题

### ✅ 问题1：Tag "Enemy" 未定义

**错误信息**：
```
UnityException: Tag: Enemy is not defined.
SimpleDemoUI.UpdateUI () (at Assets/Scripts/Demo/SimpleDemoUI.cs:95)
```

**原因**：
Unity项目中没有定义"Enemy"标签，而SimpleDemoUI尝试使用`GameObject.FindGameObjectsWithTag("Enemy")`查找敌人。

**解决方案**：
1. ✅ **自动添加标签**：创建了`TagHelper.cs`，Unity启动时自动添加必要标签
2. ✅ **查找逻辑优化**：改为优先使用`FindObjectsOfType<SimpleEnemyMover>()`
3. ✅ **错误保护**：添加了try-catch捕获标签未定义异常

**代码修改**：
```csharp
// 修改前（会抛出异常）
var enemies = GameObject.FindGameObjectsWithTag("Enemy");

// 修改后（安全）
var enemyMovers = FindObjectsOfType<SimpleEnemyMover>();
if (enemyMovers.Length > 0) { ... }
else {
    try {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
    } catch (UnityException) { ... }
}
```

---

### ✅ 问题2：AISensorBase空引用异常

**错误信息**：
```
NullReferenceException: Object reference not set to an instance of an object
Assets.Scripts.AI.AISensorBase.getCharacterByKey (System.String key)
```

**原因**：
在Demo场景中，`LevelManager.Instance`或`Character_Dict`可能为null，但代码直接访问导致空引用异常。

**解决方案**：
✅ 添加了空值检查保护

**代码修改**：
```csharp
// 修改前（可能空引用）
if (LevelManager.Instance.Character_Dict.TryGetValue(...))

// 修改后（安全）
if (LevelManager.Instance == null || LevelManager.Instance.Character_Dict == null)
{
    return null;
}
if (LevelManager.Instance.Character_Dict.TryGetValue(...))
```

---

## 新增工具

### 🛠️ TagHelper - 自动标签管理

**位置**：`Assets/Scripts/Editor/TagHelper.cs`

**功能**：
- ✅ Unity启动时自动添加常用标签
- ✅ 菜单手动添加标签：`游戏Demo → 工具 → 添加常用标签`

**自动添加的标签**：
- Enemy（敌人）
- Tower（防御塔）
- Player（玩家）
- Ingredient（食材）
- KitchenAppliance（厨具）
- Storage（仓库）
- Delivery（出餐口）

**使用方法**：
```
方法1：自动（推荐）
Unity启动 → TagHelper自动运行 → 标签已添加

方法2：手动
Unity菜单 → "游戏Demo" → "工具" → "添加常用标签"
```

---

## 其他可能的问题

### 问题：找不到"游戏Demo"菜单

**原因**：Unity还在编译中

**解决**：
1. 检查Unity左下角进度条
2. 等待1-2分钟编译完成
3. 查看Console是否有编译错误

---

### 问题：点击升级后没反应

**原因**：处于播放模式

**解决**：
1. 确保Unity不在播放模式（▶️按钮是灰色的）
2. 如果在播放，先停止 ⏹
3. 再执行升级操作

---

### 问题：播放后看不到敌人

**原因**：敌人还没生成

**解决**：
1. 等待2-5秒（敌人生成需要时间）
2. 查看Console日志："👾 生成敌人: Enemy_1_1"
3. 在Scene视图中查看敌人位置（可能在远处）

---

### 问题：玩家无法移动

**原因**：Game窗口没有焦点

**解决**：
1. 点击Game窗口获得输入焦点
2. 确保Game标签是激活状态
3. 按WASD键尝试移动

---

### 问题：有其他编译错误

**调试步骤**：
1. 打开Console窗口：`Window → General → Console`
2. 点击错误信息查看详细堆栈
3. 双击错误跳转到代码位置
4. 截图发给我具体的错误信息

---

## 常见警告（可忽略）

### ⚠️ Font警告

```
Arial.ttf is no longer a valid built in font. Please use LegacyRuntime.ttf
```

**说明**：这是Unity版本兼容性警告，不影响功能。代码已自动回退到LegacyRuntime.ttf。

---

### ⚠️ Material警告

```
Instantiating material due to calling renderer.material during edit mode.
```

**说明**：这是编辑器模式下的材质实例化警告，已修复为使用`sharedMaterial`。

---

## 诊断工具

### 检查系统状态

**在Console中查找以下日志**：

```
✓ 标签已添加: Enemy
✓ 玩家控制器已激活
✓ 敌人生成器已激活
✓ Demo UI已初始化
```

如果看到这些日志，说明系统正常初始化。

---

### 查看实时状态

**在Game视图右上角**：

```
❤️ 关卡生命: X/X
🌊 当前波次: X
👾 敌人数量: X
🗼 防御塔: X
⚡ FPS: X
```

如果UI显示正常，说明系统运行正常。

---

## 联系支持

如果遇到其他问题：

1. **截图Console错误**
2. **描述操作步骤**
3. **说明期望结果 vs 实际结果**
4. **发送给我**

我会尽快帮你解决！

---

## 修复历史

### 2025-10-18 - v1.0

- ✅ 修复：Tag "Enemy" 未定义异常
- ✅ 修复：AISensorBase空引用异常
- ✅ 新增：TagHelper自动标签管理工具
- ✅ 优化：SimpleDemoUI敌人查找逻辑
- ✅ 增强：错误处理和空值保护

---

**最后更新**：2025-10-18
**状态**：所有已知问题已修复 ✅

