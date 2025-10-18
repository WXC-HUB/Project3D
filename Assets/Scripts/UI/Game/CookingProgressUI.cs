using System;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Cooking;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 烹饪进度UI（显示在厨具上方）
    /// </summary>
    public class CookingProgressUI : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private GameObject progressPanel;
        [SerializeField] private Text ingredientNameText;
        
        [Header("颜色")]
        [SerializeField] private Color normalColor = Color.green;
        [SerializeField] private Color warningColor = Color.yellow;
        
        [Header("目标")]
        [SerializeField] private KitchenApplianceBase targetAppliance;
        
        [Header("世界空间")]
        [SerializeField] private Transform worldTarget; // 厨具的世界坐标位置
        [SerializeField] private Vector3 offset = new Vector3(0, 2, 0);
        
        private Camera mainCamera;
        
        private void Start()
        {
            mainCamera = Camera.main;
            
            if (targetAppliance != null)
            {
                targetAppliance.OnCookingProgressChanged += UpdateProgress;
                targetAppliance.OnCookingComplete += OnCookingComplete;
                targetAppliance.OnIngredientAdded += OnIngredientAdded;
            }
            
            if (progressPanel != null)
            {
                progressPanel.SetActive(false);
            }
        }
        
        private void Update()
        {
            // 如果是世界空间UI，跟随目标位置
            if (worldTarget != null && mainCamera != null)
            {
                Vector3 screenPos = mainCamera.WorldToScreenPoint(worldTarget.position + offset);
                
                // 只在目标在屏幕内时显示
                if (screenPos.z > 0)
                {
                    transform.position = screenPos;
                }
            }
        }
        
        private void UpdateProgress(object sender, KitchenApplianceBase.CookingProgressEventArgs e)
        {
            if (progressSlider != null)
            {
                progressSlider.value = e.progress;
            }
            
            if (fillImage != null)
            {
                // 快完成时变色
                fillImage.color = e.progress > 0.8f ? warningColor : normalColor;
            }
            
            if (progressPanel != null)
            {
                progressPanel.SetActive(e.isActive);
            }
        }
        
        private void OnIngredientAdded(object sender, Ingredient e)
        {
            if (ingredientNameText != null)
            {
                ingredientNameText.text = e.GetIngredientData().ingredientName;
            }
        }
        
        private void OnCookingComplete(object sender, Ingredient e)
        {
            // 可以显示完成特效
            Debug.Log("Cooking complete UI effect!");
        }
        
        /// <summary>
        /// 设置目标厨具
        /// </summary>
        public void SetTarget(KitchenApplianceBase appliance, Transform worldPos = null)
        {
            if (targetAppliance != null)
            {
                targetAppliance.OnCookingProgressChanged -= UpdateProgress;
                targetAppliance.OnCookingComplete -= OnCookingComplete;
                targetAppliance.OnIngredientAdded -= OnIngredientAdded;
            }
            
            targetAppliance = appliance;
            worldTarget = worldPos;
            
            if (targetAppliance != null)
            {
                targetAppliance.OnCookingProgressChanged += UpdateProgress;
                targetAppliance.OnCookingComplete += OnCookingComplete;
                targetAppliance.OnIngredientAdded += OnIngredientAdded;
            }
        }
        
        private void OnDestroy()
        {
            if (targetAppliance != null)
            {
                targetAppliance.OnCookingProgressChanged -= UpdateProgress;
                targetAppliance.OnCookingComplete -= OnCookingComplete;
                targetAppliance.OnIngredientAdded -= OnIngredientAdded;
            }
        }
    }
}

