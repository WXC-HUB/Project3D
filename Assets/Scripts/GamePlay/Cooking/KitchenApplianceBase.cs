using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Core;

namespace Assets.Scripts.Cooking
{
    /// <summary>
    /// 厨具基类 - 所有烹饪设备的基类
    /// </summary>
    public abstract class KitchenApplianceBase : MonoBehaviour, IIngredientHolder
    {
        [Header("厨具基础配置")]
        [SerializeField] protected CookingMethodType cookingMethod;
        [SerializeField] protected Transform ingredientHoldPoint;
        [SerializeField] protected float baseCookingTime = 5f;
        
        [Header("交互配置")]
        [SerializeField] protected float interactionRange = 2f;
        [SerializeField] protected LayerMask playerLayer;
        
        protected Ingredient currentIngredient;
        protected bool isCooking = false;
        protected float cookingProgress = 0f;
        protected float targetCookingTime = 0f;
        
        protected Queue<Ingredient> ingredientQueue = new Queue<Ingredient>(); // 食材队列
        protected Ingredient cookedResult; // 烹饪完成的结果
        
        #region Events
        
        public event EventHandler<CookingProgressEventArgs> OnCookingProgressChanged;
        public event EventHandler<Ingredient> OnCookingComplete;
        public event EventHandler<Ingredient> OnIngredientAdded;
        
        public class CookingProgressEventArgs : EventArgs
        {
            public float progress; // 0-1
            public bool isActive;
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        protected virtual void Start()
        {
            if (ingredientHoldPoint == null)
            {
                Debug.LogWarning($"{gameObject.name}: ingredientHoldPoint is null, using self transform");
                ingredientHoldPoint = transform;
            }
        }
        
        protected virtual void Update()
        {
            if (isCooking)
            {
                UpdateCooking();
            }
            
            // 检测玩家交互
            CheckPlayerInteraction();
        }
        
        #endregion
        
        #region IIngredientHolder Implementation
        
        public virtual void SetCurrentIngredient(Ingredient ingredient)
        {
            currentIngredient = ingredient;
        }
        
        public virtual Ingredient GetCurrentIngredient()
        {
            return currentIngredient;
        }
        
        public virtual Transform GetHoldTransform()
        {
            return ingredientHoldPoint;
        }
        
        public virtual bool CanAcceptIngredient(Ingredient ingredient)
        {
            // 检查食材是否允许此烹饪方法
            if (ingredient == null) return false;
            
            IngredientData data = ingredient.GetIngredientData();
            return data.canBeCooked && data.allowedMethods.Contains(cookingMethod);
        }
        
        #endregion
        
        #region Cooking Logic
        
        /// <summary>
        /// 添加食材到烹饪队列
        /// </summary>
        public virtual bool AddIngredient(Ingredient ingredient)
        {
            if (!CanAcceptIngredient(ingredient))
            {
                Debug.Log($"Cannot cook {ingredient.GetIngredientData().ingredientName} with {cookingMethod}");
                return false;
            }
            
            ingredient.SetHolder(this);
            ingredientQueue.Enqueue(ingredient);
            
            OnIngredientAdded?.Invoke(this, ingredient);
            
            // 如果当前没有在烹饪，开始烹饪
            if (!isCooking && currentIngredient == null)
            {
                StartNextCooking();
            }
            
            return true;
        }
        
        /// <summary>
        /// 开始烹饪下一个食材
        /// </summary>
        protected virtual void StartNextCooking()
        {
            if (ingredientQueue.Count == 0)
            {
                return;
            }
            
            currentIngredient = ingredientQueue.Dequeue();
            isCooking = true;
            cookingProgress = 0f;
            targetCookingTime = baseCookingTime;
            
            Debug.Log($"Started cooking {currentIngredient.GetIngredientData().ingredientName}");
        }
        
        /// <summary>
        /// 更新烹饪进度
        /// </summary>
        protected virtual void UpdateCooking()
        {
            if (currentIngredient == null)
            {
                isCooking = false;
                return;
            }
            
            cookingProgress += Time.deltaTime;
            
            // 发送进度事件
            OnCookingProgressChanged?.Invoke(this, new CookingProgressEventArgs
            {
                progress = Mathf.Clamp01(cookingProgress / targetCookingTime),
                isActive = true
            });
            
            // 检查是否完成
            if (cookingProgress >= targetCookingTime)
            {
                CompleteCooking();
            }
        }
        
        /// <summary>
        /// 完成烹饪
        /// </summary>
        protected virtual void CompleteCooking()
        {
            if (currentIngredient == null) return;
            
            Debug.Log($"Cooking complete: {currentIngredient.GetIngredientData().ingredientName}");
            
            cookedResult = currentIngredient;
            
            OnCookingComplete?.Invoke(this, cookedResult);
            
            // 重置状态
            currentIngredient = null;
            isCooking = false;
            cookingProgress = 0f;
            
            // 继续烹饪队列中的下一个
            if (ingredientQueue.Count > 0)
            {
                StartNextCooking();
            }
        }
        
        /// <summary>
        /// 拾取烹饪完成的结果
        /// </summary>
        public virtual Ingredient TakeResult()
        {
            Ingredient result = cookedResult;
            cookedResult = null;
            return result;
        }
        
        public virtual bool HasResult()
        {
            return cookedResult != null;
        }
        
        #endregion
        
        #region Interaction
        
        protected virtual void CheckPlayerInteraction()
        {
            // 这里可以检测玩家是否在范围内并按下交互键
            // 具体实现根据你们的输入系统
        }
        
        /// <summary>
        /// 玩家与厨具交互
        /// </summary>
        public virtual void OnPlayerInteract(PlayerCharacterCtrl player)
        {
            // 如果玩家持有食材，放入厨具
            // 如果厨具有完成的菜品，玩家拾取
            // 具体实现根据你们的玩家系统
            Debug.Log($"Player interacted with {cookingMethod} appliance");
        }
        
        #endregion
        
        #region Debug
        
        private void OnDrawGizmosSelected()
        {
            // 显示交互范围
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
        
        #endregion
    }
}

