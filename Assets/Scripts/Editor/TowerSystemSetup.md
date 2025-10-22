# 多防御塔系统配置指南

本文档说明如何配置三种防御塔类型：基础塔、散射塔、减速塔

## 📋 一、配置表修改

### 1. GameCharacters.csv
在文件末尾添加以下行：

```csv
10,散射防御塔,Tower,CC_Object_Tower_Scatter,12001
11,减速防御塔,Tower,CC_Object_Tower_Slow,12001
20,散射子弹,Bullet,CC_Bullet_Scatter,8001
21,减速子弹,Bullet,CC_Bullet_Slow,8001|14001
```

说明：
- ObjectID 10: 散射塔
- ObjectID 11: 减速塔
- ObjectID 20: 散射子弹（普通伤害）
- ObjectID 21: 减速子弹（命中后产生爆炸场地）

### 2. GameModifiers.csv
在文件末尾添加以下修改器：

```csv
14001,1,ListenGameEvent,CharacterDoCollide|SpawnField|11|HitPointX|HitPointY,子弹：碰撞时产生减速场地
14001,2,ListenGameEvent,CharacterDoCollide|DispelModifier|14001|DoHitCharacter,子弹：碰撞后驱散自身
15001,1,AddSimpleDect,float_mul|MaxSpeed|-0.5,减速Buff：移动速度减少50%
15001,2,BindVFX,VFX_Slow_Effect,减速Buff：绑定减速特效
```

说明：
- 14001: 减速子弹的碰撞修改器（产生场地）
- 15001: 减速Buff（降低移动速度50%）

### 3. GameFields.csv
在文件末尾添加：

```csv
11,GameField_SlowExplosion,减速爆炸场地
```

### 4. GameSkills.csv
添加新技能：

```csv
10,散射攻击,TRUE,FALSE,13001,-1,FF6600,散射塔专用攻击
11,减速攻击,TRUE,FALSE,14001,-1,3366FF,减速塔专用攻击
```

## 🎮 二、Unity预制体配置

### 1. 创建散射塔预制体

1. **复制基础塔预制体**
   - 复制 `CC_Object_Tower.prefab`
   - 重命名为 `CC_Object_Tower_Scatter.prefab`

2. **修改属性**
   - MaxHP.real_value: 100
   - MaxMP.real_value: 150
   - Damage_Shoot.real_value: 12（单发伤害降低，因为是多发）
   - MyGameObjectID: 10

3. **添加AI组件**
   - 移除原有的 AIAgentBase 组件（如果有）
   - 添加 `TowerAI_Scatter` 组件
   - 设置参数：
     - bulletCount: 3
     - spreadAngle: 15
     - scatterBulletID: 20

4. **修改行为树**（可选）
   - 如果使用行为树，保持原有行为树
   - 攻击动作会被 TowerAI_Scatter 重写

### 2. 创建减速塔预制体

1. **复制基础塔预制体**
   - 复制 `CC_Object_Tower.prefab`
   - 重命名为 `CC_Object_Tower_Slow.prefab`

2. **修改属性**
   - MaxHP.real_value: 100
   - MaxMP.real_value: 120
   - Damage_Shoot.real_value: 15（伤害适中）
   - MyGameObjectID: 11

3. **修改行为树JSON**
   - 将攻击动作中的 bullet_type 改为 21（减速子弹）
   - 或在技能配置中指定使用 ObjectID 21 的子弹

### 3. 创建子弹预制体

#### 散射子弹 (CC_Bullet_Scatter)
1. 复制 `CC_Bullet_1.prefab`
2. 重命名为 `CC_Bullet_Scatter.prefab`
3. 修改属性：
   - MyGameObjectID: 20
   - Damage_Shoot.real_value: 12
   - MaxSpeed.real_value: 8
   - 可以修改子弹的视觉效果（颜色、大小）

