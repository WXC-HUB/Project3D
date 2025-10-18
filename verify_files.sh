#!/bin/bash
echo "=== 验证所有文件 ==="
echo ""

files=(
"Assets/Scripts/GamePlay/Level/LevelHealthSystem.cs"
"Assets/Scripts/GamePlay/Level/KitchenZoneTrigger.cs"
"Assets/Scripts/GamePlay/Tower/TowerController.cs"
"Assets/Scripts/GamePlay/AI/MonsterCombatBehavior.cs"
"Assets/Scripts/GamePlay/Combat/AttackType.cs"
"Assets/Scripts/GamePlay/Combat/DamageCalculator.cs"
"Assets/Scripts/UI/Game/LevelHealthUI.cs"
)

for file in "${files[@]}"; do
  if [ -f "$file" ]; then
    size=$(wc -c < "$file")
    lines=$(wc -l < "$file")
    echo "✓ $file"
    echo "  大小: $size bytes, 行数: $lines"
  else
    echo "✗ $file 不存在！"
  fi
  echo ""
done

echo "=== 检查命名空间 ==="
grep -h "^namespace" Assets/Scripts/GamePlay/Level/*.cs Assets/Scripts/GamePlay/Combat/*.cs Assets/Scripts/GamePlay/AI/MonsterCombatBehavior.cs 2>/dev/null | sort -u

echo ""
echo "=== 检查using语句 ==="
echo "KitchenZoneTrigger.cs:"
head -4 Assets/Scripts/GamePlay/Level/KitchenZoneTrigger.cs | grep "^using"

echo ""
echo "TowerController.cs:"
head -7 Assets/Scripts/GamePlay/Tower/TowerController.cs | grep "^using"

echo ""
echo "MonsterCombatBehavior.cs:"
head -4 Assets/Scripts/GamePlay/AI/MonsterCombatBehavior.cs | grep "^using"
