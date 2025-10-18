using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Cooking
{
    /// <summary>
    /// 配方步骤 - 单个食材的处理步骤
    /// </summary>
    [System.Serializable]
    public class RecipeStep
    {
        public IngredientType ingredient;
        public CookingMethodType cookingMethod;
        public float cookingTime = 5f; // 烹饪所需时间（秒）
    }
    
    /// <summary>
    /// 配方数据 - ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "RecipeData", menuName = "Cooking/Recipe Data")]
    public class RecipeData : ScriptableObject
    {
        [Header("基础信息")]
        public int recipeID;
        public string recipeName;
        public string description;
        
        [Header("配方星级")]
        [Range(1, 3)]
        public int starLevel = 1; // 1-3星
        
        [Header("视觉表现")]
        public Sprite icon;
        public GameObject dishPrefab; // 完成后的菜品Prefab
        
        [Header("配方内容")]
        public List<RecipeStep> steps = new List<RecipeStep>(); // 烹饪步骤
        
        [Header("奖励")]
        public int energyReward = 100; // 完成后给防御塔的能量值
        public float attackBonus = 1.0f; // 攻击加成
        public float durationBonus = 0f; // 持续时间加成（秒）
        
        /// <summary>
        /// 获取所需的所有食材类型（去重）
        /// </summary>
        public List<IngredientType> GetRequiredIngredients()
        {
            List<IngredientType> result = new List<IngredientType>();
            foreach (var step in steps)
            {
                if (!result.Contains(step.ingredient))
                {
                    result.Add(step.ingredient);
                }
            }
            return result;
        }
        
        /// <summary>
        /// 获取总烹饪时间
        /// </summary>
        public float GetTotalCookingTime()
        {
            float total = 0f;
            foreach (var step in steps)
            {
                total += step.cookingTime;
            }
            return total;
        }
        
        /// <summary>
        /// 检查是否符合配方要求
        /// </summary>
        public bool IsMatch(List<IngredientType> ingredients, List<CookingMethodType> methods)
        {
            if (ingredients.Count != steps.Count || methods.Count != steps.Count)
            {
                return false;
            }
            
            for (int i = 0; i < steps.Count; i++)
            {
                if (ingredients[i] != steps[i].ingredient || methods[i] != steps[i].cookingMethod)
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}

