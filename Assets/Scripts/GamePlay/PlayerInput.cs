using Assets.Scripts.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerInput : CharacterInputBase
{
    //public Transform AimTargetRoot = null;
    public Transform MoveDirRoot = null;

    
    public bool HaveSelectTarget = false;
    private float JudgeSelectRange = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        //AimTargetRoot = transform.Find("AimTargetRoot");
        MoveDirRoot = this.transform.Find("MoveDirRoot");

        LevelEvnetManager.Instance.AddListener(EventType_VirtualInputEvent.ThumbEndDrag, this.OnThumbUp);
        LevelEvnetManager.Instance.AddListener(EventType_VirtualInputEvent.ThumbStartDrag, this.OnThumbDown);
        LevelEvnetManager.Instance.AddListener(EventType_VirtualInputEvent.ThumbKeepDragging, this.OnThumbKeep);
        LevelEvnetManager.Instance.AddListener(EventType_VirtualInputEvent.ThumbCancelDrag, this.OnThumbCancel);
        LevelEvnetManager.Instance.AddListener(EventType_VirtualInputEvent.ThumbClick, this.OnThumbClick);

        LevelEvnetManager.Instance.AddListener(EventType_VirtualInputEvent.SkillButtonClick, this.OnSKillButtonClick);

        //为角色添加移动监听和朝向监听Buff
        SkillDispatchCenter.Instance.AddModifierToCharacter(characterCtrl, -1, 2);

        //添加一个碰撞时爆炸的Buff
        //SkillDispatchCenter.Instance.AddModifierToCharacter(characterCtrl, -1, 1001);


    }

    public void OnSKillButtonClick(BaseEventArgs s)
    {
        VirtualInputEvnetArgs inputEvnet = (VirtualInputEvnetArgs)s;
        if (inputEvnet.InputOperKey == "m_Button_Skill1")
        {
            SkillUseInfo skinfo = new SkillUseInfo();
            skinfo.SkillID = 2;
            skinfo.SkillDispatchDir = inputEvnet.InputDir;
            //skinfo.SkillCastPos = transform.position;
            characterCtrl.StartUseSkill(skinfo);
        }
        if (inputEvnet.InputOperKey == "m_Button_Skill2")
        {
            SkillUseInfo skinfo = new SkillUseInfo();
            skinfo.SkillID = 4;
            skinfo.SkillDispatchDir = inputEvnet.InputDir;
            //skinfo.SkillCastPos = transform.position;
            characterCtrl.StartUseSkill(skinfo);
        }

    }

    public void OnThumbClick(BaseEventArgs s)
    {
        VirtualInputEvnetArgs inputEvnet = (VirtualInputEvnetArgs)s;
        if (inputEvnet.InputOperKey == "Right")
        {
            SkillUseInfo skinfo = new SkillUseInfo();
            skinfo.SkillID = characterCtrl.ClickSkill.GetValue();
            skinfo.SkillDispatchDir = inputEvnet.InputDir;
            characterCtrl.StartUseSkill(skinfo);
        }
    }

    public void OnThumbDown(BaseEventArgs s)
    {
        if (!isInputEnable) return;

        VirtualInputEvnetArgs inputEvent = (VirtualInputEvnetArgs)s;

        if (inputEvent.InputOperKey == "Right")
        {
            SkillUseInfo skinfo = new SkillUseInfo();
            skinfo.SkillID = characterCtrl.XuliSkill.GetValue();
            skinfo.SkillDispatchDir = inputEvent.InputDir;
            characterCtrl.StartUseSkill(skinfo);
            //characterCtrl.isObserve = true;
        }
    }

    public void OnThumbUp(BaseEventArgs s)
    {
        if (!isInputEnable) return;

        VirtualInputEvnetArgs inputEvent = (VirtualInputEvnetArgs)s;

        if (inputEvent.InputOperKey == "Right")
        {
            SkillUseInfo skinfo = new SkillUseInfo();
            skinfo.SkillID = characterCtrl.XuliSkill.GetValue();
            skinfo.SkillDispatchDir = inputEvent.InputDir;
            characterCtrl.CastSkill(skinfo);
            //characterCtrl.isObserve = true;
        }
    }

    public void OnThumbKeep(BaseEventArgs s)
    {
        if (!isInputEnable) return;
        VirtualInputEvnetArgs inputEvent = (VirtualInputEvnetArgs)s;

        if (inputEvent.InputOperKey == "Right")
        {
            // characterCtrl.DoXuli();
        }
    }

    public void OnThumbCancel(BaseEventArgs s)
    {
        if (!isInputEnable) return;

        VirtualInputEvnetArgs inputEvent = (VirtualInputEvnetArgs)s;
        if (inputEvent.InputOperKey == "Right")
        {
            SkillUseInfo skinfo = new SkillUseInfo();
            skinfo.SkillID = characterCtrl.XuliSkill.GetValue();
            skinfo.SkillDispatchDir = inputEvent.InputDir;
            characterCtrl.CancelUseSkill(skinfo);
        }
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();

        //处理Item选中
        if (this.characterCtrl.EnableMoveInput.GetValue() && this.characterCtrl.TryInputDir.GetValue().magnitude > 0)
        {
            RaycastHit2D[] sel_hit2Ds = new RaycastHit2D[100];

            int sel_hit_cnt = this.characterCtrl.col2D.Cast(this.characterCtrl.TryInputDir.GetValue().normalized, sel_hit2Ds, this.JudgeSelectRange);

            foreach (var hit in sel_hit2Ds)
            {
                if (hit.collider == null) continue;

                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("MapBlock"))
                {
                    Vector3Int tpos = LevelGridGenerator.Instance.tilemap.WorldToCell(hit.point);

                    LevelGridTileObject tile_obj_new;
                    LevelGridTileObject tile_obj_old;
                    if (this.characterCtrl.MySelectTarget!=tpos && LevelGridGenerator.Instance.tile_dictionary.TryGetValue(tpos , out tile_obj_new))
                    {
                        tile_obj_new.SetSelect(true);
                        this.HaveSelectTarget = true;
                        if(LevelGridGenerator.Instance.tile_dictionary.TryGetValue(this.characterCtrl.MySelectTarget, out tile_obj_old))
                        {
                            tile_obj_old.SetSelect(false);  
                        }

                        this.characterCtrl.MySelectTarget = tpos;
                    }



                }

                Debug.Log(this.characterCtrl.MySelectTarget);
                //DrawXuLiState();

                //this.AimTargetRoot.rotation = Quaternion.FromToRotation(Vector2.up, characterCtrl.TryAimRotDir.GetValue());
                //this.MoveDirRoot.rotation = Quaternion.FromToRotation(Vector2.up, UI_VirtualInput.instance.GetDir("Left"));
            }
        }
    }
}
