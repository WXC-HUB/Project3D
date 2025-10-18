using UnityEngine;
using Assets.Scripts.Cooking;
using Assets.Scripts.AI;

namespace Assets.Scripts.Level
{
    /// <summary>
    /// 厨房区域触发器
    /// 怪物进入此区域会扣除关卡生命值，并且怪物会死亡且不掉落食材
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class KitchenZoneTrigger : MonoBehaviour
    {
        [Header("调试")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private Color gizmoColor = new Color(1f, 0f, 0f, 0.3f);
        
        private Collider2D triggerCollider;
        
        private void Awake()
        {
            triggerCollider = GetComponent<Collider2D>();
            triggerCollider.isTrigger = true;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            // 检查是否是怪物（通过AIAgentBase或者tag判断）
            CharacterCtrlBase character = other.GetComponent<CharacterCtrlBase>();
            
            if (character == null) return;
            
            // 检查是否是敌方单位（玩家不会有AIAgentBase组件）
            AIAgentBase aiAgent = character.GetComponent<AIAgentBase>();
            if (aiAgent == null && !other.CompareTag("Enemy"))
            {
                return; // 不是怪物，可能是玩家或其他单位
            }
            
            Debug.Log($"Monster entered kitchen zone!");
            
            // 1. 扣除关卡生命值
            if (LevelHealthSystem.Instance != null)
            {
                int damage = LevelHealthSystem.Instance.GetMonsterDamage(character);
                LevelHealthSystem.Instance.TakeDamage(damage, character.gameObject);
            }
            
            // 2. 禁用食材掉落
            IngredientDropper dropper = character.GetComponent<IngredientDropper>();
            if (dropper != null)
            {
                dropper.enabled = false;
            }
            
            // 3. 通知波次管理器（如果需要计入击杀数）
            if (Assets.Scripts.Wave.WaveManager.Instance != null)
            {
                Assets.Scripts.Wave.WaveManager.Instance.NotifyMonsterKilled(character);
            }
            
            // 4. 击杀怪物
            character.Die(null);
        }
        
        #region 3D版本（如果是3D项目）
        
        private void OnTriggerEnter(Collider other)
        {
            // 检查是否是怪物（通过AIAgentBase或者tag判断）
            CharacterCtrlBase character = other.GetComponent<CharacterCtrlBase>();
            
            if (character == null) return;
            
            // 检查是否是敌方单位（玩家不会有AIAgentBase组件）
            AIAgentBase aiAgent = character.GetComponent<AIAgentBase>();
            if (aiAgent == null && !other.CompareTag("Enemy"))
            {
                return; // 不是怪物，可能是玩家或其他单位
            }
            
            Debug.Log($"Monster entered kitchen zone!");
            
            // 1. 扣除关卡生命值
            if (LevelHealthSystem.Instance != null)
            {
                int damage = LevelHealthSystem.Instance.GetMonsterDamage(character);
                LevelHealthSystem.Instance.TakeDamage(damage, character.gameObject);
            }
            
            // 2. 禁用食材掉落
            IngredientDropper dropper = character.GetComponent<IngredientDropper>();
            if (dropper != null)
            {
                dropper.enabled = false;
            }
            
            // 3. 通知波次管理器
            if (Assets.Scripts.Wave.WaveManager.Instance != null)
            {
                Assets.Scripts.Wave.WaveManager.Instance.NotifyMonsterKilled(character);
            }
            
            // 4. 击杀怪物
            character.Die(null);
        }
        
        #endregion
        
        #region 可视化调试
        
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;
            
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                Gizmos.color = gizmoColor;
                
                // 如果是BoxCollider2D，绘制矩形
                if (col is BoxCollider2D boxCol)
                {
                    Vector3 center = transform.position + (Vector3)boxCol.offset;
                    Vector3 size = boxCol.size;
                    Gizmos.DrawCube(center, size);
                }
                // 如果是CircleCollider2D，绘制圆形
                else if (col is CircleCollider2D circleCol)
                {
                    Vector3 center = transform.position + (Vector3)circleCol.offset;
                    Gizmos.DrawSphere(center, circleCol.radius);
                }
            }
        }
        
        #endregion
    }
}

