using UnityEngine;
using UnityEditor;

/// <summary>
/// 为防御塔预制体添加所需的组件
/// </summary>
public class TowerComponentSetup : EditorWindow
{
    [MenuItem("Tools/配置防御塔组件")]
    public static void SetupTowerComponents()
    {
        bool success = true;
        
        // 配置散射塔
        success &= SetupScatterTower();
        
        if (success)
        {
            Debug.Log("✅ 所有防御塔组件配置完成！");
            EditorUtility.DisplayDialog("完成", "防御塔组件配置完成！\n\n散射塔已添加 TowerAI_Scatter 组件", "确定");
        }
        else
        {
            Debug.LogWarning("⚠️ 部分防御塔配置失败，请检查预制体是否存在");
        }
    }
    
    private static bool SetupScatterTower()
    {
        string prefabPath = "Assets/Resources/CharacterPrefabs/CC_Object_Tower_Scatter.prefab";
        
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"❌ 找不到散射塔预制体: {prefabPath}");
            return false;
        }
        
        // 加载预制体实例用于编辑
        GameObject instance = PrefabUtility.LoadPrefabContents(prefabPath);
        
        // 检查是否已有 TowerAI_Scatter 组件
        TowerAI_Scatter scatterAI = instance.GetComponent<TowerAI_Scatter>();
        if (scatterAI == null)
        {
            scatterAI = instance.AddComponent<TowerAI_Scatter>();
            Debug.Log("✅ 已添加 TowerAI_Scatter 组件");
        }
        
        // 配置参数
        scatterAI.bulletCount = 3;
        scatterAI.spreadAngle = 15f;
        scatterAI.scatterBulletID = 20;
        
        // 保存预制体
        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        PrefabUtility.UnloadPrefabContents(instance);
        
        Debug.Log($"✅ 散射塔配置完成: {prefabPath}");
        return true;
    }
}

