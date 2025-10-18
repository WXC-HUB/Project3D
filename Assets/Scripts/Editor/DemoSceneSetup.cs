using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using Assets.Scripts.Cooking;
using Assets.Scripts.Wave;
using Assets.Scripts.Level;

/// <summary>
/// Demo场景自动设置工具
/// 使用方法: Unity菜单 → Tools → Setup Demo Scene
/// </summary>
public class DemoSceneSetup : EditorWindow
{
    [MenuItem("Tools/Setup Demo Scene")]
    public static void ShowWindow()
    {
        GetWindow<DemoSceneSetup>("Demo场景设置");
    }

    private void OnGUI()
    {
        GUILayout.Label("=== Demo场景一键设置 ===", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        // 检查是否在播放模式
        if (EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("请先停止播放模式，然后再使用此工具！\n\n点击Unity顶部的停止按钮 ⏹", MessageType.Warning);
            GUILayout.Space(10);
            
            if (GUILayout.Button("停止播放模式", GUILayout.Height(40)))
            {
                EditorApplication.isPlaying = false;
            }
            return;
        }
        
        GUILayout.Label("此工具将自动创建：");
        GUILayout.Label("✓ ScriptableObject数据资源");
        GUILayout.Label("✓ 必要的GameObject和组件");
        GUILayout.Label("✓ 简单的测试配置");
        GUILayout.Space(10);
        
        if (GUILayout.Button("创建Demo场景", GUILayout.Height(40)))
        {
            SetupDemoScene();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("创建ScriptableObjects", GUILayout.Height(30)))
        {
            CreateScriptableObjects(silent: false);
        }
        
        if (GUILayout.Button("设置当前场景", GUILayout.Height(30)))
        {
            SetupCurrentScene();
        }
    }

    private static void SetupDemoScene()
    {
        // 双重检查：确保不在播放模式
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("错误", "无法在播放模式下创建场景！\n\n请先停止播放模式。", "确定");
            return;
        }
        
        if (!EditorUtility.DisplayDialog("创建Demo场景", 
            "是否创建一个新的Demo场景?\n\n这将:\n- 创建所有ScriptableObject数据\n- 设置场景对象\n- 配置基础游戏功能", 
            "确定", "取消"))
        {
            return;
        }

        try
        {
            Debug.Log("========================================");
            Debug.Log("开始创建Demo场景...");
            Debug.Log("========================================");
            
            // 1. 创建ScriptableObjects (静默模式)
            Debug.Log("步骤1/4: 创建ScriptableObject数据...");
            CreateScriptableObjects(silent: true);
            
            // 2. 创建或打开场景
            Debug.Log("步骤2/4: 创建新场景...");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Debug.Log($"✓ 场景已创建: {scene.name}");
            
            // 3. 设置场景
            Debug.Log("步骤3/4: 设置场景对象...");
            SetupCurrentScene();
            
            // 4. 保存场景
            Debug.Log("步骤4/4: 保存场景...");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/DemoScene.unity");
            Debug.Log("✓ 场景已保存到: Assets/Scenes/DemoScene.unity");
            
            Debug.Log("========================================");
            Debug.Log("Demo场景创建完成!");
            Debug.Log("========================================");
            
            EditorUtility.DisplayDialog("完成", "Demo场景创建完成!\n\n场景已保存到: Assets/Scenes/DemoScene.unity\n\n现在可以点击播放按钮测试游戏!", "确定");
        }
        catch (System.Exception e)
        {
            Debug.LogError("创建Demo场景时发生错误:");
            Debug.LogError(e.Message);
            Debug.LogError(e.StackTrace);
            EditorUtility.DisplayDialog("错误", $"创建Demo场景时发生错误:\n\n{e.Message}\n\n请查看Console窗口了解详情", "确定");
        }
    }

    private static void CreateScriptableObjects(bool silent = false)
    {
        Debug.Log("=== 开始创建ScriptableObject数据 ===");
        
        // 确保目录存在
        CreateDirectoryIfNotExists("Assets/Resources/Data");
        CreateDirectoryIfNotExists("Assets/Resources/Data/Ingredients");
        CreateDirectoryIfNotExists("Assets/Resources/Data/Recipes");
        CreateDirectoryIfNotExists("Assets/Resources/Data/Waves");
        CreateDirectoryIfNotExists("Assets/Resources/Data/Drops");
        
        // 创建食材
        CreateIngredientData();
        
        // 创建配方
        CreateRecipeData();
        
        // 创建订单
        CreateOrderData();
        
        // 创建波次
        CreateWaveData();
        
        // 创建掉落表
        CreateDropTable();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("✓ ScriptableObject数据创建完成!");
        
        // 只在非静默模式显示对话框
        if (!silent)
        {
            EditorUtility.DisplayDialog("完成", "ScriptableObject数据已创建!\n\n位置: Assets/Resources/Data/", "确定");
        }
    }

