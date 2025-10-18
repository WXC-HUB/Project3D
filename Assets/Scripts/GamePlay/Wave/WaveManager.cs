using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.BaseUtils;
using Assets.Scripts.Core;

namespace Assets.Scripts.Wave
{
    /// <summary>
    /// 波次阶段
    /// </summary>
    public enum WavePhase
    {
        NotStarted,     // 未开始
        Preparation,    // 准备阶段
        InProgress,     // 波次进行中
        Interval,       // 波次间隔
        Completed       // 全部完成
    }
    
    /// <summary>
    /// 波次管理器
    /// 策划案2.3.1.6.1：波次系统
    /// </summary>
    public class WaveManager : MonoSingleton<WaveManager>
    {
        [Header("波次配置")]
        [SerializeField] private List<WaveData> waves = new List<WaveData>();
        [SerializeField] private bool autoStartNextWave = false;
        
        [Header("当前状态")]
        [SerializeField] private int currentWaveIndex = -1;
        [SerializeField] private WavePhase currentPhase = WavePhase.NotStarted;
        [SerializeField] private float phaseTimer = 0f;
        
        private int monstersSpawned = 0;
        private int monstersKilled = 0;
        private int totalMonstersInWave = 0;
        private Coroutine spawnCoroutine;
        
        #region Events
        
        public event EventHandler<WaveEventArgs> OnWaveStarted;
        public event EventHandler<WaveEventArgs> OnWaveCompleted;
        public event EventHandler<WaveEventArgs> OnAllWavesCompleted;
        public event EventHandler<PhaseEventArgs> OnPhaseChanged;
        public event EventHandler<MonsterEventArgs> OnMonsterSpawned;
        public event EventHandler<MonsterEventArgs> OnMonsterKilled;
        
        public class WaveEventArgs : EventArgs
        {
            public int waveIndex;
            public WaveData waveData;
        }
        
        public class PhaseEventArgs : EventArgs
        {
            public WavePhase phase;
            public float duration;
        }
        
        public class MonsterEventArgs : EventArgs
        {
            public CharacterCtrlBase monster;
            public int spawnedCount;
            public int totalCount;
        }
        
        #endregion
        
        #region Properties
        
        public int CurrentWaveIndex => currentWaveIndex;
        public int TotalWaves => waves.Count;
        public WavePhase CurrentPhase => currentPhase;
        public float PhaseTimer => phaseTimer;
        public int MonstersSpawned => monstersSpawned;
        public int MonstersKilled => monstersKilled;
        public int MonstersRemaining => monstersSpawned - monstersKilled;
        public bool IsInProgress => currentPhase == WavePhase.InProgress || currentPhase == WavePhase.Interval;
        public bool IsCompleted => currentPhase == WavePhase.Completed;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Update()
        {
            if (currentPhase == WavePhase.NotStarted || currentPhase == WavePhase.Completed)
                return;
            
            UpdatePhase();
        }
        
        #endregion
        
        #region Wave Control
        
        /// <summary>
        /// 开始波次系统
        /// </summary>
        public void StartWaves()
        {
            if (waves.Count == 0)
            {
                Debug.LogError("No waves configured!");
                return;
            }
            
            currentWaveIndex = -1;
            currentPhase = WavePhase.NotStarted;
            
            StartNextWave();
        }
        
        /// <summary>
        /// 开始下一波
        /// </summary>
        public void StartNextWave()
        {
            currentWaveIndex++;
            
            if (currentWaveIndex >= waves.Count)
            {
                // 所有波次完成
                CompleteAllWaves();
                return;
            }
            
            WaveData currentWave = waves[currentWaveIndex];
            totalMonstersInWave = currentWave.GetTotalMonsterCount();
            monstersSpawned = 0;
            monstersKilled = 0;
            
            // 进入准备阶段
            EnterPhase(WavePhase.Preparation, currentWave.preparationTime);
            
            OnWaveStarted?.Invoke(this, new WaveEventArgs
            {
                waveIndex = currentWaveIndex,
                waveData = currentWave
            });
            
            Debug.Log($"Wave {currentWaveIndex + 1}/{waves.Count} started. Monsters: {totalMonstersInWave}");
        }
        
        /// <summary>
        /// 跳过准备阶段
        /// </summary>
        public void SkipPreparation()
        {
            if (currentPhase == WavePhase.Preparation)
            {
                phaseTimer = 0f;
            }
        }
        
        #endregion
        
        #region Phase Management
        
        private void UpdatePhase()
        {
            phaseTimer -= Time.deltaTime;
            
            if (phaseTimer <= 0)
            {
                OnPhaseTimerExpired();
            }
        }
        
        private void OnPhaseTimerExpired()
        {
            switch (currentPhase)
            {
                case WavePhase.Preparation:
                    // 准备阶段结束，开始生成怪物
                    StartSpawning();
                    break;
                    
                case WavePhase.Interval:
                    // 间隔结束，开始下一波或完成
                    if (autoStartNextWave)
                    {
                        StartNextWave();
                    }
                    break;
            }
        }
        
