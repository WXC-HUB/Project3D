using UnityEngine;
using Assets.Scripts.Cooking;

namespace Assets.Scripts.Interaction
{
    /// <summary>
    /// 可交互的厨具
    /// </summary>
    [RequireComponent(typeof(KitchenApplianceBase))]
    public class InteractableAppliance : MonoBehaviour, IInteractable
    {
        private KitchenApplianceBase appliance;
        
        private void Awake()
        {
            appliance = GetComponent<KitchenApplianceBase>();
        }
        
        public string GetInteractionPrompt()
        {
            if (appliance == null) return "";
            
            if (appliance.HasResult())
            {
                return "拾取烹饪成品 [E]";
            }
            else
            {
                return $"使用{GetApplianceName()} [E]";
            }
        }
        
        public bool CanInteract(PlayerCharacterCtrl player)
        {
            if (appliance == null) return false;
            
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory == null) return false;
            
            // 如果有成品，可以拾取
            if (appliance.HasResult())
            {
                return !inventory.IsFull;
            }
            
            // 如果玩家持有食材，可以放入
            Ingredient current = inventory.GetCurrentIngredient();
            return current != null && appliance.CanAcceptIngredient(current);
        }
        
        public void Interact(PlayerCharacterCtrl player)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory == null) return;
            
            // 优先拾取成品
            if (appliance.HasResult())
            {
                Ingredient result = appliance.TakeResult();
                if (result != null && inventory.AddIngredient(result))
                {
                    Debug.Log("Picked up cooked ingredient");
                }
                return;
            }
            
            // 放入食材烹饪
            Ingredient current = inventory.GetCurrentIngredient();
            if (current != null && appliance.AddIngredient(current))
            {
                inventory.RemoveIngredient(current);
                Debug.Log($"Started cooking {current.GetIngredientData().ingredientName}");
            }
        }
        
        public Transform GetInteractionTransform()
        {
            return transform;
        }
        
        public int GetInteractionPriority()
        {
            return 5; // 厨具优先级中等
        }
        
        private string GetApplianceName()
        {
            if (appliance is StirFryAppliance) return "炒锅";
            if (appliance is StewAppliance) return "炖锅";
            if (appliance is FryAppliance) return "炸锅";
            if (appliance is RoastAppliance) return "烤炉";
            return "厨具";
        }
    }
}

