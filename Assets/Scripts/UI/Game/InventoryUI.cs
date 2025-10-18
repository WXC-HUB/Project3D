using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Cooking;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 背包格子UI
    /// </summary>
    public class InventorySlotUI : MonoBehaviour
    {
        [Header("UI组件")]
        public Image iconImage;
        public Text countText;
        public GameObject selectedFrame;
        public GameObject emptyIndicator;
        
        private Ingredient ingredient;
        
        public void SetIngredient(Ingredient ing, bool isSelected)
        {
            ingredient = ing;
            
            if (ingredient != null)
            {
                // 显示食材图标
                if (iconImage != null && ingredient.GetIngredientData().icon != null)
                {
                    iconImage.sprite = ingredient.GetIngredientData().icon;
                    iconImage.gameObject.SetActive(true);
                }
                
                // 隐藏空槽提示
                if (emptyIndicator != null)
                {
                    emptyIndicator.SetActive(false);
                }
            }
            else
            {
                // 空槽
                if (iconImage != null)
                {
                    iconImage.gameObject.SetActive(false);
                }
                
                if (emptyIndicator != null)
                {
                    emptyIndicator.SetActive(true);
                }
            }
            
            // 选中框
            if (selectedFrame != null)
            {
                selectedFrame.SetActive(isSelected);
            }
            
            // 数量（暂不使用，因为背包中不堆叠）
            if (countText != null)
            {
                countText.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// 背包UI
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private InventorySlotUI[] slots;
        [SerializeField] private Text capacityText;
        
        [Header("目标")]
        [SerializeField] private PlayerInventory targetInventory;
        
        private void Start()
        {
            if (targetInventory != null)
            {
                targetInventory.OnInventoryChanged += UpdateUI;
                UpdateUI(null, null); // 初始更新
            }
        }
        
        private void UpdateUI(object sender, PlayerInventory.InventoryChangedEventArgs e)
        {
            if (targetInventory == null) return;
            
            List<Ingredient> ingredients = targetInventory.GetAllIngredients();
            int selectedIndex = e?.selectedIndex ?? 0;
            
            // 更新所有槽位
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null)
                {
                    Ingredient ing = i < ingredients.Count ? ingredients[i] : null;
                    bool isSelected = i == selectedIndex && ing != null;
                    slots[i].SetIngredient(ing, isSelected);
                }
            }
            
            // 更新容量文本
            if (capacityText != null)
            {
                capacityText.text = $"{ingredients.Count}/{targetInventory.MaxCapacity}";
            }
        }
        
        /// <summary>
        /// 设置目标背包
        /// </summary>
        public void SetTarget(PlayerInventory inventory)
        {
            if (targetInventory != null)
            {
                targetInventory.OnInventoryChanged -= UpdateUI;
            }
            
            targetInventory = inventory;
            
            if (targetInventory != null)
            {
                targetInventory.OnInventoryChanged += UpdateUI;
                UpdateUI(null, null);
            }
        }
        
        private void OnDestroy()
        {
            if (targetInventory != null)
            {
                targetInventory.OnInventoryChanged -= UpdateUI;
            }
        }
    }
}

