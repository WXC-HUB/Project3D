using UnityEngine;

/// <summary>
/// 终点标记对象，用于敌人到达终点后，让子弹有一个临时追踪目标
/// </summary>
public class EndPointMarker : CharacterCtrlBase
{
    public float lifeTime = 5f;

    new void Awake()
    {
        base.Awake();
        usePhysic.real_value = false; // 标记不使用物理
        isInvincible.real_value = true; // 标记无敌
    }

    new void Update()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }
}

