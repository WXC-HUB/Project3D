using Assets.Scripts.BaseUtils;
using Assets.Scripts.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterCtrl : CharacterCtrlBase
{
    public float XuLiProgress = 0.0f;
    public float XuLiSpeed = 1f;
    public int XuLiCnt = 0;

    public Vector3Int MySelectTarget;

    public Character_Float XuliPower = new Character_Float("XuliPower", 10f);

    public Character_Bool isXuLi = new Character_Bool("IsXuLi" , false);

    
    public float NaiLi_Cur = 100f;
    public Character_Bool canXuLi = new Character_Bool("CanXuLi" , true);
    public Character_Float NaiLi_Max = new Character_Float("NaiLi_Max", 100f);
    public Character_Float NaiLi_Reduce = new Character_Float("NaiLi_Reduce", 0f);
    public Character_Float NaiLi_Regain = new Character_Float("NaiLi_Regain", 1f);

    //public int XuliSkill = 1;
    public Character_Int XuliSkill = new Character_Int("XuliSkill", 1);
    public Character_Int ClickSkill = new Character_Int("ClickSkill", 3);
    //public int TapSkill = 3;

    public Character_Bool IsAnimationRoll = new Character_Bool("IsAnimationRoll", false);

    public Character_Bool IsFOVLock = new Character_Bool("IsFOVLock", false);
    public Character_Float targetCameraFOV = new Character_Float("targetCameraFOV", 10F);

    public Animator spriteAnimator;

    // Update is called once per frame
    private void Update()
    {
        this.NaiLi_Cur = this.NaiLi_Cur + (this.NaiLi_Regain.GetValue() - this.NaiLi_Reduce.GetValue()) * Time.deltaTime;
        this.NaiLi_Cur = Mathf.Min(NaiLi_Max.GetValue(), this.NaiLi_Cur);

        UpdateAnimationStates();
        
    }

    void UpdateAnimationStates()
    {
        if(this.spriteAnimator == null)
        {
            return;
        }

        spriteAnimator.SetBool("IsRoll" , this.IsAnimationRoll.GetValue());
    }

    private void Start()
    {
        base.Start();
        UI_PlayerHUD.instance.SetSkillFocusPlayer(this);
        

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
        XuliPower.TakeEffect(this);
        canXuLi.TakeEffect(this);
        isXuLi.TakeEffect(this);

        NaiLi_Max.TakeEffect(this);
        NaiLi_Reduce.TakeEffect(this);
        NaiLi_Regain.TakeEffect(this);

        XuliSkill.TakeEffect(this); 
        ClickSkill.TakeEffect(this);

        IsAnimationRoll.TakeEffect(this);   

       

        IsFOVLock.TakeEffect(this);
        targetCameraFOV.TakeEffect(this);


    }

    public float getProtMoveLength()
    {
        float start_speed = 10 * ( XuLiCnt + XuLiProgress);
        float end_speed = MaxSpeed.GetValue();

        float range_speed = start_speed - end_speed;

        float res = (start_speed / 2) * (range_speed / LinerDrag.GetValue());

        return Mathf.Min(res, 20f);

    }
    public void DoXuli()
    {
        XuLiProgress += XuLiSpeed * Time.deltaTime;
        if (XuLiCnt <= 3)
        {
            if (XuLiProgress >= 1f)
            {
                XuLiCnt += 1;
                XuLiProgress = 0f;

                Game2D_GamePlayEvent evt = new Game2D_GamePlayEvent(EventType_Game2DPlayEvent.CharacterXuLiStageUp, this.gameObject);
                evt.event_param_dics.Add("MainCharacter", this);

                LevelEventQueue.Instance.EnqueueEvent(evt);

            }
        }
    }

    public void CancelUseSkill(SkillUseInfo skillUseInfo)
    {
        XuLiProgress = 0;
        XuLiCnt = 0;
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
        XuLiProgress = 0;
        XuLiCnt = 0;
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
        Debug.Log(this.MySelectTarget.x);

        LevelEventQueue.Instance.EnqueueEvent(evt);

        //SkillDispatchCenter.Instance.AddModifierToCharacter(this, 1, 1, skillUseInfo);
        

    }


}
