// Author : Peiyu Wang @ Daphatus
// 02 02 2025 02 43

using System;

namespace _Script.Alchemy
{
    [Serializable]
    public enum PotionType
    {
        /// <summary>
        /// 药剂类型定义
        /// </summary>
        Health,
        Mana,
        Stamina,
        Damage,
        Defense,
        Speed,
        CriticalRate,
        CriticalDamage,
        Experience,
        Luck
    }
}