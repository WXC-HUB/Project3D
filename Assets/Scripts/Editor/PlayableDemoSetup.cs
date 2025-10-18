using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// 一键设置可玩Demo场景
/// 添加所有必要的游戏逻辑组件
/// </summary>
public class PlayableDemoSetup : EditorWindow
{
    [MenuItem("游戏Demo/升级为可玩Demo")]
    public static void ShowWindow()
    {
        var window = GetWindow<PlayableDemoSetup>("可玩Demo设置");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }
    
    void OnGUI()
    {
        GUILayout.Label("🎮 将Demo场景升级为可玩版本", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        if (EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("⚠️ 请先停止播放模式", MessageType.Warning);
            if (GUILayout.Button("停止播放 ⏹", GUILayout.Height(30)))
            {
                EditorApplication.isPlaying = false;
            }
            return;
        }
        
        EditorGUILayout.HelpBox(
            "此操作将为当前打开的场景添加：\n\n" +
            "✅ 玩家WASD移动控制\n" +
            "✅ 自动生成敌人系统\n" +
            "✅ 可工作的防御塔\n" +
            "✅ 游戏UI显示\n" +
            "✅ 完整游戏循环",
            MessageType.Info);
        
        GUILayout.Space(10);
        
        var scene = SceneManager.GetActiveScene();
        GUILayout.Label($"当前场景: {scene.name}");
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("🚀 升级为可玩Demo", GUILayout.Height(40)))
        {
            UpgradeToPlayableDemo();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("📖 查看使用说明"))
        {
            Application.OpenURL("file://" + Application.dataPath + "/../WHY_DEMO_IS_STATIC.md");
        }
    }
    
    void UpgradeToPlayableDemo()
    {
        try
        {
            Debug.Log("=== 开始升级Demo为可玩版本 ===");
            
            var scene = SceneManager.GetActiveScene();
            
            // 1. 设置玩家控制
            SetupPlayerController();
            
            // 2. 设置敌人生成器
            SetupEnemySpawner();
            
            // 3. 设置防御塔
            SetupTower();
            
            // 4. 设置UI
            SetupGameUI();
            
            // 5. 确保摄像机正确设置
            SetupCamera();
            
            // 保存场景
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            
            Debug.Log("=== ✓ 可玩Demo升级完成！===");
            EditorUtility.DisplayDialog(
                "升级完成！",
                "可玩Demo已准备就绪！\n\n" +
                "点击播放按钮 ▶️ 开始游戏\n" +
                "使用WASD移动，Shift加速\n" +
                "观察敌人自动生成和防御塔攻击",
                "开始游戏 ▶️");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"升级Demo时发生错误: {e.Message}");
            Debug.LogError(e.StackTrace);
            EditorUtility.DisplayDialog("错误", $"升级失败:\n{e.Message}", "确定");
        }
    }
    
    void SetupPlayerController()
    {
        Debug.Log("1️⃣ 设置玩家控制器...");
        
        // 查找或创建玩家
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }
        if (player == null)
        {
            // 尝试找到任何Capsule
            foreach (var obj in GameObject.FindObjectsOfType<GameObject>())
            {
                if (obj.GetComponent<CapsuleCollider>() != null)
                {
                    player = obj;
                    player.name = "Player";
                    break;
                }
            }
        }
        
        if (player == null)
        {
            Debug.LogWarning("未找到玩家对象，创建新的");
            player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0, 1, 0);
            player.tag = "Player";
        }
        
        // 添加控制器
        if (player.GetComponent<SimplePlayerController>() == null)
        {
            var controller = player.AddComponent<SimplePlayerController>();
            Debug.Log("✓ 添加玩家控制器");
        }
        
        // 确保有Rigidbody
        if (player.GetComponent<Rigidbody>() == null)
        {
            var rb = player.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.drag = 5f;
            Debug.Log("✓ 添加Rigidbody");
        }
        
        Debug.Log($"✓ 玩家设置完成: {player.name}");
    }
    
    void SetupEnemySpawner()
    {
        Debug.Log("2️⃣ 设置敌人生成器...");
        
        // 查找或创建生成器
        GameObject spawner = GameObject.Find("EnemySpawner");
        if (spawner == null)
        {
            spawner = new GameObject("EnemySpawner");
            spawner.transform.position = new Vector3(0, 0, 20);
        }
        
        if (spawner.GetComponent<SimpleEnemySpawner>() == null)
        {
            var spawnerScript = spawner.AddComponent<SimpleEnemySpawner>();
            Debug.Log("✓ 添加敌人生成器");
        }
        
        Debug.Log($"✓ 敌人生成器设置完成: {spawner.name}");
    }
    
    void SetupTower()
    {
        Debug.Log("3️⃣ 设置防御塔...");
        
        // 查找或创建防御塔
        var existingTower = GameObject.Find("Tower");
        if (existingTower == null)
        {
            // 尝试加载Prefab
            var towerPrefab = Resources.Load<GameObject>("CharacterPrefabs/CC_Object_Tower");
            if (towerPrefab != null)
            {
                var tower = PrefabUtility.InstantiatePrefab(towerPrefab) as GameObject;
                tower.name = "Tower";
                tower.transform.position = new Vector3(0, 0, 10);
                Debug.Log("✓ 从Prefab创建防御塔");
            }
            else
            {
                // 创建简单的防御塔
                var tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tower.name = "Tower";
                tower.transform.position = new Vector3(0, 1, 10);
                tower.transform.localScale = new Vector3(2, 3, 2);
                tower.GetComponent<Renderer>().material.color = Color.blue;
                
                // 添加防御塔组件
                if (tower.GetComponent<Assets.Scripts.Tower.TowerController>() == null)
                {
                    tower.AddComponent<Assets.Scripts.Tower.TowerController>();
                }
                
                Debug.Log("✓ 创建简单防御塔");
            }
        }
        else
        {
            Debug.Log($"✓ 使用现有防御塔: {existingTower.name}");
        }
    }
    
    void SetupGameUI()
    {
        Debug.Log("4️⃣ 设置游戏UI...");
        
        // 查找或创建UI管理器
        GameObject uiManager = GameObject.Find("DemoUIManager");
        if (uiManager == null)
        {
            uiManager = new GameObject("DemoUIManager");
        }
        
        if (uiManager.GetComponent<SimpleDemoUI>() == null)
        {
            uiManager.AddComponent<SimpleDemoUI>();
            Debug.Log("✓ 添加Demo UI");
        }
        
        Debug.Log("✓ UI设置完成");
    }
    
    void SetupCamera()
    {
        Debug.Log("5️⃣ 设置摄像机...");
        
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            var camObj = new GameObject("Main Camera");
            mainCam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();
        }
        
        // 设置摄像机位置
        mainCam.transform.position = new Vector3(0, 10, -15);
        mainCam.transform.rotation = Quaternion.Euler(30, 0, 0);
        
        Debug.Log("✓ 摄像机设置完成");
    }
}

