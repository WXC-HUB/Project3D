using UnityEngine;
using Assets.Scripts.Tower;
using Assets.Scripts.Cooking;

namespace Assets.Scripts.Interaction
{
    /// <summary>
    /// 可交互的防御塔
    /// </summary>
    [RequireComponent(typeof(TowerController))]
    public class InteractableTower : MonoBehaviour, IInteractable
    {
        private TowerController tower;
        private TowerEnergySystem energySystem;
        
        private void Awake()
        {
            tower = GetComponent<TowerController>();
            energySystem = GetComponent<TowerEnergySystem>();
        }
        
        public string GetInteractionPrompt()
        {
            if (energySystem != null)
            {
                return $"喂食防御塔 [E] (能量: {energySystem.GetNormalizedEnergy() * 100:F0}%)";
            }
            return "喂食防御塔 [E]";
        }
        
        public bool CanInteract(PlayerCharacterCtrl player)
        {
            if (tower == null) return false;
            
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory == null || inventory.IsEmpty) return false;
            
            // 检查玩家是否持有已烹饪的食品
            // TODO: 需要判断食材是否已烹饪完成
            return tower.CanBeFed();
        }
        
        public void Interact(PlayerCharacterCtrl player)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory == null) return;
            
            Ingredient current = inventory.GetCurrentIngredient();
            if (current == null) return;
            
            // TODO: 获取食材对应的配方
            // RecipeData recipe = GetRecipeForIngredient(current);
            // if (recipe != null && tower.Feed(recipe))
            // {
            //     inventory.RemoveIngredient(current);
            //     current.DestroySelf();
            //     Debug.Log($"Fed tower with {recipe.recipeName}");
            // }
            
            Debug.Log("Tower feeding - TODO: Match recipe");
        }
        
        public Transform GetInteractionTransform()
        {
            return transform;
        }
        
        public int GetInteractionPriority()
        {
            return 10; // 防御塔优先级最高
        }
    }
}

