using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Core;
using UnityEngine.UI;
using Assets.Scripts.BaseUtils;
using TMPro;

public enum DeathCause
{
    Killed,          // 被击杀
    ReachedEnd,      // 到达终点
    Other            // 其他原因
}

public class SkillUseInfo
{
    public int SkillID = 0;
    public Vector2 SkillDispatchDir = Vector2.zero;
    public Vector2 SkillCastPos = Vector2.zero; 
    public CharacterCtrlBase dispatcher;
    public CharacterCtrlBase AimTarget;
}

public class CharacterCtrlBase : MonoBehaviour
{
    public Rigidbody2D rb;
    public Collider2D col2D;

    public SkillUseInfo fromSkillInfo;

    public int MyGameObjectID = 0;
    public InGameCharacterType MyObjectLayer = InGameCharacterType.None;
    public bool isReadyForNextRound = false;        

    public bool isAttachedToOther = false;

    /// <summary>
    /// 尸体预制体，敌人死亡后会生成此预制体供子弹继续追踪
    /// </summary>
    public GameObject corpsePrefab;

    /// <summary>
    /// 终点标记预制体（静态，全局共享）
    /// </summary>
    private static GameObject endPointMarkerPrefab;

    bool isStill;

    public float MoveSpeed_a = 2f;

    public float TargetRotation = 0.0f;

    public Dictionary<string , CharacterAttribute> AttributesDicts = new Dictionary<string , CharacterAttribute>();   
    
    public List<CharacterCtrlBase> nowAttachList = new List<CharacterCtrlBase>();

    public Character_Bool isAlive = new Character_Bool("isAlive", true); 

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

    public Character_Bool DestroyOnDie = new Character_Bool("DestroyOnDie", true);

    public Character_Bool isFixedPosition = new Character_Bool("isFixedPosition", false);

    public Character_Bool usePhysic = new Character_Bool("usePhysic", true);

    public Character_Bool canBeGrabed = new Character_Bool("canBeGrabed", false);

    public Character_Float grabDistance = new Character_Float("grabDistance", 1f);

    public List<int> Init_Modifier_List = new List<int>();
    public Character_Bool IsFollowTarget = new Character_Bool("IsFollowTarget", false);
    public CharacterCtrlBase followTarget, from_char;


    public Character_Int MaxMP = new Character_Int("MaxHP", 30);
    public int NowMP;
    public Character_Float Reduce_MP_PerSecond = new Character_Float("Reduce_MP_PerSecond", 0);
    public float have_reduce_MP = 0;
    public Character_Bool canAttack = new Character_Bool("canAttack", true);



    public Character_Int Damage_Shoot = new Character_Int("Damage_Shoot", 1);



    // Start is called before the first frame update
    protected void Start()
    {

        rb = this.GetComponent<Rigidbody2D>();  
        col2D = this.GetComponent<Collider2D>();

        NowHP = MaxHP.GetValue();
        NowMP = MaxMP.GetValue();

        for (int i = 0; i < Init_Modifier_List.Count; i++) 
        { 
            SkillDispatchCenter.Instance.AddModifierToCharacter(this , -1 , Init_Modifier_List[i]);
        }

        
    }

    protected void Awake()
    {
        isAlive.TakeEffect(this);
        
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

        usePhysic.TakeEffect(this);

        IsFollowTarget.TakeEffect(this);

        canBeGrabed.TakeEffect(this);
        grabDistance.TakeEffect(this);

        Reduce_MP_PerSecond.TakeEffect(this);
        MaxMP.TakeEffect(this);
        canAttack.TakeEffect(this);

        Damage_Shoot.TakeEffect(this);
    }


    private void FixedUpdate()
    {
        if (!isAlive.GetValue())
        {
            Die();   
        }
        
        if (this.usePhysic.GetValue())
        {
            UpdateMoveState();   //重写物理
        }

        NowMP = (int)Mathf.Min(NowMP, MaxMP.GetValue());

        if(Reduce_MP_PerSecond.GetValue() != 0)
        {
            have_reduce_MP += Time.deltaTime * Reduce_MP_PerSecond.GetValue();
            if(  Mathf.Abs(have_reduce_MP) >= 1)
            {
                NowMP = (int)Mathf.Max(0, NowMP - Mathf.Floor(have_reduce_MP));
                have_reduce_MP -= (int)Mathf.Floor(have_reduce_MP);
            }
        }

        canAttack.real_value = (NowMP > 0);
        
    }

