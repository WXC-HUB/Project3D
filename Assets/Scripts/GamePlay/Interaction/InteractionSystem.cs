using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Core;

namespace Assets.Scripts.Interaction
{
    /// <summary>
    /// 交互系统
    /// 管理玩家与场景中可交互对象的交互
    /// </summary>
    public class InteractionSystem : MonoBehaviour
    {
        [Header("交互配置")]
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        
        [Header("检测配置")]
        [SerializeField] private float detectionInterval = 0.1f; // 检测间隔
        
        private PlayerCharacterCtrl playerCtrl;
        private IInteractable currentTarget;
        private float lastDetectionTime;
        
        public event EventHandler<InteractionEventArgs> OnTargetChanged;
        public event EventHandler<InteractionEventArgs> OnInteractionPerformed;
        
        public class InteractionEventArgs : EventArgs
        {
            public IInteractable target;
            public bool isAvailable;
        }
        
        #region Properties
        
        public IInteractable CurrentTarget => currentTarget;
        public bool HasTarget => currentTarget != null;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            playerCtrl = GetComponent<PlayerCharacterCtrl>();
            if (playerCtrl == null)
            {
                Debug.LogError("InteractionSystem requires PlayerCharacterCtrl!");
            }
        }
        
        private void Update()
        {
            // 定期检测可交互对象
            if (Time.time - lastDetectionTime > detectionInterval)
            {
                DetectInteractables();
                lastDetectionTime = Time.time;
            }
            
            // 检测交互输入
            if (Input.GetKeyDown(interactKey) && HasTarget)
            {
                PerformInteraction();
            }
        }
        
        #endregion
        
        #region Detection
        
        private void DetectInteractables()
        {
            // 查找范围内的所有可交互对象
            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                transform.position,
                interactionRange,
                interactableLayer
            );
            
            IInteractable bestTarget = null;
            float bestDistance = float.MaxValue;
            int bestPriority = int.MinValue;
            
            foreach (var collider in colliders)
            {
                IInteractable interactable = collider.GetComponent<IInteractable>();
                
                if (interactable != null && interactable.CanInteract(playerCtrl))
                {
                    float distance = Vector2.Distance(transform.position, collider.transform.position);
                    int priority = interactable.GetInteractionPriority();
                    
                    // 优先级更高，或优先级相同但距离更近
                    if (priority > bestPriority || (priority == bestPriority && distance < bestDistance))
                    {
                        bestTarget = interactable;
                        bestDistance = distance;
                        bestPriority = priority;
                    }
                }
            }
            
            // 更新目标
            if (bestTarget != currentTarget)
            {
                SetTarget(bestTarget);
            }
        }
        
        private void SetTarget(IInteractable newTarget)
        {
            currentTarget = newTarget;
            
            OnTargetChanged?.Invoke(this, new InteractionEventArgs
            {
                target = newTarget,
                isAvailable = newTarget != null
            });
            
            if (newTarget != null)
            {
                Debug.Log($"Interaction target: {newTarget.GetInteractionPrompt()}");
            }
        }
        
        #endregion
        
        #region Interaction
        
        private void PerformInteraction()
        {
            if (currentTarget == null) return;
            if (!currentTarget.CanInteract(playerCtrl)) return;
            
            currentTarget.Interact(playerCtrl);
            
            OnInteractionPerformed?.Invoke(this, new InteractionEventArgs
            {
                target = currentTarget,
                isAvailable = true
            });
            
            // 交互后重新检测（有些交互会改变状态）
            DetectInteractables();
        }
        
        #endregion
        
        #region Manual Control
        
        /// <summary>
        /// 手动触发交互（用于UI按钮等）
        /// </summary>
        public void TriggerInteraction()
        {
            PerformInteraction();
        }
        
        /// <summary>
        /// 手动设置交互键
        /// </summary>
        public void SetInteractionKey(KeyCode key)
        {
            interactKey = key;
        }
        
        #endregion
        
        #region Gizmos
        
        private void OnDrawGizmosSelected()
        {
            // 显示交互范围
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position, interactionRange);
            
            // 显示当前目标
            if (currentTarget != null)
            {
                Transform targetTransform = currentTarget.GetInteractionTransform();
                if (targetTransform != null)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(transform.position, targetTransform.position);
                    Gizmos.DrawSphere(targetTransform.position, 0.2f);
                }
            }
        }
        
        #endregion
    }
}

