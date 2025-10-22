using Assets.Scripts.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class ThumbInfo
{
    public JoystickType type;
    public RectTransform background;
    public RectTransform thumb;
    public bool inDrag;
    public float maxDragDist;
    public Vector2 v;

    public ThumbInfo(RectTransform bg, RectTransform thu) 
    {
        background = bg;
        thumb = thu;
        type = JoystickType.Normal;
        inDrag = false;
        
        // 添加 null 检查
        if (background != null)
        {
            maxDragDist = background.rect.width / 2;
            Image bgImage = background.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.raycastTarget = false;
            }
        }
        else
        {
            maxDragDist = 50f; // 默认值
        }
        
        if (thumb != null)
        {
            Image thumbImage = thumb.GetComponent<Image>();
            if (thumbImage != null)
            {
                thumbImage.raycastTarget = true;
            }
        }
        
        v = Vector2.zero;
    } 
}

public class UI_VirtualInput : BaseUI<UI_VirtualInput>, IDragHandler, IBeginDragHandler, IEndDragHandler , IPointerDownHandler , IPointerUpHandler 
{
    //[Header("摇杆类型")]
    //public JoystickType type;
    //[Header("摇杆背景")]
    //public RectTransform background;
    //[Header("摇杆控制柄")]
    //public RectTransform thumb;


    //bool inDrag;//是否在拖拽中
    //float maxDragDist;//最大拖拽距离

    //private Vector2 v;
    //private Vector2 v_right;

    //private List<ThumbInfo> thumbInfos;
    private Dictionary<string, ThumbInfo> thumbInfoDic;
    private Dictionary<string, float> thumbDragTime;

    private float thumbClickDuration = .5f;
    
    private PlayerInteractionManager playerInteractionManager;
    private Button skill1Button;

    private ThumbInfo _InitThumbOperate(RectTransform bg , RectTransform thu )
    {
        ThumbInfo ret = new ThumbInfo(bg , thu);

        ret.background = bg;    
        ret.thumb = thu;

        //thumbInfos.Add(ret);
        return ret; 
    }


    public override void InitUI()
    {
        base.InitUI();
        //this.background = nodeDics["m_LeftConBG"].transform as RectTransform;
        //this.thumb = nodeDics["m_LeftConTouchPos"].transform as RectTransform ;

        this.thumbDragTime = new Dictionary<string, float>();   
        this.thumbInfoDic = new Dictionary<string, ThumbInfo> ();
        
        // 添加 null 检查
        if (!nodeDics.ContainsKey("m_LeftConBG") || !nodeDics.ContainsKey("m_LeftConTouchPos"))
        {
            Debug.LogWarning("UI_VirtualInput: 缺少左摇杆UI元素");
            return;
        }
        
        this.thumbInfoDic.Add(
                "Left",
                _InitThumbOperate(
                    nodeDics["m_LeftConBG"].transform as RectTransform,
                    nodeDics["m_LeftConTouchPos"].transform as RectTransform
                    )
            );

    }
    /// <summary>
    /// 得到方向
    /// </summary>
    public Vector2 GetDir(string key = "Left")
    {
		if( key == "Left")
		{
			// 优先使用左侧摇杆的UI输入；若未拖拽则回退到键盘轴
			if (thumbInfoDic != null && thumbInfoDic.ContainsKey("Left"))
			{
				Vector2 leftThumb = thumbInfoDic["Left"].v;
				if (leftThumb.sqrMagnitude > 0.0001f)
				{
					return leftThumb.normalized;
				}
			}
			return new Vector2(Input.GetAxis("Horizontal") , Input.GetAxis("Vertical")).normalized;
		}
        
        if(thumbInfoDic.ContainsKey( key))
        {
            return thumbInfoDic[key].v.normalized;
        } 
        //Debug.LogError("摇杆不存在：" +  key); 
        return Vector2.zero;
    }


