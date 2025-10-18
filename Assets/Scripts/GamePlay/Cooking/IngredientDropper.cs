using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Core;

namespace Assets.Scripts.Cooking
{
    /// <summary>
    /// 食材掉落组件 - 附加到怪物上
    /// </summary>
    public class IngredientDropper : MonoBehaviour
    {
        [Header("掉落配置")]
        [SerializeField] private IngredientDropTable dropTable;
        [SerializeField] private float dropForce = 3f; // 掉落时的弹射力度
        [SerializeField] private float dropSpread = 1f; // 掉落散布范围
        
        [Header("自动掉落到仓库")]
        [SerializeField] private bool autoAddToStorage = true; // 是否自动添加到仓库
        [SerializeField] private float autoCollectDelay = 2f; // 延迟自动收集时间
        
        private CharacterCtrlBase characterCtrl;
        private bool hasDropped = false;
        
        private void Awake()
        {
            characterCtrl = GetComponent<CharacterCtrlBase>();
        }
        
        private void Start()
        {
            if (characterCtrl != null)
            {
                // 监听角色死亡事件
                // 这里使用你们现有的事件系统
            }
        }
        
        /// <summary>
        /// 掉落食材（在怪物死亡时调用）
        /// </summary>
        public void DropIngredients()
        {
            if (hasDropped) return;
            if (dropTable == null)
            {
                Debug.LogWarning($"{gameObject.name} has no drop table!");
                return;
            }
            
            hasDropped = true;
            
            // 随机掉落
            List<IngredientData> drops = dropTable.RollDrops();
            
            if (drops.Count == 0)
            {
                Debug.Log($"{gameObject.name} dropped nothing");
                return;
            }
            
            Debug.Log($"{gameObject.name} dropped {drops.Count} ingredients");
            
            // 生成掉落物
            for (int i = 0; i < drops.Count; i++)
            {
                SpawnIngredientDrop(drops[i], i);
            }
        }
        
        /// <summary>
        /// 生成单个掉落食材
        /// </summary>
        private void SpawnIngredientDrop(IngredientData ingredientData, int index)
        {
            // 计算掉落位置（带随机偏移）
            Vector3 dropPosition = transform.position + new Vector3(
                Random.Range(-dropSpread, dropSpread),
                Random.Range(-dropSpread, dropSpread),
                0
            );
            
            // 生成食材
            Ingredient ingredient = Ingredient.Spawn(ingredientData, dropPosition);
            
            if (ingredient != null)
            {
                // 添加弹射效果
                if (ingredient.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    Vector2 randomDirection = Random.insideUnitCircle.normalized;
                    rb.AddForce(randomDirection * dropForce, ForceMode2D.Impulse);
                }
                
                // 如果开启自动收集，延迟后自动添加到仓库
                if (autoAddToStorage)
                {
                    StartCoroutine(AutoCollectIngredient(ingredient, ingredientData.ingredientType));
                }
            }
        }
        
        /// <summary>
        /// 自动收集食材到仓库
        /// </summary>
        private IEnumerator AutoCollectIngredient(Ingredient ingredient, IngredientType type)
        {
            yield return new WaitForSeconds(autoCollectDelay);
            
            if (ingredient != null && ingredient.GetHolder() == null)
            {
                // 如果食材还没被玩家拾取，自动添加到仓库
                FindObjectOfType<StorageCounter>()?.AddIngredient(type, 1);
                ingredient.DestroySelf();
                
                Debug.Log($"Auto-collected {type} to storage");
            }
        }
    }
}

