using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Core;

public class GameField_Explosion : GameFieldBase
{

    public override void OnFieldTick(CharacterCtrlBase target_character)
    {
        base.OnFieldTick(target_character);
    }

    public override void OnFieldStart(CharacterCtrlBase target_character)
    {
        base.OnFieldStart(target_character);
        
        // 玩家不受爆炸影响
        if (LevelManager.Instance != null && target_character == LevelManager.Instance.MyHero)
        {
            return; // 玩家不受爆炸击退影响
        }
        
        // 只对敌人生效
        if (target_character.MyObjectLayer != InGameCharacterType.Enemy)
        {
            return;
        }
        
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
                    "55",
                    "false"
                }
            );
        SkillDispatchCenter.Instance.AddModifierToCharacter(target_character, .5f, 4);
    }
}
