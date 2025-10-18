using UnityEngine;

namespace Assets.Scripts.Cooking
{
    /// <summary>
    /// 炒锅 - 铁锅
    /// </summary>
    public class StirFryAppliance : KitchenApplianceBase
    {
        [Header("炒锅特殊配置")]
        [SerializeField] private ParticleSystem fireEffect;
        [SerializeField] private AudioClip stirFrySound;
        
        protected override void Start()
        {
            base.Start();
            cookingMethod = CookingMethodType.Stir;
            
            if (fireEffect != null)
            {
                fireEffect.Stop();
            }
        }
        
        protected override void StartNextCooking()
        {
            base.StartNextCooking();
            
            // 启动火焰特效
            if (fireEffect != null)
            {
                fireEffect.Play();
            }
        }
        
        protected override void CompleteCooking()
        {
            base.CompleteCooking();
            
            // 停止火焰特效
            if (fireEffect != null && ingredientQueue.Count == 0)
            {
                fireEffect.Stop();
            }
        }
    }
}

