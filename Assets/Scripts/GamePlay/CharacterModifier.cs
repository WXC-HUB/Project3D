using Assets.Scripts.BaseUtils;
using Assets.Scripts.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;


public class SkillDispatchCenter : Singleton<SkillDispatchCenter>
{
    public void AddModifierToCharacter( CharacterCtrlBase toCharacter , float duration , int ModifierID , SkillUseInfo skillinfo = null)
    {
        List<GameModifiers> config_effects = GameTableConfig.Instance.Config_GameModifiers.FindAllLine
            (
                (GameModifiers x) => x.ModifierID == ModifierID
            );


        CharacterModifier newMod = toCharacter.gameObject.AddComponent<CharacterModifier>();
        newMod.duration = duration;
        newMod.isPermant = (duration <= 0f);
        newMod.ModifierID = ModifierID;
        for (int i = 0; i < config_effects.Count; i++) 
        {
            newMod.TakeEffect(config_effects[i] , skillinfo);    
        }

        newMod.ModifierStart();

    }

    public void DoGameAction( string actionType , SkillUseInfo skill_useinfo = null ,  CharacterCtrlBase from_character = null , CharacterCtrlBase to_character = null , List<string> action_params = null , Game2D_GamePlayEvent from_event = null )
    {
        if( actionType == "SpawnField")
        {
            Vector2 spawn_pos = new Vector2(
                Game2D_GamePlayEvent.GetEventValue<float>(from_event , action_params[1] , (string x) => (float)Convert.ToDouble(x)),
                Game2D_GamePlayEvent.GetEventValue<float>(from_event, action_params[2] , (string x) => (float)Convert.ToDouble(x))
            );
            int spwan_id = Convert.ToInt32( action_params[0]);
            GameFields gameFieldConfig = GameTableConfig.Instance.Config_GameFields.FindFirstLine((GameFields gf) => gf.FieldID == spwan_id);

            if(gameFieldConfig == null)
            {
                Debug.LogError("没有找到场配置！" + spwan_id.ToString());
            }
            else
            {
                GameObject ori_go = Resources.Load<GameObject>("FieldPrefabs/" + gameFieldConfig.FieldPrefabName) as GameObject;
                GameObject newgo = GameObject.Instantiate(ori_go , spawn_pos , new Quaternion() );
                newgo.transform.parent = LevelManager.Instance.transform    ;

                newgo.GetComponent<GameFieldBase>().InitField();
            }

        }
        else if (actionType == "DispelModifier")
        {
            int BuffType = Convert.ToInt32(action_params[0]);
            CharacterCtrlBase target = Game2D_GamePlayEvent.GetEventValue<CharacterCtrlBase>(from_event , action_params[1] , (string a)=>null);
            CharacterModifier[] targetlist = target.GetComponents<CharacterModifier>();
            for(int i = 0; i < targetlist.Length; i++)
            {
                if (targetlist[i].ModifierID == BuffType)
                {
                    targetlist[i].ModifierDispel();
                }
            }   
        }
        else if (actionType == "AddForce")
        {
            CharacterCtrlBase target = to_character != null ? to_character : Game2D_GamePlayEvent.GetEventValue<CharacterCtrlBase>(from_event , action_params[0], (string a) => null);
            Vector2 direction = new Vector2(
               Game2D_GamePlayEvent.GetEventValue<float>(from_event, action_params[1], (string x) => (float)Convert.ToDouble(x)),
               Game2D_GamePlayEvent.GetEventValue<float>(from_event , action_params[2], (string x) => (float)Convert.ToDouble(x))
            ).normalized;
            float force = Game2D_GamePlayEvent.GetEventValue<float>(from_event , action_params[3], (string x) => (float)Convert.ToDouble(x));
            bool isIgnore = Game2D_GamePlayEvent.GetEventValue<bool>(from_event, action_params[4], (string x) => (bool)Convert.ToBoolean(x));
            target.AddForce(direction, force , isIgnore);

        }
        else if(actionType == "DoDamage")
        {
            //ModifierLifeTime_End|DoDamage|MainCharacter|99999
            CharacterCtrlBase target = to_character != null ? to_character : Game2D_GamePlayEvent.GetEventValue<CharacterCtrlBase>(from_event, action_params[0], (string a) => null);
            int dmg_value = Game2D_GamePlayEvent.GetEventValue<int>(from_event, action_params[1], (string a) => (int)Convert.ToInt32(a));

            target.TakeDamage(dmg_value ,  skill_useinfo);
        }
        else if(actionType == "AddModifier")
        {
            int targetid = Game2D_GamePlayEvent.GetEventValue<int>(from_event, action_params[0], (string a) => (int)Convert.ToInt32(a)); 
            CharacterCtrlBase target = to_character != null ? to_character : 
                Game2D_GamePlayEvent.GetEventValue<CharacterCtrlBase>(from_event, action_params[1], (string a) => null);
            float target_duration = Game2D_GamePlayEvent.GetEventValue<float>(from_event, action_params[2], (string a) => (float)Convert.ToDouble(a)); ;

            SkillDispatchCenter.Instance.AddModifierToCharacter(target, target_duration, targetid, skill_useinfo);
        }
        else if (actionType == "SpawnVFX")
        {
            string prefab_name = Game2D_GamePlayEvent.GetEventValue<string>(from_event, action_params[0], (string a) => (a));
            CharacterCtrlBase parent_ctrl = Game2D_GamePlayEvent.GetEventValue<CharacterCtrlBase>(from_event, action_params[1], (string a) => null);

            if (parent_ctrl != null) 
            {
                GameObject vfxobj = Resources.Load<GameObject>("VFXPrefabs/" + prefab_name);
                GameObject spawn_obj = GameObject.Instantiate(vfxobj, parent_ctrl.transform);
            }        
        }
        else if(actionType == "SetAnimationParam")
        {
            //OnModifierStart|SetAnimationParam|MainCharacter|bool|IsCountingDown|true
            CharacterCtrlBase parent_ctrl = Game2D_GamePlayEvent.GetEventValue<CharacterCtrlBase>(from_event, action_params[0], (string a) => null);
            foreach (var item in from_event.event_param_dics)
            {
                Debug.Log(123);
                Debug.Log(from_event.event_param_dics);
                Debug.Log(item.Value );
            }
            Animator parent_animator = parent_ctrl.GetComponent<Animator>();
            
            string pType = Game2D_GamePlayEvent.GetEventValue<string>(from_event, action_params[1], (string a) => (a)).ToLower();
            string pName = Game2D_GamePlayEvent.GetEventValue<string>(from_event, action_params[2], (string a) => (a));
            string pValue = Game2D_GamePlayEvent.GetEventValue<string>(from_event, action_params[3], (string a) => (a));

            if (parent_ctrl != null) 
            {
                if(parent_animator != null)
                {
                    if(pType == "bool")
                    {
                        parent_animator.SetBool(pName, Convert.ToBoolean(pValue));
                    }
                    else if(pType == "float")
                    {
                        parent_animator.SetFloat(pName, Convert.ToSingle(pValue));
                    }
                    else if(pType == "trigger")
                    {
                        parent_animator.SetTrigger(pName);
                    }
                    else
                    {
                        Debug.LogError("未知的变量类型！" + string.Join("|", action_params));
                    }
                }
                else
                {
                    Debug.LogError("对象身上没有找到动画状态机！" + string.Join("|", action_params));
                }

            }
            else
            {
                Debug.LogError("设置动画对象为Null" + string.Join("|", action_params));
            }

        }
        else if (actionType == "SpawnInGameObjectToTile")
        {

            Vector3Int spawn_pos_tile = new Vector3Int(
                Game2D_GamePlayEvent.GetEventValue<int>(from_event, action_params[3], (string x) => (int)Convert.ToInt64(x)),
                Game2D_GamePlayEvent.GetEventValue<int>(from_event, action_params[4], (string x) => (int)Convert.ToInt64(x)),
                0
            );

            Vector3 spawn_pos = LevelGridGenerator.Instance.tilemap.GetCellCenterWorld(spawn_pos_tile);

            int spwan_id = Convert.ToInt32(action_params[0]);

            CharacterCtrlBase spawn_char = LevelManager.Instance.SpawnCharacterByID<CharacterCtrlBase>(spwan_id );

            Debug.Log(spawn_char);
            if (spawn_char != null)
            {
                LevelGridGenerator.Instance.TryAttach(spawn_pos_tile, spawn_char);
            }

        }else if (actionType == "ShootToTarget")
        {
            int bullet_type = Game2D_GamePlayEvent.GetEventValue<int>(from_event, action_params[0], (string x) => (int)Convert.ToInt64(x));
            CharacterCtrlBase from_char = Game2D_GamePlayEvent.GetEventValue<CharacterCtrlBase>(from_event, action_params[1], (string x) => null);
            CharacterCtrlBase to_char = Game2D_GamePlayEvent.GetEventValue<CharacterCtrlBase>(from_event, action_params[2], (string x) => null);

            PlayerCharacterCtrl bullet_ctrl = LevelManager.Instance.SpawnCharacterByID<PlayerCharacterCtrl>(bullet_type);
            bullet_ctrl.followTarget = to_char;
            bullet_ctrl.from_char = from_char;
            bullet_ctrl.transform.position = from_char.transform.position;  
        }

    }
}


