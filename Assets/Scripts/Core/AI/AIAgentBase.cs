using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using UnityEngine;
using Defective.JSON;
using Unity.VisualScripting;
using UnityEngine.Tilemaps;

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

        public List<Vector3Int> path; // 在Inspector中设置的路径
        public float speed = 2f; // 移动速度

        private int currentIndex = 0;
        private Vector3 currentWorldPos;

        private Tilemap tilemap;
        private bool isMoving = true;

        SpawnRootInfo spawnRootInfo = null;

        public int followPath = 101;

        private Vector3 GridToWorld(Vector3Int gridPos)
        {
            if ( tilemap != null)
            {
                // 通过GridLayout和Tilemap获取精确的世界坐标
                return tilemap.GetCellCenterWorld(gridPos);// + tilemap.tileAnchor;
                //tilemap.GetCellCenterWorld
            }

            return new Vector3Int();
        }


        public void SetFollowPath(SpawnRootInfo root)
        {
            if(spawnRootInfo == root)
            {
                return;
            }

            spawnRootInfo = root;
            tilemap = LevelGridGenerator.Instance.tilemap;
            path = root.move_points;
            transform.position = GridToWorld(path[0]);
            currentWorldPos = transform.position;
            
            spawnRootInfo = root;

            // 如果路径只有一个点，直接设置到目标位置
            if (path.Count > 1)
            {
                currentWorldPos = GridToWorld(path[1]);
            }
        }



        private void Start()
        {
            sensor = this.GetComponent<AISensorBase>();
            bindCharacterCtrl = this.GetComponent<CharacterCtrlBase>();

            speed = bindCharacterCtrl.MaxSpeed.GetValue();
            
        }

        void MoveToNextPoint()
        {
            currentIndex++;

            // 路径结束检查
            if (currentIndex >= path.Count)
            {
                isMoving = false;
                return;
            }
            else
            {
                // 设置下一个目标世界坐标
                currentWorldPos = GridToWorld(path[currentIndex]);
            }
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
            }else if (name == "FollowPath")
            {
                if( spawnRootInfo == null)
                {
                    SpawnRootInfo sp;
                    if(LevelGridGenerator.Instance.spawnroot_dictionay.TryGetValue(followPath , out sp))
                    {
                        SetFollowPath(sp);
                        
                    }
                }
                // 计算移动
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    currentWorldPos,
                    speed * Time.deltaTime
                );
                // 检查是否到达当前目标点
                if (Vector3.Distance(transform.position, currentWorldPos) < 0.01f)
                {
                    // 移动到下一个路径点
                    MoveToNextPoint();
                }
            }else if ( name == "AttackTarget")
            {
                string toDic = properties[0];
                CharacterCtrlBase characterCtrlBase;
                if (mCharacterDict.TryGetValue( toDic, out characterCtrlBase))
                {
                    if (characterCtrlBase != null)
                    {
                        (bindCharacterCtrl as PlayerCharacterCtrl).Attack(characterCtrlBase);
                    }
                }
                return false;
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
            else if (name == "IsFloatSmaller")
            {
                if (bindCharacterCtrl.AttributesDicts.ContainsKey(properties[0]) )
                {
                    return (this.bindCharacterCtrl.AttributesDicts[properties[0]] as Character_Float).GetValue() < float.Parse(properties[1]);
                }
            }
            else if (name == "IsFloatBigger")
            {
                if (bindCharacterCtrl.AttributesDicts.ContainsKey(properties[0]))
                {
                    return (this.bindCharacterCtrl.AttributesDicts[properties[0]] as Character_Float).GetValue() > float.Parse(properties[1]);
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