    private void Start()
    {
        // 添加技能按钮的 null 检查
        if (nodeDics.ContainsKey("m_Button_Skill1"))
        {
            skill1Button = this.nodeDics["m_Button_Skill1"].GetComponent<Button>();
            if (skill1Button != null)
            {
                skill1Button.onClick.AddListener(UseSKill1);
            }
        }
        
        if (nodeDics.ContainsKey("m_Button_Skill2"))
        {
            Button skill2Button = this.nodeDics["m_Button_Skill2"].GetComponent<Button>();
            if (skill2Button != null)
            {
                skill2Button.onClick.AddListener(UseSkill2);
            }
        }
        
        // 尝试获取玩家角色
        StartCoroutine(FindPlayerCharacter());
    }
    
    private IEnumerator FindPlayerCharacter()
    {
        // 等待一帧，确保玩家已经生成
        yield return null;
        
        // 查找玩家交互管理器
        PlayerInteractionManager[] managers = FindObjectsOfType<PlayerInteractionManager>();
        if (managers.Length > 0)
        {
            playerInteractionManager = managers[0];
        }
    }

    void UseSKill1()
    {
        
        VirtualInputEvnetArgs inputEvent = new VirtualInputEvnetArgs(
                EventType_VirtualInputEvent.SkillButtonClick, this.gameObject, "m_Button_Skill1", Vector2.zero
            );
        LevelEventQueue.Instance.EnqueueEvent(inputEvent);
        
    }

    void UseSkill2()
    {

        VirtualInputEvnetArgs inputEvent = new VirtualInputEvnetArgs(
                EventType_VirtualInputEvent.SkillButtonClick, this.gameObject, "m_Button_Skill2", Vector2.zero
            );
        LevelEventQueue.Instance.EnqueueEvent(inputEvent);

    }



