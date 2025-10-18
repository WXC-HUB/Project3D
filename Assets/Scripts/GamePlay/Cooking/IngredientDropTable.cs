using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Cooking
{
    /// <summary>
    /// 掉落条目
    /// </summary>
    [System.Serializable]
    public class DropEntry
    {
        public IngredientData ingredientData;
        [Range(0f, 1f)]
        public float dropChance = 1.0f; // 掉落概率
        [Range(1, 5)]
        public int minCount = 1; // 最小掉落数量
        [Range(1, 5)]
        public int maxCount = 1; // 最大掉落数量
    }
    
    /// <summary>
    /// 食材掉落表 - ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "IngredientDropTable", menuName = "Cooking/Ingredient Drop Table")]
    public class IngredientDropTable : ScriptableObject
    {
        [Header("掉落配置")]
        public List<DropEntry> normalDrops = new List<DropEntry>(); // 普通掉落
        public List<DropEntry> rareDrops = new List<DropEntry>(); // 稀有掉落（小概率）
        
        [Header("掉落参数")]
        [Range(0f, 1f)]
        public float rareDropChance = 0.1f; // 稀有掉落的触发概率
        
        /// <summary>
        /// 随机掉落食材
        /// </summary>
        public List<IngredientData> RollDrops()
        {
            List<IngredientData> result = new List<IngredientData>();
            
            // 普通掉落
            foreach (var entry in normalDrops)
            {
                if (Random.value <= entry.dropChance)
                {
                    int count = Random.Range(entry.minCount, entry.maxCount + 1);
                    for (int i = 0; i < count; i++)
                    {
                        result.Add(entry.ingredientData);
                    }
                }
            }
            
            // 稀有掉落
            if (Random.value <= rareDropChance && rareDrops.Count > 0)
            {
                DropEntry rareEntry = rareDrops[Random.Range(0, rareDrops.Count)];
                if (Random.value <= rareEntry.dropChance)
                {
                    int count = Random.Range(rareEntry.minCount, rareEntry.maxCount + 1);
                    for (int i = 0; i < count; i++)
                    {
                        result.Add(rareEntry.ingredientData);
                    }
                }
            }
            
            return result;
        }
    }
}

