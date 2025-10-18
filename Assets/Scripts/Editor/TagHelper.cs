using UnityEngine;
using UnityEditor;

/// <summary>
/// 标签辅助工具 - 自动添加常用标签
/// </summary>
[InitializeOnLoad]
public static class TagHelper
{
    static TagHelper()
    {
        // Unity启动时自动添加必要的标签
        AddTag("Enemy");
        AddTag("Tower");
        AddTag("Ingredient");
        AddTag("KitchenAppliance");
    }
    
    public static void AddTag(string tag)
    {
        // 获取TagManager
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        
        // 检查标签是否已存在
        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(tag))
            {
                found = true;
                break;
            }
        }
        
        // 如果不存在则添加
        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
            n.stringValue = tag;
            tagManager.ApplyModifiedProperties();
            Debug.Log($"✓ 标签已添加: {tag}");
        }
    }
}

/// <summary>
/// 标签管理菜单
/// </summary>
public class TagManagerMenu
{
    [MenuItem("游戏Demo/工具/添加常用标签")]
    public static void AddCommonTags()
    {
        TagHelper.AddTag("Enemy");
        TagHelper.AddTag("Tower");
        TagHelper.AddTag("Player");
        TagHelper.AddTag("Ingredient");
        TagHelper.AddTag("KitchenAppliance");
        TagHelper.AddTag("Storage");
        TagHelper.AddTag("Delivery");
        
        EditorUtility.DisplayDialog(
            "标签已添加",
            "已添加以下标签:\n\n" +
            "✓ Enemy\n" +
            "✓ Tower\n" +
            "✓ Player\n" +
            "✓ Ingredient\n" +
            "✓ KitchenAppliance\n" +
            "✓ Storage\n" +
            "✓ Delivery",
            "确定");
    }
}

