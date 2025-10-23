using Assets.Scripts.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 关卡网格物体基类，包含通用逻辑
/// </summary>
public class LevelGridTileObject : CharacterCtrlBase
{
    private void Awake()
    {
        base.Awake();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        var outline = this.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
        }
    }

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
    /// 尝试附加物品到此网格对象上
    /// </summary>
    public override bool TryAttachObject(CharacterCtrlBase attach_obj)
    {
        bool is_dish = !(null == GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.GameCharacter == attach_obj.MyGameObjectID));
        
        // 给防御塔添加菜品Buff的逻辑
        if(is_dish && this.nowAttachList.Any(x=>x.MyObjectLayer is InGameCharacterType.Tower))
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
        // 默认附加逻辑
        else
        {
            attach_obj.transform.SetParent(transform, true);
            attach_obj.transform.position = transform.position + new Vector3(0, 0,-0.5f);
            nowAttachList.Add(attach_obj);
            attach_obj.isAttachedToOther = true;
            return true;
        }
    }
}
