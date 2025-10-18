using UnityEngine;
using Assets.Scripts.Cooking;
using System.Collections.Generic;

/// <summary>
/// 烹饪系统使用示例
/// 演示如何在代码中使用烹饪系统
/// </summary>
public class CookingSystemExample : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private IngredientData tomatoData;
    [SerializeField] private IngredientData eggData;
    [SerializeField] private RecipeData tomatoEggRecipe;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private StorageCounter storage;
    [SerializeField] private StirFryAppliance stirFry;
    [SerializeField] private DeliveryCounter delivery;
    
    private void Start()
    {
        // 订阅事件
        SubscribeEvents();
        
        // 示例：创建测试订单
        CreateTestOrders();
    }
    
    private void SubscribeEvents()
    {
        // 订阅订单事件
        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.OnOrderCreated += OnOrderCreated;
            OrderManager.Instance.OnOrderCompleted += OnOrderCompleted;
            OrderManager.Instance.OnOrderExpired += OnOrderExpired;
        }
        
        // 订阅厨具事件
        if (stirFry != null)
        {
            stirFry.OnCookingProgressChanged += OnCookingProgress;
            stirFry.OnCookingComplete += OnCookingComplete;
        }
        
        // 订阅背包事件
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged += OnInventoryChanged;
        }
        
        // 订阅出餐事件
        if (delivery != null)
        {
            delivery.OnDeliverySuccess += OnDeliverySuccess;
            delivery.OnDeliveryFailed += OnDeliveryFailed;
        }
    }
    
    private void CreateTestOrders()
    {
        if (OrderManager.Instance == null || tomatoEggRecipe == null)
            return;
        
        // 创建一个小订单
        OrderManager.Instance.CreateSmallOrder(tomatoEggRecipe);
        
        // 创建一个大订单（关卡目标）
        OrderManager.Instance.CreateLargeOrder(tomatoEggRecipe, targetCount: 3);
        
        Debug.Log("Test orders created!");
    }
    
    #region Event Handlers
    
    private void OnOrderCreated(object sender, OrderManager.OrderEventArgs e)
    {
        Debug.Log($"[Order] Created: {e.order.recipe.recipeName} ({e.order.orderType})");
        
        // 这里可以更新UI，显示新订单
        // ShowOrderUI(e.order);
    }
    
    private void OnOrderCompleted(object sender, OrderManager.OrderEventArgs e)
    {
        Debug.Log($"[Order] Completed: {e.order.recipe.recipeName}");
        
        // 播放完成音效、特效
        // PlaySuccessEffect();
    }
    
    private void OnOrderExpired(object sender, OrderManager.OrderEventArgs e)
    {
        Debug.Log($"[Order] Expired: {e.order.recipe.recipeName}");
        
        // 播放失败音效
        // PlayFailureSound();
    }
    
    private void OnCookingProgress(object sender, KitchenApplianceBase.CookingProgressEventArgs e)
    {
        // 更新进度条
        // progressBar.fillAmount = e.progress;
        Debug.Log($"[Cooking] Progress: {e.progress * 100}%");
    }
    
    private void OnCookingComplete(object sender, Ingredient e)
    {
        Debug.Log($"[Cooking] Complete: {e.GetIngredientData().ingredientName}");
        
        // 播放完成音效
        // PlaySound("CookingComplete");
    }
    
    private void OnInventoryChanged(object sender, PlayerInventory.InventoryChangedEventArgs e)
    {
        Debug.Log($"[Inventory] Changed: {e.ingredients.Count} items");
        
        // 更新背包UI
        // UpdateInventoryUI(e.ingredients, e.selectedIndex);
    }
    
    private void OnDeliverySuccess(object sender, DeliveryCounter.DeliveryEventArgs e)
    {
        Debug.Log($"[Delivery] Success: {e.recipe.recipeName}");
        
        if (e.wasLargeOrder)
        {
            Debug.Log("Large order progress updated!");
        }
    }
    
    private void OnDeliveryFailed(object sender, DeliveryCounter.DeliveryEventArgs e)
    {
        Debug.Log($"[Delivery] Failed: {e.recipe.recipeName} - No matching order!");
    }
    
    #endregion
    
    #region Test Methods (用于测试，可以绑定到UI按钮)
    
    [ContextMenu("Test: Add Ingredients to Storage")]
    public void TestAddIngredientsToStorage()
    {
        if (storage == null) return;
        
        storage.AddIngredient(IngredientType.Tomato, 5);
        storage.AddIngredient(IngredientType.Egg, 5);
        storage.AddIngredient(IngredientType.Fish, 3);
        
        Debug.Log("Added test ingredients to storage");
    }
    
    [ContextMenu("Test: Spawn Ingredient")]
    public void TestSpawnIngredient()
    {
        if (tomatoData == null) return;
        
        Vector3 spawnPos = transform.position + Vector3.right * 2;
        Ingredient.Spawn(tomatoData, spawnPos);
        
        Debug.Log("Spawned test ingredient");
    }
    
    [ContextMenu("Test: Complete Full Recipe")]
    public void TestCompleteFullRecipe()
    {
        if (tomatoEggRecipe == null || delivery == null)
            return;
        
        // 模拟完成配方
        delivery.DeliverDish(tomatoEggRecipe);
    }
    
    [ContextMenu("Test: Print All Orders")]
    public void TestPrintAllOrders()
    {
        if (OrderManager.Instance == null) return;
        
        var orders = OrderManager.Instance.GetAllOrders();
        Debug.Log($"=== All Orders ({orders.Count}) ===");
        
        foreach (var order in orders)
        {
            Debug.Log($"- {order.recipe.recipeName} ({order.orderType}) " +
                     $"Progress: {order.completedCount}/{order.targetCount} " +
                     $"Time: {order.GetRemainingTime()}s");
        }
    }
    
    #endregion
    
    private void OnDestroy()
    {
        // 取消订阅
        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.OnOrderCreated -= OnOrderCreated;
            OrderManager.Instance.OnOrderCompleted -= OnOrderCompleted;
            OrderManager.Instance.OnOrderExpired -= OnOrderExpired;
        }
        
        if (stirFry != null)
        {
            stirFry.OnCookingProgressChanged -= OnCookingProgress;
            stirFry.OnCookingComplete -= OnCookingComplete;
        }
        
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= OnInventoryChanged;
        }
        
        if (delivery != null)
        {
            delivery.OnDeliverySuccess -= OnDeliverySuccess;
            delivery.OnDeliveryFailed -= OnDeliveryFailed;
        }
    }
}