public class CharacterModifier : MonoBehaviour
{
    public string ModName;

    public int ModifierID;

    public bool isModifierActive = false;
    public bool isPermant = false;
    public CharacterCtrlBase characterCtrl;
    public float duration;

    public List<CharacterAttributeDect> myDects = new List<CharacterAttributeDect>();

    public Dictionary<CharacterAttributeDect, string> Input_Sync_Dect = new Dictionary<CharacterAttributeDect, string>();

    public Dictionary<EventType_Game2DPlayEvent, List<List<string>>> listen_gameevent_list = new Dictionary<EventType_Game2DPlayEvent, List<List<string>>>();
    public Dictionary<EventType_Game2DPlayEvent, List<SkillUseInfo>> skill_info_list = new Dictionary<EventType_Game2DPlayEvent, List<SkillUseInfo>>();

    public Dictionary<EventType_Game2DPlayEvent, List<List<string>>> lifetime_event_list = new Dictionary<EventType_Game2DPlayEvent, List<List<string>>>();
    public Dictionary<EventType_Game2DPlayEvent, List<SkillUseInfo>> lifetime_skill_info_list = new Dictionary<EventType_Game2DPlayEvent, List<SkillUseInfo>>();


    public List<VFXBase> myVFXs = new List<VFXBase>();
    
