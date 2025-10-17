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
                    if (list.Count > 0) 
                    {
                        list = list.OrderBy(get_dis).ToList();
                        return list[0];
                    }
                    return null;
                }

            }
            
            return null;
        }
    }
}