#### 减速子弹 (CC_Bullet_Slow)
1. 复制 `CC_Bullet_1.prefab`
2. 重命名为 `CC_Bullet_Slow.prefab`
3. 修改属性：
   - MyGameObjectID: 21
   - Damage_Shoot.real_value: 15
   - MaxSpeed.real_value: 6（稍慢）
   - Init_Modifier_List: 添加 14001
4. 视觉效果：
   - 可以改成蓝色或紫色，表示减速效果

### 4. 创建场地效果预制体

**创建减速爆炸场地**
1. 创建新GameObject: `GO_Field_SlowExplosion`
2. 添加组件：
   - `GameField_SlowExplosion`
   - `CircleCollider2D` (Trigger)
   - `SpriteRenderer`（可选，显示减速区域）
3. 设置参数：
   - damageAmount: 10
   - slowModifierID: 15001
   - slowDuration: 3
   - lifeTime: 3（继承自GameFieldBase）
4. 保存到 `Assets/Resources/FieldPrefabs/GO_Field_SlowExplosion.prefab`

## 🔧 三、配置要点总结

### 防御塔类型对比

| 属性 | 基础塔 | 散射塔 | 减速塔 |
|------|--------|--------|--------|
| ObjectID | 2 | 10 | 11 |
| HP | 1 | 100 | 100 |
| MP | 100 | 150 | 120 |
| 伤害 | 18 | 12×3=36 | 15 |
| 特效 | 单体高伤 | 范围散射 | 减速控制 |
| 子弹ID | 3 | 20 | 21 |

### 技能系统架构

```
防御塔 (Tower)
  ├─ 基础塔: 标准单发攻击
  ├─ 散射塔: TowerAI_Scatter → 同时生成3颗子弹
  └─ 减速塔: 标准攻击 → 减速子弹(21) → 碰撞产生场地(11)
                                              └─ 减速Buff(15001)
```

## 🎯 四、测试步骤

1. **更新CSV表格**
   - 修改 `GameCharacters.csv`
   - 修改 `GameModifiers.csv`
   - 修改 `GameFields.csv`
   - 执行 `__表格更改后执行脚本！！！！.bat`

2. **创建预制体**
   - 按照上述步骤创建所有预制体
   - 确保所有资源路径正确

3. **测试散射塔**
   - 在游戏中生成 ObjectID 10 的防御塔
   - 观察是否同时发射3颗子弹
   - 验证子弹是否能正常追踪和造成伤害

4. **测试减速塔**
   - 在游戏中生成 ObjectID 11 的防御塔
   - 观察子弹命中后是否产生减速区域
   - 验证敌人是否被减速（移动速度降低50%）

## 📝 五、扩展建议

### 添加更多塔类型
- **冰冻塔**: 完全冻结敌人
- **毒素塔**: 持续伤害DOT
- **闪电塔**: 链式攻击多个敌人
- **激光塔**: 持续伤害光束

### 升级系统
在配置表中可以添加升级ID字段：
```csv
ObjectID,ObjectName,ObjectType,BindPrefab,InitModifier,UpgradeToID
10,散射防御塔Lv1,Tower,CC_Object_Tower_Scatter,12001,12
12,散射防御塔Lv2,Tower,CC_Object_Tower_Scatter_Lv2,12001|16001,0
```

## 🐛 六、常见问题

### Q: 散射塔只发射一颗子弹？
A: 检查 `TowerAI_Scatter` 组件是否正确添加，`bulletCount` 是否设置为3。

### Q: 减速效果不生效？
A: 检查：
1. 子弹的 Init_Modifier_List 是否包含 14001
2. 场地预制体是否正确保存
3. GameFields.csv 是否正确配置

### Q: 子弹不追踪目标？
A: 确保子弹的 `IsFollowTarget.real_value` 设置为 true。

### Q: 场地效果不出现？
A: 检查 Resources/FieldPrefabs 路径是否正确，预制体命名是否匹配。

---

**配置完成后，你将拥有三种完全不同的防御塔类型！** 🎉

