using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Core;

/// <summary>
/// 减速爆炸场地效果
/// 对进入范围的敌人造成伤害并施加减速Buff
/// </summary>
public class GameField_SlowExplosion : GameFieldBase
{
    public int damageAmount = 10;        // 爆炸伤害
    public int slowModifierID = 15001;   // 减速修改器ID
    public float slowDuration = 3f;       // 减速持续时间

    public override void OnFieldTick(CharacterCtrlBase target_character)
    {
        base.OnFieldTick(target_character);
    }

    public override void OnFieldStart(CharacterCtrlBase target_character)
    {
        base.OnFieldStart(target_character);
        
        // 只对敌人生效
        if (target_character.MyObjectLayer == InGameCharacterType.Enemy)
        {
            // 造成伤害
            target_character.TakeDamage(damageAmount);
            
            // 施加减速Buff
            SkillDispatchCenter.Instance.AddModifierToCharacter(
                target_character, 
                slowDuration, 
                slowModifierID
            );
            
            // 施加击退力（可选）
            SkillDispatchCenter.Instance.DoGameAction(
                actionType: "AddForce",
                skill_useinfo: null,
                from_character: null,
                to_character: target_character,
                action_params: new List<string>
                {
                    "null",
                    (target_character.transform.position.x - this.transform.position.x).ToString(),
                    (target_character.transform.position.y - this.transform.position.y).ToString(),
                    "30",  // 击退力度
                    "false"
                }
            );
        }
    }
}

