using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Cooking
{
    /// <summary>
    /// 仓库 - 存储所有获得的食材
    /// </summary>
    public class StorageCounter : MonoBehaviour
    {
        [Header("仓库配置")]
        [SerializeField] private int maxCapacity = 99; // 每种食材的最大数量
        [SerializeField] private float interactionRange = 2f;
        
        // 食材类型 -> 数量
        private Dictionary<IngredientType, int> storedIngredients = new Dictionary<IngredientType, int>();
        
        public event EventHandler<StorageChangedEventArgs> OnStorageChanged;
        
        public class StorageChangedEventArgs : EventArgs
        {
            public IngredientType ingredientType;
            public int newCount;
            public bool wasAdded; // true=添加, false=移除
        }
        
        #region Storage Management
        
        /// <summary>
        /// 添加食材到仓库
        /// </summary>
        public bool AddIngredient(IngredientType type, int count = 1)
        {
            if (!storedIngredients.ContainsKey(type))
            {
                storedIngredients[type] = 0;
            }
            
            int newCount = Mathf.Min(storedIngredients[type] + count, maxCapacity);
            int actualAdded = newCount - storedIngredients[type];
            
            if (actualAdded > 0)
            {
                storedIngredients[type] = newCount;
                
                OnStorageChanged?.Invoke(this, new StorageChangedEventArgs
                {
                    ingredientType = type,
                    newCount = newCount,
                    wasAdded = true
                });
                
                Debug.Log($"Added {actualAdded} {type} to storage. Total: {newCount}");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 从仓库移除食材
        /// </summary>
        public bool RemoveIngredient(IngredientType type, int count = 1)
        {
            if (!storedIngredients.ContainsKey(type) || storedIngredients[type] < count)
            {
                Debug.Log($"Not enough {type} in storage!");
                return false;
            }
            
            storedIngredients[type] -= count;
            
            OnStorageChanged?.Invoke(this, new StorageChangedEventArgs
            {
                ingredientType = type,
                newCount = storedIngredients[type],
                wasAdded = false
            });
            
            Debug.Log($"Removed {count} {type} from storage. Remaining: {storedIngredients[type]}");
            return true;
        }
        
        /// <summary>
        /// 获取指定食材的数量
        /// </summary>
        public int GetIngredientCount(IngredientType type)
        {
            return storedIngredients.ContainsKey(type) ? storedIngredients[type] : 0;
        }
        
        /// <summary>
        /// 检查是否有足够的食材
        /// </summary>
        public bool HasEnough(IngredientType type, int count)
        {
            return GetIngredientCount(type) >= count;
        }
        
        /// <summary>
        /// 获取所有存储的食材
        /// </summary>
        public Dictionary<IngredientType, int> GetAllIngredients()
        {
            return new Dictionary<IngredientType, int>(storedIngredients);
        }
        
        /// <summary>
        /// 清空仓库
        /// </summary>
        public void ClearStorage()
        {
            storedIngredients.Clear();
        }
        
        #endregion
        
        #region Player Interaction
        
        /// <summary>
        /// 玩家与仓库交互 - 取出食材到背包
        /// </summary>
        public bool TakeIngredientToInventory(IngredientType type, PlayerInventory inventory, IngredientData ingredientData)
        {
            if (!HasEnough(type, 1))
            {
                Debug.Log($"Storage doesn't have {type}");
                return false;
            }
            
            if (inventory.IsFull)
            {
                Debug.Log("Player inventory is full!");
                return false;
            }
            
            // 从仓库移除
            if (RemoveIngredient(type, 1))
            {
                // 生成食材实例并添加到玩家背包
                Ingredient ingredient = Ingredient.Spawn(ingredientData, transform.position);
                return inventory.AddIngredient(ingredient);
            }
            
            return false;
        }
        
        /// <summary>
        /// 玩家与仓库交互 - 存入食材
        /// </summary>
        public bool StoreIngredientFromInventory(Ingredient ingredient, PlayerInventory inventory)
        {
            if (ingredient == null) return false;
            
            IngredientType type = ingredient.GetIngredientType();
            
            if (AddIngredient(type, 1))
            {
                inventory.RemoveIngredient(ingredient);
                ingredient.DestroySelf();
                return true;
            }
            
            return false;
        }
        
        #endregion
        
        #region Debug
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
        
        #endregion
    }
}

