using Assets.Scripts.Core;
using UnityEngine;

/// <summary>
/// 掉落物品基类，用于所有可掉落的物品（如食材、菜品等）
/// </summary>
public class DropItem : CharacterCtrlBase
{
    /// <summary>
    /// 掉落物品不应该接受附加物品
    /// </summary>
    public override bool TryAttachObject(CharacterCtrlBase attach_obj)
    {
        // 掉落物品通常不能附加其他物品
        return false;
    }
}

