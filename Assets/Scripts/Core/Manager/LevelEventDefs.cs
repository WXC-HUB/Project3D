using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public enum EventType_Game2DCMD
    {
        GoNextPlayer
    }
    
    public enum EventType_Game2DPlayEvent
    {
        CharacterEndMove , 
        CharacterIsReady ,
        CharacterDoCollide,
        CharacterBeCollide,
        CharacterBeHit,
        CharacterDoHit,
        CharacterCastSkill,
        CharacterXuLiStageUp,
        CharacterCancelSkill,
        CharacterDie,

        ModifierLifeTime_Start,
        ModifierLifeTime_End,
        ModifierLifeTime_Tick
    }

    public enum EventType_VirtualInputEvent
    {
        ThumbEndDrag,
        ThumbKeepDragging,
        ThumbStartDrag,
        ThumbCancelDrag,
        ThumbClick,
        SkillButtonClick
    }


    public class Game2D_GamePlayEvent : BaseEventArgs
    {
        public SkillUseInfo skillinfo = null;
        public CharacterCtrlBase beCharacter = null;
        public CharacterCtrlBase doCharacter = null;

        public CharacterModifier fromModifier = null;

        public Dictionary<string, object> event_param_dics = new Dictionary<string, object>();

        public Game2D_GamePlayEvent(EventType_Game2DPlayEvent _t, GameObject _sender , CharacterModifier fromModifier = null) : base(_t, _sender)
        {
            this.fromModifier = fromModifier;
        }

        public static T GetEventValue<T>(Game2D_GamePlayEvent from_event ,  string key , Func<string , T> when_not )  
        {
            if(from_event == null)
            {
                return when_not(key);
            }
            if (!from_event.event_param_dics.ContainsKey(key) )
            {
                //Debug.LogWarning("Game2D_GamePlayEvent：未知的变量类型！" + key);
                return when_not(key);
            }
            
            return (T)from_event.event_param_dics[key] ;
        
        }
    }


    public class Game2D_CMD_Event : BaseEventArgs
    {
        public Game2D_CMD_Event(EventType_VirtualInputEvent _t, GameObject _sender) : base(_t, _sender) 
        {
        
        }
    }

    public class VirtualInputEvnetArgs : BaseEventArgs
    {
        public string InputOperKey;
        public Vector2 InputDir;
        public VirtualInputEvnetArgs(EventType_VirtualInputEvent _t , GameObject _sender , string InputOperKey , Vector2 InputDir ) : base(_t , _sender)
        {
            this.InputOperKey = InputOperKey;
            this.InputDir = InputDir;
        }
    }

}
