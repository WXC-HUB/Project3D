using System;
using System.Collections.Generic;
using Assets.Scripts.BaseUtils;

namespace Assets.Scripts.Core
{
	/// <summary>
	/// 菜品数据结构
	/// </summary>
	[System.Serializable]
	public class DishData
	{
		public int id;
		public string name;
	}

	/// <summary>
	/// 全局菜品提交管理器（纯C#单例）：管理菜品元数据（id、name）与提交次数，并向UI广播变更。
	/// 不依赖GameObject，数据存在内存中。
	/// </summary>
	public class DishSubmissionManager : Singleton<DishSubmissionManager>
	{
		// 菜品元数据（关卡可用的菜品列表）
		private readonly List<DishData> dishList = new List<DishData>();
		// 提交次数记录
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
			dishList.Clear();
			dishIdToCount.Clear();

			// 默认菜品（后续可改为从表或关卡配置读取）
			dishList.Add(new DishData { id = 205, name = "鱼香肉丝" });
			dishList.Add(new DishData { id = 101, name = "番茄炒蛋" });

			// 初始化数量为0
			foreach (var dish in dishList)
			{
				dishIdToCount[dish.id] = 0;
			}
		}

		/// <summary>
		/// 获取所有菜品元数据（供UI初始化用）
		/// </summary>
		public List<DishData> GetAllDishes()
		{
			return new List<DishData>(dishList);
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
			dishList.Clear();
			dishIdToCount.Clear();
		}
	}
}


