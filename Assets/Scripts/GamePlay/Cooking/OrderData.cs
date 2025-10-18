using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Cooking
{
    /// <summary>
    /// 订单类型
    /// </summary>
    public enum OrderType
    {
        Small,  // 小订单 - 喂防御塔
        Large   // 大订单 - 关卡目标
    }
    
    /// <summary>
    /// 订单数据 - 运行时订单实例
    /// </summary>
    [System.Serializable]
    public class OrderData
    {
        public int orderID;
        public OrderType orderType;
        public RecipeData recipe;
        public float timeLimit; // 时间限制（秒），0表示无限制
        public float createdTime; // 创建时间戳
        
        // 大订单目标
        public int targetCount = 1; // 需要完成的数量
        public int completedCount = 0; // 已完成的数量
        
        /// <summary>
        /// 获取剩余时间
        /// </summary>
        public float GetRemainingTime()
        {
            if (timeLimit <= 0) return float.MaxValue;
            return Mathf.Max(0, timeLimit - (Time.time - createdTime));
        }
        
        /// <summary>
        /// 是否超时
        /// </summary>
        public bool IsExpired()
        {
            return timeLimit > 0 && GetRemainingTime() <= 0;
        }
        
        /// <summary>
        /// 是否完成
        /// </summary>
        public bool IsCompleted()
        {
            return completedCount >= targetCount;
        }
        
        /// <summary>
        /// 获取完成进度 (0-1)
        /// </summary>
        public float GetProgress()
        {
            return targetCount > 0 ? (float)completedCount / targetCount : 0f;
        }
        
        /// <summary>
        /// 获取时间进度 (0-1)，1表示快超时了
        /// </summary>
        public float GetTimeProgress()
        {
            if (timeLimit <= 0) return 0f;
            return Mathf.Clamp01((Time.time - createdTime) / timeLimit);
        }
    }
}

