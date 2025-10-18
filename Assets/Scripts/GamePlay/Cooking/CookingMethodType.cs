using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Cooking
{
    /// <summary>
    /// 烹饪方式类型
    /// </summary>
    public enum CookingMethodType
    {
        None = 0,
        Stir = 1,      // 炒 - 铁锅
        Stew = 2,      // 炖 - 砂锅
        Fry = 3,       // 炸 - 炸锅
        Roast = 4,     // 烤 - 烤炉
    }
}

