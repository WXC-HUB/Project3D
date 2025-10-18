using System;
using UnityEngine;
using Assets.Scripts.Core;
using Assets.Scripts.Cooking;
using Assets.Scripts.Combat;
using Assets.Scripts.AI;

namespace Assets.Scripts.Tower
{
    /// <summary>
    /// 防御塔控制器
    /// 整合能量系统、攻击系统、AI系统
    /// </summary>
    public class TowerController : MonoBehaviour
    {
        [Header("组件引用")]
        [SerializeField] private TowerEnergySystem energySystem;
        [SerializeField] private PlayerCharacterCtrl characterCtrl;
        
        [Header("攻击配置")]
        [SerializeField] private float attackRange = 5f;
        [SerializeField] private float attackInterval = 1f;
        [SerializeField] private int baseAttack = 10;
        [SerializeField] private float skillRate = 1.0f;
        [SerializeField] private AttackType attackType = AttackType.Physical;
        [SerializeField] private LayerMask enemyLayer;
        
        private float attackTimer;
        
        [Header("视觉表现")]
        [SerializeField] private GameObject normalVisual;
        [SerializeField] private GameObject excitedVisual;
        [SerializeField] private GameObject weakVisual;
        [SerializeField] private GameObject shutdownVisual;
        
        private CharacterCtrlBase currentTarget;
        private bool isOperational = true;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (energySystem == null)
            {
                energySystem = GetComponent<TowerEnergySystem>();
            }
            
            if (characterCtrl == null)
            {
                characterCtrl = GetComponent<PlayerCharacterCtrl>();
            }
        }
        
        private void Start()
        {
            if (energySystem != null)
            {
                energySystem.OnStateChanged += OnEnergyStateChanged;
                energySystem.OnShutdown += OnTowerShutdown;
                energySystem.OnRecovered += OnTowerRecovered;
            }
        }
        
        private void Update()
        {
            if (!isOperational) return;
            if (energySystem != null && energySystem.IsShutdown) return;
            
            // 自动攻击系统
            attackTimer += Time.deltaTime;
            
            if (attackTimer >= GetModifiedAttackInterval())
            {
                FindAndAttackTarget();
                attackTimer = 0f;
            }
        }
        
        #endregion
        
        #region Feeding System
        
        /// <summary>
        /// 喂食防御塔（使用配方成品）
        /// </summary>
        public bool Feed(RecipeData recipe)
        {
            if (energySystem == null)
            {
                Debug.LogWarning("Tower has no energy system!");
                return false;
            }
            
            // 根据配方提供能量
            float energyAmount = recipe.energyReward;
            energySystem.Refill(energyAmount);
            
            // 应用配方的攻击加成（通过Modifier系统）
            if (recipe.attackBonus > 1f && characterCtrl != null)
            {
                float bonusDuration = recipe.durationBonus > 0 ? recipe.durationBonus : 30f;
                // 这里可以添加攻击力Modifier
                // SkillDispatchCenter.Instance.AddModifierToCharacter(characterCtrl, bonusDuration, attackBuffModifierID);
            }
            
            Debug.Log($"Tower fed with {recipe.recipeName}. Energy +{energyAmount}");
            return true;
        }
        
        /// <summary>
        /// 检查是否可以喂食
        /// </summary>
        public bool CanBeFed()
        {
            return energySystem != null && !energySystem.IsShutdown;
        }
        
        #endregion
        
        #region State Management
        
        private void OnEnergyStateChanged(object sender, TowerEnergySystem.StateChangedEventArgs e)
        {
            UpdateVisuals(e.newState);
            UpdateAttackMultiplier(e.newState);
            UpdateBlocking(e.newState);
            
            Debug.Log($"Tower state changed to: {e.newState}");
        }
        
        private void OnTowerShutdown(object sender, EventArgs e)
        {
            isOperational = false;
            Debug.Log("Tower shutdown! Cannot attack!");
            
            // 播放宕机特效/音效
            // PlayShutdownEffect();
        }
        
        private void OnTowerRecovered(object sender, EventArgs e)
        {
            isOperational = true;
            Debug.Log("Tower recovered!");
            
            // 播放恢复特效/音效
            // PlayRecoverEffect();
        }
        
        #endregion
        
        #region Visuals
        
        private void UpdateVisuals(TowerEnergyState state)
        {
            // 切换不同状态的视觉表现
            if (normalVisual != null) normalVisual.SetActive(state == TowerEnergyState.Normal);
            if (excitedVisual != null) excitedVisual.SetActive(state == TowerEnergyState.Excited);
            if (weakVisual != null) weakVisual.SetActive(state == TowerEnergyState.Weak);
            if (shutdownVisual != null) shutdownVisual.SetActive(state == TowerEnergyState.Shutdown);
        }
        
        #endregion
        
        #region Combat
        
        private void UpdateAttackMultiplier(TowerEnergyState state)
        {
            if (characterCtrl == null) return;
            
            float multiplier = energySystem.GetAttackMultiplier();
            
            // 通过Modifier系统应用攻击力倍率
            // 这里可以移除旧的倍率Modifier，添加新的
            // SkillDispatchCenter.Instance.AddModifierToCharacter(characterCtrl, -1, attackMultiplierModifierID);
        }
        
