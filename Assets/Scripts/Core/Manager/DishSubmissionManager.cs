using System;
using System.Collections.Generic;
using Assets.Scripts.BaseUtils;

namespace Assets.Scripts.Core
{
	/// <summary>
	/// 全局菜品提交管理器（纯C#单例）：管理菜品ID与提交次数，并向UI广播变更。
	/// 不依赖GameObject，数据存在内存中。
	/// </summary>
	public class DishSubmissionManager : Singleton<DishSubmissionManager>
	{
		// 菜品ID -> 提交次数（key 即为可用菜品列表）
		private readonly Dictionary<int, int> dishIdToCount = new Dictionary<int, int>();

		/// <summary>
		/// (dishId, newCount)
		/// </summary>
		public event Action<int, int> OnDishCountChanged;

		/// <summary>
		/// 初始化菜品数据（在LevelManager中显式调用，保证在UI创建前完成）
		/// </summary>
		public void InitializeDishes()
		{
			dishIdToCount.Clear();

			// 默认菜品（后续可改为从表或关卡配置读取）ToDo ruixiangliu
			dishIdToCount[1] = 0;
			dishIdToCount[4] = 0;
			dishIdToCount[5] = 0;
		}

		/// <summary>
		/// 从菜品ID列表初始化（供关卡配置调用）
		/// </summary>
		public void InitializeDishes(List<int> dishIds)
		{
			dishIdToCount.Clear();

			foreach (int id in dishIds)
			{
				dishIdToCount[id] = 0;
			}
		}

		/// <summary>
		/// 获取所有菜品ID列表（供UI初始化用）
		/// </summary>
		public List<int> GetAllDishIds()
		{
			return new List<int>(dishIdToCount.Keys);
		}

		/// <summary>
		/// 累加提交某个菜品的次数。
		/// </summary>
		public void AddDishSubmission(int dishId, int count = 1)
		{
			if (count == 0) return;
			if (!dishIdToCount.ContainsKey(dishId))
			{
				dishIdToCount[dishId] = 0;
			}
			dishIdToCount[dishId] = Math.Max(0, dishIdToCount[dishId] + count);
			OnDishCountChanged?.Invoke(dishId, dishIdToCount[dishId]);
		}

		/// <summary>
		/// 设置某个菜品的绝对次数。
		/// </summary>
		public void SetDishCount(int dishId, int absoluteCount)
		{
			int newVal = Math.Max(0, absoluteCount);
			dishIdToCount[dishId] = newVal;
			OnDishCountChanged?.Invoke(dishId, newVal);
		}

		/// <summary>
		/// 获取某个菜品提交次数。
		/// </summary>
		public int GetDishCount(int dishId)
		{
			return dishIdToCount.TryGetValue(dishId, out int v) ? v : 0;
		}

		/// <summary>
		/// 返回一个浅拷贝，供结算/展示使用。
		/// </summary>
		public Dictionary<int, int> GetAllDishCounts()
		{
			return new Dictionary<int, int>(dishIdToCount);
		}

		/// <summary>
		/// 清空所有数据（用于重新开始游戏或切换关卡）
		/// </summary>
		public void Clear()
		{
			dishIdToCount.Clear();
		}
	}
}


