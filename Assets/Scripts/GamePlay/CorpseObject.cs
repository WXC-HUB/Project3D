using UnityEngine;

/// <summary>
/// 尸体对象，用于敌人死亡后生成，供子弹继续追踪
/// </summary>
public class CorpseObject : CharacterCtrlBase
{
    public float lifeTime = 10f;

    new void Awake()
    {
        base.Awake();
        usePhysic.real_value = false; // 尸体不使用物理
        isInvincible.real_value = true; // 尸体无敌
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
