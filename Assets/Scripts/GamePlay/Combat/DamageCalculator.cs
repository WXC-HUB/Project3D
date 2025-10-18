using UnityEngine;

namespace Assets.Scripts.Combat
{
    /// <summary>
    /// 伤害计算器
    /// 根据策划案2.4单位设定的伤害计算公式
    /// </summary>
    public static class DamageCalculator
    {
        /// <summary>
        /// 计算伤害
        /// </summary>
        /// <param name="attack">攻击力</param>
        /// <param name="skillRate">技能倍率</param>
        /// <param name="attackType">攻击类型</param>
        /// <param name="defense">防御力</param>
        /// <param name="magicResist">魔法抗性（0-100，表示百分比）</param>
        /// <returns>最终伤害值</returns>
        public static int CalculateDamage(int attack, float skillRate, AttackType attackType, int defense, int magicResist)
        {
            // 造成的伤害 = 攻击力 × 技能倍率
            int damageDeal = (int)(attack * skillRate);
            int damageTaken;
            
            if (attackType == AttackType.Physical)
            {
                // 物理伤害计算
                if (damageDeal <= defense)
                {
                    damageTaken = 100;  // 最小伤害
                }
                else
                {
                    damageTaken = damageDeal - defense;
                }
            }
            else  // AttackType.Magic
            {
                // 魔法伤害计算
                if (damageDeal <= magicResist)
                {
                    damageTaken = 100;  // 最小伤害
                }
                else
                {
                    // 魔抗按百分比减免
                    damageTaken = (int)(damageDeal * (1 - magicResist / 100f));
                }
            }
            
            return Mathf.Max(1, damageTaken);  // 确保至少造成1点伤害
        }
        
        /// <summary>
        /// 计算伤害（简化版，不考虑防御）
        /// </summary>
        public static int CalculateSimpleDamage(int attack, float skillRate)
        {
            return Mathf.Max(1, (int)(attack * skillRate));
        }
    }
}

