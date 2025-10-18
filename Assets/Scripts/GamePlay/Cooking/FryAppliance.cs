using UnityEngine;

namespace Assets.Scripts.Cooking
{
    /// <summary>
    /// 炸锅 - 油炸锅
    /// </summary>
    public class FryAppliance : KitchenApplianceBase
    {
        [Header("炸锅特殊配置")]
        [SerializeField] private ParticleSystem oilBubbleEffect;
        [SerializeField] private AudioClip fryingSound;
        [SerializeField] private float burnWarningTime = 8f; // 炸太久会烧焦
        
        private bool isWarning = false;
        
        protected override void Start()
        {
            base.Start();
            cookingMethod = CookingMethodType.Fry;
            
            if (oilBubbleEffect != null)
            {
                oilBubbleEffect.Stop();
            }
        }
        
        protected override void StartNextCooking()
        {
            base.StartNextCooking();
            isWarning = false;
            
            // 启动气泡特效
            if (oilBubbleEffect != null)
            {
                oilBubbleEffect.Play();
            }
        }
        
        protected override void UpdateCooking()
        {
            base.UpdateCooking();
            
            // 检查是否接近烧焦
            if (!isWarning && cookingProgress > burnWarningTime)
            {
                isWarning = true;
                Debug.LogWarning("Food is about to burn!");
                // 这里可以添加警告特效
            }
        }
        
        protected override void CompleteCooking()
        {
            base.CompleteCooking();
            isWarning = false;
            
            // 停止气泡特效
            if (oilBubbleEffect != null && ingredientQueue.Count == 0)
            {
                oilBubbleEffect.Stop();
            }
        }
    }
}

