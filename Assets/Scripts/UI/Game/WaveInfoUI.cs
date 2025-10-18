using System;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Wave;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// 波次信息UI
    /// </summary>
    public class WaveInfoUI : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private Text waveNumberText;
        [SerializeField] private Text monsterCountText;
        [SerializeField] private Text phaseText;
        [SerializeField] private Slider phaseTimerSlider;
        [SerializeField] private GameObject preparationPanel;
        [SerializeField] private GameObject intervalPanel;
        
        private WaveManager waveManager;
        private float currentPhaseDuration;
        
        private void Start()
        {
            waveManager = WaveManager.Instance;
            
            if (waveManager != null)
            {
                waveManager.OnWaveStarted += OnWaveStarted;
                waveManager.OnWaveCompleted += OnWaveCompleted;
                waveManager.OnPhaseChanged += OnPhaseChanged;
                waveManager.OnMonsterSpawned += OnMonsterChanged;
                waveManager.OnMonsterKilled += OnMonsterChanged;
                waveManager.OnAllWavesCompleted += OnAllWavesCompleted;
            }
            
            if (preparationPanel != null) preparationPanel.SetActive(false);
            if (intervalPanel != null) intervalPanel.SetActive(false);
        }
        
        private void Update()
        {
            if (waveManager != null && phaseTimerSlider != null)
            {
                if (currentPhaseDuration > 0)
                {
                    float progress = 1f - (waveManager.PhaseTimer / currentPhaseDuration);
                    phaseTimerSlider.value = progress;
                }
            }
        }
        
        private void OnWaveStarted(object sender, WaveManager.WaveEventArgs e)
        {
            if (waveNumberText != null)
            {
                waveNumberText.text = $"波次 {e.waveIndex + 1}/{waveManager.TotalWaves}";
            }
            
            UpdateMonsterCount();
        }
        
        private void OnWaveCompleted(object sender, WaveManager.WaveEventArgs e)
        {
            Debug.Log($"Wave {e.waveIndex + 1} completed!");
        }
        
        private void OnPhaseChanged(object sender, WaveManager.PhaseEventArgs e)
        {
            currentPhaseDuration = e.duration;
            
            if (phaseText != null)
            {
                phaseText.text = GetPhaseText(e.phase);
            }
            
            // 显示/隐藏对应的面板
            if (preparationPanel != null)
            {
                preparationPanel.SetActive(e.phase == WavePhase.Preparation);
            }
            
            if (intervalPanel != null)
            {
                intervalPanel.SetActive(e.phase == WavePhase.Interval);
            }
            
            if (phaseTimerSlider != null)
            {
                phaseTimerSlider.gameObject.SetActive(
                    e.phase == WavePhase.Preparation || e.phase == WavePhase.Interval
                );
            }
        }
        
        private void OnMonsterChanged(object sender, WaveManager.MonsterEventArgs e)
        {
            UpdateMonsterCount();
        }
        
        private void OnAllWavesCompleted(object sender, WaveManager.WaveEventArgs e)
        {
            if (phaseText != null)
            {
                phaseText.text = "全部波次完成！";
            }
        }
        
        private void UpdateMonsterCount()
        {
            if (monsterCountText != null && waveManager != null)
            {
                monsterCountText.text = $"怪物: {waveManager.MonstersKilled}/{waveManager.MonstersSpawned}";
            }
        }
        
        private string GetPhaseText(WavePhase phase)
        {
            return phase switch
            {
                WavePhase.Preparation => "准备阶段",
                WavePhase.InProgress => "战斗中",
                WavePhase.Interval => "波次间隔",
                WavePhase.Completed => "完成",
                _ => ""
            };
        }
        
        private void OnDestroy()
        {
            if (waveManager != null)
            {
                waveManager.OnWaveStarted -= OnWaveStarted;
                waveManager.OnWaveCompleted -= OnWaveCompleted;
                waveManager.OnPhaseChanged -= OnPhaseChanged;
                waveManager.OnMonsterSpawned -= OnMonsterChanged;
                waveManager.OnMonsterKilled -= OnMonsterChanged;
                waveManager.OnAllWavesCompleted -= OnAllWavesCompleted;
            }
        }
    }
}

