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
            Debug.LogWarning($"拒绝提交：菜品 ID {dishId} ({dish_config.Name}) 不在允许提交的菜品列表中");
            return false;
        }
        
        // 提交菜品到管理器
        DishSubmissionManager.Instance.AddDishSubmission(dishId, 1);
        
        // 获取提交后的数量
        int currentCount = DishSubmissionManager.Instance.GetDishCount(dishId);
        
        // 删除菜品GameObject
        attach_obj.Die();
        
        Debug.Log($"成功提交菜品 ID: {dishId}, 名称: {dish_config.Name}, 当前数量: {currentCount}");
        
        return true;
    }

    public override bool TryAttachObject(CharacterCtrlBase attach_obj)
    {
        bool is_dish = !(null == GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.GameCharacter == attach_obj.MyGameObjectID));
        if (is_dish && this.CookType.GetValue() >0)
        {
            nowAttachList.Add(attach_obj);
            attach_obj.transform.SetParent(transform, true);
            attach_obj.transform.position = transform.position + new Vector3(0 + nowAttachList.Count * 0.3F, 0, -1.6f);
            attach_obj.isAttachedToOther = true;
            return TryAttachDish(attach_obj);
        }
        else if (is_dish && this.DishOutletType.GetValue() > 0)
        {
            return TrySubmitDish(attach_obj);
        }
        else
        {
            attach_obj.transform.SetParent(transform, true);

            attach_obj.transform.position = transform.position + new Vector3(0, 0, -1.6f);
            nowAttachList.Add(attach_obj);
            attach_obj.isAttachedToOther = true;
            return true;
        }

        return false;
        
        
    }
}
