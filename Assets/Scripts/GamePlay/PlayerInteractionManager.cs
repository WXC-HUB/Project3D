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
        return GetInteractionType() != InteractionType.None;
    }
    
    /// <summary>
    /// 获取当前的互动类型
    /// </summary>
    public InteractionType GetInteractionType()
    {
        if (playerCtrl == null)
        {
            return InteractionType.None;
        }

        // 1. 检查是否选中了有效的网格对象
        Vector3Int sel = playerCtrl.MySelectTarget;
        if (sel.x != -999 && sel.y != -999 && LevelGridGenerator.Instance.tile_dictionary.ContainsKey(sel))
        {
            LevelGridTileObject tileObj = LevelGridGenerator.Instance.tile_dictionary[sel];
            
            // 使用网格对象的 CanInteractWithPlayer 方法判断
            InteractionType interactionType = tileObj.CanInteractWithPlayer(playerCtrl);
            if (interactionType != InteractionType.None)
            {
                return interactionType;
            }
        }

        // 2. 如果手上有东西，始终可以互动（可以丢到地上）
        // 注意：这里返回 Place 类型，表示可以放置物品到地上
        if (HasItemInHand())
        {
            return InteractionType.Place;
        }

        // 3. 空手时，检查附近是否有可拾取的物品
        if (HasNearbyGrabbableItem())
        {
            return InteractionType.PickUp;
        }

        return InteractionType.None;
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
}

