using System;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Tower;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 防御塔能量槽UI
    /// </summary>
    public class TowerEnergyUI : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private Image fillImage;
        [SerializeField] private Image bonusFillImage;
        [SerializeField] private Text percentText;
        [SerializeField] private Text stateText;
        [SerializeField] private GameObject warningIcon;
        
        [Header("状态颜色")]
        [SerializeField] private Color excitedColor = Color.yellow;
        [SerializeField] private Color normalColor = Color.green;
        [SerializeField] private Color weakColor = Color.red;
        [SerializeField] private Color shutdownColor = Color.gray;
        
        [Header("目标")]
        [SerializeField] private TowerEnergySystem targetTower;
        
        private void Start()
        {
            if (targetTower != null)
            {
                targetTower.OnEnergyChanged += UpdateEnergyUI;
                targetTower.OnStateChanged += UpdateStateUI;
                targetTower.OnShutdown += OnTowerShutdown;
                targetTower.OnRecovered += OnTowerRecovered;
            }
            
            if (warningIcon != null)
            {
                warningIcon.SetActive(false);
            }
        }
        
        private void UpdateEnergyUI(object sender, TowerEnergySystem.EnergyChangedEventArgs e)
        {
            if (fillImage != null)
            {
                fillImage.fillAmount = e.normalizedEnergy;
            }
            
            if (bonusFillImage != null)
            {
                float bonusNormalized = e.bonusEnergy / targetTower.TotalMaxEnergy;
                bonusFillImage.fillAmount = bonusNormalized;
                bonusFillImage.gameObject.SetActive(e.bonusEnergy > 0);
            }
            
            if (percentText != null)
            {
                percentText.text = $"{e.normalizedEnergy * 100:F0}%";
            }
        }
        
        private void UpdateStateUI(object sender, TowerEnergySystem.StateChangedEventArgs e)
        {
            Color stateColor = GetStateColor(e.newState);
            
            if (fillImage != null)
            {
                fillImage.color = stateColor;
            }
            
            if (stateText != null)
            {
                stateText.text = GetStateText(e.newState);
                stateText.color = stateColor;
            }
            
            // 显示警告图标（衰弱和宕机状态）
            if (warningIcon != null)
            {
                bool showWarning = e.newState == TowerEnergyState.Weak || 
                                  e.newState == TowerEnergyState.Shutdown;
                warningIcon.SetActive(showWarning);
            }
        }
        
        private void OnTowerShutdown(object sender, EventArgs e)
        {
            // 播放宕机动画/音效
            Debug.Log("Tower UI: Shutdown!");
        }
        
        private void OnTowerRecovered(object sender, EventArgs e)
        {
            // 播放恢复动画/音效
            Debug.Log("Tower UI: Recovered!");
        }
        
        private Color GetStateColor(TowerEnergyState state)
        {
            return state switch
            {
                TowerEnergyState.Excited => excitedColor,
                TowerEnergyState.Normal => normalColor,
                TowerEnergyState.Weak => weakColor,
                TowerEnergyState.Shutdown => shutdownColor,
                _ => Color.white
            };
        }
        
        private string GetStateText(TowerEnergyState state)
        {
            return state switch
            {
                TowerEnergyState.Excited => "亢奋",
                TowerEnergyState.Normal => "正常",
                TowerEnergyState.Weak => "衰弱",
                TowerEnergyState.Shutdown => "宕机",
                _ => ""
            };
        }
        
        /// <summary>
        /// 动态设置目标防御塔
        /// </summary>
        public void SetTarget(TowerEnergySystem tower)
        {
            if (targetTower != null)
            {
                targetTower.OnEnergyChanged -= UpdateEnergyUI;
                targetTower.OnStateChanged -= UpdateStateUI;
                targetTower.OnShutdown -= OnTowerShutdown;
                targetTower.OnRecovered -= OnTowerRecovered;
            }
            
            targetTower = tower;
            
            if (targetTower != null)
            {
                targetTower.OnEnergyChanged += UpdateEnergyUI;
                targetTower.OnStateChanged += UpdateStateUI;
                targetTower.OnShutdown += OnTowerShutdown;
                targetTower.OnRecovered += OnTowerRecovered;
            }
        }
        
        private void OnDestroy()
        {
            if (targetTower != null)
            {
                targetTower.OnEnergyChanged -= UpdateEnergyUI;
                targetTower.OnStateChanged -= UpdateStateUI;
                targetTower.OnShutdown -= OnTowerShutdown;
                targetTower.OnRecovered -= OnTowerRecovered;
            }
        }
    }
}

