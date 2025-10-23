using Assets.Scripts.Core;
using UnityEngine;

/// <summary>
/// 出餐口，负责提交菜品
/// </summary>
public class DishOutlet : LevelGridTileObject
{
    public Character_Int DishOutletType = new Character_Int("DishOutletType", 0);

    private void Awake()
    {
        base.Awake();
        DishOutletType.TakeEffect(this);
    }

    /// <summary>
    /// 重写玩家交互判断方法，返回具体的互动类型
    /// </summary>
    public override InteractionType CanInteractWithPlayer(PlayerCharacterCtrl player)
    {
        // 出餐口的DishOutletType必须大于0才能交互
        if (this.DishOutletType.GetValue() <= 0)
        {
            return InteractionType.None;
        }
        
        // 如果玩家手上有菜品，可以互动（提交菜品）
        if (player.nowAttachList.Count > 0)
        {
            CharacterCtrlBase itemInHand = player.nowAttachList[0];
            Dish dish_info = GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.GameCharacter == itemInHand.MyGameObjectID);
            
            // 如果手上的是菜品，可以尝试提交
            if (dish_info != null)
            {
                return InteractionType.SubmitDish;
            }
        }
        
        // 其他情况不可互动（出餐口不需要拿取物品）
        return InteractionType.None;
    }
    
    /// <summary>
    /// 尝试提交菜品
    /// </summary>
    bool TrySubmitDish(CharacterCtrlBase attach_obj)
    {
        // 获取菜品ID
        Dish dish_config = GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.GameCharacter == attach_obj.MyGameObjectID);
        if (dish_config == null)
        {
            return false;
        }

        int dishId = dish_config.DishID;
        
        // 检查是否是允许提交的菜品
        if (!DishSubmissionManager.Instance.GetAllDishIds().Contains(dishId))
        {
            Debug.LogWarning($"菜品 ID {dishId} ({dish_config.Name}) 不在允许提交的菜品列表中，无事发生");
        } 
        else 
        {
            // 提交菜品到管理器
            DishSubmissionManager.Instance.AddDishSubmission(dishId, 1);
            // 获取提交后的数量
            int currentCount = DishSubmissionManager.Instance.GetDishCount(dishId);
            Debug.LogWarning($"成功提交菜品 ID: {dishId}, 名称: {dish_config.Name}, 当前数量: {currentCount}");
        }
        
        // 删除菜品GameObject
        attach_obj.Die();
        return true;
    }

    /// <summary>
    /// 重写附加物品方法
    /// </summary>
    public override bool TryAttachObject(CharacterCtrlBase attach_obj)
    {
        bool is_dish = !(null == GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.GameCharacter == attach_obj.MyGameObjectID));
        
        if (is_dish && this.DishOutletType.GetValue() > 0)
        {
            return TrySubmitDish(attach_obj);
        }
        
        // 其他情况使用基类逻辑
        return base.TryAttachObject(attach_obj);
    }
}

