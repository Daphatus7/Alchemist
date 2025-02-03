// Author : Peiyu Wang @ Daphatus
// 02 02 2025 02 42

using System;

namespace _Script.Character.PlayerAttribute
{
    /// <summary>
    /// Concrete implementation for Stamina.
    /// </summary>
    [Serializable]
    public class PlayerStamina : PlayerStat
    {
        public override StatType StatType => StatType.Stamina;
        public PlayerStamina(float maxStamina) : base(maxStamina) { }

        public override float Modify(float amount)
        {
            CurrentValue += amount;
            return amount;
        }
    }
}