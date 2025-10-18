using UnityEngine;

namespace Assets.Scripts.Cooking
{
    /// <summary>
    /// 炖锅 - 砂锅
    /// </summary>
    public class StewAppliance : KitchenApplianceBase
    {
        [Header("炖锅特殊配置")]
        [SerializeField] private ParticleSystem steamEffect;
        [SerializeField] private AudioClip bubbleSound;
        [SerializeField] private float stewTimeMultiplier = 1.5f; // 炖煮时间通常更长
        
        protected override void Start()
        {
            base.Start();
            cookingMethod = CookingMethodType.Stew;
            baseCookingTime *= stewTimeMultiplier;
            
            if (steamEffect != null)
            {
                steamEffect.Stop();
            }
        }
        
        protected override void StartNextCooking()
        {
            base.StartNextCooking();
            
            // 启动蒸汽特效
            if (steamEffect != null)
            {
                steamEffect.Play();
            }
        }
        
        protected override void CompleteCooking()
        {
            base.CompleteCooking();
            
            // 停止蒸汽特效
            if (steamEffect != null && ingredientQueue.Count == 0)
            {
                steamEffect.Stop();
            }
        }
    }
}

