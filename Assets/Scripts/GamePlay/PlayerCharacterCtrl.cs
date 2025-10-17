using Assets.Scripts.BaseUtils;
using Assets.Scripts.Core;
using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterCtrl : CharacterCtrlBase
{


    public Vector3Int MySelectTarget;

    public Character_Bool IsFOVLock = new Character_Bool("IsFOVLock", false);
    public Character_Float targetCameraFOV = new Character_Float("targetCameraFOV", 10F);

    public Animator spriteAnimator;

    public Character_Float mAttackCD_Cur = new Character_Float("mAttackCD_Cur", -1);
    public Character_Float mAttackCDReduce = new Character_Float("mAttackCDReduce", 1);
    public Character_Float mAttackCD = new Character_Float("mAttackCD", 0);

    public Character_Int myAttackSkillID = new Character_Int("myAttackSkillID", 7);



    // Update is called once per frame
    private void Update()
    {

        UpdateAnimationStates();

        if (mAttackCD_Cur.real_value > 0)
        {
            mAttackCD_Cur.real_value -= mAttackCDReduce.GetValue() * Time.deltaTime;
            mAttackCD_Cur.real_value = Math.Max(0, mAttackCD_Cur.real_value);
        }
        if(transform.name.StartsWith( "CC_Object_Tower" ))
        {

            Debug.Log(string.Format( "{0} {1} {2}" , transform.name , Time.frameCount.ToString() , mAttackCD_Cur.GetValue()));
        }
    }


    public bool Attack(CharacterCtrlBase target)
    {
        mAttackCD_Cur.real_value = mAttackCD.GetValue();

        SkillUseInfo skinfo = new SkillUseInfo();
        skinfo.SkillID = myAttackSkillID.GetValue();
        skinfo.AimTarget = target;
        //skinfo.SkillCastPos = transform.position;
        StartUseSkill(skinfo);

        return true;
    }

    void UpdateAnimationStates()
    {
        if(this.spriteAnimator == null)
        {
            return;
        }
    }

    private void Start()
    {
        base.Start();

        mAttackCD_Cur.TakeEffect(this);
        mAttackCD.TakeEffect(this);
        mAttackCDReduce.TakeEffect(this);

    }

    public void AddSkill(int SkillID)
    {

    }

    public void RemoveSkill(int SkillID) 
    {
        
    }

    private void Awake()
    {
        base.Awake();

        IsFOVLock.TakeEffect(this);
        targetCameraFOV.TakeEffect(this);


    }

    public void CancelUseSkill(SkillUseInfo skillUseInfo)
    {

        Game2D_GamePlayEvent evt = new Game2D_GamePlayEvent(EventType_Game2DPlayEvent.CharacterCancelSkill, this.gameObject);
        evt.event_param_dics.Add("MainCharacter", this);
        evt.event_param_dics.Add("AimDirX", skillUseInfo.SkillDispatchDir.normalized.x);
        evt.event_param_dics.Add("AimDirY", skillUseInfo.SkillDispatchDir.normalized.y);
        evt.event_param_dics.Add("MoveDirX", rb.velocity.x);
        evt.event_param_dics.Add("MoveDirY", rb.velocity.y);
        evt.event_param_dics.Add("InputDirX", UI_VirtualInput.instance.GetDir("Left").x);
        evt.event_param_dics.Add("InputDirY", UI_VirtualInput.instance.GetDir("Left").y);

        LevelEventQueue.Instance.EnqueueEvent(evt);
    }

    public void StartUseSkill(SkillUseInfo skillUseInfo)
    {
        GameSkills skillConfig = GameTableConfig.Instance.Config_GameSkills.FindFirstLine( (a) => a.SkillID == skillUseInfo.SkillID);
        if (skillConfig != null) 
        {
            SkillDispatchCenter.Instance.AddModifierToCharacter(this, skillConfig.ModifierDuration, skillConfig.ModifierID, skillUseInfo);
            if (skillConfig.CastImmediate)
            {
                CastSkill(skillUseInfo);
            }
        }
    }

    public void CastSkill(SkillUseInfo skillUseInfo)
    {
        //this.rb.velocity = skillUseInfo.SkillDispatchDir.normalized * XuliPower.GetValue() * XuLiCnt;
        Game2D_GamePlayEvent evt = new Game2D_GamePlayEvent(EventType_Game2DPlayEvent.CharacterCastSkill, this.gameObject);
        evt.event_param_dics.Add("MainCharacter" , this);
        evt.event_param_dics.Add("AimDirX", skillUseInfo.SkillDispatchDir.normalized.x);
        evt.event_param_dics.Add("AimDirY", skillUseInfo.SkillDispatchDir.normalized.y);
        evt.event_param_dics.Add("MoveDirX" , rb.velocity.x);
        evt.event_param_dics.Add("MoveDirY", rb.velocity.y);
        evt.event_param_dics.Add("InputDirX", UI_VirtualInput.instance.GetDir("Left").x);
        evt.event_param_dics.Add("InputDirY", UI_VirtualInput.instance.GetDir("Left").y);
        evt.event_param_dics.Add("CastPosX" , rb.transform.position.x);
        evt.event_param_dics.Add("CastPosY" , rb.transform.position.y);

        evt.event_param_dics.Add("SelectPosIntX", this.MySelectTarget.x);
        evt.event_param_dics.Add("SelectPosIntY", this.MySelectTarget.y);

        evt.event_param_dics.Add("AimTarget", skillUseInfo.AimTarget);
        Debug.Log(this.MySelectTarget.x);

        LevelEventQueue.Instance.EnqueueEvent(evt);

        //SkillDispatchCenter.Instance.AddModifierToCharacter(this, 1, 1, skillUseInfo);
        

    }


}
