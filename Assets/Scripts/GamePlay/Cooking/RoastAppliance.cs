using UnityEngine;

namespace Assets.Scripts.Cooking
{
    /// <summary>
    /// 烤炉 - 烤箱
    /// </summary>
    public class RoastAppliance : KitchenApplianceBase
    {
        [Header("烤炉特殊配置")]
        [SerializeField] private ParticleSystem heatWaveEffect;
        [SerializeField] private AudioClip roastingSound;
        [SerializeField] private GameObject ovenDoor; // 烤箱门
        [SerializeField] private float roastTimeMultiplier = 1.2f;
        
        private bool isDoorOpen = true;
        
        protected override void Start()
        {
            base.Start();
            cookingMethod = CookingMethodType.Roast;
            baseCookingTime *= roastTimeMultiplier;
            
            if (heatWaveEffect != null)
            {
                heatWaveEffect.Stop();
            }
        }
        
        protected override void StartNextCooking()
        {
            base.StartNextCooking();
            
            // 关闭烤箱门
            CloseDoor();
            
            // 启动热浪特效
            if (heatWaveEffect != null)
            {
                heatWaveEffect.Play();
            }
        }
        
        protected override void CompleteCooking()
        {
            base.CompleteCooking();
            
            // 打开烤箱门
            OpenDoor();
            
            // 停止热浪特效
            if (heatWaveEffect != null && ingredientQueue.Count == 0)
            {
                heatWaveEffect.Stop();
            }
        }
        
        private void OpenDoor()
        {
            if (ovenDoor != null && !isDoorOpen)
            {
                // 这里可以添加门打开的动画
                isDoorOpen = true;
            }
        }
        
        private void CloseDoor()
        {
            if (ovenDoor != null && isDoorOpen)
            {
                // 这里可以添加门关闭的动画
                isDoorOpen = false;
            }
        }
    }
}

