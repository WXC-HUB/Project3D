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

        UI_PlayerHUD.instance.SetSkillFocusPlayer(this.characterCtrl);
        LevelManager.Instance.MyHero = this.characterCtrl;

    }

    public void OnSKillButtonClick(BaseEventArgs s)
    {
        VirtualInputEvnetArgs inputEvnet = (VirtualInputEvnetArgs)s;
        if (inputEvnet.InputOperKey == "m_Button_Skill1")
        {
            SkillUseInfo skinfo = new SkillUseInfo();
            if (characterCtrl.nowAttachList.Count > 0)
            {

                skinfo.SkillID = 7;
            }
            else
            {
                skinfo.SkillID = 6;
            }
            skinfo.SkillDispatchDir = inputEvnet.InputDir;
            //skinfo.SkillCastPos = transform.position;
            characterCtrl.StartUseSkill(skinfo);
        }
        if (inputEvnet.InputOperKey == "m_Button_Skill2")
        {
            var newgo = LevelManager.Instance.SpawnCharacterByID<CharacterCtrlBase>(103);
            newgo.transform.position = transform.position;

            var newgo2= LevelManager.Instance.SpawnCharacterByID<CharacterCtrlBase>(106);
            newgo2.transform.position = transform.position + new Vector3(0.4F , 0.4F , 0);

            var newgo3 = LevelManager.Instance.SpawnCharacterByID<CharacterCtrlBase>(206);
            newgo3.transform.position = transform.position + new Vector3(0.8F, 0.8F, 0);
        }

    }

    public void OnThumbClick(BaseEventArgs s)
    {
        VirtualInputEvnetArgs inputEvnet = (VirtualInputEvnetArgs)s;
        // 右摇杆已删除，不再处理Right事件
    }

    public void OnThumbDown(BaseEventArgs s)
    {
        if (!isInputEnable) return;

        VirtualInputEvnetArgs inputEvent = (VirtualInputEvnetArgs)s;

        // 右摇杆已删除，不再处理Right事件
    }

    public void OnThumbUp(BaseEventArgs s)
    {
        if (!isInputEnable) return;

        VirtualInputEvnetArgs inputEvent = (VirtualInputEvnetArgs)s;

        // 右摇杆已删除，不再处理Right事件
    }

    public void OnThumbKeep(BaseEventArgs s)
    {
        if (!isInputEnable) return;
        VirtualInputEvnetArgs inputEvent = (VirtualInputEvnetArgs)s;

        // 右摇杆已删除，不再处理Right事件
    }

    public void OnThumbCancel(BaseEventArgs s)
    {
        if (!isInputEnable) return;

        VirtualInputEvnetArgs inputEvent = (VirtualInputEvnetArgs)s;
        // 右摇杆已删除，不再处理Right事件
    }

    void TestSelect()
    {
        // 添加空引用检查
        if (this.characterCtrl == null || this.characterCtrl.col2D == null)
            return;

        if (LevelGridGenerator.Instance == null || LevelGridGenerator.Instance.tilemap == null || LevelGridGenerator.Instance.tile_dictionary == null)
            return;

        bool onMoveDirectionHaveBlock = false;
        if (this.characterCtrl.EnableMoveInput.GetValue() && this.characterCtrl.TryInputDir.GetValue().magnitude > 0)
        {
            RaycastHit2D[] sel_hit2Ds = new RaycastHit2D[100];

            int sel_hit_cnt = this.characterCtrl.col2D.Cast(this.characterCtrl.TryInputDir.GetValue().normalized, sel_hit2Ds, this.JudgeSelectRange);
            
            foreach (var hit in sel_hit2Ds)
            {

                if (hit.collider == null) continue;
                Debug.DrawLine(hit.transform.position, hit.transform.position + new Vector3(0, 0, 1f));
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("MapBlock"))
                {
                    Vector3Int tpos = LevelGridGenerator.Instance.tilemap.WorldToCell(hit.point);

                    LevelGridTileObject tile_obj_new;
                    LevelGridTileObject tile_obj_old;
                    if ( LevelGridGenerator.Instance.tile_dictionary.TryGetValue(tpos, out tile_obj_new))
                    {
                        if(this.characterCtrl.MySelectTarget != tpos)
                        {
                            tile_obj_new.SetSelect(true);
                            if (LevelGridGenerator.Instance.tile_dictionary.TryGetValue(this.characterCtrl.MySelectTarget, out tile_obj_old))
                            {
                                tile_obj_old.SetSelect(false);
                            }
                        }

                        this.HaveSelectTarget = true;
                        this.characterCtrl.MySelectTarget = tpos;
                        onMoveDirectionHaveBlock = true;
                    }



                }
                //DrawXuLiState();

                //this.AimTargetRoot.rotation = Quaternion.FromToRotation(Vector2.up, characterCtrl.TryAimRotDir.GetValue());
                //this.MoveDirRoot.rotation = Quaternion.FromToRotation(Vector2.up, UI_VirtualInput.instance.GetDir("Left"));
            }
           
        }
        if (onMoveDirectionHaveBlock == false)
        {
            LevelGridTileObject tile_obj_old;
            if (LevelGridGenerator.Instance != null && 
                LevelGridGenerator.Instance.tile_dictionary != null &&
                LevelGridGenerator.Instance.tile_dictionary.TryGetValue(this.characterCtrl.MySelectTarget, out tile_obj_old))
            {
                if (tile_obj_old != null && this.characterCtrl.MySelectTarget.x != -999 && 
                    (tile_obj_old.transform.position - transform.position).magnitude > JudgeSelectRange + 0.5f)
                {
                    tile_obj_old.SetSelect(false);

                    this.characterCtrl.MySelectTarget = new Vector3Int(-999, -999, 0);
                    this.HaveSelectTarget = false;
                }
            }
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        TestSelect();   
        //处理Item选中
    }
}
