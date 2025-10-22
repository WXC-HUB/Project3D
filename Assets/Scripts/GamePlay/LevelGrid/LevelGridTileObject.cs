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
        if (NowRecipeID > 0 && NowRecipeTime > 0) 
        { 
            NowRecipeTime -= Time.deltaTime;
            Debug.Log(NowRecipeTime);
        }
        if (NowRecipeID > 0 && NowRecipeTime <= 0)
        {
            FinishNowRecipe();
        }
    }

    void FinishNowRecipe()
    {
        foreach(var item in nowAttachList)
        {
            item.Die();//直接杀死所有当前附加列表，将来记得改
        }

        
        nowAttachList.Clear();

        Recipe new_recipe = GameTableConfig.Instance.Config_Recipe.FindFirstLine(x => (x.RecipeID == NowRecipeID));
        Dish new_dis = GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.DishID == new_recipe.CookResult);
        var newgo = LevelManager.Instance.SpawnCharacterByID<CharacterCtrlBase>(new_dis.GameCharacter);
        this.TryAttachObject(newgo);

        // 清理烹饪特效和 Modifier
        // 收集需要删除的 Modifier，避免遍历时修改集合
        CharacterModifier[] modifiers = GetComponents<CharacterModifier>();
        List<CharacterModifier> toDispel = new List<CharacterModifier>();
        
        foreach (var modifier in modifiers)
        {
            if (CookModifiers.Contains(modifier.ModifierID))
            {
                toDispel.Add(modifier);
            }
        }
        
        // 统一执行 Dispel
        foreach (var modifier in toDispel)
        {
            modifier.ModifierDispel();
        }
        
        CookModifiers.Clear();

        NowRecipeID = 0;
        NowRecipeTime = 0;  
    }

    public void SetSelect(bool isSelect)
    {

        this.GetComponent<Outline>().enabled = isSelect;
    }

    bool TryAttachDish(CharacterCtrlBase attach_obj)
    {
        var ii = GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.GameCharacter == attach_obj.MyGameObjectID);
        int attach_dish_id = GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.GameCharacter == attach_obj.MyGameObjectID).DishID;
        if (NowRecipeID == 0)
        {
            List<Recipe> new_recipe_list = GameTableConfig.Instance.Config_Recipe.FindAllLine(x => (x.CookType == this.CookType.GetValue() && x.DishList.Contains(attach_dish_id)));
            new_recipe_list.Sort((a, b) => a.DishList.Count.CompareTo(b.DishList.Count));
            if (new_recipe_list.Count > 0)
            {
                Recipe new_recipe = new_recipe_list[0];
                int food_index = new_recipe.DishList.IndexOf(attach_dish_id);
                NowRecipeTime = new_recipe.CookTime[food_index];
                NowRecipeID = new_recipe.RecipeID;

                foreach (int buffid in new_recipe.OnCookBuffList)
                {
                    if (!CookModifiers.Contains(buffid))
                    {
                        SkillDispatchCenter.Instance.AddModifierToCharacter(this, -1, buffid);
                        CookModifiers.Add(buffid);
                    }
                }
                NowRecipeAddedDish = new List<int> { attach_dish_id };

                return true;
            }
        }
        else
        {
            if (NowRecipeAddedDish.Contains(attach_dish_id))
            {
                //重复添加，当前机制下直接拦截
                return false;
            }
            else
            {
                List<int> temp = NowRecipeAddedDish;
                temp.Add(attach_dish_id);
                List<Recipe> new_recipe_list = GameTableConfig.Instance.Config_Recipe.FindAllLine(
                    x => (x.CookType == this.CookType.GetValue() && (temp.All(item => x.DishList.Contains(item))))
                );
                if (new_recipe_list.Count > 0)
                {
                    Recipe new_recipe = new_recipe_list[0];
                    int food_index = new_recipe.DishList.IndexOf(attach_dish_id);
                    NowRecipeTime += new_recipe.CookTime[food_index];
                    NowRecipeID = new_recipe.RecipeID;
                    NowRecipeAddedDish = temp;

                    foreach (int buffid in new_recipe.OnCookBuffList)
                    {
                        if (!CookModifiers.Contains(buffid))
                        {
                            SkillDispatchCenter.Instance.AddModifierToCharacter(this, -1, buffid);
                            CookModifiers.Add(buffid);
                        }
                    }
                }

            }
        }

        return false;

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
        // 如果正在烹饪，不允许拿走食材
        if (NowRecipeID > 0)
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
            nowAttachList.Add(attach_obj);
            attach_obj.transform.SetParent(transform, true);
            attach_obj.transform.position = transform.position + new Vector3(0 + nowAttachList.Count * 0.3F, 0, -0.5f);
            attach_obj.isAttachedToOther = true;
            return TryAttachDish(attach_obj);
        }
        else if (is_dish && this.DishOutletType.GetValue() > 0)
        {
            return TrySubmitDish(attach_obj);
        }
        else if(is_dish && this.nowAttachList.Any(x=>x.MyObjectLayer is InGameCharacterType.Tower))
        {
            Dish dish_confg = GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.GameCharacter == attach_obj.MyGameObjectID);
            bool canAddBuff = false;
            foreach(string buff_info in dish_confg.OnEatBuffList)
            {
                var buff_p = buff_info.Split(';');
                if(buff_p.Length < 2)
                {
                    continue;
                }
                else
                {
                    int b_id = int.Parse(buff_p[0]);
                    float b_time = float.Parse(buff_p[1]);
                    foreach(var tower in this.nowAttachList)
                    {
                        if(tower.MyObjectLayer is InGameCharacterType.Tower)
                        {
                            SkillDispatchCenter.Instance.AddModifierToCharacter(tower , b_time , b_id);
                        }
                    }
                    canAddBuff = true;
                }
            }
            if (canAddBuff)
            {

                attach_obj.Die();
                return true;
            }
            else
            {
                Debug.LogWarning("提交的道具没有绑定Buff！禁止提交");
                return false;
            }
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
