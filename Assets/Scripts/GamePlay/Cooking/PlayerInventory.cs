using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Cooking
{
    /// <summary>
    /// 玩家背包系统 - 可以持有多个食材
    /// </summary>
    public class PlayerInventory : MonoBehaviour, IIngredientHolder
    {
        [Header("背包配置")]
        [SerializeField] private int maxCapacity = 6; // 最大容量
        [SerializeField] private Transform ingredientHoldPoint;
        
        private List<Ingredient> ingredients = new List<Ingredient>();
        private int currentSelectedIndex = 0;
        
        public event EventHandler<InventoryChangedEventArgs> OnInventoryChanged;
        
        public class InventoryChangedEventArgs : EventArgs
        {
            public List<Ingredient> ingredients;
            public int selectedIndex;
        }
        
        #region Properties
        
        public int CurrentCount => ingredients.Count;
        public int MaxCapacity => maxCapacity;
        public bool IsFull => ingredients.Count >= maxCapacity;
        public bool IsEmpty => ingredients.Count == 0;
        public Ingredient CurrentSelected => currentSelectedIndex < ingredients.Count ? ingredients[currentSelectedIndex] : null;
        
        #endregion
        
        private void Start()
        {
            if (ingredientHoldPoint == null)
            {
                ingredientHoldPoint = transform;
            }
        }
        
        #region IIngredientHolder Implementation
        
        public void SetCurrentIngredient(Ingredient ingredient)
        {
            // PlayerInventory使用List管理，不使用单一引用
        }
        
        public Ingredient GetCurrentIngredient()
        {
            return CurrentSelected;
        }
        
        public Transform GetHoldTransform()
        {
            return ingredientHoldPoint;
        }
        
        public bool CanAcceptIngredient(Ingredient ingredient)
        {
            return !IsFull;
        }
        
        #endregion
        
        #region Inventory Management
        
        /// <summary>
        /// 添加食材到背包
        /// </summary>
        public bool AddIngredient(Ingredient ingredient)
        {
            if (IsFull)
            {
                Debug.Log("Inventory is full!");
                return false;
            }
            
            ingredients.Add(ingredient);
            ingredient.SetHolder(this);
            
            UpdateVisuals();
            NotifyInventoryChanged();
            
            return true;
        }
        
        /// <summary>
        /// 移除指定食材
        /// </summary>
        public bool RemoveIngredient(Ingredient ingredient)
        {
            if (ingredients.Remove(ingredient))
            {
                // 调整选中索引
                if (currentSelectedIndex >= ingredients.Count && currentSelectedIndex > 0)
                {
                    currentSelectedIndex--;
                }
                
                UpdateVisuals();
                NotifyInventoryChanged();
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 移除当前选中的食材
        /// </summary>
        public Ingredient RemoveCurrentSelected()
        {
            if (CurrentSelected != null)
            {
                Ingredient ingredient = CurrentSelected;
                RemoveIngredient(ingredient);
                return ingredient;
            }
            
            return null;
        }
        
        /// <summary>
        /// 清空背包
        /// </summary>
        public void Clear()
        {
            foreach (var ingredient in ingredients)
            {
                if (ingredient != null)
                {
                    ingredient.DestroySelf();
                }
            }
            
            ingredients.Clear();
            currentSelectedIndex = 0;
            
            UpdateVisuals();
            NotifyInventoryChanged();
        }
        
        /// <summary>
        /// 获取所有食材
        /// </summary>
        public List<Ingredient> GetAllIngredients()
        {
            return new List<Ingredient>(ingredients);
        }
        
        /// <summary>
        /// 获取指定类型的食材数量
        /// </summary>
        public int GetIngredientCount(IngredientType type)
        {
            int count = 0;
            foreach (var ingredient in ingredients)
            {
                if (ingredient.GetIngredientType() == type)
                {
                    count++;
                }
            }
            return count;
        }
        
        /// <summary>
        /// 选择下一个食材
        /// </summary>
        public void SelectNext()
        {
            if (ingredients.Count == 0) return;
            
            currentSelectedIndex = (currentSelectedIndex + 1) % ingredients.Count;
            UpdateVisuals();
            NotifyInventoryChanged();
        }
        
        /// <summary>
        /// 选择上一个食材
        /// </summary>
        public void SelectPrevious()
        {
            if (ingredients.Count == 0) return;
            
            currentSelectedIndex--;
            if (currentSelectedIndex < 0)
            {
                currentSelectedIndex = ingredients.Count - 1;
            }
            
            UpdateVisuals();
            NotifyInventoryChanged();
        }
        
        /// <summary>
        /// 选择指定索引的食材
        /// </summary>
        public void SelectIndex(int index)
        {
            if (index >= 0 && index < ingredients.Count)
            {
                currentSelectedIndex = index;
                UpdateVisuals();
                NotifyInventoryChanged();
            }
        }
        
        #endregion
        
        #region Visuals
        
        private void UpdateVisuals()
        {
            // 更新食材的显示位置和大小
            for (int i = 0; i < ingredients.Count; i++)
            {
                if (ingredients[i] != null)
                {
                    // 可以在这里设置食材的相对位置，形成"背包格子"效果
                    // 或者只显示当前选中的
                    bool isSelected = (i == currentSelectedIndex);
                    ingredients[i].gameObject.SetActive(isSelected);
                }
            }
        }
        
        #endregion
        
        private void NotifyInventoryChanged()
        {
            OnInventoryChanged?.Invoke(this, new InventoryChangedEventArgs
            {
                ingredients = new List<Ingredient>(ingredients),
                selectedIndex = currentSelectedIndex
            });
        }
    }
}

