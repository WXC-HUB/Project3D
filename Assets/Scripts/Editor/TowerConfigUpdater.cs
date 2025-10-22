using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

/// <summary>
/// 防御塔配置更新工具
/// 自动更新CSV配置表，添加散射塔和减速塔
/// </summary>
public class TowerConfigUpdater : EditorWindow
{
    [MenuItem("Tools/更新防御塔配置表")]
    static void UpdateTowerConfigs()
    {
        bool success = true;
        StringBuilder log = new StringBuilder();

        try
        {
            // 1. 更新 GameCharacters.csv
            success &= UpdateGameCharacters(log);

            // 2. 更新 GameModifiers.csv
            success &= UpdateGameModifiers(log);

            // 3. 更新 GameFields.csv
            success &= UpdateGameFields(log);

            // 4. 更新 GameSkills.csv
            success &= UpdateGameSkills(log);

            if (success)
            {
                EditorUtility.DisplayDialog("成功", 
                    "防御塔配置表更新成功！\n\n" + log.ToString() + 
                    "\n下一步：\n" +
                    "1. 执行 __表格更改后执行脚本！！！！.bat\n" +
                    "2. 在Unity中创建预制体（参考TowerSystemSetup.md）", 
                    "确定");
                Debug.Log("✅ 配置更新成功：\n" + log.ToString());
            }
            else
            {
                EditorUtility.DisplayDialog("失败", 
                    "配置更新过程中出现错误，请查看Console", 
                    "确定");
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("错误", 
                $"更新配置时发生异常：\n{e.Message}", 
                "确定");
            Debug.LogError($"配置更新异常：{e}");
        }

        AssetDatabase.Refresh();
    }

    static bool UpdateGameCharacters(StringBuilder log)
    {
        string path = "Assets/Resources/Configs/GameCharacters.csv";
        if (!File.Exists(path))
        {
            Debug.LogError($"文件不存在: {path}");
            return false;
        }

        string content = File.ReadAllText(path);
        
        // 检查是否已经添加
        if (content.Contains("散射防御塔") || content.Contains("ObjectID,10,"))
        {
            log.AppendLine("- GameCharacters.csv: 已包含新配置，跳过");
            return true;
        }

        // 添加新配置
        StringBuilder newContent = new StringBuilder(content);
        if (!content.EndsWith("\n")) newContent.AppendLine();
        
        newContent.AppendLine("10,散射防御塔,Tower,CC_Object_Tower_Scatter,12001");
        newContent.AppendLine("11,减速防御塔,Tower,CC_Object_Tower_Slow,12001");
        newContent.AppendLine("20,散射子弹,Bullet,CC_Bullet_Scatter,8001");
        newContent.AppendLine("21,减速子弹,Bullet,CC_Bullet_Slow,8001|14001");

        File.WriteAllText(path, newContent.ToString());
        log.AppendLine("✅ GameCharacters.csv: 添加了4个新对象");
        return true;
    }

    static bool UpdateGameModifiers(StringBuilder log)
    {
        string path = "Assets/Resources/Configs/GameModifiers.csv";
        if (!File.Exists(path))
        {
            Debug.LogError($"文件不存在: {path}");
            return false;
        }

        string content = File.ReadAllText(path);
        
        // 检查是否已经添加
        if (content.Contains("14001,") || content.Contains("减速子弹"))
        {
            log.AppendLine("- GameModifiers.csv: 已包含新配置，跳过");
            return true;
        }

        StringBuilder newContent = new StringBuilder(content);
        if (!content.EndsWith("\n")) newContent.AppendLine();
        
        newContent.AppendLine("14001,1,ListenGameEvent,CharacterDoCollide|SpawnField|11|HitPointX|HitPointY,子弹：碰撞时产生减速场地");
        newContent.AppendLine("14001,2,ListenGameEvent,CharacterDoCollide|DispelModifier|14001|DoHitCharacter,子弹：碰撞后驱散自身");
        newContent.AppendLine("15001,1,AddSimpleDect,float_mul|MaxSpeed|-0.5,减速Buff：移动速度减少50%");
        newContent.AppendLine("15001,2,BindVFX,VFX_Slow_Effect,减速Buff：绑定减速特效");

        File.WriteAllText(path, newContent.ToString());
        log.AppendLine("✅ GameModifiers.csv: 添加了4个新修改器");
        return true;
    }

    static bool UpdateGameFields(StringBuilder log)
    {
        string path = "Assets/Resources/Configs/GameFields.csv";
        if (!File.Exists(path))
        {
            Debug.LogError($"文件不存在: {path}");
            return false;
        }

        string content = File.ReadAllText(path);
        
        // 检查是否已经添加
        if (content.Contains("GameField_SlowExplosion"))
        {
            log.AppendLine("- GameFields.csv: 已包含新配置，跳过");
            return true;
        }

        StringBuilder newContent = new StringBuilder(content);
        if (!content.EndsWith("\n")) newContent.AppendLine();
        
        newContent.AppendLine("11,GameField_SlowExplosion,减速爆炸场地");

        File.WriteAllText(path, newContent.ToString());
        log.AppendLine("✅ GameFields.csv: 添加了1个新场地");
        return true;
    }

    static bool UpdateGameSkills(StringBuilder log)
    {
        string path = "Assets/Resources/Configs/GameSkills.csv";
        if (!File.Exists(path))
        {
            Debug.LogError($"文件不存在: {path}");
            return false;
        }

        string content = File.ReadAllText(path);
        
        // 检查是否已经添加
        if (content.Contains("散射攻击"))
        {
            log.AppendLine("- GameSkills.csv: 已包含新配置，跳过");
            return true;
        }

        StringBuilder newContent = new StringBuilder(content);
        if (!content.EndsWith("\n")) newContent.AppendLine();
        
        newContent.AppendLine("10,散射攻击,TRUE,FALSE,13001,-1,FF6600,散射塔专用攻击");
        newContent.AppendLine("11,减速攻击,TRUE,FALSE,14001,-1,3366FF,减速塔专用攻击");

        File.WriteAllText(path, newContent.ToString());
        log.AppendLine("✅ GameSkills.csv: 添加了2个新技能");
        return true;
    }
}

