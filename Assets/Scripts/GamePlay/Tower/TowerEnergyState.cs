using UnityEngine;

namespace Assets.Scripts.Tower
{
    /// <summary>
    /// 防御塔能量状态
    /// </summary>
    public enum TowerEnergyState
    {
        Excited,    // 亢奋：100%-66%，增益效果，有阻挡
        Normal,     // 正常：65%-33%，普通状态，有阻挡
        Weak,       // 衰弱：32%-1%，负面效果，有阻挡
        Shutdown    // 宕机：0%，无法攻击，没有阻挡
    }
}

