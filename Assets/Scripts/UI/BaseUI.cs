using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.BaseUtils;
using UnityEngine.UI;

public class BaseUI<T> : MonoBehaviour where T: BaseUI<T>
{
    public Dictionary<string , GameObject> nodeDics = new Dictionary<string , GameObject>();
    public static T instance = null;

    public static bool LoadImageToUI(Image targetImage, string imagePath)
    {
        // 参数检查
        if (targetImage == null)
        {
            Debug.LogError("目标Image组件为空！");
            return false;
        }

        if (string.IsNullOrEmpty(imagePath))
        {
            Debug.LogError("图片路径为空！");
            return false;
        }

        // 从Resources加载图片
        Sprite loadedSprite = Resources.Load<Sprite>("Sprites/" + imagePath);

        if (loadedSprite != null)
        {
            // 成功加载，赋值给Image组件
            targetImage.sprite = loadedSprite;
            Debug.Log($"成功加载图片: {imagePath}");
            return true;
        }
        else
        {
            // 加载失败
            Debug.LogError($"无法在路径 Resources/{imagePath} 找到图片！请检查：\n" +
                          "1. 路径是否正确\n" +
                          "2. 图片是否在Resources文件夹内\n" +
                          "3. 文件扩展名是否正确（Unity会自动处理，路径中不要加扩展名）");
            return false;
        }
    }
    public virtual void InitUI()
    {
        List<GameObject> list = new List<GameObject>();
        GameUtils.getAllChilds(this.gameObject, ref list);
        foreach (GameObject node in list)
        {
            if (node.name.StartsWith("m_"))
            {
                if (nodeDics.ContainsKey(node.name))
                {
                    nodeDics[node.name] = node;
                }
                else
                {
                    nodeDics.Add(node.name, node);
                }
            }
        }
    }

    private void Awake()
    {
        Debug.Log("ui init");

        if (instance != null) {
            Debug.LogError("出现重复UI" +  instance.name);  
        }
        instance = this as T;

        InitUI();


    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