    void TakeEffect_AddSimpleDect(GameModifiers effect, SkillUseInfo skillInfo)
    {
        if (effect.ModifierParam.Count <= 0)
        {
            Debug.LogError("AddSimpleDect 无参数！");
        }

        CharacterAttributeDect add_dect = null;

        if (effect.ModifierParam[0] == "bool_set")
        {
            add_dect = gameObject.AddComponent<Character_DECT_Bool_Set>();
            (add_dect as Character_DECT_Bool_Set).effect_factor = Convert.ToBoolean(effect.ModifierParam[2]);
            (add_dect as Character_DECT_Bool_Set).attribute_name = effect.ModifierParam[1];
        }
        else if(effect.ModifierParam[0] == "float_add")
        {
            add_dect = gameObject.AddComponent<Character_DECT_Float_Add>();
            (add_dect as Character_DECT_Float_Add).effect_factor = (float)Convert.ToDouble(effect.ModifierParam[2]);
            (add_dect as Character_DECT_Float_Add).attribute_name = effect.ModifierParam[1];
        }
        else if (effect.ModifierParam[0] == "float_set")
        {
            add_dect = gameObject.AddComponent<Character_DECT_Float_Set>();
            (add_dect as Character_DECT_Float_Set).effect_factor = (float)Convert.ToDouble(effect.ModifierParam[2]);
            (add_dect as Character_DECT_Float_Set).attribute_name = effect.ModifierParam[1];
        }
        else if (effect.ModifierParam[0] == "float_mul")
        {
            add_dect = gameObject.AddComponent<Character_DECT_Float_Mul>();
            (add_dect as Character_DECT_Float_Mul).effect_factor = (float)Convert.ToDouble(effect.ModifierParam[2]);
            (add_dect as Character_DECT_Float_Mul).attribute_name = effect.ModifierParam[1];
        }
        else if (effect.ModifierParam[0] == "Vector2_set")
        {
            add_dect = gameObject.AddComponent<Character_DECT_Vector2_Set>();
            string[] vec = effect.ModifierParam[2].Split(',');
            (add_dect as Character_DECT_Vector2_Set).effect_factor = 
                new Vector2((float)Convert.ToDouble(vec[0])  , (float)Convert.ToDouble(vec[1]) );
            (add_dect as Character_DECT_Vector2_Set).attribute_name = effect.ModifierParam[1];
        }
        else if (effect.ModifierParam[0] == "Vector2_add")
        {
            add_dect = gameObject.AddComponent<Character_DECT_Vector2_Add>();
            string[] vec = effect.ModifierParam[2].Split(',');
            (add_dect as Character_DECT_Vector2_Add).effect_factor =
                new Vector2((float)Convert.ToDouble(vec[0]), (float)Convert.ToDouble(vec[1]));
            (add_dect as Character_DECT_Vector2_Add).attribute_name = effect.ModifierParam[1];
        }

        if (add_dect != null)
        {
            myDects.Add(add_dect);
        }
        else
        {
            Debug.LogError("AddSimpleDect 未知类型" + effect.ModifierParam[0]);
        }
    }

