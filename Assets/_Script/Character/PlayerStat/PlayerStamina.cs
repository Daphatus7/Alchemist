// Author : Peiyu Wang @ Daphatus
// 02 02 2025 02 42

using System;
using _Script.Character.PlayerStat;

namespace _Script.Character.PlayerStat
{
    /// <summary>
    /// Concrete implementation for Stamina.
    /// </summary>
    [Serializable]
    public class PlayerStamina : PlayerStat
    {
        public override StatType StatType => StatType.Stamina;
        public PlayerStamina() : base() { }

        public override float Modify(float amount)
        {
            CurrentValue += amount;
            return amount;
        }
    }
}