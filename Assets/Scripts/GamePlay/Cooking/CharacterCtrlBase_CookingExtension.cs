using UnityEngine;
using Assets.Scripts.Cooking;

/// <summary>
/// CharacterCtrlBase的烹饪系统扩展
/// 为角色死亡事件添加食材掉落支持
/// </summary>
public static class CharacterCtrlBase_CookingExtension
{
    /// <summary>
    /// 初始化烹饪系统扩展（在CharacterCtrlBase.Start中调用）
    /// </summary>
    public static void InitCookingExtension(this CharacterCtrlBase character)
    {
        // 获取IngredientDropper组件
        IngredientDropper dropper = character.GetComponent<IngredientDropper>();
        
        if (dropper != null)
        {
            // 这里可以监听死亡事件
            // 由于你们已经有Die方法，可以直接在那里调用dropper.DropIngredients()
        }
    }
}

