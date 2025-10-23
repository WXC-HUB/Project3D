using Assets.Scripts.BaseUtils;
using Assets.Scripts.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_TimeFollowInfo
{
    public LevelGridTileObject followChar;
    public Slider slider_last_time;

    public GameObject bindTranform;
    
    // Grid 相关
    public Transform m_Grid_Dish;
    public GameObject m_DishItem_Template;
    public List<int> currentDishList = new List<int>(); // 记录当前显示的菜品列表
    
    // 目标菜品显示
    public Image m_Goal_Dish;
    public int currentRecipeID = 0; // 记录当前显示的菜谱ID
}


public class UI_cooktime : BaseUI<UI_cooktime>
{
    public Dictionary<LevelGridTileObject, UI_TimeFollowInfo> last_time_dics = new Dictionary<LevelGridTileObject, UI_TimeFollowInfo>();
    
    // 获取菜品图标路径
    public string GetDishImageByID(int dishID)
    {
        Dish dd = GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.DishID == dishID);
        if (dd != null) 
        {
            return dd.IconPath;
        }
        return "";
    }

    // 加载图片到UI
    public void LoadImageToUI(Image image, string path)
    {
        if (image == null || string.IsNullOrEmpty(path))
            return;

        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite != null)
        {
            image.sprite = sprite;
        }
    }
    
    // 更新 Grid 中的菜品显示
    void UpdateDishGrid(UI_TimeFollowInfo uInfo)
    {
        if (uInfo.m_Grid_Dish == null || uInfo.m_DishItem_Template == null)
            return;
            
        List<int> newDishList = uInfo.followChar.NowRecipeAddedDish;
        
        // 检查列表是否有变化
        if (newDishList.Count != uInfo.currentDishList.Count || 
            !newDishList.SequenceEqual(uInfo.currentDishList))
        {
            // 清除旧的显示项（不包括模板）
            for (int i = uInfo.m_Grid_Dish.childCount - 1; i >= 0; i--)
            {
                GameObject child = uInfo.m_Grid_Dish.GetChild(i).gameObject;
                if (child != uInfo.m_DishItem_Template)
                {
                    Destroy(child);
                }
            }
            
            // 添加新的显示项
            foreach (int dishID in newDishList)
            {
                GameObject newItem = Instantiate(uInfo.m_DishItem_Template);
                newItem.transform.SetParent(uInfo.m_Grid_Dish, false);
                
                // 设置菜品图标
                Transform spriteTransform = GameUtils.FindChildInTransform(newItem.transform, "m_Sprite_Dish");
                if (spriteTransform != null)
                {
                    Image dishImage = spriteTransform.GetComponent<Image>();
                    LoadImageToUI(dishImage, GetDishImageByID(dishID));
                }
                
                newItem.SetActive(true);
            }
            
            // 更新记录的列表
            uInfo.currentDishList = new List<int>(newDishList);
        }
    }
    
    // 更新目标菜品显示
    void UpdateGoalDish(UI_TimeFollowInfo uInfo)
    {
        if (uInfo.m_Goal_Dish == null)
            return;
            
        int recipeID = uInfo.followChar.NowRecipeID;
        
        // 检查菜谱ID是否有变化
        if (recipeID != uInfo.currentRecipeID)
        {
            int targetDishID = 0;
            
            if (recipeID == -1)
            {
                // 菜谱匹配失败，显示100号废菜
                targetDishID = 100;
            }
            else if (recipeID > 0)
            {
                // 有匹配的菜谱，查找对应的成品菜
                Recipe recipe = GameTableConfig.Instance.Config_Recipe.FindFirstLine(x => x.RecipeID == recipeID);
                if (recipe != null)
                {
                    targetDishID = recipe.CookResult;
                }
            }
            
            // 更新显示
            if (targetDishID > 0)
            {
                LoadImageToUI(uInfo.m_Goal_Dish, GetDishImageByID(targetDishID));
                uInfo.m_Goal_Dish.gameObject.SetActive(true);
            }
            else
            {
                // 没有目标菜品时隐藏
                uInfo.m_Goal_Dish.gameObject.SetActive(false);
            }
            
            // 更新记录的菜谱ID
            uInfo.currentRecipeID = recipeID;
        }
    }
    
    public void CallFocus(LevelGridTileObject from_char)
    {
        if(from_char == null || from_char.NowRecipeID == 0 || last_time_dics.ContainsKey(from_char))
        {
            return;
        }
        UI_TimeFollowInfo uInfo = new UI_TimeFollowInfo();  
        uInfo.followChar = from_char;

        var item_i = nodeDics["m_Slider_Level01_s_Blue"];
        var spawn_item = Instantiate(item_i);
        uInfo.slider_last_time = spawn_item.GetComponent<Slider>();
        uInfo.bindTranform = spawn_item;
        
        // 获取 Grid 和 DishItem 模板引用
        uInfo.m_Grid_Dish = GameUtils.FindChildInTransform(spawn_item.transform, "m_Grid_Dish");
        uInfo.m_DishItem_Template = GameUtils.FindChildInTransform(spawn_item.transform, "m_DishItem").gameObject;
        if (uInfo.m_DishItem_Template != null)
        {
            uInfo.m_DishItem_Template.SetActive(false); // 隐藏模板
        }
        
        // 获取目标菜品显示
        Transform goalDishTransform = GameUtils.FindChildInTransform(spawn_item.transform, "m_Goal_Dish");
        if (goalDishTransform != null)
        {
            uInfo.m_Goal_Dish = goalDishTransform.GetComponent<Image>();
        }

        last_time_dics.Add(from_char, uInfo);   

        spawn_item.SetActive(true);
        spawn_item.transform.SetParent(transform , false);

    }
    
    // Start is called before the first frame update
    void Start()
    {
        nodeDics["m_Slider_Level01_s_Blue"].SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        var keysToRemove = new List<LevelGridTileObject>(); // 请将 YourKeyType 替换为实际的键类型

        var key_list = last_time_dics.Keys;
        foreach (var key in key_list)
        {
            if (key.NowRecipeID == 0)
            {
                keysToRemove.Add(key);
            }
            else
            {
                float slider_value = (float)key.NowRecipeTime / (float)key.ShowMaxRecipeTime;
                if (slider_value > 1)
                {
                    slider_value = 1;
                }
                if (slider_value < 0)
                {
                    slider_value = 0;
                }
                last_time_dics[key].slider_last_time.value = slider_value;
                last_time_dics[key].bindTranform.transform.position = Camera.main.WorldToScreenPoint(key.transform.position);
                
                // 更新菜品 Grid 显示
                UpdateDishGrid(last_time_dics[key]);
                
                // 更新目标菜品显示
                UpdateGoalDish(last_time_dics[key]);
            }
        }

        // 遍历结束后再删除
        foreach (var key in keysToRemove)
        {
            Destroy(last_time_dics[key].bindTranform.gameObject);
            
            last_time_dics.Remove(key);
        }
    }
}