    private static void CreateIngredientData()
    {
        // 创建番茄
        var tomato = CreateAsset<IngredientData>("Assets/Resources/Data/Ingredients/Tomato.asset");
        tomato.ingredientName = "番茄";
        tomato.ingredientType = IngredientType.Tomato;
        tomato.description = "新鲜的番茄";
        tomato.canBeCooked = true;
        EditorUtility.SetDirty(tomato);
        
        // 创建土豆
        var potato = CreateAsset<IngredientData>("Assets/Resources/Data/Ingredients/Potato.asset");
        potato.ingredientName = "土豆";
        potato.ingredientType = IngredientType.Potato;
        potato.description = "普通的土豆";
        potato.canBeCooked = true;
        EditorUtility.SetDirty(potato);
        
        Debug.Log("✓ 食材数据已创建");
    }

    private static void CreateRecipeData()
    {
        var tomato = AssetDatabase.LoadAssetAtPath<IngredientData>("Assets/Resources/Data/Ingredients/Tomato.asset");
        
        if (tomato != null)
        {
            var recipe = CreateAsset<RecipeData>("Assets/Resources/Data/Recipes/TomatoStew.asset");
            recipe.recipeID = 1;
            recipe.recipeName = "番茄炖菜";
            recipe.starLevel = 1;
            recipe.description = "简单的番茄炖菜";
            recipe.energyReward = 100;
            recipe.attackBonus = 1.2f;
            
            // 添加烹饪步骤
            recipe.steps = new System.Collections.Generic.List<RecipeStep>();
            recipe.steps.Add(new RecipeStep
            {
                ingredient = IngredientType.Tomato,
                cookingMethod = CookingMethodType.Stew,
                cookingTime = 5f
            });
            
            EditorUtility.SetDirty(recipe);
            
            Debug.Log("✓ 配方数据已创建");
        }
    }

    private static void CreateOrderData()
    {
        // OrderData 是运行时类，不是ScriptableObject，由OrderManager在运行时创建
        // 这里我们只需要确保Recipe数据存在即可
        var recipe = AssetDatabase.LoadAssetAtPath<RecipeData>("Assets/Resources/Data/Recipes/TomatoStew.asset");
        
        if (recipe != null)
        {
            Debug.Log("✓ 配方数据已验证（订单将在运行时由OrderManager创建）");
        }
        else
        {
            Debug.LogWarning("⚠ 未找到配方数据，请先创建配方");
        }
    }

    private static void CreateWaveData()
    {
        var wave = CreateAsset<WaveData>("Assets/Resources/Data/Waves/Wave1.asset");
        wave.waveID = 1;
        wave.waveName = "第1波";
        wave.preparationTime = 15f;
        wave.intervalTime = 5f;
        wave.monsters = new System.Collections.Generic.List<MonsterSpawnConfig>();
        // 注意: 敌人类型需要在Unity中手动配置，因为需要引用实际的Prefab
        EditorUtility.SetDirty(wave);
        
        Debug.Log("✓ 波次数据已创建（需要在Inspector中配置敌人）");
    }

    private static void CreateDropTable()
    {
        var tomato = AssetDatabase.LoadAssetAtPath<IngredientData>("Assets/Resources/Data/Ingredients/Tomato.asset");
        var potato = AssetDatabase.LoadAssetAtPath<IngredientData>("Assets/Resources/Data/Ingredients/Potato.asset");
        
        if (tomato != null && potato != null)
        {
            var dropTable = CreateAsset<IngredientDropTable>("Assets/Resources/Data/Drops/BasicEnemyDrop.asset");
            dropTable.rareDropChance = 0.1f;
            
            // 配置普通掉落
            dropTable.normalDrops = new System.Collections.Generic.List<DropEntry>();
            dropTable.normalDrops.Add(new DropEntry
            {
                ingredientData = tomato,
                dropChance = 0.8f,
                minCount = 1,
                maxCount = 2
            });
            dropTable.normalDrops.Add(new DropEntry
            {
                ingredientData = potato,
                dropChance = 0.6f,
                minCount = 1,
                maxCount = 1
            });
            
            // 配置稀有掉落
            dropTable.rareDrops = new System.Collections.Generic.List<DropEntry>();
            
            EditorUtility.SetDirty(dropTable);
            
            Debug.Log("✓ 掉落表已创建（80%几率掉番茄1-2个，60%几率掉土豆1个）");
        }
        else
        {
            Debug.LogWarning("⚠ 未找到食材数据，请先创建食材");
        }
    }

    private static void SetupCurrentScene()
    {
        Debug.Log("=== 开始设置场景 ===");
        
        // 1. 创建LevelHealthSystem
        CreateLevelHealthSystem();
        
        // 2. 创建WaveManager
        CreateWaveManager();
        
        // 3. 创建OrderManager
        CreateOrderManager();
        
        // 4. 创建地面
        CreateGround();
        
        // 5. 创建厨房区域触发器
        CreateKitchenZone();
        
        // 6. 创建玩家
        CreatePlayer();
        
        // 7. 创建UI提示
        CreateUIHint();
        
        Debug.Log("✓ 场景设置完成!");
    }

