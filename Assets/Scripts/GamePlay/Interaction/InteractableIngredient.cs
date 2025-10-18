using UnityEngine;
using Assets.Scripts.Cooking;

namespace Assets.Scripts.Interaction
{
    /// <summary>
    /// 可交互的食材
    /// </summary>
    [RequireComponent(typeof(Ingredient))]
    public class InteractableIngredient : MonoBehaviour, IInteractable
    {
        private Ingredient ingredient;
        
        private void Awake()
        {
            ingredient = GetComponent<Ingredient>();
        }
        
        public string GetInteractionPrompt()
        {
            if (ingredient != null)
            {
                return $"拾取 {ingredient.GetIngredientData().ingredientName} [E]";
            }
            return "拾取食材 [E]";
        }
        
        public bool CanInteract(PlayerCharacterCtrl player)
        {
            if (ingredient == null) return false;
            if (ingredient.GetHolder() != null) return false; // 已被持有
            
            // 检查玩家背包是否已满
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            return inventory != null && !inventory.IsFull;
        }
        
        public void Interact(PlayerCharacterCtrl player)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            
            if (inventory != null && ingredient != null)
            {
                if (inventory.AddIngredient(ingredient))
                {
                    Debug.Log($"Picked up {ingredient.GetIngredientData().ingredientName}");
                }
            }
        }
        
        public Transform GetInteractionTransform()
        {
            return transform;
        }
        
        public int GetInteractionPriority()
        {
            return 1; // 食材优先级较低
        }
    }
}