    public void OnBeginDrag(PointerEventData eventData)
    {
        if (thumbInfoDic == null || eventData == null || eventData.pointerCurrentRaycast.gameObject == null)
            return;

        foreach(var pair in thumbInfoDic)
        {
            var info = pair.Value;
            if (info == null || info.thumb == null)
                continue;

            if (eventData.pointerCurrentRaycast.gameObject == info.thumb.gameObject)
            {
                info.inDrag = true;
                VirtualInputEvnetArgs inputEvent = new VirtualInputEvnetArgs(
                    EventType_VirtualInputEvent.ThumbStartDrag, this.gameObject, pair.Key, info.v
                );
                LevelEventQueue.Instance.EnqueueEvent(inputEvent);

            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (thumbInfoDic == null)
            return;

        foreach (var pair in thumbInfoDic)
        {
            var info = pair.Value;
            if (info == null || info.thumb == null || info.background == null)
                continue;

            // 检查是否正在拖拽这个摇杆
            if (info.inDrag)
            {
                // 获取鼠标相对于背景中心的位置
                Vector2 backgroundCenter = info.background.position;
                Vector2 mousePos = eventData.position;
                Vector2 offset = mousePos - backgroundCenter;
                
                // 计算距离和方向
                float dist = offset.magnitude;
                Vector2 direction = offset.normalized;
                
                // 限制在最大拖拽距离内
                if (dist > info.maxDragDist)
                {
                    dist = info.maxDragDist;
                }
                
                // 设置摇杆位置（相对于背景中心）
                info.thumb.position = backgroundCenter + direction * dist;
                
                // 更新输入向量（归一化）
                info.v = direction * (dist / info.maxDragDist);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (thumbInfoDic == null)
            return;

        foreach (var pair in thumbInfoDic)
        {
            var info = pair.Value;
            if (info == null || info.thumb == null || info.background == null)
                continue;

            // 检查是否正在拖拽这个摇杆
            if (info.inDrag)
            {
                VirtualInputEvnetArgs inputEvent;

                if (thumbDragTime.ContainsKey(pair.Key) && (Time.time - thumbDragTime[pair.Key])<thumbClickDuration )
                {
                    inputEvent = new VirtualInputEvnetArgs(
                         EventType_VirtualInputEvent.ThumbCancelDrag, this.gameObject, pair.Key, info.v
                    );
                    DispatchClick(pair.Key);
                }
                else
                {
                    inputEvent = new VirtualInputEvnetArgs(
                        EventType_VirtualInputEvent.ThumbEndDrag, this.gameObject, pair.Key, info.v
                    );
                }
                LevelEventQueue.Instance.EnqueueEvent(inputEvent);

                // 重置摇杆位置到背景中心
                info.thumb.position = info.background.position;
                info.v = Vector2.zero;
                info.inDrag = false;
            }
        }
    }

    private void Update()
    {
        if (thumbInfoDic == null)
            return;

        foreach(var pair in thumbInfoDic)
        {
            var info = pair.Value;
            if (info == null || info.thumb == null || info.background == null)
                continue;

            if (!info.inDrag)
            {
                // 平滑回弹到背景中心
                info.thumb.position = Vector2.Lerp(info.thumb.position, info.background.position, 0.1f);
            }
            else
            {
            
                
                VirtualInputEvnetArgs inputEvent = new VirtualInputEvnetArgs(
                    EventType_VirtualInputEvent.ThumbKeepDragging, this.gameObject, pair.Key, info.v
                );
                LevelEventQueue.Instance.EnqueueEvent(inputEvent );
            }
        }
        
        // 更新技能按钮1的可交互状态
        UpdateSkill1ButtonState();
    }
    
    /// <summary>
    /// 更新拾取/放下按钮的可交互状态
    /// </summary>
    private void UpdateSkill1ButtonState()
    {
        if (skill1Button == null || playerInteractionManager == null)
        {
            return;
        }
        
        // 直接使用玩家交互管理器判断是否可以互动
        skill1Button.interactable = playerInteractionManager.CanInteract();
    }

    Vector2 Screen2UI(Vector2 v, RectTransform rect, Camera camera = null)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, v, camera, out pos);
        return pos;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (thumbInfoDic == null || eventData == null || eventData.pointerCurrentRaycast.gameObject == null)
            return;

        foreach (var pair in thumbInfoDic)
        {
            var info = pair.Value;
            if (info == null || info.thumb == null)
                continue;

            if (eventData.pointerCurrentRaycast.gameObject == info.thumb.gameObject)
            {
                if (thumbDragTime.ContainsKey(pair.Key))
                {
                    thumbDragTime[pair.Key] = Time.time;
                }
                else
                {
                    thumbDragTime.Add(pair.Key, Time.time);
                }

            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (thumbInfoDic == null || eventData == null || eventData.pointerCurrentRaycast.gameObject == null)
            return;

        foreach (var pair in thumbInfoDic)
        {
            var info = pair.Value;
            if (info == null || info.thumb == null)
                continue;

            if (eventData.pointerCurrentRaycast.gameObject == info.thumb.gameObject)
            {
                if (thumbDragTime.ContainsKey(pair.Key))
                {
                    float presstime = Time.time - thumbDragTime[pair.Key];

                    if (presstime < thumbClickDuration)
                    {
                        DispatchClick(pair.Key);    
                    }
                }
            }
        }
    }

    public void DispatchClick(string ButtonKey)
    {
        if (!thumbInfoDic.ContainsKey(ButtonKey))
        {
            Debug.LogError("click:不存在摇杆！" +  ButtonKey);
            return;
        }
        VirtualInputEvnetArgs inputEvent = new VirtualInputEvnetArgs(
                   EventType_VirtualInputEvent.ThumbClick, this.gameObject, ButtonKey, thumbInfoDic[ButtonKey].v
               );
        LevelEventQueue.Instance.EnqueueEvent(inputEvent);
    }


}

/// <summary>
/// 摇杆类型
/// </summary>
public enum JoystickType
{
    Normal,//固定位置
    //PosCanChange,//可变位置
    //FollowMove,//跟随移动
}