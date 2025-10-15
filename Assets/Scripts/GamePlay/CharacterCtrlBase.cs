using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Core;
using UnityEngine.UI;
using Assets.Scripts.BaseUtils;
using TMPro;

public class SkillUseInfo
{
    public int SkillID = 0;
    public Vector2 SkillDispatchDir = Vector2.zero;
    public Vector2 SkillCastPos = Vector2.zero; 
    public CharacterCtrlBase dispatcher;
}

public class CharacterCtrlBase : MonoBehaviour
{
    public Rigidbody2D rb;
    public Collider2D col2D;

    bool isStill;

    public float MoveSpeed_a = 2f;

    public float TargetRotation = 0.0f;

    public Dictionary<string , CharacterAttribute> AttributesDicts = new Dictionary<string , CharacterAttribute>();    

    //float LinerDrag = 1.5f;
    public Character_Float LinerDrag = new Character_Float("LinerDrag", 1.5f);
    public Character_Float MaxSpeed = new Character_Float("MaxSpeed", 1.5f);
    public Character_Bool EnableMoveInput = new Character_Bool("EnableMoveInput", true);
    public Character_Bool IgnoreSpeedLimit = new Character_Bool("IgnoreSpeedLimit", false);
    public Character_Vector2 TryInputDir = new Character_Vector2("TryInputDir", Vector2.zero);
    public Character_Vector2 TryAimRotDir = new Character_Vector2("TryAimRotDir", Vector2.zero);

    public Character_Int MaxHP = new Character_Int("MaxHP", 30);
    public int NowHP;
    public Character_Bool isInvincible = new Character_Bool("isInvincible", false);

    public Character_Float Mass = new Character_Float("Mass", 1f);

    public Character_Int beHitDamage = new Character_Int("beHitDamage", 0);
    public Character_Int doHitDamage = new Character_Int("doHitDamage", 1);
    public Character_Float beHitPower = new Character_Float("beHitDamage", 10f);
    public Character_Float doHitPower = new Character_Float("beHitDamage", 10f);

    public Character_Bool isFixedPosition = new Character_Bool("isFixedPosition", false);

    public List<int> Init_Modifier_List = new List<int>();  



    // Start is called before the first frame update
    protected void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();  
        col2D = this.GetComponent<Collider2D>();

        NowHP = MaxHP.GetValue();

