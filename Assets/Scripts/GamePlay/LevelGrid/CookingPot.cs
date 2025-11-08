using Assets.Scripts.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 烹饪锅状态枚举
/// </summary>
public enum CookingPotState
{
    Idle,       // 空闲状态：可以开始烹饪
    Cooking,    // 烹饪中：正在倒计时
    Finished    // 完成状态：做好了但还没拿走
}

/// <summary>
/// 烹饪锅，负责烹饪菜品
/// </summary>
public class CookingPot : LevelGridTileObject
{
    public Character_Int CookType = new Character_Int("CookType", 0);
    
    private List<int> CookModifiers = new List<int>();  
    public int NowRecipeID = 0;
    public float NowRecipeTime = 0f;
    public float MaxRecipeTime = 0f;
    public float ShowMaxRecipeTime = 0f;
    public List<int> NowRecipeAddedDish = new List<int>();
    
    // 烹饪锅的当前状态
    private CookingPotState currentState = CookingPotState.Idle;
    
    /// <summary>
    /// 获取当前状态
    /// </summary>
    public CookingPotState CurrentState => currentState;

    private void Awake()
    {
        base.Awake();
        CookType.TakeEffect(this);
    }

    void Update()
    {
        // 烹饪倒计时逻辑（只在烹饪中状态才倒计时）
        if (currentState == CookingPotState.Cooking)
        {
            NowRecipeTime -= Time.deltaTime;
            
            // 倒计时结束，完成烹饪
            if (NowRecipeTime <= 0)
            {
                FinishNowRecipe();
            }
        }
    }

    /// <summary>
    /// 完成烹饪，生成成品菜
    /// </summary>
    void FinishNowRecipe()
    {
        Debug.Log($"[CookingPot] 开始完成烹饪，RecipeID={NowRecipeID}, State={currentState}");
        
        // 清空列表
        nowAttachList.Clear();

        // 根据RecipeID产出菜品
        CharacterCtrlBase newgo = null;
        bool cookSuccess = false;
        
        // 尝试产出正常菜品
        if (NowRecipeID > 0)
        {
            Recipe recipe = GameTableConfig.Instance.Config_Recipe.FindFirstLine(x => (x.RecipeID == NowRecipeID));
            if (recipe != null)
            {
                Dish result_dish = GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.DishID == recipe.CookResult);
                if (result_dish != null)
                {
                    newgo = LevelManager.Instance.SpawnCharacterByID<CharacterCtrlBase>(result_dish.GameCharacter);
                    Debug.Log($"烹饪成功！产出菜品: {result_dish.Name}");
                    cookSuccess = true;
                }
            }
        }
        
        // 如果烹饪不成功（RecipeID=-1 或找不到菜谱/菜品），产出100号废菜
        if (!cookSuccess)
        {
            Dish fail_dish = GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.DishID == 100);
            if (fail_dish == null)
            {
                Debug.LogError("烹饪失败但找不到100号废菜配置！制作失败，回到空闲状态。");
                // 制作失败，直接回到空闲态
                SetState(CookingPotState.Idle);
                ClearCookingState();
                return;
            }
            
