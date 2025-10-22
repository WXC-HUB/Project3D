using UnityEngine;
using UnityEditor;
using System.IO;
using Assets.Scripts.AI;

/// <summary>
/// 自动生成散射塔、减速塔及其子弹预制体的Editor工具
/// </summary>
public class TowerPrefabGenerator : EditorWindow
{
    [MenuItem("Tools/生成防御塔预制体")]
    public static void GenerateTowerPrefabs()
    {
        string prefabPath = "Assets/Resources/CharacterPrefabs/";
        string fieldPrefabPath = "Assets/Resources/FieldPrefabs/";
        
        // 确保目录存在
        if (!Directory.Exists(prefabPath))
        {
            Directory.CreateDirectory(prefabPath);
        }
        if (!Directory.Exists(fieldPrefabPath))
        {
            Directory.CreateDirectory(fieldPrefabPath);
        }

        // 生成散射防御塔
        GenerateScatterTower(prefabPath);
        
        // 生成减速防御塔
        GenerateSlowTower(prefabPath);
        
        // 生成散射子弹
        GenerateScatterBullet(prefabPath);
        
        // 生成减速子弹
        GenerateSlowBullet(prefabPath);
        
        // 生成减速场地
        GenerateSlowField(fieldPrefabPath);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("✅ 所有防御塔预制体已生成完成！");
        EditorUtility.DisplayDialog("完成", "所有防御塔预制体已生成！\n\n包括：\n- 散射防御塔\n- 减速防御塔\n- 散射子弹\n- 减速子弹\n- 减速场地", "确定");
    }

    private static void GenerateScatterTower(string basePath)
    {
        string prefabName = "CC_Object_Tower_Scatter.prefab";
        string fullPath = Path.Combine(basePath, prefabName);
        
        // 如果已存在，先删除
        if (File.Exists(fullPath))
        {
            AssetDatabase.DeleteAsset(fullPath);
        }

        // 创建GameObject
        GameObject tower = new GameObject("CC_Object_Tower_Scatter");
        
        // 添加必需组件
        var ctrl = tower.AddComponent<CharacterCtrlBase>();
        var rb = tower.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        
        var collider = tower.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1f, 1f);
        
        // 添加散射AI组件
        var scatterAI = tower.AddComponent<TowerAI_Scatter>();
        scatterAI.bulletCount = 3;
        scatterAI.spreadAngle = 15f;
        scatterAI.scatterBulletID = 20;
        
        // 添加视觉标识（一个简单的橙色方块）
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(tower.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        
        // 设置材质颜色为橙色
        var renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = new Color(1f, 0.4f, 0f); // 橙色
        }
        
        // 移除视觉对象的碰撞体（避免干扰游戏逻辑）
        DestroyImmediate(visual.GetComponent<Collider>());
        
        // 保存为预制体
        PrefabUtility.SaveAsPrefabAsset(tower, fullPath);
        DestroyImmediate(tower);
        
        Debug.Log($"✅ 已生成: {prefabName}");
    }

    private static void GenerateSlowTower(string basePath)
    {
        string prefabName = "CC_Object_Tower_Slow.prefab";
        string fullPath = Path.Combine(basePath, prefabName);
        
        if (File.Exists(fullPath))
        {
            AssetDatabase.DeleteAsset(fullPath);
        }

        GameObject tower = new GameObject("CC_Object_Tower_Slow");
        
        var ctrl = tower.AddComponent<CharacterCtrlBase>();
        var rb = tower.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        
        var collider = tower.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1f, 1f);
        
        // 添加AI组件（使用原有的AIAgentBase）
        tower.AddComponent<AIAgentBase>();
        
        // 添加视觉标识（蓝色方块）
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(tower.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        
        var renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = new Color(0.2f, 0.4f, 1f); // 蓝色
        }
        
        DestroyImmediate(visual.GetComponent<Collider>());
        
        PrefabUtility.SaveAsPrefabAsset(tower, fullPath);
        DestroyImmediate(tower);
        
        Debug.Log($"✅ 已生成: {prefabName}");
    }

    private static void GenerateScatterBullet(string basePath)
    {
        string prefabName = "CC_Bullet_Scatter.prefab";
        string fullPath = Path.Combine(basePath, prefabName);
        
        if (File.Exists(fullPath))
        {
            AssetDatabase.DeleteAsset(fullPath);
        }

        GameObject bullet = new GameObject("CC_Bullet_Scatter");
        
        var ctrl = bullet.AddComponent<CharacterCtrlBase>();
        var rb = bullet.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        
        var collider = bullet.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.2f;
        
        // 添加视觉标识（小橙色球）
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.name = "Visual";
        visual.transform.SetParent(bullet.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        
        var renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = new Color(1f, 0.4f, 0f); // 橙色
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", new Color(1f, 0.4f, 0f) * 0.5f);
        }
        
        DestroyImmediate(visual.GetComponent<Collider>());
        
        PrefabUtility.SaveAsPrefabAsset(bullet, fullPath);
        DestroyImmediate(bullet);
        
        Debug.Log($"✅ 已生成: {prefabName}");
    }

    private static void GenerateSlowBullet(string basePath)
    {
        string prefabName = "CC_Bullet_Slow.prefab";
        string fullPath = Path.Combine(basePath, prefabName);
        
        if (File.Exists(fullPath))
        {
            AssetDatabase.DeleteAsset(fullPath);
        }

        GameObject bullet = new GameObject("CC_Bullet_Slow");
        
        var ctrl = bullet.AddComponent<CharacterCtrlBase>();
        var rb = bullet.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        
        var collider = bullet.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.2f;
        
        // 添加视觉标识（小蓝色球）
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.name = "Visual";
        visual.transform.SetParent(bullet.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        
        var renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = new Color(0.2f, 0.6f, 1f); // 亮蓝色
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", new Color(0.2f, 0.6f, 1f) * 0.5f);
        }
        
        DestroyImmediate(visual.GetComponent<Collider>());
        
        PrefabUtility.SaveAsPrefabAsset(bullet, fullPath);
        DestroyImmediate(bullet);
        
        Debug.Log($"✅ 已生成: {prefabName}");
    }

    private static void GenerateSlowField(string basePath)
    {
        string prefabName = "GameField_SlowExplosion.prefab";
        string fullPath = Path.Combine(basePath, prefabName);
        
        if (File.Exists(fullPath))
        {
            AssetDatabase.DeleteAsset(fullPath);
        }

        GameObject field = new GameObject("GameField_SlowExplosion");
        
        // 添加减速场地组件
        var slowField = field.AddComponent<GameField_SlowExplosion>();
        slowField.slowModifierID = 15001;
        slowField.slowDuration = 3f;
        
        var collider = field.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 2f;
        
        // 添加视觉标识（半透明蓝色圆盘）
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.name = "Visual";
        visual.transform.SetParent(field.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(4f, 0.1f, 4f); // 扁平的圆盘
        visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        
        var renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = new Color(0.2f, 0.6f, 1f, 0.4f); // 半透明蓝色
            renderer.material.SetFloat("_Mode", 3); // Transparent mode
            renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            renderer.material.SetInt("_ZWrite", 0);
            renderer.material.DisableKeyword("_ALPHATEST_ON");
            renderer.material.EnableKeyword("_ALPHABLEND_ON");
            renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            renderer.material.renderQueue = 3000;
        }
        
        DestroyImmediate(visual.GetComponent<Collider>());
        
        PrefabUtility.SaveAsPrefabAsset(field, fullPath);
        DestroyImmediate(field);
        
        Debug.Log($"✅ 已生成: {prefabName}");
    }
}

