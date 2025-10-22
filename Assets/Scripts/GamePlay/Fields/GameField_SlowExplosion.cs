using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Core;

/// <summary>
/// 减速爆炸场地效果
/// 对进入范围的敌人施加减速Buff
/// </summary>
public class GameField_SlowExplosion : GameFieldBase
{
    public int damageAmount = 0;         // 爆炸伤害（子弹已造成伤害，这里设为0）
    public int slowModifierID = 15001;   // 减速修改器ID
    public float slowDuration = 4f;       // 减速持续时间（4秒）
    public float lifeTime = 3f;           // 场地存在时间
    
    private float currentLifeTime = 0f;

    private void Update()
    {
        currentLifeTime += Time.deltaTime;
        if (currentLifeTime >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    public override void OnFieldTick(CharacterCtrlBase target_character)
    {
        base.OnFieldTick(target_character);
    }

    public override void OnFieldStart(CharacterCtrlBase target_character)
    {
        base.OnFieldStart(target_character);
        
        // 玩家不受减速影响
        if (LevelManager.Instance != null && target_character == LevelManager.Instance.MyHero)
        {
            return;
        }
        
        // 只对敌人生效
        if (target_character.MyObjectLayer == InGameCharacterType.Enemy)
        {
            // 造成伤害（如果有）
            if (damageAmount > 0)
            {
                target_character.TakeDamage(damageAmount);
            }
            
            // 施加减速Buff（会自动刷新持续时间）
            SkillDispatchCenter.Instance.AddModifierToCharacter(
                target_character, 
                slowDuration, 
                slowModifierID
            );
        }
    }
}
