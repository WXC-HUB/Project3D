using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Cooking
{
    /// <summary>
    /// 食材数据 - ScriptableObject配置
    /// </summary>
    [CreateAssetMenu(fileName = "IngredientData", menuName = "Cooking/Ingredient Data")]
    [System.Serializable]
    public class IngredientData : ScriptableObject
    {
        [Header("基础信息")]
        public IngredientType ingredientType;
        public string ingredientName;
        public string description;
        
        [Header("视觉表现")]
        public Sprite icon;
        public GameObject prefab;
        public Color iconColor = Color.white;
        
        [Header("游戏属性")]
        public bool isSpecialIngredient = false;  // 是否为特殊食材（稀有掉落）
        public float dropChance = 1.0f;            // 掉落概率（0-1）
        
        [Header("烹饪属性")]
        public bool canBeCooked = true;
        public List<CookingMethodType> allowedMethods = new List<CookingMethodType>(); // 允许的烹饪方法
    }
}

