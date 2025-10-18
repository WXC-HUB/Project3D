using System;
using UnityEngine;
using Assets.Scripts.Core;

namespace Assets.Scripts.Cooking
{
    /// <summary>
    /// 出餐口 - 提交完成的菜品
    /// </summary>
    public class DeliveryCounter : MonoBehaviour
    {
        [Header("出餐口配置")]
        [SerializeField] private Transform deliveryPoint;
        [SerializeField] private float interactionRange = 2f;
        
        [Header("特效")]
        [SerializeField] private ParticleSystem successEffect;
        [SerializeField] private ParticleSystem failEffect;
        
        public event EventHandler<DeliveryEventArgs> OnDeliverySuccess;
        public event EventHandler<DeliveryEventArgs> OnDeliveryFailed;
        
        public class DeliveryEventArgs : EventArgs
        {
            public RecipeData recipe;
            public OrderData matchedOrder;
            public bool wasLargeOrder;
        }
        
        private void Start()
        {
            if (deliveryPoint == null)
            {
                deliveryPoint = transform;
            }
        }
        
        /// <summary>
        /// 提交菜品
        /// </summary>
        public bool DeliverDish(RecipeData recipe)
        {
            if (recipe == null)
            {
                Debug.LogWarning("Cannot deliver null recipe!");
                return false;
            }
            
            // 尝试匹配订单
            OrderData matchedOrder = OrderManager.Instance.TryMatchAndCompleteOrder(recipe);
            
            if (matchedOrder != null)
            {
                // 匹配成功
                OnDeliverySuccess?.Invoke(this, new DeliveryEventArgs
                {
                    recipe = recipe,
                    matchedOrder = matchedOrder,
                    wasLargeOrder = matchedOrder.orderType == OrderType.Large
                });
                
                PlaySuccessEffect();
                
                // 根据订单类型给予奖励
                if (matchedOrder.orderType == OrderType.Small)
                {
                    // 小订单 - 可以喂给防御塔
                    Debug.Log($"Small order completed: {recipe.recipeName}. Can feed to tower!");
                }
                else
                {
                    // 大订单 - 关卡目标
                    Debug.Log($"Large order progress: {recipe.recipeName}");
                    
                    // 检查是否所有大订单都完成了
                    if (OrderManager.Instance.AreAllLargeOrdersCompleted())
                    {
                        Debug.Log("All large orders completed! Level victory!");
                        OnLevelComplete();
                    }
                }
                
                return true;
            }
            else
            {
                // 没有匹配的订单
                OnDeliveryFailed?.Invoke(this, new DeliveryEventArgs
                {
                    recipe = recipe,
                    matchedOrder = null,
                    wasLargeOrder = false
                });
                
                PlayFailEffect();
                Debug.Log($"No matching order for {recipe.recipeName}");
                return false;
            }
        }
        
        /// <summary>
        /// 关卡完成
        /// </summary>
        private void OnLevelComplete()
        {
            // 触发关卡胜利事件
            // 可以通过LevelManager或事件系统通知
            Debug.Log("=== LEVEL COMPLETE ===");
            
            // 这里可以触发结算界面等
        }
        
        #region Effects
        
        private void PlaySuccessEffect()
        {
            if (successEffect != null)
            {
                successEffect.Play();
            }
        }
        
        private void PlayFailEffect()
        {
            if (failEffect != null)
            {
                failEffect.Play();
            }
        }
        
        #endregion
        
        #region Debug
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
        
        #endregion
    }
}

