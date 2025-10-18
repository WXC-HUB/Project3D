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
        maxDragDist = background.rect.width / 2;
        background.GetComponent<Image>().raycastTarget = false;
        thumb.GetComponent<Image>().raycastTarget = true;
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

        //this.background_right = nodeDics["m_RightConBG"].transform as RectTransform;
        //this.thumb_right = nodeDics["m_RightConTouchPos"].transform as RectTransform;
        this.thumbDragTime = new Dictionary<string, float>();   
        this.thumbInfoDic = new Dictionary<string, ThumbInfo> ();
        this.thumbInfoDic.Add(
                "Left",
                _InitThumbOperate(
                    nodeDics["m_LeftConBG"].transform as RectTransform,
                    nodeDics["m_LeftConTouchPos"].transform as RectTransform
                    )
            );
        this.thumbInfoDic.Add(
                "Right",
                _InitThumbOperate(
                    nodeDics["m_RightConBG"].transform as RectTransform,
                    nodeDics["m_RightConTouchPos"].transform as RectTransform
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
            
            return new Vector2(Input.GetAxis("Horizontal") , Input.GetAxis("Vertical")).normalized;
        }
        
        // 添加空检查
        if (thumbInfoDic == null)
        {
            return Vector2.zero;
        }
        
        if(thumbInfoDic.ContainsKey( key))
        {
            return thumbInfoDic[key].v.normalized;
        } 
        Debug.LogError("摇杆不存在：" +  key); 
        return Vector2.zero;
    }


    private void Start()
    {
        // 添加空检查，防止未初始化UI时报错
        if (nodeDics == null || !nodeDics.ContainsKey("m_Button_Skill1") || !nodeDics.ContainsKey("m_Button_Skill2"))
        {
            return;
        }
        
        this.nodeDics["m_Button_Skill1"].GetComponent<Button>().onClick.AddListener(UseSKill1);
        this.nodeDics["m_Button_Skill2"].GetComponent<Button>().onClick.AddListener(UseSkill2);
    }

    void UseSKill1()
    {
        if (LevelEventQueue.Instance == null) return;
        
        VirtualInputEvnetArgs inputEvent = new VirtualInputEvnetArgs(
                EventType_VirtualInputEvent.SkillButtonClick, this.gameObject, "m_Button_Skill1", Vector2.zero
            );
        LevelEventQueue.Instance.EnqueueEvent(inputEvent);
        
    }

    void UseSkill2()
    {
        if (LevelEventQueue.Instance == null) return;

        VirtualInputEvnetArgs inputEvent = new VirtualInputEvnetArgs(
                EventType_VirtualInputEvent.SkillButtonClick, this.gameObject, "m_Button_Skill2", Vector2.zero
            );
        LevelEventQueue.Instance.EnqueueEvent(inputEvent);

    }



    public void OnBeginDrag(PointerEventData eventData)
    {
        // 添加空检查
        if (thumbInfoDic == null) return;

        foreach(var pair in thumbInfoDic)
        {
            var info = pair.Value;
            if (info == null || info.thumb == null) continue;
            if (eventData.pointerCurrentRaycast.gameObject == info.thumb.gameObject)
            {
                info.inDrag = true;
                VirtualInputEvnetArgs inputEvent = new VirtualInputEvnetArgs(
                    EventType_VirtualInputEvent.ThumbStartDrag, this.gameObject, pair.Key, info.v
                    );
                if (LevelEventQueue.Instance != null)
                {
                    LevelEventQueue.Instance.EnqueueEvent(inputEvent);
                }

            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 添加空检查
        if (thumbInfoDic == null) return;
        
        foreach (var info in thumbInfoDic.Values)
        {
            if (info == null || info.thumb == null) continue;
            if (eventData.pointerCurrentRaycast.gameObject == info.thumb.gameObject)
            {
                Vector2 targetLocalPos = Screen2UI(eventData.position, info.thumb.parent as RectTransform, eventData.pressEventCamera);
                Vector2 targetDir = targetLocalPos.normalized;
                float dist = Vector2.Distance(targetLocalPos, Vector2.zero);
                if (dist > info.maxDragDist)
                {
                    info.thumb.localPosition = targetDir * info.maxDragDist;
                }
                else
                {
                    info.thumb.localPosition = targetDir * dist;
                }

                info.v = targetDir * dist;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 添加空检查
        if (thumbInfoDic == null || thumbDragTime == null) return;
        
        foreach (var pair in thumbInfoDic)
        {
            var info = pair.Value;
            if (info == null || info.thumb == null) continue;
            if (eventData.pointerCurrentRaycast.gameObject == info.thumb.gameObject)
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
                
                if (LevelEventQueue.Instance != null)
                {
                    LevelEventQueue.Instance.EnqueueEvent(inputEvent);
                }

                info.v = Vector2.zero;
                info.inDrag = false;
            }
        }
    }

    private void Update()
    {
        // 添加空检查，防止未初始化时报错
        if (thumbInfoDic == null || thumbInfoDic.Count == 0)
        {
            return;
        }
        
        foreach(var pair in thumbInfoDic)
        {
            var info = pair.Value;
            if (info == null || info.thumb == null) continue;
            
            if (!info.inDrag)
            {
                info.thumb.anchoredPosition = Vector2.Lerp(info.thumb.anchoredPosition, Vector2.zero, 0.1f);
            }
            else
            {
            
                
                VirtualInputEvnetArgs inputEvent = new VirtualInputEvnetArgs(
                    EventType_VirtualInputEvent.ThumbKeepDragging, this.gameObject, pair.Key, info.v
                );
                if (LevelEventQueue.Instance != null)
                {
                    LevelEventQueue.Instance.EnqueueEvent(inputEvent);
                }
            }
        }

    }

    Vector2 Screen2UI(Vector2 v, RectTransform rect, Camera camera = null)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, v, camera, out pos);
        return pos;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 添加空检查
        if (thumbInfoDic == null || thumbDragTime == null) return;
        
        foreach (var pair in thumbInfoDic)
        {
            var info = pair.Value;
            if (info == null || info.thumb == null) continue;
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
        // 添加空检查
        if (thumbInfoDic == null || thumbDragTime == null) return;
        
        foreach (var pair in thumbInfoDic)
        {
            var info = pair.Value;
            if (info == null || info.thumb == null) continue;
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