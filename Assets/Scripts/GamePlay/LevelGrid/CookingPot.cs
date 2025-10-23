using Assets.Scripts.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    private void Awake()
    {
        base.Awake();
        CookType.TakeEffect(this);
    }

    void Update()
    {
        // 烹饪倒计时逻辑
        if (NowRecipeID != 0)
        {
            NowRecipeTime -= Time.deltaTime;
            // Debug.Log(NowRecipeTime);
            
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
                Debug.LogError("烹饪失败但找不到100号废菜配置！无法产出菜品。");
                ClearCookingState();
                return;
            }
            
            newgo = LevelManager.Instance.SpawnCharacterByID<CharacterCtrlBase>(fail_dish.GameCharacter);
            Debug.Log($"烹饪失败！产出废菜: {fail_dish.Name}");
        }
        
        // 清理烹饪状态
        ClearCookingState();
        
        // 将成品直接放到锅上
        if (newgo != null)
        {
            newgo.transform.SetParent(transform, true);
            newgo.transform.position = transform.position + new Vector3(0, 0, -0.5f);
            nowAttachList.Add(newgo);
            newgo.isAttachedToOther = true;
        }
    }
    
    /// <summary>
    /// 清理烹饪状态和特效
    /// </summary>
    void ClearCookingState()
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

        // 重置烹饪状态
        NowRecipeID = 0;
        NowRecipeTime = 0;
        MaxRecipeTime = 0;
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
        if (NowRecipeID == 0)
        {
            // 第一次放入食材
            NowRecipeTime = GetBaseCookTime(this.CookType.GetValue()) + dish_cook_time;
            MaxRecipeTime = NowRecipeTime;
            ShowMaxRecipeTime = MaxRecipeTime;
            NowRecipeAddedDish = new List<int> { attach_dish_id };
        }
        else
        {
            // 继续添加食材
            // 检查是否超过5个菜品
            if (NowRecipeAddedDish.Count >= 5)
            {
                Debug.Log("已经添加了5个菜品，不能再添加了！");
                return false;
            }
            
            // 检查是否重复添加
            if (NowRecipeAddedDish.Contains(attach_dish_id))
            {
                Debug.Log("不能重复添加相同的食材！");
                return false;
            }
            
            NowRecipeTime += dish_cook_time;
            MaxRecipeTime += dish_cook_time;
            NowRecipeAddedDish.Add(attach_dish_id);
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
    /// 重写拿下物品方法，烹饪进行中不允许拿走食材
    /// </summary>
    public override bool TryDropObject(CharacterCtrlBase attach_obj)
    {
        // 如果正在烹饪（NowRecipeID != 0），不允许拿走食材
        if (NowRecipeID != 0)
        {
            Debug.Log("烹饪进行中，无法拿走食材！");
            return false;
        }

        // 否则使用基类的默认逻辑
        return base.TryDropObject(attach_obj);
    }

    /// <summary>
    /// 重写附加物品方法
    /// </summary>
    public override bool TryAttachObject(CharacterCtrlBase attach_obj)
    {
        bool is_dish = !(null == GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.GameCharacter == attach_obj.MyGameObjectID));
        
        if (is_dish && this.CookType.GetValue() > 0)
        {
            // 获取dish信息
            Dish dish_info = GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.GameCharacter == attach_obj.MyGameObjectID);
            if (dish_info == null)
            {
                return false;
            }
            
            // 检查是否是成品菜
            if (IsFinishedDish(dish_info.DishID))
            {
                Debug.Log("成品菜不能放入锅里烹饪！");
                return false;
            }
            
            // 检查锅的状态
            // 如果锅上已经有成品菜（做完了但还没拿走），不允许添加食材
            if (NowRecipeID == 0 && nowAttachList.Count > 0)
            {
                Debug.Log("锅上已经有成品菜，请先取走再添加新食材！");
                return false;
            }
            
            // 其他情况（锅空或正在烹饪中）都可以添加食材
            return TryAttachDish(attach_obj);
        }
        
        // 其他情况使用基类逻辑
        return base.TryAttachObject(attach_obj);
    }
}

