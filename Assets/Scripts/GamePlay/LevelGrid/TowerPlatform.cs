using Assets.Scripts.Core;
using System.Linq;
using UnityEngine;

/// <summary>
/// 防御塔平台，负责给防御塔添加菜品Buff
/// </summary>
public class TowerPlatform : LevelGridTileObject
{
    /// <summary>
    /// 重写玩家交互判断方法，返回具体的互动类型
    /// </summary>
    public override InteractionType CanInteractWithPlayer(PlayerCharacterCtrl player)
    {
        // 1. 如果玩家手上有菜品，并且平台上有防御塔，可以互动（给防御塔添加Buff）
        if (player.nowAttachList.Count > 0)
        {
            CharacterCtrlBase itemInHand = player.nowAttachList[0];
            Dish dish_info = GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.GameCharacter == itemInHand.MyGameObjectID);
            
            // 如果手上的是菜品，并且平台上有防御塔
            if (dish_info != null && this.nowAttachList.Any(x => x.MyObjectLayer is InGameCharacterType.Tower))
            {
                return InteractionType.AddBuffToTower;
            }
        }
        
        // 其他情况使用基类逻辑（可以放置防御塔或拿起物品）
        return base.CanInteractWithPlayer(player);
    }
    
    /// <summary>
    /// 重写附加物品方法，处理给防御塔添加Buff的逻辑
    /// </summary>
    public override bool TryAttachObject(CharacterCtrlBase attach_obj)
    {
        bool is_dish = !(null == GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.GameCharacter == attach_obj.MyGameObjectID));
        
        // 如果是菜品且平台上有防御塔，则给防御塔添加Buff
        if (is_dish && this.nowAttachList.Any(x => x.MyObjectLayer is InGameCharacterType.Tower))
        {
            return TryAddBuffToTower(attach_obj);
        }
        
        // 其他情况使用基类的默认逻辑
        return base.TryAttachObject(attach_obj);
    }

    /// <summary>
    /// 尝试给防御塔添加菜品Buff
    /// </summary>
    bool TryAddBuffToTower(CharacterCtrlBase attach_obj)
    {
        Dish dish_config = GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.GameCharacter == attach_obj.MyGameObjectID);
        if (dish_config == null)
        {
            return false;
        }

        bool canAddBuff = false;
        
        // 遍历菜品的所有Buff
        foreach (string buff_info in dish_config.OnEatBuffList)
        {
            var buff_p = buff_info.Split(';');
            if (buff_p.Length < 2)
            {
                continue;
            }

            int b_id = int.Parse(buff_p[0]);
            float b_time = float.Parse(buff_p[1]);
            
            // 给所有防御塔添加Buff
            foreach (var tower in this.nowAttachList)
            {
                if (tower.MyObjectLayer is InGameCharacterType.Tower)
                {
                    SkillDispatchCenter.Instance.AddModifierToCharacter(tower, b_time, b_id);
                }
            }
            canAddBuff = true;
        }

        if (canAddBuff)
        {
            attach_obj.Die();
            Debug.Log($"成功给防御塔添加菜品Buff: {dish_config.Name}");
            return true;
        }
        else
        {
            Debug.LogWarning($"菜品 {dish_config.Name} 没有绑定Buff！禁止提交");
            return false;
        }
    }
}

