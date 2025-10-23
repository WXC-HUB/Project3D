using Assets.Scripts.BaseUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class UI_TimeFollowInfo
{
    public LevelGridTileObject followChar;
    public Slider slider_last_time;

    public GameObject bindTranform;
}


public class UI_cooktime : BaseUI<UI_cooktime>
{
    public Dictionary<LevelGridTileObject, UI_TimeFollowInfo> last_time_dics = new Dictionary<LevelGridTileObject, UI_TimeFollowInfo>();
    
    public void CallFocus(LevelGridTileObject from_char)
    {
        if(from_char == null || from_char.NowRecipeID == 0 || last_time_dics.ContainsKey(from_char))
        {
            return;
        }
        UI_TimeFollowInfo uInfo = new UI_TimeFollowInfo();  
        uInfo.followChar = from_char;

        var item_i = nodeDics["m_Slider_Level01_s_Blue"];
        var spawn_item = Instantiate(item_i);
        uInfo.slider_last_time = spawn_item.GetComponent<Slider>();
        uInfo.bindTranform = spawn_item;

        last_time_dics.Add(from_char, uInfo);   

        spawn_item.SetActive(true);
        spawn_item.transform.SetParent(transform , false);

    }
    
    // Start is called before the first frame update
    void Start()
    {
        nodeDics["m_Slider_Level01_s_Blue"].SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        var keysToRemove = new List<LevelGridTileObject>(); // 请将 YourKeyType 替换为实际的键类型

        var key_list = last_time_dics.Keys;
        foreach (var key in key_list)
        {
            if (key.NowRecipeID == 0)
            {
                keysToRemove.Add(key);
            }
            else
            {
                float slider_value = (float)key.NowRecipeTime / (float)key.ShowMaxRecipeTime;
                if (slider_value > 1)
                {
                    slider_value = 1;
                }
                if (slider_value < 0)
                {
                    slider_value = 0;
                }
                last_time_dics[key].slider_last_time.value = slider_value;
                last_time_dics[key].bindTranform.transform.position = Camera.main.WorldToScreenPoint(key.transform.position);
            }
        }

        // 遍历结束后再删除
        foreach (var key in keysToRemove)
        {
            Destroy(last_time_dics[key].bindTranform.gameObject);
            
            last_time_dics.Remove(key);
        }
    }
}
