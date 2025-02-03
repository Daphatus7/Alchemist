// Author : Peiyu Wang @ Daphatus
// 02 02 2025 02 43

using System;

namespace _Script.Alchemy
{
    /// <summary>
    /// Type of potions that will be implemented
    /// </summary>
    [Serializable]
    public enum PotionType
    {
        IncreaseHealthMax,
        IncreaseManaMax,
        IncreaseStaminaMax,
        IncreaseDamageMax,
        IncreaseDefense,
        IncreaseSpeed,
        IncreaseCriticalRate,
        IncreaseCriticalDamage,
        Experience,
        Luck
    }
}