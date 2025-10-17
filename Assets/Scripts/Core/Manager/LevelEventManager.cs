using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.BaseUtils;
using UnityEngine;

namespace Assets.Scripts.Core
{

    /// <summary>
    /// 事件信息类基类
    /// </summary>
    public abstract class BaseEventArgs
    {
        public readonly Enum m_Type;
        public readonly GameObject sender;
        public BaseEventArgs(Enum _t, GameObject _sender)
        {
            m_Type = _t;
            sender = _sender;
        }
    }


    public class LevelEvnetManager : Singleton<LevelEvnetManager>
    {
        /// <summary>
        /// 事件链
        /// </summary>
        private Dictionary<Enum, List<Action<BaseEventArgs>>> eventEntitys = null;

        
        private LevelEvnetManager()
        {
            InitEvent();
        }

        /// <summary>
        /// 得到指定枚举项的所有事件链
        /// </summary>
        /// <param name="_type">指定枚举项</param>
        /// <returns>事件链</returns>
        private List<Action<BaseEventArgs>> GetEventList(Enum _type)
        {
            if (!eventEntitys.ContainsKey(_type))
            {
                eventEntitys.Add(_type, new List<Action<BaseEventArgs>>());
            }
            return eventEntitys[_type];
        }
        /// <summary>
        /// 添加事件
        /// </summary>
        /// <param name="_type">指定类型</param>
        /// <param name="action">指定事件</param>
        private void AddEvent(Enum _type, Action<BaseEventArgs> action)
        {
            List<Action<BaseEventArgs>> actions = GetEventList(_type);
            if (!actions.Contains(action))
            {
                actions.Add(action);
            }
        }

        /// <summary>
        /// 执行事件
        /// </summary>
        /// <param name="_type">指定事件类型</param>
        /// <param name="args">事件参数</param>
        private void CallEvent(BaseEventArgs args)
        {
            List<Action<BaseEventArgs>> actions = GetEventList(args.m_Type);
            //for (int i = actions.Count - 1; i >= 0; --i)
            //{
            //    if (null != actions[i])
            //    {
            //        actions[i](args);
            //    }
            //}

            for(int i = 0; i < actions.Count; ++i)
            {
                if (null != actions[i])
                {
                    actions[i](args);
                }
            }
        }
        /// <summary>
        /// 删除指定的事件
        /// </summary>
        /// <param name="_type">指定类型</param>
        /// <param name="action">指定的事件</param>
        private void DelEvent(Enum _type, Action<BaseEventArgs> action)
        {
            List<Action<BaseEventArgs>> actions = GetEventList(_type);
            if (actions.Contains(action))
            {
                actions.Remove(action);
            }
        }
        /// <summary>
        /// 删除指定的事件
        /// </summary>
        /// <param name="action">指定的事件</param>
        private void DelEvent(Action<BaseEventArgs> action)
        {
            if (eventEntitys.Count > 0)
            {
                foreach (List<Action<BaseEventArgs>> actions in eventEntitys.Values)
                {
                    if (actions.Contains(action))
                    {
                        actions.Remove(action);
                    }
                }
            }
        }
        /// <summary>
        /// 初始化事件链
        /// </summary>
        private void InitEvent()
        {
            eventEntitys = new Dictionary<Enum, List<Action<BaseEventArgs>>>();
        }

        /// <summary>
        /// 添加事件监听
        /// </summary>
        /// <param name="_type">事件类型</param>
        /// <param name="action">事件</param>
        public void AddListener(Enum _type, Action<BaseEventArgs> action)
        {
            //ValidCheck(_type);
            if (null != Instance)
            {
                Instance.AddEvent(_type, action);
            }
        }


        /// <summary>
        /// 事件分发
        /// </summary>
        /// <param name="_type">事件类型</param>
        /// <param name="args">事件参数</param>
        public void EventDispatch(BaseEventArgs args)
        {
            //Debug.Log(" --- Dispatch --- " + args.GetType().ToString());
            if (null != Instance)
            {
                Instance.CallEvent(args);
            }
        }
        /// <summary>
        /// 移除事件监听
        /// </summary>
        /// <param name="_type">事件类型</param>
        /// <param name="action">事件</param>
        public void DelListener(Enum _type, Action<BaseEventArgs> action)
        {
            
            if (null != Instance)
            {
                Instance.DelEvent(_type, action);
            }
        }
        /// <summary>
        /// 移除事件监听
        /// </summary>
        /// <param name="_type">事件类型</param>
        /// <param name="action">事件</param>
        public void DelListener(Action<BaseEventArgs> action)
        {
            if (null != Instance)
            {
                Instance.DelEvent(action);
            }
        }
        /// <summary>
        /// 移除所有事件
        /// </summary>
        public void RemoveAllListener()
        {
            if (null != Instance)
            {
                Instance.InitEvent();
            }
        }
    }

}
