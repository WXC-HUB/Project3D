using System;
using UnityEngine;

namespace Assets.Scripts.Tower
{
    /// <summary>
    /// 防御塔能量系统
    /// 策划案2.3.1.5.2：能量槽机制
    /// </summary>
    public class TowerEnergySystem : MonoBehaviour
    {
        [Header("能量配置")]
        [SerializeField] private float maxEnergy = 100f;
        [SerializeField] private float currentEnergy = 100f;
        [SerializeField] private float energyDecayRate = 2f; // 每秒衰减速度
        
        [Header("能量阈值")]
        [SerializeField] private float excitedThreshold = 66f;   // 亢奋阈值 (66%)
        [SerializeField] private float normalThreshold = 33f;     // 正常阈值 (33%)
        [SerializeField] private float weakThreshold = 1f;        // 衰弱阈值 (1%)
        
        [Header("额外能量（喂食加成）")]
        [SerializeField] private float bonusEnergy = 0f;          // 额外能量条
        [SerializeField] private float maxBonusEnergy = 50f;      // 最大额外能量
        
        [Header("状态效果")]
        [SerializeField] private float excitedAttackBonus = 1.5f; // 亢奋攻击加成
        [SerializeField] private float weakAttackPenalty = 0.5f;  // 衰弱攻击惩罚
        
        [Header("部署状态")]
        [SerializeField] private bool isDeployed = false;
        [SerializeField] private bool isFirstRound = true; // 第一回合不衰减
        
        private TowerEnergyState currentState = TowerEnergyState.Excited;
        private bool isDecayPaused = false;
        
        #region Events
        
        public event EventHandler<EnergyChangedEventArgs> OnEnergyChanged;
        public event EventHandler<StateChangedEventArgs> OnStateChanged;
        public event EventHandler OnShutdown;
        public event EventHandler OnRecovered;
        
        public class EnergyChangedEventArgs : EventArgs
        {
            public float currentEnergy;
            public float maxEnergy;
            public float bonusEnergy;
            public float normalizedEnergy; // 0-1
        }
        
        public class StateChangedEventArgs : EventArgs
        {
            public TowerEnergyState oldState;
            public TowerEnergyState newState;
        }
        
        #endregion
        
        #region Properties
        
        public float CurrentEnergy => currentEnergy;
        public float MaxEnergy => maxEnergy;
        public float BonusEnergy => bonusEnergy;
        public float TotalMaxEnergy => maxEnergy + bonusEnergy;
        public TowerEnergyState CurrentState => currentState;
        public bool IsShutdown => currentState == TowerEnergyState.Shutdown;
        public bool CanAttack => !IsShutdown;
        public bool HasBlocking => !IsShutdown; // 宕机时没有阻挡
        
        /// <summary>
        /// 获取标准化能量值 (0-1)
        /// </summary>
        public float GetNormalizedEnergy()
        {
            if (bonusEnergy > 0)
            {
                // 优先消耗额外能量
                return 1f;
            }
            return Mathf.Clamp01(currentEnergy / maxEnergy);
        }
        
        /// <summary>
        /// 获取当前攻击倍率
        /// </summary>
        public float GetAttackMultiplier()
        {
            switch (currentState)
            {
                case TowerEnergyState.Excited:
                    return excitedAttackBonus;
                case TowerEnergyState.Normal:
                    return 1f;
                case TowerEnergyState.Weak:
                    return weakAttackPenalty;
                case TowerEnergyState.Shutdown:
                    return 0f;
                default:
                    return 1f;
            }
        }
        
        /// <summary>
        /// 获取当前攻击速度倍率
        /// 策划案：亢奋状态攻速+30%，衰弱状态攻速-30%
        /// </summary>
        public float GetAttackSpeedMultiplier()
        {
            switch (currentState)
            {
                case TowerEnergyState.Excited:
                    return 1.3f;  // 攻速+30%
                case TowerEnergyState.Normal:
                    return 1f;
                case TowerEnergyState.Weak:
                    return 0.7f;  // 攻速-30%
                case TowerEnergyState.Shutdown:
                    return 0f;    // 宕机无法攻击
                default:
                    return 1f;
            }
        }
        
