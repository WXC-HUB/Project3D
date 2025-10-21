using Assets.Scripts.Core;
using UnityEngine;

/// <summary>
/// 出餐口：接受其他系统触发的提交调用。
/// 由其它玩家交互逻辑在合适时机调用 AcceptDish。
/// </summary>
public class DishOutlet : MonoBehaviour
{
	/// <summary>
	/// 接受一道菜的提交，默认数量为1。
	/// </summary>
	public void AcceptDish(int dishId, int count = 1)
	{
		if (count == 0) return;
		DishSubmissionManager.Instance.AddDishSubmission(dishId, count);
	}
}


