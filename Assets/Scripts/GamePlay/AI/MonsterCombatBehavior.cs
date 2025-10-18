using UnityEngine;
using Assets.Scripts.Core;
using Assets.Scripts.Tower;

namespace Assets.Scripts.AI
{
    /// <summary>
    /// 怪物战斗行为
    /// 自动攻击范围内的防御塔
    /// </summary>
    public class MonsterCombatBehavior : MonoBehaviour
    {
        [Header("攻击配置")]
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackInterval = 2f;
        [SerializeField] private int energyDamage = 10;  // 对防御塔造成的能量伤害
        [SerializeField] private LayerMask towerLayer;
        
        [Header("调试")]
        [SerializeField] private bool showDebugGizmos = true;
        
        private float attackTimer;
        private TowerController currentTarget;
        private CharacterCtrlBase characterCtrl;
        
        private void Awake()
        {
            characterCtrl = GetComponent<CharacterCtrlBase>();
        }
        
        private void Update()
        {
            // 如果怪物已死亡，不再攻击
            if (characterCtrl == null || !characterCtrl.gameObject.activeSelf)
            {
                return;
            }
            
            attackTimer += Time.deltaTime;
            
            if (attackTimer >= attackInterval)
            {
                TryAttackTower();
                attackTimer = 0f;
            }
        }
        
        /// <summary>
        /// 尝试攻击防御塔
        /// </summary>
        private void TryAttackTower()
        {
            // 检查当前目标是否仍在范围内
            if (currentTarget != null && currentTarget.gameObject.activeSelf)
            {
                float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
                if (distance <= attackRange)
                {
                    AttackTower(currentTarget);
                    return;
                }
            }
            
            // 寻找新目标
            currentTarget = FindNearestTower();
            
            if (currentTarget != null)
            {
                AttackTower(currentTarget);
            }
        }
        
        /// <summary>
        /// 寻找最近的防御塔
        /// </summary>
        private TowerController FindNearestTower()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange, towerLayer);
            
            TowerController nearestTower = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var col in colliders)
            {
                TowerController tower = col.GetComponent<TowerController>();
                
                if (tower != null && tower.gameObject.activeSelf)
                {
                    float distance = Vector3.Distance(transform.position, tower.transform.position);
                    
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestTower = tower;
                    }
                }
            }
            
            return nearestTower;
        }
        
        /// <summary>
        /// 攻击防御塔
        /// </summary>
        private void AttackTower(TowerController tower)
        {
            if (tower == null) return;
            
            // 对防御塔造成能量伤害
            tower.TakeEnergyDamage(energyDamage);
            
            Debug.Log($"Monster {characterCtrl.gameObject.name} attacked tower for {energyDamage} energy damage");
            
            // 这里可以播放攻击动画
            // PlayAttackAnimation();
        }
        
        /// <summary>
        /// 检查是否有防御塔在攻击范围内
        /// </summary>
        public bool HasTowerInRange()
        {
            return FindNearestTower() != null;
        }
        
        #region 调试可视化
        
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;
            
            // 绘制攻击范围
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            // 绘制当前目标连线
            if (currentTarget != null && currentTarget.gameObject.activeSelf)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, currentTarget.transform.position);
            }
        }
        
        #endregion
    }
}

