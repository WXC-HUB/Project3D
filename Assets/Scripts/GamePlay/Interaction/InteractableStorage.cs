using UnityEngine;
using Assets.Scripts.Cooking;
using System.Collections.Generic;

namespace Assets.Scripts.Interaction
{
    /// <summary>
    /// 可交互的仓库
    /// </summary>
    [RequireComponent(typeof(StorageCounter))]
    public class InteractableStorage : MonoBehaviour, IInteractable
    {
        [Header("配置")]
        [SerializeField] private List<IngredientData> availableIngredients = new List<IngredientData>();
        
        private StorageCounter storage;
        private int currentIngredientIndex = 0;
        
        private void Awake()
        {
            storage = GetComponent<StorageCounter>();
        }
        
        public string GetInteractionPrompt()
        {
            return "访问仓库 [E]";
        }
        
        public bool CanInteract(PlayerCharacterCtrl player)
        {
            if (storage == null) return false;
            
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory == null) return false;
            
            // 玩家有食材可以存入，或仓库有食材可以取出
            return !inventory.IsEmpty || storage.GetAllIngredients().Count > 0;
        }
        
        public void Interact(PlayerCharacterCtrl player)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory == null) return;
            
            // 简化版交互：
            // 如果玩家有食材，存入仓库
            // 如果玩家没有食材，从仓库取出第一个可用的
            
            if (!inventory.IsEmpty)
            {
                // 存入当前选中的食材
                Ingredient current = inventory.GetCurrentIngredient();
                if (current != null)
                {
                    storage.StoreIngredientFromInventory(current, inventory);
                    Debug.Log("Stored ingredient in storage");
                }
            }
            else
            {
                // 从仓库取出食材
                TakeIngredientFromStorage(inventory);
            }
        }
        
        private void TakeIngredientFromStorage(PlayerInventory inventory)
        {
            var allIngredients = storage.GetAllIngredients();
            
            foreach (var pair in allIngredients)
            {
                if (pair.Value > 0)
                {
                    // 找到对应的IngredientData
                    IngredientData data = availableIngredients.Find(d => d.ingredientType == pair.Key);
                    if (data != null)
                    {
                        if (storage.TakeIngredientToInventory(pair.Key, inventory, data))
                        {
                            Debug.Log($"Took {data.ingredientName} from storage");
                            return;
                        }
                    }
                }
            }
            
            Debug.Log("Storage is empty or inventory is full");
        }
        
        public Transform GetInteractionTransform()
        {
            return transform;
        }
        
        public int GetInteractionPriority()
        {
            return 7; // 仓库优先级较高
        }
    }
}

