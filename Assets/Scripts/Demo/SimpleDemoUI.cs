using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 简单的Demo UI显示
/// 显示游戏状态、提示信息等
/// </summary>
public class SimpleDemoUI : MonoBehaviour
{
    [Header("UI元素")]
    [SerializeField] private Text infoText;
    [SerializeField] private Text controlsText;
    [SerializeField] private Text statusText;
    
    [Header("更新频率")]
    [SerializeField] private float updateInterval = 0.5f;
    
    private float nextUpdateTime = 0f;
    private Assets.Scripts.Level.LevelHealthSystem levelHealth;
    private Assets.Scripts.Wave.WaveManager waveManager;
    
    void Start()
    {
        // 自动创建UI如果不存在
        if (infoText == null || controlsText == null || statusText == null)
        {
            CreateUI();
        }
        
        // 查找系统
        levelHealth = FindObjectOfType<Assets.Scripts.Level.LevelHealthSystem>();
        waveManager = FindObjectOfType<Assets.Scripts.Wave.WaveManager>();
        
        // 显示控制说明
        if (controlsText != null)
        {
            controlsText.text = "=== 控制说明 ===\n" +
                              "WASD / 方向键: 移动\n" +
                              "Shift: 加速奔跑\n" +
                              "ESC: 退出播放模式";
        }
        
        Debug.Log("✓ Demo UI已初始化");
    }
    
    void Update()
    {
        if (Time.time >= nextUpdateTime)
        {
            UpdateUI();
            nextUpdateTime = Time.time + updateInterval;
        }
        
        // ESC退出播放模式
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
    
    void UpdateUI()
    {
        if (statusText == null) return;
        
        string status = "=== 游戏状态 ===\n\n";
        
        // 关卡生命
        if (levelHealth != null)
        {
            status += $"❤️ 关卡生命: {levelHealth.CurrentHealth}/{levelHealth.MaxHealth}\n";
            status += levelHealth.IsGameOver ? "💀 游戏结束\n" : "";
        }
        else
        {
            status += "❤️ 关卡生命: -/-\n";
        }
        
        // 波次信息
        if (waveManager != null)
        {
            status += $"🌊 当前波次: {waveManager.CurrentWaveIndex + 1}\n";
            status += waveManager.IsInProgress ? "⚔️ 战斗中\n" : "⏸️ 准备中\n";
        }
        else
        {
            status += "🌊 波次: -\n";
        }
        
        // 敌人数量
        int enemyCount = 0;
        
        // 优先通过SimpleEnemyMover查找（Demo专用）
        var enemyMovers = FindObjectsOfType<SimpleEnemyMover>();
        if (enemyMovers != null && enemyMovers.Length > 0)
        {
            enemyCount = enemyMovers.Length;
        }
        else
        {
            // 尝试通过Tag查找（需要先定义Tag）
            try
            {
                var enemies = GameObject.FindGameObjectsWithTag("Enemy");
                if (enemies != null)
                {
                    enemyCount = enemies.Length;
                }
            }
            catch (UnityException)
            {
                // Tag未定义，通过名字查找
                var allObjects = FindObjectsOfType<GameObject>();
                foreach (var obj in allObjects)
                {
                    if (obj.name.StartsWith("Enemy_"))
                    {
                        enemyCount++;
                    }
                }
            }
        }
        status += $"👾 敌人数量: {enemyCount}\n";
        
        // 防御塔数量
        var towers = FindObjectsOfType<Assets.Scripts.Tower.TowerController>();
        status += $"🗼 防御塔: {(towers != null ? towers.Length : 0)}\n";
        
        // FPS
        status += $"\n⚡ FPS: {(int)(1f / Time.deltaTime)}\n";
        
        statusText.text = status;
    }
    
    void CreateUI()
    {
        // 查找或创建Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var canvasObj = new GameObject("DemoCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // 创建控制说明文本（左上角）
        if (controlsText == null)
        {
            var controlsObj = new GameObject("ControlsText");
            controlsObj.transform.SetParent(canvas.transform);
            
            var rt = controlsObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(10, -10);
            rt.sizeDelta = new Vector2(300, 150);
            
            controlsText = controlsObj.AddComponent<Text>();
            controlsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (controlsText.font == null)
            {
                controlsText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            controlsText.fontSize = 14;
            controlsText.color = Color.white;
            controlsText.alignment = TextAnchor.UpperLeft;
        }
        
        // 创建状态文本（右上角）
        if (statusText == null)
        {
            var statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(canvas.transform);
            
            var rt = statusObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.anchoredPosition = new Vector2(-10, -10);
            rt.sizeDelta = new Vector2(300, 250);
            
            statusText = statusObj.AddComponent<Text>();
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (statusText.font == null)
            {
                statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            statusText.fontSize = 14;
            statusText.color = Color.white;
            statusText.alignment = TextAnchor.UpperRight;
        }
    }
}


