using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.BaseUtils;
using UnityEngine.UIElements;


namespace Assets.Scripts.UI
{
    public enum UILayers
    {
        UILayers_Default
    }
   
    public class UIManager : Singleton<UIManager>
    {
        public GameObject uiRoot;

        public void InitUIManager()
        {
            uiRoot = GameObject.Find("UIRoot");
        }

        public Dictionary<UILayers , int> layerValues = new Dictionary<UILayers , int>()
        {
            {UILayers.UILayers_Default, 0},
        };

        public int GetNewUILayerValue(UILayers layer)
        {
            return layerValues[layer];  
        }

        public GameObject getGameObjectByUIName(string ui_name)
        {
            return Resources.Load<GameObject>("UIPrefabs/" + ui_name);
            
            //Debug.Log("Assets/UI/Prefabs/" + ui_name);
            //return AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefabs/" + ui_name + ".prefab");

        }
        
        public T CreateUIByName<T>(string ui_name , UILayers layer = 0) where T : BaseUI<T>
        {
            GameObject gameObject = getGameObjectByUIName(ui_name: ui_name);
            //Debug.Log(gameObject);
            GameObject new_UIObject =  GameObject.Instantiate(gameObject, uiRoot.transform);

            new_UIObject.GetComponent<Canvas>().sortingOrder = GetNewUILayerValue(layer);
            T retMono = new_UIObject.GetComponent<T>();

            retMono.InitUI();
            return retMono;
        }
    }
}
