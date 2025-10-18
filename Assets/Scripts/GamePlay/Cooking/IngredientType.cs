using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Cooking
{
    /// <summary>
    /// 食材类型枚举
    /// </summary>
    public enum IngredientType
    {
        None = 0,
        
        // 基础食材
        Tomato = 1,         // 番茄
        Fish = 2,           // 鱼
        Mushroom = 3,       // 香菇
        Potato = 4,         // 土豆
        Beef = 5,           // 牛肉
        Milk = 6,           // 牛奶
        Onion = 7,          // 洋葱
        Chicken = 8,        // 鸡肉
        Egg = 9,            // 鸡蛋
        Pepper = 10,        // 辣椒
        Radish = 11,        // 萝卜
        Octopus = 12,       // 章鱼
        Pork = 13,          // 猪肉
        
        // 特殊调料
        PickledPepper = 101,  // 泡辣椒
        PickledRadish = 102,  // 泡萝卜
        Curry = 103,          // 咖喱块
        SichuanPepper = 104,  // 青花椒
        Coriander = 105,      // 香菜
        Ginger = 106,         // 生姜
        GreenOnion = 107,     // 大葱
    }
}

