/// <summary>
/// 玩家与网格对象的互动类型
/// </summary>
public enum InteractionType
{
    /// <summary>
    /// 无互动
    /// </summary>
    None = 0,
    
    /// <summary>
    /// 放置物品（通用）
    /// </summary>
    Place = 1,
    
    /// <summary>
    /// 拾取物品（通用）
    /// </summary>
    PickUp = 2,
    
    /// <summary>
    /// 向烹饪锅添加食材
    /// </summary>
    AddIngredient = 10,
    
    /// <summary>
    /// 从烹饪锅拾取成品菜
    /// </summary>
    PickUpDish = 11,
    
    /// <summary>
    /// 向出餐口提交菜品
    /// </summary>
    SubmitDish = 20,
    
    /// <summary>
    /// 给防御塔添加Buff
    /// </summary>
    AddBuffToTower = 30,
}

