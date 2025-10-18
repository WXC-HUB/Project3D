using System;
using UnityEngine;
using Assets.Scripts.BaseUtils;

namespace Assets.Scripts.Level
{
    /// <summary>
    /// 关卡生命值系统
    /// 怪物进入厨房区域会扣除生命值，生命值为0时判负
    /// </summary>
    public class LevelHealthSystem : MonoSingleton<LevelHealthSystem>
    {
        [Header("生命值设置")]
        [SerializeField] private int maxHealth = 10;
        
        private int currentHealth;
        
        /// <summary>
        /// 生命值变化事件
        /// </summary>
        public event EventHandler<HealthChangedEventArgs> OnHealthChanged;
        
        /// <summary>
        /// 游戏失败事件
        /// </summary>
        public event EventHandler OnGameOver;
        
        /// <summary>
        /// 当前生命值
        /// </summary>
        public int CurrentHealth => currentHealth;
        
        /// <summary>
        /// 最大生命值
        /// </summary>
        public int MaxHealth => maxHealth;
        
        /// <summary>
        /// 是否已经失败
        /// </summary>
        public bool IsGameOver => currentHealth <= 0;
        
        private void Start()
        {
            InitializeHealth();
        }
        
        /// <summary>
        /// 初始化生命值
        /// </summary>
        public void InitializeHealth()
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(this, new HealthChangedEventArgs
            {
                currentHealth = currentHealth,
                maxHealth = maxHealth,
                damage = 0
            });
        }
        
        /// <summary>
        /// 受到伤害
        /// </summary>
        /// <param name="damage">伤害值</param>
        /// <param name="source">伤害来源（可选）</param>
        public void TakeDamage(int damage, GameObject source = null)
        {
            if (IsGameOver)
            {
                Debug.LogWarning("Game is already over, cannot take more damage.");
                return;
            }
            
            if (damage <= 0)
            {
                Debug.LogWarning("Damage must be greater than 0.");
                return;
            }
            
            int oldHealth = currentHealth;
            currentHealth = Mathf.Max(0, currentHealth - damage);
            
            Debug.Log($"Level took {damage} damage. Health: {oldHealth} -> {currentHealth}");
            
            // 触发生命值变化事件
            OnHealthChanged?.Invoke(this, new HealthChangedEventArgs
            {
                currentHealth = currentHealth,
                maxHealth = maxHealth,
                damage = damage,
                source = source
            });
            
            // 检查是否失败
            if (currentHealth <= 0)
            {
                TriggerGameOver();
            }
        }
        
        /// <summary>
        /// 根据怪物类型计算伤害值
        /// </summary>
        /// <param name="monster">怪物对象</param>
        /// <returns>伤害值</returns>
        public int GetMonsterDamage(CharacterCtrlBase monster)
        {
            if (monster == null) return 1;
            
            // 通过GameObject名称判断类型
            string name = monster.gameObject.name.ToLower();
            
            // BOSS怪物（名称包含"boss"）
            if (name.Contains("boss"))
            {
                return 5;
            }
            // 精英怪物（名称包含"elite"或"精英"）
            else if (name.Contains("elite") || name.Contains("精英"))
            {
                return 3;
            }
            // 小怪（默认）
            else
            {
                return 1;
            }
        }
        
        /// <summary>
        /// 触发游戏失败
        /// </summary>
        private void TriggerGameOver()
        {
            Debug.Log("Game Over! All health depleted.");
            
            OnGameOver?.Invoke(this, EventArgs.Empty);
            
            // 可以在这里暂停游戏
            // Time.timeScale = 0;
        }
        
        /// <summary>
        /// 重置生命值（用于重新开始游戏）
        /// </summary>
        public void ResetHealth()
        {
            InitializeHealth();
            // Time.timeScale = 1;
        }
        
        #region 调试方法
        
        /// <summary>
        /// 设置最大生命值（仅用于测试）
        /// </summary>
        public void SetMaxHealth(int value)
        {
            maxHealth = Mathf.Max(1, value);
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            
            OnHealthChanged?.Invoke(this, new HealthChangedEventArgs
            {
                currentHealth = currentHealth,
                maxHealth = maxHealth,
                damage = 0
            });
        }
        
        #endregion
    }
    
    /// <summary>
    /// 生命值变化事件参数
    /// </summary>
    public class HealthChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 当前生命值
        /// </summary>
        public int currentHealth;
        
        /// <summary>
        /// 最大生命值
        /// </summary>
        public int maxHealth;
        
        /// <summary>
        /// 受到的伤害
        /// </summary>
        public int damage;
        
        /// <summary>
        /// 伤害来源（可选）
        /// </summary>
        public GameObject source;
        
        /// <summary>
        /// 生命值百分比
        /// </summary>
        public float HealthPercentage => maxHealth > 0 ? (float)currentHealth / maxHealth : 0;
    }
}