        /// <summary>
        /// 减少能量（怪物攻击时调用）
        /// </summary>
        /// <param name="damage">伤害值</param>
        public void DecreaseEnergy(int damage)
        {
            TakeDamage(damage);
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Update()
        {
            if (!isDeployed) return;
            if (isDecayPaused) return;
            if (isFirstRound) return; // 第一回合不衰减
            
            // 能量衰减
            DecayEnergy(Time.deltaTime);
        }
        
        #endregion
        
        #region Energy Management
        
        /// <summary>
        /// 部署防御塔
        /// </summary>
        public void Deploy()
        {
            isDeployed = true;
            currentEnergy = maxEnergy;
            bonusEnergy = 0f;
            UpdateState();
            
            Debug.Log("Tower deployed with full energy!");
        }
        
        /// <summary>
        /// 开始能量衰减（第一回合结束后）
        /// </summary>
        public void StartDecay()
        {
            isFirstRound = false;
            Debug.Log("Tower energy decay started");
        }
        
        /// <summary>
        /// 能量衰减
        /// </summary>
        private void DecayEnergy(float deltaTime)
        {
            float decayAmount = energyDecayRate * deltaTime;
            
            // 优先扣除额外能量
            if (bonusEnergy > 0)
            {
                bonusEnergy = Mathf.Max(0, bonusEnergy - decayAmount);
            }
            else
            {
                currentEnergy = Mathf.Max(0, currentEnergy - decayAmount);
            }
            
            NotifyEnergyChanged();
            UpdateState();
        }
        
        /// <summary>
        /// 受到伤害（怪物攻击）
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (IsShutdown) return;
            
            // 优先扣除额外能量
            if (bonusEnergy > 0)
            {
                bonusEnergy = Mathf.Max(0, bonusEnergy - damage);
            }
            else
            {
                currentEnergy = Mathf.Max(0, currentEnergy - damage);
            }
            
            NotifyEnergyChanged();
            UpdateState();
            
            Debug.Log($"Tower took {damage} damage. Energy: {currentEnergy}/{maxEnergy}");
        }
        
        /// <summary>
        /// 补充能量（喂食）
        /// </summary>
        public void Refill(float amount)
        {
            if (currentState == TowerEnergyState.Excited)
            {
                // 亢奋状态下喂食：提升属性并延长能量槽
                bonusEnergy = Mathf.Min(maxBonusEnergy, bonusEnergy + amount);
                Debug.Log($"Tower is excited! Bonus energy added: {bonusEnergy}");
            }
            else
            {
                // 其他状态：回满能量
                currentEnergy = maxEnergy;
                bonusEnergy = 0f;
                
                if (IsShutdown)
                {
                    OnRecovered?.Invoke(this, EventArgs.Empty);
                    Debug.Log("Tower recovered from shutdown!");
                }
                else
                {
                    Debug.Log("Tower energy refilled!");
                }
            }
            
            NotifyEnergyChanged();
            UpdateState();
        }
        
        /// <summary>
        /// 完全补满（包括额外能量）
        /// </summary>
        public void FullRefill()
        {
            currentEnergy = maxEnergy;
            bonusEnergy = maxBonusEnergy;
            NotifyEnergyChanged();
            UpdateState();
        }
        
        #endregion
        
        #region State Management
        
        private void UpdateState()
        {
            TowerEnergyState newState = CalculateState();
            
            if (newState != currentState)
            {
                TowerEnergyState oldState = currentState;
                currentState = newState;
                
                OnStateChanged?.Invoke(this, new StateChangedEventArgs
                {
                    oldState = oldState,
                    newState = newState
                });
                
                if (newState == TowerEnergyState.Shutdown)
                {
                    OnShutdown?.Invoke(this, EventArgs.Empty);
                }
                
                Debug.Log($"Tower state changed: {oldState} -> {newState}");
            }
        }
        
        private TowerEnergyState CalculateState()
        {
            // 有额外能量时保持亢奋
            if (bonusEnergy > 0)
            {
                return TowerEnergyState.Excited;
            }
            
            float percentage = (currentEnergy / maxEnergy) * 100f;
            
            if (percentage <= 0f)
                return TowerEnergyState.Shutdown;
            else if (percentage <= weakThreshold)
                return TowerEnergyState.Weak;
            else if (percentage <= normalThreshold)
                return TowerEnergyState.Normal;
            else if (percentage <= excitedThreshold)
                return TowerEnergyState.Normal;
            else
                return TowerEnergyState.Excited;
        }
        
        #endregion
        
        #region Utility
        
        /// <summary>
        /// 暂停能量衰减
        /// </summary>
        public void PauseDecay()
        {
            isDecayPaused = true;
        }
        
        /// <summary>
        /// 恢复能量衰减
        /// </summary>
        public void ResumeDecay()
        {
            isDecayPaused = false;
        }
        
        private void NotifyEnergyChanged()
        {
            OnEnergyChanged?.Invoke(this, new EnergyChangedEventArgs
            {
                currentEnergy = currentEnergy,
                maxEnergy = maxEnergy,
                bonusEnergy = bonusEnergy,
                normalizedEnergy = GetNormalizedEnergy()
            });
        }
        
        #endregion
        
        #region Debug
        
        [ContextMenu("Test: Refill Energy")]
        public void TestRefill()
        {
            Refill(100f);
        }
        
        [ContextMenu("Test: Take Damage")]
        public void TestTakeDamage()
        {
            TakeDamage(20f);
        }
        
        [ContextMenu("Test: Deploy Tower")]
        public void TestDeploy()
        {
            Deploy();
        }
        
        private void OnValidate()
        {
            // 确保阈值合理
            excitedThreshold = Mathf.Clamp(excitedThreshold, normalThreshold + 1, 99);
            normalThreshold = Mathf.Clamp(normalThreshold, weakThreshold + 1, excitedThreshold - 1);
            weakThreshold = Mathf.Clamp(weakThreshold, 1, normalThreshold - 1);
        }
        
        #endregion
    }
}

