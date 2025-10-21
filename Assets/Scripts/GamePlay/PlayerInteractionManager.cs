using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Core;

/// <summary>
/// 玩家交互管理器
/// 负责判断玩家是否可以与周围环境进行交互
/// </summary>
public class PlayerInteractionManager : MonoBehaviour
{
    private PlayerCharacterCtrl playerCtrl;

    private void Start()
    {
        // 获取玩家角色组件
        playerCtrl = GetComponent<PlayerCharacterCtrl>();
        if (playerCtrl == null)
        {
            Debug.LogError("PlayerInteractionManager 必须附加在 PlayerCharacterCtrl 对象上！");
        }
    }

    /// <summary>
    /// 检查玩家是否可以执行交互操作
    /// </summary>
    public bool CanInteract()
    {
        if (playerCtrl == null)
        {
            return false;
        }

        // 1. 手上有东西 → 始终可以互动（可以丢掉或放到建筑上）
        if (HasItemInHand())
        {
            return true;
        }

        // 2. 空手时，只有以下情况可以互动：
        //    - 靠近可拾取的物品
        //    - 靠近完成了菜品制作的厨房（可以拿成品）
        if (HasNearbyGrabbableItem() || HasNearbyFinishedDish())
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 检查手上是否有物品
    /// </summary>
    private bool HasItemInHand()
    {
        return playerCtrl.nowAttachList.Count > 0;
    }

    /// <summary>
    /// 检查附近是否有可拾取的物品（地上的食材/菜品）
    /// </summary>
    private bool HasNearbyGrabbableItem()
    {
        // 检查 Dish 类型的物品
        if (LevelManager.Instance.Character_Dict.ContainsKey(InGameCharacterType.Dish))
        {
            List<CharacterCtrlBase> dishList = LevelManager.Instance.Character_Dict[InGameCharacterType.Dish];

            // 清理已销毁的对象
            dishList.RemoveAll(item => item == null);

            foreach (var item in dishList)
            {
                // 检查距离是否在拾取范围内，且物品未被附加
                float distance = (item.transform.position - playerCtrl.transform.position).magnitude;
                if (distance <= playerCtrl.grabDistance.GetValue() && !item.isAttachedToOther)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 检查附近是否有完成了菜品制作的厨房（可以拿成品）
    /// </summary>
    private bool HasNearbyFinishedDish()
    {
        // 获取玩家当前选中的格子
        Vector3Int sel = playerCtrl.MySelectTarget;

        // 如果玩家选中了有效格子
        if (sel.x != -999 && sel.y != -999 && LevelGridGenerator.Instance.tile_dictionary.ContainsKey(sel))
        {
            LevelGridTileObject tileObj = LevelGridGenerator.Instance.tile_dictionary[sel];

            // 检查是否是锅（CookType > 0）且有成品菜
            // NowRecipeID == 0 表示没有正在烹饪（要么还没开始，要么已经做完）
            // nowAttachList.Count > 0 表示锅上有东西
            if (tileObj.CookType.GetValue() > 0 && tileObj.NowRecipeID == 0 && tileObj.nowAttachList.Count > 0)
            {
                return true;
            }
        }

        return false;
    }
}

