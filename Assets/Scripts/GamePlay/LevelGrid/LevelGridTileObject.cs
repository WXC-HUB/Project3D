using Assets.Scripts.Core;
using UnityEngine;

/// <summary>
/// 关卡网格物体基类，提供最基础的功能
/// </summary>
public class LevelGridTileObject : CharacterCtrlBase
{
    private void Awake()
    {
        base.Awake();
    }
    
    void Start()
    {
        var outline = this.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
        }
    }

    /// <summary>
    /// 设置选中状态（显示/隐藏轮廓）
    /// </summary>
    public void SetSelect(bool isSelect)
    {
        var outline = this.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = isSelect;
        }
    }
    
    /// <summary>
    /// 判断是否是成品菜（某个菜谱的产出结果或废菜）
    /// </summary>
    protected bool IsFinishedDish(int dishId)
    {
        // 1. 检查是否是100号废菜
        if (dishId == 100)
        {
            return true;
        }
        
        // 2. 检查这个dishId是否是任何Recipe的CookResult
        Recipe recipe = GameTableConfig.Instance.Config_Recipe.FindFirstLine(x => x.CookResult == dishId);
        return recipe != null;
    }
    
    /// <summary>
    /// 判断玩家可以与此网格对象进行何种互动
    /// 子类可以重写此方法来实现自定义的交互逻辑
    /// </summary>
    /// <param name="player">玩家角色控制器</param>
    /// <returns>互动类型，如果返回 InteractionType.None 表示不可互动</returns>
    public virtual InteractionType CanInteractWithPlayer(PlayerCharacterCtrl player)
    {
        // 默认实现：
        // 1. 如果玩家手上有东西，可以互动（放置物品）
        if (player.nowAttachList.Count > 0)
        {
            return InteractionType.Place;
        }
        
        // 2. 如果这个网格对象上有物品，玩家空手可以互动（拾取物品）
        if (nowAttachList.Count > 0)
        {
            return InteractionType.PickUp;
        }
        
        // 其他情况不可互动
        return InteractionType.None;
    }
    
    /// <summary>
    /// 尝试附加物品到此网格对象上（默认实现）
    /// 子类可以重写此方法来实现自定义逻辑
    /// </summary>
    public override bool TryAttachObject(CharacterCtrlBase attach_obj)
    {
        // 默认附加逻辑：简单地将物体放到这个位置上
        attach_obj.transform.SetParent(transform, true);
        attach_obj.transform.position = transform.position + new Vector3(0, 0, -0.5f);
        nowAttachList.Add(attach_obj);
        attach_obj.isAttachedToOther = true;
        return true;
    }
}