    public void TakeEffect_SyncInputValue(GameModifiers effect, SkillUseInfo skillInfo)
    {
        if (effect.ModifierParam.Count <= 0)
        {
            Debug.LogError("SyncInputValue 无参数！");
        }

        CharacterAttributeDect add_dect = null;

        if (effect.ModifierParam[0] == "Vector2_set")
        {
             add_dect = gameObject.AddComponent<Character_DECT_Vector2_Set>();
            (add_dect as Character_DECT_Vector2_Set).attribute_name = effect.ModifierParam[1];

        }else if(effect.ModifierParam[0] == "Vector2_add")
        {
             add_dect = gameObject.AddComponent<Character_DECT_Vector2_Add>();
            (add_dect as Character_DECT_Vector2_Add).attribute_name = effect.ModifierParam[1];

        }

        if (add_dect != null)
        {
            myDects.Add(add_dect);
            if( Input_Sync_Dect.ContainsKey(add_dect))
            {
                Input_Sync_Dect[add_dect] = effect.ModifierParam[2];
            }
            else
            {
                Input_Sync_Dect.Add(add_dect, effect.ModifierParam[2]);
            }

        }
        else
        {
            Debug.LogError("SyncInputValue 未知类型" + effect.ModifierParam[0]);
        }

    }


