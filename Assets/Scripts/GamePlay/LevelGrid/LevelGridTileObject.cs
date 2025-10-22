using Assets.Scripts.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelGridTileObject : CharacterCtrlBase
{
    public Character_Int CookType = new Character_Int("CookType", 0);
    public Character_Int DishOutletType = new Character_Int("DishOutletType", 0);

    List<int> CookModifiers = new List<int>();  

    public int NowRecipeID = 0;
    public float NowRecipeTime = 0f;
    public List<int> NowRecipeAddedDish = new List<int>();
    private void Awake()
    {
        base.Awake();

        CookType.TakeEffect(this);
        DishOutletType.TakeEffect(this);
    }
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Outline>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // 烹饪倒计时逻辑
        if (NowRecipeID != 0)
        {
            NowRecipeTime -= Time.deltaTime;
            Debug.Log(NowRecipeTime);
            
            // 倒计时结束，完成烹饪
            if (NowRecipeTime <= 0)
            {
                FinishNowRecipe();
            }
        }
    }

    void FinishNowRecipe()
    {
        // 食材已经在添加时销毁了，这里只需要清空列表（如果有成品菜的话）
        // 注意：正常情况下，烹饪时nowAttachList应该是空的
        // 只有做完后成品菜会在nowAttachList中
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
                // 清理状态后直接返回
                ClearCookingState();
                return;
            }
            
            newgo = LevelManager.Instance.SpawnCharacterByID<CharacterCtrlBase>(fail_dish.GameCharacter);
            Debug.Log($"烹饪失败！产出废菜: {fail_dish.Name}");
        }
        
        // 先清理烹饪状态
        ClearCookingState();
        
        // 将成品直接放到锅上（不通过TryAttachObject，避免再次进入烹饪逻辑）
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
    }

    public void SetSelect(bool isSelect)
    {

        this.GetComponent<Outline>().enabled = isSelect;
    }

    /// <summary>
    /// 获取每种CookType的基础烹饪时间
    /// </summary>
    float GetBaseCookTime(int cookType)
    {
        switch (cookType)
        {
            case 1: return 5f;  // 锅
            case 2: return 3f;  // 烤
            case 3: return 8f;  // 煮
            default: return 5f; // 默认5秒
        }
    }

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
            NowRecipeAddedDish = new List<int> { attach_dish_id };
        }
        else
        {
            // 继续添加食材
            // 检查是否重复添加
            if (NowRecipeAddedDish.Contains(attach_dish_id))
            {
                Debug.Log("不能重复添加相同的食材！");
                return false;
            }
            
            NowRecipeTime += dish_cook_time;
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
        
        // 步骤4: 销毁食材GameObject
        attach_obj.Die();
        return true;
    }

    bool TrySubmitDish(CharacterCtrlBase attach_obj)
    {
        // 获取菜品ID
        Dish dish_config = GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.GameCharacter == attach_obj.MyGameObjectID);
        if (dish_config == null)
        {
            return false;
        }

        int dishId = dish_config.DishID;
        
        // 检查是否是允许提交的菜品
        if (!DishSubmissionManager.Instance.GetAllDishIds().Contains(dishId))
        {
            Debug.LogWarning($"菜品 ID {dishId} ({dish_config.Name}) 不在允许提交的菜品列表中，无事发生");
        } else {
            // 提交菜品到管理器
            DishSubmissionManager.Instance.AddDishSubmission(dishId, 1);
            // 获取提交后的数量
            int currentCount = DishSubmissionManager.Instance.GetDishCount(dishId);
            Debug.LogWarning($"成功提交菜品 ID: {dishId}, 名称: {dish_config.Name}, 当前数量: {currentCount}");
        }
        
        // 删除菜品GameObject
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

    public override bool TryAttachObject(CharacterCtrlBase attach_obj)
    {
        bool is_dish = !(null == GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.GameCharacter == attach_obj.MyGameObjectID));
        if (is_dish && this.CookType.GetValue() >0)
        {
            // 检查锅的状态
            // 如果锅上已经有成品菜（做完了但还没拿走），不允许添加食材
            // 判断：NowRecipeID == 0（已完成） 且 nowAttachList.Count > 0（有成品菜）
            if (NowRecipeID == 0 && nowAttachList.Count > 0)
            {
                Debug.Log("锅上已经有成品菜，请先取走再添加新食材！");
                return false;
            }
            
            // 其他情况（锅空或正在烹饪中）都可以添加食材
            // 食材会在TryAttachDish中立即销毁，不添加到nowAttachList
            return TryAttachDish(attach_obj);
        }
        else if (is_dish && this.DishOutletType.GetValue() > 0)
        {
            return TrySubmitDish(attach_obj);
        }
        else
        {
            attach_obj.transform.SetParent(transform, true);

            attach_obj.transform.position = transform.position + new Vector3(0, 0,-0.5f);
            nowAttachList.Add(attach_obj);
            attach_obj.isAttachedToOther = true;
            return true;
        }

        return false;
        
        
    }
}
