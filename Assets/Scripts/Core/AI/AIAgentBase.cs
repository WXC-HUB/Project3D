using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using UnityEngine;
using Defective.JSON;
using Unity.VisualScripting;

namespace Assets.Scripts.AI
{
    public enum AIAgentState
    {
        NotReady,
        Ready,
        Running,
        Pause,
    }

    public class AIAgentBase : MonoBehaviour
    {
       
        
        public Dictionary<string , Vector2> mPosDict = new Dictionary<string , Vector2>();
        public Dictionary<string , CharacterCtrlBase> mCharacterDict = new Dictionary<string , CharacterCtrlBase>();

        public BTree curTree = null;
        public AIAgentState state = AIAgentState.Running;

        public CharacterCtrlBase bindCharacterCtrl = null;
        public AISensorBase sensor = null;

        public int isFollowRoad = 0;
        public Vector3Int curPos = new Vector3Int(-999,-999);
        public int NowIndex = 0;

        private void Start()
        {
            sensor = this.GetComponent<AISensorBase>();
            bindCharacterCtrl = this.GetComponent<CharacterCtrlBase>(); 
        }

        public bool DoAction( string name , string my_params )
        {
            List<string> properties = my_params.Split('|').ToList();
            if( name == "GetCharacterToDic")
            {
                string getType = properties[0];
                string toDic = properties[1];   
                CharacterCtrlBase resCtrl = sensor.getCharacterByKey(getType);
                if (mCharacterDict.ContainsKey(toDic)) 
                {
                    mCharacterDict[toDic] = resCtrl;
                }
                else
                {
                    mCharacterDict.Add(toDic, resCtrl);
                }
            }
            if (name == "FollowPath")
            {
                if (isFollowRoad == 0)
                {
                    return false;
                }
                SpawnRootInfo sp;
                Vector3Int target_pos = new Vector3Int();
                if(LevelGridGenerator.Instance.spawnroot_dictionay.TryGetValue(isFollowRoad , out sp))
                {
                    curPos = LevelGridGenerator.Instance.tilemap.WorldToCell(transform.position);
                    
                    if ( sp.move_points.Contains(curPos))
                    {
                        if(sp.move_points.IndexOf(curPos) == NowIndex)
                        {
                            NowIndex += 1;
                        }
                        if( NowIndex < sp.move_points.Count - 1)
                        {
                            target_pos = sp.move_points[NowIndex];
                        }
      
                    }
                    else
                    {
                        target_pos = sp.move_points[NowIndex];
                    }

                    Debug.Log("goto:" +  target_pos);

                    this.transform.position = Vector3.Lerp(transform.position, LevelGridGenerator.Instance.tilemap.CellToWorld(target_pos), 10* Time.deltaTime);
                }
                else
                {
                    return false ;
                }
            }
            return false;
        }

        public bool DoCondition(string name, string my_params)
        {
            List<string> properties = my_params.Split('|').ToList();
            if (name == "IsBool")
            {
                if(bindCharacterCtrl.AttributesDicts.ContainsKey(properties[0]))
                {
                    return (this.bindCharacterCtrl.AttributesDicts[properties[0]] as Character_Bool).GetValue();
                }
                else
                {
                    return false;
                }
            }
            else if (name == "IsFloat")
            {
                if (bindCharacterCtrl.AttributesDicts.ContainsKey(properties[0]) )
                {
                    return MathF.Abs(float.Parse(properties[1]) -
                        (this.bindCharacterCtrl.AttributesDicts[properties[1]] as Character_Float).GetValue() ) <= 0.001f ; 
                }
            }
            else if (name == "HasCharacter")
            {
                return mCharacterDict.ContainsKey(properties[0]) && mCharacterDict[properties[0]] != null;
            }
            else if(name == "NearPos")
            {
                if( mPosDict.ContainsKey(properties[0]) && mPosDict[properties[0]] != null)
                {
                    return ((this.bindCharacterCtrl.AttributesDicts[properties[0]] as Character_Vector2).GetValue()
                        - this.mPosDict[properties[0]]).magnitude <= 0.001f;
                }

            }

            return false;
        }

       
        private void Update()
        {
            if (state == AIAgentState.Running  && curTree != null)
            {
                //curTree.RunNode();
            }
        }
    }
}