    public void TakeEffect(GameModifiers effect , SkillUseInfo skillInfo)
    {
        if (effect.ActType == "AddSimpleDect")
        {
            TakeEffect_AddSimpleDect(effect, skillInfo);
        }else if (effect.ActType == "SyncInputValue")
        {
            TakeEffect_SyncInputValue(effect, skillInfo);
        }else if (effect.ActType == "MoveRandomlyTemp")
        {
            CharacterAttributeDect add_dect;
            add_dect = gameObject.AddComponent<Character_DECT_Vector2_Set>();
            (add_dect as Character_DECT_Vector2_Set).attribute_name = effect.ModifierParam[1];

            myDects.Add(add_dect);
            if (Input_Sync_Dect.ContainsKey(add_dect))
            {
                Input_Sync_Dect[add_dect] = "TempRandom";
            }
            else
            {
                Input_Sync_Dect.Add(add_dect, "TempRandom");
            }
        }
        else if (effect.ActType == "ListenGameEvent")
        {
            EventType_Game2DPlayEvent EventType = 
                (EventType_Game2DPlayEvent)Enum.Parse(typeof(EventType_Game2DPlayEvent), effect.ModifierParam[0]);

            if (!listen_gameevent_list.ContainsKey(EventType))
            {
                LevelEvnetManager.Instance.AddListener(EventType, OnTriggerEvent);
            }

            if (  this.listen_gameevent_list.ContainsKey( EventType ))
            {
                this.listen_gameevent_list[EventType].Add(effect.ModifierParam);
            }
            else
            {
                this.listen_gameevent_list.Add(EventType, new List<List<string>> { effect.ModifierParam });
            }

            if (this.skill_info_list.ContainsKey(EventType))
            {
                this.skill_info_list[EventType].Add(skillInfo);
            }
            else
            {
                this.skill_info_list.Add(EventType, new List<SkillUseInfo> { skillInfo });
            }         
            
        }
        else if (effect.ActType == "LifeTimeEvent")
        {
            EventType_Game2DPlayEvent EventType =
                (EventType_Game2DPlayEvent)Enum.Parse(typeof(EventType_Game2DPlayEvent), effect.ModifierParam[0]);

            if (!lifetime_event_list.ContainsKey(EventType))
            {
                LevelEvnetManager.Instance.AddListener(EventType, OnTriggerLifeTimeEvent);
            }
            if (this.lifetime_event_list.ContainsKey(EventType))
            {
                this.lifetime_event_list[EventType].Add(effect.ModifierParam);
            }
            else
            {
                this.lifetime_event_list.Add(EventType, new List<List<string>> { effect.ModifierParam });
            }

            if (this.lifetime_skill_info_list.ContainsKey(EventType))
            {
                this.lifetime_skill_info_list[EventType].Add(skillInfo);
            }
            else
            {
                this.lifetime_skill_info_list.Add(EventType, new List<SkillUseInfo> { skillInfo });
            }
            
            
        }
        else if(effect.ActType == "SpawnVFX")
        {
            GameObject vfxobj = Resources.Load<GameObject>("VFXPrefabs/" + effect.ModifierParam[0]);
            GameObject spawn_obj = Instantiate(vfxobj , parent:this.transform );
        }

        else if(effect.ActType == "BindVFX")
        {
            GameObject vfxobj = Resources.Load<GameObject>("VFXPrefabs/" + effect.ModifierParam[0]);
            GameObject spawn_obj = Instantiate(vfxobj, parent: this.transform);

            if (vfxobj != null && vfxobj.GetComponent<VFXBase>() != null)
            {
                myVFXs.Add(spawn_obj.GetComponent<VFXBase>());
            }
        }
    }

    void OnTriggerLifeTimeEvent(BaseEventArgs aa)
    {
        Game2D_GamePlayEvent gameEvent = (Game2D_GamePlayEvent)aa;
        if (gameEvent.sender != this.gameObject || gameEvent.fromModifier != this) return;

        List<List<string>> param_lists = this.lifetime_event_list[(EventType_Game2DPlayEvent)aa.m_Type];

        for (int i = 0; i < param_lists.Count; i++)
        {

            SkillDispatchCenter.Instance.DoGameAction(
                    actionType: param_lists[i][1],
                    skill_useinfo: lifetime_skill_info_list[(EventType_Game2DPlayEvent)aa.m_Type][i],
                    from_character: characterCtrl,
                    to_character: null, //这里先临时传个Null
                    action_params: param_lists[i].GetRange(2, param_lists[i].Count - 2),
                    from_event: aa as Game2D_GamePlayEvent
                );
        }

    }