        private void UpdateBlocking(TowerEnergyState state)
        {
            bool hasBlocking = energySystem.HasBlocking;
            
            // 更新碰撞体状态
            if (characterCtrl != null && characterCtrl.col2D != null)
            {
                characterCtrl.col2D.enabled = hasBlocking;
            }
        }
        
        /// <summary>
        /// 寻找并攻击目标
        /// </summary>
        private void FindAndAttackTarget()
        {
            // 如果当前目标仍然有效且在范围内，继续攻击
            if (currentTarget != null && currentTarget.gameObject.activeSelf)
            {
                float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
                if (distance <= attackRange)
                {
                    AttackEnemy(currentTarget);
                    return;
                }
            }
            
            // 寻找新目标
            currentTarget = FindNearestEnemy();
            
            if (currentTarget != null)
            {
                AttackEnemy(currentTarget);
            }
        }
        
        /// <summary>
        /// 寻找最近的敌人
        /// </summary>
        private CharacterCtrlBase FindNearestEnemy()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
            
            CharacterCtrlBase nearestEnemy = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var col in colliders)
            {
                CharacterCtrlBase enemy = col.GetComponent<CharacterCtrlBase>();
                
                // 检查是否是敌方单位（有AIAgentBase组件或Enemy tag的才是怪物）
                if (enemy != null && enemy.gameObject.activeSelf)
                {
                    // 确保不攻击玩家或友军
                    AIAgentBase aiAgent = enemy.GetComponent<AIAgentBase>();
                    if (aiAgent != null || enemy.CompareTag("Enemy"))
                    {
                        float distance = Vector3.Distance(transform.position, enemy.transform.position);
                        
                        if (distance < nearestDistance)
                        {
                            nearestDistance = distance;
                            nearestEnemy = enemy;
                        }
                    }
                }
            }
            
            return nearestEnemy;
        }
        
        /// <summary>
        /// 攻击敌人
        /// </summary>
        private void AttackEnemy(CharacterCtrlBase enemy)
        {
            if (enemy == null) return;
            
            // 计算伤害
            int damage = GetModifiedAttack();
            
            // 创建技能使用信息
            SkillUseInfo skillInfo = new SkillUseInfo
            {
                dispatcher = characterCtrl,  // 必须是CharacterCtrlBase类型
                SkillID = 0  // 普通攻击
            };
            
            // 造成伤害
            enemy.TakeDamage(damage, skillInfo);
            
            Debug.Log($"Tower attacked {enemy.gameObject.name} for {damage} damage");
        }
        
        /// <summary>
        /// 获取修改后的攻击力（考虑能量状态）
        /// </summary>
        private int GetModifiedAttack()
        {
            float multiplier = energySystem != null ? energySystem.GetAttackMultiplier() : 1.0f;
            return (int)(baseAttack * skillRate * multiplier);
        }
        
        /// <summary>
        /// 获取修改后的攻击间隔（考虑能量状态）
        /// </summary>
        private float GetModifiedAttackInterval()
        {
            if (energySystem == null) return attackInterval;
            
            float speedMultiplier = energySystem.GetAttackSpeedMultiplier();
            // 攻速倍率转换为间隔：攻速x1.3 = 间隔÷1.3
            return attackInterval / speedMultiplier;
        }
        
        /// <summary>
        /// 受到怪物攻击
        /// </summary>
        public void OnMonsterAttack(float damage)
        {
            if (energySystem != null)
            {
                energySystem.TakeDamage(damage);
            }
        }
        
        /// <summary>
        /// 受到能量伤害（怪物攻击）
        /// </summary>
        public void TakeEnergyDamage(int damage)
        {
            if (energySystem != null)
            {
                energySystem.DecreaseEnergy(damage);
            }
        }
        
        #endregion
        
        #region Gizmos
        
        private void OnDrawGizmosSelected()
        {
            // 显示攻击范围
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            // 绘制当前目标连线
            if (currentTarget != null && currentTarget.gameObject.activeSelf)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, currentTarget.transform.position);
            }
            
            // 显示能量状态颜色
            if (energySystem != null)
            {
                Color stateColor = Color.white;
                switch (energySystem.CurrentState)
                {
                    case TowerEnergyState.Excited:
                        stateColor = Color.yellow;
                        break;
                    case TowerEnergyState.Normal:
                        stateColor = Color.green;
                        break;
                    case TowerEnergyState.Weak:
                        stateColor = Color.red;
                        break;
                    case TowerEnergyState.Shutdown:
                        stateColor = Color.gray;
                        break;
                }
                
                Gizmos.color = stateColor;
                Gizmos.DrawSphere(transform.position + Vector3.up * 2, 0.3f);
            }
        }
        
        #endregion
        
        private void OnDestroy()
        {
            if (energySystem != null)
            {
                energySystem.OnStateChanged -= OnEnergyStateChanged;
                energySystem.OnShutdown -= OnTowerShutdown;
                energySystem.OnRecovered -= OnTowerRecovered;
            }
        }
    }
}

