using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Wave
{
    /// <summary>
    /// 怪物生成配置
    /// </summary>
    [System.Serializable]
    public class MonsterSpawnConfig
    {
        public int monsterID;           // 怪物ID（对应CharacterID）
        public int count;               // 生成数量
        public float spawnInterval;     // 生成间隔
        public int spawnRootID;         // 生成路径ID
    }
    
    /// <summary>
    /// 波次数据配置
    /// </summary>
    [CreateAssetMenu(fileName = "WaveData", menuName = "Game/Wave Data")]
    public class WaveData : ScriptableObject
    {
        [Header("波次基础信息")]
        public int waveID;
        public string waveName;
        
        [Header("怪物配置")]
        public List<MonsterSpawnConfig> monsters = new List<MonsterSpawnConfig>();
        
        [Header("时间配置")]
        public float preparationTime = 15f;    // 准备时间（秒）
        public float intervalTime = 5f;        // 波次间隔（秒）
        
        /// <summary>
        /// 获取总怪物数量
        /// </summary>
        public int GetTotalMonsterCount()
        {
            int total = 0;
            foreach (var config in monsters)
            {
                total += config.count;
            }
            return total;
        }
    }
}

