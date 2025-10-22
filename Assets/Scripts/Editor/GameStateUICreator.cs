using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

public class GameStateUICreator : EditorWindow
{
    [MenuItem("Tools/创建游戏状态UI")]
    static void CreateGameStateUI()
    {
        // 确保Resources/UIPrefabs目录存在
        string prefabPath = "Assets/Resources/UIPrefabs";
        if (!Directory.Exists(prefabPath))
        {
            Directory.CreateDirectory(prefabPath);
        }

        // 创建Canvas（如果场景中没有）
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 创建UI_GameState根对象
        GameObject uiGameStateObj = new GameObject("UI_GameState");
        
        // 添加Canvas组件（UIManager需要）
        Canvas uiCanvas = uiGameStateObj.AddComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // 添加CanvasScaler
        CanvasScaler canvasScaler = uiGameStateObj.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        
        // 添加GraphicRaycaster（用于UI交互）
        uiGameStateObj.AddComponent<GraphicRaycaster>();
        
        // 添加RectTransform并设置为全屏
        RectTransform uiRect = uiGameStateObj.GetComponent<RectTransform>();
        uiRect.anchorMin = Vector2.zero;
        uiRect.anchorMax = Vector2.one;
        uiRect.sizeDelta = Vector2.zero;
        uiRect.anchoredPosition = Vector2.zero;

        // 添加UI_GameState脚本
        UI_GameState uiGameState = uiGameStateObj.AddComponent<UI_GameState>();

        // 创建半透明背景（可选）
        GameObject bgObj = new GameObject("m_Background");
        bgObj.transform.SetParent(uiGameStateObj.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.5f); // 半透明黑色背景
        bgObj.SetActive(false); // 初始隐藏

        // 创建文本对象
        GameObject textObj = new GameObject("m_Text_Message");
        textObj.transform.SetParent(uiGameStateObj.transform, false);
        
        // 设置文本的RectTransform
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(800, 200);
        textRect.anchoredPosition = Vector2.zero;

        // 添加Text组件并设置属性
        Text text = textObj.AddComponent<Text>();
        text.text = "游戏消息";
        
        // Unity 2022.3+ 使用 LegacyRuntime.ttf 替代 Arial.ttf
        Font builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (builtinFont == null)
        {
            // 如果LegacyRuntime不存在，尝试使用Arial（兼容旧版本）
            builtinFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        text.font = builtinFont;
        
        text.fontSize = 72;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        
        // 添加Outline效果使文字更清晰（可选，手动在Inspector中调整）
        Outline outline = textObj.AddComponent<Outline>();
        
        // 添加Shadow效果（可选，手动在Inspector中调整）
        Shadow shadow = textObj.AddComponent<Shadow>();

        // 初始隐藏文本
        textObj.SetActive(false);

        // 保存为预制体
        string prefabFullPath = prefabPath + "/UI_GameState.prefab";
        
        // 如果预制体已存在，先删除
        if (File.Exists(prefabFullPath))
        {
            AssetDatabase.DeleteAsset(prefabFullPath);
        }

        // 创建预制体
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(uiGameStateObj, prefabFullPath);
        
        if (prefab != null)
        {
            Debug.Log($"✅ UI_GameState预制体创建成功: {prefabFullPath}");
            
            // 删除场景中的临时对象
            DestroyImmediate(uiGameStateObj);
            
            // 选中创建的预制体
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            
            EditorUtility.DisplayDialog("成功", 
                "UI_GameState预制体已创建！\n\n" +
                "位置: Assets/Resources/UIPrefabs/UI_GameState.prefab\n\n" +
                "下一步：取消注释 LevelManager.cs 中的初始化代码", 
                "确定");
        }
        else
        {
            Debug.LogError("❌ 创建UI_GameState预制体失败");
            EditorUtility.DisplayDialog("失败", "创建预制体时发生错误", "确定");
        }

        AssetDatabase.Refresh();
    }
}

