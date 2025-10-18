using UnityEngine;
using Assets.Scripts.Cooking;

namespace Assets.Scripts.Interaction
{
    /// <summary>
    /// 可交互的出餐口
    /// </summary>
    [RequireComponent(typeof(DeliveryCounter))]
    public class InteractableDelivery : MonoBehaviour, IInteractable
    {
        private DeliveryCounter delivery;
        
        private void Awake()
        {
            delivery = GetComponent<DeliveryCounter>();
        }
        
        public string GetInteractionPrompt()
        {
            return "提交订单 [E]";
        }
        
        public bool CanInteract(PlayerCharacterCtrl player)
        {
            if (delivery == null) return false;
            
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory == null || inventory.IsEmpty) return false;
            
            // 检查玩家是否持有已烹饪的食材
            // 这里简化处理，假设烹饪后的食材有特殊标记
            return true;
        }
        
        public void Interact(PlayerCharacterCtrl player)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory == null) return;
            
            Ingredient current = inventory.GetCurrentIngredient();
            if (current == null) return;
            
            // TODO: 这里需要一个方法来判断食材对应的配方
            // 简化版本：直接匹配配方
            // RecipeData recipe = FindMatchingRecipe(current);
            // if (recipe != null && delivery.DeliverDish(recipe))
            // {
            //     inventory.RemoveIngredient(current);
            //     current.DestroySelf();
            //     Debug.Log("Order delivered!");
            // }
            
            Debug.Log("Delivery interaction - TODO: Match recipe");
        }
        
        public Transform GetInteractionTransform()
        {
            return transform;
        }
        
        public int GetInteractionPriority()
        {
            return 8; // 出餐口优先级高
        }
    }
}

