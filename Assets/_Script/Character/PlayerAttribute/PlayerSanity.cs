// Author : Peiyu Wang @ Daphatus
// 03 02 2025 02 05

using System;

namespace _Script.Character.PlayerAttribute
{
    /// <summary>
    /// Concrete implementation for Sanity.
    /// </summary>
    [Serializable]
    public class PlayerSanity : PlayerStat
    {
        public PlayerSanity(float maxSanity) : base(maxSanity) { }
        
        public override StatType StatType => StatType.Sanity;
        public override float Modify(float amount)
        {
            CurrentValue += amount;
            return amount;
        }
    }
}