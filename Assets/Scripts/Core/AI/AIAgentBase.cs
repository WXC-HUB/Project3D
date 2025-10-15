using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using UnityEngine;
using Defective.JSON;

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

        private void Start()
        {
            sensor = this.GetComponent<AISensorBase>();
            curTree = AIUtiles.LoadBTreeFromFile("BehaviorTreeConfig/Tower1" , this);
            bindCharacterCtrl = this.GetComponent<CharacterCtrlBase>(); 
        }

        public bool DoAction( BTNode tNode )
        {
            if( tNode.name == "GetCharacterToDic")
            {
                CharacterCtrlBase resCtrl = sensor.getCharacterByKey(tNode.properties.getType);
                if (mCharacterDict.ContainsKey(tNode.properties.toDic)) 
                {
                    mCharacterDict[tNode.properties.toDic] = resCtrl;
                }
                else
                {
                    mCharacterDict.Add(tNode.properties.toDic, resCtrl);
                }
            }

            Debug.LogError(String.Format("未知的AI Action：{0} {1}", tNode.name, tNode.id));
            return false;
        }

        public bool DoCondition(BTNode tNode)
        {
            if (tNode.name == "IsBool")
            {
                if( bindCharacterCtrl.AttributesDicts.ContainsKey(tNode.properties.pName))
                {
                    return Boolean.Parse(tNode.properties.pValue) 
                        == (this.bindCharacterCtrl.AttributesDicts[tNode.properties.pName] as Character_Bool).GetValue();
                }
                else
                {
                    return false;
                }
            }
            else if (tNode.name == "IsFloat")
            {
                if (bindCharacterCtrl.AttributesDicts.ContainsKey(tNode.properties.pName))
                {
                    return MathF.Abs(float.Parse(tNode.properties.pValue) -
                        (this.bindCharacterCtrl.AttributesDicts[tNode.properties.pName] as Character_Float).GetValue() ) <= 0.001f ; 
                }
            }
            else if (tNode.name == "HasCharacter")
            {
                return mCharacterDict.ContainsKey(tNode.properties.pName) && mCharacterDict[tNode.properties.pName] != null;
            }
            else if(tNode.name == "NearPos")
            {
                if( mPosDict.ContainsKey(tNode.properties.pName) && mPosDict[tNode.properties.pName] != null)
                {
                    return ((this.bindCharacterCtrl.AttributesDicts[tNode.properties.pName] as Character_Vector2).GetValue()
                        - this.mPosDict[tNode.properties.pName]).magnitude <= 0.001f;
                }

                
            }

            Debug.LogError(String.Format("未知的AI Condition：{0} {1}", tNode.name, tNode.id));
            return false;
        }

       
        private void Update()
        {
            if (state == AIAgentState.Running  && curTree != null)
            {
                curTree.RunNode();
            }
        }
    }
}
