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
        // �������
        if (targetImage == null)
        {
            Debug.LogError("Ŀ��Image���Ϊ�գ�");
            return false;
        }

        if (string.IsNullOrEmpty(imagePath))
        {
            Debug.LogError("ͼƬ·��Ϊ�գ�");
            return false;
        }

        // ��Resources����ͼƬ
        Sprite loadedSprite = Resources.Load<Sprite>("Sprites/" + imagePath);

        if (loadedSprite != null)
        {
            // �ɹ����أ���ֵ��Image���
            targetImage.sprite = loadedSprite;
            Debug.Log($"�ɹ�����ͼƬ: {imagePath}");
            return true;
        }
        else
        {
            // ����ʧ��
            Debug.LogError($"�޷���·�� Resources/{imagePath} �ҵ�ͼƬ�����飺\n" +
                          "1. ·���Ƿ���ȷ\n" +
                          "2. ͼƬ�Ƿ���Resources�ļ�����\n" +
                          "3. �ļ���չ���Ƿ���ȷ��Unity���Զ�������·���в�Ҫ����չ����");
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

        if (instance != null && instance != this) {
            Debug.LogWarning("Duplicate UI instance detected, destroying new one: " + this.name);
            Destroy(this.gameObject);
            return;
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