    private void UpdateMoveState()
    {
        if (this.IsFollowTarget.GetValue()) 
        {
            if(this.followTarget != null)
            {

                Vector3 target_pos = this.followTarget.transform.position;
                Vector3 move_len = target_pos - transform.position;
                float move_dis = MaxSpeed.GetValue() * Time.deltaTime;
                if (move_len.magnitude <= move_dis)
                {
                    transform.position = target_pos;

                    Game2D_GamePlayEvent beCollideEvent = new Game2D_GamePlayEvent(EventType_Game2DPlayEvent.CharacterHitTarget, gameObject);
                    beCollideEvent.doCharacter = this;
                    beCollideEvent.beCharacter = followTarget;
                    beCollideEvent.event_param_dics.Add("HitPointX", transform.position.x);
                    beCollideEvent.event_param_dics.Add("HitPointY", transform.position.y);
                    beCollideEvent.event_param_dics.Add("DoHitCharacter", this);
                    beCollideEvent.event_param_dics.Add("BeHitCharacter", followTarget);
                    beCollideEvent.skillinfo = fromSkillInfo;

                    LevelEventQueue.Instance.EnqueueEvent(beCollideEvent);
                }
                else
                {
                    transform.position += move_len.normalized * move_dis;
                }
            }
            else
            {
                Die();
            }

        }
        
        if(col2D == null || rb == null)
        {
            return;
        }
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
                    Game2D_GamePlayEvent beCollideEvent = new Game2D_GamePlayEvent(EventType_Game2DPlayEvent.CharacterBeCollide, beCollideCtr.gameObject);
                    
                    beCollideEvent.doCharacter = this;
                    beCollideEvent.beCharacter = beCollideCtr;
                    beCollideEvent.event_param_dics.Add("HitPointX", hit.point.x);
                    beCollideEvent.event_param_dics.Add("HitPointY", hit.point.y);
                    beCollideEvent.event_param_dics.Add("DoHitCharacter", this);
                    beCollideEvent.event_param_dics.Add("BeHitCharacter", beCollideCtr);
                    LevelEventQueue.Instance.EnqueueEvent(beCollideEvent);

                    /*
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
                    */


                    //this.rb.velocity = this.rb.velocity.normalized * beCollideCtr.beHitPower.GetValue() / this.Mass.GetValue();

                    //施加伤害
                    //this.NowHP -= beCollideCtr.beHitDamage.GetValue();


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

            // 3D角色移动时转向移动方向（只对玩家生效）
            if (this is PlayerCharacterCtrl && this.TryInputDir.GetValue().magnitude > 0.1f)
            {
                // 在XY平面上旋转（Z轴朝向摄像机）
                Vector3 moveDir3D = new Vector3(this.TryInputDir.GetValue().x, this.TryInputDir.GetValue().y, 0f);
                // 使用LookRotation，指定up为forward（Z轴），这样角色在XY平面上旋转
                Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, moveDir3D);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
            }
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
    public void Update()
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
            int new_dmg = skillUseInfo == null ? damage : damage * skillUseInfo.dispatcher.Damage_Shoot.GetValue();
            this.NowHP -= new_dmg;
        }

        if(this.NowHP < 0)
        {
            this.Die(DeathCause.Killed, skillUseInfo);
        }
    }

    /// <summary>
    /// 角色死亡/消失的统一方法
    /// </summary>
    /// <param name="cause">死亡原因</param>
    /// <param name="skillUseInfo">技能信息（可选）</param>
    public void Die(DeathCause cause = DeathCause.Other, SkillUseInfo skillUseInfo = null)
    {
        Game2D_GamePlayEvent beCollideEvent = new Game2D_GamePlayEvent(EventType_Game2DPlayEvent.CharacterDie, gameObject);
        beCollideEvent.event_param_dics.Add("PositionX", transform.position.x);
        beCollideEvent.event_param_dics.Add("PositionY", transform.position.y);
        beCollideEvent.event_param_dics.Add("DeathCause", cause.ToString());
        if (null != skillUseInfo) 
        {
            beCollideEvent.event_param_dics.Add("Killer", skillUseInfo.dispatcher);
        }
        LevelEventQueue.Instance.EnqueueEvent(beCollideEvent);

        foreach(var i in GetComponents<CharacterModifier>())
        {
            i.ModifierDispel();
        }
        // 创建替代目标（尸体或终点标记）
        CharacterCtrlBase replaceTarget = null;
        
        if (cause == DeathCause.Killed && corpsePrefab != null)
        {
            // 被击杀：生成尸体
            GameObject corpse = Instantiate(corpsePrefab, transform.position, transform.rotation, LevelManager.Instance.LevelObjectsRoot);
            corpse.name = $"Corpse_{gameObject.name}";
            replaceTarget = corpse.GetComponent<CharacterCtrlBase>();
            if (replaceTarget != null)
            {
                Debug.Log($"生成尸体: {corpse.name} at {transform.position}");
            }
        }
        else if (cause == DeathCause.ReachedEnd)
        {
            // 到达终点：生成临时标记
            if (endPointMarkerPrefab == null)
            {
                endPointMarkerPrefab = new GameObject("EndPointMarker_Prefab");
                endPointMarkerPrefab.AddComponent<EndPointMarker>();
                endPointMarkerPrefab.SetActive(false);
            }
            
            GameObject marker = Instantiate(endPointMarkerPrefab, transform.position, Quaternion.identity, LevelManager.Instance.LevelObjectsRoot);
            marker.name = $"EndPointMarker_{gameObject.name}";
            marker.SetActive(true);
            replaceTarget = marker.GetComponent<CharacterCtrlBase>();
            if (replaceTarget != null)
            {
                Debug.Log($"生成终点标记: {marker.name} at {transform.position}");
            }
        }

        // 转移所有追踪此对象的子弹到新目标
        if (replaceTarget != null)
        {
            int transferredCount = 0;
            foreach (var charType in LevelManager.Instance.Character_Dict.Values)
            {
                foreach (var character in charType)
                {
                    if (character != null && character.followTarget == this)
                    {
                        character.followTarget = replaceTarget;
                        transferredCount++;
                    }
                }
            }
            if (transferredCount > 0)
            {
                Debug.Log($"转移了 {transferredCount} 个子弹到新目标: {replaceTarget.gameObject.name}");
            }
        }

        // 根据死亡原因决定是否掉落食材
        if (cause == DeathCause.Killed)
        {
            DropIngredient();
        }
        else if (cause == DeathCause.ReachedEnd)
        {
            // 到达终点，扣除基地血量
            OnEnemyReachedEnd();
        }

        GameObject.Destroy(gameObject);
        LevelManager.Instance.ClearCharDic();
    }

    /// <summary>
    /// 敌人到达终点的处理
    /// </summary>
    private void OnEnemyReachedEnd()
    {
        // 判断是否是敌人类型
        if (MyGameObjectID == 0)
        {
            return;
        }

        GameCharacters config = GameTableConfig.Instance.Config_GameCharacters.FindFirstLine(x => x.ObjectID == MyGameObjectID);
        if (config == null || config.ObjectType != "Enemy")
        {
            return;
        }

        LevelManager.Instance.MyHero.TakeDamage(1);

        // TODO: 扣除基地血量的逻辑
        // 例如: LevelManager.Instance.BaseHP -= 10;
        Debug.Log($"敌人 {config.ObjectName} 到达终点！基地受到伤害！");
    }

    /// <summary>
    /// 掉落食材逻辑
    /// </summary>
    private void DropIngredient()
    {
        // 判断是否是敌人类型
        if (MyGameObjectID == 0)
        {
            return; // 没有配置ID，不处理
        }

        GameCharacters config = GameTableConfig.Instance.Config_GameCharacters.FindFirstLine(x => x.ObjectID == MyGameObjectID);
        if (config == null || config.ObjectType != "Enemy")
        {
            return; // 不是敌人类型，不掉落
        }

        // 掉落概率检查
        float dropChance = 1f;
        if (Random.value > dropChance)
        {
            // Debug.Log("敌人死亡：未掉落食材（概率未命中）");
            return;
        }

        // 获取可提交菜品列表
        List<int> submittableDishIds = DishSubmissionManager.Instance.GetAllDishIds();
        if (submittableDishIds == null || submittableDishIds.Count == 0)
        {
            return;
        }

        // 统计食材出现次数作为权重
        Dictionary<int, int> ingredientWeights = new Dictionary<int, int>();

        foreach (int dishId in submittableDishIds)
        {
            // 检查是否是成品菜（在Recipe中作为CookResult）
            Recipe recipe = GameTableConfig.Instance.Config_Recipe.FindFirstLine(x => x.CookResult == dishId);
            
            if (recipe != null)
            {
                // 是成品菜，取原料列表
                foreach (int ingredientId in recipe.DishList)
                {
                    // 验证原料在Dish表中存在
                    Dish dish = GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.DishID == ingredientId);
                    if (dish != null)
                    {
                        // 统计该食材出现次数
                        if (ingredientWeights.ContainsKey(ingredientId))
                        {
                            ingredientWeights[ingredientId]++;
                        }
                        else
                        {
                            ingredientWeights[ingredientId] = 1;
                        }
                    }
                }
            }
            else
            {
                // 不是成品菜，取自己
                Dish dish = GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.DishID == dishId);
                if (dish != null)
                {
                    if (ingredientWeights.ContainsKey(dishId))
                    {
                        ingredientWeights[dishId]++;
                    }
                    else
                    {
                        ingredientWeights[dishId] = 1;
                    }
                }
            }
        }

        // 如果没有可掉落的食材，直接返回
        if (ingredientWeights.Count == 0)
        {
            return;
        }

        // 基于权重随机选择食材
        int selectedDishId = WeightedRandomSelect(ingredientWeights);

        // 获取食材配置
        Dish selectedDish = GameTableConfig.Instance.Config_Dish.FindFirstLine(x => x.DishID == selectedDishId);
        if (selectedDish != null)
        {
            // 生成食材GameObject
            CharacterCtrlBase droppedItem = LevelManager.Instance.SpawnCharacterByID<CharacterCtrlBase>(selectedDish.GameCharacter);
            if (droppedItem != null)
            {
                droppedItem.transform.position = transform.position;
                Debug.Log($"敌人死亡掉落食材: {selectedDish.Name} (DishID: {selectedDishId}, 权重: {ingredientWeights[selectedDishId]})");
            }
        }
    }

    /// <summary>
    /// 基于权重的随机选择
    /// </summary>
    /// <param name="weights">物品ID及其权重</param>
    /// <returns>被选中的物品ID</returns>
    private int WeightedRandomSelect(Dictionary<int, int> weights)
    {
        // 计算总权重
        int totalWeight = 0;
        foreach (var weight in weights.Values)
        {
            totalWeight += weight;
        }

        // 生成随机值
        int randomValue = Random.Range(0, totalWeight);

        // 根据权重选择
        int cumulativeWeight = 0;
        foreach (var kvp in weights)
        {
            cumulativeWeight += kvp.Value;
            if (randomValue < cumulativeWeight)
            {
                return kvp.Key;
            }
        }

        // 默认返回第一个（理论上不会到达这里）
        return new List<int>(weights.Keys)[0];
    }

    public virtual bool TryAttachObject(CharacterCtrlBase attach_obj)
    {
        // 先设置父级，保持世界坐标
        attach_obj.transform.SetParent(transform, true);
        
        // 只有玩家才会让物品移到固定位置
        if (this is PlayerCharacterCtrl)
        {
            // 然后只修改本地位置，不改变旋转和缩放
            // XY平面：放在正面（Y轴正方向，即头顶朝向）
            attach_obj.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        }
        
        nowAttachList.Add(attach_obj);
        attach_obj.isAttachedToOther = true;    
        return true;
    }

    public virtual bool TryDropObject(CharacterCtrlBase attach_obj)
    {
        if (nowAttachList.Contains(attach_obj))
        {
            attach_obj.transform.SetParent(LevelManager.Instance.LevelObjectsRoot);
            nowAttachList.Remove(attach_obj);
            attach_obj.isAttachedToOther = false;
            
            // 只有玩家才执行"丢"的动画
            if (this is PlayerCharacterCtrl)
            {
                StartCoroutine(ThrowObjectAnimation(attach_obj));
            }
            
            return true;
        }

        return false;
    }
    
    /// <summary>
    /// 物体被丢出的动画效果：刷到人物中央，然后落到地上
    /// </summary>
    private IEnumerator ThrowObjectAnimation(CharacterCtrlBase obj)
    {
        // 检查物体是否已被销毁
        if (obj == null)
        {
            yield break;
        }
        
        // 停止物体的所有物理速度
        if (obj.rb != null)
        {
            obj.rb.velocity = Vector2.zero;
            obj.rb.angularVelocity = 0f;
        }
        
        // 1. 立即把物品移动到人物中央稍微上方
        Vector3 dropStartPos = new Vector3(
            transform.position.x,
            transform.position.y + 0.5f,  // 稍微在人物上方一点
            obj.transform.position.z
        );
        obj.transform.position = dropStartPos;
        
        // 2. 目标位置：往下落到角色的Y位置（地面）
        Vector3 targetPos = new Vector3(
            transform.position.x,
            transform.position.y,
            dropStartPos.z
        );
        
        // 3. 匀速下落到目标位置
        float duration = 0.3f;  // 下落持续时间
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            // 检查物体是否已被销毁（比如提交菜品时）
            if (obj == null)
            {
                yield break;
            }
            
            // 检查物体是否被重新附加到其他对象（比如锅）
            if (obj.isAttachedToOther)
            {
                // 物体已经被附加到其他对象，停止动画
                yield break;
            }
            
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 匀速插值
            obj.transform.position = Vector3.Lerp(dropStartPos, targetPos, t);
            
            // 持续强制速度为0
            if (obj.rb != null)
            {
                obj.rb.velocity = Vector2.zero;
            }
            
            yield return null;
        }
        
        // 4. 确保到达精确位置并完全静止
        // 再次检查物体是否已被销毁
        if (obj == null)
        {
            yield break;
        }
        
        obj.transform.position = targetPos;
        
        if (obj.rb != null)
        {
            obj.rb.velocity = Vector2.zero;
            obj.rb.angularVelocity = 0f;
        }
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