        private void EnterPhase(WavePhase phase, float duration)
        {
            currentPhase = phase;
            phaseTimer = duration;
            
            OnPhaseChanged?.Invoke(this, new PhaseEventArgs
            {
                phase = phase,
                duration = duration
            });
            
            Debug.Log($"Entered phase: {phase} ({duration}s)");
        }
        
        #endregion
        
        #region Monster Spawning
        
        private void StartSpawning()
        {
            if (currentWaveIndex < 0 || currentWaveIndex >= waves.Count)
                return;
            
            EnterPhase(WavePhase.InProgress, float.MaxValue);
            
            WaveData currentWave = waves[currentWaveIndex];
            
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
            }
            
            spawnCoroutine = StartCoroutine(SpawnMonstersCoroutine(currentWave));
        }
        
        private IEnumerator SpawnMonstersCoroutine(WaveData waveData)
        {
            foreach (var monsterConfig in waveData.monsters)
            {
                for (int i = 0; i < monsterConfig.count; i++)
                {
                    SpawnMonster(monsterConfig);
                    yield return new WaitForSeconds(monsterConfig.spawnInterval);
                }
            }
            
            Debug.Log($"All monsters spawned for wave {currentWaveIndex + 1}");
        }
        
        private void SpawnMonster(MonsterSpawnConfig config)
        {
            // 通过LevelManager生成怪物
            PlayerCharacterCtrl monster = LevelManager.Instance.SpawnCharacterByID<PlayerCharacterCtrl>(config.monsterID);
            
            if (monster != null)
            {
                monstersSpawned++;
                
                // 设置生成点和路径
                var spawnRoot = LevelGridGenerator.Instance.spawnroot_dictionay.TryGetValue(config.spawnRootID, out var root);
                if (spawnRoot && root != null)
                {
                    monster.transform.position = LevelGridGenerator.Instance.tilemap.GetCellCenterWorld(root.start_point);
                    
                    var aiAgent = monster.GetComponent<Assets.Scripts.AI.AIAgentBase>();
                    if (aiAgent != null)
                    {
                        aiAgent.SetFollowPath(root);
                    }
                }
                
                OnMonsterSpawned?.Invoke(this, new MonsterEventArgs
                {
                    monster = monster,
                    spawnedCount = monstersSpawned,
                    totalCount = totalMonstersInWave
                });
                
                Debug.Log($"Monster spawned: {monstersSpawned}/{totalMonstersInWave}");
            }
        }
        
        #endregion
        
        #region Monster Tracking
        
        /// <summary>
        /// 通知怪物被击杀
        /// </summary>
        public void NotifyMonsterKilled(CharacterCtrlBase monster)
        {
            if (currentPhase != WavePhase.InProgress)
                return;
            
            monstersKilled++;
            
            OnMonsterKilled?.Invoke(this, new MonsterEventArgs
            {
                monster = monster,
                spawnedCount = monstersKilled,
                totalCount = totalMonstersInWave
            });
            
            Debug.Log($"Monster killed: {monstersKilled}/{totalMonstersInWave}");
            
            // 检查是否所有怪物都被击杀
            if (monstersKilled >= totalMonstersInWave && monstersSpawned >= totalMonstersInWave)
            {
                CompleteCurrentWave();
            }
        }
        
        #endregion
        
        #region Wave Completion
        
        private void CompleteCurrentWave()
        {
            if (currentWaveIndex < 0 || currentWaveIndex >= waves.Count)
                return;
            
            WaveData currentWave = waves[currentWaveIndex];
            
            OnWaveCompleted?.Invoke(this, new WaveEventArgs
            {
                waveIndex = currentWaveIndex,
                waveData = currentWave
            });
            
            Debug.Log($"Wave {currentWaveIndex + 1} completed!");
            
            // 进入间隔阶段
            if (currentWaveIndex < waves.Count - 1)
            {
                EnterPhase(WavePhase.Interval, currentWave.intervalTime);
            }
            else
            {
                CompleteAllWaves();
            }
        }
        
        private void CompleteAllWaves()
        {
            EnterPhase(WavePhase.Completed, 0f);
            
            OnAllWavesCompleted?.Invoke(this, new WaveEventArgs
            {
                waveIndex = currentWaveIndex,
                waveData = null
            });
            
            Debug.Log("=== ALL WAVES COMPLETED ===");
        }
        
        #endregion
        
        #region Utility
        
        public WaveData GetCurrentWave()
        {
            if (currentWaveIndex >= 0 && currentWaveIndex < waves.Count)
            {
                return waves[currentWaveIndex];
            }
            return null;
        }
        
        public void ResetWaves()
        {
            currentWaveIndex = -1;
            currentPhase = WavePhase.NotStarted;
            monstersSpawned = 0;
            monstersKilled = 0;
            
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }
        
        #endregion
    }
}

