using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;

using UnityEngine;
using Assets.Scripts.Core;

namespace Assets.Scripts.AI
{
    
    public class AISensorBase : MonoBehaviour 
    {
        float get_dis(CharacterCtrlBase x)
        {
            return (x.transform.position - transform.position).magnitude;
        }
        public CharacterCtrlBase getCharacterByKey(string key)
        {
            if (key == "NearestEnemy")
            {
                List<CharacterCtrlBase> list;
                if (LevelManager.Instance.Character_Dict.TryGetValue(InGameCharacterType.Enemy, out list))
                {
                    list.RemoveAll(x => x == null);
                    if (list.Count > 0) 
                    {
                        list = list.OrderBy(get_dis).ToList();
                        return list[0];
                    }
                    return null;
                }
            }
            else if (key.StartsWith("NearestDish_") )
            {
                int dishID = int.Parse(key.Split("_")[1]);
                List<CharacterCtrlBase> list;
                if (LevelManager.Instance.Character_Dict.TryGetValue(InGameCharacterType.Dish, out list))
                {
                    list.RemoveAll(x => x == null);
                    list = list.OrderBy(get_dis).ToList();
                    foreach (CharacterCtrlBase item in list) 
                    {
                        if(item != null && item.dishID == dishID)
                        {
                            return item;
                        }
                    }
                    return null;
                }
            }
            
            return null;
        }
    }
}