    void OnTriggerEvent( BaseEventArgs aa)
    {
        if(aa.m_Type is EventType_Game2DPlayEvent.CharacterDie)
        {
            Debug.Log(aa.sender);
            Debug.Log(this.gameObject);
            Debug.Log("12122323");
        }
        if (aa.sender != this.gameObject) return;
        
        List<List<string>> param_lists = this.listen_gameevent_list[ (EventType_Game2DPlayEvent)aa.m_Type ];

        for(int i = 0; i < param_lists.Count; i++)
        {
           
            SkillDispatchCenter.Instance.DoGameAction(
                    actionType: param_lists[i][1],
                    skill_useinfo: skill_info_list[(EventType_Game2DPlayEvent)aa.m_Type][i],
                    from_character: characterCtrl,
                    to_character: null, //这里先临时传个Null
                    action_params: param_lists[i].GetRange(2, param_lists[i].Count-2),
                    from_event:aa as Game2D_GamePlayEvent
                );
        }
    }


    public void SyncInputDect() 
    {
        foreach (var pair in Input_Sync_Dect)
        {
            if(pair.Key is Character_DECT_Vector2_Set)
            {
                if( pair.Value == "TempRandom")
                {
                    (pair.Key as Character_DECT_Vector2_Set).effect_factor
                        = new Vector2((
                            (Time.timeSinceLevelLoad % 2) - 1),
                            (((Time.timeSinceLevelLoad + 186) % 3) - 1)).normalized;
                }
                else if (pair.Value.StartsWith("AIBoard_"))
                {

                }
                else
                {
                    (pair.Key as Character_DECT_Vector2_Set).effect_factor
                        = (UI_VirtualInput.instance).GetDir(pair.Value);
                }
                
            }
        }
    }

    public void ModifierStart()
    {
        //Debug.Log("Start:" + this.ModifierID.ToString());
        isModifierActive = true;
        this.characterCtrl = GetComponent<CharacterCtrlBase>();   

        Game2D_GamePlayEvent modEvent = new Game2D_GamePlayEvent(EventType_Game2DPlayEvent.ModifierLifeTime_Start, this.gameObject, fromModifier: this);
        modEvent.event_param_dics.Add("MainCharacter", this.characterCtrl);
        LevelEvnetManager.Instance.EventDispatch(modEvent);
    }

    public void ModifierQuit()
    {
        //Debug.Log("Quit:" + this.ModifierID.ToString());

        Game2D_GamePlayEvent modEvent = new Game2D_GamePlayEvent(EventType_Game2DPlayEvent.ModifierLifeTime_End, this.gameObject, fromModifier: this);
        modEvent.event_param_dics.Add("MainCharacter", this.characterCtrl);
        LevelEvnetManager.Instance.EventDispatch(modEvent);

        LevelEvnetManager.Instance.DelListener(this.OnTriggerEvent);
        LevelEvnetManager.Instance.DelListener(this.OnTriggerLifeTimeEvent);
        foreach (var item in myDects)
        {
            Component.Destroy(item);
        }


        foreach (var item in myVFXs)
        {
            item.CallDestroy();
        }

        Component.Destroy(this);
    }

    public void ModifierDispel()
    {
        //Debug.Log("Dispel:" + this.ModifierID.ToString());

        Game2D_GamePlayEvent modEvent = new Game2D_GamePlayEvent(EventType_Game2DPlayEvent.ModifierLifeTime_End, this.gameObject, fromModifier: this);
        modEvent.event_param_dics.Add("MainCharacter", this.characterCtrl);
        LevelEvnetManager.Instance.EventDispatch(modEvent);

        LevelEvnetManager.Instance.DelListener(this.OnTriggerEvent);
        LevelEvnetManager.Instance.DelListener(this.OnTriggerLifeTimeEvent);
        foreach (var item in myDects)
        {
            Component.Destroy(item);
        }

        foreach(var item in myVFXs)
        {
            item.CallDestroy();
        }

        Component.Destroy(this);
    }

    private void Update()
    {
        if (isModifierActive && (!isPermant) )
        {
            duration -= Time.deltaTime; 
            if(duration <= 0 ) {ModifierQuit();}
        }

        SyncInputDect();    
    }
}
