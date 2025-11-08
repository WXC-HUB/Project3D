using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TowerAttackRange : MonoBehaviour
{
    public List<Vector2Int> attackRange = new List<Vector2Int>();
    
    /// <summary>
    /// 判断目标是否在防御塔攻击范围内
    /// </summary>
    /// <param name="towerTransform">防御塔的Transform</param>
    /// <param name="towerDirection">防御塔朝向 (上下左右)</param>
    /// <param name="targetTransform">受攻击物体的Transform</param>
    /// <param name="tilemap">单元格地图</param>
    /// <param name="attackRange">攻击范围相对坐标列表</param>
    /// <returns>目标是否在攻击范围内</returns>
    public bool IsTargetInAttackRange(
        Transform towerTransform,
        Vector2 towerDirection,
        Transform targetTransform,
        Tilemap tilemap)
    {
        if (towerTransform == null || targetTransform == null || tilemap == null || attackRange == null)
            return false;

        // 获取防御塔在Tilemap中的单元格位置
        Vector3Int towerCellPos = tilemap.WorldToCell(towerTransform.position);

        // 获取目标在Tilemap中的单元格位置
        Vector3Int targetCellPos = tilemap.WorldToCell(targetTransform.position);

        // 计算目标相对于防御塔的单元格偏移
        Vector2Int relativeOffset = new Vector2Int(
            targetCellPos.x - towerCellPos.x,
            targetCellPos.y - towerCellPos.y
        );

        // 根据防御塔朝向旋转偏移坐标
        Vector2Int rotatedOffset = RotateOffsetByDirection(relativeOffset, towerDirection);


        DrawAttackRangeDebug(towerTransform , towerDirection , tilemap , attackRange , GetColorFromTransform(transform));
        // 检查旋转后的偏移是否在攻击范围内
        return attackRange.Contains(rotatedOffset);
    }

    /// <summary>
    /// 根据防御塔朝向旋转偏移坐标
    /// </summary>
    private static Vector2Int RotateOffsetByDirection(Vector2Int offset, Vector2 direction)
    {
        if (direction == Vector2.up) // 上
        {
            // 不需要旋转，保持原样
            return offset;
        }
        else if (direction == Vector2.down) // 下
        {
            // 180度旋转
            return new Vector2Int(-offset.x, -offset.y);
        }
        else if (direction == Vector2.left) // 左
        {
            // 逆时针旋转90度
            return new Vector2Int(offset.y, -offset.x);
        }
        else if (direction == Vector2.right) // 右
        {
            // 顺时针旋转90度
            return new Vector2Int(-offset.y, offset.x);
        }

        // 默认情况，不旋转
        return offset;
    }

    /// <summary>
    /// 绘制攻击范围的Debug图形（每帧调用）
    /// </summary>
    public void DrawAttackRangeDebug(
        Transform towerTransform,
        Vector2 towerDirection,
        Tilemap tilemap,
        List<Vector2Int> attackRange,
        Color rangeColor)
    {
        if (towerTransform == null || tilemap == null || attackRange == null)
            return;

        // 获取防御塔在Tilemap中的单元格位置
        Vector3Int towerCellPos = tilemap.WorldToCell(towerTransform.position);

        // 绘制防御塔位置（绿色）
        DrawCell(tilemap, towerCellPos, Color.green, 0.1f);

        // 绘制每个攻击范围单元格
        foreach (Vector2Int rangeOffset in attackRange)
        {
            // 根据防御塔朝向旋转偏移坐标（反向旋转，因为我们要从基础范围转换到实际方向）
            Vector2Int rotatedOffset = RotateOffsetForDrawing(rangeOffset, towerDirection);

            Vector3Int attackCellPos = new Vector3Int(
                towerCellPos.x + rotatedOffset.x,
                towerCellPos.y + rotatedOffset.y,
                towerCellPos.z
            );

            // 绘制攻击范围单元格
            DrawCell(tilemap, attackCellPos, rangeColor, 0.05f);

            // 绘制从防御塔到攻击范围的连线
            Vector3 towerWorldPos = tilemap.GetCellCenterWorld(towerCellPos);
            Vector3 attackWorldPos = tilemap.GetCellCenterWorld(attackCellPos);
            Debug.DrawLine(towerWorldPos, attackWorldPos, rangeColor * 0.7f, 0f, false);
        }

        // 绘制防御塔朝向箭头
        DrawDirectionArrow(towerTransform.position, towerDirection, Color.yellow);
    }

    public static Color GetColorFromTransform(Transform transform)
    {
        int id = transform.GetInstanceID();
        // 使用一个哈希函数，将id映射到0~1之间
        float r = -(float)((id * 0.1) % 1.0);
        float g = -(float)((id * 0.2) % 1.0);
        float b = -(float)((id * 0.3) % 1.0);
        return new Color(r, g, b);
    }

    /// <summary>
    /// 绘制单个单元格
    /// </summary>
    private static void DrawCell(Tilemap tilemap, Vector3Int cellPos, Color color, float duration = 0f)
    {
        Vector3 cellCenter = tilemap.GetCellCenterWorld(cellPos);
        Vector3 cellSize = tilemap.cellSize;

        // 计算单元格的四个角
        Vector3 halfSize = cellSize * 0.5f;
        Vector3 bottomLeft = cellCenter - halfSize;
        Vector3 bottomRight = cellCenter + new Vector3(halfSize.x, -halfSize.y, 0);
        Vector3 topLeft = cellCenter + new Vector3(-halfSize.x, halfSize.y, 0);
        Vector3 topRight = cellCenter + halfSize;

        // 绘制单元格边框
        Debug.DrawLine(bottomLeft, bottomRight, color, duration, false);
        Debug.DrawLine(bottomRight, topRight, color, duration, false);
        Debug.DrawLine(topRight, topLeft, color, duration, false);
        Debug.DrawLine(topLeft, bottomLeft, color, duration, false);

        // 绘制对角线（可选，让范围更明显）
        Debug.DrawLine(bottomLeft, topRight, color * 0.8f, duration, false);
        Debug.DrawLine(bottomRight, topLeft, color * 0.8f, duration, false);
    }

    /// <summary>
    /// 绘制防御塔朝向箭头
    /// </summary>
    private static void DrawDirectionArrow(Vector3 position, Vector2 direction, Color color, float arrowSize = 0.5f)
    {
        Vector3 startPos = position;
        Vector3 endPos = startPos + (Vector3)direction * arrowSize;

        // 绘制主箭头线
        Debug.DrawLine(startPos, endPos, color, 0f, false);

        // 绘制箭头两侧
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0) * 0.2f;
        Debug.DrawLine(endPos, endPos - (Vector3)direction * 0.2f + perpendicular, color, 0f, false);
        Debug.DrawLine(endPos, endPos - (Vector3)direction * 0.2f - perpendicular, color, 0f, false);
    }

    /// <summary>
    /// 为绘制而旋转偏移坐标（与判断逻辑中的旋转相反）
    /// </summary>
    private static Vector2Int RotateOffsetForDrawing(Vector2Int offset, Vector2 direction)
    {
        if (direction == Vector2.up) // 上
        {
            return offset;
        }
        else if (direction == Vector2.down) // 下
        {
            return new Vector2Int(-offset.x, -offset.y);
        }
        else if (direction == Vector2.left) // 左
        {
            return new Vector2Int(-offset.y, offset.x);
        }
        else if (direction == Vector2.right) // 右
        {
            return new Vector2Int(offset.y, -offset.x);
        }

        return offset;
    }
}