            newgo = LevelManager.Instance.SpawnCharacterByID<CharacterCtrlBase>(fail_dish.GameCharacter);
            Debug.Log($"烹饪失败！产出废菜: {fail_dish.Name}");
        }
        
        // 只清理特效和时间，保留 RecipeID（玩家拿走后才清零）
        ClearCookingEffects();
        
        // 将成品直接放到锅上
        if (newgo != null)
        {
            newgo.transform.SetParent(transform, true);
            newgo.transform.position = transform.position + new Vector3(0, 0, -0.5f);
            nowAttachList.Add(newgo);
            newgo.isAttachedToOther = true;
            
            // 转到完成状态
            SetState(CookingPotState.Finished);
            Debug.Log($"[CookingPot] 烹饪完成，转到完成状态，RecipeID={NowRecipeID}");
        }
        else
        {
            Debug.LogError("[CookingPot] 未能生成成品菜品对象！制作失败，回到空闲状态。");
            // 制作失败，直接回到空闲态
            SetState(CookingPotState.Idle);
            ClearCookingState();
        }
    }
    
    /// <summary>
    /// 清理烹饪特效和时间，但保留 RecipeID
    /// </summary>
    void ClearCookingEffects()
    {
        // 清理烹饪特效和 Modifier
        CharacterModifier[] modifiers = GetComponents<CharacterModifier>();
        List<CharacterModifier> toDispel = new List<CharacterModifier>();
        
        foreach (var modifier in modifiers)
        {
            if (CookModifiers.Contains(modifier.ModifierID))
            {
                toDispel.Add(modifier);
            }
        }
        
        foreach (var modifier in toDispel)
        {
            modifier.ModifierDispel();
        }
        
        CookModifiers.Clear();

        // 只重置时间，保留 RecipeID
        NowRecipeTime = 0;
        MaxRecipeTime = 0;
    }
    
    /// <summary>
    /// 完全清理烹饪状态
    /// </summary>
    void ClearCookingState()
    {
        ClearCookingEffects();
        
        // 重置 RecipeID 和食材列表
        NowRecipeID = 0;
        NowRecipeAddedDish.Clear();
    }
    
    /// <summary>
    /// 设置烹饪锅状态
    /// </summary>
    void SetState(CookingPotState newState)
    {
        if (currentState != newState)
        {
            Debug.Log($"[CookingPot] 状态转换: {currentState} -> {newState}");
            currentState = newState;
        }
    }
    
    /// <summary>
    /// 判断是否正在烹饪（还在倒计时）
    /// </summary>
    public bool IsCooking()
    {
        return currentState == CookingPotState.Cooking;
    }
    
    /// <summary>
    /// 判断是否已完成烹饪（做好了但还没拿走）
    /// </summary>
    public bool IsFinished()
    {
        return currentState == CookingPotState.Finished;
    }
    
    /// <summary>
    /// 判断锅是否空闲（可以开始新的烹饪）
    /// </summary>
    public bool IsIdle()
    {
        return currentState == CookingPotState.Idle;
    }

    /// <summary>
    /// 获取每种CookType的基础烹饪时间
    /// </summary>
    float GetBaseCookTime(int cookType)
    {
        switch (cookType)
        {
            case 1: return 7.0f;
            case 2: return 7.0f;
            case 3: return 7.0f;
            default: return 7.0f;
        }
    }
    
    /// <summary>
    /// 检查是否可以添加指定的菜品（不执行实际添加操作）
    /// </summary>
    /// <param name="dishId">菜品ID</param>
    /// <returns>true表示可以添加，false表示不能添加</returns>
    bool CanAddDish(int dishId)
    {
        // 检查CookType
        if (this.CookType.GetValue() <= 0)
        {
            return false;
        }
        
        // 检查是否是成品菜
        if (IsFinishedDish(dishId))
        {
            return false;
        }
        
        // 检查锅的状态
        if (IsFinished())
        {
            // 已完成烹饪，不能添加食材
            return false;
        }
        
        if (IsCooking())
        {
            // 正在烹饪中，检查是否超过3个菜品
            if (NowRecipeAddedDish.Count >= 3)
            {
                return false;
            }
            
            // 检查是否重复添加
            if (NowRecipeAddedDish.Contains(dishId))
            {
                return false;
            }
        }
        
        // 空闲状态或正在烹饪且满足条件，可以添加
        return true;
    }

    /// <summary>
    /// 尝试添加菜品到锅里
    /// </summary>
    bool TryAttachDish(CharacterCtrlBase attach_obj)
    {
        // 获取食材信息
        Dish dish_info = GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.GameCharacter == attach_obj.MyGameObjectID);
        if (dish_info == null)
        {
            return false;
        }
        
        int attach_dish_id = dish_info.DishID;
        float dish_cook_time = dish_info.CookTime;
        
        // 步骤1: 更新时间和食材列表
        if (IsIdle())
        {
            // 第一次放入食材，开始烹饪
            NowRecipeTime = GetBaseCookTime(this.CookType.GetValue()) + dish_cook_time;
            MaxRecipeTime = NowRecipeTime;
            ShowMaxRecipeTime = MaxRecipeTime;
            NowRecipeAddedDish = new List<int> { attach_dish_id };
            
            // 转到烹饪状态
            SetState(CookingPotState.Cooking);
        }
        else if (IsCooking())
        {
            // 继续添加食材
            // 检查是否超过3个菜品（这里再次检查是为了防御性编程）
            if (NowRecipeAddedDish.Count >= 3)
            {
                Debug.Log("已经添加了3个菜品，不能再添加了！");
                return false;
            }
            
            // 检查是否重复添加（这里再次检查是为了防御性编程）
            if (NowRecipeAddedDish.Contains(attach_dish_id))
            {
                Debug.Log("不能重复添加相同的食材！");
                return false;
            }
            
            NowRecipeTime += dish_cook_time;
            MaxRecipeTime += dish_cook_time;
            NowRecipeAddedDish.Add(attach_dish_id);
        }
        else
        {
            // 完成状态不应该走到这里
            Debug.LogError("[CookingPot] 错误：完成状态下不应该调用TryAttachDish");
            return false;
        }
        
        // 步骤2: 统一进行菜谱匹配
        List<Recipe> matching_recipes = GameTableConfig.Instance.Config_Recipe.FindAllLine(
            x => (x.CookType == this.CookType.GetValue() && 
                  x.DishList.Count == NowRecipeAddedDish.Count &&
                  NowRecipeAddedDish.All(dishId => x.DishList.Contains(dishId)))
        );
        
        // 步骤3: 处理匹配结果
        if (matching_recipes.Count > 0)
        {
            // 找到匹配的菜谱
            Recipe matched_recipe = matching_recipes[0];
            bool recipeChanged = (NowRecipeID != matched_recipe.RecipeID);
            
            NowRecipeID = matched_recipe.RecipeID;
            
            if (recipeChanged)
            {
                Debug.Log($"找到匹配的菜谱！RecipeID: {NowRecipeID}");
            }
            
            // 添加烹饪特效（如果还没有）
            foreach (int buffid in matched_recipe.OnCookBuffList)
            {
                if (!CookModifiers.Contains(buffid))
                {
                    SkillDispatchCenter.Instance.AddModifierToCharacter(this, -1, buffid);
                    CookModifiers.Add(buffid);
                }
            }
        }
        else
        {
            // 没有匹配的菜谱，设为-1
            NowRecipeID = -1;
            Debug.Log($"未找到匹配的菜谱，RecipeID设为-1");
        }

        UI_cooktime.instance.CallFocus(this);

        // 步骤4: 销毁食材GameObject
        attach_obj.Die();
        return true;
    }

    /// <summary>
    /// 重写玩家交互判断方法，返回具体的互动类型（基于状态判断）
    /// </summary>
    public override InteractionType CanInteractWithPlayer(PlayerCharacterCtrl player)
    {
        // 1. 如果玩家手上有食材/菜品
        if (player.nowAttachList.Count > 0)
        {
            CharacterCtrlBase itemInHand = player.nowAttachList[0];
            Dish dish_info = GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.GameCharacter == itemInHand.MyGameObjectID);
            
            // 如果手上的是菜品，检查是否可以添加
            if (dish_info != null && CanAddDish(dish_info.DishID))
            {
                return InteractionType.AddIngredient;
            }
        }
        
        // 2. 如果玩家空手
        if (player.nowAttachList.Count == 0)
        {
            // 如果正在烹饪中，不允许拿走
            if (IsCooking())
            {
                return InteractionType.None;
            }
            
            // 如果锅已完成（有成品菜），可以拿走
            if (IsFinished())
            {
                return InteractionType.PickUpDish;
            }
        }
        
        return InteractionType.None;
    }
    
    /// <summary>
    /// 重写拿下物品方法（基于状态判断）
    /// </summary>
    public override bool TryDropObject(CharacterCtrlBase attach_obj)
    {
        // 如果正在烹饪中，不允许拿走
        if (IsCooking())
        {
            Debug.Log("烹饪进行中，无法拿走食材！");
            return false;
        }

        // 使用基类的默认逻辑拿走物品
        bool result = base.TryDropObject(attach_obj);
        
        // 如果成功拿走物品，并且是完成状态，清除烹饪状态并转到空闲态
        if (result && IsFinished())
        {
            Debug.Log("成品已被拿走，清除烹饪状态，回到空闲状态");
            ClearCookingState();
            SetState(CookingPotState.Idle);
        }
        
        return result;
    }

    /// <summary>
    /// 重写附加物品方法（使用CanAddDish统一判断逻辑）
    /// </summary>
    public override bool TryAttachObject(CharacterCtrlBase attach_obj)
    {
        // 获取dish信息
        Dish dish_info = GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.GameCharacter == attach_obj.MyGameObjectID);
        
        // 如果不是菜品，返回false
        if (dish_info == null)
        {
            return false;
        }
        
        // 使用统一的CanAddDish方法判断是否可以添加
        if (CanAddDish(dish_info.DishID))
        {
            return TryAttachDish(attach_obj);
        }
        
        // 不能添加，返回false
        return false;
    }
}

