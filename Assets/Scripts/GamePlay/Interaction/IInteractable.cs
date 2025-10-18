using UnityEngine;

namespace Assets.Scripts.Interaction
{
    /// <summary>
    /// 可交互接口
    /// 所有可以被玩家交互的对象都实现此接口
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// 获取交互提示文本
        /// </summary>
        string GetInteractionPrompt();
        
        /// <summary>
        /// 是否可以交互
        /// </summary>
        bool CanInteract(PlayerCharacterCtrl player);
        
        /// <summary>
        /// 执行交互
        /// </summary>
        void Interact(PlayerCharacterCtrl player);
        
        /// <summary>
        /// 获取交互位置
        /// </summary>
        Transform GetInteractionTransform();
        
        /// <summary>
        /// 交互优先级（数字越大优先级越高）
        /// </summary>
        int GetInteractionPriority();
    }
}

