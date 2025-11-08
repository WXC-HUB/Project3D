using Assets.Scripts.BaseUtils;
using Assets.Scripts.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UI_TimeFollowInfo
{
    public CookingPot followChar;
    public Slider slider_last_time;

    public GameObject bindTranform;
    
    // Grid 相关
    public Transform m_Grid_Dish;
    public GameObject m_DishItem_Template;
    public List<int> currentDishList = new List<int>(); // 记录当前显示的菜品列表
    
    // 目标菜品显示
    public Image m_Goal_Dish;
    public int currentRecipeID = 0; // 记录当前显示的菜谱ID
    
    // 完成菜品显示
    public Image m_Finish_Dish;
    public bool isShowingFinished = false; // 标记是否正在显示完成状态
}


public class UI_cooktime : BaseUI<UI_cooktime>
{
    public Dictionary<CookingPot, UI_TimeFollowInfo> last_time_dics = new Dictionary<CookingPot, UI_TimeFollowInfo>();
    
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
    
    public void CallFocus(CookingPot from_char)
    {
        if(from_char == null || from_char.NowRecipeID == 0 || last_time_dics.ContainsKey(from_char))
        {
            return;
        }
        UI_TimeFollowInfo uInfo = new UI_TimeFollowInfo();  
        uInfo.followChar = from_char;

        // 使用根节点结构
        var item_i = nodeDics["m_CookingUI_Root"];
        var spawn_item = Instantiate(item_i);
        uInfo.bindTranform = spawn_item; // bindTransform 绑定到根节点
        
        // 查找 Slider
        Transform sliderTransform = GameUtils.FindChildInTransform(spawn_item.transform, "m_Slider_Level01_s_Blue");
        if (sliderTransform != null)
        {
            uInfo.slider_last_time = sliderTransform.GetComponent<Slider>();
            
            // 从 Slider 下查找 Grid 和 Goal_Dish
            uInfo.m_Grid_Dish = GameUtils.FindChildInTransform(sliderTransform, "m_Grid_Dish");
            
            // m_DishItem 是 m_Grid_Dish 的子元素
            if (uInfo.m_Grid_Dish != null)
            {
                uInfo.m_DishItem_Template = GameUtils.FindChildInTransform(uInfo.m_Grid_Dish, "m_DishItem").gameObject;
                if (uInfo.m_DishItem_Template != null)
                {
                    uInfo.m_DishItem_Template.SetActive(false); // 隐藏模板
                }
            }
            
            // 获取目标菜品显示
            Transform goalDishTransform = GameUtils.FindChildInTransform(sliderTransform, "m_Goal_Dish");
            if (goalDishTransform != null)
            {
                uInfo.m_Goal_Dish = goalDishTransform.GetComponent<Image>();
            }
        }
        
        // 获取完成菜品显示（Finish_Dish 在根节点下，与 Slider 并列）
        Transform finishDishTransform = GameUtils.FindChildInTransform(spawn_item.transform, "m_Finish_Dish");
        if (finishDishTransform != null)
        {
            uInfo.m_Finish_Dish = finishDishTransform.GetComponent<Image>();
        }

        last_time_dics.Add(from_char, uInfo);   

        spawn_item.SetActive(true);
        spawn_item.transform.SetParent(transform , false);

    }
    
    // Start is called before the first frame update
    void Start()
    {
        // 隐藏模板
        nodeDics["m_CookingUI_Root"].SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        var keysToRemove = new List<CookingPot>();

        var key_list = last_time_dics.Keys;
        foreach (var key in key_list)
        {
            // 如果锅空闲了（RecipeID被清0），删除UI
            if (key.IsIdle())
            {
                Debug.Log($"[UI_cooktime] 锅空闲，删除UI - RecipeID={key.NowRecipeID}");
                keysToRemove.Add(key);
            }
            // 如果锅已完成烹饪
            else if (key.IsFinished())
            {
                // 切换到完成状态显示
                if (!last_time_dics[key].isShowingFinished)
                {
                    Debug.Log($"[UI_cooktime] 锅已完成烹饪，切换到完成状态 - RecipeID={key.NowRecipeID}, Time={key.NowRecipeTime}");
                    SwitchToFinishedState(last_time_dics[key], key);
                }
                
                // 继续跟随位置
                last_time_dics[key].bindTranform.transform.position = Camera.main.WorldToScreenPoint(key.transform.position);
            }
            // 如果锅正在烹饪中
            else if (key.IsCooking())
            {
                // 如果之前显示的是完成状态，需要切换回烹饪状态（这种情况一般不会发生，但为了安全性）
                if (last_time_dics[key].isShowingFinished)
                {
                    SwitchToCookingState(last_time_dics[key]);
                }
                
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
    
    /// <summary>
    /// 切换到完成状态显示
    /// </summary>
    void SwitchToFinishedState(UI_TimeFollowInfo uInfo, CookingPot cookPot)
    {
        uInfo.isShowingFinished = true;
        
        // 隐藏 Slider（子元素 Grid 和 Goal_Dish 会自动隐藏）
        if (uInfo.slider_last_time != null)
            uInfo.slider_last_time.gameObject.SetActive(false);
        
        // 显示完成菜品
        if (uInfo.m_Finish_Dish != null)
        {
            int finishedDishID = 0;
            
            // 根据 RecipeID 获取完成的菜品ID
            if (cookPot.NowRecipeID == -1)
            {
                // 菜谱匹配失败，显示100号废菜
                finishedDishID = 100;
            }
            else if (cookPot.NowRecipeID > 0)
            {
                // 有匹配的菜谱，查找对应的成品菜
                Recipe recipe = GameTableConfig.Instance.Config_Recipe.FindFirstLine(x => x.RecipeID == cookPot.NowRecipeID);
                if (recipe != null)
                {
                    finishedDishID = recipe.CookResult;
                }
            }
            
            if (finishedDishID > 0)
            {
                LoadImageToUI(uInfo.m_Finish_Dish, GetDishImageByID(finishedDishID));
                uInfo.m_Finish_Dish.gameObject.SetActive(true);
                Debug.Log($"显示完成菜品: DishID={finishedDishID}");
            }
        }
    }
    
    /// <summary>
    /// 切换到烹饪状态显示（一般用于状态恢复）
    /// </summary>
    void SwitchToCookingState(UI_TimeFollowInfo uInfo)
    {
        uInfo.isShowingFinished = false;
        
        // 显示 Slider（子元素 Grid 和 Goal_Dish 会自动显示）
        if (uInfo.slider_last_time != null)
            uInfo.slider_last_time.gameObject.SetActive(true);
        
        // 隐藏完成菜品
        if (uInfo.m_Finish_Dish != null)
            uInfo.m_Finish_Dish.gameObject.SetActive(false);
    }
}
