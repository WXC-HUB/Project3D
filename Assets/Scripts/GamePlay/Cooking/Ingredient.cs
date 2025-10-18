using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Cooking
{
    /// <summary>
    /// 食材运行时对象 - 可被拾取、放置、烹饪
    /// </summary>
    public class Ingredient : MonoBehaviour
    {
        [SerializeField] private IngredientData ingredientData;
        
        private IIngredientHolder currentHolder;
        private float dropTimestamp;
        private const float AUTO_DESPAWN_TIME = 30f; // 30秒后自动消失
        
        public IngredientData GetIngredientData() => ingredientData;
        public IngredientType GetIngredientType() => ingredientData.ingredientType;
        
        private void Start()
        {
            dropTimestamp = Time.time;
        }
        
        private void Update()
        {
            // 如果没有被持有，超时后自动消失
            if (currentHolder == null && Time.time - dropTimestamp > AUTO_DESPAWN_TIME)
            {
                DestroySelf();
            }
        }
        
        #region Holder Logic
        
        public void SetHolder(IIngredientHolder newHolder)
        {
            // 清除旧持有者
            currentHolder?.SetCurrentIngredient(null);
            
            // 设置新持有者
            newHolder?.SetCurrentIngredient(this);
            
            currentHolder = newHolder;
            
            if (newHolder != null)
            {
                // 设置Transform
                transform.SetParent(newHolder.GetHoldTransform());
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                
                // 禁用物理和碰撞
                if (TryGetComponent<Rigidbody2D>(out var rb))
                {
                    rb.simulated = false;
                }
                if (TryGetComponent<Collider2D>(out var col))
                {
                    col.enabled = false;
                }
            }
            else
            {
                transform.SetParent(null);
                
                // 启用物理和碰撞
                if (TryGetComponent<Rigidbody2D>(out var rb))
                {
                    rb.simulated = true;
                }
                if (TryGetComponent<Collider2D>(out var col))
                {
                    col.enabled = true;
                }
            }
        }
        
        public IIngredientHolder GetHolder() => currentHolder;
        
        #endregion
        
        public void DestroySelf()
        {
            SetHolder(null);
            Destroy(gameObject);
        }
        
        /// <summary>
        /// 生成食材
        /// </summary>
        public static Ingredient Spawn(IngredientData data, Vector3 position, IIngredientHolder holder = null)
        {
            if (data.prefab == null)
            {
                Debug.LogError($"Ingredient {data.ingredientName} has no prefab!");
                return null;
            }
            
            GameObject spawned = Instantiate(data.prefab, position, Quaternion.identity);
            Ingredient ingredient = spawned.GetComponent<Ingredient>();
            
            if (ingredient == null)
            {
                ingredient = spawned.AddComponent<Ingredient>();
                ingredient.ingredientData = data;
            }
            
            if (holder != null)
            {
                ingredient.SetHolder(holder);
            }
            
            return ingredient;
        }
    }
}

