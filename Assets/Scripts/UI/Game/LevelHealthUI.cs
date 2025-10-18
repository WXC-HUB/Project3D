using System;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Level;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 关卡生命值UI
    /// 显示当前生命值和最大生命值
    /// </summary>
    public class LevelHealthUI : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private Text healthText;
        [SerializeField] private GameObject gameOverPanel;
        
        [Header("颜色设置")]
        [SerializeField] private Color healthyColor = Color.green;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color criticalColor = Color.red;
        
        [Header("阈值")]
        [SerializeField] private float warningThreshold = 0.5f;
        [SerializeField] private float criticalThreshold = 0.3f;
        
        private LevelHealthSystem healthSystem;
        
        private void Start()
        {
            healthSystem = LevelHealthSystem.Instance;
            
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged += OnHealthChanged;
                healthSystem.OnGameOver += OnGameOver;
                
                // 初始更新
                UpdateUI(healthSystem.CurrentHealth, healthSystem.MaxHealth);
            }
            
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }
        
        private void OnHealthChanged(object sender, HealthChangedEventArgs e)
        {
            UpdateUI(e.currentHealth, e.maxHealth);
            
            // 播放受击效果
            if (e.damage > 0)
            {
                PlayDamageEffect(e.damage);
            }
        }
        
        private void OnGameOver(object sender, EventArgs e)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
        }
        
        private void UpdateUI(int current, int max)
        {
            // 更新血条
            if (healthSlider != null)
            {
                healthSlider.maxValue = max;
                healthSlider.value = current;
            }
            
            // 更新文本
            if (healthText != null)
            {
                healthText.text = $"{current}/{max}";
            }
            
            // 更新颜色
            if (fillImage != null && max > 0)
            {
                float percentage = (float)current / max;
                
                if (percentage <= criticalThreshold)
                {
                    fillImage.color = criticalColor;
                }
                else if (percentage <= warningThreshold)
                {
                    fillImage.color = warningColor;
                }
                else
                {
                    fillImage.color = healthyColor;
                }
            }
        }
        
        private void PlayDamageEffect(int damage)
        {
            // 可以添加屏幕震动、红屏闪烁等效果
            Debug.Log($"Level took {damage} damage!");
            
            // 示例：文本闪烁
            if (healthText != null)
            {
                // StartCoroutine(FlashText());
            }
        }
        
        private void OnDestroy()
        {
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged -= OnHealthChanged;
                healthSystem.OnGameOver -= OnGameOver;
            }
        }
    }
}