    private static void CreateLevelHealthSystem()
    {
        var go = new GameObject("LevelHealthSystem");
        var system = go.AddComponent<LevelHealthSystem>();
        // 配置会通过Inspector或代码默认值设置
        Debug.Log("✓ 关卡生命系统已创建");
    }

    private static void CreateWaveManager()
    {
        var go = new GameObject("WaveManager");
        var manager = go.AddComponent<Assets.Scripts.Wave.WaveManager>();
        
        // 尝试加载波次数据
        var wave1 = AssetDatabase.LoadAssetAtPath<WaveData>("Assets/Resources/Data/Waves/Wave1.asset");
        if (wave1 != null)
        {
            // 注意: waves是数组，需要通过反射或在Inspector中设置
            Debug.Log("✓ 波次管理器已创建（请在Inspector中配置波次数据）");
        }
        else
        {
            Debug.Log("✓ 波次管理器已创建");
        }
    }

    private static void CreateOrderManager()
    {
        var go = new GameObject("OrderManager");
        var manager = go.AddComponent<OrderManager>();
        
        Debug.Log("✓ 订单管理器已创建");
    }

    private static void CreateGround()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = "Ground";
        go.transform.position = Vector3.zero;
        go.transform.localScale = new Vector3(5, 1, 5);
        
        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            // 编辑模式使用 sharedMaterial 避免材质泄漏
            renderer.sharedMaterial.color = new Color(0.8f, 0.8f, 0.7f);
        }
        
        Debug.Log("✓ 地面已创建");
    }

    private static void CreateKitchenZone()
    {
        var go = new GameObject("KitchenZone");
        go.transform.position = new Vector3(0, 0.5f, -20);
        
        var trigger = go.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(20, 5, 5);
        
        var zone = go.AddComponent<KitchenZoneTrigger>();
        
        // 添加可视化标记
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "VisualMarker";
        cube.transform.SetParent(go.transform);
        cube.transform.localPosition = Vector3.zero;
        cube.transform.localScale = new Vector3(20, 5, 5);
        
        var renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1, 0, 0, 0.3f);
            mat.SetFloat("_Mode", 3); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            // 编辑模式使用 sharedMaterial
            renderer.sharedMaterial = mat;
        }
        
        DestroyImmediate(cube.GetComponent<Collider>());
        
        Debug.Log("✓ 厨房区域已创建（红色半透明区域）");
    }

    private static void CreatePlayer()
    {
        // 检查是否有现成的玩家Prefab
        var playerPrefab = Resources.Load<GameObject>("CharacterPrefabs/CC_Object_Player");
        
        if (playerPrefab != null)
        {
            var player = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
            if (player != null)
            {
                player.transform.position = new Vector3(0, 1, 0);
                Debug.Log("✓ 玩家已创建（使用现有Prefab）");
                return;
            }
        }
        
        // 如果没有Prefab，创建简单的玩家对象
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "Player";
        go.transform.position = new Vector3(0, 1, 0);
        
        var rb = go.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        Debug.Log("✓ 简单玩家对象已创建（需要添加玩家控制脚本）");
    }

    private static void CreateUIHint()
    {
        var canvas = new GameObject("DemoUICanvas");
        var canvasComponent = canvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        
        canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // 创建提示文本
        var textGO = new GameObject("HintText");
        textGO.transform.SetParent(canvas.transform);
        
        var rectTransform = textGO.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(10, -10);
        rectTransform.sizeDelta = new Vector2(400, 200);
        
        var text = textGO.AddComponent<UnityEngine.UI.Text>();
        text.text = "=== Demo场景 ===\n\n系统状态显示:\n- 关卡生命: 查看Console\n- 波次信息: 查看Console\n\n提示: 打开Console窗口查看详细信息";
        
        // Unity 2022+ 使用 LegacyRuntime.ttf 代替 Arial.ttf
        Font builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (builtinFont == null)
        {
            // 如果LegacyRuntime也不存在，尝试Arial（旧版本Unity）
            builtinFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        
        if (builtinFont != null)
        {
            text.font = builtinFont;
        }
        else
        {
            Debug.LogWarning("无法加载内置字体，UI文本将使用默认字体");
        }
        
        text.fontSize = 14;
        text.color = Color.white;
        
        Debug.Log("✓ UI提示已创建");
    }

    // 辅助方法
    private static T CreateAsset<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
        }
        return asset;
    }

    private static void CreateDirectoryIfNotExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parentFolder = Path.GetDirectoryName(path).Replace('\\', '/');
            string folderName = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parentFolder, folderName);
        }
    }
}

