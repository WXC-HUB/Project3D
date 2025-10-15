using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.BaseUtils;

public class BaseUI<T> : MonoBehaviour where T: BaseUI<T>
{
    public Dictionary<string , GameObject> nodeDics = new Dictionary<string , GameObject>();
    public static T instance = null;  
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
            Debug.LogError("≥ˆœ÷÷ÿ∏¥UI" +  instance.name);  
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