        for (int i = 0; i < Init_Modifier_List.Count; i++) 
        { 
            SkillDispatchCenter.Instance.AddModifierToCharacter(this , -1 , Init_Modifier_List[i]);
        }

        
    }

    protected void Awake()
    {
        LinerDrag.TakeEffect(this);
        MaxSpeed.TakeEffect(this);
        EnableMoveInput.TakeEffect(this);
        IgnoreSpeedLimit.TakeEffect(this);  
        TryInputDir.TakeEffect(this);  
        TryAimRotDir.TakeEffect(this);  

        Mass.TakeEffect(this);  
        MaxHP.TakeEffect(this);
        beHitDamage.TakeEffect(this);
        doHitDamage.TakeEffect(this);

        beHitPower.TakeEffect(this);
        doHitPower.TakeEffect(this);    

        isInvincible.TakeEffect(this);
        isFixedPosition.TakeEffect(this);
    }

    private void FixedUpdate()
    {
        UpdateMoveState();   //重写物理
    }

    private void UpdateMoveState()
    {
        
        //处理碰撞    
        RaycastHit2D[] hit2Ds = new RaycastHit2D[100];
        int hit_cnt = this.col2D.Cast(this.rb.velocity, hit2Ds, this.rb.velocity.magnitude * Time.fixedDeltaTime);

        foreach (var hit in hit2Ds) 
        {
            if(hit.collider == null) continue;

            if(hit.transform.gameObject.layer == LayerMask.NameToLayer("MapBlock") )
            {
                //直接反射
                this.rb.position = PhysicUtils.getNewPositionAfterCircleHit2D(this.rb, hit);
                this.rb.velocity = PhysicUtils.getNewMoveSpeedAfterCircleHit2D(this.rb , hit);

                Game2D_GamePlayEvent beCollideEvent = new Game2D_GamePlayEvent(EventType_Game2DPlayEvent.CharacterDoCollide, this.gameObject);
                beCollideEvent.doCharacter = this;
                beCollideEvent.beCharacter = null;
                beCollideEvent.event_param_dics.Add("HitPointX", hit.point.x);
                beCollideEvent.event_param_dics.Add("HitPointY", hit.point.y);
                beCollideEvent.event_param_dics.Add("DoHitCharacter", this);
                beCollideEvent.event_param_dics.Add("BeHitCharacter", null);
                LevelEventQueue.Instance.EnqueueEvent(beCollideEvent);

                break;
            }
            if(hit.transform.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                
                
                CharacterCtrlBase beCollideCtr = hit.transform.GetComponent<CharacterCtrlBase>();
                if(beCollideCtr == null)
                {
                    Debug.LogError("Player Transform上没有找到碰撞基类");
                }
                else
                {
                    //校准碰撞位置
                    this.rb.position = PhysicUtils.getNewPositionAfterCircleHit2D(this.rb, hit);

                    //处理对方的速度
                    SkillDispatchCenter.Instance.AddModifierToCharacter(beCollideCtr, .5f, 4);
                    beCollideCtr.rb.velocity = PhysicUtils.getNewMoveSpeedAfterCircleHit2D(beCollideCtr.rb, hit) - hit.normal * this.rb.velocity.magnitude * this.Mass.GetValue();
                    beCollideCtr.rb.velocity = beCollideCtr.rb.velocity.normalized * this.doHitPower.GetValue() / beCollideCtr.Mass.GetValue();

                    beCollideCtr.rb.velocity = -1 * hit.normal * this.rb.velocity.magnitude;

                    //处理自己的速度
                    if ( beCollideCtr.isFixedPosition.GetValue())
                    {
                        //直接反射
                        this.rb.velocity = PhysicUtils.getNewMoveSpeedAfterCircleHit2D(this.rb, hit);
                        
                    }
                    else
                    {
                        //this.rb.velocity = PhysicUtils.getMoveSpeedAfterHit
                        //    ( this.Mass.GetValue(), this.rb.velocity, beCollideCtr.Mass.GetValue() , beCollideCtr.rb.velocity, hit);
                        this.rb.velocity = PhysicUtils.getNewMoveSpeedAfterCircleHit2D(this.rb, hit);
                    }
                    SkillDispatchCenter.Instance.AddModifierToCharacter(this, .5f, 4);


                    
                    //this.rb.velocity = this.rb.velocity.normalized * beCollideCtr.beHitPower.GetValue() / this.Mass.GetValue();

                    //施加伤害
                    //this.NowHP -= beCollideCtr.beHitDamage.GetValue();
                    this.TakeDamage(beCollideCtr.beHitDamage.GetValue());
                    beCollideCtr.TakeDamage(this.doHitDamage.GetValue());

                    Game2D_GamePlayEvent beCollideEvent = new Game2D_GamePlayEvent(EventType_Game2DPlayEvent.CharacterBeCollide, beCollideCtr.gameObject);
                    beCollideEvent.doCharacter = this;
                    beCollideEvent.beCharacter = beCollideCtr;
                    beCollideEvent.event_param_dics.Add("HitPointX", hit.point.x);
                    beCollideEvent.event_param_dics.Add("HitPointY", hit.point.y);
                    beCollideEvent.event_param_dics.Add("DoHitCharacter", this);
                    beCollideEvent.event_param_dics.Add("BeHitCharacter", beCollideCtr);
                    LevelEventQueue.Instance.EnqueueEvent(beCollideEvent);

                    //this.rb.velocity = Vector2.zero;
                }

                break;
            }
        
        }

        
        //处理位移输入
        if (this.EnableMoveInput.GetValue())
        {
            //Vector3 mv = new Vector3(this.TryInputDir.GetValue().x, this.TryInputDir.GetValue().y, 0);
            // Debug.Log(this.TryInputDir.GetValue());
            this.rb.velocity += this.MoveSpeed_a * this.TryInputDir.GetValue() * Time.fixedDeltaTime;

            
        }



        //个人速度限制
        if (!this.IgnoreSpeedLimit.GetValue())
        {
            this.rb.velocity = this.rb.velocity.magnitude >= MaxSpeed.GetValue() ? this.rb.velocity.normalized * MaxSpeed.GetValue() : this.rb.velocity;
        }


        //处理线性阻尼
        if (this.rb.velocity.magnitude <= LinerDrag.GetValue() * Time.fixedDeltaTime + 0.001f)
        {
            this.rb.velocity = Vector3.zero;
            this.isStill = true;
        }
        else
        {
            this.rb.velocity = this.rb.velocity.normalized * (this.rb.velocity.magnitude - LinerDrag.GetValue() * Time.fixedDeltaTime);
            this.isStill = false;
        }

        //处理速度锁定
        if (this.isFixedPosition.GetValue()) 
        {
            this.rb.constraints = RigidbodyConstraints2D.FreezePosition;
        }
        else
        {
            this.rb.constraints = RigidbodyConstraints2D.None;
        }


    }

    public void AddForce(Vector2 direction , float Force , bool ignoreInitSpeed = false)
    {
        //Debug.LogError("add!" +  Force);
        if (this.isFixedPosition.GetValue()) return;
        if (ignoreInitSpeed) 
        {
            this.rb.velocity = direction.normalized * Force / this.Mass.GetValue();
        }
        else
        {
            //Vector3 mv = new Vector3(direction.x , direction.y , 0);
            this.rb.velocity += direction.normalized * Force / this.Mass.GetValue();
        }
        
    }

    public bool isNowStill()
    {
        return isStill;
    }

    // Update is called once per frame
    void Update()
    {
        if( this.EnableMoveInput.GetValue())
        {
            UpdateAllInput();
        }

    }

    public void TakeDamage(int damage , SkillUseInfo skillUseInfo = null)
    {
        if (this.isInvincible.GetValue())
        {
            return;
        }
        else
        {
            this.NowHP -= damage;
        }

        if(this.NowHP < 0)
        {
            this.Die(skillUseInfo);
        }
    }

    public void Die(SkillUseInfo skillUseInfo)
    {
        Game2D_GamePlayEvent beCollideEvent = new Game2D_GamePlayEvent(EventType_Game2DPlayEvent.CharacterDie, gameObject);
        beCollideEvent.event_param_dics.Add("PositionX", transform.position.x);
        beCollideEvent.event_param_dics.Add("PositionY", transform.position.y);
        if (null != skillUseInfo) 
        {
            beCollideEvent.event_param_dics.Add("Killer", skillUseInfo.dispatcher);
        }
        LevelEventQueue.Instance.EnqueueEvent(beCollideEvent);

        GameObject.Destroy(gameObject , .2f); 
    }

    void UpdateAllInput()
    {
        if (UI_VirtualInput.instance != null)
        {
            //Vector2 input = (UI_VirtualInput.instance as UI_VirtualInput).GetDir("Left");
            Vector2 rot_input = (UI_VirtualInput.instance as UI_VirtualInput).GetDir("Right");
            
        }
    }

}
