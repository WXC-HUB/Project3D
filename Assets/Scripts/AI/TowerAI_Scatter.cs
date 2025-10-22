using Assets.Scripts.Core;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 散射塔组件 - 拦截攻击行为并发射多颗子弹
/// 配合CharacterModifier的ShootToTarget事件使用
/// </summary>
public class TowerAI_Scatter : MonoBehaviour
{
    [Header("散射参数")]
    [Tooltip("同时发射的子弹数量")]
    public int bulletCount = 3;
    
    [Tooltip("散射角度（度）")]
    public float spreadAngle = 15f;
    
    [Tooltip("散射子弹的ObjectID")]
    public int scatterBulletID = 20;

    private CharacterCtrlBase ownerCharacter;

    private void Start()
    {
        ownerCharacter = GetComponent<CharacterCtrlBase>();
        
        if (ownerCharacter == null)
        {
            Debug.LogError("TowerAI_Scatter: 找不到CharacterCtrlBase组件！");
        }
    }

    /// <summary>
    /// 公共方法：向多个不同的敌人发射子弹
    /// 可以被修改器系统或其他脚本调用
    /// </summary>
    public void ShootScatterBulletsToTarget(CharacterCtrlBase primaryTarget , SkillUseInfo skil )
    {
        if (primaryTarget == null || ownerCharacter == null)
        {
            Debug.LogWarning("TowerAI_Scatter: 目标或所有者为空，无法发射");
            return;
        }

        // 查找附近的多个敌人
        List<CharacterCtrlBase> targets = FindNearbyEnemies(primaryTarget, bulletCount);
        
        if (targets.Count == 0)
        {
            Debug.LogWarning("TowerAI_Scatter: 找不到可攻击的敌人");
            return;
        }

        // 为每个目标发射一颗子弹
        for (int i = 0; i < targets.Count; i++)
        {
            CharacterCtrlBase target = targets[i];
            Vector2 direction = (target.transform.position - transform.position).normalized;

            // 生成子弹
            CharacterCtrlBase bullet = LevelManager.Instance.SpawnCharacterByID<CharacterCtrlBase>(scatterBulletID);
            
            if (bullet != null)
            {
                bullet.transform.position = transform.position;
                bullet.followTarget = target;
                bullet.from_char = ownerCharacter;

                // 创建技能信息
                SkillUseInfo skillInfo = skil;
                bullet.fromSkillInfo = skillInfo;
                
                Debug.Log($"散射塔发射子弹 {i + 1}/{targets.Count}，目标: {target.name}");
            }
            else
            {
                Debug.LogWarning($"TowerAI_Scatter: 无法生成子弹 (ObjectID: {scatterBulletID})，请检查配置表和预制体");
            }
        }
    }
    
    /// <summary>
    /// 查找附近的多个敌人（优先距离近的）
    /// </summary>
    private List<CharacterCtrlBase> FindNearbyEnemies(CharacterCtrlBase primaryTarget, int maxCount)
    {
        List<CharacterCtrlBase> result = new List<CharacterCtrlBase>();
        
        // 首先添加主目标
        result.Add(primaryTarget);
        
        // 查找其他敌人
        if (LevelManager.Instance.Character_Dict.ContainsKey(InGameCharacterType.Enemy))
        {
            List<CharacterCtrlBase> allEnemies = LevelManager.Instance.Character_Dict[InGameCharacterType.Enemy];
            
            // 按距离排序
            List<CharacterCtrlBase> sortedEnemies = new List<CharacterCtrlBase>(allEnemies);
            sortedEnemies.Remove(primaryTarget); // 移除主目标，避免重复
            sortedEnemies.RemoveAll(e => e == null || e.gameObject == null || e.NowHP <= 0); // 移除无效或死亡的敌人
            
            sortedEnemies.Sort((a, b) =>
            {
                float distA = Vector3.Distance(transform.position, a.transform.position);
                float distB = Vector3.Distance(transform.position, b.transform.position);
                return distA.CompareTo(distB);
            });
            
            // 添加最近的敌人，直到达到最大数量
            for (int i = 0; i < sortedEnemies.Count && result.Count < maxCount; i++)
            {
                result.Add(sortedEnemies[i]);
            }
        }
        
        return result;
    }
}

