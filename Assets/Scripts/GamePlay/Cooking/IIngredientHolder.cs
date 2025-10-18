using UnityEngine;

namespace Assets.Scripts.Cooking
{
    /// <summary>
    /// 可以持有食材的接口 - 玩家、厨具、仓库等
    /// </summary>
    public interface IIngredientHolder
    {
        /// <summary>
        /// 设置当前持有的食材
        /// </summary>
        void SetCurrentIngredient(Ingredient ingredient);
        
        /// <summary>
        /// 获取当前持有的食材
        /// </summary>
        Ingredient GetCurrentIngredient();
        
        /// <summary>
        /// 获取食材的放置位置Transform
        /// </summary>
        Transform GetHoldTransform();
        
        /// <summary>
        /// 是否可以接收此食材
        /// </summary>
        bool CanAcceptIngredient(Ingredient ingredient);
    }
}

