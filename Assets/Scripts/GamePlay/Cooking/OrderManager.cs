using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.BaseUtils;
using Assets.Scripts.Core;

namespace Assets.Scripts.Cooking
{
    /// <summary>
    /// 订单管理器 - 管理所有订单的生成、完成、超时
    /// </summary>
    public class OrderManager : MonoSingleton<OrderManager>
    {
        [Header("订单配置")]
        [SerializeField] private int maxSmallOrders = 3; // 最多同时存在的小订单数
        [SerializeField] private float smallOrderInterval = 30f; // 小订单生成间隔
        [SerializeField] private float smallOrderTimeLimit = 60f; // 小订单时间限制
        
        [Header("配方池")]
        [SerializeField] private List<RecipeData> availableRecipes = new List<RecipeData>();
        
        private List<OrderData> smallOrders = new List<OrderData>();
        private List<OrderData> largeOrders = new List<OrderData>();
        
        private float nextSmallOrderTime = 0f;
        private int orderIDCounter = 0;
        
        #region Events
        
        public event EventHandler<OrderEventArgs> OnOrderCreated;
        public event EventHandler<OrderEventArgs> OnOrderCompleted;
        public event EventHandler<OrderEventArgs> OnOrderExpired;
        
        public class OrderEventArgs : EventArgs
        {
            public OrderData order;
        }
        
        #endregion
        
        private void Start()
        {
            base.Awake();
            
            // 初始化时生成一个小订单
            nextSmallOrderTime = Time.time + smallOrderInterval;
        }
        
        private void Update()
        {
            // 自动生成小订单
            if (Time.time >= nextSmallOrderTime && smallOrders.Count < maxSmallOrders)
            {
                CreateRandomSmallOrder();
                nextSmallOrderTime = Time.time + smallOrderInterval;
            }
            
            // 检查订单超时
            CheckExpiredOrders();
        }
        
        #region Order Creation
        
        /// <summary>
        /// 创建随机小订单
        /// </summary>
        public OrderData CreateRandomSmallOrder()
        {
            if (availableRecipes.Count == 0)
            {
                Debug.LogWarning("No available recipes!");
                return null;
            }
            
            RecipeData randomRecipe = availableRecipes[UnityEngine.Random.Range(0, availableRecipes.Count)];
            return CreateSmallOrder(randomRecipe);
        }
        
        /// <summary>
        /// 创建小订单
        /// </summary>
        public OrderData CreateSmallOrder(RecipeData recipe)
        {
            OrderData order = new OrderData
            {
                orderID = ++orderIDCounter,
                orderType = OrderType.Small,
                recipe = recipe,
                timeLimit = smallOrderTimeLimit,
                createdTime = Time.time,
                targetCount = 1,
                completedCount = 0
            };
            
            smallOrders.Add(order);
            OnOrderCreated?.Invoke(this, new OrderEventArgs { order = order });
            
            Debug.Log($"Created small order: {recipe.recipeName}");
            return order;
        }
        
        /// <summary>
        /// 创建大订单（关卡目标）
        /// </summary>
        public OrderData CreateLargeOrder(RecipeData recipe, int targetCount)
        {
            OrderData order = new OrderData
            {
                orderID = ++orderIDCounter,
                orderType = OrderType.Large,
                recipe = recipe,
                timeLimit = 0, // 大订单通常没有时间限制
                createdTime = Time.time,
                targetCount = targetCount,
                completedCount = 0
            };
            
            largeOrders.Add(order);
            OnOrderCreated?.Invoke(this, new OrderEventArgs { order = order });
            
            Debug.Log($"Created large order: {recipe.recipeName} x{targetCount}");
            return order;
        }
        
        #endregion
        
        #region Order Completion
        
        /// <summary>
        /// 完成订单
        /// </summary>
        public bool CompleteOrder(OrderData order)
        {
            if (order == null) return false;
            
            order.completedCount++;
            
            if (order.IsCompleted())
            {
                // 订单完全完成
                if (order.orderType == OrderType.Small)
                {
                    smallOrders.Remove(order);
                }
                else
                {
                    largeOrders.Remove(order);
                }
                
                OnOrderCompleted?.Invoke(this, new OrderEventArgs { order = order });
                Debug.Log($"Order completed: {order.recipe.recipeName}");
                
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 尝试匹配并完成订单
        /// </summary>
        public OrderData TryMatchAndCompleteOrder(RecipeData recipe)
        {
            // 优先匹配小订单
            foreach (var order in smallOrders)
            {
                if (order.recipe == recipe)
                {
                    CompleteOrder(order);
                    return order;
                }
            }
            
            // 然后匹配大订单
            foreach (var order in largeOrders)
            {
                if (order.recipe == recipe)
                {
                    CompleteOrder(order);
                    return order;
                }
            }
            
            return null;
        }
        
        #endregion
        
        #region Order Queries
        
        public List<OrderData> GetSmallOrders() => new List<OrderData>(smallOrders);
        public List<OrderData> GetLargeOrders() => new List<OrderData>(largeOrders);
        public List<OrderData> GetAllOrders()
        {
            List<OrderData> all = new List<OrderData>();
            all.AddRange(smallOrders);
            all.AddRange(largeOrders);
            return all;
        }
        
        public bool HasOrder(RecipeData recipe)
        {
            foreach (var order in GetAllOrders())
            {
                if (order.recipe == recipe)
                    return true;
            }
            return false;
        }
        
        #endregion
        
        #region Order Expiration
        
        private void CheckExpiredOrders()
        {
            // 检查小订单超时
            for (int i = smallOrders.Count - 1; i >= 0; i--)
            {
                if (smallOrders[i].IsExpired())
                {
                    OrderData expiredOrder = smallOrders[i];
                    smallOrders.RemoveAt(i);
                    
                    OnOrderExpired?.Invoke(this, new OrderEventArgs { order = expiredOrder });
                    Debug.Log($"Order expired: {expiredOrder.recipe.recipeName}");
                }
            }
        }
        
        #endregion
        
        #region Level Control
        
        /// <summary>
        /// 检查是否所有大订单都已完成
        /// </summary>
        public bool AreAllLargeOrdersCompleted()
        {
            return largeOrders.Count == 0 || largeOrders.TrueForAll(o => o.IsCompleted());
        }
        
        /// <summary>
        /// 清空所有订单
        /// </summary>
        public void ClearAllOrders()
        {
            smallOrders.Clear();
            largeOrders.Clear();
        }
        
        #endregion
    }
}

