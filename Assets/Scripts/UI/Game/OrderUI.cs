using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Cooking;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 单个订单UI项
    /// </summary>
    public class OrderUIItem : MonoBehaviour
    {
        [Header("UI组件")]
        public Image iconImage;
        public Text recipeNameText;
        public Text progressText;
        public Slider timerSlider;
        public GameObject warningEffect;
        public Image[] starIcons;
        
        private OrderData orderData;
        
        public void SetOrderData(OrderData data)
        {
            orderData = data;
            UpdateUI();
        }
        
        public void UpdateUI()
        {
            if (orderData == null) return;
            
            // 配方名称
            if (recipeNameText != null)
            {
                recipeNameText.text = orderData.recipe.recipeName;
            }
            
            // 配方图标
            if (iconImage != null && orderData.recipe.icon != null)
            {
                iconImage.sprite = orderData.recipe.icon;
            }
            
            // 进度（大订单）
            if (progressText != null)
            {
                if (orderData.orderType == OrderType.Large)
                {
                    progressText.text = $"{orderData.completedCount}/{orderData.targetCount}";
                }
                else
                {
                    progressText.text = "";
                }
            }
            
            // 倒计时
            if (timerSlider != null && orderData.timeLimit > 0)
            {
                float timeProgress = orderData.GetTimeProgress();
                timerSlider.value = 1f - timeProgress;
                timerSlider.gameObject.SetActive(true);
                
                // 快超时时显示警告
                if (warningEffect != null)
                {
                    warningEffect.SetActive(timeProgress > 0.7f);
                }
            }
            else if (timerSlider != null)
            {
                timerSlider.gameObject.SetActive(false);
            }
            
            // 星级
            if (starIcons != null)
            {
                for (int i = 0; i < starIcons.Length; i++)
                {
                    if (starIcons[i] != null)
                    {
                        starIcons[i].gameObject.SetActive(i < orderData.recipe.starLevel);
                    }
                }
            }
        }
        
        private void Update()
        {
            if (orderData != null && !orderData.IsExpired())
            {
                UpdateUI();
            }
        }
    }
    
    /// <summary>
    /// 订单列表UI
    /// </summary>
    public class OrderUI : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private Transform smallOrderContainer;
        [SerializeField] private Transform largeOrderContainer;
        [SerializeField] private GameObject orderItemPrefab;
        
        [Header("大订单完成UI")]
        [SerializeField] private GameObject victoryPanel;
        
        private OrderManager orderManager;
        private Dictionary<int, OrderUIItem> orderUIItems = new Dictionary<int, OrderUIItem>();
        
        private void Start()
        {
            orderManager = OrderManager.Instance;
            
            if (orderManager != null)
            {
                orderManager.OnOrderCreated += OnOrderCreated;
                orderManager.OnOrderCompleted += OnOrderCompleted;
                orderManager.OnOrderExpired += OnOrderExpired;
            }
            
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(false);
            }
        }
        
        private void OnOrderCreated(object sender, OrderManager.OrderEventArgs e)
        {
            CreateOrderUI(e.order);
        }
        
        private void OnOrderCompleted(object sender, OrderManager.OrderEventArgs e)
        {
            RemoveOrderUI(e.order.orderID);
            
            // 检查是否所有大订单都完成
            if (orderManager.AreAllLargeOrdersCompleted() && victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }
        }
        
        private void OnOrderExpired(object sender, OrderManager.OrderEventArgs e)
        {
            RemoveOrderUI(e.order.orderID);
        }
        
        private void CreateOrderUI(OrderData orderData)
        {
            if (orderItemPrefab == null) return;
            
            Transform container = orderData.orderType == OrderType.Small ? 
                                 smallOrderContainer : largeOrderContainer;
            
            if (container == null) return;
            
            GameObject itemObj = Instantiate(orderItemPrefab, container);
            OrderUIItem item = itemObj.GetComponent<OrderUIItem>();
            
            if (item != null)
            {
                item.SetOrderData(orderData);
                orderUIItems[orderData.orderID] = item;
            }
        }
        
        private void RemoveOrderUI(int orderID)
        {
            if (orderUIItems.TryGetValue(orderID, out OrderUIItem item))
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
                orderUIItems.Remove(orderID);
            }
        }
        
        private void OnDestroy()
        {
            if (orderManager != null)
            {
                orderManager.OnOrderCreated -= OnOrderCreated;
                orderManager.OnOrderCompleted -= OnOrderCompleted;
                orderManager.OnOrderExpired -= OnOrderExpired;
            }
        }
    }
}

